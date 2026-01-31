using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Networking.Extensions;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static bool initialized = false;
        public static CustomButton zoomOutButton;
        public static PoolablePlayer targetDisplay;

        public static CustomButton sheriffShootButton;
        public static CustomButton survivorSafeguardButton;
        public static TMP_Text survivorSafeguardButtonText;
        public static CustomButton juggernautKillButton;
        public static CustomButton seerRevealButton;
        public static CustomButton engineerRepairButton;
        public static TMP_Text engineerRepairButtonText;
        public static CustomButton veteranAlertButton;
        public static TMP_Text veteranAlertButtonText;
        public static CustomButton camouflagerButton;
        public static CustomButton morphlingButton;
        public static CustomButton pursuerButton;
        public static TMP_Text pursuerButtonBlanksText;
        public static CustomButton amnesiacRememberButton;
        public static CustomButton thiefKillButton;
        public static CustomButton medicShieldButton;
        public static CustomButton spyVitalsButton;
        public static TMP_Text spyVitalsChargesText;
        public static CustomButton spyAdminTableButton;
        public static TMP_Text spyAdminTableChargesText;
        public static CustomButton trackerTrackPlayerButton;
        public static TMP_Text trackerTrackPlayerButtonText;
        public static CustomButton trapperButton;
        public static TMP_Text trapperChargesText;
        public static CustomButton detectiveInspectButton;
        public static CustomButton detectiveExamineButton;
        public static CustomButton guardianAngelProtectButton;
        public static TMP_Text guardianAngelProtectButtonText;
        public static CustomButton swooperSwoopButton;
        public static CustomButton arsonistDouseButton;
        public static CustomButton arsonistIgniteButton;
        public static CustomButton werewolfRampageButton;
        public static CustomButton werewolfKillButton;
        public static CustomButton placeVentButton;
        public static CustomButton janitorCleanButton;
        public static CustomButton undertakerDragButton;
        public static CustomButton grenadierFlashButton;
        public static CustomButton blackmailerBlackmailButton;
        public static TMP_Text blackmailerBlackmailButtonText;
        public static CustomButton politicianCampaignButton;
        public static CustomButton mayorBodyguardButton;
        public static TMP_Text mayorBodyguardButtonText;
        public static CustomButton draculaButton;
        public static CustomButton vampireButton;
        public static CustomButton poisonButton;
        public static CustomButton poisonBlindTrapButton;
        public static TMP_Text poisonBlindTrapButtonText;
        public static CustomButton venererButton;
        public static CustomButton plaguebearerInfectButton;
        public static CustomButton pestilenceKillButton;
        public static CustomButton doomsayerButton;
        public static CustomButton glitchKillButton;
        public static CustomButton glitchMimicButton;
        public static CustomButton glitchHackButton;
        public static CustomButton buttonBarryButton;
        public static CustomButton sateliteButton;
        public static CustomButton cannibalEatButton;
        public static CustomButton escapistButton;
        public static CustomButton vampireHunterStakeButton;
        public static TMP_Text vampireHunterStakeButtonText;
        public static CustomButton oracleButton;
        public static CustomButton lookoutZoomButton;
        public static TMP_Text lookoutZoomButtonText;
        public static CustomButton plumberSealButton;
        public static TMP_Text plumberSealButtonText;
        public static CustomButton plumberFlushButton;
        public static TMP_Text plumberFlushButtonText;
        public static CustomButton disperserButton;
        public static TMP_Text disperserButtonText;
        public static CustomButton bansheeScareButton;
        public static CustomButton poltergeistButton;
        public static CustomButton deceiverPlaceDecoyButton;
        public static CustomButton deceiverDestroyDecoyButton;
        public static CustomButton deceiverSwapDecoyButton;

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
            sheriffShootButton.MaxTimer = Sheriff.cooldown;
            survivorSafeguardButton.MaxTimer = Survivor.cooldown;
            survivorSafeguardButton.EffectDuration = Survivor.duration;
            juggernautKillButton.MaxTimer = Juggernaut.cooldown;
            seerRevealButton.MaxTimer = Seer.cooldown;
            engineerRepairButton.MaxTimer = 0f;
            veteranAlertButton.MaxTimer = Veteran.cooldown;
            veteranAlertButton.EffectDuration = Veteran.duration;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            camouflagerButton.EffectDuration = Camouflager.duration;
            morphlingButton.MaxTimer = Morphling.cooldown;
            morphlingButton.EffectDuration = Morphling.duration;
            pursuerButton.MaxTimer = Pursuer.cooldown;
            amnesiacRememberButton.MaxTimer = 0f;
            thiefKillButton.MaxTimer = Thief.cooldown;
            medicShieldButton.MaxTimer = 0f;
            spyVitalsButton.MaxTimer = Spy.cooldown;
            spyVitalsButton.EffectDuration = Spy.duration;
            spyAdminTableButton.MaxTimer = Spy.cooldown;
            spyAdminTableButton.EffectDuration = Spy.duration;
            trackerTrackPlayerButton.MaxTimer = Tracker.cooldown;
            trapperButton.MaxTimer = Trapper.cooldown;
            detectiveInspectButton.MaxTimer = 3f;
            detectiveExamineButton.MaxTimer = Detective.cooldown;
            guardianAngelProtectButton.MaxTimer = GuardianAngel.cooldown;
            guardianAngelProtectButton.EffectDuration = GuardianAngel.duration;
            swooperSwoopButton.MaxTimer = Swooper.cooldown;
            swooperSwoopButton.EffectDuration = Swooper.duration;
            arsonistDouseButton.MaxTimer = Arsonist.douseCooldown;
            arsonistIgniteButton.MaxTimer = Arsonist.igniteCooldown;
            werewolfRampageButton.MaxTimer = Werewolf.cooldown;
            werewolfRampageButton.EffectDuration = Werewolf.duration;
            werewolfKillButton.MaxTimer = Werewolf.killCooldown;
            placeVentButton.MaxTimer = Miner.placeVentCooldown;
            janitorCleanButton.MaxTimer = Janitor.cooldown;
            undertakerDragButton.MaxTimer = Undertaker.cooldown;
            grenadierFlashButton.MaxTimer = Grenadier.cooldown;
            grenadierFlashButton.EffectDuration = Grenadier.duration;
            blackmailerBlackmailButton.MaxTimer = Blackmailer.cooldown;
            politicianCampaignButton.MaxTimer = Politician.cooldown;
            mayorBodyguardButton.MaxTimer = Mayor.cooldown;
            mayorBodyguardButton.EffectDuration = Mayor.duration;
            draculaButton.MaxTimer = Dracula.cooldown;
            vampireButton.MaxTimer = Vampire.cooldown;
            poisonButton.MaxTimer = Poisoner.cooldown;
            poisonButton.EffectDuration = Poisoner.delay;
            poisonBlindTrapButton.MaxTimer = Poisoner.trapCooldown;
            venererButton.MaxTimer = Venerer.cooldown;
            venererButton.EffectDuration = Venerer.duration;
            plaguebearerInfectButton.MaxTimer = Plaguebearer.cooldown;
            pestilenceKillButton.MaxTimer = Pestilence.cooldown;
            doomsayerButton.MaxTimer = Doomsayer.cooldown;
            glitchKillButton.MaxTimer = Glitch.killCooldown;
            glitchMimicButton.MaxTimer = Glitch.morphCooldown;
            glitchMimicButton.EffectDuration = Glitch.morphDuration;
            glitchHackButton.MaxTimer = Glitch.hackCooldown;
            glitchHackButton.EffectDuration = Glitch.hackDuration;
            buttonBarryButton.MaxTimer = 0f;
            sateliteButton.MaxTimer = Satelite.corpsesTrackingCooldown;
            sateliteButton.EffectDuration = Satelite.corpsesTrackingDuration;
            cannibalEatButton.MaxTimer = Cannibal.cooldown;
            escapistButton.MaxTimer = Escapist.cooldown;
            vampireHunterStakeButton.MaxTimer = VampireHunter.cooldown;
            oracleButton.MaxTimer = Oracle.cooldown;
            lookoutZoomButton.MaxTimer = Lookout.cooldown;
            lookoutZoomButton.EffectDuration = Lookout.duration;
            plumberSealButton.MaxTimer = Plumber.sealVentCooldown;
            plumberFlushButton.MaxTimer = Plumber.flushCooldown;
            disperserButton.MaxTimer = 10f;
            bansheeScareButton.MaxTimer = Banshee.cooldown;
            bansheeScareButton.EffectDuration = Banshee.duration;
            poltergeistButton.MaxTimer = Poltergeist.cooldown;
            deceiverPlaceDecoyButton.MaxTimer = Deceiver.placeCooldown;
            deceiverDestroyDecoyButton.MaxTimer = 3f;
            deceiverSwapDecoyButton.MaxTimer = Deceiver.swapCooldown;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            zoomOutButton.MaxTimer = 0f;
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

        public static void Postfix(HudManager __instance)
        {
            initialized = false;

            try
            {
                createButtonsPostfix(__instance);
            }
            catch { }
        }

        public static void createButtonsPostfix(HudManager __instance)
        {
            // get map id, or raise error to wait...
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            // Sheriff Shoot
            sheriffShootButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsSheriff(out Sheriff sheriff);
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(sheriff.sheriff, sheriff.currentTarget);
                    sheriff.sheriff.rpcCampaignPlayer(sheriff.currentTarget);
                    sheriff.sheriff.rpcInfectPlayer(sheriff.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        sheriffShootButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        sheriffShootButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(sheriff.currentTarget, sheriff.sheriff);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(sheriff.currentTarget, sheriff.sheriff, true);
                            if (!sheriff.currentTarget.isEvil())
                            {
                                sheriff.currentTarget.addGameInfoRpc(InfoType.AddMisfire);
                                sheriff.sheriff.addGameInfoRpc(InfoType.AddMisfire);
                            }
                            else
                            {
                                sheriff.currentTarget.addGameInfoRpc(InfoType.AddKill);
                            }
                        }
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        bool isCorrectKill = sheriff.currentTarget.Data.Role.IsImpostor && (sheriff.currentTarget != Mini.mini || Mini.isGrownUp()) || sheriff.currentTarget.isNeutralBenign() && Sheriff.canKillNeutralBenign || sheriff.currentTarget.isNeutralEvil() && Sheriff.canKillNeutralEvil || sheriff.currentTarget.isNeutralKilling() && Sheriff.canKillNeutralKilling;
                        if (isCorrectKill)
                        {
                            Helpers.MurderPlayer(sheriff.sheriff, sheriff.currentTarget, true);
                            sheriff.sheriff.addGameInfoRpc(InfoType.AddCorrectShot);
                        }
                        else
                        {
                            if (Sheriff.whoDiesOnMissfire == Sheriff.MissfireDeath.Sheriff)
                            {
                                Helpers.MurderPlayer(sheriff.sheriff, sheriff.sheriff, false);
                            }
                            else if (Sheriff.whoDiesOnMissfire == Sheriff.MissfireDeath.Target)
                            {
                                Helpers.MurderPlayer(sheriff.sheriff, sheriff.currentTarget, true);
                            }
                            else if (Sheriff.whoDiesOnMissfire == Sheriff.MissfireDeath.Both)
                            {
                                Helpers.MurderPlayer(sheriff.sheriff, sheriff.currentTarget, false);
                                Helpers.MurderPlayer(sheriff.sheriff, sheriff.sheriff, false);
                            }
                            sheriff.sheriff.addGameInfoRpc(InfoType.AddMisfire);
                        }
                    }
                    sheriffShootButton.Timer = sheriffShootButton.MaxTimer;
                    sheriff.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.IsSheriff(out Sheriff sheriff) && !sheriff.sheriff.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsSheriff(out Sheriff sheriff);
                    return sheriff.currentTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { sheriffShootButton.Timer = sheriffShootButton.MaxTimer; },
                Sheriff.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Survivor safeguard
            survivorSafeguardButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsSurvivor(out Survivor survivor);
                    survivor.remainingSafeguards--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SurvivorSafeguard, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.survivorSafeguard(PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("survivorSafeguard");
                },
                () => { return PlayerControl.LocalPlayer.IsSurvivor(out Survivor survivor) && !survivor.survivor.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsSurvivor(out Survivor survivor);
                    if (survivorSafeguardButtonText != null) survivorSafeguardButtonText.text = $"{survivor.remainingSafeguards}";
                    return survivor.remainingSafeguards > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer;
                    survivorSafeguardButton.isEffectActive = false;
                    survivorSafeguardButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Survivor.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, Survivor.duration, () => { survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer; }
            );
            survivorSafeguardButtonText = GameObject.Instantiate(survivorSafeguardButton.actionButton.cooldownTimerText, survivorSafeguardButton.actionButton.cooldownTimerText.transform.parent);
            survivorSafeguardButtonText.text = "";
            survivorSafeguardButtonText.enableWordWrapping = false;
            survivorSafeguardButtonText.transform.localScale = Vector3.one * 0.5f;
            survivorSafeguardButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Juggernaut Kill
            juggernautKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Juggernaut.juggernaut, Juggernaut.currentTarget);
                    Juggernaut.juggernaut.rpcCampaignPlayer(Juggernaut.currentTarget);
                    Juggernaut.juggernaut.rpcInfectPlayer(Juggernaut.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Juggernaut.currentTarget, Juggernaut.juggernaut);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(Juggernaut.currentTarget, Juggernaut.juggernaut);
                            if (!Juggernaut.currentTarget.isEvil()) Juggernaut.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                            else Juggernaut.currentTarget.addGameInfoRpc(InfoType.AddKill);
                        }
                        return;
                    }
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        juggernautKillButton.Timer = Survivor.cooldownReset - (Juggernaut.cooldownDecrease * Juggernaut.numberOfKills);
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        juggernautKillButton.Timer = GuardianAngel.cooldownReset - (Juggernaut.cooldownDecrease * Juggernaut.numberOfKills);
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(Juggernaut.juggernaut, Juggernaut.currentTarget, true);
                        Juggernaut.numberOfKills++;
                        Juggernaut.juggernaut.addGameInfoRpc(InfoType.AddKill);
                    }
                    juggernautKillButton.Timer = juggernautKillButton.MaxTimer - (Juggernaut.cooldownDecrease * Juggernaut.numberOfKills);
                    Juggernaut.currentTarget = null;
                },
                () => { return Juggernaut.juggernaut != null && Juggernaut.juggernaut == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Juggernaut.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { juggernautKillButton.Timer = juggernautKillButton.MaxTimer - (Juggernaut.cooldownDecrease * Juggernaut.numberOfKills); },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Seer Reveal
            seerRevealButton = new CustomButton(
                () =>
                {
                    if (Seer.currentTarget != null)
                    {
                        Seer.seer.rpcCampaignPlayer(Seer.currentTarget);
                        Seer.seer.rpcInfectPlayer(Seer.currentTarget);
                        Seer.seer.showFlashForSixthSense(Seer.currentTarget);
                        if (Helpers.checkSuspendAction(Seer.seer, Seer.currentTarget)) return;
                        Seer.revealedIds.Add(Seer.currentTarget.PlayerId);
                        seerRevealButton.Timer = seerRevealButton.MaxTimer;
                        foreach (byte playerId in Seer.revealedIds)
                        {
                            if (playerIcons.ContainsKey(playerId))
                            {
                                playerIcons[playerId].setSemiTransparent(false);
                            }
                        }
                        Seer.currentTarget = null;
                        SoundEffectsManager.play("seerReveal");
                    }
                },
                () => { return Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Seer.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { seerRevealButton.Timer = seerRevealButton.MaxTimer; },
                Seer.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () =>
                {
                    Engineer.remainingFixes--;
                    engineerRepairButton.Timer = 0f;
                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedRepair();
                    SoundEffectsManager.play("engineerRepair");
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.engineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                },
                () => { return Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    if (engineerRepairButtonText != null) engineerRepairButtonText.text = $"{Engineer.remainingFixes}";
                    return sabotageActive && Engineer.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Engineer.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );
            engineerRepairButtonText = GameObject.Instantiate(engineerRepairButton.actionButton.cooldownTimerText, engineerRepairButton.actionButton.cooldownTimerText.transform.parent);
            engineerRepairButtonText.text = "";
            engineerRepairButtonText.enableWordWrapping = false;
            engineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
            engineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Veteran Alert
            veteranAlertButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsVeteran(out Veteran veteran);
                    veteran.remainingAlerts--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VeteranAlert, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.veteranAlert(PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("veteranAlert");
                },
                () => { return PlayerControl.LocalPlayer.IsVeteran(out Veteran veteran) && !veteran.veteran.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsVeteran(out Veteran veteran);
                    if (veteranAlertButtonText != null) veteranAlertButtonText.text = $"{veteran.remainingAlerts}";
                    return veteran.remainingAlerts > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    veteranAlertButton.Timer = veteranAlertButton.MaxTimer;
                    veteranAlertButton.isEffectActive = false;
                    veteranAlertButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Veteran.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, Veteran.duration, () => { veteranAlertButton.Timer = veteranAlertButton.MaxTimer; }
            );
            veteranAlertButtonText = GameObject.Instantiate(veteranAlertButton.actionButton.cooldownTimerText, veteranAlertButton.actionButton.cooldownTimerText.transform.parent);
            veteranAlertButtonText.text = "";
            veteranAlertButtonText.enableWordWrapping = false;
            veteranAlertButtonText.transform.localScale = Vector3.one * 0.5f;
            veteranAlertButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CamouflagerCamouflage, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.camouflagerCamouflage();
                    SoundEffectsManager.play("morphlingMorph");
                },
                () => { return Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Camouflager.duration, () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    SoundEffectsManager.play("morphlingMorph");
                }
            );

            // Morphling morph
            morphlingButton = new CustomButton(
                () =>
                {
                    if (Morphling.sampledPlayer == null)
                    {
                        morphlingButton.HasEffect = false;
                        morphlingButton.Timer = 3f;
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer && !player.Data.Disconnected)
                            {
                                if (!player.Data.IsDead) Morphling.morphTargets.Add(player.PlayerId);
                                else
                                {
                                    foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                                    {
                                        if (body.ParentId == player.PlayerId) Morphling.morphTargets.Add(player.PlayerId);
                                    }
                                }
                            }
                        }
                        byte[] morphtargetIDs = Morphling.morphTargets.ToArray();
                        var pk = new Morphling.PlayerMenu((x) =>
                        {
                            Morphling.morphling.rpcCampaignPlayer(x);
                            Morphling.morphling.rpcInfectPlayer(x);
                            Morphling.morphling.showFlashForSixthSense(x);
                            Morphling.sampledPlayer = x;
                            morphlingButton.Sprite = Morphling.getMorphSprite();
                            SoundEffectsManager.play("morphlingSample");

                            // Add poolable player to the button so that the target outfit is shown
                            setButtonTargetDisplay(Morphling.sampledPlayer, morphlingButton);
                        }, (y) =>
                        {
                            return morphtargetIDs.Contains(y.PlayerId);
                        });
                        Coroutines.Start(pk.Open(0f, true));
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MorphlingMorph, SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.morphlingMorph(Morphling.sampledPlayer.PlayerId);
                        SoundEffectsManager.play("morphlingMorph");
                        Morphling.sampledPlayer = null;
                        morphlingButton.HasEffect = true;
                    }
                },
                () => { return Morphling.morphling != null && Morphling.morphling == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
                () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Morphling.sampledPlayer = null;
                    setButtonTargetDisplay(null);
                },
                Morphling.getSampleSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Morphling.duration, () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    SoundEffectsManager.play("morphlingMorph");
                    setButtonTargetDisplay(null);
                }
            );

            // Pursuer blank
            pursuerButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsPursuer(out Pursuer pursuer);
                    pursuer.pursuer.rpcCampaignPlayer(pursuer.currentTarget);
                    pursuer.pursuer.rpcInfectPlayer(pursuer.currentTarget);
                    pursuer.pursuer.showFlashForSixthSense(pursuer.currentTarget);
                    if (Helpers.checkSuspendAction(pursuer.pursuer, pursuer.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, SendOption.Reliable, -1);
                    writer.Write(pursuer.currentTarget.PlayerId);
                    writer.Write(Byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setBlanked(pursuer.currentTarget.PlayerId, Byte.MaxValue);
                    pursuer.blanksNumber--;
                    SoundEffectsManager.play("pursuerBlank");
                },
                () => { return PlayerControl.LocalPlayer.IsPursuer(out Pursuer pursuer) && !pursuer.pursuer.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsPursuer(out Pursuer pursuer);
                    if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = $"{pursuer.blanksNumber}";
                    return pursuer.currentTarget != null && pursuer.blanksNumber > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.getTargetSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonBlanksText.text = "";
            pursuerButtonBlanksText.enableWordWrapping = false;
            pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Amnesiac remember
            amnesiacRememberButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsAmnesiac(out Amnesiac amnesiac);
                    if (amnesiac.localArrows != null)
                    {
                        foreach (Arrow arrow in amnesiac.localArrows)
                            if (arrow?.arrow != null)
                                UnityEngine.Object.Destroy(arrow.arrow);
                    }
                    amnesiac.localArrows = new List<Arrow>();
                    amnesiac.amnesiac.rpcCampaignPlayer(Helpers.playerById(amnesiac.currentTarget.ParentId));
                    amnesiac.amnesiac.rpcInfectPlayer(Helpers.playerById(amnesiac.currentTarget.ParentId));
                    amnesiac.amnesiac.showFlashForSixthSense(Helpers.playerById(amnesiac.currentTarget.ParentId));
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AmnesiacTakeRole, Hazel.SendOption.Reliable, -1);
                    writer.Write(amnesiac.currentTarget.ParentId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.amnesiacTakeRole(amnesiac.currentTarget.ParentId, PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("amnesiacRemember");
                },
                () => { return PlayerControl.LocalPlayer.IsAmnesiac(out Amnesiac amnesiac) && !amnesiac.amnesiac.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsAmnesiac(out Amnesiac amnesiac);
                    return amnesiac.currentTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { amnesiacRememberButton.Timer = 0f; },
                Amnesiac.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Thief Kill
            thiefKillButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.IsThief(out Thief thief1);
                    PlayerControl thief = PlayerControl.LocalPlayer;
                    PlayerControl target = thief1.currentTarget;
                    thief.rpcCampaignPlayer(target);
                    thief.rpcInfectPlayer(target);
                    thief.showFlashForSixthSense(target);
                    var result = Helpers.checkMuderAttempt(thief, target);
                    if (result == MurderAttemptResult.BlankKill)
                    {
                        thiefKillButton.Timer = thiefKillButton.MaxTimer;
                        return;
                    }
                    if (result == MurderAttemptResult.SurvivorReset)
                    {
                        thiefKillButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (result == MurderAttemptResult.GAReset)
                    {
                        thiefKillButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (result == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(target, thief);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(target, thief);
                            if (!target.isEvil()) target.addGameInfoRpc(InfoType.AddCorrectShot);
                            else target.addGameInfoRpc(InfoType.AddKill);
                        }
                        return;
                    }

                    if (thief1.suicideFlag)
                    {
                        // Suicide
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(0);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, thief.PlayerId, 0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        thief.addGameInfoRpc(InfoType.AddMisfire);
                        thief.clearAllTasks();
                    }

                    // Steal role if survived.
                    if (!thief.Data.IsDead && result == MurderAttemptResult.PerformKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ThiefStealsRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        writer.Write(thief.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.thiefStealsRole(target.PlayerId, thief.PlayerId);
                    }

                    // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                    if (result == MurderAttemptResult.PerformKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(thief.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, target.PlayerId, byte.MaxValue);
                        thief.addGameInfoRpc(InfoType.AddKill);
                    }
                },
                () => { return PlayerControl.LocalPlayer.IsThief(out Thief thief) && !thief.thief.Data.IsDead; },
                () =>
                {
                    PlayerControl.LocalPlayer.IsThief(out Thief thief);
                    return thief.currentTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () =>
                {
                    Medic.medic.rpcCampaignPlayer(Medic.currentTarget);
                    Medic.medic.rpcInfectPlayer(Medic.currentTarget);
                    Medic.medic.showFlashForSixthSense(Medic.currentTarget);
                    if (Helpers.checkSuspendAction(Medic.medic, Medic.currentTarget)) return;
                    medicShieldButton.Timer = 0f;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, Medic.setShieldAfterMeeting ? (byte)CustomRPC.SetFutureShielded : (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (Medic.setShieldAfterMeeting)
                        RPCProcedure.setFutureShielded(Medic.currentTarget.PlayerId);
                    else
                        RPCProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                    Medic.meetingAfterShielding = false;

                    SoundEffectsManager.play("medicShield");
                },
                () => { return Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { medicShieldButton.Timer = 0f; },
                Medic.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Spy Admin Table
            spyAdminTableButton = new CustomButton(
                () =>
                {
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                    {
                        HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                        __instance.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                    }
                    if (Spy.cantMove) PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                    Spy.chargesAdminTable--;
                },
                () => { return Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (spyAdminTableChargesText != null) spyAdminTableChargesText.text = $"{Spy.chargesAdminTable} / {Spy.toolsNumber}";
                    return Spy.chargesAdminTable > 0;
                },
                () =>
                {
                    spyAdminTableButton.Timer = spyAdminTableButton.MaxTimer;
                    spyAdminTableButton.isEffectActive = false;
                    spyAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Spy.getAdminSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, 0f, () =>
                {
                    spyAdminTableButton.Timer = spyAdminTableButton.MaxTimer;
                    if (!spyVitalsButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                    if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
                },
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3, "ADMIN"
            );
            spyAdminTableChargesText = GameObject.Instantiate(spyAdminTableButton.actionButton.cooldownTimerText, spyAdminTableButton.actionButton.cooldownTimerText.transform.parent);
            spyAdminTableChargesText.text = "";
            spyAdminTableChargesText.enableWordWrapping = false;
            spyAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
            spyAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Spy Vitals
            spyVitalsButton = new CustomButton(
                () =>
                {
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
                    {
                        if (Spy.vitals == null)
                        {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals") || x.gameObject.name.Contains("Vitals"));
                            if (e == null || Camera.main == null) return;
                            Spy.vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        Spy.vitals.transform.SetParent(Camera.main.transform, false);
                        Spy.vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        Spy.vitals.Begin(null);
                    }
                    else
                    {
                        if (Spy.doorLog == null)
                        {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            Spy.doorLog = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        Spy.doorLog.transform.SetParent(Camera.main.transform, false);
                        Spy.doorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        Spy.doorLog.Begin(null);
                    }

                    if (Spy.cantMove) PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                    Spy.chargesVitals--;
                },
                () => { return Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && GameOptionsManager.Instance.currentGameOptions.MapId != 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3; },
                () =>
                {
                    if (spyVitalsChargesText != null) spyVitalsChargesText.text = $"{Spy.chargesVitals} / {Spy.toolsNumber}";
                    spyVitalsButton.actionButton.graphic.sprite = Helpers.isMira() ? Spy.getLogSprite() : Spy.getVitalsSprite();
                    spyVitalsButton.actionButton.OverrideText(Helpers.isMira() ? "DOORLOG" : "VITALS");
                    return Spy.chargesVitals > 0;
                },
                () =>
                {
                    spyVitalsButton.Timer = spyVitalsButton.MaxTimer;
                    spyVitalsButton.isEffectActive = false;
                    spyVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Spy.getVitalsSprite(), CustomButton.ButtonPositions.lowerRowCenter, __instance, KeyCode.G,
                true, 0f, () =>
                {
                    spyVitalsButton.Timer = spyVitalsButton.MaxTimer;
                    if (!spyAdminTableButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                    if (Minigame.Instance)
                    {
                        if (Helpers.isMira()) Spy.doorLog.ForceClose();
                        else Spy.vitals.ForceClose();
                    }
                },
                false, Helpers.isMira() ? "DOORLOG" : "VITALS"
            );
            spyVitalsChargesText = GameObject.Instantiate(spyVitalsButton.actionButton.cooldownTimerText, spyVitalsButton.actionButton.cooldownTimerText.transform.parent);
            spyVitalsChargesText.text = "";
            spyVitalsChargesText.enableWordWrapping = false;
            spyVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
            spyVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Tracker button
            trackerTrackPlayerButton = new CustomButton(
                () =>
                {
                    if (Tracker.currentTarget != null)
                    {
                        Tracker.tracker.rpcCampaignPlayer(Tracker.currentTarget);
                        Tracker.tracker.rpcInfectPlayer(Tracker.currentTarget);
                        Tracker.tracker.showFlashForSixthSense(Tracker.currentTarget);
                        if (Helpers.checkSuspendAction(Tracker.tracker, Tracker.currentTarget)) return;
                        Tracker.trackedPlayers.Add(Tracker.currentTarget);

                        Arrow arrow = new(Tracker.currentTarget.Data.Color);
                        arrow.arrow.SetActive(false);
                        Tracker.TrackedPlayerLocalArrows.Add(Tracker.currentTarget.PlayerId, arrow);
                    }
                    Tracker.numberOfTracks--;
                    SoundEffectsManager.play("trackerTrackPlayer");
                    trackerTrackPlayerButton.Timer = trackerTrackPlayerButton.MaxTimer;
                    Tracker.currentTarget = null;
                },
                () => { return Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (trackerTrackPlayerButtonText != null) trackerTrackPlayerButtonText.text = $"{Tracker.numberOfTracks}";
                    return Tracker.currentTarget != null && Tracker.numberOfTracks > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    trackerTrackPlayerButton.Timer = trackerTrackPlayerButton.MaxTimer;
                    if (Tracker.resetTargetAfterMeeting) Tracker.resetTracked();
                },
                Tracker.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            trackerTrackPlayerButtonText = GameObject.Instantiate(trackerTrackPlayerButton.actionButton.cooldownTimerText, trackerTrackPlayerButton.actionButton.cooldownTimerText.transform.parent);
            trackerTrackPlayerButtonText.text = "";
            trackerTrackPlayerButtonText.enableWordWrapping = false;
            trackerTrackPlayerButtonText.transform.localScale = Vector3.one * 0.5f;
            trackerTrackPlayerButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Trapper button
            trapperButton = new CustomButton(
                () =>
                {
                    Trapper.maxTraps--;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    pos.y -= 0.2727f;
                    pos.z += 0.001f;
                    Trapper.traps.Add(TrapExtentions.CreateTrap(pos));
                    SoundEffectsManager.play("trapperTrap");
                    trapperButton.Timer = trapperButton.MaxTimer;
                },
                () => { return Trapper.trapper != null && Trapper.trapper == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (trapperChargesText != null) trapperChargesText.text = $"{Trapper.maxTraps}";
                    return PlayerControl.LocalPlayer.CanMove && Trapper.maxTraps > 0;
                },
                () =>
                {
                    trapperButton.Timer = trapperButton.MaxTimer;
                    Trapper.trappedPlayers = new List<RoleInfo>();
                    if (Trapper.removeOnNewRound)
                    {
                        Trapper.traps.ClearTraps();
                        Trapper.traps = new List<Trap>();
                        Trapper.maxTraps = CustomOptionHolder.trapperMaxTraps.getInt();
                    }
                },
                Trapper.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText, trapperButton.actionButton.cooldownTimerText.transform.parent);
            trapperChargesText.text = "";
            trapperChargesText.enableWordWrapping = false;
            trapperChargesText.transform.localScale = Vector3.one * 0.5f;
            trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Detective Inspect
            detectiveInspectButton = new CustomButton(
                () =>
                {
                    DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == Detective.currentDeadBodyTarget?.ParentId)?.FirstOrDefault();
                    if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                    {
                        Detective.currentDetectedKiller = deadPlayer.killerIfExisting;
                    }
                    Detective.detective.rpcCampaignPlayer(Helpers.playerById(Detective.currentDeadBodyTarget.ParentId));
                    Detective.detective.rpcInfectPlayer(Helpers.playerById(Detective.currentDeadBodyTarget.ParentId));
                    Detective.detective.showFlashForSixthSense(Helpers.playerById(Detective.currentDeadBodyTarget.ParentId));
                    detectiveInspectButton.Timer = detectiveInspectButton.MaxTimer;
                    Detective.currentDeadBodyTarget = null;
                    SoundEffectsManager.play("morphlingSample");
                },
                () => { return Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Detective.currentDeadBodyTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { detectiveInspectButton.Timer = detectiveInspectButton.MaxTimer; },
                Detective.getInspectButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Detective Examine
            detectiveExamineButton = new CustomButton(
                () =>
                {
                    Detective.detective.rpcCampaignPlayer(Detective.currentTarget);
                    Detective.detective.rpcInfectPlayer(Detective.currentTarget);
                    Detective.detective.showFlashForSixthSense(Detective.currentTarget);
                    if (Helpers.checkSuspendAction(Detective.detective, Detective.currentTarget)) return;
                    if (!PlayerControl.LocalPlayer.isFlashedByGrenadier())
                    {
                        if (Detective.currentTarget.PlayerId == Detective.currentDetectedKiller.PlayerId) Helpers.showFlash(Palette.ImpostorRed);
                        else Helpers.showFlash(Color.green);
                    }
                    detectiveExamineButton.Timer = detectiveExamineButton.MaxTimer;
                    Detective.currentTarget = null;
                    SoundEffectsManager.play("seerReveal");
                },
                () => { return Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Detective.currentTarget != null && Detective.currentDetectedKiller != null && PlayerControl.LocalPlayer.CanMove; },
                () => { detectiveExamineButton.Timer = detectiveExamineButton.MaxTimer; },
                Detective.getExamineButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Guardian Angel Protect
            guardianAngelProtectButton = new CustomButton(
                () =>
                {
                    GuardianAngel.guardianAngel.rpcCampaignPlayer(GuardianAngel.target);
                    GuardianAngel.guardianAngel.rpcInfectPlayer(GuardianAngel.target);
                    GuardianAngel.guardianAngel.showFlashForSixthSense(GuardianAngel.target);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelProtect, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelProtect();
                    SoundEffectsManager.play("gaProtect");
                },
                () => { return GuardianAngel.guardianAngel != null && GuardianAngel.guardianAngel == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (guardianAngelProtectButtonText != null) guardianAngelProtectButtonText.text = $"{GuardianAngel.numberOfProtects}";
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer;
                    guardianAngelProtectButton.isEffectActive = false;
                    guardianAngelProtectButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                GuardianAngel.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, GuardianAngel.duration, () => { guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer; }
            );
            guardianAngelProtectButtonText = GameObject.Instantiate(guardianAngelProtectButton.actionButton.cooldownTimerText, guardianAngelProtectButton.actionButton.cooldownTimerText.transform.parent);
            guardianAngelProtectButtonText.text = "";
            guardianAngelProtectButtonText.enableWordWrapping = false;
            guardianAngelProtectButtonText.transform.localScale = Vector3.one * 0.5f;
            guardianAngelProtectButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Swooper Swoop
            swooperSwoopButton = new CustomButton(
                () =>
                {
                    MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                    invisibleWriter.Write(Swooper.swooper.PlayerId);
                    invisibleWriter.Write(byte.MinValue);
                    AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                    RPCProcedure.swooperSwoop(Swooper.swooper.PlayerId, byte.MinValue);
                },
                () => { return Swooper.swooper != null && Swooper.swooper == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    swooperSwoopButton.Timer = swooperSwoopButton.MaxTimer;
                    swooperSwoopButton.isEffectActive = false;
                    swooperSwoopButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Swooper.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Swooper.duration, () =>
                {
                    swooperSwoopButton.Timer = swooperSwoopButton.MaxTimer;
                    SoundEffectsManager.play("morphlingMorph");
                }
            );

            // Arsonist Douse
            arsonistDouseButton = new CustomButton(
                () =>
                {
                    Arsonist.arsonist.rpcCampaignPlayer(Arsonist.currentDouseTarget);
                    Arsonist.arsonist.rpcInfectPlayer(Arsonist.currentDouseTarget);
                    Arsonist.arsonist.showFlashForSixthSense(Arsonist.currentDouseTarget);
                    if (Helpers.checkSuspendAction(Arsonist.arsonist, Arsonist.currentDouseTarget)) return;
                    Arsonist.dousedPlayers.Add(Arsonist.currentDouseTarget);
                    SoundEffectsManager.play("arsonistDouse");
                    arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                    if (Arsonist.triggerBothCooldowns) arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                    Arsonist.currentDouseTarget = null;
                },
                () => { return Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Arsonist.currentDouseTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer; },
                Arsonist.getDouseButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, __instance, KeyCode.F
            );

            // Arsonist Ignite
            arsonistIgniteButton = new CustomButton(
                () =>
                {
                    Arsonist.arsonist.rpcCampaignPlayer(Arsonist.currentIgniteTarget);
                    Arsonist.arsonist.rpcInfectPlayer(Arsonist.currentIgniteTarget);
                    Arsonist.arsonist.showFlashForSixthSense(Arsonist.currentIgniteTarget);
                    if (Helpers.checkSuspendAction(Arsonist.arsonist, Arsonist.currentIgniteTarget)) return;
                    foreach (PlayerControl player in Arsonist.dousedPlayers)
                    {
                        if (player == null || player.Data.IsDead || player.Data.Disconnected) continue;
                        Helpers.checkMurderAttemptAndKill(Arsonist.arsonist, player, false, false);

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                        writer.Write(player.PlayerId);
                        writer.Write((byte)DeadPlayer.CustomDeathReason.Arson);
                        writer.Write(Arsonist.arsonist.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Arson, Arsonist.arsonist);
                        Arsonist.arsonist.addGameInfoRpc(InfoType.AddKill);
                    }
                    Arsonist.dousedPlayers = new List<PlayerControl>();
                    arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                    if (Arsonist.triggerBothCooldowns) arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                    Arsonist.currentIgniteTarget = null;
                },
                () => { return Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Arsonist.currentIgniteTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer; },
                Arsonist.getIgniteButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Werewolf Rampage
            werewolfRampageButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WerewolfRampage, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.werewolfRampage();
                    SoundEffectsManager.play("werewolfRampage");
                },
                () => { return Werewolf.werewolf != null && Werewolf.werewolf == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer;
                    werewolfRampageButton.isEffectActive = false;
                    werewolfRampageButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Werewolf.getButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, __instance, KeyCode.F,
                true, Werewolf.duration, () => { werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer; }
            );

            // Werewolf Kill
            werewolfKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Werewolf.werewolf, Werewolf.currentTarget);
                    Werewolf.werewolf.rpcCampaignPlayer(Werewolf.currentTarget);
                    Werewolf.werewolf.rpcInfectPlayer(Werewolf.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Werewolf.currentTarget, Werewolf.werewolf);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(Werewolf.currentTarget, Werewolf.werewolf);
                            if (!Werewolf.currentTarget.isEvil()) Werewolf.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                            else Werewolf.currentTarget.addGameInfoRpc(InfoType.AddKill);
                        }
                        return;
                    }
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        werewolfKillButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        werewolfKillButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(Werewolf.werewolf, Werewolf.currentTarget, true);
                        Werewolf.werewolf.addGameInfoRpc(InfoType.AddKill);
                    }
                    werewolfKillButton.Timer = werewolfKillButton.MaxTimer;
                    Werewolf.currentTarget = null;
                },
                () => { return Werewolf.werewolf != null && Werewolf.werewolf == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Werewolf.currentTarget != null && Werewolf.isRampageActive && PlayerControl.LocalPlayer.CanMove; },
                () => { werewolfKillButton.Timer = werewolfKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Miner Place Vent
            placeVentButton = new CustomButton(
                () =>
                {
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceVent, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.placeVent(buff);
                    SoundEffectsManager.play("minerMine");
                    placeVentButton.Timer = placeVentButton.MaxTimer;
                },
                () => { return Miner.miner != null && Miner.miner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !MinerVent.hasMinerVentLimitReached(); },
                () => { placeVentButton.Timer = placeVentButton.MaxTimer; },
                Miner.getPlaceBoxButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () =>
                {
                    Janitor.janitor.rpcCampaignPlayer(Helpers.playerById(Janitor.currentTarget.ParentId));
                    Janitor.janitor.rpcInfectPlayer(Helpers.playerById(Janitor.currentTarget.ParentId));
                    Janitor.janitor.showFlashForSixthSense(Helpers.playerById(Janitor.currentTarget.ParentId));
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(Janitor.currentTarget.ParentId);
                    writer.Write(Janitor.janitor.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.cleanBody(Janitor.currentTarget.ParentId, Janitor.janitor.PlayerId);
                    SoundEffectsManager.play("cleanerClean");

                    janitorCleanButton.Timer = janitorCleanButton.MaxTimer;
                    Janitor.janitor.SetKillTimerUnchecked(janitorCleanButton.MaxTimer);
                    Janitor.janitor.addGameInfoRpc(InfoType.AddClean);
                    Janitor.currentTarget = null;
                },
                () => { return Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Janitor.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer; },
                Janitor.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Undertaker Drag/Drop
            undertakerDragButton = new CustomButton(
                () =>
                {
                    if (Undertaker.dragedBody == null)
                    {
                        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                        {
                            if (collider2D.tag == "DeadBody")
                            {
                                DeadBody deadBody = collider2D.GetComponent<DeadBody>();
                                if (deadBody && !deadBody.Reported)
                                {
                                    Vector2 playerPosition = PlayerControl.LocalPlayer.GetTruePosition();
                                    Vector2 deadBodyPosition = deadBody.TruePosition;
                                    if (Vector2.Distance(deadBodyPosition, playerPosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(playerPosition, deadBodyPosition, Constants.ShipAndObjectsMask, false))
                                    {
                                        NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(deadBody.ParentId);
                                        Undertaker.undertaker.rpcCampaignPlayer(Helpers.playerById(playerInfo.PlayerId));
                                        Undertaker.undertaker.rpcInfectPlayer(Helpers.playerById(playerInfo.PlayerId));
                                        Undertaker.undertaker.showFlashForSixthSense(Helpers.playerById(playerInfo.PlayerId));
                                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragBody, Hazel.SendOption.Reliable, -1);
                                        writer.Write(playerInfo.PlayerId);
                                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                                        RPCProcedure.dragBody(playerInfo.PlayerId);
                                        Undertaker.dragedBody = deadBody;
                                        undertakerDragButton.Sprite = Undertaker.getDropButtonSprite();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DropBody, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.dropBody();
                        Undertaker.dragedBody = null;
                        undertakerDragButton.Timer = undertakerDragButton.MaxTimer;
                        undertakerDragButton.Sprite = Undertaker.getDragButtonSprite();
                    }
                },
                () => { return Undertaker.undertaker != null && Undertaker.undertaker == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (Undertaker.dragedBody != null) return true;
                    else
                    {
                        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                        {
                            if (collider2D.tag == "DeadBody")
                            {
                                DeadBody deadBody = collider2D.GetComponent<DeadBody>();
                                Vector2 deadBodyPosition = deadBody.TruePosition;
                                deadBodyPosition.x -= 0.2f;
                                deadBodyPosition.y -= 0.2f;
                                return PlayerControl.LocalPlayer.CanMove && Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), deadBodyPosition) < 0.80f;
                            }
                        }
                        return false;
                    }
                },
                () =>
                {
                    undertakerDragButton.Timer = undertakerDragButton.MaxTimer;
                    undertakerDragButton.Sprite = Undertaker.getDragButtonSprite();
                },
                Undertaker.getDragButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Grenadier Flash
            grenadierFlashButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.grenadierFlash(false);
                    SoundEffectsManager.play("grenadierFlash");
                },
                () => { return Grenadier.grenadier != null && Grenadier.grenadier == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;

                    return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer;
                    grenadierFlashButton.isEffectActive = false;
                    grenadierFlashButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Grenadier.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Grenadier.duration, () => { grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer; }
            );

            // Blackmailer blackmail
            blackmailerBlackmailButton = new CustomButton(
                () =>
                {
                    Blackmailer.blackmailer.rpcCampaignPlayer(Blackmailer.currentTarget);
                    Blackmailer.blackmailer.rpcInfectPlayer(Blackmailer.currentTarget);
                    Blackmailer.blackmailer.showFlashForSixthSense(Blackmailer.currentTarget);
                    if (Helpers.checkSuspendAction(Blackmailer.blackmailer, Blackmailer.currentTarget)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BlackmailPlayer, Hazel.SendOption.Reliable, -1);
                    writer.Write(Blackmailer.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.blackmailPlayer(Blackmailer.currentTarget.PlayerId);
                    SoundEffectsManager.play("blackmailerBlackmail");
                    blackmailerBlackmailButton.Timer = blackmailerBlackmailButton.MaxTimer;
                    Blackmailer.currentTarget = null;
                },
                () => { return Blackmailer.blackmailer != null && Blackmailer.blackmailer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (blackmailerBlackmailButtonText != null) blackmailerBlackmailButtonText.text = Blackmailer.blackmailed == null ? "" : Blackmailer.blackmailed.Data.DefaultOutfit.PlayerName;
                    return Blackmailer.currentTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { blackmailerBlackmailButton.Timer = blackmailerBlackmailButton.MaxTimer; },
                Blackmailer.getBlackmailButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );
            blackmailerBlackmailButtonText = GameObject.Instantiate(blackmailerBlackmailButton.actionButton.cooldownTimerText, blackmailerBlackmailButton.actionButton.cooldownTimerText.transform.parent);
            blackmailerBlackmailButtonText.text = "";
            blackmailerBlackmailButtonText.enableWordWrapping = false;
            blackmailerBlackmailButtonText.transform.localScale = Vector3.one * 0.5f;
            blackmailerBlackmailButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Politician Campaign
            politicianCampaignButton = new CustomButton(
                () =>
                {
                    Politician.politician.rpcCampaignPlayer(Politician.currentTarget);
                    Politician.politician.rpcInfectPlayer(Politician.currentTarget);
                    Politician.politician.showFlashForSixthSense(Politician.currentTarget);
                    if (Helpers.checkSuspendAction(Politician.politician, Politician.currentTarget)) return;
                    politicianCampaignButton.Timer = politicianCampaignButton.MaxTimer;
                    Politician.currentTarget = null;
                },
                () => { return Politician.politician != null && Politician.politician == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Politician.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { politicianCampaignButton.Timer = politicianCampaignButton.MaxTimer; },
                Politician.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Mayor bodyguard
            mayorBodyguardButton = new CustomButton(
                () =>
                {
                    Mayor.numberOfGuards--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MayorBodyguard, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mayorBodyguard();
                    SoundEffectsManager.play("veteranAlert");
                },
                () => { return Mayor.mayor != null && Mayor.mayor == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (mayorBodyguardButtonText != null) mayorBodyguardButtonText.text = $"{Mayor.numberOfGuards}";
                    return Mayor.numberOfGuards > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    mayorBodyguardButton.Timer = mayorBodyguardButton.MaxTimer;
                    mayorBodyguardButton.isEffectActive = false;
                    mayorBodyguardButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Mayor.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, Mayor.duration, () => { mayorBodyguardButton.Timer = mayorBodyguardButton.MaxTimer; }
            );
            mayorBodyguardButtonText = GameObject.Instantiate(mayorBodyguardButton.actionButton.cooldownTimerText, mayorBodyguardButton.actionButton.cooldownTimerText.transform.parent);
            mayorBodyguardButtonText.text = "";
            mayorBodyguardButtonText.enableWordWrapping = false;
            mayorBodyguardButtonText.transform.localScale = Vector3.one * 0.5f;
            mayorBodyguardButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Dracula Bite
            draculaButton = new CustomButton(
                () =>
                {
                    if (Dracula.canCreateVampire && Vampire.vampire == null)
                    {
                        Dracula.dracula.rpcCampaignPlayer(Dracula.currentTarget);
                        Dracula.dracula.rpcInfectPlayer(Dracula.currentTarget);
                        Dracula.dracula.showFlashForSixthSense(Dracula.currentTarget);
                        if (Helpers.checkSuspendAction(Dracula.dracula, Dracula.currentTarget)) return;
                        if (Dracula.currentTarget == Mayor.mayor)
                        {
                            MurderAttemptResult murder = Helpers.checkMuderAttempt(Dracula.dracula, Dracula.currentTarget);
                            if (murder == MurderAttemptResult.SuppressKill) return;
                            if (murder == MurderAttemptResult.ReverseKill)
                            {
                                MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Dracula.currentTarget, Dracula.dracula);
                                if (reverseMurder == MurderAttemptResult.PerformKill)
                                {
                                    Helpers.MurderPlayer(Dracula.currentTarget, Dracula.dracula);
                                    if (!Dracula.currentTarget.isEvil()) Dracula.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                                    else Dracula.currentTarget.addGameInfoRpc(InfoType.AddKill);
                                }
                                return;
                            }
                            if (murder == MurderAttemptResult.SurvivorReset)
                            {
                                draculaButton.Timer = Survivor.cooldownReset;
                                return;
                            }
                            if (murder == MurderAttemptResult.GAReset)
                            {
                                draculaButton.Timer = GuardianAngel.cooldownReset;
                                return;
                            }
                            if (murder == MurderAttemptResult.PerformKill)
                            {
                                Helpers.MurderPlayer(Dracula.dracula, Dracula.currentTarget, true);
                                Dracula.dracula.addGameInfoRpc(InfoType.AddKill);
                            }
                            draculaButton.Timer = draculaButton.MaxTimer;
                            Dracula.currentTarget = null;
                        }
                        else
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraculaCreatesVampire, Hazel.SendOption.Reliable, -1);
                            writer.Write(Dracula.currentTarget.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.draculaCreatesVampire(Dracula.currentTarget.PlayerId);
                            SoundEffectsManager.play("draculaBite");
                        }
                    }
                    else
                    {
                        MurderAttemptResult murder = Helpers.checkMuderAttempt(Dracula.dracula, Dracula.currentTarget);
                        Dracula.dracula.rpcCampaignPlayer(Dracula.currentTarget);
                        Dracula.dracula.rpcInfectPlayer(Dracula.currentTarget);
                        Dracula.dracula.showFlashForSixthSense(Dracula.currentTarget);
                        if (murder == MurderAttemptResult.SuppressKill) return;
                        if (murder == MurderAttemptResult.ReverseKill)
                        {
                            MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Dracula.currentTarget, Dracula.dracula);
                            if (reverseMurder == MurderAttemptResult.PerformKill)
                            {
                                Helpers.MurderPlayer(Dracula.currentTarget, Dracula.dracula);
                                if (!Dracula.currentTarget.isEvil()) Dracula.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                                else Dracula.currentTarget.addGameInfoRpc(InfoType.AddKill);
                            }
                            return;
                        }
                        if (murder == MurderAttemptResult.SurvivorReset)
                        {
                            draculaButton.Timer = Survivor.cooldownReset;
                            return;
                        }
                        if (murder == MurderAttemptResult.GAReset)
                        {
                            draculaButton.Timer = GuardianAngel.cooldownReset;
                            return;
                        }
                        if (murder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(Dracula.dracula, Dracula.currentTarget, true);
                            Dracula.dracula.addGameInfoRpc(InfoType.AddKill);
                        }
                        draculaButton.Timer = draculaButton.MaxTimer;
                        Dracula.currentTarget = null;
                    }
                },
                () => { return Dracula.dracula != null && Dracula.dracula == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Dracula.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { draculaButton.Timer = draculaButton.MaxTimer; },
                Dracula.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Vampire Bite
            vampireButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Vampire.vampire, Vampire.currentTarget);
                    Vampire.vampire.rpcCampaignPlayer(Vampire.currentTarget);
                    Vampire.vampire.rpcInfectPlayer(Vampire.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Vampire.currentTarget, Vampire.vampire);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(Vampire.currentTarget, Vampire.vampire);
                            if (!Vampire.currentTarget.isEvil()) Vampire.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                            else Vampire.currentTarget.addGameInfoRpc(InfoType.AddKill);
                        }
                        return;
                    }
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        vampireButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        vampireButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(Vampire.vampire, Vampire.currentTarget, true);
                        Vampire.vampire.addGameInfoRpc(InfoType.AddKill);
                    }
                    vampireButton.Timer = vampireButton.MaxTimer;
                    Vampire.currentTarget = null;
                },
                () => { return Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { vampireButton.Timer = vampireButton.MaxTimer; },
                Dracula.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Poisoner poison
            poisonButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Poisoner.poisoner, Poisoner.currentTarget);
                    Poisoner.poisoner.rpcCampaignPlayer(Poisoner.currentTarget);
                    Poisoner.poisoner.rpcInfectPlayer(Poisoner.currentTarget);
                    Poisoner.poisoner.showFlashForSixthSense(Poisoner.currentTarget);
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Poisoner.poisoned = Poisoner.currentTarget;

                        // Notify players about poison
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                        writer.Write(Poisoner.poisoned.PlayerId);
                        writer.Write((byte)0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.poisonerSetPoisoned(Poisoner.poisoned.PlayerId, 0);

                        byte lastTimer = (byte)Poisoner.delay;
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Poisoner.delay, new Action<float>((p) =>
                        { // Delayed action
                            if (p <= 1f)
                            {
                                byte timer = (byte)poisonButton.Timer;
                                if (timer != lastTimer)
                                {
                                    lastTimer = timer;
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    writer.Write((byte)RPCProcedure.GhostInfoTypes.PoisonTimer);
                                    writer.Write(timer);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                }
                            }
                            if (p == 1f)
                            {
                                // Perform kill if possible and reset poison (regardless whether the kill was successful or not)
                                var res = Helpers.checkMurderAttemptAndKill(Poisoner.poisoner, Poisoner.poisoned, showAnimation: false);
                                if (res == MurderAttemptResult.PerformKill)
                                {
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                                    writer.Write(byte.MaxValue);
                                    writer.Write(byte.MaxValue);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue);
                                }
                            }
                        })));
                        SoundEffectsManager.play("poisonerPoison");
                        poisonButton.HasEffect = true;
                    }
                    else if (murder == MurderAttemptResult.BlankKill)
                    {
                        poisonButton.Timer = poisonButton.MaxTimer;
                        poisonButton.HasEffect = false;
                    }
                    else if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        poisonButton.Timer = Survivor.cooldownReset;
                        poisonButton.HasEffect = false;
                    }
                    else if (murder == MurderAttemptResult.GAReset)
                    {
                        poisonButton.Timer = GuardianAngel.cooldownReset;
                        poisonButton.HasEffect = false;
                    }
                    else if (murder == MurderAttemptResult.ReverseKill)
                    {
                        Helpers.checkMurderAttemptAndKill(Poisoner.currentTarget, Poisoner.poisoner, showAnimation: false);
                        poisonButton.HasEffect = false;
                    }
                    else
                    {
                        poisonButton.HasEffect = false;
                    }
                },
                () => { return Poisoner.poisoner != null && Poisoner.poisoner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Poisoner.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    poisonButton.Timer = poisonButton.MaxTimer;
                    poisonButton.isEffectActive = false;
                    poisonButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Poisoner.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.Q,
                false, 0f, () => { poisonButton.Timer = poisonButton.MaxTimer; }
            );

            // Poisoner Blind trap
            poisonBlindTrapButton = new CustomButton(
                () =>
                {
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlindTrap, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setBlindTrap(buff);

                    SoundEffectsManager.play("trapperTrap");
                    poisonBlindTrapButton.Timer = poisonBlindTrapButton.MaxTimer;
                },
                () => { return Poisoner.poisoner != null && Poisoner.poisoner == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (poisonBlindTrapButtonText != null) poisonBlindTrapButtonText.text = $"{Poisoner.charges}";
                    return Poisoner.charges > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { poisonBlindTrapButton.Timer = poisonBlindTrapButton.MaxTimer; },
                Poisoner.getTrapButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, __instance, KeyCode.G
            );
            poisonBlindTrapButtonText = GameObject.Instantiate(poisonBlindTrapButton.actionButton.cooldownTimerText, poisonBlindTrapButton.actionButton.cooldownTimerText.transform.parent);
            poisonBlindTrapButtonText.text = "";
            poisonBlindTrapButtonText.enableWordWrapping = false;
            poisonBlindTrapButtonText.transform.localScale = Vector3.one * 0.5f;
            poisonBlindTrapButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Venerer Ability
            venererButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VenererCamouflage, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.venererCamouflage();
                    SoundEffectsManager.play("morphlingMorph");
                },
                () => { return Venerer.venerer != null && Venerer.venerer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (Venerer.numberOfKills == 0) venererButton.Sprite = Venerer.getNoAbilitiesButton();
                    if (Venerer.numberOfKills == 1) venererButton.Sprite = Venerer.getCamoButton();
                    if (Venerer.numberOfKills == 2) venererButton.Sprite = Venerer.getSpeedButton();
                    if (Venerer.numberOfKills >= 3) venererButton.Sprite = Venerer.getFreezeButton();
                    return Venerer.numberOfKills > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    venererButton.Timer = venererButton.MaxTimer;
                    venererButton.isEffectActive = false;
                    venererButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Venerer.getNoAbilitiesButton(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F,
                true, Venerer.duration, () => { venererButton.Timer = venererButton.MaxTimer; }
            );

            // Plaguebearer Campaign
            plaguebearerInfectButton = new CustomButton(
                () =>
                {
                    Plaguebearer.plaguebearer.rpcCampaignPlayer(Plaguebearer.currentTarget);
                    Plaguebearer.plaguebearer.rpcInfectPlayer(Plaguebearer.currentTarget);
                    Plaguebearer.plaguebearer.showFlashForSixthSense(Plaguebearer.currentTarget);
                    if (Helpers.checkSuspendAction(Plaguebearer.plaguebearer, Plaguebearer.currentTarget)) return;
                    plaguebearerInfectButton.Timer = plaguebearerInfectButton.MaxTimer;
                    Plaguebearer.currentTarget = null;
                },
                () => { return Plaguebearer.plaguebearer != null && Plaguebearer.plaguebearer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Plaguebearer.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { plaguebearerInfectButton.Timer = plaguebearerInfectButton.MaxTimer; },
                Plaguebearer.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Pestilence Kill
            pestilenceKillButton = new CustomButton(
                () =>
                {
                    Pestilence.pestilence.rpcCampaignPlayer(Pestilence.currentTarget);
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Pestilence.pestilence, Pestilence.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill) return;
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        pestilenceKillButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        pestilenceKillButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(Pestilence.pestilence, Pestilence.currentTarget, true);
                        Pestilence.pestilence.addGameInfoRpc(InfoType.AddKill);
                    }
                    pestilenceKillButton.Timer = pestilenceKillButton.MaxTimer;
                    Pestilence.currentTarget = null;
                },
                () => { return Pestilence.pestilence != null && Pestilence.pestilence == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Pestilence.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { pestilenceKillButton.Timer = pestilenceKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Doomsayer Observe
            doomsayerButton = new CustomButton(
                () =>
                {
                    Doomsayer.doomsayer.rpcCampaignPlayer(Doomsayer.currentTarget);
                    Doomsayer.doomsayer.rpcInfectPlayer(Doomsayer.currentTarget);
                    Doomsayer.doomsayer.showFlashForSixthSense(Doomsayer.currentTarget);
                    if (Helpers.checkSuspendAction(Doomsayer.doomsayer, Doomsayer.currentTarget)) return;
                },
                () => { return Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Doomsayer.currentTarget != null && Doomsayer.canObserve && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    doomsayerButton.Timer = doomsayerButton.MaxTimer;
                    Doomsayer.canObserve = true;
                },
                Doomsayer.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, 0f, () =>
                {
                    doomsayerButton.Timer = doomsayerButton.MaxTimer;
                    Doomsayer.canObserve = false;

                    var msg = Doomsayer.GetInfo(Doomsayer.currentTarget);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{msg}");

                    // Ghost Info
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.DetectiveOrMedicInfo);
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            );

            // Glitch Kill
            glitchKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(Glitch.glitch, Glitch.currentTarget);
                    Glitch.glitch.rpcCampaignPlayer(Glitch.currentTarget);
                    Glitch.glitch.rpcInfectPlayer(Glitch.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill) return;
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        MurderAttemptResult reverseMurder = Helpers.checkMuderAttempt(Glitch.currentTarget, Glitch.glitch);
                        if (reverseMurder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(Glitch.currentTarget, Glitch.glitch);
                            if (!Glitch.currentTarget.isEvil()) Glitch.currentTarget.addGameInfoRpc(InfoType.AddCorrectShot);
                            else Glitch.currentTarget.addGameInfoRpc(InfoType.AddKill);
                        }
                        return;
                    }
                    if (murder == MurderAttemptResult.SurvivorReset)
                    {
                        glitchKillButton.Timer = Survivor.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.GAReset)
                    {
                        glitchKillButton.Timer = GuardianAngel.cooldownReset;
                        return;
                    }
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(Glitch.glitch, Glitch.currentTarget, true);
                        Glitch.glitch.addGameInfoRpc(InfoType.AddKill);
                    }
                    glitchKillButton.Timer = glitchKillButton.MaxTimer;
                    Glitch.currentTarget = null;
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Glitch.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { glitchKillButton.Timer = glitchKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );

            // Glitch Mimic
            glitchMimicButton = new CustomButton(
                () =>
                {
                    if (Glitch.sampledPlayer == null)
                    {
                        glitchMimicButton.HasEffect = false;
                        glitchMimicButton.Timer = 3f;
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer && !player.Data.Disconnected)
                            {
                                if (!player.Data.IsDead) Glitch.mimicTargets.Add(player.PlayerId);
                                else
                                {
                                    foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                                    {
                                        if (body.ParentId == player.PlayerId) Glitch.mimicTargets.Add(player.PlayerId);
                                    }
                                }
                            }
                        }
                        byte[] mimictargetIDs = Glitch.mimicTargets.ToArray();
                        var pk = new Glitch.PlayerMenu((x) =>
                        {
                            Glitch.glitch.rpcCampaignPlayer(x);
                            Glitch.glitch.rpcInfectPlayer(x);
                            Glitch.glitch.showFlashForSixthSense(x);
                            Glitch.sampledPlayer = x;
                            SoundEffectsManager.play("morphlingSample");
                        }, (y) =>
                        {
                            return mimictargetIDs.Contains(y.PlayerId);
                        });
                        Coroutines.Start(pk.Open(0f, true));
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchMimic, SendOption.Reliable, -1);
                        writer.Write(Glitch.sampledPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.glitchMimic(Glitch.sampledPlayer.PlayerId);
                        Glitch.sampledPlayer = null;
                        SoundEffectsManager.play("morphlingMorph");
                        glitchMimicButton.HasEffect = true;
                    }
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
                () =>
                {
                    glitchMimicButton.Timer = glitchMimicButton.MaxTimer;
                    glitchMimicButton.isEffectActive = false;
                    glitchMimicButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Glitch.sampledPlayer = null;
                },
                Glitch.getMimicButtonSprite(), new Vector3(0f, 0f, 0f), __instance, KeyCode.F,
                true, Glitch.morphDuration, () => { glitchMimicButton.Timer = glitchMimicButton.MaxTimer; }, true
            );

            // Glitch Hack
            glitchHackButton = new CustomButton(
                () =>
                {
                    Glitch.glitch.rpcCampaignPlayer(Glitch.currentTarget);
                    Glitch.glitch.rpcInfectPlayer(Glitch.currentTarget);
                    Glitch.glitch.showFlashForSixthSense(Glitch.currentTarget);
                    if (Helpers.checkSuspendAction(Glitch.glitch, Glitch.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchHack, SendOption.Reliable, -1);
                    writer.Write(Glitch.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.glitchHack(Glitch.currentTarget.PlayerId);
                    SoundEffectsManager.play("glitchHack");
                },
                () => { return Glitch.glitch != null && Glitch.glitch == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Glitch.currentTarget && Glitch.hackedPlayer == null && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    glitchHackButton.Timer = glitchHackButton.MaxTimer;
                    glitchHackButton.isEffectActive = false;
                    glitchHackButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Glitch.getHackButtonSprite(), new Vector3(1f, 0f, 0f), __instance, KeyCode.G,
                true, Glitch.hackDuration, () => { glitchHackButton.Timer = glitchHackButton.MaxTimer; }, true
            );

            // Button Barry Button
            buttonBarryButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    Helpers.handlePoisonerKillOnBodyReport();
                    ButtonBarry.isCalledEmergency = true;
                    PlayerControl.LocalPlayer.RemainingEmergencies++;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                },
                () =>
                {
                    if (glitchMimicButton.HasButton())
                        buttonBarryButton.PositionOffset = new Vector3(0f, 1f, 0f);
                    return ButtonBarry.buttonBarry != null && ButtonBarry.buttonBarry == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => { return !ButtonBarry.isCalledEmergency && PlayerControl.LocalPlayer.CanMove; },
                () => { buttonBarryButton.Timer = buttonBarryButton.MaxTimer; },
                ButtonBarry.getButtonSprite(), new Vector3(0f, 0f, 0f), __instance, null, true
            );

            // Satelite Button
            sateliteButton = new CustomButton(
                () =>
                {
                    Satelite.corpsesTrackingTimer = Satelite.corpsesTrackingDuration;
                    SoundEffectsManager.play("sateliteTrackCorpses");
                },
                () =>
                {
                    if (glitchMimicButton.HasButton())
                        sateliteButton.PositionOffset = new Vector3(0f, 1f, 0f);
                    return Satelite.satelite != null && Satelite.satelite == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    sateliteButton.Timer = sateliteButton.MaxTimer;
                    sateliteButton.isEffectActive = false;
                    sateliteButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Satelite.getTrackCorpsesButtonSprite(), new Vector3(0f, 0f, 0f), __instance, null,
                true, Satelite.corpsesTrackingDuration, () => { sateliteButton.Timer = sateliteButton.MaxTimer; }, true
            );

            // Cannibal Eat
            cannibalEatButton = new CustomButton(
                () =>
                {
                    Cannibal.cannibal.rpcCampaignPlayer(Helpers.playerById(Cannibal.currentTarget.ParentId));
                    Cannibal.cannibal.rpcInfectPlayer(Helpers.playerById(Cannibal.currentTarget.ParentId));
                    Cannibal.cannibal.showFlashForSixthSense(Helpers.playerById(Cannibal.currentTarget.ParentId));
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(Cannibal.currentTarget.ParentId);
                    writer.Write(Cannibal.cannibal.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.cleanBody(Cannibal.currentTarget.ParentId, Cannibal.cannibal.PlayerId);
                    SoundEffectsManager.play("cannibalEat");

                    janitorCleanButton.Timer = janitorCleanButton.MaxTimer;
                    Cannibal.cannibal.addGameInfoRpc(InfoType.AddEat);
                    Cannibal.currentTarget = null;
                },
                () => { return Cannibal.cannibal != null && Cannibal.cannibal == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Cannibal.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { cannibalEatButton.Timer = cannibalEatButton.MaxTimer; },
                Cannibal.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.F
            );

            // Escapist Button
            escapistButton = new CustomButton(
                () =>
                {
                    if (Escapist.jumpLocation == Vector3.zero)
                    {
                        Escapist.jumpLocation = PlayerControl.LocalPlayer.transform.localPosition;
                        escapistButton.Sprite = Escapist.getEscapeButtonSprite();
                        SoundEffectsManager.play("trackerTrackPlayer");
                        escapistButton.Timer = 1f;
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPosition, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(Escapist.jumpLocation.x);
                        writer.Write(Escapist.jumpLocation.y);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        PlayerControl.LocalPlayer.transform.position = Escapist.jumpLocation;

                        escapistButton.Timer = escapistButton.MaxTimer;
                        Escapist.jumpLocation = Vector3.zero;
                    }
                },
                () => { return Escapist.escapist != null && Escapist.escapist == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    Escapist.jumpLocation = Vector3.zero;
                    escapistButton.Timer = escapistButton.MaxTimer;
                },
                Escapist.getMarkButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Vampire Hunter Stake
            vampireHunterStakeButton = new CustomButton(
                () =>
                {
                    VampireHunter.vampireHunter.rpcCampaignPlayer(VampireHunter.currentTarget);
                    VampireHunter.vampireHunter.rpcInfectPlayer(VampireHunter.currentTarget);
                    VampireHunter.vampireHunter.showFlashForSixthSense(VampireHunter.currentTarget);
                    if (Helpers.checkSuspendAction(VampireHunter.vampireHunter, VampireHunter.currentTarget)) return;

                    if (VampireHunter.currentTarget == Dracula.dracula || VampireHunter.currentTarget == Vampire.vampire)
                    {
                        MurderAttemptResult murder = Helpers.checkMurderAttemptAndKill(VampireHunter.vampireHunter, VampireHunter.currentTarget);
                        if (murder == MurderAttemptResult.PerformKill)
                        {
                            VampireHunter.vampireHunter.addGameInfoRpc(InfoType.AddCorrectShot);
                            vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer;
                        }
                        if (murder == MurderAttemptResult.SurvivorReset) vampireHunterStakeButton.Timer = Survivor.cooldownReset;
                        if (murder == MurderAttemptResult.GAReset) vampireHunterStakeButton.Timer = GuardianAngel.cooldownReset;
                        if (murder == MurderAttemptResult.BlankKill) vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer;
                    }
                    else
                    {
                        VampireHunter.failedStakes++;
                        Helpers.showFlash(Palette.ImpostorRed);
                        VampireHunter.vampireHunter.addGameInfoRpc(InfoType.AddMisfire);
                        vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer;
                    }
                },
                () => { return VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (vampireHunterStakeButtonText != null) vampireHunterStakeButtonText.text = $"{VampireHunter.maxFailedStakes - VampireHunter.failedStakes}";
                    return VampireHunter.currentTarget != null && VampireHunter.canStake && PlayerControl.LocalPlayer.CanMove;
                },
                () => { vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer; },
                VampireHunter.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
            vampireHunterStakeButtonText = GameObject.Instantiate(vampireHunterStakeButton.actionButton.cooldownTimerText, vampireHunterStakeButton.actionButton.cooldownTimerText.transform.parent);
            vampireHunterStakeButtonText.text = "";
            vampireHunterStakeButtonText.enableWordWrapping = false;
            vampireHunterStakeButtonText.transform.localScale = Vector3.one * 0.5f;
            vampireHunterStakeButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Oracle Confess
            oracleButton = new CustomButton(
                () =>
                {
                    Oracle.oracle.rpcCampaignPlayer(Oracle.currentTarget);
                    Oracle.oracle.rpcInfectPlayer(Oracle.currentTarget);
                    Oracle.oracle.showFlashForSixthSense(Oracle.currentTarget);
                    if (Helpers.checkSuspendAction(Oracle.oracle, Oracle.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OracleSetConfessor, Hazel.SendOption.Reliable, -1);
                    writer.Write(Oracle.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.oracleSetConfessor(Oracle.currentTarget.PlayerId);
                    SoundEffectsManager.play("seerReveal");

                    oracleButton.Timer = oracleButton.MaxTimer;
                    Oracle.currentTarget = null;
                },
                () => { return Oracle.oracle != null && Oracle.oracle == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Oracle.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { oracleButton.Timer = oracleButton.MaxTimer; },
                Oracle.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );

            // Lookout Zoom
            lookoutZoomButton = new CustomButton(
                () =>
                {
                    Lookout.toggleZoom();
                    Lookout.remainingZooms--;
                },
                () => { return Lookout.lookout != null && Lookout.lookout == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (lookoutZoomButtonText != null) lookoutZoomButtonText.text = $"{Lookout.remainingZooms}";
                    return Lookout.remainingZooms > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    lookoutZoomButton.Timer = lookoutZoomButton.MaxTimer;
                    lookoutZoomButton.isEffectActive = false;
                    lookoutZoomButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Lookout.getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F,
                true, Lookout.duration, () =>
                {
                    Lookout.toggleZoom(true);
                    lookoutZoomButton.Timer = lookoutZoomButton.MaxTimer;
                }
            );
            lookoutZoomButtonText = GameObject.Instantiate(lookoutZoomButton.actionButton.cooldownTimerText, lookoutZoomButton.actionButton.cooldownTimerText.transform.parent);
            lookoutZoomButtonText.text = "";
            lookoutZoomButtonText.enableWordWrapping = false;
            lookoutZoomButtonText.transform.localScale = Vector3.one * 0.5f;
            lookoutZoomButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Plumber Seal Vent
            plumberSealButton = new CustomButton(
                () =>
                {
                    Plumber.remainingSeals--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SealVent, Hazel.SendOption.Reliable);
                    writer.WritePacked(Plumber.ventTarget.Id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.sealVent(Plumber.ventTarget.Id);
                    SoundEffectsManager.play("plumberSealVent");

                    plumberSealButton.Timer = plumberSealButton.MaxTimer;
                    Plumber.ventTarget = null;
                },
                () => { return Plumber.plumber != null && Plumber.plumber == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (plumberSealButtonText != null) plumberSealButtonText.text = $"{Plumber.remainingSeals}";
                    return Plumber.remainingSeals > 0 && Plumber.ventTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => { plumberSealButton.Timer = plumberSealButton.MaxTimer; },
                Plumber.getCloseVentButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, __instance, KeyCode.F
            );
            plumberSealButtonText = GameObject.Instantiate(plumberSealButton.actionButton.cooldownTimerText, plumberSealButton.actionButton.cooldownTimerText.transform.parent);
            plumberSealButtonText.text = "";
            plumberSealButtonText.enableWordWrapping = false;
            plumberSealButtonText.transform.localScale = Vector3.one * 0.5f;
            plumberSealButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Plumber Flush
            plumberFlushButton = new CustomButton(
                () =>
                {
                    Plumber.remainingFlushs--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlumberFlush, Hazel.SendOption.Reliable);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.plumberFlush();
                    
                    plumberFlushButton.Timer = plumberFlushButton.MaxTimer;
                },
                () => { return Plumber.plumber != null && Plumber.plumber == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (plumberFlushButtonText != null) plumberFlushButtonText.text = $"{Plumber.remainingFlushs}";
                    return Plumber.remainingFlushs > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { plumberFlushButton.Timer = plumberFlushButton.MaxTimer; },
                Plumber.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
            plumberFlushButtonText = GameObject.Instantiate(plumberFlushButton.actionButton.cooldownTimerText, plumberFlushButton.actionButton.cooldownTimerText.transform.parent);
            plumberFlushButtonText.text = "";
            plumberFlushButtonText.enableWordWrapping = false;
            plumberFlushButtonText.transform.localScale = Vector3.one * 0.5f;
            plumberFlushButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Disperser Disperse
            disperserButton = new CustomButton(
                () =>
                {
                    Disperser.remainingDisperses--;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DisperserDisperse, Hazel.SendOption.Reliable);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.disperserDisperse();
                },
                () => { return Disperser.disperser != null && Disperser.disperser == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (disperserButtonText != null) disperserButtonText.text = $"{Disperser.remainingDisperses}";
                    return Disperser.remainingDisperses > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { disperserButton.Timer = disperserButton.MaxTimer; },
                Disperser.getButtonSprite(), new Vector3(0f, 0f, 0f), __instance, null, true
            );
            disperserButtonText = GameObject.Instantiate(disperserButton.actionButton.cooldownTimerText, disperserButton.actionButton.cooldownTimerText.transform.parent);
            disperserButtonText.text = "";
            disperserButtonText.enableWordWrapping = false;
            disperserButtonText.transform.localScale = Vector3.one * 0.5f;
            disperserButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Banshee Scare
            bansheeScareButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BansheeScare, Hazel.SendOption.Reliable);
                    writer.Write(Banshee.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.bansheeScare(Banshee.currentTarget.PlayerId);
                },
                () => { return Banshee.banshee != null && Banshee.banshee == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Banshee.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    bansheeScareButton.Timer = bansheeScareButton.MaxTimer;
                    bansheeScareButton.isEffectActive = false;
                    bansheeScareButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Banshee.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q,
                true, Banshee.duration, () => { bansheeScareButton.Timer = bansheeScareButton.MaxTimer; }
            );

            // Poltergeist move body
            poltergeistButton = new CustomButton(
                () =>
                {
                    var deadBody = Poltergeist.targetBody;
                    if (deadBody == null) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoltergeistMove, Hazel.SendOption.Reliable);
                    writer.Write(deadBody.ParentId);
                    writer.Write(PlayerControl.LocalPlayer.GetTruePosition());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    Poltergeist.MoveDeadBody(deadBody.ParentId, PlayerControl.LocalPlayer.GetTruePosition());
                    poltergeistButton.Timer = poltergeistButton.MaxTimer;
                },
                () => { return Poltergeist.poltergeist != null && Poltergeist.poltergeist == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    Poltergeist.targetBody = Helpers.GetDeadBody(PlayerControl.LocalPlayer.GetTruePosition());
                    return Poltergeist.targetBody && PlayerControl.LocalPlayer.CanMove;
                },
                () => { poltergeistButton.Timer = poltergeistButton.MaxTimer; },
                Poltergeist.getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
            
            // Deceiver Place Decoy
            deceiverPlaceDecoyButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeceiverPlaceDecoy, Hazel.SendOption.Reliable);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.transform.position);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.deceiverPlaceDecoy(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.transform.position);
                    deceiverDestroyDecoyButton.Timer = 3f;
                },
                () => { return Deceiver.deceiver != null && Deceiver.deceiver == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Deceiver.decoy == null && PlayerControl.LocalPlayer.CanMove; },
                () => { deceiverPlaceDecoyButton.Timer = deceiverPlaceDecoyButton.MaxTimer; },
                Deceiver.getDecoyButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, __instance, KeyCode.F
            );

            // Deceiver Destroy Decoy
            deceiverDestroyDecoyButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeceiverDecoyDestroy, Hazel.SendOption.Reliable);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(Deceiver.decoy.Id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.deceiverDecoyDestroy(PlayerControl.LocalPlayer, Deceiver.decoy.Id);
                    deceiverPlaceDecoyButton.Timer = deceiverPlaceDecoyButton.MaxTimer;
                },
                () => { return Deceiver.deceiver != null && Deceiver.deceiver == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Deceiver.decoy != null && PlayerControl.LocalPlayer.CanMove; },
                () => { deceiverDestroyDecoyButton.Timer = deceiverDestroyDecoyButton.MaxTimer; },
                Deceiver.getDecoyDestroyButtonSprite(), CustomButton.ButtonPositions.lowerRowCenter, __instance, KeyCode.G
            );

            // Deceiver Swap Decoy
            deceiverSwapDecoyButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeceiverDecoySwap, Hazel.SendOption.Reliable);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(Deceiver.decoy.Id);
                    writer.Write(PlayerControl.LocalPlayer.transform.position);
                    writer.Write(Deceiver.decoy.GameObject.transform.position);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.deceiverDecoySwap(PlayerControl.LocalPlayer, Deceiver.decoy.Id, PlayerControl.LocalPlayer.transform.position, Deceiver.decoy.GameObject.transform.position);
                    deceiverSwapDecoyButton.Timer = deceiverSwapDecoyButton.MaxTimer;
                },
                () => { return Deceiver.deceiver != null && Deceiver.deceiver == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Deceiver.decoy != null && PlayerControl.LocalPlayer.CanMove; },
                () => { deceiverSwapDecoyButton.Timer = deceiverSwapDecoyButton.MaxTimer; },
                Deceiver.getDecoySwapButtonSprite(), CustomButton.ButtonPositions.upperRowFarLeft, __instance, KeyCode.H
            );
            
            // Zoom Button
            zoomOutButton = new CustomButton(
                () => { Helpers.toggleZoom(); },
                () =>
                {
                    bool zoomButtonActive = true;
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    if (!PlayerControl.LocalPlayer.isEvil()) zoomButtonActive = numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
                    return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead && zoomButtonActive;
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { zoomOutButton.Timer = 0f; },
                null, new Vector3(0.4f, 2.8f, 0), __instance, KeyCode.KeypadPlus
            );
            zoomOutButton.Timer = 0f;
            
            // Set the default (or settings from the previous game) timers / durations when spawning the buttons
            initialized = true;
            setCustomButtonCooldowns();
        }
    }
}