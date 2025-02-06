using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using static TownOfUs.TownOfUs;
using static TownOfUs.GameHistory;
using TownOfUs.Utilities;
using UnityEngine;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using Sentry.Internal.Extensions;
using Reactor.Utilities.Extensions;
using AmongUs.Data;

namespace TownOfUs.Patches {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch {
        // Helpers
        static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = true, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null) {
            PlayerControl result = null;
            float num = AmongUs.GameOptions.GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor)) {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents)) {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)) {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        static void setPlayerOutline(PlayerControl target, Color color) {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines() {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

                bool hasVisibleShield = false;
                Color color = Color.white;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && !Helpers.isActiveCamoComms() && Medic.shieldVisible(target)) {
                    hasVisibleShield = true;
                    color = Color.cyan;
                }

                if (hasVisibleShield) {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                    target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
                }
                else {
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
            target = setTarget(true, true, new List<PlayerControl>() { Vampire.wasImpostor ? Vampire.vampire : null, Dracula.wasImpostor ? Dracula.dracula : null });
            
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        public static void updatePlayerInfo() {
            Vector3 colorBlindTextMeetingInitialLocalPos = new Vector3(0.3384f, -0.16666f, -0.01f);
            Vector3 colorBlindTextMeetingInitialLocalScale = new Vector3(0.9f, 1f, 1f);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                
                // Colorblind Text in Meeting
                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active) {
                    playerVoteArea.ColorBlindName.transform.localPosition = colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                    playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
                }

                // Colorblind Text During the round
                if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active) {
                    p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
                }

                p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);  // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

                if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target) || (Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && Sleuth.reported.Any(x => x.PlayerId == p.PlayerId)) || p == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead) {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null) {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.transform.localPosition += Vector3.up * 0.225f;
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                        playerInfo.color = playerInfo.color.SetAlpha(1f);
                    }
    
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null) {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null) {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true, false);
                    string roleText = RoleInfo.GetRolesString(p, true, TOUMapOptions.ghostsSeeModifier);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";                    
                    if (p == PlayerControl.LocalPlayer) {
                        if (p.Data.IsDead) roleNames = roleText;
                        playerInfoText = $"{roleNames}";
                        if (Mercenary.mercenary != null && Mercenary.mercenary == p) playerInfoText = $"{roleNames}" + Helpers.cs(Mercenary.color, $" ({Mercenary.numOfNeededAttempts - Mercenary.attemptedMurder})");
                        if (HudManager.Instance.TaskPanel != null) {
                            TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"Tasks {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (TOUMapOptions.ghostsSeeRoles && TOUMapOptions.ghostsSeeInformation ) {
                        playerInfoText = $"{roleText} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (TOUMapOptions.ghostsSeeInformation ) {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (TOUMapOptions.ghostsSeeRoles) {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }
                    if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target) || (Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && Sleuth.reported.Any(x => x.PlayerId == p.PlayerId))) {
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

            if (Helpers.isCamoComms() && !Helpers.isActiveCamoComms()) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CamouflagerCamouflage, SendOption.Reliable, -1);
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.camouflagerCamouflage(0);
            }
            
            float oldCamouflageTimer = Camouflager.camouflageTimer;
            float oldMorphTimer = Morphling.morphTimer;
            Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
            Morphling.morphTimer = Mathf.Max(0f, Morphling.morphTimer - Time.fixedDeltaTime);

            if (mushRoomSaboIsActive) return;
            if (Helpers.isCamoComms()) return;
            if (Helpers.wasActiveCamoComms() && Camouflager.camouflageTimer <= 0f) {
                Camouflager.resetCamouflage();
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null) {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f) {
                Camouflager.resetCamouflage();
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null) {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
            if (mushroomSaboWasActive) {
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null) {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (Camouflager.camouflageTimer > 0) {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        player.setLook("", 6, "", "", "", "");
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f && oldMorphTimer > 0f && Morphling.morphTimer <= 0f && Morphling.morphling != null)
                Morphling.resetMorph();
            mushroomSaboWasActive = false;
        }

        static void morphlingSetTarget() {
            if (Morphling.morphling == null || Morphling.morphling != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = setTarget();
            setPlayerOutline(Morphling.currentTarget, Helpers.impAbilityTargetColor);
        }

        static void snitchUpdate() {
            if (Snitch.localArrows == null) return;

            foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);

            if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
            int numberOfTasks = playerTotal - playerCompleted;

            if (numberOfTasks <= Snitch.taskCountForReveal && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.isNeutral() && Snitch.includeNeutral || PlayerControl.LocalPlayer.isNeutralKiller() && Snitch.includeKillingNeutral)) {
                if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Color.blue));
                if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null) {
                    Snitch.localArrows[0].arrow.SetActive(true);
                    Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
                }
            }
            else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0) {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool revealImp = p.Data.Role.IsImpostor;
                    bool revealNeut = p.isNeutral() && Snitch.includeNeutral;
                    bool revealKNeut = p.isNeutralKiller() && Snitch.includeKillingNeutral;
                    bool arrowforNeut = revealKNeut || revealNeut;

                    if (!p.Data.IsDead && (revealImp || revealNeut || revealKNeut)) {
                        if (arrowIndex >= Snitch.localArrows.Count) {
                            Snitch.localArrows.Add(new Arrow(Palette.ImpostorRed));
                        }
                        if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null) {
                            Snitch.localArrows[arrowIndex].arrow.SetActive(true);
                            Snitch.localArrows[arrowIndex].Update(p.transform.position, arrowforNeut ? Color.gray : Palette.ImpostorRed);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        static void sheriffSetTarget() {
            if (Sheriff.sheriff == null || Sheriff.sheriff != PlayerControl.LocalPlayer) return;
            Sheriff.currentTarget = setTarget();
            setPlayerOutline(Sheriff.currentTarget, Sheriff.color);
        }

        static void shifterSetTarget() {
            if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;
            Shifter.currentTarget = setTarget();
            if (Shifter.futureShift == null) setPlayerOutline(Shifter.currentTarget, Color.yellow);
        }

        static void baitUpdate() {
            if (!Bait.active.Any()) return;

            // Bait report
            foreach (KeyValuePair<DeadPlayer, float> entry in new Dictionary<DeadPlayer, float>(Bait.active)) {
                Bait.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value <= 0) {
                    Bait.active.Remove(entry.Key);
                    if (entry.Key.killerIfExisting != null && entry.Key.killerIfExisting.PlayerId == PlayerControl.LocalPlayer.PlayerId) {
                        Helpers.handlePoisonerKillOnBodyReport();

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(entry.Key.killerIfExisting.PlayerId);
                        writer.Write(entry.Key.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId, entry.Key.player.PlayerId);
                    }
                }
            }
        }

        static void medicSetTarget() {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setTarget();
            if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.color);
        }

        static void draculaSetTarget() {
            if (Dracula.dracula == null || Dracula.dracula != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Dracula.canCreateVampireFromImpostor)
                if (Vampire.vampire != null) untargetablePlayers.Add(Vampire.vampire);
            Dracula.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            setPlayerOutline(Dracula.currentTarget, Dracula.color);
        }

        static void vampireSetTarget() {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Dracula.dracula != null) untargetablePlayers.Add(Dracula.dracula);
            Vampire.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            setPlayerOutline(Vampire.currentTarget, Vampire.color);
        }

        static void vampireCheckPromotion() {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;
            if (Vampire.vampire.Data.IsDead == true) return;
            if (Dracula.dracula == null || Dracula.dracula?.Data?.Disconnected == true) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }
        }

        static void poisonerSetTarget() {
            if (Poisoner.poisoner == null || Poisoner.poisoner != PlayerControl.LocalPlayer) return;
            Poisoner.currentTarget = setTarget(true, true, new List<PlayerControl>() { Vampire.wasImpostor ? Vampire.vampire : null, Dracula.wasImpostor ? Dracula.dracula : null});
            setPlayerOutline(Vampire.currentTarget, Helpers.impAbilityTargetColor);
        }

        static void scavengerUpdate() {
            if (Scavenger.scavenger == null || PlayerControl.LocalPlayer != Scavenger.scavenger || Scavenger.localArrows == null || !Scavenger.showArrows) return;
            if (Scavenger.scavenger.Data.IsDead) {
                foreach (Arrow arrow in Scavenger.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Scavenger.localArrows = new List<Arrow>();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Scavenger.localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate) {
                foreach (Arrow arrow in Scavenger.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Scavenger.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in deadBodies) {
                if (arrowUpdate) {
                    Scavenger.localArrows.Add(new Arrow(Color.blue));
                    Scavenger.localArrows[index].arrow.SetActive(true);
                }
                if (Scavenger.localArrows[index] != null) Scavenger.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        public static void executionerUpdate() {
            if (Executioner.executioner == null || Executioner.executioner != PlayerControl.LocalPlayer) return;

            // Promote to Pursuer
            if (Executioner.target != null && Executioner.target.Data.Disconnected && !Executioner.executioner.Data.IsDead) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotesToPursuer();
                return;
            }
        }

        public static void lawyerUpdate() {
            if (Lawyer.lawyer == null || Lawyer.lawyer != PlayerControl.LocalPlayer) return;

            // Promote to Pursuer
            if (Lawyer.target != null && Lawyer.target.Data.Disconnected && !Lawyer.lawyer.Data.IsDead) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
                return;
            }
        }

        static void pursuerSetTarget() {
            if (Pursuer.pursuer == null || Pursuer.pursuer != PlayerControl.LocalPlayer) return;
            Pursuer.target = setTarget();
            setPlayerOutline(Pursuer.target, Pursuer.color);
        }

        public static void guardianAngelUpdate() {
            if (GuardianAngel.guardianAngel == null || GuardianAngel.guardianAngel != PlayerControl.LocalPlayer) return;

            if (GuardianAngel.target != null && GuardianAngel.target.Data.Disconnected && !GuardianAngel.guardianAngel.Data.IsDead) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianAngelPromotes();
                return;
            }
        }

        static void fallenAngelSetTarget() {
            if (FallenAngel.fallenAngel == null || FallenAngel.fallenAngel != PlayerControl.LocalPlayer) return;
            FallenAngel.currentTarget = setTarget();
            setPlayerOutline(FallenAngel.currentTarget, FallenAngel.color);
        }

        static void amnesiacUpdate() {
            if (Amnesiac.amnesiac == null || Amnesiac.amnesiac != PlayerControl.LocalPlayer || Amnesiac.localArrows == null || !Amnesiac.showArrows) return;
            if (Amnesiac.amnesiac.Data.IsDead) {
                foreach (Arrow arrow in Amnesiac.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Amnesiac.localArrows = new List<Arrow>();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Amnesiac.localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate) {
                foreach (Arrow arrow in Amnesiac.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Amnesiac.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in deadBodies.Where(x => GameHistory.deadPlayers.Any(y => y.player.PlayerId == x.ParentId && y.timeOfDeath.AddSeconds(Amnesiac.delay) < System.DateTime.UtcNow))) {
                if (arrowUpdate) {
                    Amnesiac.localArrows.Add(new Arrow(Palette.EnabledColor));
                    Amnesiac.localArrows[index].arrow.SetActive(true);
                }
                if (Amnesiac.localArrows[index] != null) Amnesiac.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        static void investigatorUpdateFootPrints() {
            if (Investigator.investigator == null || Investigator.investigator != PlayerControl.LocalPlayer) return;

            Investigator.timer -= Time.fixedDeltaTime;
            if (Investigator.timer <= 0f) {
                Investigator.timer = Investigator.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent) 
                    {
                        FootprintHolder.Instance.MakeFootprint(player);
                    }
                }
            }
        }

        static void investigatorSetTarget() {
            if (Investigator.investigator == null || Investigator.investigator != PlayerControl.LocalPlayer) return;
            Investigator.currentTarget = setTarget();
            if (Investigator.watching == null) setPlayerOutline(Investigator.currentTarget, Investigator.color);
        }

        static void seerSetTarget() {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;
            Seer.currentTarget = setTarget(untargetablePlayers: Seer.revealedPlayers);
            setPlayerOutline(Seer.currentTarget, Seer.color);
        }

        static void juggernautSetTarget() {
            if (Juggernaut.juggernaut == null || Juggernaut.juggernaut != PlayerControl.LocalPlayer) return;
            Juggernaut.currentTarget = setTarget();
            setPlayerOutline(Juggernaut.currentTarget, Juggernaut.color);
        }

        static void swooperUpdate() {
            if (Swooper.isInvisble && Swooper.invisibleTimer <= 0 && Swooper.swooper == PlayerControl.LocalPlayer) {
                MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                invisibleWriter.Write(Swooper.swooper.PlayerId);
                invisibleWriter.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                RPCProcedure.swooperSwoop(Swooper.swooper.PlayerId, byte.MaxValue);
            }
        }

        static void mercenarySetTarget() {
            if (Mercenary.mercenary == null || Mercenary.mercenary != PlayerControl.LocalPlayer) return;
            Mercenary.currentTarget = setTarget();
            if (Mercenary.shielded == null) setPlayerOutline(Mercenary.currentTarget, Mercenary.color);
        }

        static void blackmailerSetTarget() {
            if (!Blackmailer.blackmailer || Blackmailer.blackmailer != PlayerControl.LocalPlayer) return;
            Blackmailer.currentTarget = setTarget(untargetablePlayers: new List<PlayerControl>() { Blackmailer.blackmailed } );
            setPlayerOutline(Blackmailer.currentTarget, Helpers.impAbilityTargetColor);
        }

        static void phantomUpdate() {
            if (Phantom.isInvisble && Phantom.ghostTimer <= 0 && Phantom.phantom == PlayerControl.LocalPlayer) {
                MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PhantomInvis, Hazel.SendOption.Reliable, -1);
                invisibleWriter.Write(Phantom.phantom.PlayerId);
                invisibleWriter.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                RPCProcedure.phantomInvis(Phantom.phantom.PlayerId, byte.MaxValue);
            }
        }

        static void doomsayerSetTarget() {
            if (!Doomsayer.doomsayer || Doomsayer.doomsayer != PlayerControl.LocalPlayer) return;
            Doomsayer.currentTarget = setTarget(untargetablePlayers: Doomsayer.observedPlayers);
            setPlayerOutline(Doomsayer.currentTarget, Doomsayer.color);
        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            
            if (PlayerControl.LocalPlayer == __instance) {
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
                // Morphling
                morphlingSetTarget();
                // Morphling and Camouflager
                morphlingAndCamouflagerUpdate();
                // Snitch
                snitchUpdate();
                // Sheriff
                sheriffSetTarget();
                // Shifter
                shifterSetTarget();
                // Medic
                medicSetTarget();
                // Dracula
                draculaSetTarget();
                // Vampire
                vampireSetTarget();
                vampireCheckPromotion();
                // Poisoner
                poisonerSetTarget();
                // Scavenger
                scavengerUpdate();
                // Executioner
                executionerUpdate();
                // Lawyer
                lawyerUpdate();
                // Pursuer
                pursuerSetTarget();
                // Guardian Angel
                guardianAngelUpdate();
                // Fallen Angel
                fallenAngelSetTarget();
                // Amnesiac
                amnesiacUpdate();
                // Investigator
                investigatorUpdateFootPrints();
                investigatorSetTarget();
                // Seer
                seerSetTarget();
                // Juggernaut
                juggernautSetTarget();
                // Swooper
                swooperUpdate();
                // Mercenary
                mercenarySetTarget();
                // Blackmailer
                blackmailerSetTarget();
                // Phantom
                phantomUpdate();
                // Doomsayer
                doomsayerSetTarget();

                // -- MODIFIER--
                // Bait
                baitUpdate();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static bool Prefix(PlayerControl __instance) {
            Helpers.handlePoisonerKillOnBodyReport();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo target)
        {
            // Medic Report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId && Medic.getInfoOnReport;
            if (isMedicReport) {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport) {
                        if (timeSinceDeath < Medic.reportNameDuration * 1000) {
                            msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        } else if (timeSinceDeath < Medic.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting) ? "lighter" : "darker";
                            msg =  $"Body Report: The killer appears to be a {typeOfColor} color! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        } else {
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
            bool isSleuthReport = Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && __instance.PlayerId == Sleuth.sleuth.PlayerId;
            if (isSleuthReport) {
                PlayerControl reported = Helpers.playerById(target.PlayerId);
                Sleuth.reported.Add(reported);
            }
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

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
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

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && target == Lovers.lover1) || (Lovers.lover2 != null && target == Lovers.lover2)) {
                PlayerControl otherLover = target == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) {
                    otherLover.MurderPlayer(otherLover);
                    GameHistory.overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide, otherLover);
                }
            }

            // Bait
            if (Bait.bait != null && target == Bait.bait) {
                float reportDelay = (float) rnd.Next((int)Bait.reportDelayMin, (int)Bait.reportDelayMax + 1);
                Bait.active.Add(deadPlayer, reportDelay);
            }

            // Vampire promotion trigger on murder
            if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && target == Dracula.dracula && Dracula.dracula == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Executioner.target && AmongUsClient.Instance.AmHost && Executioner.executioner != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotesToPursuer();
            }

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
            }

            // Kill Guardian Angel trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == GuardianAngel.target && AmongUsClient.Instance.AmHost && GuardianAngel.guardianAngel != null) {
                Helpers.MurderPlayer(GuardianAngel.guardianAngel, GuardianAngel.guardianAngel, false);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                writer.Write(GuardianAngel.guardianAngel.PlayerId);
                writer.Write((byte)DeadPlayer.CustomDeathReason.GuardianAngelSuicide);
                writer.Write(GuardianAngel.guardianAngel.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                GameHistory.overrideDeathReasonAndKiller(GuardianAngel.guardianAngel, DeadPlayer.CustomDeathReason.GuardianAngelSuicide, GuardianAngel.guardianAngel);  // TODO: only executed on host?!
            }
            
            // Cleaner Button Sync
            if (Cleaner.cleaner != null && PlayerControl.LocalPlayer == Cleaner.cleaner && __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer;

            if (target != null) {
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
        public static void Prefix(PlayerControl source, bool canMove) {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (Morphling.morphling != null && source.Data.PlayerId == Morphling.morphling.PlayerId) {
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

            // Lover suicide trigger on exile
            if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2)) {
                PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) {
                    otherLover.Exiled();
                    GameHistory.overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide, otherLover);
                }
            }

            // Vampire promotion trigger on exile
            if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && __instance == Dracula.dracula && Dracula.dracula == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }

            // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
            if (Lawyer.lawyer != null && __instance == Lawyer.target && !Lawyer.lawyer.Data.IsDead) {
                PlayerControl lawyer = Lawyer.lawyer;
                if (AmongUsClient.Instance.AmHost && ((Lawyer.target != Jester.jester) || Lawyer.targetWasGuessed)) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerPromotesToPursuer();
                }

                if (!Lawyer.targetWasGuessed) {
                    if (Lawyer.lawyer != null) Lawyer.lawyer.Exiled();
                    if (Pursuer.pursuer != null) Pursuer.pursuer.Exiled();

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                    writer.Write(lawyer.PlayerId);
                    writer.Write((byte)DeadPlayer.CustomDeathReason.LawyerSuicide);
                    writer.Write(lawyer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    GameHistory.overrideDeathReasonAndKiller(lawyer, DeadPlayer.CustomDeathReason.LawyerSuicide, lawyer);  // TODO: only executed on host?!
                }
            }

            // Guardian Angel promote trigger on exile (the host sends the call such that everyone recieves the update before a possible game End)
            if (GuardianAngel.guardianAngel != null && __instance == GuardianAngel.target && !GuardianAngel.guardianAngel.Data.IsDead) {
                PlayerControl guardianAngel = GuardianAngel.guardianAngel;
                if (AmongUsClient.Instance.AmHost) {
                    MessageWriter faWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(faWriter);
                    RPCProcedure.guardianAngelPromotes();
                }
            }

            if (__instance != null) {
                Transform playerInfoTransform = __instance.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }
    }
    
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] {typeof(PlayerControl), typeof(DisconnectReasons) })]
    public static class GameDataHandleDisconnectPatch {
        public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason) {            
            // TODO: Find checker if Game Started then return
            if (AdditionalTempData.playerRoles.Count == 0) return;
            
            if (!player.Data.IsDead)
            {
                deadPlayers.Add(new DeadPlayer(player, DateTime.Now, DeadPlayer.CustomDeathReason.Disconnect, null));
                player.Data.IsDead = true;
            }
            
            OnGameEndPatch.UpdatePlayerSummary(player);
            List<DeadPlayer> killedPlayers = GetKilledPlayers(player);
            
            // Update death reasons for killed players
            // before killer disconnected
            foreach (DeadPlayer dp in killedPlayers)
            {
                // If player disconnected
                // It's DeathReason will be already updated
                if(!dp.player) continue;
                
                AdditionalTempData.PlayerRoleInfo deadPlayerRole = AdditionalTempData.playerRoles.Find(x => x.PlayerName.Equals(dp.player.Data.PlayerName));
                deadPlayerRole.DeathReason = dp.player.getDeathReasonString();
            }
        }
    }
}
