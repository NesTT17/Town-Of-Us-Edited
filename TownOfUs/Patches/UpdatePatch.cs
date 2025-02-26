using HarmonyLib;
using UnityEngine;
using TownOfUs.Objects;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using System;
using AmongUs.Data;

namespace TownOfUs.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        private static Dictionary<byte, (string name, Color color)> TagColorDict = new();
        static void resetNameTagsAndColors() {
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

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setPlayerNameColorForSnitch(PlayerControl p, Color color) {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null && Snitch.seeInMeeting)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors() {
            var localPlayer = PlayerControl.LocalPlayer;
            var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
            setPlayerNameColor(localPlayer, localRole.color);

            if (Dracula.dracula != null && Dracula.dracula == localPlayer) {
                setPlayerNameColor(Dracula.dracula, Dracula.color);
                if (Vampire.vampire != null) {
                    setPlayerNameColor(Vampire.vampire, Vampire.color);
                }
                if (Dracula.fakeVampire != null) {
                    setPlayerNameColor(Dracula.fakeVampire, Vampire.color);
                }
            }

            if (Vampire.vampire != null && Vampire.vampire == localPlayer) {
                setPlayerNameColor(Vampire.vampire, Vampire.color);
                if (Dracula.dracula != null) {
                    setPlayerNameColor(Dracula.dracula, Dracula.color);
                }
            }

            if (Vampire.vampire != null && Vampire.wasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Vampire.vampire, Palette.ImpostorRed);
            }
            if (Dracula.dracula != null && Dracula.wasTeamRed && localPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Dracula.dracula, Palette.ImpostorRed);
            }
        }

        static void setNameTags() {
            // Reveal players for Snitch
            if (Snitch.snitch != null) {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                int numberOfTasks = playerTotal - playerCompleted;
                if (numberOfTasks <= Snitch.taskCountForReveal && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.isNeutral() && Snitch.includeNeutral || PlayerControl.LocalPlayer.isNeutralKiller() && Snitch.includeKillingNeutral)) {
                    setPlayerNameColor(Snitch.snitch, Snitch.color);
                } else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0) {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        string colorBlindText = "";
                        bool revealImp = p.Data.Role.IsImpostor;
                        bool revealNeut = p.isNeutral() && Snitch.includeNeutral;
                        bool revealKNeut = p.isNeutralKiller() && Snitch.includeKillingNeutral;
                        Color color = Color.white;
                        if (revealImp) {
                            color = Palette.ImpostorRed;
                            colorBlindText = " (Imp)";
                        } else if (revealNeut || revealKNeut) {
                            color = Color.gray;
                            colorBlindText = " (Neutral)";
                        }

                        if (!p.Data.IsDead && (revealImp || revealNeut || revealKNeut)) {
                            setPlayerNameColorForSnitch(p, color);
                            if (TOUMapOptions.extendedColorblindMode) {
                                p.cosmetics.nameText.text += colorBlindText;

                                if (MeetingHud.Instance != null && Snitch.seeInMeeting)
                                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                        if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                                            player.NameText.text += colorBlindText;
                            }
                        }
                    }
                }
            }

            // Lovers
            if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == PlayerControl.LocalPlayer || Lovers.lover2 == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())) {
                string suffix = Helpers.cs(Lovers.color, " ♥");
                Lovers.lover1.cosmetics.nameText.text += suffix;
                Lovers.lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.lover1.PlayerId == player.TargetPlayerId || Lovers.lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Mayor
            if (Mayor.mayor != null && Mayor.isRevealed && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer != Mayor.mayor) {
                setPlayerNameColor(Mayor.mayor, Mayor.color);
            }

            // Add medic shield info:
            if (MeetingHud.Instance != null && Medic.medic != null && Medic.shielded != null && Medic.shieldVisible(Medic.shielded)) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.TargetPlayerId == Medic.shielded.PlayerId)
                        player.NameText.text += Helpers.cs(Medic.color, " <b>+</b>");
            }

            // Executioner
            if (Executioner.executioner != null && Executioner.target != null && (Executioner.executioner == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())) {
                Color color = Executioner.color;
                PlayerControl target = Executioner.target;
                string suffix = Helpers.cs(color, " X");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Lawyer
            if (Lawyer.lawyer != null && Lawyer.target != null && (Lawyer.lawyer == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())) {
                Color color = Lawyer.color;
                PlayerControl target = Lawyer.target;
                string suffix = Helpers.cs(color, " §");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Guardian Angel
            if (GuardianAngel.guardianAngel != null && GuardianAngel.target != null && (GuardianAngel.guardianAngel == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())) {
                Color color = GuardianAngel.color;
                PlayerControl target = GuardianAngel.target;
                string suffix = Helpers.cs(color, " ★");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Seer
            if (Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead) {
                foreach (PlayerControl player in Seer.revealedPlayers) {
                    if (player != null) {
                        Color color = (player.Data.Role.IsImpostor || player.isNeutral() && Seer.neutRed || player.isNeutralKiller() && Seer.kneutRed) ? Color.red : Color.green;
                        string colorBlindText = (player.Data.Role.IsImpostor || player.isNeutral() && Seer.neutRed || player.isNeutralKiller() && Seer.kneutRed) ? " (Red)" : " (Green)";
                        setPlayerNameColor(player, color);
                        if (TOUMapOptions.extendedColorblindMode) {
                            player.cosmetics.nameText.text += colorBlindText;

                            if (MeetingHud.Instance != null)
                                foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                                    if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                        playerVoteArea.NameText.text += colorBlindText;
                        }
                    }
                }
            }

            // Add ingame blackmailed info
            if (Blackmailer.blackmailed != null && !Blackmailer.blackmailed.Data.IsDead && (PlayerControl.LocalPlayer == Blackmailer.blackmailer || Helpers.shouldShowGhostInfo()))
                Blackmailer.blackmailed.cosmetics.nameText.text += Helpers.cs(Helpers.impAbilityTargetColor, " (BM)");
            
            // Show flashed players for Grenadier
            if (Grenadier.grenadier != null && (Grenadier.grenadier == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())) {
                foreach (PlayerControl player in Grenadier.flashedPlayers) {
                    if (!player.Data.Role.IsImpostor && !player.Data.IsDead) {
                        setPlayerNameColor(player, Color.black);
                        if (TOUMapOptions.extendedColorblindMode) player.cosmetics.nameText.text += " (Flashed)";
                    }
                }
            }

            // Highlight confessor if oracle is dead
            if (Oracle.confessor != null && (Oracle.oracle.Data.IsDead || Oracle.oracle.Data.Disconnected)) {
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Oracle.confessor.PlayerId) {
                            if (Oracle.revealedFaction == FactionId.Crewmate) player.NameText.text = $"<color=#00FFFFFF>({Oracle.accuracy * 10}% Crew) </color>" + player.NameText.text;
                            else if (Oracle.revealedFaction == FactionId.Impostor) player.NameText.text = $"<color=#FF0000FF>({Oracle.accuracy * 10}% Imp) </color>" + player.NameText.text;
                            else player.NameText.text = $"<color=#808080FF>({Oracle.accuracy * 10}% Neut) </color>" + player.NameText.text;
                        }
            }

            // Display lighter / darker color for all alive players
            if (PlayerControl.LocalPlayer != null && MeetingHud.Instance != null && TOUMapOptions.showLighterDarker) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    var target = Helpers.playerById(player.TargetPlayerId);
                    if (target != null)  player.NameText.text += $" ({(Helpers.isLighterColor(target) ? "L" : "D")})";
                }
            }
        }

        static void updateShielded() {
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead) {
                Medic.shielded = null;
            }
        }

        static void updateMercenaryShielded() {
            if (Mercenary.shielded == null) return;

            if (Mercenary.shielded.Data.IsDead || Mercenary.mercenary == null || Mercenary.mercenary.Data.IsDead) {
                Mercenary.shielded = null;
            }
        }

        static void timerUpdate() {
            var dt = Time.deltaTime;
            Swooper.invisibleTimer -= dt;
            Phantom.ghostTimer -= dt;
            Grenadier.flashTimer -= dt;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance) {
                __instance.KillButton.Hide();
                return;
            }
            bool enabled = !PlayerControl.LocalPlayer.Data.IsDead;
            if (Poisoner.poisoner != null && Poisoner.poisoner == PlayerControl.LocalPlayer)
                enabled = false;
            
            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();
        }

        static void updateReportButton(HudManager __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance) __instance.ReportButton.Hide();
            else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
        }
         
        static void updateVentButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance) __instance.ImpostorVentButton.Hide();
            else if (PlayerControl.LocalPlayer.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();
        }

        static void updateUseButton(HudManager __instance) {
            if (MeetingHud.Instance) __instance.UseButton.Hide();
        }

        static void updateBlindReport() {
            if (Blind.blind == null || PlayerControl.LocalPlayer != Blind.blind) return;
            DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();
            updateMercenaryShielded();
            setNameTags();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();

            // Report, Use and Vent Button Disabling
            updateReportButton(__instance);
            updateVentButton(__instance);
            updateUseButton(__instance);
            updateBlindReport();
            if (!MeetingHud.Instance) __instance.AbilityButton?.Update();

            // Fix dead player's pets being visible by just always updating whether the pet should be visible at all.
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                var pet = target.GetPet();
                if (pet != null) {
                    pet.Visible = (PlayerControl.LocalPlayer.Data.IsDead && target.Data.IsDead || !target.Data.IsDead) && !target.inVent;
                }
            }
        }
    }
}
