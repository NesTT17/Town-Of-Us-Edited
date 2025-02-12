using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static bool initialized = false;
        public static CustomButton zoomOutButton;
        public static PoolablePlayer targetDisplay;

        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton engineerRepairButton;
        public static TMPro.TMP_Text engineerRepairButtonText;
        private static CustomButton engineerDoorsButton;
        public static CustomButton sheriffKillButton;
        public static CustomButton shifterShiftButton;
        public static CustomButton medicShieldButton;
        public static CustomButton draculaButton;
        public static CustomButton vampireButton;
        public static CustomButton poisonerButton;
        public static CustomButton scavengerEatButton;
        public static CustomButton pursuerButton;
        public static TMPro.TMP_Text pursuerButtonBlanksText;
        public static CustomButton guardianAngelProtectButton;
        public static TMPro.TMP_Text guardianAngelProtectButtonText;
        public static CustomButton survivorSafeguardButton;
        public static TMPro.TMP_Text survivorSafeguardButtonText;
        public static CustomButton fallenAngelKillButton;
        public static CustomButton amnesiacRememberButton;
        public static CustomButton investigatorWatchButton;
        public static TMPro.TMP_Text investigatorWatchButtonText;
        public static CustomButton veteranAlertButton;
        public static TMPro.TMP_Text veteranAlertButtonText;
        public static CustomButton seerRevealButton;
        public static CustomButton juggernautKillButton;
        public static CustomButton swooperSwoopButton;
        public static CustomButton mercenaryShieldButton;
        public static CustomButton blackmailerButton;
        public static CustomButton escapistButton;
        public static CustomButton minerPlaceVentButton;
        public static CustomButton cleanerCleanButton;
        public static CustomButton trapperButton;
        public static TMPro.TMP_Text trapperChargesText;
        public static CustomButton buttonBarryButton;
        public static CustomButton phantomInvisButton;
        public static CustomButton grenadierFlashButton;
        public static CustomButton doomsayerObserveButton;
        public static CustomButton trackerTrackButton;
        public static TMPro.TMP_Text trackerTrackButtonText;
        public static CustomButton werewolfRampageButton;
        public static CustomButton werewolfKillButton;
        public static CustomButton detectiveExamineButton;
        public static CustomButton glitchKillButton;
        public static CustomButton glitchMimicButton;
        public static CustomButton glichHackButton;
        
        public static void setCustomButtonCooldowns() {
            if (!initialized) {
                try {
                    createButtonsPostfix(HudManager.Instance);
                } 
                catch {
                    TownOfUsPlugin.Logger.LogWarning("Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                    return;
                }
            }
            morphlingButton.MaxTimer = Morphling.cooldown;
            morphlingButton.EffectDuration = Morphling.duration;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            camouflagerButton.EffectDuration = Camouflager.duration;
            engineerRepairButton.MaxTimer = 0f;
            engineerDoorsButton.MaxTimer = Engineer.cooldown;
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
            shifterShiftButton.MaxTimer = 0f;
            medicShieldButton.MaxTimer = 0f;
            draculaButton.MaxTimer = Dracula.cooldown;
            vampireButton.MaxTimer = Vampire.cooldown;
            poisonerButton.MaxTimer = Poisoner.cooldown;
            poisonerButton.EffectDuration = Poisoner.delay;
            scavengerEatButton.MaxTimer = Scavenger.cooldown;
            pursuerButton.MaxTimer = Pursuer.cooldown;
            guardianAngelProtectButton.MaxTimer = GuardianAngel.cooldown;
            guardianAngelProtectButton.EffectDuration = GuardianAngel.duration;
            survivorSafeguardButton.MaxTimer = Survivor.cooldown;
            survivorSafeguardButton.EffectDuration = Survivor.duration;
            fallenAngelKillButton.MaxTimer = FallenAngel.cooldown;
            amnesiacRememberButton.MaxTimer = 0f;
            investigatorWatchButton.MaxTimer = 0f;
            veteranAlertButton.MaxTimer = Veteran.cooldown;
            veteranAlertButton.EffectDuration = Veteran.duration;
            seerRevealButton.MaxTimer = Seer.cooldown;
            juggernautKillButton.MaxTimer = Juggernaut.cooldown;
            swooperSwoopButton.MaxTimer = Swooper.swoopCooldown;
            swooperSwoopButton.EffectDuration = Swooper.swoopDuration;
            mercenaryShieldButton.MaxTimer = 0f;
            blackmailerButton.MaxTimer = Blackmailer.cooldown;
            escapistButton.MaxTimer = Escapist.markCooldown;
            escapistButton.EffectDuration = Escapist.recallDuration;
            minerPlaceVentButton.MaxTimer = Miner.placeVentCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.cooldown;
            trapperButton.MaxTimer = Trapper.cooldown;
            buttonBarryButton.MaxTimer = 0f;
            phantomInvisButton.MaxTimer = Phantom.cooldown;
            phantomInvisButton.EffectDuration = Phantom.duration;
            grenadierFlashButton.MaxTimer = Grenadier.cooldown;
            grenadierFlashButton.EffectDuration = Grenadier.duration;
            doomsayerObserveButton.MaxTimer = Doomsayer.cooldown;
            trackerTrackButton.MaxTimer = Tracker.cooldown;
            werewolfRampageButton.MaxTimer = Werewolf.cooldown;
            werewolfRampageButton.EffectDuration = Werewolf.duration;
            werewolfKillButton.MaxTimer = Werewolf.killCooldown;
            detectiveExamineButton.MaxTimer = Detective.initialCooldown;
            glitchKillButton.MaxTimer = Glitch.killCooldown;
            glitchMimicButton.MaxTimer = Glitch.morphCooldown;
            glitchMimicButton.EffectDuration = Glitch.morphDuration;
            glichHackButton.MaxTimer = Glitch.hackCooldown;
            glichHackButton.EffectDuration = Glitch.hackDuration;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            zoomOutButton.MaxTimer = 0f;
        }

        private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null, Vector3? offset=null) {
            if (target == null || button == null) {
                if (targetDisplay != null) {  // Reset the poolable player
                    targetDisplay.gameObject.SetActive(false);
                    GameObject.Destroy(targetDisplay.gameObject);
                    targetDisplay = null;
                }
                return;
            }
            // Add poolable player to the button so that the target outfit is shown
            button.actionButton.cooldownTimerText.transform.localPosition = new Vector3(0, 0, -1f);  // Before the poolable player
            targetDisplay = UnityEngine.Object.Instantiate<PoolablePlayer>(Patches.IntroCutsceneOnDestroyPatch.playerPrefab, button.actionButton.transform);
            NetworkedPlayerInfo data = target.Data;
            target.SetPlayerMaterialColors(targetDisplay.cosmetics.currentBodySprite.BodySprite);
            targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
            targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
            targetDisplay.cosmetics.nameText.text = "";  // Hide the name!
            targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
            if (offset != null) targetDisplay.transform.localPosition += (Vector3)offset;
            targetDisplay.transform.localScale = Vector3.one * 0.33f;
            targetDisplay.setSemiTransparent(false);
            targetDisplay.gameObject.SetActive(true);
        }

        public static void Postfix(HudManager __instance) {
            initialized = false;

            try {
                createButtonsPostfix(__instance);
            } catch { }
        }

        public static void createButtonsPostfix(HudManager __instance) {
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            // Morphling morph
            morphlingButton = new CustomButton(
                () => {
                    if (Morphling.sampledTarget != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MorphlingMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.morphlingMorph(Morphling.sampledTarget.PlayerId);
                        Helpers.checkWatchFlash(Morphling.morphling);
                        Morphling.sampledTarget = null;
                        morphlingButton.EffectDuration = Morphling.duration;
                    } else if (Morphling.currentTarget != null) {
                        if (Helpers.checkAndDoVetKill(Morphling.currentTarget)) {
                            morphlingButton.HasEffect = false;
                            return;
                        }
                        Helpers.checkWatchFlash(Morphling.currentTarget);
                        Morphling.sampledTarget = Morphling.currentTarget;
                        morphlingButton.Sprite = Morphling.getMorphSprite();
                        morphlingButton.EffectDuration = 1f;

                        // Add poolable player to the button so that the target outfit is shown
                        setButtonTargetDisplay(Morphling.sampledTarget, morphlingButton);
                    }
                },
                () => { return Morphling.morphling != null && Morphling.morphling == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove && !Helpers.isActiveCamoComms() && !Helpers.MushroomSabotageActive() && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { 
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Morphling.sampledTarget = null;
                    setButtonTargetDisplay(null);
                },
                Morphling.getSampleSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Morphling.duration, () => {
                    if (Morphling.sampledTarget == null) {
                        morphlingButton.Timer = morphlingButton.MaxTimer;
                        morphlingButton.Sprite = Morphling.getSampleSprite();

                        // Reset the poolable player
                        setButtonTargetDisplay(null);
                    }
                }
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CamouflagerCamouflage, Hazel.SendOption.Reliable, -1);
                    writer.Write(1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.camouflagerCamouflage(1);
                    Helpers.checkWatchFlash(Camouflager.camouflager);
                },
                () => { return Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.isActiveCamoComms() && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Camouflager.duration, () => { camouflagerButton.Timer = camouflagerButton.MaxTimer; }
            );

            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () => {
                    engineerRepairButton.Timer = 0f;
                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedRepair();
                    Helpers.checkWatchFlash(Engineer.engineer);
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator()) {
                        if (task.TaskType == TaskTypes.FixLights) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.engineerFixLights();
                        } else if (task.TaskType == TaskTypes.RestoreOxy) {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        } else if (task.TaskType == TaskTypes.ResetReactor) {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                        } else if (task.TaskType == TaskTypes.ResetSeismic) {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        } else if (task.TaskType == TaskTypes.FixComms) {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        } else if (task.TaskType == TaskTypes.StopCharles) {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                },
                () => { return Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer && Engineer.remainingFixes > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (engineerRepairButtonText != null) engineerRepairButtonText.text = $"{Engineer.remainingFixes}";
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && Engineer.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {},
                Engineer.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );
            // Deputy Handcuff button handcuff counter
            engineerRepairButtonText = GameObject.Instantiate(engineerRepairButton.actionButton.cooldownTimerText, engineerRepairButton.actionButton.cooldownTimerText.transform.parent);
            engineerRepairButtonText.text = "";
            engineerRepairButtonText.enableWordWrapping = false;
            engineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
            engineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Engineer Doors
            engineerDoorsButton = new CustomButton(
                () => {
                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedDoors, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedDoors();
                    Helpers.checkWatchFlash(Engineer.engineer);
                    engineerDoorsButton.Timer = engineerDoorsButton.MaxTimer;
                },
                () => { return Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { engineerDoorsButton.Timer = engineerDoorsButton.MaxTimer; },
                Engineer.getDoorButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.Q
            );

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () => {
                    MurderAttemptResult murderAttemptResult = Helpers.checkMuderAttempt(Sheriff.sheriff, Sheriff.currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;
                    if (murderAttemptResult == MurderAttemptResult.ReverseKill) {
                        Helpers.checkMurderAttemptAndKill(Sheriff.currentTarget, Sheriff.sheriff);
                        return;
                    }

                    if (murderAttemptResult == MurderAttemptResult.PerformKill) {
                        byte targetId = 0;
                        byte originalTargetId = 0;
                        if (Sheriff.currentTarget.Data.Role.IsImpostor || Sheriff.currentTarget.isNeutral() && Sheriff.canKillNeutrals || Sheriff.currentTarget.isNeutralKiller() && Sheriff.canKillKNeutrals) {
                            targetId = Sheriff.currentTarget.PlayerId;
                            originalTargetId = targetId;
                        }
                        else {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            originalTargetId = Sheriff.currentTarget.PlayerId;
                        }

                        // Reset sheriff deathreason
                        if (targetId == Sheriff.sheriff.PlayerId) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareSheriffSelfShot, Hazel.SendOption.Reliable, -1);
                            writer.Write(originalTargetId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.shareSheriffSelfShot(originalTargetId);
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(Sheriff.sheriff.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.uncheckedMurderPlayer(Sheriff.sheriff.Data.PlayerId, targetId, Byte.MaxValue);
                    }

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                    Sheriff.currentTarget = null;
                },
                () => { return Sheriff.sheriff != null && Sheriff.sheriff == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Shifter shift
            shifterShiftButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Shifter.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetFutureShifted, Hazel.SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Shifter.currentTarget);
                },
                () => { return Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer && Shifter.futureShift == null && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { shifterShiftButton.Timer = shifterShiftButton.MaxTimer; },
                Shifter.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Medic.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Medic.currentTarget);
                },
                () => { return Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { medicShieldButton.Timer = medicShieldButton.MaxTimer; },
                Medic.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Dracula Button
            draculaButton = new CustomButton(
                () => {
                    if (Dracula.canCreateVampire) {
                        if (Mayor.mayor != null && Mayor.isRevealed && Dracula.currentTarget == Mayor.mayor) {
                            if (Helpers.checkMurderAttemptAndKill(Dracula.dracula, Dracula.currentTarget) == MurderAttemptResult.SuppressKill) return;
                        } else {
                            if (Helpers.checkAndDoVetKill(Dracula.currentTarget)) return;
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraculaCreatesVampire, Hazel.SendOption.Reliable, -1);
                            writer.Write(Dracula.currentTarget.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.draculaCreatesVampire(Dracula.currentTarget.PlayerId);
                            Helpers.checkWatchFlash(Dracula.currentTarget);
                        }
                    } else {
                        if (Helpers.checkMurderAttemptAndKill(Dracula.dracula, Dracula.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    }

                    draculaButton.Timer = draculaButton.MaxTimer;
                    Dracula.currentTarget = null;
                },
                () => { return Dracula.dracula != null && Dracula.dracula == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Dracula.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { draculaButton.Timer = draculaButton.MaxTimer; },
                Dracula.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Vampire Button
            vampireButton  = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(Vampire.vampire, Vampire.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    
                    vampireButton.Timer = vampireButton.MaxTimer;
                    Vampire.currentTarget = null;
                },
                () => { return Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Vampire.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { draculaButton.Timer = draculaButton.MaxTimer; },
                Dracula.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Poisoner Button
            poisonerButton = new CustomButton(
                () => {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Poisoner.poisoner, Poisoner.currentTarget);
                    if (murder == MurderAttemptResult.PerformKill) {
                        Poisoner.poisoned = Poisoner.currentTarget;

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                        writer.Write(Poisoner.poisoned.PlayerId);
                        writer.Write((byte)0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.poisonerSetPoisoned(Poisoner.poisoned.PlayerId, 0);

                        byte lastTimer = (byte)Poisoner.delay;
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Poisoner.delay, new Action<float>((p) => { // Delayed action
                            if (p <= 1f) {
                                byte timer = (byte)poisonerButton.Timer;
                                if (timer != lastTimer) {
                                    lastTimer = timer;
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    writer.Write((byte)RPCProcedure.GhostInfoTypes.PoisonerTimer);
                                    writer.Write(timer);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                }
                            }
                            if (p == 1f) {
                                var res = Helpers.checkMurderAttemptAndKill(Poisoner.poisoner, Poisoner.poisoned, showAnimation: false);
                                if (res == MurderAttemptResult.PerformKill) {
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                                    writer.Write(byte.MaxValue);
                                    writer.Write(byte.MaxValue);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue);
                                }
                            }
                        })));
                        poisonerButton.HasEffect = true; // Trigger effect on this click
                    } else if (murder == MurderAttemptResult.BlankKill) {
                        if (Poisoner.currentTarget == Mercenary.shielded) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.mercenaryAddMurder();
                        }
                        poisonerButton.Timer = poisonerButton.MaxTimer;
                        poisonerButton.HasEffect = false;
                    } else if (murder == MurderAttemptResult.ReverseKill) {
                        Helpers.checkMurderAttemptAndKill(Poisoner.currentTarget, Poisoner.poisoner);
                        poisonerButton.HasEffect = false;
                        return;
                    } else {
                        poisonerButton.HasEffect = false;
                    }
                },
                () => { return Poisoner.poisoner != null && Poisoner.poisoner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Poisoner.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    poisonerButton.Timer = poisonerButton.MaxTimer;
                    poisonerButton.isEffectActive = false;
                    poisonerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Poisoner.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.Q,
                false, 0f, () => { poisonerButton.Timer = poisonerButton.MaxTimer; }
            );

            // Scavenger Eat
            scavengerEatButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody") {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported) {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false)) {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    writer.Write(Scavenger.scavenger.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.cleanBody(playerInfo.PlayerId, Scavenger.scavenger.PlayerId);
                                    Helpers.checkWatchFlash(Helpers.playerById(playerInfo.PlayerId));

                                    scavengerEatButton.Timer = scavengerEatButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Scavenger.scavenger != null && Scavenger.scavenger == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { scavengerEatButton.Timer = scavengerEatButton.MaxTimer; },
                Scavenger.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Pursuer button
            pursuerButton = new CustomButton(
                () => {
                    if (Pursuer.target != null) {
                        if (Helpers.checkAndDoVetKill(Pursuer.target)) return;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                        writer.Write(Pursuer.target.PlayerId);
                        writer.Write(Byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setBlanked(Pursuer.target.PlayerId, Byte.MaxValue);
                        Helpers.checkWatchFlash(Pursuer.target);

                        Pursuer.target = null;
                        Pursuer.blanks++;
                        pursuerButton.Timer = pursuerButton.MaxTimer;
                    }

                },
                () => { return Pursuer.pursuer != null && Pursuer.pursuer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = $"{Pursuer.blanksNumber - Pursuer.blanks}";
                    return Pursuer.blanksNumber > Pursuer.blanks && PlayerControl.LocalPlayer.CanMove && Pursuer.target != null && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.getTargetSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            // Pursuer button blanks left
            pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonBlanksText.text = "";
            pursuerButtonBlanksText.enableWordWrapping = false;
            pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Guardian Angel Protect
            guardianAngelProtectButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelProtect, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelProtect();
                    Helpers.checkWatchFlash(GuardianAngel.target);
                },
                () => { return GuardianAngel.guardianAngel != null && GuardianAngel.guardianAngel == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (guardianAngelProtectButtonText != null) guardianAngelProtectButtonText.text = $"{GuardianAngel.remainingProtects}";
                    return GuardianAngel.remainingProtects > 0 && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {
                    guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer;
                    guardianAngelProtectButton.isEffectActive = false;
                    guardianAngelProtectButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                GuardianAngel.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F,
                true, GuardianAngel.duration, () => { guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer; }
            );
            // Guardian Angel button protects left
            guardianAngelProtectButtonText = GameObject.Instantiate(guardianAngelProtectButton.actionButton.cooldownTimerText, guardianAngelProtectButton.actionButton.cooldownTimerText.transform.parent);
            guardianAngelProtectButtonText.text = "";
            guardianAngelProtectButtonText.enableWordWrapping = false;
            guardianAngelProtectButtonText.transform.localScale = Vector3.one * 0.5f;
            guardianAngelProtectButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Survivor Safeguard
            survivorSafeguardButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SurvivorSafeguard, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.survivorSafeguard();
                    Helpers.checkWatchFlash(Survivor.survivor);
                },
                () => { return Survivor.survivor != null && Survivor.survivor == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (survivorSafeguardButtonText != null) survivorSafeguardButtonText.text = $"{Survivor.remainingSafeguards}";
                    return Survivor.remainingSafeguards > 0 && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {
                    survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer;
                    survivorSafeguardButton.isEffectActive = false;
                    survivorSafeguardButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Survivor.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F,
                true, Survivor.duration, () => { survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer; }
            );
            // Survivor button safeguards left
            survivorSafeguardButtonText = GameObject.Instantiate(survivorSafeguardButton.actionButton.cooldownTimerText, survivorSafeguardButton.actionButton.cooldownTimerText.transform.parent);
            survivorSafeguardButtonText.text = "";
            survivorSafeguardButtonText.enableWordWrapping = false;
            survivorSafeguardButtonText.transform.localScale = Vector3.one * 0.5f;
            survivorSafeguardButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Fallen Angel Kill
            fallenAngelKillButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(FallenAngel.fallenAngel, FallenAngel.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    FallenAngel.currentTarget = null;
                    fallenAngelKillButton.Timer = fallenAngelKillButton.MaxTimer;
                },
                () => { return FallenAngel.fallenAngel != null && FallenAngel.fallenAngel == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return FallenAngel.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { fallenAngelKillButton.Timer = fallenAngelKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Amnesiac Remember
            amnesiacRememberButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody") {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported) {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false)) {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AmnesiacRemember, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.amnesiacRemember(playerInfo.PlayerId);
                                    Helpers.checkWatchFlash(Helpers.playerById(playerInfo.PlayerId));

                                    amnesiacRememberButton.Timer = amnesiacRememberButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Amnesiac.amnesiac != null && Amnesiac.amnesiac == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { amnesiacRememberButton.Timer = amnesiacRememberButton.MaxTimer; },
                Amnesiac.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Investigator Watch
            investigatorWatchButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Investigator.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.InvestigatorWatchPlayer, SendOption.Reliable, -1);
                    writer.Write(Investigator.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.investigatorWatchPlayer(Investigator.currentTarget.PlayerId);
                },
                () => { return Investigator.investigator != null && Investigator.investigator == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (investigatorWatchButtonText != null) investigatorWatchButtonText.text = Investigator.watching == null ? "" : $"{Investigator.watching.Data.DefaultOutfit.PlayerName}";
                    return Investigator.currentTarget && Investigator.watching == null && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; 
                },
                () => {
                    Investigator.watching = null;
                    investigatorWatchButton.Timer = investigatorWatchButton.MaxTimer;
                },
                Investigator.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );
            // Investigator Watch show watching
            investigatorWatchButtonText = GameObject.Instantiate(investigatorWatchButton.actionButton.cooldownTimerText, investigatorWatchButton.actionButton.cooldownTimerText.transform.parent);
            investigatorWatchButtonText.text = "";
            investigatorWatchButtonText.enableWordWrapping = false;
            investigatorWatchButtonText.transform.localScale = Vector3.one * 0.5f;
            investigatorWatchButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Veteran alert
            veteranAlertButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VeteranAlert, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.veteranAlert();
                    Helpers.checkWatchFlash(Veteran.veteran);
                },
                () => { return Veteran.veteran != null && Veteran.veteran == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (veteranAlertButtonText != null) veteranAlertButtonText.text = $"{Veteran.remainingAlerts}";
                    return Veteran.remainingAlerts > 0 && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {
                    veteranAlertButton.Timer = veteranAlertButton.MaxTimer;
                    veteranAlertButton.isEffectActive = false;
                    veteranAlertButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Veteran.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, Veteran.duration, () => { veteranAlertButton.Timer = veteranAlertButton.MaxTimer; }
            );
            // Veteran alert button counter
            veteranAlertButtonText = GameObject.Instantiate(veteranAlertButton.actionButton.cooldownTimerText, veteranAlertButton.actionButton.cooldownTimerText.transform.parent);
            veteranAlertButtonText.text = "";
            veteranAlertButtonText.enableWordWrapping = false;
            veteranAlertButtonText.transform.localScale = Vector3.one * 0.5f;
            veteranAlertButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Seer Reveal
            seerRevealButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Seer.currentTarget)) return;
                    Seer.revealedPlayers.Add(Seer.currentTarget);
                    seerRevealButton.Timer = seerRevealButton.MaxTimer;
                    Helpers.checkWatchFlash(Seer.currentTarget);
                },
                () => { return Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Seer.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { seerRevealButton.Timer = seerRevealButton.MaxTimer; },
                Seer.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Juggernaut Kill
            juggernautKillButton = new CustomButton(
                () => {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Juggernaut.juggernaut, Juggernaut.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill) {
                        Helpers.checkMurderAttemptAndKill(Juggernaut.currentTarget, Juggernaut.juggernaut);
                        return;
                    }

                    if (murder == MurderAttemptResult.BlankKill || murder == MurderAttemptResult.PerformKill) {
                        if (murder == MurderAttemptResult.PerformKill) {
                            Helpers.MurderPlayer(Juggernaut.juggernaut, Juggernaut.currentTarget, true);
                            juggernautKillButton.MaxTimer -= Juggernaut.cooldownReduce;
                        }

                        if (murder == MurderAttemptResult.BlankKill && Juggernaut.currentTarget == Mercenary.shielded) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.mercenaryAddMurder();
                        }

                        Juggernaut.currentTarget = null;
                        juggernautKillButton.Timer = juggernautKillButton.MaxTimer;
                    }
                },
                () => { return Juggernaut.juggernaut != null && Juggernaut.juggernaut == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Juggernaut.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { juggernautKillButton.Timer = juggernautKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Swooper Swoop
            swooperSwoopButton = new CustomButton(
                () => {
                    MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                    invisibleWriter.Write(Swooper.swooper.PlayerId);
                    invisibleWriter.Write(byte.MinValue);
                    AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                    RPCProcedure.swooperSwoop(Swooper.swooper.PlayerId, byte.MinValue);
                    Helpers.checkWatchFlash(Swooper.swooper);
                },
                () => { return Swooper.swooper != null && Swooper.swooper == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    swooperSwoopButton.Timer = swooperSwoopButton.MaxTimer;
                    swooperSwoopButton.isEffectActive = false;
                    swooperSwoopButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Swooper.getSwoopButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Swooper.swoopDuration, () => { swooperSwoopButton.Timer = swooperSwoopButton.MaxTimer; }
            );

            // Mercenary Shield
            mercenaryShieldButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Mercenary.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryShield, Hazel.SendOption.Reliable, -1);
                    writer.Write(Mercenary.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryShield(Mercenary.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Mercenary.currentTarget);
                    mercenaryShieldButton.Timer = mercenaryShieldButton.MaxTimer;
                },
                () => { return Mercenary.mercenary != null && Mercenary.mercenary == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Mercenary.shielded == null && Mercenary.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { mercenaryShieldButton.Timer = mercenaryShieldButton.MaxTimer; },
                Mercenary.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Blackmailer Blackmail
            blackmailerButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Blackmailer.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BlackmailPlayer, SendOption.Reliable, -1);
                    writer.Write(Blackmailer.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.blackmailPlayer(Blackmailer.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Blackmailer.currentTarget);
                },
                () => { return Blackmailer.blackmailer != null && Blackmailer.blackmailer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Blackmailer.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    blackmailerButton.Timer = blackmailerButton.MaxTimer;
                },
                Blackmailer.getBlackmailButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Escapist Button
            escapistButton = new CustomButton(
                () => {
                    Helpers.checkWatchFlash(Escapist.escapist);
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));
                    
                    if (Escapist.markedLocation == null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EscapistMarkLocation, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.escapistMarkLocation(buff);
                        escapistButton.Sprite = Escapist.getRecallButtonSprite();
                        escapistButton.Timer = 10f;
                        escapistButton.HasEffect = false;
                    } else {
                        // Jump to location
                        var exit = (Vector3)Escapist.markedLocation;

                        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EscapistRecall, Hazel.SendOption.Reliable);
                        writer.Write(Byte.MaxValue);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.escapistRecall(true, buff);

                        escapistButton.EffectDuration = Escapist.recallDuration;
                        escapistButton.Timer = 10f;
                        escapistButton.HasEffect = true;
                    }
                },
                () => { return Escapist.escapist != null && Escapist.escapist == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    if (Escapist.markStaysOverMeeting) {
                        escapistButton.Timer = 10f;
                    } else {
                        Escapist.markedLocation = null;
                        escapistButton.Timer = escapistButton.MaxTimer;
                        escapistButton.Sprite = Escapist.getMarkButtonSprite();
                    }

                    if (escapistButton.isEffectActive) {
                        escapistButton.Timer = escapistButton.MaxTimer;
                        escapistButton.isEffectActive = false;
                        escapistButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                        escapistButton.HasEffect = false;
                        escapistButton.Sprite = Escapist.getMarkButtonSprite();
                    }
                },
                Escapist.getMarkButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Escapist.recallDuration, () => {
                    if (TransportationToolPatches.isUsingTransportation(Escapist.escapist)) {
                        escapistButton.Timer = 0.5f;
                        escapistButton.isEffectActive = true;
                        escapistButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                        return;
                    }
                    if (Escapist.escapist.inVent) {
                        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    }

                    // jump back!
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));
                    var exit = (Vector3)Escapist.markedLocation;

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EscapistRecall, Hazel.SendOption.Reliable);
                    writer.Write((byte)0);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.escapistRecall(false, buff);

                    escapistButton.Timer = escapistButton.MaxTimer;
                    escapistButton.isEffectActive = false;
                    escapistButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    escapistButton.HasEffect = false;
                    escapistButton.Sprite = Escapist.getMarkButtonSprite();
                    if (Minigame.Instance) {
                        Minigame.Instance.Close();
                    }
                }
            );

            // Miner Place Vent
            minerPlaceVentButton = new CustomButton(
                () => {
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0*sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1*sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceVent, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeVent(buff);

                    Helpers.checkWatchFlash(Miner.miner);

                    minerPlaceVentButton.Timer = minerPlaceVentButton.MaxTimer;
                },
                () => { return Miner.miner != null && Miner.miner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !MinerVent.hasLimitReached() && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { minerPlaceVentButton.Timer = minerPlaceVentButton.MaxTimer; },
                Miner.getPlaceBoxButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    writer.Write(Cleaner.cleaner.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.cleanBody(playerInfo.PlayerId, Cleaner.cleaner.PlayerId);

                                    Cleaner.cleaner.killTimer = cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                                    Helpers.checkWatchFlash(Helpers.playerById(playerInfo.PlayerId));
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Cleaner.cleaner != null && Cleaner.cleaner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Trapper button
            trapperButton = new CustomButton(
                () => {
                    Helpers.checkWatchFlash(Trapper.trapper);

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    pos.z += 0.001f;
                    Trapper.traps.Add(TrapExtentions.CreateTrap(pos));
                    Trapper.maxTraps--;

                    trapperButton.Timer = trapperButton.MaxTimer;
                },
                () => { return Trapper.trapper != null && Trapper.trapper == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (trapperChargesText != null) trapperChargesText.text = $"{Trapper.maxTraps}";
                    return PlayerControl.LocalPlayer.CanMove && Trapper.maxTraps > 0 && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => { trapperButton.Timer = trapperButton.MaxTimer; },
                Trapper.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            // Trapper Charges
            trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText, trapperButton.actionButton.cooldownTimerText.transform.parent);
            trapperChargesText.text = "";
            trapperChargesText.enableWordWrapping = false;
            trapperChargesText.transform.localScale = Vector3.one * 0.5f;
            trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Button Barry
            buttonBarryButton = new CustomButton(
                () => {
                    Helpers.handlePoisonerKillOnBodyReport();
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedCmdReportDeadBody(ButtonBarry.buttonBarry.PlayerId, byte.MaxValue);
                    ButtonBarry.usedButton = true;
                },
                () => { return ButtonBarry.buttonBarry != null && ButtonBarry.buttonBarry == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !ButtonBarry.usedButton && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;},
                () => { buttonBarryButton.Timer = buttonBarryButton.MaxTimer; },
                ButtonBarry.getButtonSprite(), new Vector3(0f, 1f, 0f), __instance, null, true
            );

            // Phantom Invis
            phantomInvisButton = new CustomButton(
                () => {
                    MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PhantomInvis, Hazel.SendOption.Reliable, -1);
                    invisibleWriter.Write(Phantom.phantom.PlayerId);
                    invisibleWriter.Write(byte.MinValue);
                    AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                    RPCProcedure.phantomInvis(Phantom.phantom.PlayerId, byte.MinValue);
                    Helpers.checkWatchFlash(Phantom.phantom);
                },
                () => { return Phantom.phantom != null && Phantom.phantom == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    phantomInvisButton.Timer = phantomInvisButton.MaxTimer;
                    phantomInvisButton.isEffectActive = false;
                    phantomInvisButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Phantom.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Phantom.duration, () => { phantomInvisButton.Timer = phantomInvisButton.MaxTimer; }, false, "Invis"
            );

            // Grenadier Flash
            grenadierFlashButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.grenadierFlash(false);
                    Helpers.checkWatchFlash(Grenadier.grenadier);
                },
                () => { return Grenadier.grenadier != null && Grenadier.grenadier == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;

                    return !sabotageActive && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {
                    grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer;
                    grenadierFlashButton.isEffectActive = false;
                    grenadierFlashButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Grenadier.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Grenadier.duration, () => { grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer; }
            );

            // Doomsayer Observe
            doomsayerObserveButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Doomsayer.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DoomsayerObserve, SendOption.Reliable, -1);
                    writer.Write(Doomsayer.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.doomsayerObserve(Doomsayer.currentTarget.PlayerId);
                    doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer;

                    Helpers.checkWatchFlash(Doomsayer.currentTarget);
                },
                () => { return Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Doomsayer.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer; },
                Doomsayer.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Tracker Track
            trackerTrackButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Tracker.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TrackerUseTrack, SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.trackerUseTrack(Tracker.currentTarget.PlayerId);
                    trackerTrackButton.Timer = trackerTrackButton.MaxTimer;

                    Helpers.checkWatchFlash(Tracker.currentTarget);
                },
                () => { return Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (trackerTrackButtonText != null) trackerTrackButtonText.text = $"{Tracker.maxTracksPerRound - Tracker.tracksInRound}";
                    return Tracker.currentTarget && Tracker.tracksInRound < Tracker.maxTracksPerRound && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening;
                },
                () => {
                    trackerTrackButton.Timer = trackerTrackButton.MaxTimer;
                },
                Tracker.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            // Tracker tracks
            trackerTrackButtonText = GameObject.Instantiate(trackerTrackButton.actionButton.cooldownTimerText, trackerTrackButton.actionButton.cooldownTimerText.transform.parent);
            trackerTrackButtonText.text = "";
            trackerTrackButtonText.enableWordWrapping = false;
            trackerTrackButtonText.transform.localScale = Vector3.one * 0.5f;
            trackerTrackButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Werewolf Rampage
            werewolfRampageButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WerewolfRampage, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.werewolfRampage();

                    Helpers.checkWatchFlash(Werewolf.werewolf);
                },
                () => { return Werewolf.werewolf != null && Werewolf.werewolf == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer;
                    werewolfRampageButton.isEffectActive = false;
                    werewolfRampageButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Werewolf.getButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, __instance, KeyCode.F,
                true, Werewolf.duration, () => { werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer; }
            );

            // Werewolf Kill
            werewolfKillButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(Werewolf.werewolf, Werewolf.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    werewolfKillButton.Timer = werewolfKillButton.MaxTimer;
                    Werewolf.currentTarget = null;
                },
                () => { return Werewolf.werewolf != null && Werewolf.werewolf == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Werewolf.currentTarget && Werewolf.isRampageActive && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { werewolfKillButton.Timer = werewolfKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Detective Examine
            detectiveExamineButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Detective.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveExamine, SendOption.Reliable, -1);
                    writer.Write(Detective.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.detectiveExamine(Detective.currentTarget.PlayerId);
                    detectiveExamineButton.Timer = Detective.cooldown;

                    if (Detective.currentTarget != null) {
                        DeadPlayer player = GameHistory.deadPlayers.Where(x => x.killerIfExisting.PlayerId == Detective.currentTarget.PlayerId).FirstOrDefault();
                        if (player == null) {
                            if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green);
                        } else {
                            float timeSinceDeath = ((float)(DateTime.UtcNow - player.timeOfDeath).TotalMilliseconds);
                            if (timeSinceDeath < Detective.bloodTime * 1000) {
                                if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.red);
                            } else {
                                if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green);
                            }
                        }
                    }

                    Helpers.checkWatchFlash(Detective.currentTarget);
                },
                () => { return Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Detective.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { detectiveExamineButton.Timer = detectiveExamineButton.MaxTimer; },
                Detective.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Glitch Kill
            glitchKillButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(Glitch.glitch, Glitch.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    glitchKillButton.Timer = glitchKillButton.MaxTimer;
                    Glitch.currentTarget = null;
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Glitch.currentTarget && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => { glitchKillButton.Timer = glitchKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
            
            // Glitch Mimic
            glitchMimicButton = new CustomButton(
                () => {
                    if (Glitch.sampledPlayer == null) {
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        foreach (var player in PlayerControl.AllPlayerControls) {
                            if (player != PlayerControl.LocalPlayer && !player.Data.Disconnected) {
                                if (!player.Data.IsDead) Glitch.mimicTargets.Add(player.PlayerId);
                                else {
                                    foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>()) {
                                        if (body.ParentId == player.PlayerId) Glitch.mimicTargets.Add(player.PlayerId);
                                    }
                                }
                            }
                        }
                        byte[] mimictargetIDs = Glitch.mimicTargets.ToArray();
                        var pk = new PlayerMenu((x) => {
                            Glitch.sampledPlayer = x;
                        }, (y) => {
                            return mimictargetIDs.Contains(y.PlayerId);
                        });
                        Coroutines.Start(pk.Open(0f, true));
                        glitchMimicButton.HasEffect = false;
                    } else {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchMimic, SendOption.Reliable, -1);
                        writer.Write(Glitch.sampledPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.glitchMimic(Glitch.sampledPlayer.PlayerId);
                        Helpers.checkWatchFlash(Glitch.glitch);
                        Glitch.sampledPlayer = null;
                        glitchMimicButton.HasEffect = true;
                        glitchMimicButton.EffectDuration = Glitch.morphDuration;
                    }                    
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;},
                () => { return PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    glitchMimicButton.Timer = glitchMimicButton.MaxTimer;
                    glitchMimicButton.isEffectActive = false;
                    glitchMimicButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Glitch.sampledPlayer = null;
                },
                Glitch.getMimicButtonSprite(), new Vector3(0f, 1f, 0f), __instance, KeyCode.F,
                true, Glitch.morphDuration, () => { glitchMimicButton.Timer = glitchMimicButton.MaxTimer; }, true
            );

            // Glitch Hack
            glichHackButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Glitch.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchHack, SendOption.Reliable, -1);
                    writer.Write(Glitch.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.glitchHack(Glitch.currentTarget.PlayerId);

                    Helpers.checkWatchFlash(Glitch.currentTarget);
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Glitch.currentTarget && Glitch.hackedPlayer == null && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.Chat.IsOpenOrOpening; },
                () => {
                    glichHackButton.Timer = glichHackButton.MaxTimer;
                    glichHackButton.isEffectActive = false;
                    glichHackButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Glitch.sampledPlayer = null;
                },
                Glitch.getHackButtonSprite(), new Vector3(0f, 0f, 0f), __instance, KeyCode.G,
                true, Glitch.hackDuration, () => { glichHackButton.Timer = glichHackButton.MaxTimer; }, true
            );

            // Zoom Button
            zoomOutButton = new CustomButton(
                () => { Helpers.toggleZoom();
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { return; },
                null, new Vector3(0.4f, 2.8f, 0), __instance, KeyCode.KeypadPlus
            );
            zoomOutButton.Timer = 0f;
            
            // Set the default (or settings from the previous game) timers / durations when spawning the buttons
            initialized = true;
            setCustomButtonCooldowns();
        }
    }
}