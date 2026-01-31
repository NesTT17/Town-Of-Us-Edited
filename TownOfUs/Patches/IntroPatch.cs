using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactor.Utilities;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using PowerTools;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static Vector3 bottomLeft;
        public static bool isCooldownResetted = false;
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            int playerCounter = 0;
            if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
            {
                float aspect = Camera.main.aspect;
                float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
                float xpos = 1.75f - safeOrthographicSize * aspect * 1.70f;
                float ypos = 0.15f - safeOrthographicSize * 1.7f;
                bottomLeft = new Vector3(xpos / 2, ypos / 2, -61f);

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    NetworkedPlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    playerIcons[p.PlayerId] = player;
                    player.gameObject.SetActive(false);
                    if (PlayerControl.LocalPlayer == Seer.seer && p != Seer.seer)
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.4f;
                        player.transform.localScale = Vector3.one * 0.3f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    }
                    else
                    {
                        player.transform.localPosition = bottomLeft;
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    PlayerControl.LocalPlayer.SetKillTimer(10f);
                    foreach (CustomButton button in CustomButton.buttons)
                    {
                        button.Timer = 10f;
                        button.Update();
                    }
                }
            }

            // Force Scavenger to load a new Bounty when the Intro is over
            if (Scavenger.bounty != null && PlayerControl.LocalPlayer == Scavenger.scavenger)
            {
                Scavenger.bountyUpdateTimer = 0f;
                if (FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    Scavenger.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    Scavenger.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    Scavenger.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -0.35f, -62f);
                    Scavenger.cooldownText.transform.localScale = Vector3.one * 0.4f;
                    Scavenger.cooldownText.gameObject.SetActive(true);
                }
            }

            // First kill
            if (AmongUsClient.Instance.AmHost && shieldFirstKill && firstKillName != "")
            {
                PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(firstKillName));
                if (target != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetFirstKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFirstKill(target.PlayerId);
                }
            }

            // Reset Cooldown
            if (!isCooldownResetted)
            {
                isCooldownResetted = true;
                PlayerControl.LocalPlayer.SetKillTimer(10f);
                foreach (CustomButton button in CustomButton.buttons)
                {
                    button.Timer = 10f;
                    button.Update();
                }
            }

            // Random Spawn Positions
            if (CustomOptionHolder.randomSpawnPositions.getSelection() != 0)
            {
                if (MapBehaviour.Instance)
                    MapBehaviour.Instance.Close();
                if (Minigame.Instance)
                    Minigame.Instance.ForceClose();
                if (PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                }

                var SpawnPositions = GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
                {
                    0 => MapData.SkeldSpawnPosition,
                    1 => MapData.MiraSpawnPosition,
                    2 => MapData.PolusSpawnPosition,
                    3 => MapData.DleksSpawnPosition,
                    4 => MapData.AirshipSpawnPosition,
                    5 => MapData.FungleSpawnPosition,
                    _ => MapData.FindVentSpawnPositions(),
                };
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SpawnPositions[rnd.Next(SpawnPositions.Count)]);
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch
    {
        public static IEnumerator CoBegin(IntroCutscene __instance)
        {
            SoundManager.Instance.PlaySound(__instance.IntroStinger, false, 1f, null);
            if (GameManager.Instance.IsNormal())
            {
                __instance.LogPlayerRoleData();
                __instance.HideAndSeekPanels.SetActive(false);
                __instance.CrewmateRules.SetActive(false);
                __instance.ImpostorRules.SetActive(false);
                __instance.ImpostorName.gameObject.SetActive(false);
                __instance.ImpostorTitle.gameObject.SetActive(false);
                var list = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                list =
                    IntroCutscene.SelectTeamToShow(
                        (Func<NetworkedPlayerInfo, bool>)(pcd =>
                            !PlayerControl.LocalPlayer.Data.Role.IsImpostor ||
                            pcd.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType
                        )
                    );
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    __instance.ImpostorText.gameObject.SetActive(false);
                }
                else
                {
                    int adjustedNumImpostors = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
                    if (adjustedNumImpostors == 1)
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS, new UnityEngine.Object());
                    }
                    else
                    {
                        var parameters = new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { (Il2CppSystem.Object)adjustedNumImpostors });
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, parameters);
                    }
                    __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
                    __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
                }
                yield return __instance.ShowTeam(list, 3f);
                yield return SetUpRoleTextPatch.SetRoleTexts(__instance).WrapToIl2Cpp();
            }
            else
            {
                __instance.LogPlayerRoleData();
                __instance.HideAndSeekPanels.SetActive(true);
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    __instance.CrewmateRules.SetActive(false);
                    __instance.ImpostorRules.SetActive(true);
                }
                else
                {
                    __instance.CrewmateRules.SetActive(true);
                    __instance.ImpostorRules.SetActive(false);
                }
                Il2CppSystem.Collections.Generic.List<PlayerControl> list2 = IntroCutscene.SelectTeamToShow(
                    (Func<NetworkedPlayerInfo, bool>)(pcd => PlayerControl.LocalPlayer.Data.Role.IsImpostor != pcd.Role.IsImpostor)
                );
                PlayerControl impostor = PlayerControl.AllPlayerControls.Find(
                    (Il2CppSystem.Predicate<PlayerControl>)(pc => pc.Data.Role.IsImpostor)
                );
                GameManager.Instance.SetSpecialCosmetics(impostor);
                __instance.ImpostorName.gameObject.SetActive(true);
                __instance.ImpostorTitle.gameObject.SetActive(true);
                __instance.BackgroundBar.enabled = false;
                __instance.TeamTitle.gameObject.SetActive(false);
                if (impostor != null)
                {
                    __instance.ImpostorName.text = impostor.Data.PlayerName;
                }
                else
                {
                    __instance.ImpostorName.text = "???";
                }
                yield return new WaitForSecondsRealtime(0.1f);
                PoolablePlayer playerSlot = null;
                if (impostor != null)
                {
                    playerSlot = __instance.CreatePlayer(1, 1, impostor.Data, false);
                    playerSlot.SetBodyType(PlayerBodyTypes.Normal);
                    playerSlot.SetFlipX(false);
                    playerSlot.transform.localPosition = __instance.impostorPos;
                    playerSlot.transform.localScale = Vector3.one * __instance.impostorScale;
                }
                yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
                yield return new WaitForSecondsRealtime(6f);
                if (playerSlot != null)
                {
                    playerSlot.gameObject.SetActive(false);
                }
                __instance.HideAndSeekPanels.SetActive(false);
                __instance.CrewmateRules.SetActive(false);
                __instance.ImpostorRules.SetActive(false);
                LogicOptionsHnS logicOptionsHnS = GameManager.Instance.LogicOptions as LogicOptionsHnS;
                LogicHnSMusic logicHnSMusic = GameManager.Instance.GetLogicComponent<LogicHnSMusic>() as LogicHnSMusic;
                if (logicHnSMusic != null)
                {
                    logicHnSMusic.StartMusicWithIntro();
                }
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    float crewmateLeadTime = (float)logicOptionsHnS.GetCrewmateLeadTime();
                    __instance.HideAndSeekTimerText.gameObject.SetActive(true);
                    PoolablePlayer poolablePlayer;
                    AnimationClip animationClip;
                    if (AprilFoolsMode.ShouldHorseAround())
                    {
                        poolablePlayer = __instance.HorseWrangleVisualSuit;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                        animationClip = __instance.HnSSeekerSpawnHorseAnim;
                        __instance.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                        __instance.HorseWrangleVisualPlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, false, null, false);
                    }
                    else if (AprilFoolsMode.ShouldLongAround())
                    {
                        poolablePlayer = __instance.HideAndSeekPlayerVisual;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
                        animationClip = __instance.HnSSeekerSpawnLongAnim;
                    }
                    else
                    {
                        poolablePlayer = __instance.HideAndSeekPlayerVisual;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                        animationClip = __instance.HnSSeekerSpawnAnim;
                    }
                    poolablePlayer.SetBodyCosmeticsVisible(false);
                    poolablePlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, false, null, false);
                    SpriteAnim component = poolablePlayer.GetComponent<SpriteAnim>();
                    poolablePlayer.gameObject.SetActive(true);
                    poolablePlayer.ToggleName(false);
                    component.Play(animationClip, 1f);
                    while (crewmateLeadTime > 0f)
                    {
                        __instance.HideAndSeekTimerText.text = Mathf.RoundToInt(crewmateLeadTime).ToString();
                        crewmateLeadTime -= Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    ShipStatus.Instance.HideCountdown = (float)logicOptionsHnS.GetCrewmateLeadTime();
                    if (AprilFoolsMode.ShouldHorseAround())
                    {
                        if (impostor != null)
                        {
                            impostor.AnimateCustom(__instance.HnSSeekerSpawnHorseInGameAnim);
                        }
                    }
                    else if (AprilFoolsMode.ShouldLongAround())
                    {
                        if (impostor != null)
                        {
                            impostor.AnimateCustom(__instance.HnSSeekerSpawnLongInGameAnim);
                        }
                    }
                    else if (impostor != null)
                    {
                        impostor.AnimateCustom(__instance.HnSSeekerSpawnAnim);
                        impostor.cosmetics.SetBodyCosmeticsVisible(false);
                    }
                }
                impostor = null;
                playerSlot = null;
            }
            ShipStatus.Instance.StartSFX();
            UnityEngine.Object.Destroy(__instance.gameObject);
            yield break;
        }

        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer.isAnyNeutral())
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.factionId != FactionId.Modifier && info.factionId != FactionId.Ghost).FirstOrDefault();
            var neutralColor = new Color32(76, 84, 78, 255);
            if (roleInfo == null) return;
            if (roleInfo.factionId == FactionId.NeutralBenign || roleInfo.factionId == FactionId.NeutralEvil || roleInfo.factionId == FactionId.NeutralKilling)
            {
                __instance.BackgroundBar.material.color = neutralColor;
                __instance.TeamTitle.text = "Neutral";
                __instance.TeamTitle.color = neutralColor;
            }
        }

        public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance)
        {
            yield return new WaitForSeconds(5f);
            __instance.YouAreText.gameObject.SetActive(false);
            __instance.RoleText.gameObject.SetActive(false);
            __instance.RoleBlurbText.gameObject.SetActive(false);
            __instance.ourCrewmate.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
        class CreatePlayerPatch
        {
            public static void Postfix(IntroCutscene __instance, bool impostorPositioning, ref PoolablePlayer __result)
            {
                if (impostorPositioning) __result.SetNameColor(Palette.ImpostorRed);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
        class IntroCutsceneCoBeginPatch
        {
            public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
            {
                __result = CoBegin(__instance).WrapToIl2Cpp();

                return false;
            }
        }

        [HarmonyPatch]
        class SetUpRoleTextPatch
        {
            static int seed = 0;
            static public IEnumerator SetRoleTexts(IntroCutscene __instance)
            {
                seed = rnd.Next(5000);
                
                // Don't override the intro of the vanilla roles
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
                RoleInfo roleInfo = infos.Where(info => info.factionId != FactionId.Modifier && info.factionId != FactionId.Ghost).FirstOrDefault();
                RoleInfo modifierInfo = infos.Where(info => info.factionId == FactionId.Modifier).FirstOrDefault();

                __instance.RoleBlurbText.text = "";
                if (roleInfo != null)
                {
                    __instance.RoleText.text = roleInfo.name;
                    __instance.RoleText.color = roleInfo.color;
                    __instance.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.RoleBlurbText.color = roleInfo.color;
                    __instance.YouAreText.color = roleInfo.color;
                }
                if (modifierInfo != null)
                {
                    if (modifierInfo.roleId != RoleId.Lover)
                        __instance.RoleBlurbText.text += Helpers.cs(modifierInfo.color, $"\n{modifierInfo.introDescription}");
                    else
                    {
                        PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                        __instance.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
                    }
                }

                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f, null);
                __instance.YouAreText.gameObject.SetActive(true);
                __instance.RoleText.gameObject.SetActive(true);
                __instance.RoleBlurbText.gameObject.SetActive(true);
                if (__instance.ourCrewmate == null)
                {
                    __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                    __instance.ourCrewmate.gameObject.SetActive(false);
                }
                __instance.ourCrewmate.gameObject.SetActive(true);
                __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
                __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);
                __instance.ourCrewmate.ToggleName(false);
                yield return new WaitForSeconds(2.5f);
                __instance.YouAreText.gameObject.SetActive(false);
                __instance.RoleText.gameObject.SetActive(false);
                __instance.RoleBlurbText.gameObject.SetActive(false);
                __instance.ourCrewmate.gameObject.SetActive(false);
                yield break;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }
}