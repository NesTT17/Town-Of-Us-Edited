using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Modules;

[HarmonyPatch]
internal class RoleDraftEx
{
    public static bool isEnabled => CustomOptionHolder.isDraftMode.getBool() && (gameMode == CustomGamemodes.Classic || gameMode == CustomGamemodes.Guesser);
    public static bool isRunning;

    public static List<byte> pickOrder = new();
    public static bool picked;
    public static float timer;
    private static List<Transform> buttons = new();
    private static TextMeshPro feedText;
    public static List<byte> alreadyPicked = new();
    private static Dictionary<byte, byte> playerRoles = new();
    private static readonly SimpleTable _pickTable = new SimpleTable().AddColumn(alignment: SimpleTable.Alignment.Right).AddColumn(manualWidth: 20);

    private static Sprite CrewmatePick;
    public static Sprite getCrewmatePick()
        => CrewmatePick ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RoleDraft.Crewmate.png", 115f);
    private static Sprite ImpostorPick;
    public static Sprite getImpostorPick()
        => ImpostorPick ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RoleDraft.Impostor.png", 115f);
    private static Sprite NeutralPick;
    public static Sprite getNeutralPick()
        => NeutralPick ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RoleDraft.Neutral.png", 115f);
    private static Sprite RandomPick;
    public static Sprite getRandomPick()
        => RandomPick ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RoleDraft.Random.png", 115f);

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowTeam))]
    private class ShowRolePatch
    {
        [HarmonyPostfix]
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!isEnabled) return;
            var newEnumerator = new PatchedEnumerator()
            {
                enumerator = __result.WrapToManaged(),
                Postfix = CoSelectRoles(__instance)
            };
            __result = newEnumerator.GetEnumerator().WrapToIl2Cpp();
        }
    }

    public static IEnumerator CoSelectRoles(IntroCutscene __instance)
    {
        isRunning = true;
        bool playedAlert = false;
        feedText = UnityEngine.Object.Instantiate(__instance.TeamTitle, __instance.transform);

        var aspectPosition = feedText.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
        aspectPosition.DistanceFromEdge = new Vector2(1.62f, 1.2f);
        aspectPosition.AdjustPosition();
        feedText.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        feedText.transform.position += new Vector3(0f, 0.6f);
        feedText.text = "<size=200%>Player's Picks:</size>\n\n";
        feedText.alignment = TextAlignmentOptions.TopLeft;
        feedText.autoSizeTextContainer = true;
        feedText.fontSize = 3f;
        feedText.enableAutoSizing = false;

        __instance.TeamTitle.transform.localPosition = __instance.TeamTitle.transform.localPosition + new Vector3(1f, 0f);
        __instance.TeamTitle.text = "Currently Picking:";
        __instance.BackgroundBar.enabled = false;
        __instance.TeamTitle.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        __instance.TeamTitle.autoSizeTextContainer = true;
        __instance.TeamTitle.enableAutoSizing = false;
        __instance.TeamTitle.fontSize = 5;
        __instance.TeamTitle.alignment = TextAlignmentOptions.Top;
        __instance.ImpostorText.gameObject.SetActive(false);

        GameObject.Find("BackgroundLayer")?.SetActive(false);
        foreach (var player in UnityEngine.Object.FindObjectsOfType<PoolablePlayer>())
        {
            if (player.name.Contains("Dummy"))
            {
                player.gameObject.SetActive(false);
            }
        }
        __instance.FrontMost.gameObject.SetActive(false);

        if (AmongUsClient.Instance.AmHost)
        {
            sendPickOrder();
        }

        while (pickOrder.Count == 0)
        {
            yield return null;
        }

        var roleData = Patches.RoleManagerSelectRolesPatch.getRoleAssignmentData();
        roleData.crewSettings.Add((byte)RoleId.Dracula, CustomOptionHolder.draculaSpawnRate.data);
        if (CustomOptionHolder.draculaSpawnRate.getSelection() > 0)
            roleData.crewSettings.Add((byte)RoleId.VampireHunter, CustomOptionHolder.vampireHunterSpawnRate.data);

        while (pickOrder.Count > 0)
        {
            picked = false;
            timer = 0;
            float maxTimer = CustomOptionHolder.draftModeTimeToChoose.getFloat();
            string playerText = "";

            while (timer < maxTimer || !picked)
            {
                if (pickOrder.Count == 0)
                    break;

                // wait for pick
                timer += Time.deltaTime;

                if (PlayerControl.LocalPlayer.PlayerId == pickOrder[0])
                {
                    if (!playedAlert)
                    {
                        playedAlert = true;
                        SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 1f, null);
                    }
                    // Animate beginning of choice, by changing background color
                    float min = 50 / 255f;
                    Color backGroundColor = new Color(min, min, min, 1);
                    if (timer < 1)
                    {
                        float max = 230 / 255f;
                        if (timer < 0.5f) // White flash
                        {
                            float p = timer / 0.5f;
                            float value = (float)Math.Pow(p, 2f) * max;
                            backGroundColor = new Color(value, value, value, 1);
                        }
                        else
                        {
                            float p = (1 - timer) / 0.5f;
                            float value = (float)Math.Pow(p, 2f) * max + (1 - (float)Math.Pow(p, 2f)) * min;
                            backGroundColor = new Color(value, value, value, 1);
                        }

                    }
                    HudManager.Instance.FullScreen.color = backGroundColor;
                    GameObject.Find("BackgroundLayer")?.SetActive(false);

                    // enable pick, wait for pick
                    Color youColor = timer - (int)timer > 0.5 ? Color.red : Color.yellow;
                    playerText = Helpers.cs(youColor, "You!");

                    // Available Roles:
                    List<RoleInfo> availableRoles = new();
                    foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos)
                    {
                        // Handle role pairings that are blocked, e.g. Vampire Warlock, Cleaner Vulture etc.
                        bool blocked = false;
                        foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings)
                        {
                            if (alreadyPicked.Contains(blockedRoleId.Key) && blockedRoleId.Value.ToList().Contains((byte)roleInfo.roleId))
                            {
                                blocked = true;
                                break;
                            }
                        }
                        if (blocked)
                        {
                            roleData.crewSettings.Remove((byte)roleInfo.roleId);
                            roleData.impSettings.Remove((byte)roleInfo.roleId);
                            roleData.nonKillingNeutralSettings.Remove((byte)roleInfo.roleId);
                            roleData.killingNeutralSettings.Remove((byte)roleInfo.roleId);
                            continue;
                        }

                        if (roleInfo.factionId == FactionId.Modifier) continue;
                        int impostorCount = PlayerControl.AllPlayerControls.ToArray().ToList().Where(x => x.Data.Role.IsImpostor).Count();

                        // Remove Impostor Roles
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !roleInfo.isImpostor) continue;
                        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor && roleInfo.isImpostor) continue;

                        if (roleData.nonKillingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.nonKillingNeutralSettings[(byte)roleInfo.roleId].rate == 0)
                            continue;
                        else if (roleData.killingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.killingNeutralSettings[(byte)roleInfo.roleId].rate == 0)
                            continue;
                        else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0)
                            continue;
                        else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0)
                            continue;

                        if (roleInfo.roleId == RoleId.Pursuer)
                            continue;
                        if (roleInfo.roleId == RoleId.Mayor)
                            continue;
                        if (roleInfo.roleId == RoleId.Pestilence)
                            continue;
                        if (roleInfo.roleId == RoleId.Vampire)
                            continue;
                        if (roleInfo.roleId == RoleId.Survivor)
                            continue;
                        if (roleInfo.roleId == RoleId.Agent && impostorCount < 2)
                            continue;
                        if (roleInfo.roleId == RoleId.Crewmate)
                            continue;
                        if (roleInfo.roleId == RoleId.Impostor)
                            continue;

                        int impsPicked = alreadyPicked.Where(x => RoleInfo.roleInfoById[(RoleId)x].isImpostor).Count();
                        // Hanlde forcing of 100% roles for impostors
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                        {
                            int impsMax = impostorCount;
                            int impsMin = impostorCount;
                            if (impsMin > impsMax) impsMin = impsMax;
                            int impsLeft = pickOrder.Where(x => Helpers.playerById(x).Data.Role.IsImpostor).Count();
                            int imps100 = roleData.impSettings.Where(x => x.Value.rate == 10).Count();
                            if (imps100 > impsMax) imps100 = impsMax;
                            int imps100Picked = alreadyPicked.Where(x => roleData.impSettings.GetValueSafe(x).rate == 10).Count();
                            if (imps100 - imps100Picked >= impsLeft && !(roleData.impSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0)) continue;
                            if (impsMin - impsPicked >= impsLeft && roleInfo.roleId == RoleId.Impostor) continue;
                            if (roleInfo.roleId == RoleId.Impostor) continue;
                            if (impsPicked >= impsMax && roleInfo.roleId != RoleId.Impostor) continue;
                        }
                        // Player is no impostor! Handle forcing of 100% roles for crew and neutral
                        else
                        {
                            // No more neutrals possible!
                            int nonKillingNeutralsPicked = alreadyPicked.Where(x => RoleInfo.roleInfoById[(RoleId)x].factionId == FactionId.BenignNeutral || RoleInfo.roleInfoById[(RoleId)x].factionId == FactionId.EvilNeutral).Count();
                            int killingNeutralsPicked = alreadyPicked.Where(x => RoleInfo.roleInfoById[(RoleId)x].factionId == FactionId.KillingNeutral).Count();
                            int crewPicked = alreadyPicked.Count - impsPicked - nonKillingNeutralsPicked - killingNeutralsPicked;
                            int nonKillingNeutralsMax = CustomOptionHolder.nonKillingNeutralRolesCountMax.getSelection();
                            int nonKillingNeutralsMin = CustomOptionHolder.nonKillingNeutralRolesCountMin.getSelection();
                            int killingNeutralsMax = CustomOptionHolder.killingNeutralRolesCountMax.getSelection();
                            int killingNeutralsMin = CustomOptionHolder.killingNeutralRolesCountMin.getSelection();
                            int nonKillingNeutrals100 = roleData.nonKillingNeutralSettings.Where(x => x.Value.rate == 10).Count();
                            int killingNeutrals100 = roleData.killingNeutralSettings.Where(x => x.Value.rate == 10).Count();
                            if (nonKillingNeutrals100 > nonKillingNeutralsMin) nonKillingNeutralsMin = nonKillingNeutrals100;
                            if (killingNeutrals100 > killingNeutralsMin) killingNeutralsMin = killingNeutrals100;
                            if (nonKillingNeutralsMin > nonKillingNeutralsMax) nonKillingNeutralsMin = nonKillingNeutralsMax;
                            if (killingNeutralsMin > killingNeutralsMax) killingNeutralsMin = killingNeutralsMax;

                            // If crewmate fill disabled and crew picked the amount of allowed crewmates alreay: no more crewmate except vanilla crewmate allowed!
                            int crewLimit = PlayerControl.AllPlayerControls.Count - impostorCount - (nonKillingNeutralsMin > nonKillingNeutrals100 ? nonKillingNeutralsMin : nonKillingNeutrals100 > nonKillingNeutralsMax ? nonKillingNeutralsMax : nonKillingNeutrals100) - (killingNeutralsMin > killingNeutrals100 ? killingNeutralsMin : killingNeutrals100 > killingNeutralsMax ? killingNeutralsMax : killingNeutrals100);
                            int maxCrew = crewLimit;
                            if (maxCrew > crewLimit)
                                maxCrew = crewLimit;
                            if (crewPicked >= crewLimit && roleInfo.factionId != FactionId.BenignNeutral && roleInfo.factionId != FactionId.EvilNeutral && roleInfo.factionId != FactionId.KillingNeutral && roleInfo.roleId != RoleId.Crewmate) continue;

                            // Fill roles means no crewmates allowed!
                            if (roleInfo.roleId == RoleId.Crewmate) continue;
                            bool allowAnyNeutral = false;
                            if (nonKillingNeutralsPicked >= nonKillingNeutralsMax && (roleInfo.factionId == FactionId.BenignNeutral || roleInfo.factionId == FactionId.EvilNeutral)) continue;
                            if (killingNeutralsPicked >= killingNeutralsMax && roleInfo.factionId == FactionId.KillingNeutral) continue;

                            // More neutrals needed? Then no more crewmates! This takes precedence over crew roles set to 100%!
                            var crewmatesLeft = pickOrder.Count - pickOrder.Where(x => Helpers.playerById(x).Data.Role.IsImpostor).Count();
                            if (crewmatesLeft <= nonKillingNeutralsMin - nonKillingNeutralsPicked && roleInfo.factionId != FactionId.BenignNeutral && roleInfo.factionId != FactionId.EvilNeutral)
                            {
                                continue;
                            }
                            else if (crewmatesLeft <= killingNeutralsMin - killingNeutralsPicked && roleInfo.factionId != FactionId.KillingNeutral)
                            {
                                continue;
                            }
                            else if ((nonKillingNeutralsMin - nonKillingNeutrals100 > nonKillingNeutralsPicked) || (killingNeutralsMin - killingNeutrals100 > killingNeutralsPicked))
                                allowAnyNeutral = true;

                            // Handle 100% Roles PER Faction.
                            int nonKillingNeutrals100Picked = alreadyPicked.Where(x => roleData.nonKillingNeutralSettings.GetValueSafe(x).rate == 10).Count();
                            if (nonKillingNeutrals100 > nonKillingNeutralsMax) nonKillingNeutrals100 = nonKillingNeutralsMax;

                            int killingNeutrals100Picked = alreadyPicked.Where(x => roleData.killingNeutralSettings.GetValueSafe(x).rate == 10).Count();
                            if (killingNeutrals100 > killingNeutralsMax) killingNeutrals100 = killingNeutralsMax;

                            int crew100 = roleData.crewSettings.Where(x => x.Value.rate == 10).Count();
                            int crew100Picked = alreadyPicked.Where(x => roleData.crewSettings.GetValueSafe(x).rate == 10).Count();
                            if (nonKillingNeutrals100 > nonKillingNeutralsMax) nonKillingNeutrals100 = nonKillingNeutralsMax;
                            if (killingNeutrals100 > killingNeutralsMax) killingNeutrals100 = killingNeutralsMax;

                            if (crew100 > maxCrew) crew100 = maxCrew;
                            if (((nonKillingNeutrals100 - nonKillingNeutrals100Picked >= crewmatesLeft) || ((roleInfo.factionId == FactionId.BenignNeutral || roleInfo.factionId == FactionId.EvilNeutral) && nonKillingNeutrals100 - nonKillingNeutrals100Picked >= nonKillingNeutralsMax - nonKillingNeutralsPicked)) && !(nonKillingNeutrals100Picked >= nonKillingNeutralsMax) && !(roleData.nonKillingNeutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0)) continue;
                            if ((killingNeutrals100 - killingNeutrals100Picked >= crewmatesLeft || roleInfo.factionId == FactionId.KillingNeutral && killingNeutrals100 - killingNeutrals100Picked >= killingNeutralsMax - killingNeutralsPicked) && !(killingNeutrals100Picked >= killingNeutralsMax) && !(roleData.killingNeutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0)) continue;
                            if (!(allowAnyNeutral && (roleInfo.factionId == FactionId.BenignNeutral || roleInfo.factionId == FactionId.EvilNeutral || roleInfo.factionId == FactionId.KillingNeutral)) && crew100 - crew100Picked >= crewmatesLeft && !(roleData.crewSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0)) continue;

                            if (!(allowAnyNeutral && (roleInfo.factionId == FactionId.BenignNeutral || roleInfo.factionId == FactionId.EvilNeutral || roleInfo.factionId == FactionId.KillingNeutral)) && nonKillingNeutrals100 + killingNeutrals100 + crew100 - nonKillingNeutrals100Picked - killingNeutrals100Picked - crew100Picked >= crewmatesLeft && !(roleData.crewSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0 || roleData.nonKillingNeutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0 || roleData.killingNeutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Count() > 0)) continue;
                        }
                        availableRoles.TryAdd(roleInfo);
                    }

                    availableRoles = availableRoles.OrderBy(_ => Guid.NewGuid()).ToList();

                    // Fallback for if all roles are somehow removed. (This is only the case if there is a bug, hence print a warning
                    if (availableRoles.Count == 0)
                    {
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                            availableRoles.Add(RoleInfo.impostor);
                        else
                            availableRoles.Add(RoleInfo.crewmate);
                        TownOfUsPlugin.Logger.LogWarning("Draft Mode: Fallback triggered, because no roles were left. Forced addition of basegame Imp/Crewmate");
                    }

                    List<RoleInfo> originalAvailable = new(availableRoles);
                    // remove some roles, so that you can't always get the same roles:
                    if (availableRoles.Count > CustomOptionHolder.draftModeAmountOfChoices.getFloat())
                    {
                        int countToRemove = availableRoles.Count - (int)CustomOptionHolder.draftModeAmountOfChoices.getFloat();
                        while (countToRemove-- > 0)
                        {
                            var toRemove = availableRoles.OrderBy(_ => Guid.NewGuid()).First();
                            availableRoles.Remove(toRemove);
                        }
                    }

                    if (timer >= maxTimer)
                    {
                        sendPick((byte)availableRoles.OrderBy(_ => Guid.NewGuid()).First().roleId, SelectFlags.Normal);
                    }

                    if (GameObject.Find("RoleButton") == null)
                    {
                        int i = 0;
                        int num28 = 0;
                        int buttonsPerRow = 3;
                        int lastRow = availableRoles.Count / buttonsPerRow;
                        int buttonsInLastRow = availableRoles.Count % buttonsPerRow;

                        Transform transform = DestroyableSingleton<HudManager>.Instance.SettingsButton.transform;
                        TextMeshPro taskText = DestroyableSingleton<HudManager>.Instance.TaskPanel.taskText;

                        foreach (RoleInfo roleInfo in availableRoles)
                        {
                            if (num28 == buttonsPerRow) num28 = 0;
                            float row = i / buttonsPerRow;
                            float col = i % buttonsPerRow;
                            if (buttonsInLastRow != 0 && row == lastRow)
                            {
                                col += (buttonsPerRow - buttonsInLastRow) / 2f;
                            }
                            row += (4 - lastRow - 1) / 2f;

                            Transform roleButton = UnityEngine.Object.Instantiate<Transform>(transform, __instance.TeamTitle.transform);
                            roleButton.GetComponent<AspectPosition>().Destroy();
                            roleButton.transform.position = __instance.TeamTitle.transform.position;
                            roleButton.name = "RoleButton";
                            roleButton.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.55f);
                            roleButton.transform.localScale = new Vector3(3f, 3f);
                            SpriteRenderer roleButtonSprite = roleButton.FindChild("Active").GetComponent<SpriteRenderer>();
                            SpriteRenderer roleButtonSpriteInactive = roleButton.FindChild("Inactive").GetComponent<SpriteRenderer>();
                            roleButton.FindChild("Background").gameObject.active = false;
                            roleButtonSprite.sprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlankPlate.png", 100f);
                            roleButtonSpriteInactive.sprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlankPlate.png", 100f);
                            roleButtonSprite.color = Color.green;
                            roleButton.transform.localPosition = new Vector3(FloatRange.SpreadToEdges(-8f, 8f, num28, 3), -7.5f - row * 2.25f);
                            num28++;

                            TextMeshPro roleButtonText = UnityEngine.Object.Instantiate<TextMeshPro>(taskText, roleButton.transform);
                            roleButtonText.text = Helpers.cs(roleInfo.color, roleInfo.name);
                            roleButtonText.alignment = TextAlignmentOptions.Center;
                            roleButtonText.transform.localPosition = new Vector3(0f, 0f, -1f);
                            roleButtonText.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

                            PassiveButton roleButtonAction = roleButton.GetComponent<PassiveButton>();
                            roleButtonAction.OnClick.RemoveAllListeners();
                            roleButtonAction.OnClick = new Button.ButtonClickedEvent();
                            roleButtonAction.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                            {
                                sendPick((byte)roleInfo.roleId);
                            }));
                            buttons.Add(roleButton);
                            i++;
                        }

                        Transform randomRoleButton = UnityEngine.Object.Instantiate<Transform>(transform, __instance.TeamTitle.transform);
						randomRoleButton.GetComponent<AspectPosition>().Destroy();
						randomRoleButton.transform.position = __instance.TeamTitle.transform.position;
						randomRoleButton.name = "RoleButton";
						randomRoleButton.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.55f);
						randomRoleButton.transform.localScale = new Vector3(3f, 3f);
						SpriteRenderer randomRoleButtonSprite = randomRoleButton.FindChild("Active").GetComponent<SpriteRenderer>();
						SpriteRenderer randomRoleButtonSpriteInactive = randomRoleButton.FindChild("Inactive").GetComponent<SpriteRenderer>();
						randomRoleButton.FindChild("Background").gameObject.active = false;
						randomRoleButtonSprite.sprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlankPlate.png", 100f);
						randomRoleButtonSpriteInactive.sprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlankPlate.png", 100f);
						randomRoleButtonSprite.color = Color.green;
						randomRoleButton.transform.localPosition = new Vector3(0f, -8f);

						TextMeshPro randomRoleButtonText = UnityEngine.Object.Instantiate<TextMeshPro>(taskText, randomRoleButton.transform);
						randomRoleButtonText.text = Helpers.cs(Palette.AcceptedGreen, "Random");
						randomRoleButtonText.alignment = TextAlignmentOptions.Center;
						randomRoleButtonText.transform.localPosition = new Vector3(0f, 0f, -1f);
						randomRoleButtonText.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

						PassiveButton randomRoleButtonAction = randomRoleButton.GetComponent<PassiveButton>();
						randomRoleButtonAction.OnClick = new Button.ButtonClickedEvent();
                        randomRoleButtonAction.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
						{
							sendPick((byte)originalAvailable.OrderBy((RoleInfo _) => Guid.NewGuid()).First().roleId, SelectFlags.Random);
						}));
						buttons.Add(randomRoleButton);
                    }
                }
                else
                {
                    int currentPick = PlayerControl.AllPlayerControls.Count - pickOrder.Count + 1;
                    playerText = $"Anonymous Player {currentPick}";
                    HudManager.Instance.FullScreen.color = Color.black;
                }

                __instance.TeamTitle.text = $"{Helpers.cs(Color.white, "<size=280%>Welcome to the Role Draft Ex!</size>")}\n\n\n<size=200%> Currently Picking:</size>\n\n\n<size=250%>{playerText}</size>";
                int waitMore = pickOrder.IndexOf(PlayerControl.LocalPlayer.PlayerId);
                string waitMoreText = "";
                if (waitMore > 0)
                {
                    waitMoreText = $" ({waitMore} rounds until your turn)";
                }
                __instance.TeamTitle.text += $"\n\n{waitMoreText}\nRandom Selection In... {(int)(maxTimer + 1 - timer)}\n {(SoundManager.MusicVolume > -80 ? "♫ Music: Ultimate Superhero 3 - Kenët & Rez ♫" : "")}";
                yield return null;
            }
        }

        HudManager.Instance.FullScreen.color = Color.black;
        __instance.FrontMost.gameObject.SetActive(true);
        GameObject.Find("BackgroundLayer")?.SetActive(true);

        if (AmongUsClient.Instance.AmHost)
        {
            RoleManagerSelectRolesPatch.assignRoleTargets(null); // Assign targets for Lawyer, Executioner & Guardian Angel
            if (RoleManagerSelectRolesPatch.isGuesserGamemode) RoleManagerSelectRolesPatch.assignGuesserGamemode();
            RoleManagerSelectRolesPatch.assignModifiers(); // Assign modifier
        }

        float myTimer = 0f;
        while (myTimer < 3f)
        {
            myTimer += Time.deltaTime;
            Color c = new Color(0, 0, 0, myTimer / 3.0f);
            __instance.FrontMost.color = c;
            yield return null;
        }
        isRunning = false;
        yield break;
    }

    public static void receivePick(byte playerId, byte roleId, byte flag = 0)
    {
        if (!isEnabled) return;
        RPCProcedure.setRole(roleId, playerId);
        alreadyPicked.Add(roleId);
        playerRoles.Add(playerId, roleId);

        var isRandom = flag > 0;
        var reasons = ((SelectFlags)flag).ToString();

        try
        {
            pickOrder.Remove(playerId);
            timer = 0;
            picked = true;
            RoleInfo roleInfo = RoleInfo.allRoleInfos.First(x => (byte)x.roleId == roleId);
            var isLocalPlayer = playerId == PlayerControl.LocalPlayer.PlayerId;
            var reasonString = isRandom ? Helpers.cs(Color.yellow, $" (RND)") : "";
            var roleString = isLocalPlayer
                ? isRandom ? $"{Helpers.cs(roleInfo.color, roleInfo.name + reasonString)}" : $"{Helpers.cs(roleInfo.color, roleInfo.name)}"
                : BuildRoleString(roleInfo, isRandom, reasons);

            string line = $"{(playerId == PlayerControl.LocalPlayer.PlayerId ? "You" : alreadyPicked.Count)}:";
            _pickTable.AddRow(line, roleString);
            feedText.text = "<size=200%>Ready Players:</size>\n\n" + _pickTable.ToString();
        }
        catch (Exception e) { TownOfUsPlugin.Logger.LogError(e); }

        static string BuildRoleString(RoleInfo roleInfo, bool isRandom, string reason)
        {
            if (isRandom) return Helpers.cs(Color.green, $"{reason}");

            if (!CustomOptionHolder.draftModeShowRoles.getBool())
                return "Unknown Role";

            if (CustomOptionHolder.draftModeHideImpRoles.getBool() && roleInfo.factionId == FactionId.Impostor)
                return Helpers.cs(Palette.ImpostorRed, "Impostor Role");
            else if (CustomOptionHolder.draftModeHideNeutralBenignRoles.getBool() && roleInfo.factionId == FactionId.BenignNeutral)
                return Helpers.cs(Palette.Blue, "Neutral Benign Role");
            else if (CustomOptionHolder.draftModeHideNeutralEvilRoles.getBool() && roleInfo.factionId == FactionId.EvilNeutral)
                return Helpers.cs(Palette.Blue, "Neutral Evil Role");
            else if (CustomOptionHolder.draftModeHideNeutralKillingRoles.getBool() && roleInfo.factionId == FactionId.KillingNeutral)
                return Helpers.cs(Palette.Blue, "Neutral Killing Role");
            else if (CustomOptionHolder.draftModeHideCrewmateRoles.getBool() && roleInfo.factionId == FactionId.Crewmate)
                return Helpers.cs(Palette.Blue, "Crewmate Role");
            else return Helpers.cs(roleInfo.color, roleInfo.name);
        }
    }

    public static void sendPick(byte RoleId, SelectFlags flag = SelectFlags.Normal)
    {
        if (playerRoles.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out _)) return;

        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraftModePick, SendOption.Reliable, -1);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(RoleId);
        writer.Write((byte)flag);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        receivePick(PlayerControl.LocalPlayer.PlayerId, RoleId, (byte)flag);
        try
        {
            foreach (var button in buttons)
                UnityEngine.Object.Destroy(button?.gameObject);
            buttons = new();
        }
        catch { }
    }

    public static void sendPickOrder()
    {
        pickOrder = PlayerControl.AllPlayerControls.ToArray().Select(x => x.PlayerId).OrderBy(_ => Guid.NewGuid()).ToList().ToList();
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraftModePickOrder, SendOption.Reliable, -1);
        writer.Write((byte)pickOrder.Count);
        foreach (var item in pickOrder)
        {
            writer.Write(item);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void receivePickOrder(int amount, MessageReader reader)
    {
        pickOrder.Clear();
        for (int i = 0; i < amount; i++)
        {
            pickOrder.Add(reader.ReadByte());
        }
    }

    public static void Clear()
    {

        isRunning = false;
        alreadyPicked = new();
        playerRoles = new();
        buttons = new();
        _pickTable.ClearRows();
    }
    
    private class PatchedEnumerator() : IEnumerable
    {
        public IEnumerator enumerator;
        public IEnumerator Postfix;
        public IEnumerator GetEnumerator()
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
            while (Postfix.MoveNext())
                yield return Postfix.Current;
        }
    }

    public enum SelectFlags
    {
        Normal,
        Disconnect,
        Random
    }
}