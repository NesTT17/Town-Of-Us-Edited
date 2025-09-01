using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static Vector3 bottomLeft;
        public static bool isCooldownReseted = false;
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
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
                    PlayerControl.LocalPlayer.SetKillTimer(ResetButtonCooldown.initialCooldown);
                    foreach (CustomButton button in CustomButton.buttons)
                    {
                        button.Timer = ResetButtonCooldown.initialCooldown;
                        button.Update();
                    }
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    playerIcons[p.PlayerId] = player;
                    player.gameObject.SetActive(false);

                    player.transform.localPosition = bottomLeft;
                    player.transform.localScale = Vector3.one * 0.4f;
                    player.gameObject.SetActive(false);
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
            firstKillName = "";

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            foreach (var bountyHunter in BountyHunter.players)
            {
                if (bountyHunter.bounty != null)
                {
                    if (PlayerControl.LocalPlayer == bountyHunter.player) bountyHunter.bountyUpdateTimer = 0f;
                }
            }

            // Reset Cooldown after Draft Mode
            if (!RoleDraftEx.isRunning && !isCooldownReseted)
            {
                isCooldownReseted = true;
                PlayerControl.LocalPlayer.SetKillTimer(ResetButtonCooldown.initialCooldown);
                foreach (CustomButton button in CustomButton.buttons)
                {
                    button.Timer = ResetButtonCooldown.initialCooldown;
                    button.Update();
                }
            }

            if (!Helpers.isMovedVentOnPolus)
            {
                if (!MapUtilities.CachedShipStatus) return;
                if (!Helpers.isPolus()) return;

                Vent bathroomVent = GameObject.FindObjectsOfType<Vent>().ToList().FirstOrDefault(x => x.name == "BathroomVent");
                if (bathroomVent == null) return;

                Vector3 initialBathroomVentPos = bathroomVent.transform.localPosition;
                bathroomVent.transform.localPosition = new Vector3(initialBathroomVentPos.x, initialBathroomVentPos.y - 1.2f, initialBathroomVentPos.z);

                Helpers.isMovedVentOnPolus = true;
            }

            if (CustomOptionHolder.randomSpawnPositions.getSelection() == 1 || CustomOptionHolder.randomSpawnPositions.getSelection() == 2) 
            { 
                RPCProcedure.systemSpreadPlayers();
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer.isAnyNeutral())
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Agent.exists && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in players)
                {
                    if (PlayerControl.LocalPlayer != p && (p.isRole(RoleId.Agent) || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }

            // Role draft: If spy is enabled, don't show the team
            if (RoleDraftEx.isEnabled && CustomOptionHolder.agentSpawnRate.getSelection() > 0 && PlayerControl.AllPlayerControls.ToArray().ToList().Where(x => x.Data.Role.IsImpostor).Count() > 1)
            {
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.factionId != FactionId.Modifier).FirstOrDefault();
            var neutralColor = new Color32(76, 84, 78, 255);
            if (roleInfo == null)
            {
                return;
            }
            if (roleInfo.factionId == FactionId.BenignNeutral || roleInfo.factionId == FactionId.EvilNeutral || roleInfo.factionId == FactionId.KillingNeutral)
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

        [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
        class SetUpRoleTextPatch
        {
            private static int last;
            public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
            {
                if (__instance.__4__this.GetInstanceID() == last)
                    return;
                last = __instance.__4__this.GetInstanceID();
                // Don't override the intro of the vanilla roles
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
                RoleInfo roleInfo = infos.Where(info => info.factionId != FactionId.Modifier).FirstOrDefault();
                RoleInfo modifierInfo = infos.Where(info => info.factionId == FactionId.Modifier).FirstOrDefault();

                __instance.__4__this.RoleBlurbText.text = "";
                if (roleInfo != null)
                {
                    __instance.__4__this.RoleText.text = roleInfo.name;
                    __instance.__4__this.RoleText.color = roleInfo.color;
                    __instance.__4__this.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.__4__this.RoleBlurbText.color = roleInfo.color;
                }
                if (modifierInfo != null)
                {
                    __instance.__4__this.RoleBlurbText.text += Helpers.cs(modifierInfo.color, $"\n{modifierInfo.introDescription}");
                }
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