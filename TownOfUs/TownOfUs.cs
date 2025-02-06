using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch]
    public static class TownOfUs
    {
        public static DateTime startTime = DateTime.UtcNow;
        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);

        public static void clearAndReloadRoles() {
            startTime = DateTime.UtcNow;
            PlayerGameInfo.clearAndReload();
            
            Morphling.clearAndReload();
            Camouflager.clearAndReload();
            Snitch.clearAndReload();
            Engineer.clearAndReload();
            Sheriff.clearAndReload();
            Jester.clearAndReload();
            Shifter.clearAndReload();
            Spy.clearAndReload();
            Swapper.clearAndReload();
            Mayor.clearAndReload();
            Medic.clearAndReload();
            Dracula.clearAndReload();
            Vampire.clearAndReload();
            Poisoner.clearAndReload();
            Scavenger.clearAndReload();
            Executioner.clearAndReload();
            Lawyer.clearAndReload();
            Pursuer.clearAndReload();
            GuardianAngel.clearAndReload();
            FallenAngel.clearAndReload();
            Survivor.clearAndReload();
            Amnesiac.clearAndReload();
            Investigator.clearAndReload();
            Veteran.clearAndReload();
            Seer.clearAndReload();
            Juggernaut.clearAndReload();
            Swooper.clearAndReload();
            Mercenary.clearAndReload();
            Blackmailer.clearAndReload();
            Escapist.clearAndReload();
            Miner.clearAndReload();
            Cleaner.clearAndReload();
            Trapper.clearAndReload();
            Phantom.clearAndReload();
            Grenadier.clearAndReload();
            Doomsayer.clearAndReload();
            
            Lovers.clearAndReload();
            Blind.clearAndReload();
            Bait.clearAndReload();
            Sleuth.clearAndReload();
            Tiebreaker.clearAndReload();
            ButtonBarry.clearAndReload();
            Indomitable.clearAndReload();
            Drunk.clearAndReload();
            Torch.clearAndReload();
            DoubleShot.clearAndReload();
            
            // Gamemodes
            HandleGuesser.clearAndReload();
        }
    }

    public static class Morphling {
        public static PlayerControl morphling;
        public static PlayerControl morphTarget;
        public static PlayerControl sampledTarget;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;
        
        public static float morphTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;

        private static Sprite sampleSprite;
        public static Sprite getSampleSprite()
            => sampleSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SampleButton.png", 115f);
        private static Sprite morphSprite;
        public static Sprite getMorphSprite()
            => morphSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MorphButton.png", 115f);

        public static void resetMorph() {
            morphTarget = null;
            morphTimer = 0f;
            if (morphling == null) return;
            morphling.setDefaultLook();
        }

        public static void clearAndReload() {
            resetMorph();
            morphling = null;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
            duration = CustomOptionHolder.morphlingDuration.getFloat();
        }
    }

    public static class Camouflager {
        public static PlayerControl camouflager;
        public static Color color = Palette.ImpostorRed;
        
        public static bool camoComms;
        public static float camouflageTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamoButton.png", 115f);
        
        public static void resetCamouflage() {
            if (Helpers.isCamoComms()) return;
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p == Swooper.swooper && Swooper.isInvisble || p == Phantom.phantom && Phantom.isInvisble)
                    continue;
                p.setDefaultLook();
                camoComms = false;
            }
        }

        public static void clearAndReload() {
            resetCamouflage();
            camouflager = null;
            camouflageTimer = 0f;
            camoComms = false;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
            duration = CustomOptionHolder.camouflagerDuration.getFloat();
        }
    }

    public static class Snitch {
        public static PlayerControl snitch;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.83f, 0.69f, 0.22f, 1f);

        public static int taskCountForReveal = 1;
        public static bool includeNeutral = false;
        public static bool includeKillingNeutral = false;
        public static bool seeInMeeting = false;

        public static void clearAndReload() {
            if (localArrows != null) {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat());
            includeNeutral = CustomOptionHolder.snitchIncludeNeutral.getBool();
            includeKillingNeutral = CustomOptionHolder.snitchIncludeKillingNeutral.getBool();
            seeInMeeting = CustomOptionHolder.snitchSeeInMeeting.getBool();
            snitch = null;
        }
    }

    public static class Engineer {
        public static PlayerControl engineer;
        public static Color color = new Color(1f, 0.65f, 0.04f, 1f);

        public static float cooldown = 30f;
        public static int remainingFixes = 1;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.RepairButton.png", 115f);
        
        private static Sprite doorsButtonSprite;
        public static Sprite getDoorButtonSprite()
            => doorsButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.EngineerDoors.png", 115f);

        public static void clearAndReload() {
            engineer = null;
            cooldown = CustomOptionHolder.engineerDoorsCooldown.getFloat();
            remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        }
    }

    public static class Sheriff {
        public static PlayerControl sheriff;
        public static PlayerControl currentTarget;
        public static Color color = Color.yellow;

        public static float cooldown = 30f;
        public static bool canKillNeutrals = false;
        public static bool canKillKNeutrals = false;

        public static void clearAndReload() {
            sheriff = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
            canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
            canKillKNeutrals = CustomOptionHolder.sheriffCanKillKNeutrals.getBool();
        }
    }

    public static class Jester {
        public static PlayerControl jester;
        public static Color color = new Color(1f, 0.75f, 0.8f, 1f);

        public static bool triggerJesterWin = false;

        public static bool canCallEmergency = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload() {
            jester = null;
            triggerJesterWin = false;
            canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
            hasImpostorVision = CustomOptionHolder.jesterHasImpostorVision.getBool();
        }
    }

    public static class Shifter {
        public static PlayerControl shifter;
        public static PlayerControl futureShift;
        public static PlayerControl currentTarget;
        public static Color color = Color.gray;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShiftButton.png", 115f);
        
        public static void clearAndReload() {
            shifter = null;
            currentTarget = null;
            futureShift = null;
        }
    }

    public static class Spy {
        public static PlayerControl spy;
        public static Color color = new Color(0.8f, 0.64f, 0.8f, 1f);

        public static void clearAndReload() {
            spy = null;
        }
    }

    public static class Swapper {
        public static PlayerControl swapper;
        public static Color color = new Color(0.4f, 0.9f, 0.4f, 1f);
        
        public static byte playerId1 = Byte.MaxValue;
        public static byte playerId2 = Byte.MaxValue;
        
        public static bool canCallEmergency = false;
        public static bool canOnlySwapOthers = false;

        private static Sprite spriteCheck;
        public static Sprite getCheckSprite()
            => spriteCheck ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwapperCheck.png", 150f);
        
        public static void clearAndReload() {
            swapper = null;
            playerId1 = Byte.MaxValue;
            playerId2 = Byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
            canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        }
    }

    public static class Mayor {
        public static PlayerControl mayor;
        public static Color color = new Color(0.44f, 0.31f, 0.66f, 1f);

        public static bool isRevealed = false;

        private static Sprite spriteCheck;
        public static Sprite getCheckSprite()
            => spriteCheck ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MayorReveal.png", 100f);
        
        public static void clearAndReload() {
            mayor = null;
            isRevealed = false;
        }
    }

    public static class Medic {
        public static PlayerControl medic;
        public static PlayerControl shielded;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0f, 0.4f, 0f, 1f);

        public static bool usedShield;

        public static int showShielded = 0; // 0 - Everyone, 1 - Shielded, 2 - Medic, 3 - Medic+Shielded, 4 - Nobody
        public static int getsNotification = 0; // 0 - Everyone, 1 - Shielded, 2 - Medic, 3 - Medic+Shielded, 4 - Nobody
        public static bool getInfoOnReport = false;
        public static float reportNameDuration = 0f;
        public static float reportColorDuration = 20f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShieldButton.png", 115f);
        
        public static bool shieldVisible(PlayerControl target) {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;

            if (shielded != null && ((target == shielded && !isMorphedMorphling) || (isMorphedMorphling && Morphling.morphTarget == shielded))) {
                hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || showShielded == 1 & PlayerControl.LocalPlayer == shielded // Shielded
                    || showShielded == 2 & PlayerControl.LocalPlayer == medic // Medic
                    || showShielded == 3 && (PlayerControl.LocalPlayer ==  shielded || PlayerControl.LocalPlayer == medic);
            }
            return hasVisibleShield;
        }

        public static void clearAndReload() {
            medic = null;
            shielded = null;
            currentTarget = null;
            usedShield = false;
            showShielded = CustomOptionHolder.medicShowShielded.getSelection();
            getsNotification = CustomOptionHolder.medicGetsNotification.getSelection();
            getInfoOnReport = CustomOptionHolder.medicGetsInfo.getBool();
            reportNameDuration = CustomOptionHolder.medicReportNameDuration.getFloat();
            reportColorDuration = CustomOptionHolder.medicReportColorDuration.getFloat();
        }
    }

    public static class Dracula {
        public static PlayerControl dracula;
        public static PlayerControl fakeVampire;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> formerDraculas = new List<PlayerControl>();
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        public static bool wasTeamRed;
        public static bool wasImpostor;
        public static int numberOfAllVampires = 1;
        public static bool canCreateVampire = true;
        
        public static int maxVampires = 2;
        public static float cooldown = 30f;
        public static float createVampireCooldown = 30f;
        public static bool canUseVents = true;
        public static bool canCreateVampireFromImpostor = true;
        public static bool hasImpostorVision = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BiteButton.png", 100f);
        
        public static void removeCurrentDracula() {
            if (!formerDraculas.Any(x => x.PlayerId == dracula.PlayerId)) formerDraculas.Add(dracula);
            dracula = null;
            currentTarget = null;
            fakeVampire = null;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
        }

        public static void clearAndReload() {
            dracula = null;
            fakeVampire = null;
            currentTarget = null;
            formerDraculas.Clear();
            numberOfAllVampires = 1;
            canCreateVampire = true;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.draculaCanUseVents.getBool();
            canCreateVampireFromImpostor = CustomOptionHolder.draculaCanCreateVampireFromImpostor.getBool();
            hasImpostorVision = CustomOptionHolder.draculaAndVampireHaveImpostorVision.getBool();
            maxVampires = Mathf.RoundToInt(CustomOptionHolder.draculaMaxVampires.getFloat());
            wasTeamRed = wasImpostor = false;
        }
    }

    public static class Vampire {
        public static PlayerControl vampire;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);

        public static bool wasTeamRed;
        public static bool wasImpostor;
        
        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload() {
            vampire = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.vampireCanUseVents.getBool();
            hasImpostorVision = CustomOptionHolder.draculaAndVampireHaveImpostorVision.getBool();
            wasTeamRed = wasImpostor = false;
        }
    }

    public static class Poisoner {
        public static PlayerControl poisoner;
        public static PlayerControl poisoned;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;
        public static float delay = 10f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PoisonButton.png", 100f);

        public static void clearAndReload() {
            poisoner = null;
            poisoned = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.poisonerCooldown.getFloat();
            delay = CustomOptionHolder.poisonerKillDelay.getFloat();
        }
    }

    public static class Scavenger {
        public static PlayerControl scavenger;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.6f, 0.15f, 0.15f, 1f);
        
        public static int eatenBodies = 0;
        public static bool triggerScavengerWin = false;

        public static float cooldown = 30f;
        public static int scavengerNumberToWin = 4;
        public static bool canUseVents = true;
        public static bool showArrows = true;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DevourButton.png", 100f);
        
        public static void clearAndReload() {
            scavenger = null;
            eatenBodies = 0;
            triggerScavengerWin = false;
            cooldown = CustomOptionHolder.scavengerCooldown.getFloat();
            scavengerNumberToWin = Mathf.RoundToInt(CustomOptionHolder.scavengerNumberToWin.getFloat());
            canUseVents = CustomOptionHolder.scavengerCanUseVents.getBool();
            showArrows = CustomOptionHolder.scavengerShowArrows.getBool();
            if (localArrows != null) {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
        }
    }

    public static class Executioner {
        public static PlayerControl executioner;
        public static PlayerControl target;
        public static Color color = new Color(0.55f, 0.25f, 0.02f, 1f);
        
        public static bool targetWasGuessed = false;
        public static bool triggerExecutionerWin = false;
        
        public static float vision = 1f;
        public static bool canCallEmergency = true;

        public static void clearAndReload(bool clearTarget = true) {
            executioner = null;
            if (clearTarget) {
                target = null;
                targetWasGuessed = false;
            }
            triggerExecutionerWin = false;
            vision = CustomOptionHolder.executionerVision.getFloat();
            canCallEmergency = CustomOptionHolder.executionerCanCallEmergency.getBool();
        }
    }

    public static class Lawyer {
        public static PlayerControl lawyer;
        public static PlayerControl target;
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);
        
        public static bool targetWasGuessed = false;

        public static float vision = 1f;
        public static bool lawyerKnowsRole = false;
        public static bool canCallEmergency = true;
        public static bool targetCanBeJester = false;

        public static void clearAndReload(bool clearTarget = true) {
            lawyer = null;
            if (clearTarget) {
                target = null;
                targetWasGuessed = false;
            }
            vision = CustomOptionHolder.lawyerVision.getFloat();
            lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
            targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
            canCallEmergency = CustomOptionHolder.lawyerCanCallEmergency.getBool();
        }
    }

    public static class Pursuer {
        public static PlayerControl pursuer;
        public static PlayerControl target;
        public static List<PlayerControl> blankedList = new List<PlayerControl>();
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);

        public static int blanks = 0;
        public static bool notAckedExiled = false;

        public static float cooldown = 30f;
        public static int blanksNumber = 5;

        public static Sprite blank;
        public static Sprite getTargetSprite()
            => blank ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PursuerButton.png", 115f);

        public static void clearAndReload() {
            pursuer = null;
            target = null;
            blankedList = new List<PlayerControl>();
            blanks = 0;
            notAckedExiled = false;
            cooldown = CustomOptionHolder.pursuerCooldown.getFloat();
            blanksNumber = Mathf.RoundToInt(CustomOptionHolder.pursuerBlanksNumber.getFloat());
        }
    }

    public static class GuardianAngel {
        public static PlayerControl guardianAngel;
        public static PlayerControl target;
        public static Color color = new Color(0.7f, 1f, 1f, 1f);

        public static bool promoteToFA = false;

        public static bool protectActive = false;
        public static bool targetWasGuessed = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int remainingProtects = 3;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ProtectButton.png", 100f);

        public static void clearAndReload(bool clearTarget = true) {
            guardianAngel = null;
            if (clearTarget) {
                target = null;
                targetWasGuessed = false;
            }
            protectActive = false;
            cooldown = CustomOptionHolder.guardianAngelProtectCooldown.getFloat();
            duration = CustomOptionHolder.guardianAngelProtectDuration.getFloat();
            remainingProtects = Mathf.RoundToInt(CustomOptionHolder.guardianAngelNumberOfProtects.getFloat());
        }
    }

    public static class FallenAngel {
        public static PlayerControl fallenAngel;
        public static PlayerControl target;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.6f, 1f, 1f, 1f);

        public static float cooldown = 30f;

        public static void clearAndReload() {
            fallenAngel = null;
            target = null;
            currentTarget = null;
            cooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        }
    }

    public static class Survivor {
        public static PlayerControl survivor;
        public static Color color = new Color(1f, 0.9f, 0.3f, 1f);

        public static bool safeguardActive = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int remainingSafeguards = 3;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SafeguardButton.png", 100f);

        public static void clearAndReload(bool clearTarget = true) {
            survivor = null;
            safeguardActive = false;
            cooldown = CustomOptionHolder.survivorSafeguardCooldown.getFloat();
            duration = CustomOptionHolder.survivorSafeguardDuration.getFloat();
            remainingSafeguards = Mathf.RoundToInt(CustomOptionHolder.survivorNumberOfSafeguards.getFloat());
        }
    }

    public static class Amnesiac {
        public static PlayerControl amnesiac;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.5f, 0.7f, 1f, 1f);

        public static bool showArrows = false;
        public static float delay = 3f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RememberButton.png", 100f);

        public static void clearAndReload() {
            amnesiac = null;
            if (localArrows != null) {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            showArrows = CustomOptionHolder.amnesiacShowArrows.getBool();
            delay = CustomOptionHolder.amnesiacDelay.getFloat();
        }
    }

    public static class Investigator {
        public static PlayerControl investigator;
        public static PlayerControl watching;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0f, 0.7f, 0.7f, 1f);

        public static float timer = 6.2f;

        public static bool seeFlashColor = false;
        public static float footprintIntervall = 1f;
        public static float footprintDuration = 1f;
        public static bool anonymousFootprints = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.WatchButton.png", 115f);
        
        public static void clearAndReload() {
            investigator = null;
            watching = null;
            currentTarget = null;
            timer = 6.2f;
            seeFlashColor = CustomOptionHolder.investigatorSeeColor.getBool();
            anonymousFootprints = CustomOptionHolder.investigatorAnonymousFootprints.getBool();
            footprintIntervall = CustomOptionHolder.investigatorFootprintIntervall.getFloat();
            footprintDuration = CustomOptionHolder.investigatorFootprintDuration.getFloat();
        }
    }

    public static class Veteran {
        public static PlayerControl veteran;
        public static Color color = new Color(0.6f, 0.5f, 0.25f, 1f);

        public static bool isAlertActive = false;

        public static float cooldown = 30f;
        public static float duration = 5f;
        public static int remainingAlerts = 5;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.AlertButton.png", 100f);
        
        public static void clearAndReload() {
            veteran = null;
            isAlertActive = false;
            cooldown = CustomOptionHolder.veteranCooldown.getFloat();
            duration = CustomOptionHolder.veteranDuration.getFloat();
            remainingAlerts = Mathf.RoundToInt(CustomOptionHolder.veteranNumberOfAlerts.getFloat());
        }
    }

    public static class Seer {
        public static PlayerControl seer;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> revealedPlayers = new List<PlayerControl>();
        public static Color color = new Color(1f, 0.8f, 0.5f, 1f);

        public static float cooldown = 30f;
        public static bool neutRed = false;
        public static bool kneutRed = false;
        
        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RevealButton.png", 115f);
        
        public static void clearAndReload() {
            seer = null;
            currentTarget = null;
            revealedPlayers = new List<PlayerControl>();
            cooldown = CustomOptionHolder.seerCooldown.getFloat();
            neutRed = CustomOptionHolder.seerNeutRed.getBool();
            kneutRed = CustomOptionHolder.seerKNeutRed.getBool();
        }
    }

    public static class Juggernaut {
        public static PlayerControl juggernaut;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.55f, 0f, 0.3f, 1f);

        public static float cooldown = 30f;
        public static float cooldownReduce = 10f;
        public static bool hasImpostorVision = false;
        public static bool canUseVents = false;

        public static void clearAndReload() {
            juggernaut = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.juggernautCooldown.getFloat();
            cooldownReduce = CustomOptionHolder.juggernautCooldownReduce.getFloat();
            hasImpostorVision = CustomOptionHolder.juggernautHasImpostorVision.getBool();
            canUseVents = CustomOptionHolder.juggernautCanVent.getBool();
        }
    }

    public static class Swooper {
        public static PlayerControl swooper;
        public static Color color = Palette.ImpostorRed;
        
        public static bool isInvisble = false;
        public static float invisibleTimer = 0f;

        public static float swoopCooldown = 30f;
        public static float swoopDuration = 5f;
        
        public static Sprite swoopButtonSprite;
        public static Sprite getSwoopButtonSprite()
            => swoopButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwoopButton.png", 100f);
        
        public static void clearAndReload() {
            swooper = null;
            isInvisble = false;
            invisibleTimer = 0f;
            swoopCooldown = CustomOptionHolder.swooperCooldown.getFloat();
            swoopDuration = CustomOptionHolder.swooperDuration.getFloat();
        }
    }

    public static class Mercenary {
        public static PlayerControl mercenary;
        public static PlayerControl shielded;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.65f, 0.61f, 0.58f, 1f);

        public static int attemptedMurder = 0;
        public static bool triggerMercenaryWin = false;

        public static int numOfNeededAttempts = 3;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShieldButton.png", 115f);
        
        public static void clearAndReload() {
            mercenary = null;
            shielded = null;
            currentTarget = null;
            attemptedMurder = 0;
            triggerMercenaryWin = false;
            numOfNeededAttempts = Mathf.RoundToInt(CustomOptionHolder.mercenaryNeededAttempts.getFloat());
        }
    }

    public static class Blackmailer {
        public static PlayerControl blackmailer;
        public static PlayerControl blackmailed;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;

        public static bool alreadyShook;

        public static float cooldown = 30f;

        private static Sprite overlaySprite;
        public static Sprite getBlackmailOverlaySprite()
            => overlaySprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailOverlay.png", 100f);

        private static Sprite blackmailButtonSprite;
        public static Sprite getBlackmailButtonSprite() 
            => blackmailButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailButton.png", 100f);

        public static void clearAndReload() {
            blackmailer = null;
            blackmailed = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.blackmailerCooldown.getFloat();
        }
    }

    public static class Escapist {
        public static PlayerControl escapist;
        public static Color color = Palette.ImpostorRed;

        public static float recallDuration = 0;
        public static Vector3? markedLocation = null;

        public static float markCooldown = 0;
        public static bool markStaysOverMeeting = false;
        
        private static Sprite markButtonSprite;
        public static Sprite getMarkButtonSprite()
            => markButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MarkButton.png", 100f);
        
        private static Sprite recallButtonSprite;
        public static Sprite getRecallButtonSprite()
            => recallButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RecallButton.png", 100f);
        
        public static void markLocation(Vector3 position) {
            markedLocation = position;
        }

        public static void clearAndReload() {
            escapist = null;
            recallDuration = CustomOptionHolder.escapistRecallDuration.getFloat();
            markCooldown = CustomOptionHolder.escapistMarkCooldown.getFloat();
            markStaysOverMeeting = CustomOptionHolder.escapistMarkStaysOverMeeting.getBool();
            markedLocation = null;   
        }
    }

    public static class Miner {
        public static PlayerControl miner;
        public static Color color = Palette.ImpostorRed;

        public static float placeVentCooldown = 30f;
        
        private static Sprite placeVentButtonSprite;
        public static Sprite getPlaceBoxButtonSprite()
            => placeVentButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MineButton.png", 100f);
        
        public static void clearAndReload() {
            miner = null;
            placeVentCooldown = CustomOptionHolder.minerPlaceVentCooldown.getFloat();
        }
    }

    public static class Cleaner {
        public static PlayerControl cleaner;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CleanButton.png", 100f);
        
        public static void clearAndReload() {
            cleaner = null;
            cooldown = CustomOptionHolder.cleanerCooldown.getFloat();
        }
    }

    public static class Trapper {
        public static PlayerControl trapper;
        public static Color color = new Color(0.65f, 0.82f, 0.7f, 1f);
        public static Material trapMaterial = TownOfUsPlugin.bundledAssets.Get<Material>("trap");
        
        public static List<Trap> traps = new List<Trap>();
        public static List<PlayerControl> trappedPlayers = new List<PlayerControl>();

        public static float cooldown = 30f;
        public static bool removeTraps = false;
        public static int maxTraps = 5;
        public static float minTimeInTrap = 0.5f;
        public static float trapSize = 0.25f;
        public static int minPlayersInTrap = 3;
        public static bool neutShowsEvil = false;
        public static bool kneutShowsEvil = false;

        private static Sprite trapButtonSprite;
        public static Sprite getButtonSprite()
            => trapButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.TrapButton.png", 100f);
        
        public static void clearAndReload() {
            trapper = null;
            trappedPlayers = new List<PlayerControl>();
            traps.ClearTraps();
            cooldown = CustomOptionHolder.trapperCooldown.getFloat();
            removeTraps = CustomOptionHolder.trapperTrapsRemoveOnNewRound.getBool();
            maxTraps = Mathf.RoundToInt(CustomOptionHolder.trapperMaxTraps.getFloat());
            minTimeInTrap = CustomOptionHolder.trapperMinAmountOfTimeInTrap.getFloat();
            trapSize = CustomOptionHolder.trapperTrapSize.getFloat();
            minPlayersInTrap = Mathf.RoundToInt(CustomOptionHolder.trapperMinAmountOfPlayersInTrap.getFloat());
            neutShowsEvil = CustomOptionHolder.trapperNeutShowsEvil.getBool();
            kneutShowsEvil = CustomOptionHolder.trapperKNeutShowsEvil.getBool();
        }
    }

    public static class Phantom {
        public static PlayerControl phantom;
        public static Color color = Palette.ImpostorRed;
        
        public static bool isInvisble = false;
        public static float ghostTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        
        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PhantomButton.png", 115f);
        
        public static void clearAndReload() {
            phantom = null;
            isInvisble = false;
            ghostTimer = 0f;
            cooldown = CustomOptionHolder.phantomInvisCooldown.getFloat();
            duration = CustomOptionHolder.phantomInvisDuration.getFloat();
        }
    }

    public static class Grenadier {
        public static PlayerControl grenadier;
        public static Color color = Palette.ImpostorRed;
        public static Color flash = new Color32(153, 153, 153, byte.MaxValue);
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> flashedPlayers = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int radius = 2;
        public static bool indicateFlashedCrewmates = false;
        
        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FlashButton.png", 100f);

        public static void clearAndReload() {
            grenadier = null;
            flashedPlayers.Clear();
            cooldown = CustomOptionHolder.grenadierCooldown.getFloat();
            duration = CustomOptionHolder.grenadierDuration.getFloat() + 0.5f;
            radius = CustomOptionHolder.grenadierFlashRadius.getSelection();
            indicateFlashedCrewmates = CustomOptionHolder.grenadierIndicateCrewmates.getBool();
        }
    }

    public static class Doomsayer {
        public static PlayerControl doomsayer;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> observedPlayers = new List<PlayerControl>();
        public static Color color = new Color(0f, 1f, 0.5f, 1f);

        public static int guessedToWin = 0;
        public static bool triggerDoomsayerWin = false;

        public static float cooldown = 30f;
        public static int guessesToWin = 3;
        
        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ObserveButton.png", 100f);

        public static void clearAndReload() {
            doomsayer = null;
            currentTarget = null;
            observedPlayers = new List<PlayerControl>();
            guessedToWin = 0;
            triggerDoomsayerWin = false;
            cooldown = CustomOptionHolder.doomsayerCooldown.getFloat();
            guessesToWin = Mathf.RoundToInt(CustomOptionHolder.doomsayerGuessesToWin.getFloat());
        }
    }

    public static class Guesser {
        public static PlayerControl niceGuesser;
        public static PlayerControl evilGuesser;
        public static Color color = new Color(1f, 1f, 0.6f, 1f);

        public static int remainingShotsEvilGuesser = 2;
        public static int remainingShotsNiceGuesser = 2;

        public static bool isGuesser (byte playerId) {
            if ((niceGuesser != null && niceGuesser.PlayerId == playerId) || (evilGuesser != null && evilGuesser.PlayerId == playerId)) return true;
            return false;
        }

        public static void clear (byte playerId) {
            if (niceGuesser != null && niceGuesser.PlayerId == playerId) niceGuesser = null;
            else if (evilGuesser != null && evilGuesser.PlayerId == playerId) evilGuesser = null;
        }

        public static int remainingShots(byte playerId, bool shoot = false) {
            int remainingShots = remainingShotsEvilGuesser;
            if (niceGuesser != null && niceGuesser.PlayerId == playerId) {
                remainingShots = remainingShotsNiceGuesser;
                if (shoot) remainingShotsNiceGuesser = Mathf.Max(0, remainingShotsNiceGuesser - 1);
            } else if (shoot) {
                remainingShotsEvilGuesser = Mathf.Max(0, remainingShotsEvilGuesser - 1);
            }
            return remainingShots;
        }

        public static void clearAndReload() {
            niceGuesser = null;
            evilGuesser = null;
            remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
            remainingShotsNiceGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
        }
    }

    public static class Lovers {
        public static PlayerControl lover1;
        public static PlayerControl lover2;
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);

        public static bool bothDie = true;
        public static bool enableChat = true;
        // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
        public static bool notAckedExiledIsLover = false;

        public static bool existing() {
            return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
        }

        public static bool existingAndAlive() {
            return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static PlayerControl otherLover(PlayerControl oneLover) {
            if (!existingAndAlive()) return null;
            if (oneLover == lover1) return lover2;
            if (oneLover == lover2) return lover1;
            return null;
        }

        public static bool existingWithKiller() {
            return existing() && (lover1.isKiller() || lover2.isKiller());
        }

        public static bool hasAliveKillingLover(this PlayerControl player) {
            if (!Lovers.existingAndAlive() || !existingWithKiller())
                return false;
            return (player != null && (player == lover1 || player == lover2));
        }

        public static void clearAndReload() {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
            enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
        }

        public static PlayerControl getPartner(this PlayerControl player) {
            if (player == null)
                return null;
            if (lover1 == player)
                return lover2;
            if (lover2 == player)
                return lover1;
            return null;
        }
    }

    public static class Blind {
        public static PlayerControl blind;

        public static void clearAndReload() {
            blind = null;
        }
    }

    public static class Bait {
        public static PlayerControl bait;
        public static Dictionary<DeadPlayer, float> active = new Dictionary<DeadPlayer, float>();
        
        public static float reportDelayMin = 0f;
        public static float reportDelayMax = 0f;

        public static void clearAndReload() {
            bait = null;
            active = new Dictionary<DeadPlayer, float>();
            reportDelayMin = CustomOptionHolder.modifierBaitReportDelayMin.getFloat();
            reportDelayMax = CustomOptionHolder.modifierBaitReportDelayMax.getFloat();
            if (reportDelayMin > reportDelayMax) reportDelayMin = reportDelayMax;
        }
    }

    public static class Sleuth {
        public static PlayerControl sleuth;
        public static List<PlayerControl> reported = new List<PlayerControl>();

        public static void clearAndReload() {
            sleuth = null;
            reported = new List<PlayerControl>();
        }
    }

    public static class Tiebreaker {
        public static PlayerControl tiebreaker;

        public static bool isTiebreak = false;

        public static void clearAndReload() {
            tiebreaker = null;
            isTiebreak = false;
        }
    }

    public static class ButtonBarry {
        public static PlayerControl buttonBarry;

        public static bool usedButton = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.EmergencyButton.png", 100f);

        public static void clearAndReload() {
            buttonBarry = null;
            usedButton = false;
        }
    }

    public static class Indomitable {
        public static PlayerControl indomitable;

        public static void clearAndReload() {
            indomitable = null;
        }
    }

    public static class Drunk {
        public static PlayerControl drunk;

        public static void clearAndReload() {
            drunk = null;
        }
    }

    public static class Sunglasses {
        public static PlayerControl sunglasses;

        public static int vision = 1;

        public static void clearAndReload() {
            sunglasses = null;
            vision = CustomOptionHolder.modifierSunglassesVision.getSelection() + 1;
        }
    }

    public static class Torch {
        public static PlayerControl torch;

        public static void clearAndReload() {
            torch = null;
        }
    }

    public static class DoubleShot {
        public static PlayerControl doubleShot;
        
        public static bool lifeUsed = false;

        public static void clearAndReload() {
            doubleShot = null;
        }
    }
}