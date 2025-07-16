using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
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
        public static CustomButton buttonBarryButton;
        public static CustomButton phantomInvisButton;
        public static CustomButton grenadierFlashButton;
        public static CustomButton doomsayerObserveButton;
        public static CustomButton werewolfRampageButton;
        public static CustomButton werewolfKillButton;
        public static CustomButton detectiveExamineButton;
        public static CustomButton glitchKillButton;
        public static CustomButton glitchMimicButton;
        public static CustomButton glichHackButton;
        public static CustomButton venererAbilityButton;
        public static CustomButton disperserDisperseButton;
        public static CustomButton bomberButton;
        public static CustomButton vampireHunterStakeButton;
        public static TMPro.TMP_Text vampireHunterStakeButtonText;
        public static CustomButton vhVeteranAlertButton;
        public static TMPro.TMP_Text vhVeteranAlertButtonText;
        public static CustomButton timeLordShieldButton;
        public static CustomButton timeLordRewindTimeButton;
        public static CustomButton oracleConfessButton;
        public static CustomButton medusaPetrifyButton;
        public static CustomButton archerShowWeaponButton;
        public static CustomButton archerKillButton;
        
        public static void setCustomButtonCooldowns()
        {
            if (!initialized)
            {
                try
                {
                    createButtonsPostfix(HudManager.Instance);
                }
                catch
                {
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
            buttonBarryButton.MaxTimer = 0f;
            phantomInvisButton.MaxTimer = Phantom.cooldown;
            phantomInvisButton.EffectDuration = Phantom.duration;
            grenadierFlashButton.MaxTimer = Grenadier.cooldown;
            grenadierFlashButton.EffectDuration = Grenadier.duration;
            doomsayerObserveButton.MaxTimer = Doomsayer.cooldown;
            werewolfRampageButton.MaxTimer = Werewolf.cooldown;
            werewolfRampageButton.EffectDuration = Werewolf.duration;
            werewolfKillButton.MaxTimer = Werewolf.killCooldown;
            detectiveExamineButton.MaxTimer = Detective.initialCooldown;
            glitchKillButton.MaxTimer = Glitch.killCooldown;
            glitchMimicButton.MaxTimer = Glitch.morphCooldown;
            glitchMimicButton.EffectDuration = Glitch.morphDuration;
            glichHackButton.MaxTimer = Glitch.hackCooldown;
            glichHackButton.EffectDuration = Glitch.hackDuration;
            venererAbilityButton.MaxTimer = Venerer.cooldown;
            venererAbilityButton.EffectDuration = Venerer.duration;
            disperserDisperseButton.MaxTimer = 0f;
            bomberButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            bomberButton.EffectDuration = Bomber.delay;
            vampireHunterStakeButton.MaxTimer = VampireHunter.stakeCooldown;
            vhVeteranAlertButton.MaxTimer = VampireHunter.cooldown;
            vhVeteranAlertButton.EffectDuration = VampireHunter.duration;
            timeLordShieldButton.MaxTimer = TimeLord.cooldown;
            timeLordShieldButton.EffectDuration = TimeLord.shieldDuration;
            timeLordRewindTimeButton.MaxTimer = TimeLord.rewindCooldown;
            timeLordRewindTimeButton.EffectDuration = TimeLord.rewindTime;
            oracleConfessButton.MaxTimer = Oracle.cooldown;
            medusaPetrifyButton.MaxTimer = Medusa.cooldown;
            medusaPetrifyButton.EffectDuration = Medusa.delay;
            archerShowWeaponButton.MaxTimer = 10f;
            archerKillButton.MaxTimer = Archer.cooldown;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            zoomOutButton.MaxTimer = 0f;
        }

        public static void resetTimeLordButton()
        {
            timeLordShieldButton.Timer = timeLordShieldButton.MaxTimer;
            timeLordShieldButton.isEffectActive = false;
            timeLordShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }

        private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null, Vector3? offset = null)
        {
            if (target == null || button == null)
            {
                if (targetDisplay != null)
                {  // Reset the poolable player
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
                        if (Helpers.checkAndDoVHVetKill(Morphling.currentTarget)) {
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
                () => { return (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove && !Helpers.isActiveCamoComms() && !Helpers.MushroomSabotageActive(); },
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
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.isActiveCamoComms(); },
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
                    return sabotageActive && Engineer.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
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
                () => { return PlayerControl.LocalPlayer.CanMove; },
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

                        // Armored sheriff shot doesnt kill if backfired
                        if (targetId == Sheriff.sheriff.PlayerId && Helpers.checkArmored(Sheriff.sheriff, true, true))
                            return;
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
                () => { return Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Shifter shift
            shifterShiftButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Shifter.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Shifter.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetFutureShifted, Hazel.SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Shifter.currentTarget);
                },
                () => { return Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer && Shifter.futureShift == null && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove; },
                () => { shifterShiftButton.Timer = shifterShiftButton.MaxTimer; },
                Shifter.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Medic.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Medic.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Medic.currentTarget);
                },
                () => { return Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { medicShieldButton.Timer = medicShieldButton.MaxTimer; },
                Medic.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Dracula Button
            draculaButton = new CustomButton(
                () => {
                    if (Dracula.canCreateVampire) {
                        if (VampireHunter.vampireHunter != null && Dracula.currentTarget == VampireHunter.vampireHunter)
                        {
                            Helpers.checkMurderAttemptAndKill(Dracula.currentTarget, Dracula.dracula);
                            return;
                        }
                        else if (Mayor.mayor != null && Mayor.isRevealed && Dracula.currentTarget == Mayor.mayor)
                        {
                            Helpers.checkMurderAttemptAndKill(Dracula.dracula, Dracula.currentTarget);
                            return;
                        }
                        else
                        {
                            if (Helpers.checkAndDoVetKill(Dracula.currentTarget)) return;
                            if (Helpers.checkAndDoVHVetKill(Dracula.currentTarget)) return;
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
                () => { return Dracula.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return Vampire.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { vampireButton.Timer = vampireButton.MaxTimer; },
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
                () => { return Poisoner.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { scavengerEatButton.Timer = scavengerEatButton.MaxTimer; },
                Scavenger.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Pursuer button
            pursuerButton = new CustomButton(
                () => {
                    if (Pursuer.target != null) {
                        if (Helpers.checkAndDoVetKill(Pursuer.target)) return;
                        if (Helpers.checkAndDoVHVetKill(Pursuer.target)) return;
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
                    return Pursuer.blanksNumber > Pursuer.blanks && PlayerControl.LocalPlayer.CanMove && Pursuer.target != null;
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
                    return GuardianAngel.remainingProtects > 0 && PlayerControl.LocalPlayer.CanMove;
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
                    return Survivor.remainingSafeguards > 0 && PlayerControl.LocalPlayer.CanMove;
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
                () => { return FallenAngel.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { amnesiacRememberButton.Timer = amnesiacRememberButton.MaxTimer; },
                Amnesiac.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Investigator Watch
            investigatorWatchButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Investigator.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Investigator.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.InvestigatorWatchPlayer, SendOption.Reliable, -1);
                    writer.Write(Investigator.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.investigatorWatchPlayer(Investigator.currentTarget.PlayerId);
                },
                () => { return Investigator.investigator != null && Investigator.investigator == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (investigatorWatchButtonText != null) investigatorWatchButtonText.text = Investigator.watching == null ? "" : $"{Investigator.watching.Data.DefaultOutfit.PlayerName}";
                    return Investigator.currentTarget && Investigator.watching == null && PlayerControl.LocalPlayer.CanMove; 
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
                    return Veteran.remainingAlerts > 0 && PlayerControl.LocalPlayer.CanMove;
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
                () => { return Seer.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return Juggernaut.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return PlayerControl.LocalPlayer.CanMove; },
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
                    if (Helpers.checkAndDoVHVetKill(Mercenary.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryShield, Hazel.SendOption.Reliable, -1);
                    writer.Write(Mercenary.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryShield(Mercenary.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Mercenary.currentTarget);
                    mercenaryShieldButton.Timer = mercenaryShieldButton.MaxTimer;
                },
                () => { return Mercenary.mercenary != null && Mercenary.mercenary == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Mercenary.shielded == null && Mercenary.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { mercenaryShieldButton.Timer = mercenaryShieldButton.MaxTimer; },
                Mercenary.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Blackmailer Blackmail
            blackmailerButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Blackmailer.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Blackmailer.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BlackmailPlayer, SendOption.Reliable, -1);
                    writer.Write(Blackmailer.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.blackmailPlayer(Blackmailer.currentTarget.PlayerId);
                    Helpers.checkWatchFlash(Blackmailer.currentTarget);
                },
                () => { return Blackmailer.blackmailer != null && Blackmailer.blackmailer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Blackmailer.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return PlayerControl.LocalPlayer.CanMove; },
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
                () => { return PlayerControl.LocalPlayer.CanMove && !MinerVent.hasLimitReached(); },
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
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Button Barry
            buttonBarryButton = new CustomButton(
                () => {
                    PlayerControl.LocalPlayer.RemainingEmergencies++;
                    Helpers.handlePoisonerKillOnBodyReport();
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedCmdReportDeadBody(ButtonBarry.buttonBarry.PlayerId, byte.MaxValue);
                    ButtonBarry.usedButton = true;
                },
                () => { return ButtonBarry.buttonBarry != null && ButtonBarry.buttonBarry == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !ButtonBarry.usedButton && PlayerControl.LocalPlayer.CanMove;},
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
                () => { return PlayerControl.LocalPlayer.CanMove; },
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

                    return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
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
                    if (Helpers.checkAndDoVHVetKill(Doomsayer.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DoomsayerObserve, SendOption.Reliable, -1);
                    writer.Write(Doomsayer.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.doomsayerObserve(Doomsayer.currentTarget.PlayerId);
                    doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer;

                    Helpers.checkWatchFlash(Doomsayer.currentTarget);
                },
                () => { return Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Doomsayer.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer; },
                Doomsayer.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F,
                true, 0f, () =>
                {
                    doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer;
                    var msg = Doomsayer.GetInfo(Doomsayer.currentTarget);

                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{msg}", false);

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
            );

            // Werewolf Rampage
            werewolfRampageButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WerewolfRampage, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.werewolfRampage();

                    Helpers.checkWatchFlash(Werewolf.werewolf);
                },
                () => { return Werewolf.werewolf != null && Werewolf.werewolf == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
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
                () => { return Werewolf.currentTarget && Werewolf.isRampageActive && PlayerControl.LocalPlayer.CanMove; },
                () => { werewolfKillButton.Timer = werewolfKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Detective Examine
            detectiveExamineButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(Detective.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Detective.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveExamine, SendOption.Reliable, -1);
                    writer.Write(Detective.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.detectiveExamine(Detective.currentTarget.PlayerId);
                    detectiveExamineButton.Timer = Detective.cooldown;

                    if (Detective.currentTarget != null) {
                        DeadPlayer player = GameHistory.deadPlayers.Where(x => x.killerIfExisting.PlayerId == Detective.currentTarget.PlayerId).FirstOrDefault();
                        if (player == null) {
                            if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green, message: "GREEN");
                        } else {
                            float timeSinceDeath = ((float)(DateTime.UtcNow - player.timeOfDeath).TotalMilliseconds);
                            if (timeSinceDeath < Detective.bloodTime * 1000) {
                                if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.red, message: "RED");
                            } else {
                                if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green, message: "GREEN");
                            }
                        }
                    }

                    Helpers.checkWatchFlash(Detective.currentTarget);
                },
                () => { return Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Detective.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return Glitch.currentTarget && PlayerControl.LocalPlayer.CanMove; },
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
                () => { return PlayerControl.LocalPlayer.CanMove; },
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
                    if (Helpers.checkAndDoVHVetKill(Glitch.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchHack, SendOption.Reliable, -1);
                    writer.Write(Glitch.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.glitchHack(Glitch.currentTarget.PlayerId);

                    Helpers.checkWatchFlash(Glitch.currentTarget);
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Glitch.currentTarget && Glitch.hackedPlayer == null && PlayerControl.LocalPlayer.CanMove; },
                () => {
                    glichHackButton.Timer = glichHackButton.MaxTimer;
                    glichHackButton.isEffectActive = false;
                    glichHackButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Glitch.sampledPlayer = null;
                },
                Glitch.getHackButtonSprite(), new Vector3(0f, 0f, 0f), __instance, KeyCode.G,
                true, Glitch.hackDuration, () => { glichHackButton.Timer = glichHackButton.MaxTimer; }, true
            );

            // Venerer Ability
            venererAbilityButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VenererCamo, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.venererCamo();

                    Helpers.checkWatchFlash(Venerer.venerer);
                },
                () => { return Venerer.venerer != null && Venerer.venerer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Venerer.numberOfKills == 1) venererAbilityButton.Sprite = Venerer.getcamoButton();
                    if (Venerer.numberOfKills == 2) venererAbilityButton.Sprite = Venerer.getspeedButton();
                    if (Venerer.numberOfKills >= 3) venererAbilityButton.Sprite = Venerer.getfreezeButton();
                    return Venerer.numberOfKills > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    venererAbilityButton.Timer = venererAbilityButton.MaxTimer;
                    venererAbilityButton.isEffectActive = false;
                    venererAbilityButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Venerer.getnoAbilitiesButton(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Venerer.duration, () => { venererAbilityButton.Timer = venererAbilityButton.MaxTimer; }
            );

            // Disperser disperse
            disperserDisperseButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.Disperse, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.disperse();

                    Helpers.checkWatchFlash(Disperser.disperser);
                },
                () => { return Disperser.disperser != null && Disperser.disperser == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Disperser.isButtonUsed && PlayerControl.LocalPlayer.CanMove; },
                () => {},
                Disperser.getButtonSprite(), new Vector3(0, 1f, 0), __instance, null, true
            );

            // Bomber Bomb
            bomberButton = new CustomButton(
                () => {
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    pos.z += 0.001f;
                    Bomber.DetonatePoint = pos;
                    Bomber.Bomb = BombExtensions.CreateBomb(pos);

                    Helpers.checkWatchFlash(Bomber.bomber);
                    bomberButton.Sprite = Bomber.getDetonateButtonSprite();
                },
                () => { return Bomber.bomber != null && Bomber.bomber == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    Bomber.Bomb.ClearBomb();
                    bomberButton.Sprite = Bomber.getPlantButtonSprite();
                    bomberButton.Timer = bomberButton.MaxTimer;
                    bomberButton.isEffectActive = false;
                    bomberButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Bomber.getPlantButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Bomber.delay, () =>
                {
                    bomberButton.Sprite = Bomber.getPlantButtonSprite();

                    var playersToDie = Helpers.GetClosestPlayers(Bomber.DetonatePoint, Bomber.radius, false);
                    playersToDie = Helpers.Shuffle(playersToDie);
                    while (playersToDie.Count > Bomber.maxKills) playersToDie.Remove(playersToDie[playersToDie.Count - 1]);
                    foreach (PlayerControl player in playersToDie)
                    {
                        Helpers.checkMurderAttemptAndKill(Bomber.bomber, player, showAnimation: false, ignoreIfKillerIsDead: true);
                    }

                    bomberButton.Timer = bomberButton.MaxTimer;
                    PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    Bomber.Bomb.ClearBomb();
                }
            );

            // Vampire Hunter Stake
            vampireHunterStakeButton = new CustomButton(
                () => {
                    if (Helpers.checkAndDoVetKill(VampireHunter.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(VampireHunter.currentTarget)) return;

                    if (VampireHunter.currentTarget != null) {
                        if (VampireHunter.currentTarget == Dracula.dracula || VampireHunter.currentTarget == Vampire.vampire) {
                            MurderAttemptResult murder = Helpers.checkMuderAttempt(VampireHunter.vampireHunter, VampireHunter.currentTarget);
                            if (murder == MurderAttemptResult.SuppressKill) return;

                            if (murder == MurderAttemptResult.BlankKill || murder == MurderAttemptResult.PerformKill) {
                                if (murder == MurderAttemptResult.PerformKill) {
                                    Helpers.MurderPlayer(VampireHunter.vampireHunter, VampireHunter.currentTarget, true);
                                }
                                vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer;
                            }
                        } else {
                            VampireHunter.usedStakes++;
                        }
                    }
                },
                () => { return VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (vampireHunterStakeButtonText != null) vampireHunterStakeButtonText.text = $"{VampireHunter.maxFailedStakes - VampireHunter.usedStakes}";
                    return VampireHunter.currentTarget && VampireHunter.canStake && VampireHunter.usedStakes < VampireHunter.maxFailedStakes && PlayerControl.LocalPlayer.CanMove;
                },
                () => { vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer; },
                VampireHunter.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
            // Vampire Hunter Stake button counter
            vampireHunterStakeButtonText = GameObject.Instantiate(vampireHunterStakeButton.actionButton.cooldownTimerText, vampireHunterStakeButton.actionButton.cooldownTimerText.transform.parent);
            vampireHunterStakeButtonText.text = "";
            vampireHunterStakeButtonText.enableWordWrapping = false;
            vampireHunterStakeButtonText.transform.localScale = Vector3.one * 0.5f;
            vampireHunterStakeButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // VH Veteran alert
            vhVeteranAlertButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VHVeteranAlert, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.vhVeteranAlert();
                    Helpers.checkWatchFlash(VampireHunter.veteran);
                },
                () => { return VampireHunter.veteran != null && VampireHunter.veteran == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (vhVeteranAlertButtonText != null) vhVeteranAlertButtonText.text = $"{VampireHunter.remainingAlerts}";
                    return VampireHunter.remainingAlerts > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    vhVeteranAlertButton.Timer = vhVeteranAlertButton.MaxTimer;
                    vhVeteranAlertButton.isEffectActive = false;
                    vhVeteranAlertButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Veteran.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, VampireHunter.duration, () => { vhVeteranAlertButton.Timer = vhVeteranAlertButton.MaxTimer; }
            );
            // VH Veteran alert button counter
            vhVeteranAlertButtonText = GameObject.Instantiate(vhVeteranAlertButton.actionButton.cooldownTimerText, vhVeteranAlertButton.actionButton.cooldownTimerText.transform.parent);
            vhVeteranAlertButtonText.text = "";
            vhVeteranAlertButtonText.enableWordWrapping = false;
            vhVeteranAlertButtonText.transform.localScale = Vector3.one * 0.5f;
            vhVeteranAlertButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Time Lord Rewind
            timeLordRewindTimeButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeLordRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeLordRewindTime();
                },
                () => { return TimeLord.timeLord != null && TimeLord.timeLord == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    timeLordRewindTimeButton.Timer = timeLordRewindTimeButton.MaxTimer;
                    timeLordRewindTimeButton.isEffectActive = false;
                    timeLordRewindTimeButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeLord.getRewindButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.G,
                true, TimeLord.rewindTime, () => { timeLordRewindTimeButton.Timer = timeLordRewindTimeButton.MaxTimer; }
            );

            // Time Lord Shield
            timeLordShieldButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeLordShield, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeLordShield();
                },
                () => { return TimeLord.timeLord != null && TimeLord.timeLord == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    timeLordShieldButton.Timer = timeLordShieldButton.MaxTimer;
                    timeLordShieldButton.isEffectActive = false;
                    timeLordShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeLord.getTimeShieldButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, TimeLord.shieldDuration, () => { timeLordShieldButton.Timer = timeLordShieldButton.MaxTimer; }
            );

            // Oracle confess
            oracleConfessButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkAndDoVetKill(Oracle.currentTarget)) return;
                    if (Helpers.checkAndDoVHVetKill(Oracle.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OracleConfess, Hazel.SendOption.Reliable, -1);
                    writer.Write(Oracle.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.oracleConfess(Oracle.currentTarget.PlayerId);

                    Oracle.investigated = true;
                    Oracle.currentTarget = null;
                    oracleConfessButton.Timer = oracleConfessButton.MaxTimer;

                    Helpers.checkWatchFlash(Oracle.currentTarget);
                },
                () => { return Oracle.oracle != null && Oracle.oracle == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Oracle.currentTarget && !Oracle.investigated && PlayerControl.LocalPlayer.CanMove; },
                () => { oracleConfessButton.Timer = oracleConfessButton.MaxTimer; },
                Oracle.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, 0f, () => { oracleConfessButton.Timer = oracleConfessButton.MaxTimer; }
            );

            // Medusa Petrify
            medusaPetrifyButton = new CustomButton(
                () =>
                {
                    if (Medusa.currentTarget != null)
                    {
                        Medusa.petrified = Medusa.currentTarget;
                        medusaPetrifyButton.HasEffect = true;
                    }
                },
                () => { return Medusa.medusa != null && Medusa.medusa == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && Medusa.currentTarget; },
                () =>
                {
                    Medusa.petrified = null;
                    medusaPetrifyButton.isEffectActive = false;
                    medusaPetrifyButton.Timer = medusaPetrifyButton.MaxTimer;
                    medusaPetrifyButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Medusa.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Medusa.duration, () =>
                {
                    if (Medusa.petrified != null && !Medusa.petrified.Data.IsDead) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedusaPetrify, Hazel.SendOption.Reliable, -1);
                        writer.Write(Medusa.petrified.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.medusaPetrify(Medusa.petrified.PlayerId);
                        Medusa.petrified = null;
                    }
                    medusaPetrifyButton.Timer = medusaPetrifyButton.MaxTimer;
                }
            );

            // Archer ShowWeapong
            archerShowWeaponButton = new CustomButton(
                () => {

                    Archer.weaponDuration = 0;
                    archerShowWeaponButton.Timer = 2f;
                    Archer.weaponEquiped = !Archer.weaponEquiped;
                    if (Archer.bow != null) {
                        Archer.bow.gameObject.SetActive(Archer.weaponEquiped);
                    }

                },
                () => { return Archer.archer != null && Archer.archer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Archer.bow != null) {
                        var targetPosition = Archer.archer.transform.position + new Vector3(0.8f * (float)Math.Cos(Archer.mouseArcherAngle), 0.8f * (float)Math.Sin(Archer.mouseArcherAngle));
                        Archer.bow.transform.position += (targetPosition - Archer.bow.transform.position) * 0.4f;
                        Archer.bow.GetComponent<SpriteRenderer>().transform.eulerAngles = new Vector3(0f, 0f, (float)(Archer.mouseArcherAngle * 360f / Math.PI / 2f));
                        if (Math.Cos(Archer.mouseArcherAngle) < 0.0) {
                            if (Archer.bow.GetComponent<SpriteRenderer>().transform.localScale.y > 0)
                                Archer.bow.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(1f, -1f);
                        }
                        else {
                            if (Archer.bow.GetComponent<SpriteRenderer>().transform.localScale.y < 0)
                                Archer.bow.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(1f, 1f);
                        }

                        if (Archer.weaponEquiped) {
                            archerShowWeaponButton.actionButton.graphic.sprite = Archer.getHideBowButtonSprite();
                            if (Archer.archer.inVent)
                                Archer.bow.active = false;
                            else
                                Archer.bow.active = true;
                        }
                        else {
                            archerShowWeaponButton.actionButton.graphic.sprite = Archer.getPickBowButtonSprite();
                        }

                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    Archer.weaponDuration = 0;
                    Archer.weaponEquiped = false;
                    if (Archer.bow != null) {
                        Archer.bow.gameObject.SetActive(Archer.weaponEquiped);
                    }
                    archerShowWeaponButton.Timer = archerShowWeaponButton.MaxTimer;
                },
                Archer.getPickBowButtonSprite(),
                new Vector3(-3f, -0.06f, 0),
                __instance,
                KeyCode.F
            );

            // Archer Kill
            archerKillButton = new CustomButton(
                () => {
                    PlayerControl target = Archer.GetShootPlayer(Archer.shotSize * 0.2f, Archer.shotRange);

                    if (target != null) {
                        MurderAttemptResult murderAttemptResult = Helpers.checkMuderAttempt(Archer.archer, target);
                        if (murderAttemptResult == MurderAttemptResult.BlankKill)
                        {
                            archerKillButton.Timer = archerKillButton.MaxTimer;
                            target = null;
                            return;
                        }
                        else if (murderAttemptResult == MurderAttemptResult.SuppressKill)
                        {
                            return;
                        }
                        else if (murderAttemptResult == MurderAttemptResult.ReverseKill)
                        {
                            Helpers.checkMurderAttemptAndKill(target, Archer.archer);
                            return;
                        }

                        if (murderAttemptResult == MurderAttemptResult.PerformKill)
                        {

                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                            killWriter.Write(Archer.archer.Data.PlayerId);
                            killWriter.Write(target.PlayerId);
                            killWriter.Write(0);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            RPCProcedure.uncheckedMurderPlayer(Archer.archer.Data.PlayerId, target.PlayerId, 0);
                        }
                    }
                    else {
                        target = PlayerControl.LocalPlayer;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShowArcherNotification, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.showArcherNotification(target.PlayerId);

                    Archer.weaponDuration = 0;
                    Archer.weaponEquiped = false;
                    Archer.bow.gameObject.SetActive(false);
                    archerKillButton.Timer = archerKillButton.MaxTimer;
                    archerShowWeaponButton.Timer = archerShowWeaponButton.MaxTimer;
                    target = null;
                },
                () => { return Archer.archer != null && Archer.archer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Archer.weaponEquiped) {

                        if (Archer.bow == null) {
                            Archer.bow = new GameObject("Weapon");
                            var renderer = Archer.bow.AddComponent<SpriteRenderer>();

                            renderer.sprite = Archer.getBowSprite();
                            renderer.transform.parent = Archer.archer.transform;
                            renderer.color = new Color(1, 1, 1, 1);
                            renderer.transform.position = new Vector3(0, 0, -30f);
                        }

                        if (Archer.Guides.Count == 0) {
                            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

                                var obj = new GameObject("WeaponGuide");
                                var renderer = obj.AddComponent<SpriteRenderer>();

                                renderer.sprite = Archer.getGuideSprite();
                                renderer.transform.parent = HudManager.Instance.transform;
                                renderer.color = new Color(0, 0, 0, 0);
                                renderer.transform.position = new Vector3(0, 0, -30f);
                                Archer.Guides[player.PlayerId] = renderer;

                            }
                        }

                        Archer.weaponDuration += Time.deltaTime;

                        float r = 0f, g = 0f, b = 0f, a = 0f;

                        if (Archer.weaponDuration > Archer.AimAssistDelay) {
                            float value = 1f - (Archer.weaponDuration - Archer.AimAssistDelay);
                            if (value > 0) r = g = b = a = value;
                            r = 0.2f + 0.8f * r;
                            g = 0.4f + 0.6f * g;
                            g = 0.8f + 0.2f * b;
                            a = 0.6f + 0.4f * a;

                            if (Archer.weaponDuration > Archer.AimAssistDelay + Archer.AimAssistDuration) {
                                value = Archer.weaponDuration - Archer.AimAssistDelay - Archer.AimAssistDuration;
                                if (value < 0) value = 0f;
                                a *= 1f - value;
                            }
                        }

                        Color color = new Color(r, g, b, a);

                        foreach (var guide in Archer.Guides) {
                            guide.Value.color = Color.clear;
                        }

                        foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                            if (player.Data.IsDead) continue;
                            if (!Archer.Guides.ContainsKey(player.PlayerId)) continue;

                            if (GameOptionsManager.Instance.currentGameOptions.MapId == 5) {
                                if ((Archer.archer.transform.position.y > 0 && player.transform.position.y > 0) || (Archer.archer.transform.position.y < 0 && player.transform.position.y < 0)) {
                                    Archer.Guides[player.PlayerId].color = color;
                                    Vector3 dir = player.transform.position - PlayerControl.LocalPlayer.transform.position;
                                    float angle = Mathf.Atan2(dir.y, dir.x);

                                    float oldAng = Archer.Guides[player.PlayerId].transform.eulerAngles.z * (float)Math.PI / 180f;
                                    Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                                    Vector2 oldPos = new Vector2(Mathf.Cos(oldAng), Mathf.Sin(oldAng));
                                    newPos = oldPos + (newPos - oldPos) * 0.15f;

                                    angle = Mathf.Atan2(newPos.y, newPos.x);
                                    Archer.Guides[player.PlayerId].transform.eulerAngles = new Vector3(0, 0, angle * 180f / (float)Math.PI);
                                    Archer.Guides[player.PlayerId].transform.localPosition = new Vector3(Mathf.Cos(angle) * 2f, Mathf.Sin(angle) * 2f, -30f);
                                }
                            }
                            else {
                                Archer.Guides[player.PlayerId].color = color;
                                Vector3 dir = player.transform.position - PlayerControl.LocalPlayer.transform.position;
                                float angle = Mathf.Atan2(dir.y, dir.x);

                                float oldAng = Archer.Guides[player.PlayerId].transform.eulerAngles.z * (float)Math.PI / 180f;
                                Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                                Vector2 oldPos = new Vector2(Mathf.Cos(oldAng), Mathf.Sin(oldAng));
                                newPos = oldPos + (newPos - oldPos) * 0.15f;

                                angle = Mathf.Atan2(newPos.y, newPos.x);
                                Archer.Guides[player.PlayerId].transform.eulerAngles = new Vector3(0, 0, angle * 180f / (float)Math.PI);
                                Archer.Guides[player.PlayerId].transform.localPosition = new Vector3(Mathf.Cos(angle) * 2f, Mathf.Sin(angle) * 2f, -30f);
                            }
                        }

                        foreach (var deadBody in UnityEngine.Object.FindObjectsOfType<DeadBody>()) {
                            if (GameOptionsManager.Instance.currentGameOptions.MapId == 5) {
                                if ((Archer.archer.transform.position.y > 0 && deadBody.transform.position.y > 0) || (Archer.archer.transform.position.y < 0 && deadBody.transform.position.y < 0)) {
                                    Archer.Guides[deadBody.ParentId].color = color;
                                    Vector3 dir = deadBody.transform.position - PlayerControl.LocalPlayer.transform.position;
                                    float angle = Mathf.Atan2(dir.y, dir.x);

                                    float oldAng = Archer.Guides[deadBody.ParentId].transform.eulerAngles.z * (float)Math.PI / 180f;
                                    Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                                    Vector2 oldPos = new Vector2(Mathf.Cos(oldAng), Mathf.Sin(oldAng));
                                    newPos = oldPos + (newPos - oldPos) * 0.15f;

                                    angle = Mathf.Atan2(newPos.y, newPos.x);
                                    Archer.Guides[deadBody.ParentId].transform.eulerAngles = new Vector3(0, 0, angle * 180f / (float)Math.PI);
                                    Archer.Guides[deadBody.ParentId].transform.localPosition = new Vector3(Mathf.Cos(angle) * 2f, Mathf.Sin(angle) * 2f, -30f);
                                }
                            }
                            else {
                                Archer.Guides[deadBody.ParentId].color = color;
                                Vector3 dir = deadBody.transform.position - PlayerControl.LocalPlayer.transform.position;
                                float angle = Mathf.Atan2(dir.y, dir.x);

                                float oldAng = Archer.Guides[deadBody.ParentId].transform.eulerAngles.z * (float)Math.PI / 180f;
                                Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                                Vector2 oldPos = new Vector2(Mathf.Cos(oldAng), Mathf.Sin(oldAng));
                                newPos = oldPos + (newPos - oldPos) * 0.15f;

                                angle = Mathf.Atan2(newPos.y, newPos.x);
                                Archer.Guides[deadBody.ParentId].transform.eulerAngles = new Vector3(0, 0, angle * 180f / (float)Math.PI);
                                Archer.Guides[deadBody.ParentId].transform.localPosition = new Vector3(Mathf.Cos(angle) * 2f, Mathf.Sin(angle) * 2f, -30f);
                            }
                        }

                        Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
                        Archer.mouseArcherAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);

                    }
                    else {
                        foreach (var guide in Archer.Guides) {
                            guide.Value.color *= 0.7f;
                        }
                    }
                    return Archer.weaponEquiped && PlayerControl.LocalPlayer.CanMove;
                },
                () => { archerKillButton.Timer = archerKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                new Vector3(-1f, 1f, 0),
                __instance,
                KeyCode.Mouse1
            );

            // Zoom Button
            zoomOutButton = new CustomButton(
                () => { Helpers.toggleZoom(); },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead; },
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