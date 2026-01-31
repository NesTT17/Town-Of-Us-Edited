using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using Sentry.Internal.Extensions;
using Reactor.Utilities.Extensions;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        // Helpers
        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null, bool evenWhenDead = false)
        {
            PlayerControl result = null;
            float num = AmongUs.GameOptions.NormalGameOptionsV09.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (!evenWhenDead && targetingPlayer.Data.IsDead) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

            color = color.SetAlpha(Shy.visibility(target.PlayerId));

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        static DeadBody setDeadTarget()
        {
            PlayerControl LocalPlayer = PlayerControl.LocalPlayer;
            bool isDead = LocalPlayer.Data.IsDead;
            Vector2 truePosition = LocalPlayer.GetTruePosition();
            float maxDistance = AmongUs.GameOptions.NormalGameOptionsV09.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)] * 0.75f;
            Il2CppReferenceArray<Collider2D> colliders = Physics2D.OverlapCircleAll(truePosition, maxDistance, Constants.PlayersOnlyMask);
            DeadBody closestBody = null;
            float closestDistance = float.MaxValue;
            if (isDead)
                return closestBody;

            foreach (Collider2D collider in colliders)
            {
                if (collider.tag != "DeadBody")
                    continue;

                DeadBody component = collider.GetComponent<DeadBody>();
                float distance = Vector2.Distance(truePosition, component.TruePosition);

                if (
                    PhysicsHelpers.AnythingBetween(truePosition, component.TruePosition, Constants.ShipAndObjectsMask, false) ||
                    distance > maxDistance ||
                    distance >= closestDistance
                )
                    continue;

                closestBody = component;
                closestDistance = distance;
            }
            return closestBody;
        }

        static void setDeadBodyOutline(DeadBody dead, Color color)
        {
            if (dead == null || dead.bodyRenderers[0] == null) return;

            if (dead != null || dead.bodyRenderers[0] != null)
            {
                dead.bodyRenderers[0].material.SetFloat("_Outline", 1f);
                dead.bodyRenderers[0].material.SetColor("_OutlineColor", color);
            }
            else
            {
                dead.bodyRenderers[0].material.SetFloat("_Outline", 0f);
            }
        }

        // Update functions
        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

                bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
                bool isMorphedGlitch = target == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
                bool isMorphedVenerer = target == Venerer.venerer && Venerer.abilityTimer > 0f;
                bool hasVisibleShield = false;
                Color color = Medic.shieldedColor;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Medic.shieldVisible(target))
                    hasVisibleShield = true;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Medic.exShieldVisible(target))
                    hasVisibleShield = true;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && GuardianAngel.protectVisible(target))
                {
                    color = Color.yellow;
                    hasVisibleShield = true;
                }

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && firstKillPlayer != null && shieldFirstKill && ((target == firstKillPlayer && !isMorphedMorphling && !isMorphedGlitch && !isMorphedVenerer) || (isMorphedMorphling && Morphling.morphTarget == firstKillPlayer) || (isMorphedGlitch && Glitch.morphPlayer == firstKillPlayer)))
                {
                    hasVisibleShield = true;
                    color = Color.blue;
                }

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

        static void setPetVisibility()
        {
            bool localalive = !PlayerControl.LocalPlayer.Data.IsDead;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                bool playeralive = !player.Data.IsDead;
                player.cosmetics.SetPetVisible((localalive && playeralive) || !localalive);
            }
        }

        static void impostorSetTarget()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
            { // !isImpostor || !canMove || isDead
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            target = setTarget(true, killPlayersInVent, new List<PlayerControl>() { Dracula.wasImpostor ? Dracula.dracula : null, Vampire.wasImpostor ? Vampire.vampire : null });

            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
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
                }

                // Colorblind Text During the round
                if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active)
                {
                    p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
                }

                p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);  // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

                if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target) || (Executioner.executionerKnowsRole && PlayerControl.LocalPlayer == Executioner.executioner && p == Executioner.target) || (Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && Sleuth.reported.Contains(p)) || (Poucher.poucher != null && Poucher.poucher == PlayerControl.LocalPlayer && Poucher.killed.Contains(p)) || (Mayor.mayor != null && Mayor.mayor == p && Mayor.mayor != PlayerControl.LocalPlayer) || (GuardianAngel.gaKnowsRole && PlayerControl.LocalPlayer == GuardianAngel.guardianAngel && p == GuardianAngel.target) || p == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead)
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
                    string roleNames = RoleInfo.GetRolesString(p, true, false);
                    string roleText = RoleInfo.GetRolesString(p, true, ghostsSeeModifier, ghostsSeeInformation);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == PlayerControl.LocalPlayer)
                    {
                        if (p.Data.IsDead) roleNames = roleText;
                        playerInfoText = $"{roleNames}";
                        if (p == Swapper.swapper) playerInfoText = $"{roleNames}" + Helpers.cs(Swapper.color, $" ({Swapper.charges})");
                        if (Drunk.drunk.Any(x => x.PlayerId == p.PlayerId)) playerInfoText = $"{roleNames}" + Helpers.cs(Drunk.color, $" ({Drunk.meetings})");
                        if (HudManager.Instance.TaskPanel != null)
                        {
                            TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"Tasks {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (ghostsSeeRoles && ghostsSeeTasks)
                    {
                        playerInfoText = $"{roleText} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (ghostsSeeTasks)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (ghostsSeeRoles)
                    {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }
                    if (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target || Executioner.executionerKnowsRole && PlayerControl.LocalPlayer == Executioner.executioner && p == Executioner.target || Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && Sleuth.reported.Contains(p) || Poucher.poucher != null && Poucher.poucher == PlayerControl.LocalPlayer && Poucher.killed.Contains(p) || Mayor.mayor != null && Mayor.mayor == p && Mayor.mayor != PlayerControl.LocalPlayer || GuardianAngel.gaKnowsRole && PlayerControl.LocalPlayer == GuardianAngel.guardianAngel && p == GuardianAngel.target)
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

        static void sheriffSetTarget()
        {
            if (!PlayerControl.LocalPlayer.IsSheriff(out Sheriff sheriff)) return;
            sheriff.currentTarget = setTarget(false, killPlayersInVent);
            setPlayerOutline(sheriff.currentTarget, Sheriff.color);
        }

        static void juggernautSetTarget()
        {
            if (Juggernaut.juggernaut == null || Juggernaut.juggernaut != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Juggernaut.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers);
            setPlayerOutline(Juggernaut.currentTarget, Juggernaut.color);
        }

        static void seerSetTarget()
        {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            foreach (var playerId in Seer.revealedIds)
            {
                PlayerControl target = Helpers.playerById(playerId);
                untargetables.Add(target);
            }
            Seer.currentTarget = setTarget(false, abilityPlayersInVent, untargetables);
            setPlayerOutline(Seer.currentTarget, Seer.color);
        }

        static void engineerUpdate()
        {
            if (Engineer.engineer == null || PlayerControl.LocalPlayer != Engineer.engineer || !Engineer.canRecharge || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Engineer.rechargedTasks)
            {
                Engineer.rechargedTasks += Engineer.rechargeTasksNumber;
                Engineer.remainingFixes++;
            }
        }

        static void snitchUpdate()
        {
            if (Snitch.snitch == null) return;
            if (!Snitch.needsUpdate) return;

            bool snitchIsDead = Snitch.snitch.Data.IsDead;
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);

            if (playerTotal == 0) return;
            PlayerControl local = PlayerControl.LocalPlayer;

            int numberOfTasks = playerTotal - playerCompleted;

            if (Snitch.isRevealed && ((Snitch.targets == Snitch.Targets.EvilPlayers && Helpers.isEvil(local)) || (Snitch.targets == Snitch.Targets.Killers && Helpers.isKiller(local))))
            {
                if (Snitch.text == null)
                {
                    Snitch.text = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    Snitch.text.enableWordWrapping = false;
                    Snitch.text.transform.localScale = Vector3.one * 0.75f;
                    Snitch.text.transform.localPosition += new Vector3(0f, 1.8f, -69f);
                    Snitch.text.gameObject.SetActive(true);
                }
                else
                {
                    Snitch.text.text = $"Snitch is alive: " + playerCompleted + "/" + playerTotal;
                    if (snitchIsDead) Snitch.text.text = $"Snitch is dead!";
                }
            }
            else if (Snitch.text != null)
                Snitch.text.Destroy();

            if (snitchIsDead)
            {
                if (MeetingHud.Instance == null) Snitch.needsUpdate = false;
                return;
            }
            if (numberOfTasks <= Snitch.taskCountForReveal) Snitch.isRevealed = true;
        }

        static void snitchArrowsUpdate()
        {
            if (Snitch.localArrows == null) return;
            foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);
            if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
            int numberOfTasks = playerTotal - playerCompleted;
            if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0 && Snitch.mode == Snitch.Mode.Arrows)
            {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool arrowForEvil = Snitch.targets == Snitch.Targets.EvilPlayers && Helpers.isEvil(p);
                    bool arrowForKillers = Snitch.targets == Snitch.Targets.Killers && Helpers.isKiller(p);
                    if (!p.Data.IsDead && (arrowForEvil || arrowForKillers))
                    {
                        if (arrowIndex >= Snitch.localArrows.Count)
                        {
                            Snitch.localArrows.Add(new Arrow(Palette.ImpostorRed));
                        }
                        if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null)
                        {
                            Snitch.localArrows[arrowIndex].arrow.SetActive(true);
                            Snitch.localArrows[arrowIndex].Update(p.transform.position, Palette.ImpostorRed);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        static void veteranUpdate()
        {
            if (!PlayerControl.LocalPlayer.IsVeteran(out Veteran veteran) || !Veteran.canRecharge || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == veteran.rechargedTasks)
            {
                veteran.rechargedTasks += Veteran.rechargeTasksNumber;
                veteran.remainingAlerts++;
            }
        }

        static bool mushroomSaboWasActive = false;
        static void morphlingAndCamouflagerAndVenererAndGlitchUpdate()
        {
            bool mushRoomSaboIsActive = Helpers.MushroomSabotageActive();
            if (!mushroomSaboWasActive) mushroomSaboWasActive = mushRoomSaboIsActive;

            float oldCamouflageTimer = Camouflager.camouflageTimer;
            float oldMorphTimer = Morphling.morphTimer;
            float oldAbilityTimer = Venerer.abilityTimer;
            float oldMimicTimer = Glitch.morphTimer;
            Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
            Morphling.morphTimer = Mathf.Max(0f, Morphling.morphTimer - Time.fixedDeltaTime);
            Venerer.abilityTimer = Mathf.Max(0f, Venerer.abilityTimer - Time.fixedDeltaTime);
            Glitch.morphTimer = Mathf.Max(0f, Glitch.morphTimer - Time.fixedDeltaTime);

            if (mushRoomSaboIsActive) return;

            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f)
            {
                Camouflager.resetCamouflage();
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null)
                {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (Venerer.abilityTimer > 0f && Venerer.venerer != null)
                {
                    Venerer.venerer.setLook("", 15, "", "", "", "");
                    Venerer.venerer.cosmetics.colorBlindText.text = "";
                }
                if (Glitch.morphTimer > 0f && Glitch.glitch != null && Glitch.morphPlayer != null)
                {
                    PlayerControl target = Glitch.morphPlayer;
                    Glitch.glitch.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
            if (mushroomSaboWasActive)
            {
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null)
                {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (Venerer.abilityTimer > 0f && Venerer.venerer != null)
                {
                    Venerer.venerer.setLook("", 15, "", "", "", "");
                    Venerer.venerer.cosmetics.colorBlindText.text = "";
                }
                if (Glitch.morphTimer > 0f && Glitch.glitch != null && Glitch.morphPlayer != null)
                {
                    PlayerControl target = Glitch.morphPlayer;
                    Glitch.glitch.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (Camouflager.camouflageTimer > 0)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        player.setLook("", 15, "", "", "", "");
                        player.cosmetics.colorBlindText.text = "";
                    }
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f && oldMorphTimer > 0f && Morphling.morphTimer <= 0f && Morphling.morphling != null)
                Morphling.resetMorph();
            // Venerer reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f && oldAbilityTimer > 0f && Venerer.abilityTimer <= 0f && Venerer.venerer != null)
                Venerer.resetMorph();
            // Glitch reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f && oldMimicTimer > 0f && Glitch.morphTimer <= 0f && Glitch.glitch != null)
                Glitch.resetMorph();
            mushroomSaboWasActive = false;
        }

        static void pursuerSetTarget()
        {
            if (!PlayerControl.LocalPlayer.IsPursuer(out Pursuer pursuer)) return;
            pursuer.currentTarget = setTarget(false, abilityPlayersInVent);
            setPlayerOutline(pursuer.currentTarget, Pursuer.color);
        }

        static void amnesiacSetTarget()
        {
            if (!PlayerControl.LocalPlayer.IsAmnesiac(out Amnesiac amnesiac)) return;
            amnesiac.currentTarget = setDeadTarget();
            setDeadBodyOutline(amnesiac.currentTarget, Amnesiac.color);
        }

        static void amnesiacUpdate()
        {
            if (!PlayerControl.LocalPlayer.IsAmnesiac(out Amnesiac amnesiac) || amnesiac.localArrows == null || !Amnesiac.showArrows) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                foreach (Arrow arrow in amnesiac.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                amnesiac.localArrows = [];
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = amnesiac.localArrows.Count != deadBodies.Count();
            int index = 0;

            foreach (DeadBody db in deadBodies.Where(x => GameHistory.deadPlayers.Any(y => y.player.PlayerId == x.ParentId && y.timeOfDeath.AddSeconds(Amnesiac.delay) < System.DateTime.UtcNow)))
            {
                if (arrowUpdate)
                {
                    amnesiac.localArrows.Add(new Arrow(Palette.EnabledColor));
                    amnesiac.localArrows[index].arrow.SetActive(true);
                }
                if (amnesiac.localArrows[index] != null) amnesiac.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        static void thiefSetTarget()
        {
            if (!PlayerControl.LocalPlayer.IsThief(out Thief thief)) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            thief.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers);
            setPlayerOutline(thief.currentTarget, Thief.color);
        }

        static void lawyerUpdate()
        {
            if (Lawyer.lawyer == null || Lawyer.lawyer != PlayerControl.LocalPlayer) return;

            if (Lawyer.target != null && Lawyer.target.Data.Disconnected && !Lawyer.lawyer.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotes();
            }
        }

        static void executionerUpdate()
        {
            if (Executioner.executioner == null || Executioner.executioner != PlayerControl.LocalPlayer) return;

            if (Executioner.target != null && Executioner.target.Data.Disconnected && !Executioner.executioner.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotes();
            }
        }

        static void medicSetTarget()
        {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setTarget(false, abilityPlayersInVent);
            if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
        }

        static void swapperUpdate()
        {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Swapper.rechargedTasks)
            {
                Swapper.rechargedTasks += Swapper.rechargeTasksNumber;
                Swapper.charges++;
            }
        }

        static void investigatorFootprintUpdate()
        {
            if (Investigator.investigator == null || Investigator.investigator != PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead) return;

            Investigator.timer -= Time.fixedDeltaTime;
            if (Investigator.timer <= 0f)
            {
                Investigator.timer = Investigator.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent)
                    {
                        FootprintHolder.Instance.MakeFootprint(player);
                    }
                }
            }
        }

        static void spyUpdate()
        {
            if (Spy.spy == null || PlayerControl.LocalPlayer != Spy.spy || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Spy.rechargedTasks)
            {
                Spy.rechargedTasks += Spy.rechargeTasksNumber;
                if (Spy.toolsNumber > Spy.chargesVitals) Spy.chargesVitals++;
                if (Spy.toolsNumber > Spy.chargesAdminTable) Spy.chargesAdminTable++;
            }
        }

        static void trackerSetTarget()
        {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            Tracker.currentTarget = setTarget(false, abilityPlayersInVent, Tracker.trackedPlayers);
            setPlayerOutline(Tracker.currentTarget, Tracker.color);
        }

        static void trackerUpdate()
        {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;

            if (Tracker.tracker.Data.IsDead)
            {
                foreach (Arrow arrow in Tracker.TrackedPlayerLocalArrows.Values)
                    UnityEngine.Object.Destroy(arrow.arrow);
                Tracker.TrackedPlayerLocalArrows.Clear();
                return;
            }

            if (Tracker.TrackedPlayerLocalArrows != null)
            {
                Tracker.timeUntilUpdate -= Time.deltaTime;

                if (Tracker.timeUntilUpdate <= 0f && Tracker.trackedPlayers.Count > 0)
                {
                    foreach (PlayerControl player in Tracker.trackedPlayers)
                    {
                        if (Tracker.TrackedPlayerLocalArrows.TryGetValue(player.PlayerId, out Arrow arrow))
                        {
                            arrow.arrow.SetActive(true);
                            if (!player.Data.IsDead)
                            {
                                arrow.Update(player.transform.position);
                            }
                            else
                            {
                                foreach (DeadPlayer deadPlayer in deadPlayers)
                                {
                                    if (deadPlayer.player == player)
                                    {
                                        Tracker.TrackedPlayerLocalArrows[player.PlayerId].Update(deadPlayer.player.transform.position);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    Tracker.timeUntilUpdate = Tracker.updateIntervall;
                }
            }
        }

        static void detectiveSetTarget()
        {
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;
            Detective.currentTarget = setTarget(false, abilityPlayersInVent);
            if (Detective.currentDetectedKiller != null) setPlayerOutline(Detective.currentTarget, Detective.color);
        }

        static void detectiveSetBodyTarget()
        {
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;
            Detective.currentDeadBodyTarget = setDeadTarget();
            setDeadBodyOutline(Detective.currentDeadBodyTarget, Detective.color);
        }

        static void mysticUpdate()
        {
            if (Mystic.mystic == null || Mystic.mystic != PlayerControl.LocalPlayer || Mystic.localArrows == null) return;
            if (Mystic.mystic.Data.IsDead)
            {
                foreach (Arrow arrow in Mystic.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Mystic.localArrows = new List<Arrow>();
                return;
            }

            var validBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().Where(x => deadPlayers.Any(y => y.player.PlayerId == x.ParentId && y.timeOfDeath.AddSeconds(Mystic.duration) > System.DateTime.UtcNow));
            bool arrowUpdate = Mystic.localArrows.Count != validBodies.Count();
            int index = 0;

            if (arrowUpdate)
            {
                foreach (Arrow arrow in Mystic.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Mystic.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in validBodies)
            {
                if (arrowUpdate)
                {
                    Mystic.localArrows.Add(new Arrow(Palette.EnabledColor));
                    Mystic.localArrows[index].arrow.SetActive(true);
                }
                if (Mystic.localArrows[index] != null) Mystic.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        static void gaUpdate()
        {
            if (GuardianAngel.guardianAngel == null || GuardianAngel.guardianAngel != PlayerControl.LocalPlayer) return;

            if (GuardianAngel.target != null && GuardianAngel.target.Data.Disconnected && !GuardianAngel.guardianAngel.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianAngelPromotes();
            }
        }

        static void swooperUpdate()
        {
            if (Swooper.isInvisible && Swooper.invisibleTimer <= 0f && Swooper.swooper == PlayerControl.LocalPlayer)
            {
                MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                invisibleWriter.Write(Swooper.swooper.PlayerId);
                invisibleWriter.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                RPCProcedure.swooperSwoop(Swooper.swooper.PlayerId, byte.MaxValue);
            }
        }

        static void arsonistSetTarget()
        {
            if (Arsonist.arsonist == null || Arsonist.arsonist != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => !Arsonist.dousedPlayers.Contains(x)).ToList();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetables.Add(Mini.mini);
            Arsonist.currentDouseTarget = setTarget(false, abilityPlayersInVent, Arsonist.dousedPlayers);
            Arsonist.currentIgniteTarget = setTarget(false, killPlayersInVent, untargetables);
            setPlayerOutline(Arsonist.currentDouseTarget, Arsonist.color);
            setPlayerOutline(Arsonist.currentIgniteTarget, Arsonist.color);
        }

        static void werewolfSetTarget()
        {
            if (Werewolf.werewolf == null || Werewolf.werewolf != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Werewolf.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers);
            if (Werewolf.isRampageActive) setPlayerOutline(Werewolf.currentTarget, Werewolf.color);
        }

        static void janitorSetTarget()
        {
            if (Janitor.janitor == null || Janitor.janitor != PlayerControl.LocalPlayer) return;
            Janitor.currentTarget = setDeadTarget();
            setDeadBodyOutline(Janitor.currentTarget, Palette.ImpostorRed);
        }

        static void undertakerDragBodyUpdate()
        {
            if (Undertaker.undertaker == null || Undertaker.undertaker.Data.IsDead) return;
            if (Undertaker.dragedBody != null)
            {
                Vector3 currentPosition = Undertaker.undertaker.transform.position;
                Undertaker.dragedBody.transform.position = currentPosition;
            }
        }

        static void blackmailerSetTarget()
        {
            if (Blackmailer.blackmailer == null || Blackmailer.blackmailer != PlayerControl.LocalPlayer) return;
            Blackmailer.currentTarget = setTarget(false, abilityPlayersInVent);
            setPlayerOutline(Blackmailer.currentTarget, Blackmailer.color);
        }

        static void politicianSetTarget()
        {
            if (Politician.politician == null || Politician.politician != PlayerControl.LocalPlayer) return;
            Politician.currentTarget = setTarget(false, abilityPlayersInVent, Politician.campaignedPlayers);
            setPlayerOutline(Politician.currentTarget, Politician.color);
        }

        static void draculaSetTarget()
        {
            if (Dracula.dracula == null || Dracula.dracula != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Dracula.canCreateVampireFromNeutralBenign && Dracula.canCreateVampireFromNeutralEvil && Dracula.canCreateVampireFromNeutralKilling && Dracula.canCreateVampireFromImpostor)
            {
                if (Vampire.vampire != null) untargetablePlayers.Add(Vampire.vampire);
            }
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Dracula.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers: untargetablePlayers);
            setPlayerOutline(Dracula.currentTarget, Dracula.color);
        }

        static void vampireSetTarget()
        {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Dracula.dracula != null) untargetablePlayers.Add(Dracula.dracula);
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Vampire.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            if (Vampire.canKill) setPlayerOutline(Vampire.currentTarget, Vampire.color);
        }

        static void vampireCheckPromotion()
        {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;
            if (Vampire.vampire.Data.IsDead == true) return;
            if (Dracula.dracula == null || Dracula.dracula?.Data?.Disconnected == true || Dracula.dracula?.Data?.IsDead == true)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }
        }

        static void poisonerSetTarget()
        {
            if (Poisoner.poisoner == null || Poisoner.poisoner != PlayerControl.LocalPlayer) return;
            PlayerControl target = null;
            target = setTarget(true, killPlayersInVent, new List<PlayerControl>() { Vampire.wasImpostor ? Vampire.vampire : null, Dracula.wasImpostor ? Dracula.dracula : null, Mini.mini == null ? null : Mini.mini });
            Poisoner.currentTarget = target;
            setPlayerOutline(Poisoner.currentTarget, Poisoner.color);
        }

        static void plaguebearerSetTarget()
        {
            if (Plaguebearer.plaguebearer == null || Plaguebearer.plaguebearer != PlayerControl.LocalPlayer) return;
            Plaguebearer.currentTarget = setTarget(false, abilityPlayersInVent, Plaguebearer.infectedPlayers);
            setPlayerOutline(Plaguebearer.currentTarget, Plaguebearer.color);
        }

        static void pestilenceSetTarget()
        {
            if (Pestilence.pestilence == null || Pestilence.pestilence != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Pestilence.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers);
            setPlayerOutline(Pestilence.currentTarget, Pestilence.color);
        }

        static void doomsayerSetTarget()
        {
            if (Doomsayer.doomsayer == null || Doomsayer.doomsayer != PlayerControl.LocalPlayer) return;
            Doomsayer.currentTarget = setTarget(false, abilityPlayersInVent);
            if (Doomsayer.canObserve) setPlayerOutline(Doomsayer.currentTarget, Doomsayer.color);
        }

        static void glitchSetTarget()
        {
            if (Glitch.glitch == null || Glitch.glitch != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
            Glitch.currentTarget = setTarget(false, killPlayersInVent, untargetablePlayers);
            setPlayerOutline(Glitch.currentTarget, Glitch.color);
        }

        static void baitUpdate()
        {
            if (!Bait.active.Any()) return;

            // Bait report
            foreach (KeyValuePair<DeadPlayer, float> entry in new Dictionary<DeadPlayer, float>(Bait.active))
            {
                Bait.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value <= 0)
                {
                    Bait.active.Remove(entry.Key);
                    if (entry.Key.killerIfExisting != null && entry.Key.killerIfExisting.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        Helpers.handlePoisonerKillOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(entry.Key.killerIfExisting.PlayerId);
                        writer.Write(entry.Key.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId, entry.Key.player.PlayerId);
                    }
                }
            }
        }

        static void playerSizeUpdate(PlayerControl p)
        {
            // Set default player size
            CircleCollider2D collider = p.Collider.CastFast<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.defaultColliderRadius;
            collider.offset = Mini.defaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Mini.mini == Morphling.morphling && Morphling.morphTimer > 0 || Mini.mini == Glitch.glitch && Glitch.morphTimer > 0 || Mini.mini == Venerer.venerer && Venerer.abilityTimer > 0f) return;

            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float correctedColliderRadius = Mini.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.mini)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Glitch.glitch != null && p == Glitch.glitch && Glitch.morphPlayer == Mini.mini && Glitch.morphTimer > 0f)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        public static void miniCooldownUpdate()
        {
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini)
            {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                sheriffShootButton.MaxTimer = Sheriff.cooldown * multiplier;
                poisonButton.MaxTimer = Vampire.cooldown * multiplier;
                draculaButton.MaxTimer = Dracula.cooldown * multiplier;
                vampireButton.MaxTimer = Vampire.cooldown * multiplier;
                janitorCleanButton.MaxTimer = Janitor.cooldown * multiplier;
                thiefKillButton.MaxTimer = Thief.cooldown * multiplier;
            }
        }

        static void multitaskerUpdate()
        {
            if (Multitasker.multitasker.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId))
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return;
                if (!Minigame.Instance) return;

                var Base = Minigame.Instance as MonoBehaviour;
                SpriteRenderer[] rends = Base.GetComponentsInChildren<SpriteRenderer>();
                for (var i = 0; i < rends.Length; i++)
                {
                    var oldColor1 = rends[i].color[0];
                    var oldColor2 = rends[i].color[1];
                    var oldColor3 = rends[i].color[2];
                    rends[i].color = new Color(oldColor1, oldColor2, oldColor3, 0.5f);
                }
            }
        }

        static void radarUpdate()
        {
            if (Radar.radar == null || PlayerControl.LocalPlayer != Radar.radar || MeetingHud.Instance)
                return;

            if (Radar.radar?.Data.IsDead == true)
            {
                if (Radar.localArrow?.arrow != null)
                    UnityEngine.Object.Destroy(Radar.localArrow?.arrow);
                Radar.localArrow = null;
                return;
            }

            PlayerControl closestPlayer = null;
            float closestDistance = float.MaxValue;
            Vector2 refPosition = PlayerControl.LocalPlayer.GetTruePosition();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player.PlayerId == PlayerControl.LocalPlayer.PlayerId || !player.Collider.enabled)
                    continue;

                float distance = Vector2.Distance(refPosition, player.GetTruePosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                if (Radar.localArrow == null)
                {
                    Radar.localArrow = new Arrow(Radar.color);
                    Radar.localArrow.arrow.SetActive(true);
                }
                Radar.localArrow.Update(closestPlayer.transform.position);
            }
            else
            {
                if (Radar.localArrow != null && Radar.localArrow.arrow != null)
                {
                    Radar.localArrow.arrow.SetActive(false);
                }
            }
        }

        static void saboteurUpdate()
        {
            if (Saboteur.saboteur == null || PlayerControl.LocalPlayer != Saboteur.saboteur) return;
            var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
            if (system.AnyActive) system.Timer = 30f;
            else if (system.Timer > 30f - Saboteur.reduceCooldown) system.Timer = 30f - Saboteur.reduceCooldown;
        }

        static void sateliteUpdate()
        {
            if (Satelite.satelite != null && Satelite.satelite == PlayerControl.LocalPlayer && Satelite.corpsesTrackingTimer >= 0f && !Satelite.satelite.Data.IsDead)
            {
                bool arrowsCountChanged = Satelite.localArrows.Count != Satelite.deadBodyPositions.Count();
                int index = 0;

                if (arrowsCountChanged)
                {
                    foreach (Arrow arrow in Satelite.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                    Satelite.localArrows = new List<Arrow>();
                }
                foreach (Vector3 position in Satelite.deadBodyPositions)
                {
                    if (arrowsCountChanged)
                    {
                        Satelite.localArrows.Add(new Arrow(Satelite.color));
                        Satelite.localArrows[index].arrow.SetActive(true);
                    }
                    if (Satelite.localArrows[index] != null) Satelite.localArrows[index].Update(position);
                    index++;
                }
            }
            else if (Satelite.localArrows.Count > 0)
            {
                foreach (Arrow arrow in Satelite.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Satelite.localArrows = new List<Arrow>();
            }
        }

        static void cannibalSetTarget()
        {
            if (Cannibal.cannibal == null || Cannibal.cannibal != PlayerControl.LocalPlayer) return;
            Cannibal.currentTarget = setDeadTarget();
            setDeadBodyOutline(Cannibal.currentTarget, Cannibal.color);
        }

        static void cannibalUpdate()
        {
            if (Cannibal.cannibal == null || PlayerControl.LocalPlayer != Cannibal.cannibal || Cannibal.localArrows == null || !Cannibal.showArrows) return;
            if (Cannibal.cannibal.Data.IsDead)
            {
                foreach (Arrow arrow in Cannibal.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Cannibal.localArrows = new List<Arrow>();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Cannibal.localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate)
            {
                foreach (Arrow arrow in Cannibal.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Cannibal.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in deadBodies)
            {
                if (arrowUpdate)
                {
                    Cannibal.localArrows.Add(new Arrow(Color.blue));
                    Cannibal.localArrows[index].arrow.SetActive(true);
                }
                if (Cannibal.localArrows[index] != null) Cannibal.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        static void scavengerUpdate()
        {
            if (Scavenger.scavenger == null || PlayerControl.LocalPlayer != Scavenger.scavenger) return;

            if (Scavenger.scavenger.Data.IsDead)
            {
                if (Scavenger.arrow != null || Scavenger.arrow.arrow != null) UnityEngine.Object.Destroy(Scavenger.arrow.arrow);
                Scavenger.arrow = null;
                if (Scavenger.cooldownText != null && Scavenger.cooldownText.gameObject != null) UnityEngine.Object.Destroy(Scavenger.cooldownText.gameObject);
                Scavenger.cooldownText = null;
                Scavenger.bounty = null;
                foreach (PoolablePlayer p in playerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
                return;
            }

            Scavenger.arrowUpdateTimer -= Time.fixedDeltaTime;
            Scavenger.bountyUpdateTimer -= Time.fixedDeltaTime;

            if (Scavenger.bounty == null || Scavenger.bountyUpdateTimer <= 0f)
            {
                // Set new bounty
                Scavenger.bounty = null;
                Scavenger.arrowUpdateTimer = 0f; // Force arrow to update
                Scavenger.bountyUpdateTimer = Scavenger.bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && (p != Vampire.vampire || !Vampire.wasTeamRed) && (p != Dracula.dracula || !Dracula.wasTeamRed) && (p != Mini.mini || Mini.isGrownUp()) && (Lovers.getPartner(Scavenger.scavenger) == null || p != Lovers.getPartner(Scavenger.scavenger))) possibleTargets.Add(p);
                }
                Scavenger.bounty = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                if (Scavenger.bounty == null) return;

                // Ghost Info
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)RPCProcedure.GhostInfoTypes.BountyTarget);
                writer.Write(Scavenger.bounty.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    foreach (PoolablePlayer pp in playerIcons.Values) pp.gameObject.SetActive(false);
                    if (playerIcons.ContainsKey(Scavenger.bounty.PlayerId) && playerIcons[Scavenger.bounty.PlayerId].gameObject != null)
                        playerIcons[Scavenger.bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Hide in meeting
            if (MeetingHud.Instance && playerIcons.ContainsKey(Scavenger.bounty.PlayerId) && playerIcons[Scavenger.bounty.PlayerId].gameObject != null)
                playerIcons[Scavenger.bounty.PlayerId].gameObject.SetActive(false);

            // Update Cooldown Text
            if (Scavenger.cooldownText != null)
            {
                Scavenger.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(Scavenger.bountyUpdateTimer, 0, Scavenger.bountyDuration)).ToString();
                Scavenger.cooldownText.gameObject.SetActive(!MeetingHud.Instance);  // Show if not in meeting
            }

            // Update Arrow
            if (Scavenger.showArrow && Scavenger.bounty != null)
            {
                if (Scavenger.arrow == null) Scavenger.arrow = new Arrow(Color.red);
                if (Scavenger.arrowUpdateTimer <= 0f)
                {
                    Scavenger.arrow.Update(Scavenger.bounty.transform.position);
                    Scavenger.arrowUpdateTimer = Scavenger.arrowUpdateIntervall;
                }
                Scavenger.arrow.Update();
            }
        }

        static void vampireHunterSetTarget()
        {
            if (VampireHunter.vampireHunter == null || VampireHunter.vampireHunter != PlayerControl.LocalPlayer) return;
            VampireHunter.currentTarget = setTarget(false, killPlayersInVent);
            if (VampireHunter.canStake) setPlayerOutline(VampireHunter.currentTarget, VampireHunter.color);
        }

        static void vampireHunterUpdate()
        {
            if (VampireHunter.vampireHunter == null || VampireHunter.vampireHunter != PlayerControl.LocalPlayer || VampireHunter.vampireHunter.Data.IsDead || !VampireHunter.selfKillOnFailedStakes) return;
            if (VampireHunter.failedStakes == VampireHunter.maxFailedStakes)
            {
                Helpers.MurderPlayer(VampireHunter.vampireHunter, VampireHunter.vampireHunter, false);
            }
        }

        public static void vampireHunterCheckPromotion(bool isMeeting = false)
        {
            if (VampireHunter.vampireHunter == null || VampireHunter.vampireHunter != PlayerControl.LocalPlayer) return;
            if (VampireHunter.vampireHunter.Data.IsDead == true || !isMeeting) return;

            bool draculaNotStopPromotion = (Dracula.dracula == null || Dracula.dracula?.Data?.Disconnected == true || Dracula.dracula.Data.IsDead);
            bool vampireNotStopPromotion = (Vampire.vampire == null || Vampire.vampire?.Data?.Disconnected == true || Vampire.vampire.Data.IsDead);
            if (draculaNotStopPromotion && vampireNotStopPromotion)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireHunterPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampireHunterPromotes();
            }
        }

        static void oracleSetTarget()
        {
            if (Oracle.oracle == null || Oracle.oracle != PlayerControl.LocalPlayer) return;
            Oracle.currentTarget = setTarget(false, abilityPlayersInVent);
            setPlayerOutline(Oracle.currentTarget, Oracle.color);
        }

        static void lookoutUpdate()
        {
            if (Lookout.lookout == null || PlayerControl.LocalPlayer != Lookout.lookout || !Lookout.canRecharge || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Lookout.rechargedTasks)
            {
                Lookout.rechargedTasks += Lookout.rechargeTasksNumber;
                Lookout.remainingZooms++;
            }
        }

        static void plumberSetTarget()
        {
            if (Plumber.plumber == null || Plumber.plumber != PlayerControl.LocalPlayer || MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < MapUtilities.CachedShipStatus.AllVents.Length; i++)
            {
                Vent vent = MapUtilities.CachedShipStatus.AllVents[i];
                if (vent.gameObject.name.StartsWith("MinerVent_") || vent.gameObject.name.StartsWith("SealedVent_") || vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = vent;
                }
            }
            Plumber.ventTarget = target;
        }

        static void plumberUpdate()
        {
            if (Plumber.plumber == null || PlayerControl.LocalPlayer != Plumber.plumber || !Plumber.canRecharge || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Plumber.rechargedTasks)
            {
                Plumber.rechargedTasks += Plumber.rechargeTasksNumber;
                Plumber.remainingFlushs++;
            }
        }

        static void disperserUpdate()
        {
            if (Disperser.disperser == null || PlayerControl.LocalPlayer != Disperser.disperser || !Disperser.canRecharge || PlayerControl.LocalPlayer.Data.IsDead) return;
            if (Disperser.numberOfKills == Disperser.rechargedKills)
            {
                Disperser.rechargedKills += Disperser.rechargeKillsNumber;
                Disperser.remainingDisperses++;
            }
        }

        public static void bansheeSetTarget()
        {
            if (Banshee.banshee == null || Banshee.banshee != PlayerControl.LocalPlayer) return;
            Banshee.currentTarget = setTarget(false, abilityPlayersInVent, evenWhenDead: true);
            setPlayerOutline(Banshee.currentTarget, Banshee.color);
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            // Mini and Morphling shrink
            playerSizeUpdate(__instance);

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
                // Sheriff
                sheriffSetTarget();
                // Juggernaut
                juggernautSetTarget();
                // Seer
                seerSetTarget();
                // Engineer
                engineerUpdate();
                // Veteran
                veteranUpdate();
                // Snitch
                snitchUpdate();
                snitchArrowsUpdate();
                // Morphling, Camouflager, Venerer and Glitch
                morphlingAndCamouflagerAndVenererAndGlitchUpdate();
                // Pursuer
                pursuerSetTarget();
                // Amnesiac
                amnesiacSetTarget();
                amnesiacUpdate();
                // Thief
                thiefSetTarget();
                // Lawyer
                lawyerUpdate();
                // Executioner
                executionerUpdate();
                // Medic
                medicSetTarget();
                // Swapper
                swapperUpdate();
                // Investigator
                investigatorFootprintUpdate();
                // Spy
                spyUpdate();
                spyUpdate();
                // Tracker
                trackerSetTarget();
                trackerUpdate();
                // Detective
                detectiveSetTarget();
                detectiveSetBodyTarget();
                // Mystic
                mysticUpdate();
                // Guardian Angel
                gaUpdate();
                // Swooper
                swooperUpdate();
                // Arsonist
                arsonistSetTarget();
                // Werewolf
                werewolfSetTarget();
                // Janitor
                janitorSetTarget();
                // Undertaker
                undertakerDragBodyUpdate();
                // Blackmailer
                blackmailerSetTarget();
                // Politician
                politicianSetTarget();
                // Dracula
                draculaSetTarget();
                // Sidekick
                vampireSetTarget();
                vampireCheckPromotion();
                // Poisoner
                poisonerSetTarget();
                BlindTrap.Update();
                // Plaguebearer
                plaguebearerSetTarget();
                // Pestilence
                pestilenceSetTarget();
                // Doomsayer
                doomsayerSetTarget();
                // Glitch
                glitchSetTarget();
                // Cannibal
                cannibalSetTarget();
                cannibalUpdate();
                // Scavenger
                scavengerUpdate();
                // Vampire Hunte
                vampireHunterSetTarget();
                vampireHunterUpdate();
                vampireHunterCheckPromotion();
                // Oracle
                oracleSetTarget();
                // Lookout
                lookoutUpdate();
                // Plumber
                plumberSetTarget();
                plumberUpdate();

                // -- MODIFIER--
                // Bait
                baitUpdate();
                // Shy
                Shy.update();
                // mini (for the cooldowns)
                miniCooldownUpdate();
                // Multitasker
                multitaskerUpdate();
                // Radar
                radarUpdate();
                // Saboteur
                saboteurUpdate();
                // Satelite
                sateliteUpdate();
                // Disperser
                disperserUpdate();

                // -- Ghost Roles --
                bansheeSetTarget();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch
    {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance)
        {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && (__instance.myPlayer == Mini.mini || (Morphling.morphling != null && __instance.myPlayer == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f) || (Glitch.glitch != null && __instance.myPlayer == Glitch.glitch && Glitch.morphPlayer == Mini.mini && Glitch.morphTimer > 0f));
            correctOffset = correctOffset && !(Mini.mini == Morphling.morphling && Morphling.morphTimer > 0f) && !(Mini.mini == Glitch.glitch && Glitch.morphTimer > 0f) && !(Mini.mini == Venerer.venerer && Venerer.abilityTimer > 0f);
            if (correctOffset)
            {
                float currentScaling = (Mini.growingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            Helpers.handlePoisonerKillOnBodyReport();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo target)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId && Medic.reportInfo;
            bool isDetectiveReport = Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && __instance.PlayerId == Detective.detective.PlayerId && Detective.reportInfo;
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
                            msg = $"Body Report: The killer role appears to be {RoleInfo.getRoleInfoForPlayer(deadPlayer.killerIfExisting).FirstOrDefault().name}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                        }
                        else if (timeSinceDeath < Detective.reportFactionDuration * 1000)
                        {
                            var faction = deadPlayer.killerIfExisting.Data.Role.IsImpostor ? "Impostor" : (deadPlayer.killerIfExisting.isAnyNeutral() ? "Neutral" : "Crewmate");
                            msg = $"Body Report: The killer faction appears to be a {faction}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
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
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);

                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.DetectiveOrMedicInfo);
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

            bool isSleuthReport = Sleuth.sleuth != null && Sleuth.sleuth == PlayerControl.LocalPlayer && __instance.PlayerId == Sleuth.sleuth.PlayerId;
            if (isSleuthReport)
            {
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

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
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

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && target == Lovers.lover1) || (Lovers.lover2 != null && target == Lovers.lover2))
            {
                PlayerControl otherLover = target == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                {
                    otherLover.MurderPlayer(otherLover);
                    overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide);
                }
            }

            // Vampire promotion trigger on murder
            if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && target == Dracula.dracula && AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }

            // Snitch
            if (Snitch.snitch != null && PlayerControl.LocalPlayer.PlayerId == Snitch.snitch.PlayerId && MapBehaviourPatch.herePoints.Keys.Any(x => x == target.PlayerId))
            {
                foreach (var a in MapBehaviourPatch.herePoints.Where(x => x.Key == target.PlayerId))
                {
                    UnityEngine.Object.Destroy(a.Value);
                    MapBehaviourPatch.herePoints.Remove(a.Key);
                }
            }

            // Lawyer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null && !Lawyer.lawyer.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotes();
            }

            // Executioner promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Executioner.target && AmongUsClient.Instance.AmHost && Executioner.executioner != null && !Executioner.executioner.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotes();
            }

            // GuardianAngel promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == GuardianAngel.target && AmongUsClient.Instance.AmHost && GuardianAngel.guardianAngel != null && !GuardianAngel.guardianAngel.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianAngelPromotes();
            }

            // Mystic show flash
            if (Mystic.mystic != null && !Mystic.mystic.Data.IsDead && Mystic.mystic != target && (PlayerControl.LocalPlayer == Mystic.mystic && !PlayerControl.LocalPlayer.isFlashedByGrenadier() || Helpers.shouldShowGhostInfo()))
                Helpers.showFlash(Mystic.color, 1f, "Mystic Info: Someone died");

            // Janitor Button Sync
            if (Janitor.janitor != null && PlayerControl.LocalPlayer == Janitor.janitor && __instance == Janitor.janitor && janitorCleanButton != null)
                janitorCleanButton.Timer = Janitor.janitor.killTimer;

            // Count Venerer kills
            if (Venerer.venerer != null && PlayerControl.LocalPlayer == Venerer.venerer && __instance == Venerer.venerer)
                Venerer.numberOfKills++;

            // Set Scavenger cooldown
            if (Scavenger.scavenger != null && PlayerControl.LocalPlayer == Scavenger.scavenger && __instance == Scavenger.scavenger)
            {
                if (target == Scavenger.bounty)
                {
                    Scavenger.scavenger.SetKillTimer(Scavenger.bountyKillCooldown);
                    Scavenger.bountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    Scavenger.scavenger.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + Scavenger.punishmentTime);
            }

            // Oracle confess player
            if (Oracle.oracle != null && AmongUsClient.Instance.AmHost && Oracle.oracle == target)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)CustomRPC.OracleConfess, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.oracleConfess();
            }

            // Lookout Reset Zoom
            if (Lookout.lookout != null && Lookout.lookout == target && Lookout.zoomOutStatus)
            {
                Lookout.toggleZoom(true);
            }

            // Bait self report
            if (Bait.bait.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
            {
                float reportDelay = (float)rnd.Next((int)Bait.reportDelayMin, (int)Bait.reportDelayMax + 1);
                Bait.active.Add(deadPlayer, reportDelay);

                if (Bait.showKillFlash && __instance == PlayerControl.LocalPlayer) Helpers.showFlash(Bait.color);
            }

            // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
            if (Mini.mini != null && __instance == Mini.mini && __instance == PlayerControl.LocalPlayer)
            {
                float multiplier = 1f;
                if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(__instance.killTimer * multiplier);
            }

            // VIP Modifier
            if (Vip.vip.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
            {
                Color color = Color.yellow;
                if (Vip.showColor)
                {
                    color = Palette.CrewmateBlue;
                    if (__instance.Data.Role.IsImpostor) color = Color.red;
                    else if (__instance.isAnyNeutral()) color = Color.gray;
                }
                Helpers.showFlash(color, 1.5f);
            }

            // Handle Frosty Chill
            if (Frosty.frosty != null && Frosty.frosty == target)
            {
                Frosty.chilled = __instance;
                Frosty.lastChilled = DateTime.UtcNow;
                Frosty.isChilled = true;
            }

            // Satelite store body positions
            if (Satelite.deadBodyPositions != null) Satelite.deadBodyPositions.Add(target.transform.position);

            // Disperser add kills
            if (Disperser.disperser != null && Disperser.disperser == __instance)
                Disperser.numberOfKills++;

            // Poucher add info
            if (Poucher.poucher != null && Poucher.poucher == __instance && target != null)
                Poucher.killed.Add(target);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            if (Underdog.underdog != null && PlayerControl.LocalPlayer == Underdog.underdog)
            {
                int impsAlive = PlayerControl.AllPlayerControls.ToArray().Count(x => x.Data.Role.IsImpostor && x != null && !x.Data.IsDead && !x.Data.Disconnected);
                if (impsAlive >= 2 && Underdog.increaseCooldown) addition = Underdog.addition;
                else addition = -(Underdog.addition);
            }
            if (Scavenger.scavenger != null && PlayerControl.LocalPlayer == Scavenger.scavenger) addition = Scavenger.punishmentTime;

            float max = Mathf.Max(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;
            player.killTimer = time;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
    }

    [HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), nameof(KillAnimation._CoPerformKill_d__2.MoveNext))]
    class KillAnimationCoPerformKillPatch
    {
        public static bool hideNextAnimation = false;
        public static void Prefix(KillAnimation._CoPerformKill_d__2 __instance)
        {
            if (hideNextAnimation)
                __instance.source = __instance.target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch
    {
        private static int? colorId = null;
        public static void Prefix(PlayerControl source, bool canMove)
        {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (Morphling.morphling != null && source.Data.PlayerId == Morphling.morphling.PlayerId)
            {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
            if (Venerer.venerer != null && source.Data.PlayerId == Venerer.venerer.PlayerId)
            {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove)
        {
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
            if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2))
            {
                PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                {
                    otherLover.Exiled();
                    overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoverSuicide);
                }
            }

            // Vampire promotion trigger on exile
            if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && __instance == Dracula.dracula && AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes();
            }

            if (Lawyer.lawyer != null && __instance == Lawyer.target)
            {
                PlayerControl lawyer = Lawyer.lawyer;
                if (AmongUsClient.Instance.AmHost && (!Lawyer.target.IsJester(out _) || Lawyer.targetWasGuessed) && !Lawyer.lawyer.Data.IsDead)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerPromotes();
                }
                if (!Lawyer.targetWasGuessed && !Lawyer.lawyer.Data.IsDead)
                {
                    if (Lawyer.lawyer != null) Lawyer.lawyer.Exiled();

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                    writer.Write(lawyer.PlayerId);
                    writer.Write((byte)DeadPlayer.CustomDeathReason.LawyerSuicide);
                    writer.Write(lawyer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    overrideDeathReasonAndKiller(lawyer, DeadPlayer.CustomDeathReason.LawyerSuicide, lawyer);  // TODO: only executed on host?!
                }
            }

            if (Executioner.executioner != null && __instance == Executioner.target)
            {
                if (AmongUsClient.Instance.AmHost && Executioner.targetWasGuessed && !Executioner.executioner.Data.IsDead)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerPromotes();
                }
            }

            // GuardianAngel promotion trigger on exile (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == GuardianAngel.target && AmongUsClient.Instance.AmHost && GuardianAngel.guardianAngel != null && !GuardianAngel.guardianAngel.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianAngelPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdate
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            bool shouldInvert = Drunk.drunk.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0 && Drunk.meetings > 0;
            if (__instance.AmOwner &&
                AmongUsClient.Instance &&
                AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started &&
                !PlayerControl.LocalPlayer.Data.IsDead &&
                shouldInvert &&
                GameData.Instance &&
                __instance.myPlayer.CanMove)
                __instance.body.velocity *= -1;
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    public static class GameDataHandleDisconnectPatch
    {
        public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (MeetingHud.Instance)
            {
                MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, player.PlayerId);
            }
        }
    }
}