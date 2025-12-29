using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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
                    foreach (var morphling in Morphling.players)
                    {
                        if (morphling.morphTimer > 0f && morphling.morphTarget != null && morphling.player == player)
                            playerName = morphling.morphTarget.Data.PlayerName;
                    }
                    foreach (var glitch in Glitch.players)
                    {
                        if (Glitch.morphTimer > 0f && Glitch.morphPlayer != null && glitch.player == player)
                            playerName = Glitch.morphPlayer.Data.PlayerName;
                    }
                    var nameText = player.cosmetics.nameText;

                    nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                    nameText.color = nameText.color.SetAlpha(Chameleon.local.visibility(player.PlayerId));
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
            p.cosmetics.nameText.color = color.SetAlpha(Chameleon.local.visibility(p.PlayerId));
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

            if (localPlayer.isRole(RoleId.Dracula))
            {
                setPlayerNameColor(localPlayer, Dracula.color);
                var dracula = Dracula.getRole(localPlayer);
                var vampire = Dracula.getVampire(localPlayer);
                if (vampire != null && vampire.player != null)
                {
                    setPlayerNameColor(vampire.player, Dracula.color);
                }
                if (dracula.fakeVampire != null)
                {
                    setPlayerNameColor(dracula.fakeVampire, Dracula.color);
                }
            }

            // No else if here, as a Lover of team Vampires needs the colors
            if (localPlayer.isRole(RoleId.Vampire))
            {
                setPlayerNameColor(localPlayer, Vampire.color);
                var dracula = Vampire.getRole(localPlayer).dracula;
                if (dracula != null && dracula.player != null)
                {
                    setPlayerNameColor(dracula.player, Dracula.color);
                }
            }

            if (localPlayer.Data.Role.IsImpostor)
            {
                foreach (var vampire in Vampire.players)
                {
                    if (vampire.player != null && vampire.wasTeamRed)
                    {
                        setPlayerNameColor(vampire.player, Palette.ImpostorRed);
                    }
                }
                foreach (var dracula in Dracula.players)
                {
                    if (dracula.player != null && dracula.wasTeamRed)
                    {
                        setPlayerNameColor(dracula.player, Palette.ImpostorRed);
                    }
                }
                foreach (var agent in Agent.allPlayers) setPlayerNameColor(agent, Agent.color);
            }
        }

        static void setNameTags()
        {
            // Add medic shield info:
            foreach (var medic in Medic.players)
            {
                if (MeetingHud.Instance != null && Medic.shieldVisible(medic.shielded))
                {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == medic.shielded.PlayerId)
                            player.NameText.text = Helpers.cs(Medic.color, "[") + player.NameText.text + Helpers.cs(Medic.color, "]");
                }
            }

            // Add merc shield info:
            foreach (var mercenary in Mercenary.players)
            {
                if (MeetingHud.Instance != null && Mercenary.shieldVisible(mercenary.shielded))
                {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == mercenary.shielded.PlayerId)
                            player.NameText.text = Helpers.cs(Mercenary.color, "[") + player.NameText.text + Helpers.cs(Mercenary.color, "]");
                }
            }

            // Lawyer
            bool localIsLawyer = Lawyer.target != null && (PlayerControl.LocalPlayer.isRole(RoleId.Lawyer) || Helpers.shouldShowGhostInfo());
            bool localIsKnowingTarget = Lawyer.target != null && Lawyer.targetKnows && Lawyer.target == PlayerControl.LocalPlayer;
            if (localIsLawyer || (localIsKnowingTarget && Lawyer.hasAlivePlayers))
            {
                Color color = Lawyer.color;
                PlayerControl target = Lawyer.target;
                string suffix = Helpers.cs(color, " §");
                target.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, Lawyer.target) ? "" : suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Executioner
            if (Executioner.target != null && (PlayerControl.LocalPlayer.isRole(RoleId.Executioner) || Helpers.shouldShowGhostInfo()))
            {
                Color color = Executioner.color;
                PlayerControl target = Executioner.target;
                string suffix = Helpers.cs(color, " X");
                target.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, Executioner.target) ? "" : suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Guardian Angel
            bool localIsGA = GuardianAngel.target != null && (PlayerControl.LocalPlayer.isRole(RoleId.GuardianAngel) || Helpers.shouldShowGhostInfo());
            bool localIsKnowingGATarget = GuardianAngel.target != null && GuardianAngel.targetKnows && GuardianAngel.target == PlayerControl.LocalPlayer;
            if (localIsGA || (localIsKnowingGATarget && GuardianAngel.hasAlivePlayers))
            {
                Color color = GuardianAngel.color;
                PlayerControl target = GuardianAngel.target;
                string suffix = Helpers.cs(color, " ★");
                target.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, GuardianAngel.target) ? "" : suffix;;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Politician
            if ((PlayerControl.LocalPlayer.isRole(RoleId.Politician) || Helpers.shouldShowGhostInfo()) && Politician.exists)
            {
                foreach (var playerId in Politician.campaignedPlayers)
                {
                    PlayerControl player = Helpers.playerById(playerId);
                    if (player != null && !player.Data.Disconnected && !player.isRole(RoleId.Politician))
                    {
                        Color color = player.Data.IsDead ? Color.gray : Politician.color;
                        player.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, player) ? "" : Helpers.cs(color, " c");

                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                                if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                    playerVoteArea.NameText.text += Helpers.cs(color, " c");
                    }
                }
            }

            // Plaguebearer
            if ((PlayerControl.LocalPlayer.isRole(RoleId.Plaguebearer) || Helpers.shouldShowGhostInfo()) && Plaguebearer.exists)
            {
                foreach (var playerId in Plaguebearer.infectedPlayers)
                {
                    PlayerControl player = Helpers.playerById(playerId);
                    if (player != null && !player.Data.Disconnected && !player.isRole(RoleId.Plaguebearer))
                    {
                        Color color = player.Data.IsDead ? Color.gray : Plaguebearer.color;
                        player.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, player) ? "" : Helpers.cs(color, " p");

                        if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                            if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                playerVoteArea.NameText.text += Helpers.cs(color, " p");
                    }
                }
            }

            // Arsonist
            if ((PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) || Helpers.shouldShowGhostInfo()) && Arsonist.exists)
            {
                foreach (PlayerControl player in Arsonist.dousedPlayers)
                {
                    if (player != null && !player.Data.Disconnected)
                    {
                        Color color = player.Data.IsDead ? Color.gray : Arsonist.color;
                        player.cosmetics.nameText.text += Helpers.hidePlayerName(PlayerControl.LocalPlayer, player) ? "" : Helpers.cs(color, " ♨");

                        if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                            if (playerVoteArea.TargetPlayerId == player.PlayerId)
                                playerVoteArea.NameText.text += Helpers.cs(color, " ♨");
                    }
                }
            }

            // Former thief
            if (Thief.formerThiefs != null && (Thief.formerThiefs.Contains(PlayerControl.LocalPlayer) || Helpers.shouldShowGhostInfo()))
            {
                string suffix = Helpers.cs(Thief.color, " $");
                foreach (var formerThief in Thief.formerThiefs)
                    formerThief.cosmetics.nameText.text += suffix;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Thief.formerThiefs.Any(x => x.PlayerId == player.TargetPlayerId))
                            player.NameText.text += suffix;
            }

            // Lovers
            if (PlayerControl.LocalPlayer.isLovers() && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                string suffix = Lovers.getIcon(PlayerControl.LocalPlayer);
                var lover1 = PlayerControl.LocalPlayer;
                var lover2 = PlayerControl.LocalPlayer.getPartner();

                lover1.cosmetics.nameText.text += suffix;
                lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (lover1.PlayerId == player.TargetPlayerId || lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Show flashed players for Grenadier
            if (Grenadier.exists && (PlayerControl.LocalPlayer.isRole(RoleId.Grenadier) || Helpers.shouldShowGhostInfo()))
            {
                foreach (PlayerControl player in Grenadier.flashedPlayers)
                {
                    if (!player.Data.Role.IsImpostor && !player.Data.IsDead)
                    {
                        setPlayerNameColor(player, Color.black);
                    }
                }
            }

            // Display lighter / darker color for all alive players
            if (PlayerControl.LocalPlayer != null && MeetingHud.Instance != null && showLighterDarker)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    var target = Helpers.playerById(player.TargetPlayerId);
                    if (target != null) player.NameText.text += $" ({(Helpers.isLighterColor(target) ? "L" : "D")})";
                }
            }
        }

        static void updateShielded()
        {

        }

        static void timerUpdate()
        {
            var dt = Time.deltaTime;
            Grenadier.flashTimer -= dt;
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
            if (Poisoner.exists && PlayerControl.LocalPlayer.isRole(RoleId.Poisoner)) enabled = Poisoner.getRole(PlayerControl.LocalPlayer).enabledKillButton;

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
            else if (PlayerControl.LocalPlayer.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();
            if (Rewired.ReInput.players.GetPlayer(0).GetButtonDown(RewiredConsts.Action.UseVent) && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.roleCanUseVents())
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

        static void seerUpdate()
        {
            if (!PlayerControl.LocalPlayer.isRole(RoleId.Seer)) return;
            var seerRole = RoleBase<Seer>.getRole(PlayerControl.LocalPlayer);
            if (seerRole.player != PlayerControl.LocalPlayer) return;

            foreach (PlayerControl player in seerRole.revealedPlayers)
            {
                bool evil = false;

                if (player.Data.Role.IsImpostor || player.isBenignNeutral() && Seer.benignNeutralsShowsEvil || player.isEvilNeutral() && Seer.evilNeutralsShowsEvil || player.isKillingNeutral() && Seer.killingNeutralsShowsEvil) evil = true;
                else evil = false;

                setPlayerNameColor(player, evil ? Color.red : Color.green);
            }
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
            // Oracle
            Oracle.update(MeetingHud.Instance);

            HudUpdate();

            // Deputy Sabotage, Use and Vent Button Disabling
            updateReportButton(__instance);
            updateVentButton(__instance);
            // Meeting hide buttons if needed (used for the map usage, because closing the map would show buttons)
            updateSabotageButton(__instance);
            updateUseButton(__instance);
            if (!MeetingHud.Instance) __instance.AbilityButton?.Update();

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