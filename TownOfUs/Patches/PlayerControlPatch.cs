using System;
using System.Linq;
using UnityEngine;
using Sentry.Internal.Extensions;
using System.Collections.Generic;
using Assets.CoreScripts;

namespace TownOfUs.Patches {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch {
        // Update functions
        static void setBasePlayerOutlines() {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

                bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
                bool isMorphedGlitch = Glitch.players.Any(x => x.player == target && Glitch.morphPlayer != null && Glitch.morphTimer > 0f);
                bool isMorphedSwooper = Swooper.players.Any(x => x.player == target && x.isInvisble);
                bool isMorphedVenerer = Venerer.players.Any(x => x.player == target && x.morphTimer > 0f);
 
                bool hasVisibleShield = false;
                Color color = Medic.shieldedColor;
                
                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && firstKillPlayer != null && shieldFirstKill && ((target == firstKillPlayer && !isMorphedMorphling && !isMorphedGlitch && !isMorphedSwooper && !isMorphedVenerer) || (isMorphedMorphling && Morphling.getRole(target).morphTarget == firstKillPlayer) || (isMorphedGlitch && Glitch.morphPlayer == firstKillPlayer)))
                {
                    hasVisibleShield = true;
                    color = Color.blue;
                }

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Medic.shieldVisible(target))
                    hasVisibleShield = true;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && GuardianAngel.protectVisible(target))
                {
                    hasVisibleShield = true;
                    color = Color.yellow;
                }

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Mercenary.shieldVisible(target))
                    hasVisibleShield = true;

                if (hasVisibleShield)
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                    target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
                }
                else
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                }                
            }
        }

        static void setPetVisibility() {
            bool localalive = PlayerControl.LocalPlayer.Data.IsDead;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                bool playeralive = !player.Data.IsDead;
                player.cosmetics.SetPetVisible((localalive && playeralive) || !localalive);
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor ||!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead) { // !isImpostor || !canMove || isDead
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            if (Agent.exists)
            {
                if (Agent.impostorsCanKillAnyone)
                    target = Helpers.setTarget(false, true);
                else
                {
                    var untargetables = new List<PlayerControl>();
                    untargetables.AddRange(Agent.allPlayers);
                    untargetables.AddRange(Dracula.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    untargetables.AddRange(Vampire.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    target = Helpers.setTarget(true, true, untargetables);
                }
            }
            else
            {
                var untargetables = new List<PlayerControl>();
                untargetables.AddRange(Dracula.players.Where(x => x.wasTeamRed).Select(x => x.player));
                untargetables.AddRange(Vampire.players.Where(x => x.wasTeamRed).Select(x => x.player));
                target = Helpers.setTarget(true, true, untargetables);
            }

            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        public static bool ShouldSeePlayerInfo(PlayerControl player)
        {
            if (player == null || player.Data == null)
                return false;

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null) return false;

            if (local.Data.Role.IsImpostor && player.Data.Role.IsImpostor && !Agent.exists)
            {
                return true;
            }

            return
                Lawyer.lawyerKnowsRole && local.isRole(RoleId.Lawyer) && player == Lawyer.target ||
                GuardianAngel.knowsRole && local.isRole(RoleId.GuardianAngel) && player == GuardianAngel.target ||
                player.isRole(RoleId.Mayor) && player != local && !player.Data.IsDead ||
                local.hasModifier(RoleId.Sleuth) && Sleuth.players.Any(x => x.player.PlayerId == local.PlayerId && x.reported.Contains(player)) ||
                local == player ||
                local.Data.IsDead;
        }

        public static void updatePlayerInfo()
        {
            Vector3 colorBlindTextMeetingInitialLocalPos = new Vector3(0.3384f, -0.16666f, -0.01f);
            Vector3 colorBlindTextMeetingInitialLocalScale = new Vector3(0.9f, 1f, 1f);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                // Colorblind Text in Meeting
                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active)
                {
                    playerVoteArea.ColorBlindName.transform.localPosition = colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                    playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
                    playerVoteArea.ColorBlindName.color = Palette.PlayerColors[p.Data.DefaultOutfit.ColorId];
                }

                // Colorblind Text During the round
                if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active)
                {
                    p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
                    p.cosmetics.colorBlindText.color = Palette.PlayerColors[p.Data.DefaultOutfit.ColorId];
                }
                p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);  // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

                if (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer.isRole(RoleId.Lawyer) && p == Lawyer.target || GuardianAngel.knowsRole && PlayerControl.LocalPlayer.isRole(RoleId.GuardianAngel) && p == GuardianAngel.target || p.isRole(RoleId.Mayor) && p != PlayerControl.LocalPlayer && !p.Data.IsDead || PlayerControl.LocalPlayer.hasModifier(RoleId.Sleuth) && Sleuth.players.Any(x => x.player.PlayerId == PlayerControl.LocalPlayer.PlayerId && x.reported.Contains(p)) || PlayerControl.LocalPlayer == p || PlayerControl.LocalPlayer.Data.IsDead)
                {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null)
                    {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.transform.localPosition += Vector3.up * 0.225f;
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                        playerInfo.color = playerInfo.color.SetAlpha(1f);
                    }

                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null)
                    {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null)
                    {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true, true, true);
                    string roleText = RoleInfo.GetRolesString(p, true, ghostsSeeModifier);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";
                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == PlayerControl.LocalPlayer)
                    {
                        if (p.Data.IsDead) roleNames = roleText;
                        playerInfoText = $"{roleNames}";
                        if (HudManager.Instance.TaskPanel != null)
                        {
                            TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"Tasks {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (ghostsSeeRoles && ghostsSeeInformation)
                    {
                        playerInfoText = $"{roleText} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (ghostsSeeInformation)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (ghostsSeeRoles)
                    {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }

                    if (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer.isRole(RoleId.Lawyer) && p == Lawyer.target || GuardianAngel.knowsRole && PlayerControl.LocalPlayer.isRole(RoleId.GuardianAngel) && p == GuardianAngel.target || p.isRole(RoleId.Mayor) && p != PlayerControl.LocalPlayer && !p.Data.IsDead || PlayerControl.LocalPlayer.hasModifier(RoleId.Sleuth) && Sleuth.players.Any(x => x.player.PlayerId == PlayerControl.LocalPlayer.PlayerId && x.reported.Contains(p)))
                    {
                        roleText = RoleInfo.GetRolesString(p, true, false, true);
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible);
                    if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }
            }
        }

        static bool mushroomSaboWasActive = false;
        static void morphlingAndCamouflagerUpdate() {
            bool mushRoomSaboIsActive = Helpers.MushroomSabotageActive();
            if (!mushroomSaboWasActive) mushroomSaboWasActive = mushRoomSaboIsActive;

            float oldCamouflageTimer = Camouflager.camouflageTimer;
            Dictionary<byte, float> oldMorphTimer = new();
            Dictionary<byte, float> oldMimicTimer = new();
            Dictionary<byte, float> oldAbilityTimer = new();
            Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
            foreach (var morphling in Morphling.players) {
                oldMorphTimer[morphling.player.PlayerId] = morphling.morphTimer;
                morphling.morphTimer = Mathf.Max(0f, morphling.morphTimer - Time.fixedDeltaTime);
            }            
            foreach (var glitch in Glitch.players) {
                oldMimicTimer[glitch.player.PlayerId] = Glitch.morphTimer;
                Glitch.morphTimer = Mathf.Max(0f, Glitch.morphTimer - Time.fixedDeltaTime);
            }
            foreach (var venerer in Venerer.players) {
                oldAbilityTimer[venerer.player.PlayerId] = Glitch.morphTimer;
                venerer.morphTimer = Mathf.Max(0f, venerer.morphTimer - Time.fixedDeltaTime);
            }

            if (mushRoomSaboIsActive) return;

            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f)
            {
                Camouflager.resetCamouflage();
                foreach (var morphling in Morphling.players)
                {
                    if (morphling.morphTimer > 0f && morphling.morphTarget != null)
                    {
                        PlayerControl target = morphling.morphTarget;
                        morphling.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                foreach (var glitch in Glitch.players)
                {
                    if (Glitch.morphTimer > 0f && Glitch.morphPlayer != null)
                    {
                        PlayerControl target = Glitch.morphPlayer;
                        glitch.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                foreach (var venerer in Venerer.players)
                {
                    if (venerer.morphTimer > 0f)
                    {
                        venerer.player.setLook("", 15, "", "", "", "");
                        venerer.player.cosmetics.colorBlindText.text = "";
                    }
                }
            }

            // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
            if (mushroomSaboWasActive)
            {
                foreach (var morphling in Morphling.players)
                {
                    if (morphling.morphTimer > 0f && morphling.morphTarget != null)
                    {
                        PlayerControl target = morphling.morphTarget;
                        morphling.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                foreach (var glitch in Glitch.players)
                {
                    if (Glitch.morphTimer > 0f && Glitch.morphPlayer != null)
                    {
                        PlayerControl target = Glitch.morphPlayer;
                        glitch.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                foreach (var venerer in Venerer.players)
                {
                    if (venerer.morphTimer > 0f)
                    {
                        venerer.player.setLook("", 15, "", "", "", "");
                        venerer.player.cosmetics.colorBlindText.text = "";
                    }
                }
                if (Camouflager.camouflageTimer > 0)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        player.setLook("", 6, "", "", "", "");
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f)
            {
                foreach (var morphling in Morphling.players)
                {
                    if (morphling.morphTimer <= 0f && oldMorphTimer.TryGetValue(morphling.player.PlayerId, out float value) && value > 0f) morphling.resetMorph();
                }
                foreach (var glitch in Glitch.players)
                {
                    if (Glitch.morphTimer <= 0f && oldMimicTimer.TryGetValue(glitch.player.PlayerId, out float value) && value > 0f) glitch.resetMorph();
                }
                foreach (var venerer in Venerer.players)
                {
                    if (venerer.morphTimer <= 0f && oldAbilityTimer.TryGetValue(venerer.player.PlayerId, out float value) && value > 0f) venerer.resetMorph();
                }
            }
            mushroomSaboWasActive = false;
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            if (PlayerControl.LocalPlayer == __instance)
            {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

                // Update Player Info
                updatePlayerInfo();

                //Update pet visibility
                setPetVisibility();

                // Impostor
                impostorSetTarget();
                // Morphling and Camouflager
                morphlingAndCamouflagerUpdate();
            }

            FixedUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static bool Prefix(PlayerControl __instance) {
            Helpers.handleVampireKillOnBodyReport();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            bool isMedicReport = PlayerControl.LocalPlayer.isRole(RoleId.Medic) && Medic.allPlayers.Any(x => x.PlayerId == __instance.PlayerId) && Medic.getInfoOnReport;
            bool isDetectiveReport = PlayerControl.LocalPlayer.isRole(RoleId.Detective) && Detective.allPlayers.Any(x => x.PlayerId == __instance.PlayerId) && Detective.getInfoOnReport;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();
                if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport)
                    {
                        if (timeSinceDeath < Medic.reportNameDuration * 1000)
                        {
                            msg = $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                        else if (timeSinceDeath < Medic.reportColorDuration * 1000)
                        {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting) ? "lighter" : "darker";
                            msg = $"Body Report: The killer appears to be a {typeOfColor} color! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                        else
                        {
                            msg = $"Body Report: The corpse is too old to gain information from! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                    }
                    else if (isDetectiveReport)
                    {
                        if (timeSinceDeath < Detective.reportRoleDuration * 1000)
                        {
                            msg = $"Body Report: The killer appears to be {RoleInfo.GetRolesString(deadPlayer.killerIfExisting, false, false, true)}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                        else if (timeSinceDeath < Detective.reportFactionDuration * 1000)
                        {
                            var factionId = "";
                            if (deadPlayer.killerIfExisting.Data.Role.IsImpostor) factionId = "Impostor";
                            else if (deadPlayer.killerIfExisting.isAnyNeutral()) factionId = "Neutral";
                            else factionId = "Crewmate";
                            msg = $"Body Report: The killer appears to be a {factionId}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                        else
                        {
                            msg = $"Body Report: The corpse is too old to gain information from! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);

                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }
            }

            // Sleuth report
            bool isSleuthReport = PlayerControl.LocalPlayer.hasModifier(RoleId.Sleuth) && Sleuth.allPlayers.Any(x => x.PlayerId == __instance.PlayerId);
            if (isSleuthReport)
            {
                PlayerControl reported = Helpers.playerById(target.PlayerId);
                Sleuth.getModifier(PlayerControl.LocalPlayer).reported.Add(reported);
            }

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!target.Disconnected && player.PlayerId == target.PlayerId)
                {
                    if (Plaguebearer.isInfected(PlayerControl.LocalPlayer) || Plaguebearer.isInfected(player))
                    {
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, player);
                    }
                    if (Politician.isCampaigned(PlayerControl.LocalPlayer) || Politician.isCampaigned(player))
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, player);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static class HandleMurderRequestPatch
    {
        static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            __instance.isKilling = false;
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
                return false;
            if (!target || __instance.Data.Disconnected) // MODIFIED
            {
                int num = target ? target.PlayerId : -1;
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            NetworkedPlayerInfo data = target.Data;
            if (data == null || data.IsDead || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
            {
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            if (MeetingHud.Instance)
            {
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            __instance.isKilling = true;
            __instance.RpcMurderPlayer(target, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.hasFakeTasks())
                target.clearAllTasks();

            // First kill (set before lover suicide)
            if (firstKillName == "") firstKillName = target.Data.PlayerName;

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost)
            {
                foreach (var lawyer in Lawyer.allPlayers)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    writer.Write(lawyer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerPromotesToPursuer(lawyer.PlayerId);
                }
            }

            // Survivor promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == GuardianAngel.target && AmongUsClient.Instance.AmHost)
            {
                foreach (var ga in GuardianAngel.allPlayers)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                    writer.Write(ga.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelPromotes(ga.PlayerId);
                }
            }

            // Amnesiac promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Executioner.target && AmongUsClient.Instance.AmHost)
            {
                foreach (var executioner in Executioner.allPlayers)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToAmnesiac, Hazel.SendOption.Reliable, -1);
                    writer.Write(executioner.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerPromotesToAmnesiac(executioner.PlayerId);
                }
            }

            if (Mystic.exists && !PlayerControl.LocalPlayer.isFlashedByGrenadier() && (PlayerControl.LocalPlayer.isRole(RoleId.Mystic) || Helpers.shouldShowGhostInfo()))
                Helpers.showFlash(Mystic.color, 1f, "Mystic Info: Someone died");

            if (Vip.exists && target.hasModifier(RoleId.Vip))
                Helpers.showFlash(Vip.color);
                        
            __instance.OnKill(target);
            target.OnDeath(__instance);

            if (target != null)
            {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class PlayerControlSetCoolDownPatch {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]float time) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (PlayerControl.LocalPlayer.isRole(RoleId.BountyHunter)) addition = BountyHunter.punishmentTime;

            __instance.killTimer = Mathf.Clamp(time, 0f, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch {
        public static bool hideNextAnimation = false;
        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)]ref PlayerControl source, [HarmonyArgument(1)]ref PlayerControl target) {
            if (hideNextAnimation)
                source = target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch {
        private static int? colorId = null;
        public static void Prefix(PlayerControl source, bool canMove)
        {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (Morphling.players.Any(x => x.player.PlayerId == source.Data.PlayerId))
            {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
            if (Glitch.players.Any(x => x.player.PlayerId == source.Data.PlayerId))
            {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove) {
            if (colorId.HasValue) source.RawSetColor(colorId.Value);
            colorId = null;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);


            // Remove fake tasks when player dies
            if (__instance.hasFakeTasks())
                __instance.clearAllTasks();

            // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == Lawyer.target)
            {
                if (AmongUsClient.Instance.AmHost && ((!Lawyer.target.isRole(RoleId.Jester)) || Lawyer.targetWasGuessed))
                {
                    foreach (var lawyer in Lawyer.allPlayers)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                        writer.Write(lawyer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.lawyerPromotesToPursuer(lawyer.PlayerId);
                    }
                }
            }

            // Survivor promotion trigger on exile (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == GuardianAngel.target && AmongUsClient.Instance.AmHost)
            {
                foreach (var ga in GuardianAngel.allPlayers)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                    writer.Write(ga.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelPromotes(ga.PlayerId);
                }
            }
            
            __instance.OnDeath(killer: null);
            
            if (__instance != null)
            {
                Transform playerInfoTransform = __instance.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }
    }
    
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] {typeof(PlayerControl), typeof(DisconnectReasons) })]
    public static class GameDataHandleDisconnectPatch {
        public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (MeetingHud.Instance)
            {
                MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, player.PlayerId);
            }

            if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                if (player != null && !player.Data.IsDead) overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Disconnect, null);
            }

            if (MeetingHud.Instance)
            {
                foreach (var pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == player.PlayerId)
                    {
                        pva.Overlay.gameObject.SetActive(true);

                        pva.UnsetVote();
                        var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                        if (voteAreaPlayer?.AmOwner == false) continue;
                        MeetingHud.Instance.ClearVote();
                    }
                }
            }

            if (RoleDraftEx.isEnabled && RoleDraftEx.isRunning)
            {
                if (RoleDraftEx.pickOrder != null && RoleDraftEx.pickOrder.Count > 0 && RoleDraftEx.pickOrder.Any(x => x == player.PlayerId))
                {
                    RoleDraftEx.pickOrder.Remove(player.PlayerId);
                    RoleDraftEx.timer = 0;
                    RoleDraftEx.picked = true;
                }
            }
        }
    }
}
