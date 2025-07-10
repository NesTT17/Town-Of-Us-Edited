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

        public static void clearAndReloadRoles()
        {
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
            Mystic.clearAndReload();
            Tracker.clearAndReload();
            Werewolf.clearAndReload();
            Detective.clearAndReload();
            Glitch.clearAndReload();
            Venerer.clearAndReload();
            BountyHunter.clearAndReload();
            Oracle.clearAndReload();
            Bomber.clearAndReload();
            VampireHunter.clearAndReload();
            TimeLord.clearAndReload();

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
            Disperser.clearAndReload();
            Armored.clearAndReload();

            // Gamemodes
            HandleGuesser.clearAndReload();
        }
    }

    public static class Morphling
    {
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

        public static void resetMorph()
        {
            morphTarget = null;
            morphTimer = 0f;
            if (morphling == null) return;
            morphling.setDefaultLook();
        }

        public static void clearAndReload()
        {
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

    public static class Camouflager
    {
        public static PlayerControl camouflager;
        public static Color color = Palette.ImpostorRed;

        public static bool camoComms;
        public static float camouflageTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamoButton.png", 115f);

        public static void resetCamouflage()
        {
            if (Helpers.isCamoComms()) return;
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == Swooper.swooper && Swooper.isInvisble || p == Phantom.phantom && Phantom.isInvisble)
                    continue;
                p.setDefaultLook();
                camoComms = false;
            }
        }

        public static void clearAndReload()
        {
            resetCamouflage();
            camouflager = null;
            camouflageTimer = 0f;
            camoComms = false;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
            duration = CustomOptionHolder.camouflagerDuration.getFloat();
        }
    }

    public static class Snitch
    {
        public static PlayerControl snitch;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.83f, 0.69f, 0.22f, 1f);

        public static int taskCountForReveal = 1;
        public static bool includeNeutral = false;
        public static bool includeKillingNeutral = false;
        public static bool seeInMeeting = false;

        public static void clearAndReload()
        {
            if (localArrows != null)
            {
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

    public static class Engineer
    {
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

        public static void clearAndReload()
        {
            engineer = null;
            cooldown = CustomOptionHolder.engineerDoorsCooldown.getFloat();
            remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        }
    }

    public static class Sheriff
    {
        public static PlayerControl sheriff;
        public static PlayerControl currentTarget;
        public static Color color = Color.yellow;

        public static float cooldown = 30f;
        public static bool canKillNeutrals = false;
        public static bool canKillKNeutrals = false;

        public static void clearAndReload()
        {
            sheriff = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
            canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
            canKillKNeutrals = CustomOptionHolder.sheriffCanKillKNeutrals.getBool();
        }
    }

    public static class Jester
    {
        public static PlayerControl jester;
        public static Color color = new Color(1f, 0.75f, 0.8f, 1f);

        public static bool triggerJesterWin = false;

        public static bool canCallEmergency = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload()
        {
            jester = null;
            triggerJesterWin = false;
            canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
            hasImpostorVision = CustomOptionHolder.jesterHasImpostorVision.getBool();
        }
    }

    public static class Shifter
    {
        public static PlayerControl shifter;
        public static PlayerControl futureShift;
        public static PlayerControl currentTarget;
        public static Color color = Color.gray;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShiftButton.png", 115f);

        public static void clearAndReload()
        {
            shifter = null;
            currentTarget = null;
            futureShift = null;
        }
    }

    public static class Spy
    {
        public static PlayerControl spy;
        public static Color color = new Color(0.8f, 0.64f, 0.8f, 1f);

        public static void clearAndReload()
        {
            spy = null;
        }
    }

    public static class Swapper
    {
        public static PlayerControl swapper;
        public static Color color = new Color(0.4f, 0.9f, 0.4f, 1f);

        public static byte playerId1 = Byte.MaxValue;
        public static byte playerId2 = Byte.MaxValue;

        public static bool canCallEmergency = false;
        public static bool canOnlySwapOthers = false;

        private static Sprite spriteCheck;
        public static Sprite getCheckSprite()
            => spriteCheck ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwapperCheck.png", 150f);

        public static void clearAndReload()
        {
            swapper = null;
            playerId1 = Byte.MaxValue;
            playerId2 = Byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
            canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        }
    }

    public static class Mayor
    {
        public static PlayerControl mayor;
        public static Color color = new Color(0.44f, 0.31f, 0.66f, 1f);

        public static bool isRevealed = false;

        private static Sprite spriteCheck;
        public static Sprite getCheckSprite()
            => spriteCheck ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MayorReveal.png", 100f);

        public static void clearAndReload()
        {
            mayor = null;
            isRevealed = false;
        }
    }

    public static class Medic
    {
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

        public static bool shieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
            bool isMorphedGlitch = target == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
            bool isMorphedVenerer = target == Venerer.venerer && Venerer.morphTimer > 0f;

            if (shielded != null && ((target == shielded && !isMorphedMorphling && !isMorphedGlitch && !isMorphedVenerer) || (isMorphedMorphling && Morphling.morphTarget == shielded) || (isMorphedGlitch && Glitch.morphPlayer == shielded)))
            {
                hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || showShielded == 1 & PlayerControl.LocalPlayer == shielded // Shielded
                    || showShielded == 2 & PlayerControl.LocalPlayer == medic // Medic
                    || showShielded == 3 && (PlayerControl.LocalPlayer == shielded || PlayerControl.LocalPlayer == medic);
            }
            return hasVisibleShield;
        }

        public static void clearAndReload()
        {
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

    public static class Dracula
    {
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

        public static void removeCurrentDracula()
        {
            if (!formerDraculas.Any(x => x.PlayerId == dracula.PlayerId)) formerDraculas.Add(dracula);
            dracula = null;
            currentTarget = null;
            fakeVampire = null;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
        }

        public static void clearAndReload()
        {
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

    public static class Vampire
    {
        public static PlayerControl vampire;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);

        public static bool wasTeamRed;
        public static bool wasImpostor;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload()
        {
            vampire = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.vampireCanUseVents.getBool();
            hasImpostorVision = CustomOptionHolder.draculaAndVampireHaveImpostorVision.getBool();
            wasTeamRed = wasImpostor = false;
        }
    }

    public static class Poisoner
    {
        public static PlayerControl poisoner;
        public static PlayerControl poisoned;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;
        public static float delay = 10f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PoisonButton.png", 100f);

        public static void clearAndReload()
        {
            poisoner = null;
            poisoned = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.poisonerCooldown.getFloat();
            delay = CustomOptionHolder.poisonerKillDelay.getFloat();
        }
    }

    public static class Scavenger
    {
        public static PlayerControl scavenger;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.6f, 0.15f, 0.15f, 1f);

        public static int eatenBodies = 0;
        public static bool triggerScavengerWin = false;

        public static float cooldown = 30f;
        public static int scavengerNumberToWin = 4;
        public static bool canUseVents = true;
        public static bool showArrows = true;
        public static bool canEatWithGuess = true;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DevourButton.png", 100f);

        public static void clearAndReload()
        {
            scavenger = null;
            eatenBodies = 0;
            triggerScavengerWin = false;
            cooldown = CustomOptionHolder.scavengerCooldown.getFloat();
            scavengerNumberToWin = Mathf.RoundToInt(CustomOptionHolder.scavengerNumberToWin.getFloat());
            canUseVents = CustomOptionHolder.scavengerCanUseVents.getBool();
            showArrows = CustomOptionHolder.scavengerShowArrows.getBool();
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            canEatWithGuess = CustomOptionHolder.scavengerCanEatWithGuess.getBool();
        }
    }

    public static class Executioner
    {
        public static PlayerControl executioner;
        public static PlayerControl target;
        public static Color color = new Color(0.55f, 0.25f, 0.02f, 1f);

        public static bool targetWasGuessed = false;
        public static bool triggerExecutionerWin = false;

        public static float vision = 1f;
        public static bool canCallEmergency = true;

        public static void clearAndReload(bool clearTarget = true)
        {
            executioner = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            triggerExecutionerWin = false;
            vision = CustomOptionHolder.executionerVision.getFloat();
            canCallEmergency = CustomOptionHolder.executionerCanCallEmergency.getBool();
        }
    }

    public static class Lawyer
    {
        public static PlayerControl lawyer;
        public static PlayerControl target;
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);

        public static bool targetWasGuessed = false;

        public static float vision = 1f;
        public static bool lawyerKnowsRole = false;
        public static bool canCallEmergency = true;
        public static bool targetCanBeJester = false;

        public static void clearAndReload(bool clearTarget = true)
        {
            lawyer = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            vision = CustomOptionHolder.lawyerVision.getFloat();
            lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
            targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
            canCallEmergency = CustomOptionHolder.lawyerCanCallEmergency.getBool();
        }
    }

    public static class Pursuer
    {
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

        public static void clearAndReload()
        {
            pursuer = null;
            target = null;
            blankedList = new List<PlayerControl>();
            blanks = 0;
            notAckedExiled = false;
            cooldown = CustomOptionHolder.pursuerCooldown.getFloat();
            blanksNumber = Mathf.RoundToInt(CustomOptionHolder.pursuerBlanksNumber.getFloat());
        }
    }

    public static class GuardianAngel
    {
        public static PlayerControl guardianAngel;
        public static PlayerControl target;
        public static Color color = new Color(0.7f, 1f, 1f, 1f);

        public static bool promoteToFA = false;

        public static bool protectActive = false;
        public static bool targetWasGuessed = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int remainingProtects = 3;
        public static int showProtected = 0; // 0 - Everyone, 1 - Protected, 2 - GA, 3 - GA+Protected, 4 - Nobody

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ProtectButton.png", 100f);

        public static bool protectVisible(PlayerControl player)
        {
            bool hasVisibleProtect = false;
            bool isMorphedMorphling = player == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
            bool isMorphedGlitch = target == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
            bool isMorphedVenerer = target == Venerer.venerer && Venerer.morphTimer > 0f;

            if (target != null && ((player == target && !isMorphedMorphling && !isMorphedGlitch && !isMorphedVenerer) || (isMorphedMorphling && Morphling.morphTarget == target) || (isMorphedGlitch && Glitch.morphPlayer == target)))
            {
                hasVisibleProtect = showProtected == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || showProtected == 1 & PlayerControl.LocalPlayer == target // Protected
                    || showProtected == 2 & PlayerControl.LocalPlayer == guardianAngel // GA
                    || showProtected == 3 && (PlayerControl.LocalPlayer == target || PlayerControl.LocalPlayer == guardianAngel);
            }
            return hasVisibleProtect;
        }

        public static void clearAndReload(bool clearTarget = true)
        {
            guardianAngel = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            protectActive = false;
            cooldown = CustomOptionHolder.guardianAngelProtectCooldown.getFloat();
            duration = CustomOptionHolder.guardianAngelProtectDuration.getFloat();
            showProtected = CustomOptionHolder.guardianAngelShowProtected.getSelection();
            remainingProtects = Mathf.RoundToInt(CustomOptionHolder.guardianAngelNumberOfProtects.getFloat());
        }
    }

    public static class FallenAngel
    {
        public static PlayerControl fallenAngel;
        public static PlayerControl target;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.6f, 1f, 1f, 1f);

        public static float cooldown = 30f;

        public static void clearAndReload(bool clearTarget = true)
        {
            fallenAngel = null;
            if (clearTarget) target = null;
            currentTarget = null;
            cooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        }
    }

    public static class Survivor
    {
        public static PlayerControl survivor;
        public static Color color = new Color(1f, 0.9f, 0.3f, 1f);

        public static bool safeguardActive = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int remainingSafeguards = 3;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SafeguardButton.png", 100f);

        public static void clearAndReload(bool clearTarget = true)
        {
            survivor = null;
            safeguardActive = false;
            cooldown = CustomOptionHolder.survivorSafeguardCooldown.getFloat();
            duration = CustomOptionHolder.survivorSafeguardDuration.getFloat();
            remainingSafeguards = Mathf.RoundToInt(CustomOptionHolder.survivorNumberOfSafeguards.getFloat());
        }
    }

    public static class Amnesiac
    {
        public static PlayerControl amnesiac;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.5f, 0.7f, 1f, 1f);

        public static bool showArrows = false;
        public static float delay = 3f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RememberButton.png", 100f);

        public static void clearAndReload()
        {
            amnesiac = null;
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            showArrows = CustomOptionHolder.amnesiacShowArrows.getBool();
            delay = CustomOptionHolder.amnesiacDelay.getFloat();
        }
    }

    public static class Investigator
    {
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

        public static void clearAndReload()
        {
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

    public static class Veteran
    {
        public static PlayerControl veteran;
        public static Color color = new Color(0.6f, 0.5f, 0.25f, 1f);

        public static bool isAlertActive = false;

        public static float cooldown = 30f;
        public static float duration = 5f;
        public static int remainingAlerts = 5;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.AlertButton.png", 100f);

        public static void clearAndReload()
        {
            veteran = null;
            isAlertActive = false;
            cooldown = CustomOptionHolder.veteranCooldown.getFloat();
            duration = CustomOptionHolder.veteranDuration.getFloat();
            remainingAlerts = Mathf.RoundToInt(CustomOptionHolder.veteranNumberOfAlerts.getFloat());
        }
    }

    public static class Seer
    {
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

        public static void clearAndReload()
        {
            seer = null;
            currentTarget = null;
            revealedPlayers = new List<PlayerControl>();
            cooldown = CustomOptionHolder.seerCooldown.getFloat();
            neutRed = CustomOptionHolder.seerNeutRed.getBool();
            kneutRed = CustomOptionHolder.seerKNeutRed.getBool();
        }
    }

    public static class Juggernaut
    {
        public static PlayerControl juggernaut;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.55f, 0f, 0.3f, 1f);

        public static float cooldown = 30f;
        public static float cooldownReduce = 10f;
        public static bool hasImpostorVision = false;
        public static bool canUseVents = false;

        public static void clearAndReload()
        {
            juggernaut = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.juggernautCooldown.getFloat();
            cooldownReduce = CustomOptionHolder.juggernautCooldownReduce.getFloat();
            hasImpostorVision = CustomOptionHolder.juggernautHasImpostorVision.getBool();
            canUseVents = CustomOptionHolder.juggernautCanVent.getBool();
        }
    }

    public static class Swooper
    {
        public static PlayerControl swooper;
        public static Color color = Palette.ImpostorRed;

        public static bool isInvisble = false;
        public static float invisibleTimer = 0f;

        public static float swoopCooldown = 30f;
        public static float swoopDuration = 5f;

        public static Sprite swoopButtonSprite;
        public static Sprite getSwoopButtonSprite()
            => swoopButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwoopButton.png", 100f);

        public static void clearAndReload()
        {
            swooper = null;
            isInvisble = false;
            invisibleTimer = 0f;
            swoopCooldown = CustomOptionHolder.swooperCooldown.getFloat();
            swoopDuration = CustomOptionHolder.swooperDuration.getFloat();
        }
    }

    public static class Mercenary
    {
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

        public static void clearAndReload()
        {
            mercenary = null;
            shielded = null;
            currentTarget = null;
            attemptedMurder = 0;
            triggerMercenaryWin = false;
            numOfNeededAttempts = Mathf.RoundToInt(CustomOptionHolder.mercenaryNeededAttempts.getFloat());
        }
    }

    public static class Blackmailer
    {
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

        public static void clearAndReload()
        {
            blackmailer = null;
            blackmailed = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.blackmailerCooldown.getFloat();
        }
    }

    public static class Escapist
    {
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

        public static void markLocation(Vector3 position)
        {
            markedLocation = position;
        }

        public static void clearAndReload()
        {
            escapist = null;
            recallDuration = CustomOptionHolder.escapistRecallDuration.getFloat();
            markCooldown = CustomOptionHolder.escapistMarkCooldown.getFloat();
            markStaysOverMeeting = CustomOptionHolder.escapistMarkStaysOverMeeting.getBool();
            markedLocation = null;
        }
    }

    public static class Miner
    {
        public static PlayerControl miner;
        public static Color color = Palette.ImpostorRed;

        public static float placeVentCooldown = 30f;

        private static Sprite placeVentButtonSprite;
        public static Sprite getPlaceBoxButtonSprite()
            => placeVentButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MineButton.png", 100f);

        public static void clearAndReload()
        {
            miner = null;
            placeVentCooldown = CustomOptionHolder.minerPlaceVentCooldown.getFloat();
        }
    }

    public static class Cleaner
    {
        public static PlayerControl cleaner;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CleanButton.png", 100f);

        public static void clearAndReload()
        {
            cleaner = null;
            cooldown = CustomOptionHolder.cleanerCooldown.getFloat();
        }
    }

    public static class Trapper
    {
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

        public static void clearAndReload()
        {
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

    public static class Phantom
    {
        public static PlayerControl phantom;
        public static Color color = Palette.ImpostorRed;

        public static bool isInvisble = false;
        public static float ghostTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PhantomButton.png", 115f);

        public static void clearAndReload()
        {
            phantom = null;
            isInvisble = false;
            ghostTimer = 0f;
            cooldown = CustomOptionHolder.phantomInvisCooldown.getFloat();
            duration = CustomOptionHolder.phantomInvisDuration.getFloat();
        }
    }

    public static class Grenadier
    {
        public static PlayerControl grenadier;
        public static Color color = Palette.ImpostorRed;
        public static Color flash = new Color32(153, 153, 153, byte.MaxValue);
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> flashedPlayers = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

        public static float flashTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static int radius = 2;
        public static bool indicateFlashedCrewmates = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FlashButton.png", 100f);

        public static void clearAndReload()
        {
            grenadier = null;
            flashedPlayers.Clear();
            flashTimer = 0f;
            cooldown = CustomOptionHolder.grenadierCooldown.getFloat();
            duration = CustomOptionHolder.grenadierDuration.getFloat() + 0.5f;
            radius = CustomOptionHolder.grenadierFlashRadius.getSelection();
            indicateFlashedCrewmates = CustomOptionHolder.grenadierIndicateCrewmates.getBool();
        }
    }

    public static class Doomsayer
    {
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

        public static void clearAndReload()
        {
            doomsayer = null;
            currentTarget = null;
            observedPlayers = new List<PlayerControl>();
            guessedToWin = 0;
            triggerDoomsayerWin = false;
            cooldown = CustomOptionHolder.doomsayerCooldown.getFloat();
            guessesToWin = Mathf.RoundToInt(CustomOptionHolder.doomsayerGuessesToWin.getFloat());
        }
    }

    public static class Mystic
    {
        public static PlayerControl mystic;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.3f, 0.6f, 0.9f, 1f);

        public static float duration = 1f;

        public static void clearAndReload()
        {
            mystic = null;
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            duration = CustomOptionHolder.mysticArrowDuration.getFloat();
        }
    }

    public static class Tracker
    {
        public static PlayerControl tracker;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> trackedPlayers = new List<PlayerControl>();
        public static Color color = new Color(0f, 0.6f, 0f, 1f);

        public static int tracksInRound = 0;

        public static float cooldown = 30f;
        public static int maxTracksPerRound = 3;
        public static bool resetAfterMeeting = false;

        public static void resetTracked()
        {
            trackedPlayers = new List<PlayerControl>();
            tracksInRound = 0;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.TrackButton.png", 100f);

        public static void clearAndReload()
        {
            tracker = null;
            currentTarget = null;
            resetTracked();
            cooldown = CustomOptionHolder.trackerCooldown.getFloat();
            maxTracksPerRound = Mathf.RoundToInt(CustomOptionHolder.trackerMaxTracksPerRound.getFloat());
            resetAfterMeeting = CustomOptionHolder.trackerResetAfterMeeting.getBool();
        }
    }

    public static class Werewolf
    {
        public static PlayerControl werewolf;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.66f, 0.4f, 0.16f, 1f);

        public static bool isRampageActive = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float killCooldown = 3f;
        public static bool canVent = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RampageButton.png", 100f);

        public static void clearAndReload()
        {
            werewolf = null;
            currentTarget = null;
            isRampageActive = false;
            cooldown = CustomOptionHolder.werewolfRampageCooldown.getFloat();
            duration = CustomOptionHolder.werewolfRampageDuration.getFloat();
            killCooldown = CustomOptionHolder.werewolfKillCooldown.getFloat();
            canVent = CustomOptionHolder.werewolfCanVent.getBool();
        }
    }

    public static class Detective
    {
        public static PlayerControl detective;
        public static PlayerControl examined;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.3f, 0.3f, 1f, 1f);

        public static float initialCooldown = 30f;
        public static float cooldown = 10f;
        public static float bloodTime = 30f;
        public static bool getInfoOnReport = false;
        public static float reportRoleDuration = 15f;
        public static float reportFactionDuration = 30f;
        public static bool getExamineInfo = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ExamineButton.png", 100f);

        public static void clearAndReload()
        {
            detective = null;
            examined = null;
            currentTarget = null;
            initialCooldown = CustomOptionHolder.detectiveInitialCooldown.getFloat();
            cooldown = CustomOptionHolder.detectiveCooldown.getFloat();
            bloodTime = CustomOptionHolder.detectiveBloodTime.getFloat();
            getInfoOnReport = CustomOptionHolder.detectiveGetInfoOnReport.getBool();
            reportRoleDuration = CustomOptionHolder.detectiveReportRoleDuration.getFloat();
            reportFactionDuration = CustomOptionHolder.detectiveReportFactionDuration.getFloat();
            getExamineInfo = CustomOptionHolder.detectiveGetExamineInfo.getBool();
        }
    }

    public static class Glitch
    {
        public static PlayerControl glitch;
        public static PlayerControl morphPlayer;
        public static PlayerControl sampledPlayer;
        public static PlayerControl hackedPlayer;
        public static PlayerControl currentTarget;
        public static List<byte> mimicTargets = new List<byte>();
        public static Dictionary<PlayerControl, DateTime> hackedPlayers = new Dictionary<PlayerControl, DateTime>();
        public static Color color = Color.green;

        public static float morphTimer = 0f;

        public static float morphCooldown = 30f;
        public static float morphDuration = 10f;
        public static float hackCooldown = 30f;
        public static float hackDuration = 10f;
        public static float killCooldown = 30f;
        public static bool canVent = false;
        public static bool hasImpostorVision = false;

        private static Sprite mimicButtonSprite;
        public static Sprite getMimicButtonSprite()
            => mimicButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MimicButton.png", 100f);

        private static Sprite hackButtonSprite;
        public static Sprite getHackButtonSprite()
            => hackButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.HackButton.png", 100f);

        public static void resetMorph()
        {
            morphPlayer = null;
            morphTimer = 0f;
            if (glitch == null) return;
            glitch.setDefaultLook();
        }

        public static void clearAndReload()
        {
            glitch = null;
            resetMorph();
            sampledPlayer = null;
            hackedPlayer = null;
            currentTarget = null;
            mimicTargets = new List<byte>();
            hackedPlayers = new Dictionary<PlayerControl, DateTime>();
            morphCooldown = CustomOptionHolder.glitchMimicCooldown.getFloat();
            morphDuration = CustomOptionHolder.glitchMimicDuration.getFloat();
            hackCooldown = CustomOptionHolder.glitchHackCooldown.getFloat();
            hackDuration = CustomOptionHolder.glitchHackDuration.getFloat();
            killCooldown = CustomOptionHolder.glitchKillCooldown.getFloat();
            canVent = CustomOptionHolder.glitchCanVent.getBool();
            hasImpostorVision = CustomOptionHolder.glitchHasImpostorVision.getBool();
        }
    }

    public static class Venerer
    {
        public static PlayerControl venerer;
        public static Color color = Palette.ImpostorRed;

        public static int numberOfKills = 0;
        public static float morphTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float speedMultiplier = 1.25f;
        public static float freezeMultiplier = 0.75f;

        private static Sprite noAbilitiesButton;
        public static Sprite getnoAbilitiesButton()
            => noAbilitiesButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.NoAbilitiesButton.png", 100f);

        private static Sprite camoButton;
        public static Sprite getcamoButton()
            => camoButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamouflageButton.png", 100f);

        private static Sprite speedButton;
        public static Sprite getspeedButton()
            => speedButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SpeedButton.png", 100f);

        private static Sprite freezeButton;
        public static Sprite getfreezeButton()
            => freezeButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FreezeButton.png", 100f);

        public static void resetMorph()
        {
            morphTimer = 0f;
            if (venerer == null) return;
            venerer.setDefaultLook();
        }

        public static void clearAndReload()
        {
            venerer = null;
            resetMorph();
            numberOfKills = 0;
            cooldown = CustomOptionHolder.venererCooldown.getFloat();
            duration = CustomOptionHolder.venererDuration.getFloat();
            speedMultiplier = CustomOptionHolder.venererSpeedMultiplier.getFloat();
            freezeMultiplier = CustomOptionHolder.venererFreezeMultiplier.getFloat();
        }
    }

    public static class BountyHunter
    {
        public static PlayerControl bountyHunter;
        public static PlayerControl bounty;
        public static TMPro.TextMeshPro cooldownText;
        public static Arrow arrow;
        public static Color color = Palette.ImpostorRed;

        public static float arrowUpdateTimer = 0f;
        public static float bountyUpdateTimer = 0f;

        public static float bountyDuration = 30f;
        public static bool showArrow = true;
        public static float bountyKillCooldown = 0f;
        public static float punishmentTime = 15f;
        public static float arrowUpdateIntervall = 10f;

        public static void clearAndReload()
        {
            arrow = new Arrow(Palette.EnabledColor);
            bountyHunter = null;
            bounty = null;
            arrowUpdateTimer = 0f;
            bountyUpdateTimer = 0f;
            if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = null;
            if (cooldownText != null && cooldownText.gameObject != null) UnityEngine.Object.Destroy(cooldownText.gameObject);
            cooldownText = null;
            foreach (PoolablePlayer p in TOUMapOptions.playerIcons.Values)
            {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            bountyDuration = CustomOptionHolder.bountyHunterBountyDuration.getFloat();
            bountyKillCooldown = CustomOptionHolder.bountyHunterReducedCooldown.getFloat();
            punishmentTime = CustomOptionHolder.bountyHunterPunishmentTime.getFloat();
            showArrow = CustomOptionHolder.bountyHunterShowArrow.getBool();
            arrowUpdateIntervall = CustomOptionHolder.bountyHunterArrowUpdateIntervall.getFloat();
        }
    }

    public static class Oracle
    {
        public static PlayerControl oracle;
        public static PlayerControl confessor;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.75f, 0f, 0.75f, 1f);

        public static FactionId revealedFaction;

        public static int accuracy = 10;
        public static float cooldown = 30f;
        public static bool neutShowsEvil = false;
        public static bool kneutShowsEvil = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ConfessButton.png", 100f);

        public static void clearAndReload()
        {
            oracle = null;
            confessor = null;
            currentTarget = null;
            revealedFaction = FactionId.Crewmate;
            accuracy = CustomOptionHolder.oracleAccuracy.getSelection();
            cooldown = CustomOptionHolder.oracleCooldown.getFloat();
            neutShowsEvil = CustomOptionHolder.oracleNeutShowsEvil.getBool();
            kneutShowsEvil = CustomOptionHolder.oracleKNeutShowsEvil.getBool();
        }
    }

    public static class Bomber
    {
        public static PlayerControl bomber;
        public static Color color = Palette.ImpostorRed;
        public static Material bombMaterial = TownOfUsPlugin.bundledAssets.Get<Material>("bomb");

        public static Vector3 DetonatePoint;
        public static Bomb Bomb = new Bomb();

        public static float delay = 10f;
        public static int maxKills = 3;
        public static float radius = 1f;


        private static Sprite plantButtonSprite;
        public static Sprite getPlantButtonSprite()
            => plantButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PlantButton.png", 100f);

        private static Sprite detonateButtonSprite;
        public static Sprite getDetonateButtonSprite()
            => detonateButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DetonateButton.png", 100f);

        public static void clearAndReload()
        {
            bomber = null;
            Bomb = new Bomb();
            delay = CustomOptionHolder.bomberDetonateDelay.getFloat();
            maxKills = Mathf.RoundToInt(CustomOptionHolder.bomberMaxKillsInDetonation.getFloat());
            radius = CustomOptionHolder.bomberDetonateRadius.getFloat();
        }
    }

    public static class VampireHunter
    {
        // Vampire Hunter
        public static PlayerControl vampireHunter;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.7f, 0.7f, 0.9f, 1f);

        public static int usedStakes = 0;
        public static bool canStake = true;

        public static float stakeCooldown = 30f;
        public static int maxFailedStakes = 3;
        public static bool canStakeRoundOne = false;
        public static bool selfKillAfterFinalStake = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.StakeButton.png", 100f);

        public static void clearAndReloadVampireHunter()
        {
            vampireHunter = null;
            currentTarget = null;
            usedStakes = 0;
            canStake = true;
            stakeCooldown = CustomOptionHolder.vampireHunterCooldown.getFloat();
            maxFailedStakes = Mathf.RoundToInt(CustomOptionHolder.vampireHunterMaxFailedStakes.getFloat());
            canStakeRoundOne = CustomOptionHolder.vampireHunterCanStakeRoundOne.getBool();
            selfKillAfterFinalStake = CustomOptionHolder.vampireHunterSelfKillAfterFinalStake.getBool();
        }

        // Veteran
        public static PlayerControl veteran;

        public static bool isAlertActive = false;

        public static float cooldown = 30f;
        public static float duration = 5f;
        public static int remainingAlerts = 5;

        public static void clearAndReloadVeteran()
        {
            veteran = null;
            isAlertActive = false;
            cooldown = CustomOptionHolder.veteranCooldown.getFloat();
            duration = CustomOptionHolder.veteranDuration.getFloat();
            remainingAlerts = Mathf.RoundToInt(CustomOptionHolder.veteranNumberOfAlerts.getFloat());
        }

        public static void clearAndReload()
        {
            clearAndReloadVeteran();
            clearAndReloadVampireHunter();
        }
    }

    public static class Guesser
    {
        public static PlayerControl niceGuesser;
        public static PlayerControl evilGuesser;
        public static Color color = new Color(1f, 1f, 0.6f, 1f);

        public static int remainingShotsEvilGuesser = 2;
        public static int remainingShotsNiceGuesser = 2;

        public static bool isGuesser(byte playerId)
        {
            if ((niceGuesser != null && niceGuesser.PlayerId == playerId) || (evilGuesser != null && evilGuesser.PlayerId == playerId)) return true;
            return false;
        }

        public static void clear(byte playerId)
        {
            if (niceGuesser != null && niceGuesser.PlayerId == playerId) niceGuesser = null;
            else if (evilGuesser != null && evilGuesser.PlayerId == playerId) evilGuesser = null;
        }

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            int remainingShots = remainingShotsEvilGuesser;
            if (niceGuesser != null && niceGuesser.PlayerId == playerId)
            {
                remainingShots = remainingShotsNiceGuesser;
                if (shoot) remainingShotsNiceGuesser = Mathf.Max(0, remainingShotsNiceGuesser - 1);
            }
            else if (shoot)
            {
                remainingShotsEvilGuesser = Mathf.Max(0, remainingShotsEvilGuesser - 1);
            }
            return remainingShots;
        }

        public static void clearAndReload()
        {
            niceGuesser = null;
            evilGuesser = null;
            remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
            remainingShotsNiceGuesser = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
        }
    }

    public static class Lovers
    {
        public static PlayerControl lover1;
        public static PlayerControl lover2;
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);

        public static bool bothDie = true;
        public static bool enableChat = true;
        // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
        public static bool notAckedExiledIsLover = false;

        public static bool existing()
        {
            return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
        }

        public static bool existingAndAlive()
        {
            return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static PlayerControl otherLover(PlayerControl oneLover)
        {
            if (!existingAndAlive()) return null;
            if (oneLover == lover1) return lover2;
            if (oneLover == lover2) return lover1;
            return null;
        }

        public static bool existingWithKiller()
        {
            return existing() && (lover1.isKiller() || lover2.isKiller());
        }

        public static bool hasAliveKillingLover(this PlayerControl player)
        {
            if (!Lovers.existingAndAlive() || !existingWithKiller())
                return false;
            return (player != null && (player == lover1 || player == lover2));
        }

        public static void clearAndReload()
        {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
            enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
        }

        public static PlayerControl getPartner(this PlayerControl player)
        {
            if (player == null)
                return null;
            if (lover1 == player)
                return lover2;
            if (lover2 == player)
                return lover1;
            return null;
        }
    }

    public static class Blind
    {
        public static PlayerControl blind;

        public static void clearAndReload()
        {
            blind = null;
        }
    }

    public static class Bait
    {
        public static PlayerControl bait;
        public static Dictionary<DeadPlayer, float> active = new Dictionary<DeadPlayer, float>();

        public static float reportDelayMin = 0f;
        public static float reportDelayMax = 0f;

        public static void clearAndReload()
        {
            bait = null;
            active = new Dictionary<DeadPlayer, float>();
            reportDelayMin = CustomOptionHolder.modifierBaitReportDelayMin.getFloat();
            reportDelayMax = CustomOptionHolder.modifierBaitReportDelayMax.getFloat();
            if (reportDelayMin > reportDelayMax) reportDelayMin = reportDelayMax;
        }
    }

    public static class Sleuth
    {
        public static PlayerControl sleuth;
        public static List<PlayerControl> reported = new List<PlayerControl>();

        public static void clearAndReload()
        {
            sleuth = null;
            reported = new List<PlayerControl>();
        }
    }

    public static class Tiebreaker
    {
        public static PlayerControl tiebreaker;

        public static bool isTiebreak = false;

        public static void clearAndReload()
        {
            tiebreaker = null;
            isTiebreak = false;
        }
    }

    public static class ButtonBarry
    {
        public static PlayerControl buttonBarry;

        public static bool usedButton = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.EmergencyButton.png", 100f);

        public static void clearAndReload()
        {
            buttonBarry = null;
            usedButton = false;
        }
    }

    public static class Indomitable
    {
        public static PlayerControl indomitable;

        public static void clearAndReload()
        {
            indomitable = null;
        }
    }

    public static class Drunk
    {
        public static PlayerControl drunk;

        public static void clearAndReload()
        {
            drunk = null;
        }
    }

    public static class Sunglasses
    {
        public static PlayerControl sunglasses;

        public static int vision = 1;

        public static void clearAndReload()
        {
            sunglasses = null;
            vision = CustomOptionHolder.modifierSunglassesVision.getSelection() + 1;
        }
    }

    public static class Torch
    {
        public static PlayerControl torch;

        public static void clearAndReload()
        {
            torch = null;
        }
    }

    public static class DoubleShot
    {
        public static PlayerControl doubleShot;

        public static bool lifeUsed = false;

        public static void clearAndReload()
        {
            doubleShot = null;
            lifeUsed = false;
        }
    }

    public static class Disperser
    {
        public static PlayerControl disperser;

        public static bool isButtonUsed = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DisperseButton.png", 100f);

        public static void clearAndReload()
        {
            disperser = null;
            isButtonUsed = false;
        }
    }

    public static class Armored
    {
        public static PlayerControl armored;

        public static bool isBrokenArmor = false;

        public static void clearAndReload()
        {
            armored = null;
            isBrokenArmor = false;
        }
    }

    public static class TimeLord
    {
        public static PlayerControl timeLord;
        public static Color color = new Color(0f, 0f, 1f, 1f);

        public static bool shieldActive = false;
        public static bool isRewinding = false;

        public static float rewindTime = 3f;
        public static float cooldown = 30f;
        public static float rewindCooldown = 30f;
        public static bool canUseRewind = true;
        public static float shieldDuration = 3f;
        public static bool reviveDuringRewind = false;
        
        private static Sprite buttonSprite;
        public static Sprite getTimeShieldButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.TimeShieldButton.png", 115f);
            
        private static Sprite twoButtonSprite;
        public static Sprite getRewindButtonSprite()
            => twoButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RewindButton.png", 100f);

        public static void clearAndReload()
        {
            timeLord = null;
            shieldActive = false;
            isRewinding = false;
            rewindTime = CustomOptionHolder.timeLordRewindTime.getFloat();
            shieldDuration = CustomOptionHolder.timeLordShieldDuration.getFloat();
            cooldown = CustomOptionHolder.timeLordCooldown.getFloat();
            reviveDuringRewind = CustomOptionHolder.timeLordReviveDuringRewind.getBool();
            rewindCooldown = CustomOptionHolder.timeLordRewindCooldown.getFloat();
            canUseRewind = CustomOptionHolder.timeLordCanRewind.getBool();
        }

        public static void RewindTime()
        {
            shieldActive = false; // Shield is no longer active when rewinding
            if (TimeLord.timeLord != null && TimeLord.timeLord == PlayerControl.LocalPlayer)
            {
                HudManagerStartPatch.resetTimeLordButton();
            }

            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeLord.rewindTime / 2, new Action<float>((p) =>
            {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

            if (TimeLord.timeLord == null || PlayerControl.LocalPlayer == TimeLord.timeLord) return; // Time Lord himself does not rewind

            TimeLord.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();

            if (reviveDuringRewind)
            {
                DeadPlayer deadPlayer = GameHistory.deadPlayers.FirstOrDefault();
                if ((DateTime.UtcNow - deadPlayer.timeOfDeath).Seconds <= rewindTime)
                {
                    // Revive Player
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)CustomRPC.RevivePlayer, SendOption.Reliable);
                    writer.Write(deadPlayer.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.stopStart(deadPlayer.player.PlayerId);
                    // Remove dead player
                    GameHistory.deadPlayers.Remove(deadPlayer);
                }
            }
        }
    }
}