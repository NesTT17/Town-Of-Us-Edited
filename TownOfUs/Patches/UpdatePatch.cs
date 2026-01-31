using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        private static Dictionary<byte, (string name, Color color)> TagColorDict = new();
        static void resetNameTagsAndColors()
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var myData = PlayerControl.LocalPlayer.Data;
            var amImpostor = myData.Role.IsImpostor;
            var morphTimerNotUp = Morphling.morphTimer > 0f;
            var morphTargetNotNull = Morphling.morphTarget != null;
            var mimicTimerNotUp = Glitch.morphTimer > 0f;
            var mimicTargetNotNull = Glitch.morphPlayer != null;

            var dict = TagColorDict;
            dict.Clear();

            foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var player = data.Object;
                string text = data.PlayerName;
                Color color;
                if (player)
                {
                    var playerName = text;
                    if (morphTimerNotUp && morphTargetNotNull && Morphling.morphling == player) playerName = Morphling.morphTarget.Data.PlayerName;
                    if (mimicTimerNotUp && mimicTargetNotNull && Glitch.glitch == player) playerName = Glitch.morphPlayer.Data.PlayerName;
                    var nameText = player.cosmetics.nameText;

                    nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                    nameText.color = nameText.color.SetAlpha(Shy.visibility(player.PlayerId));
                }
                else
                {
                    color = Color.white;
                }


                dict.Add(data.PlayerId, (text, color));
            }

            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    var data = dict[playerVoteArea.TargetPlayerId];
                    var text = playerVoteArea.NameText;
                    text.text = data.name;
                    text.color = data.color;
                }
            }
        }

        static void setPlayerNameColor(PlayerControl p, Color color)
        {
            p.cosmetics.nameText.color = color.SetAlpha(Shy.visibility(p.PlayerId));
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors()
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
            setPlayerNameColor(localPlayer, localRole.color);

            if (Dracula.dracula != null && Dracula.dracula == localPlayer)
            {
                // Dracula can see his vampire
                setPlayerNameColor(Dracula.dracula, Dracula.color);
                if (Vampire.vampire != null)
                {
                    setPlayerNameColor(Vampire.vampire, Dracula.color);
                }
                if (Dracula.fakeVampire != null)
                {
                    setPlayerNameColor(Dracula.fakeVampire, Dracula.color);
                }
            }

            // No else if here, as a Lover of team Vampires needs the colors
            if (Vampire.vampire != null && Vampire.vampire == localPlayer)
            {
                // Vampire can see the dracula
                setPlayerNameColor(Vampire.vampire, Vampire.color);
                if (Dracula.dracula != null)
                {
                    setPlayerNameColor(Dracula.dracula, Vampire.color);
                }
            }

            // No else if here, as the Impostors need the name to be colored
            if (Dracula.dracula != null && Dracula.wasTeamRed && localPlayer.Data.Role.IsImpostor)
            {
                setPlayerNameColor(Dracula.dracula, Palette.ImpostorRed);
            }
            if (Vampire.vampire != null && Vampire.wasTeamRed && localPlayer.Data.Role.IsImpostor)
            {
                setPlayerNameColor(Vampire.vampire, Palette.ImpostorRed);
            }
        }

        static void setNameTags()
        {
            // Show flashed players for Grenadier
            if (Grenadier.grenadier != null && PlayerControl.LocalPlayer == Grenadier.grenadier && Grenadier.flashTimer > 0.5f)
            {
                foreach (PlayerControl player in Grenadier.flashedPlayers)
                {
                    if (!player.Data.Role.IsImpostor && !player.Data.IsDead)
                    {
                        setPlayerNameColor(player, Color.black);
                    }
                }
            }

            // Show Politician campaigned
            if (Politician.politician != null && !Politician.politician.Data.IsDead)
            {
                foreach (PlayerControl player in Politician.campaignedPlayers)
                {
                    string suffix = Helpers.cs(Politician.color, " c");
                    if (player == null || player.Data.Disconnected || player == Politician.politician) continue;
                    if (Politician.politician == PlayerControl.LocalPlayer) setPlayerNameColor(player, Palette.CrewmateBlue);
                    if (Helpers.shouldShowGhostInfo())
                    {
                        player.cosmetics.nameText.text += suffix;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                                if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                    playerVoteArea.NameText.text += suffix;
                    }
                }
            }

            // Show Plaguebearer infected
            if (Plaguebearer.plaguebearer != null && !Plaguebearer.plaguebearer.Data.IsDead)
            {
                string suffix = Helpers.cs(Plaguebearer.color, " p");
                foreach (PlayerControl player in Plaguebearer.infectedPlayers)
                {
                    if (player == null || player.Data.Disconnected || player == Plaguebearer.plaguebearer) continue;
                    if (Plaguebearer.plaguebearer == PlayerControl.LocalPlayer) setPlayerNameColor(player, Color.black);
                    if (Helpers.shouldShowGhostInfo())
                    {
                        player.cosmetics.nameText.text += suffix;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                                if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                    playerVoteArea.NameText.text += suffix;
                    }
                }
            }

            // Show mayor for player
            if (Mayor.mayor != null && Mayor.mayor != PlayerControl.LocalPlayer)
                setPlayerNameColor(Mayor.mayor, Mayor.color);
            
            // Former Thieves
            if (Thief.formerThieves != null && (Thief.formerThieves.Contains(PlayerControl.LocalPlayer) || Helpers.shouldShowGhostInfo()))
            {
                string suffix = Helpers.cs(Thief.color, " $");
                foreach (PlayerControl player in Thief.formerThieves)
                {
                    player.cosmetics.nameText.text += suffix;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                            if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                playerVoteArea.NameText.text += suffix;
                }
            }

            // Lawyer
            bool localIsLawyer = Lawyer.lawyer != null && Lawyer.target != null && (Lawyer.lawyer == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo());
            bool localIsTarget = Lawyer.lawyer != null && Lawyer.target != null && Lawyer.target == PlayerControl.LocalPlayer && Lawyer.targetKnows;
            if (localIsLawyer || localIsTarget)
            {
                string suffix = Helpers.cs(Lawyer.color, " §");
                Lawyer.target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Lawyer.target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Executioner
            if (Executioner.executioner != null && Executioner.target != null && (Executioner.executioner == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo()))
            {
                string suffix = Helpers.cs(Executioner.color, " X");
                Executioner.target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Executioner.target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Guardian Angel
            bool localIsGA = GuardianAngel.guardianAngel != null && GuardianAngel.target != null && (GuardianAngel.guardianAngel == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo());
            bool localIsGATarget = GuardianAngel.guardianAngel != null && GuardianAngel.target != null && GuardianAngel.target == PlayerControl.LocalPlayer && GuardianAngel.targetKnows;
            if (localIsGA || localIsGATarget)
            {
                string suffix = Helpers.cs(GuardianAngel.color, " ★");
                GuardianAngel.target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == GuardianAngel.target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Lovers
            if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == PlayerControl.LocalPlayer || Lovers.lover2 == PlayerControl.LocalPlayer))
            {
                string suffix = Helpers.cs(Lovers.color, " ♥");
                Lovers.lover1.cosmetics.nameText.text += suffix;
                Lovers.lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.lover1.PlayerId == player.TargetPlayerId || Lovers.lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Add medic shield info:
            if (MeetingHud.Instance != null && Medic.medic != null && Medic.shielded != null && Medic.shieldVisible(Medic.shielded))
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (player.TargetPlayerId == Medic.shielded.PlayerId)
                    {
                        player.NameText.text = Helpers.cs(Medic.color, "[") + player.NameText.text + Helpers.cs(Medic.color, "]");
                    }
                }
            }

            // Add medic exsShield info:
            if (MeetingHud.Instance != null && Medic.exShielded != null && Medic.exShieldVisible(Medic.exShielded))
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (player.TargetPlayerId == Medic.exShielded.PlayerId)
                    {
                        player.NameText.text = Helpers.cs(Medic.color, "[") + player.NameText.text + Helpers.cs(Medic.color, "]");
                    }
                }
            }
        }

        static void updateShielded()
        {
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead)
            {
                Medic.shielded = null;
            }
        }

        static void timerUpdate()
        {
            var dt = Time.deltaTime;
            Swooper.invisibleTimer -= dt;
            Grenadier.flashTimer -= dt;
            Satelite.corpsesTrackingTimer -= dt;
            Banshee.scareTimer -= dt;
        }

        static void seerUpdate()
        {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;
            foreach (byte playerId in Seer.revealedIds)
            {
                PlayerControl target = Helpers.playerById(playerId);
                Color color = (target.Data.Role.IsImpostor || target.IsSheriff(out _) && Seer.sheriffShowsEvil || target.IsVeteran(out _) && Seer.veteranShowsEvil || target.isNeutralBenign() && Seer.benignNeutralsShowsEvil || target.isNeutralEvil() && Seer.evilNeutralsShowsEvil || target.isNeutralKilling() && Seer.killingNeutralsShowsEvil) ? Color.red : Color.green;
                if (playerIcons.ContainsKey(target.PlayerId))
                {
                    playerIcons[target.PlayerId].cosmetics.nameText.color = color;
                }
                setPlayerNameColor(target, color);
            }
        }

        static void updateBlindReport()
        {
            if (Blind.blind != null && Blind.blind == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead)
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
        }

        static void miniUpdate()
        {
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Mini.mini == Morphling.morphling && Morphling.morphTimer > 0f || Mini.mini == Glitch.glitch && Glitch.morphTimer > 0f || Mini.mini == Venerer.venerer && Venerer.abilityTimer > 0f || Mini.mini == Swooper.swooper && Swooper.isInvisible) return;

            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>";
            if (!Mini.isGrowingUpInMeeting && MeetingHud.Instance != null && Mini.ageOnMeetingStart != 0 && !(Mini.ageOnMeetingStart >= 18))
                suffix = " <color=#FAD934FF>(" + Mini.ageOnMeetingStart + ")</color>";

            Mini.mini.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            if (Morphling.morphling != null && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
                Morphling.morphling.cosmetics.nameText.text += suffix;
        }

        static void oracleUpdate()
        {
            if (Oracle.confessor != null && (Oracle.oracle == null || Oracle.oracle.Data.IsDead || Oracle.oracle.Data.Disconnected))
            {
                foreach (var state in MeetingHud.Instance.playerStates)
                {
                    if (Oracle.confessor.PlayerId != state.TargetPlayerId) continue;

                    if (Oracle.revealedFactionId == FactionId.Crewmate) state.NameText.text = state.NameText.text + $" <size=60%>(<color=#00FFFFFF>{Oracle.accuracy}% Crew</color>) </size>";
                    else if (Oracle.revealedFactionId == FactionId.Impostor) state.NameText.text = state.NameText.text + $" <size=60%>(<color=#FF0000FF>{Oracle.accuracy}% Imp</color>) </size>";
                    else state.NameText.text = state.NameText.text + $" <size=60%>(<color=#808080FF>{Oracle.accuracy}% Neut</color>) </size>";
                }
            }
        }
        
        static void updateImpostorKillButton(HudManager __instance)
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance)
            {
                __instance.KillButton.Hide();
                return;
            }
            bool enabled = !PlayerControl.LocalPlayer.Data.IsDead;
            if (Poisoner.poisoner != null && Poisoner.poisoner == PlayerControl.LocalPlayer)
                enabled = false;

            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();
        }

        static void updateReportButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance) __instance.ReportButton.Hide();
            else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
        }

        static void updateVentButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
            else if (PlayerControl.LocalPlayer.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled)
            {
                __instance.ImpostorVentButton.Show();

            }
            if (Rewired.ReInput.players.GetPlayer(0).GetButtonDown(RewiredConsts.Action.UseVent) && PlayerControl.LocalPlayer.roleCanUseVents() && __instance.ImpostorVentButton.currentTarget != null)
            {
                __instance.ImpostorVentButton.DoClick();
            }
        }

        static void updateUseButton(HudManager __instance)
        {
            if (MeetingHud.Instance) __instance.UseButton.Hide();
        }

        static void updateSabotageButton(HudManager __instance)
        {
            if (MeetingHud.Instance) __instance.SabotageButton.Hide();
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();
            setNameTags();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Seer
            seerUpdate();
            // Blind
            updateBlindReport();
            // Mini
            miniUpdate();
            // Oracle
            oracleUpdate();

            // Report, Sabotage, Use and Vent Button Disabling
            updateReportButton(__instance);
            updateVentButton(__instance);
            updateSabotageButton(__instance);
            updateUseButton(__instance);

            // Fix dead player's pets being visible by just always updating whether the pet should be visible at all.
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                var pet = target.GetPet();
                if (pet != null)
                {
                    pet.Visible = (PlayerControl.LocalPlayer.Data.IsDead && target.Data.IsDead || !target.Data.IsDead) && !target.inVent;
                }
            }
        }
    }
}