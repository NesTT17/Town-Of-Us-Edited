using System.Collections.Generic;
using UnityEngine;
using Types = TownOfUs.CustomOption.CustomOptionType;

namespace TownOfUs
{
    public class CustomOptionHolder {
        public static string[] rates = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
        public static string[] ratesModifier = new string[]{"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3"};
        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
        
        public static CustomOption presetSelection;
        public static CustomOption nonKillingNeutralRolesCountMin;
        public static CustomOption nonKillingNeutralRolesCountMax;
        public static CustomOption killingNeutralRolesCountMin;
        public static CustomOption killingNeutralRolesCountMax;
        public static CustomOption modifiersCountMin;
        public static CustomOption modifiersCountMax;
        
        public static CustomOption anyPlayerCanStopStart;
        public static CustomOption camoComms;
        public static CustomOption hidePlayerNames;
        public static CustomOption allowParallelMedBayScans;
        
        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableAirShip;
        public static CustomOption dynamicMapEnableFungle;
        
        public static CustomOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;

        public static CustomOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;
        
        public static CustomOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchIncludeNeutral;
        public static CustomOption snitchIncludeKillingNeutral;
        public static CustomOption snitchSeeInMeeting;
        
        public static CustomOption engineerSpawnRate;
        public static CustomOption engineerDoorsCooldown;
        public static CustomOption engineerNumberOfFixes;
        
        public static CustomOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffCanKillNeutrals;
        public static CustomOption sheriffCanKillKNeutrals;
        
        public static CustomOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterHasImpostorVision;
        
        public static CustomOption shifterSpawnRate;
        
        public static CustomOption spySpawnRate;
        
        public static CustomOption guesserSpawnRate;
        public static CustomOption guesserIsImpGuesserRate;
        public static CustomOption guesserNumberOfShots;
        public static CustomOption guesserHasMultipleShotsPerMeeting;
        public static CustomOption guesserKillsThroughShield;
        public static CustomOption guesserSpawnBothRate;
        public static CustomOption guesserCantGuessSnitchIfTaksDone;
        
        public static CustomOption modifierLover;
        public static CustomOption modifierLoverImpLoverRate;
        public static CustomOption modifierLoverBothDie;
        public static CustomOption modifierLoverEnableChat;

        public static CustomOption modifierBlind;
        
        public static CustomOption modifierBait;
        public static CustomOption modifierBaitReportDelayMin;
        public static CustomOption modifierBaitReportDelayMax;
        
        public static CustomOption swapperSpawnRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;
        
        public static CustomOption modifierSleuth;
        
        public static CustomOption modifierTieBreaker;
        
        public static CustomOption mayorSpawnRate;
        
        public static CustomOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicGetsNotification;
        public static CustomOption medicGetsInfo;
        public static CustomOption medicReportNameDuration;
        public static CustomOption medicReportColorDuration;
        
        public static CustomOption draculaSpawnRate;
        public static CustomOption draculaKillCooldown;
        public static CustomOption draculaCanUseVents;
        public static CustomOption draculaMaxVampires;
        public static CustomOption vampireCanUseVents;
        public static CustomOption draculaCanCreateVampireFromImpostor;
        public static CustomOption draculaAndVampireHaveImpostorVision;
        
        public static CustomOption poisonerSpawnRate;
        public static CustomOption poisonerKillDelay;
        public static CustomOption poisonerCooldown;
        
        public static CustomOption scavengerSpawnRate;
        public static CustomOption scavengerCooldown;
        public static CustomOption scavengerNumberToWin;
        public static CustomOption scavengerCanUseVents;
        public static CustomOption scavengerShowArrows;
        
        public static CustomOption executionerSpawnRate;
        public static CustomOption executionerVision;
        public static CustomOption executionerCanCallEmergency;
        
        public static CustomOption lawyerSpawnRate;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption lawyerCanCallEmergency;
        public static CustomOption lawyerTargetCanBeJester;

        public static CustomOption pursuerSettings;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;
        
        public static CustomOption guardianAngelSpawnRate;
        public static CustomOption guardianAngelProtectCooldown;
        public static CustomOption guardianAngelProtectDuration;
        public static CustomOption guardianAngelNumberOfProtects;
        public static CustomOption survivorSafeguardCooldown;
        public static CustomOption survivorSafeguardDuration;
        public static CustomOption survivorNumberOfSafeguards;

        public static CustomOption amnesiacSpawnRate;
        public static CustomOption amnesiacShowArrows;
        public static CustomOption amnesiacDelay;
        
        public static CustomOption investigatorSpawnRate;
        public static CustomOption investigatorSeeColor;
        public static CustomOption investigatorAnonymousFootprints;
        public static CustomOption investigatorFootprintIntervall;
        public static CustomOption investigatorFootprintDuration;
        
        public static CustomOption veteranSpawnRate;
        public static CustomOption veteranCooldown;
        public static CustomOption veteranDuration;
        public static CustomOption veteranNumberOfAlerts;
        
        public static CustomOption seerSpawnRate;
        public static CustomOption seerCooldown;
        public static CustomOption seerNeutRed;
        public static CustomOption seerKNeutRed;

        public static CustomOption juggernautSpawnRate;
        public static CustomOption juggernautCooldown;
        public static CustomOption juggernautCooldownReduce;
        public static CustomOption juggernautHasImpostorVision;
        public static CustomOption juggernautCanVent;
        
        public static CustomOption swooperSpawnRate;
        public static CustomOption swooperCooldown;
        public static CustomOption swooperDuration;
        
        public static CustomOption mercenarySpawnRate;
        public static CustomOption mercenaryNeededAttempts;
        
        public static CustomOption blackmailerSpawnRate;
        public static CustomOption blackmailerCooldown;

        public static CustomOption escapistSpawnRate;
        public static CustomOption escapistRecallDuration;
        public static CustomOption escapistMarkCooldown;
        public static CustomOption escapistMarkStaysOverMeeting;
        
        public static CustomOption minerSpawnRate;
        public static CustomOption minerPlaceVentCooldown;
        
        public static CustomOption cleanerSpawnRate;
        public static CustomOption cleanerCooldown;
        
        public static CustomOption trapperSpawnRate;
        public static CustomOption trapperCooldown;
        public static CustomOption trapperTrapsRemoveOnNewRound;
        public static CustomOption trapperMaxTraps;
        public static CustomOption trapperMinAmountOfTimeInTrap;
        public static CustomOption trapperTrapSize;
        public static CustomOption trapperMinAmountOfPlayersInTrap;
        public static CustomOption trapperNeutShowsEvil;
        public static CustomOption trapperKNeutShowsEvil;
        
        public static CustomOption phantomSpawnRate;
        public static CustomOption phantomInvisCooldown;
        public static CustomOption phantomInvisDuration;

        public static CustomOption modifierButtonBarry;
        
        public static CustomOption modifierIndomitable;
        
        public static CustomOption modifierDrunk;
        
        public static CustomOption modifierSunglasses;
        public static CustomOption modifierSunglassesVision;
        
        public static CustomOption modifierTorch;

        public static CustomOption modifierDoubleShot;

        public static CustomOption grenadierSpawnRate;
        public static CustomOption grenadierCooldown;
        public static CustomOption grenadierDuration;
        public static CustomOption grenadierFlashRadius;
        public static CustomOption grenadierIndicateCrewmates;

        public static CustomOption doomsayerSpawnRate;
        public static CustomOption doomsayerCooldown;
        public static CustomOption doomsayerGuessesToWin;

        // Guesser Gamemode
        public static CustomOption guesserGamemodeCrewNumber;
        public static CustomOption guesserGamemodeNNeutralNumber;
        public static CustomOption guesserGamemodeKNeutralNumber;
        public static CustomOption guesserGamemodeImpNumber;
        public static CustomOption guesserGamemodeNumberOfShots;
        public static CustomOption guesserGamemodeHasMultipleShotsPerMeeting;
        public static CustomOption guesserGamemodeKillsThroughShield;
        public static CustomOption guesserGamemodeCantGuessSnitchIfTaksDone;
        public static CustomOption guesserGamemodeNewVampCanAssassinate;

        public static void Load() {
            CustomOption.vanillaSettings = TownOfUsPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

            // Role Options
            presetSelection = CustomOption.Create(0, Types.General, "Preset", presets, null, true);

            nonKillingNeutralRolesCountMin = CustomOption.Create(300, Types.General, "Minimum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f, null, true, heading: "Role Count Settings");
            nonKillingNeutralRolesCountMax = CustomOption.Create(301, Types.General, "Maximum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMin = CustomOption.Create(302, Types.General, "Minimum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMax = CustomOption.Create(303, Types.General, "Maximum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            modifiersCountMin = CustomOption.Create(304, Types.General, "Minimum Modifiers", 15f, 0f, 15f, 1f);
            modifiersCountMax = CustomOption.Create(305, Types.General, "Maximum Modifiers", 15f, 0f, 15f, 1f);

            anyPlayerCanStopStart = CustomOption.Create(1, Types.General, "Any Player Can Stop The Start", false, null, true, heading: "Custom Gameplay Options");
            camoComms = CustomOption.Create(2, Types.General, "Camouglaged Comms", false);
            hidePlayerNames = CustomOption.Create(3, Types.General, "Hide Player Names", false);
            allowParallelMedBayScans = CustomOption.Create(4, Types.General, "Allow Parallel MedBay Scans", false);

            // Guesser Gamemode (2000 - 2999)
            guesserGamemodeCrewNumber = CustomOption.Create(2001, Types.General, "Number of Crew Guessers", 15f, 0f, 15f, 1f, null, true, heading: "Amount of Guessers");
            guesserGamemodeNNeutralNumber = CustomOption.Create(2002, Types.General, "Number of Neutral Non-Killing Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeKNeutralNumber = CustomOption.Create(2002, Types.General, "Number of Neutral Killing Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeImpNumber = CustomOption.Create(2003, Types.General, "Number of Impostor Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeNumberOfShots = CustomOption.Create(2004, Types.General, "Guesser Number Of Shots", 3f, 1f, 15f, 1f, null, true, heading: "Guesser Settings");
            guesserGamemodeHasMultipleShotsPerMeeting = CustomOption.Create(2005, Types.General, "Guesser Can Shoot Multiple Times Per Meeting", false, null);
            guesserGamemodeKillsThroughShield = CustomOption.Create(2006, Types.General, "Guesses Ignore The Medic Shield", true, null);
            guesserGamemodeCantGuessSnitchIfTaksDone = CustomOption.Create(2007, Types.General, "Guesser Can't Guess Snitch When Tasks Completed", true, null);
            guesserGamemodeNewVampCanAssassinate = CustomOption.Create(2008, Types.General, "New Vampire Can Assassinate", true);

            dynamicMap = CustomOption.Create(500, Types.General, "Play On A Random Map", false, null, true, heading: "Random Maps");
            dynamicMapEnableSkeld = CustomOption.Create(501, Types.General, "Skeld", rates, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(502, Types.General, "Mira", rates, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(503, Types.General, "Polus", rates, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(504, Types.General, "Airship", rates, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(506, Types.General, "Fungle", rates, dynamicMap, false);

            engineerSpawnRate = CustomOption.Create(15, Types.Crewmate, cs(Engineer.color, "Engineer"), rates, null, true);
            engineerDoorsCooldown = CustomOption.Create(16, Types.Crewmate, "Doors Open CD", 30f, 10f, 60f, 2.5f, engineerSpawnRate);
            engineerNumberOfFixes = CustomOption.Create(17, Types.Crewmate, "Number Of Sabotage Fixes", 1f, 1f, 3f, 1f, engineerSpawnRate);
            
            investigatorSpawnRate = CustomOption.Create(105, Types.Crewmate, cs(Investigator.color, "Investigator"), rates, null, true);
            investigatorSeeColor = CustomOption.Create(106, Types.Crewmate, "Can See Target Player Color", false, investigatorSpawnRate);
            investigatorAnonymousFootprints = CustomOption.Create(107, Types.Crewmate, "Anonymous Footprints", false, investigatorSpawnRate); 
            investigatorFootprintIntervall = CustomOption.Create(108, Types.Crewmate, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);
            investigatorFootprintDuration = CustomOption.Create(109, Types.Crewmate, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);

            mayorSpawnRate = CustomOption.Create(54, Types.Crewmate, cs(Mayor.color, "Mayor"), rates, null, true);

            medicSpawnRate = CustomOption.Create(55, Types.Crewmate, cs(Medic.color, "Medic"), rates, null, true);
            medicShowShielded = CustomOption.Create(56, Types.Crewmate, "Show Shielded Player", new string[] {"Everyone", "Shielded", "Medic", "Shielded + Medic", "Nobody"}, medicSpawnRate);
            medicGetsNotification = CustomOption.Create(57, Types.Crewmate, "Show Murder Attempt", new string[] {"Everyone", "Shielded", "Medic", "Shielded + Medic", "Nobody"}, medicSpawnRate);
            medicGetsInfo = CustomOption.Create(58, Types.Crewmate, "Gets Dead Body Info On Report", false, medicSpawnRate);
            medicReportNameDuration = CustomOption.Create(59, Types.Crewmate, "Time Where Medic Reports Will Have Name", 0f, 0f, 30f, 0.25f, medicGetsInfo);
            medicReportColorDuration = CustomOption.Create(60, Types.Crewmate, "Time Where Medic Reports Will Have Color Type", 0f, 0f, 60f, 0.25f, medicGetsInfo);

            seerSpawnRate = CustomOption.Create(115, Types.Crewmate, cs(Seer.color, "Seer"), rates, null, true);
            seerCooldown = CustomOption.Create(116, Types.Crewmate, "Reveal Cooldown", 30f, 10f, 60f, 2.5f, seerSpawnRate);
            seerNeutRed = CustomOption.Create(117, Types.Crewmate, "Neutrals Shows Evil", false, seerSpawnRate);
            seerKNeutRed = CustomOption.Create(118, Types.Crewmate, "Killing Neutrals Shows Evil", false, seerSpawnRate);

            sheriffSpawnRate = CustomOption.Create(25, Types.Crewmate, cs(Sheriff.color, "Sheriff"), rates, null, true);
            sheriffCooldown = CustomOption.Create(26, Types.Crewmate, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            sheriffCanKillNeutrals = CustomOption.Create(27, Types.Crewmate, "Sheriff Can Kill Neutrals", false, sheriffSpawnRate);
            sheriffCanKillKNeutrals = CustomOption.Create(28, Types.Crewmate, "Sheriff Can Kill Killing Neutrals", false, sheriffSpawnRate);

            shifterSpawnRate = CustomOption.Create(40, Types.Crewmate, cs(Shifter.color, "Shifter"), rates, null, true);

            snitchSpawnRate = CustomOption.Create(10, Types.Crewmate, cs(Snitch.color, "Snitch"), rates, null, true);
            snitchSeeInMeeting = CustomOption.Create(14, Types.Crewmate, "Reveal Players In Meeting", false, snitchSpawnRate);
            snitchLeftTasksForReveal = CustomOption.Create(11, Types.Crewmate, "Task Count Where The Snitch Will Be Revealed", 1f, 0f, 5f, 1f, snitchSpawnRate);
            snitchIncludeNeutral = CustomOption.Create(12, Types.Crewmate, "Reveal Neutral Non-Killing Roles", false, snitchSpawnRate);
            snitchIncludeKillingNeutral = CustomOption.Create(13, Types.Crewmate, "Reveal Neutral Killing Roles", false, snitchSpawnRate);

            spySpawnRate = CustomOption.Create(41, Types.Crewmate, cs(Spy.color, "Spy"), rates, null, true);

            swapperSpawnRate = CustomOption.Create(45, Types.Crewmate, cs(Swapper.color, "Swapper"), rates, null, true);
            swapperCanCallEmergency = CustomOption.Create(46, Types.Crewmate, "Swapper Can Call Emergency Meeting", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(47, Types.Crewmate, "Swapper Can Only Swap Others", false, swapperSpawnRate);

            trapperSpawnRate = CustomOption.Create(150, Types.Crewmate, cs(Trapper.color, "Trapper"), rates, null, true);
            trapperMinAmountOfTimeInTrap = CustomOption.Create(151, Types.Crewmate, "Min Amount Of Time In Trap To Register", 1f, 0f, 15f, 0.5f, trapperSpawnRate);
            trapperCooldown = CustomOption.Create(152, Types.Crewmate, "Trapper Cooldown", 30f, 10f, 60f, 2.5f, trapperSpawnRate);
            trapperTrapsRemoveOnNewRound = CustomOption.Create(153, Types.Crewmate, "Traps Removed After Each Round", false, trapperSpawnRate);
            trapperMaxTraps = CustomOption.Create(154, Types.Crewmate, "Maximum Number Of Traps Per Game", 5f, 1f, 15f, 1f, trapperSpawnRate);
            trapperTrapSize = CustomOption.Create(155, Types.Crewmate, "Trap Size", 0.25f, 0.125f, 1f, 0.125f, trapperSpawnRate);
            trapperMinAmountOfPlayersInTrap = CustomOption.Create(156, Types.Crewmate, "Minimum Number Of Roles Required To Trigger Trap", 3f, 1f, 5f, 1f, trapperSpawnRate);
            trapperNeutShowsEvil = CustomOption.Create(157, Types.Crewmate, "Neutrals Shows Evil", false, trapperSpawnRate);
            trapperKNeutShowsEvil = CustomOption.Create(158, Types.Crewmate, "Killing Neutrals Shows Evil", false, trapperSpawnRate);

            veteranSpawnRate = CustomOption.Create(110, Types.Crewmate, cs(Veteran.color, "Veteran"), rates, null, true);
            veteranCooldown = CustomOption.Create(111, Types.Crewmate, "Alert Cooldown", 30f, 10f, 60f, 2.5f, veteranSpawnRate);
            veteranDuration = CustomOption.Create(112, Types.Crewmate, "Alert Duration", 5f, 1f, 20f, 0.5f, veteranSpawnRate);
            veteranNumberOfAlerts = CustomOption.Create(113, Types.Crewmate, "Number Of Alerts", 5f, 1f, 15f, 1f, veteranSpawnRate);

            guesserSpawnRate = CustomOption.Create(310, Types.Crewmate, $"{cs(Guesser.color, "Vigilante")}/{cs(Palette.ImpostorRed, "Assassin")}", rates, null, true);
            guesserIsImpGuesserRate = CustomOption.Create(311, Types.Crewmate, "Chance That The Guesser Is An Impostor", rates, guesserSpawnRate);
            guesserNumberOfShots = CustomOption.Create(312, Types.Crewmate, "Number Of Shots", 2f, 1f, 15f, 1f, guesserSpawnRate);
            guesserHasMultipleShotsPerMeeting = CustomOption.Create(313, Types.Crewmate, "Can Shoot Multiple Times Per Meeting", false, guesserSpawnRate);
            guesserKillsThroughShield  = CustomOption.Create(314, Types.Crewmate, "Guesses Ignore The Medic Shield", true, guesserSpawnRate);
            guesserSpawnBothRate = CustomOption.Create(315, Types.Crewmate, "Both Guesser Spawn Rate", rates, guesserSpawnRate);
            guesserCantGuessSnitchIfTaksDone = CustomOption.Create(316, Types.Crewmate, "Can't Guess Snitch When Tasks Completed", true, guesserSpawnRate);

            amnesiacSpawnRate = CustomOption.Create(100, Types.Neutral, cs(Amnesiac.color, "Amnesiac"), rates, null, true);
            amnesiacShowArrows = CustomOption.Create(101, Types.Neutral, "Show Arrows To Dead Bodies", false, amnesiacSpawnRate);
            amnesiacDelay = CustomOption.Create(102, Types.Neutral, "Arrow Appears Delay", 3f, 0f, 5f, 0.25f, amnesiacShowArrows);

            doomsayerSpawnRate = CustomOption.Create(175, Types.Neutral, cs(Doomsayer.color, "Doomsayer"), rates, null, true);
            doomsayerCooldown = CustomOption.Create(176, Types.Neutral, "Observe Cooldown", 30f, 10f, 60f, 2.5f, doomsayerSpawnRate);
            doomsayerGuessesToWin = CustomOption.Create(177, Types.Neutral, "Num Of Correct Guesses To Win", 3f, 2f, 5f, 1f, doomsayerSpawnRate);

            executionerSpawnRate = CustomOption.Create(75, Types.Neutral, cs(Executioner.color, "Executioner"), rates, null, true);
            executionerVision = CustomOption.Create(76, Types.Neutral, "Executioner Vision", 1f, 0.25f, 3f, 0.25f, executionerSpawnRate);
            executionerCanCallEmergency = CustomOption.Create(77, Types.Neutral, "Executioner Can Call Emergency Meeting", true, executionerSpawnRate);

            guardianAngelSpawnRate = CustomOption.Create(90, Types.Neutral, cs(GuardianAngel.color, "Guardian Angel"), rates, null, true);
            guardianAngelProtectCooldown = CustomOption.Create(91, Types.Neutral, "Protect Cooldown", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            guardianAngelProtectDuration = CustomOption.Create(92, Types.Neutral, "Protect Duration", 10f, 1f, 20f, 0.5f, guardianAngelSpawnRate);
            guardianAngelNumberOfProtects = CustomOption.Create(93, Types.Neutral, "Number Of Protects", 5f, 1f, 15f, 1f, guardianAngelSpawnRate);
            survivorSafeguardCooldown = CustomOption.Create(94, Types.Neutral, "Morphling Cooldown", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            survivorSafeguardDuration = CustomOption.Create(95, Types.Neutral, "Morph Duration", 10f, 1f, 20f, 0.5f, guardianAngelSpawnRate);
            survivorNumberOfSafeguards = CustomOption.Create(96, Types.Neutral, "Number Of Safeguards", 5f, 1f, 15f, 1f, guardianAngelSpawnRate);

            jesterSpawnRate = CustomOption.Create(35, Types.Neutral, cs(Jester.color, "Jester"), rates, null, true);
            jesterCanCallEmergency = CustomOption.Create(36, Types.Neutral, "Jester Can Call Emergency Meeting", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(37, Types.Neutral, "Jester Has Impostor Vision", false, jesterSpawnRate);

            lawyerSpawnRate = CustomOption.Create(80, Types.Neutral, cs(Lawyer.color, "Lawyer"), rates, null, true);
            lawyerVision = CustomOption.Create(81, Types.Neutral, "Lawyer Vision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate);
            lawyerKnowsRole = CustomOption.Create(82, Types.Neutral, "Lawyer Knows Target Role", false, lawyerSpawnRate);
            lawyerCanCallEmergency = CustomOption.Create(83, Types.Neutral, "Lawyer Can Call Emergency Meeting", true, lawyerSpawnRate);
            lawyerTargetCanBeJester = CustomOption.Create(84, Types.Neutral, "Lawyer Target Can Be The Jester", false, lawyerSpawnRate);

            mercenarySpawnRate = CustomOption.Create(130, Types.Neutral, cs(Mercenary.color, "Mercenary"), rates, null, true);
            mercenaryNeededAttempts = CustomOption.Create(131, Types.Neutral, "Murders Required To Win", 3f, 2f, 15f, 1f, mercenarySpawnRate);

            pursuerSettings = CustomOption.Create(85, Types.Neutral, cs(Pursuer.color, "Pursuer"), new string[] {"Hide Settings", "Show Settings"}, null, true);
            pursuerCooldown = CustomOption.Create(86, Types.Neutral, "Pursuer Blank Cooldown", 30f, 5f, 60f, 2.5f, pursuerSettings);
            pursuerBlanksNumber = CustomOption.Create(87, Types.Neutral, "Pursuer Number Of Blanks", 5f, 1f, 20f, 1f, pursuerSettings);

            scavengerSpawnRate = CustomOption.Create(70, Types.Neutral, cs(Scavenger.color, "Scavenger"), rates, null, true);
            scavengerCooldown = CustomOption.Create(71, Types.Neutral, "Scavenger Cooldown", 15f, 10f, 60f, 2.5f, scavengerSpawnRate);
            scavengerNumberToWin = CustomOption.Create(72, Types.Neutral, "Number Of Corpses Needed To Be Eaten", 4f, 1f, 10f, 1f, scavengerSpawnRate);
            scavengerCanUseVents = CustomOption.Create(73, Types.Neutral, "Scavenger Can Use Vents", true, scavengerSpawnRate);
            scavengerShowArrows = CustomOption.Create(74, Types.Neutral, "Show Arrows Pointing Towards The Corpses", true, scavengerSpawnRate);

            draculaSpawnRate = CustomOption.Create(61, Types.NeutralKiller, cs(Dracula.color, "Dracula"), rates, null, true);
            draculaKillCooldown = CustomOption.Create(62, Types.NeutralKiller, "Dracula/Vampire Bite Cooldown", 30f, 10f, 60f, 2.5f, draculaSpawnRate);
            draculaCanUseVents = CustomOption.Create(63, Types.NeutralKiller, "Dracula Can Vent", false, draculaSpawnRate);
            draculaMaxVampires = CustomOption.Create(64, Types.NeutralKiller, "Max Number Of Vampires", 2f, 2f, 4f, 1f, draculaSpawnRate);
            draculaAndVampireHaveImpostorVision = CustomOption.Create(67, Types.NeutralKiller, "Dracula/Vampire Has Impostor Vision", false, draculaSpawnRate);
            draculaCanCreateVampireFromImpostor = CustomOption.Create(66, Types.NeutralKiller, "Dracula Can Create Vampire From Impostor", false, draculaSpawnRate);
            vampireCanUseVents = CustomOption.Create(65, Types.NeutralKiller, "Vampire Can Vent", false, draculaSpawnRate);

            juggernautSpawnRate = CustomOption.Create(120, Types.NeutralKiller, cs(Juggernaut.color, "Juggernaut"), rates, null, true);
            juggernautCooldown = CustomOption.Create(121, Types.NeutralKiller, "Initial Kill Cooldown", 30f, 10f, 60f, 2.5f, juggernautSpawnRate);
            juggernautCooldownReduce = CustomOption.Create(122, Types.NeutralKiller, "Cooldown Reduce On Kill", 10f, 1f, 20f, 0.5f, juggernautSpawnRate);
            juggernautHasImpostorVision = CustomOption.Create(123, Types.NeutralKiller, "Juggernaut Has Impostor Vision", false, juggernautSpawnRate);
            juggernautCanVent = CustomOption.Create(124, Types.NeutralKiller, "Juggernaut Can Use Vents", false, juggernautSpawnRate);

            blackmailerSpawnRate = CustomOption.Create(140, Types.Impostor, cs(Blackmailer.color, "Blackmailer"), rates, null, true);
            blackmailerCooldown = CustomOption.Create(141, Types.Impostor, "Blackmail Cooldown", 30f, 2.5f, 60f, 2.5f, blackmailerSpawnRate);

            camouflagerSpawnRate = CustomOption.Create(30, Types.Impostor, cs(Camouflager.color, "Camouflager"), rates, null, true);
            camouflagerCooldown = CustomOption.Create(31, Types.Impostor, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);
            camouflagerDuration = CustomOption.Create(32, Types.Impostor, "Camo Duration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate);

            cleanerSpawnRate = CustomOption.Create(145, Types.Impostor, cs(Cleaner.color, "Cleaner"), rates, null, true);
            cleanerCooldown = CustomOption.Create(146, Types.Impostor, "Cleaner Cooldown", 30f, 10f, 60f, 2.5f, cleanerSpawnRate);

            escapistSpawnRate = CustomOption.Create(135, Types.Impostor, cs(Escapist.color, "Escapist"), rates, null, true);
            escapistRecallDuration = CustomOption.Create(136, Types.Impostor, "Recall Duration", 10f, 1f, 20f, 0.5f, escapistSpawnRate);
            escapistMarkCooldown = CustomOption.Create(137, Types.Impostor, "Mark Location Cooldown", 30f, 10f, 60f, 2.5f, escapistSpawnRate);
            escapistMarkStaysOverMeeting = CustomOption.Create(138, Types.Impostor, "Marked Location Stays After Meeting", true, escapistSpawnRate);

            grenadierSpawnRate = CustomOption.Create(170, Types.Impostor, cs(Grenadier.color, "Grenadier"), rates, null, true);
            grenadierCooldown = CustomOption.Create(171, Types.Impostor, "Grenadier Cooldown", 30f, 10f, 60f, 2.5f, grenadierSpawnRate);
            grenadierDuration = CustomOption.Create(172, Types.Impostor, "Flash Duration", 10f, 1f, 20f, 0.5f, grenadierSpawnRate);
            grenadierFlashRadius = CustomOption.Create(173, Types.Impostor, "Flash Radius", new string[] { "All Map", "0.25x", "0.5x", "0.75x", "1x", "1.25x", "1.5x", "1.75x", "2x", "2.25x", "2.5x", "2.75x", "3x" }, grenadierSpawnRate);
            grenadierIndicateCrewmates = CustomOption.Create(174, Types.Impostor, "Indicate Flashed Crewmates", false, grenadierSpawnRate);

            morphlingSpawnRate = CustomOption.Create(20, Types.Impostor, cs(Morphling.color, "Morphling"), rates, null, true);
            morphlingCooldown = CustomOption.Create(21, Types.Impostor, "Morphling Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);
            morphlingDuration = CustomOption.Create(22, Types.Impostor, "Morph Duration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate);

            minerSpawnRate = CustomOption.Create(160, Types.Impostor, cs(Miner.color, "Miner"), rates, null, true);
            minerPlaceVentCooldown = CustomOption.Create(161, Types.Impostor, "Place Vent Cooldown", 10f, 2.5f, 30f, 2.5f, minerSpawnRate);

            phantomSpawnRate = CustomOption.Create(165, Types.Impostor, cs(Phantom.color, "Phantom"), rates, null, true);
            phantomInvisCooldown = CustomOption.Create(166, Types.Impostor, "Invis Cooldown", 30f, 10f, 60f, 2.5f, phantomSpawnRate);
            phantomInvisDuration = CustomOption.Create(167, Types.Impostor, "Invis Duration", 10f, 1f, 20f, 0.5f, phantomSpawnRate);

            poisonerSpawnRate = CustomOption.Create(50, Types.Impostor, cs(Poisoner.color, "Poisoner"), rates, null, true);
            poisonerKillDelay = CustomOption.Create(51, Types.Impostor, "Poisoner Kill Delay", 10f, 1f, 20f, 1f, poisonerSpawnRate);
            poisonerCooldown = CustomOption.Create(52, Types.Impostor, "Poisoner Cooldown", 30f, 10f, 60f, 2.5f, poisonerSpawnRate);

            swooperSpawnRate = CustomOption.Create(125, Types.Impostor, cs(Swooper.color, "Swooper"), rates, null, true);
            swooperCooldown = CustomOption.Create(126, Types.Impostor, "Swooper Cooldown", 30f, 10f, 60f, 2.5f, swooperSpawnRate);
            swooperDuration = CustomOption.Create(127, Types.Impostor, "Swoop Duration", 10f, 1f, 20f, 0.5f, swooperSpawnRate);

            modifierBait = CustomOption.Create(1001, Types.Modifier, cs(Palette.CrewmateBlue, "Bait"), rates, null, true);
            modifierBaitReportDelayMin = CustomOption.Create(1002, Types.Modifier, "Bait Report Delay Min", 0f, 0f, 10f, 1f, modifierBait);
            modifierBaitReportDelayMax = CustomOption.Create(1003, Types.Modifier, "Bait Report Delay Max", 0f, 0f, 10f, 1f, modifierBait);

            modifierBlind = CustomOption.Create(1005, Types.Modifier, cs(Palette.CrewmateBlue, "Blind"), rates, null, true);

            modifierIndomitable = CustomOption.Create(1030, Types.Modifier, cs(Palette.CrewmateBlue, "Indomitable"), rates, null, true);

            modifierSunglasses = CustomOption.Create(1006, Types.Modifier, cs(Palette.CrewmateBlue, "Sunglasses"), rates, null, true);
            modifierSunglassesVision = CustomOption.Create(1007, Types.Modifier, "Sunglasses Vision", new string[] { "-10%", "-20%", "-30%", "-40%", "-50%" }, modifierSunglasses);

            modifierTorch= CustomOption.Create(1008, Types.Modifier, cs(Palette.CrewmateBlue, "Torch"), rates, null, true);

            modifierButtonBarry = CustomOption.Create(1025, Types.Modifier, cs(Color.yellow, "Button Barry"), rates, null, true);

            modifierDrunk = CustomOption.Create(1004, Types.Modifier, cs(Color.yellow, "Drunk"), rates, null, true);

            modifierLover = CustomOption.Create(1010, Types.Modifier, cs(Color.yellow, "Lovers"), rates, null, true);
            modifierLoverImpLoverRate = CustomOption.Create(1011, Types.Modifier, "Chance That One Lover Is Killer", rates, modifierLover);
            modifierLoverBothDie = CustomOption.Create(1012, Types.Modifier, "Both Lovers Die", true, modifierLover);
            modifierLoverEnableChat = CustomOption.Create(1013, Types.Modifier, "Enable Lover Chat", true, modifierLover);

            modifierSleuth = CustomOption.Create(1015, Types.Modifier, cs(Color.yellow, "Sleuth"), rates, null, true);

            modifierTieBreaker = CustomOption.Create(1020, Types.Modifier, cs(Color.yellow, "Tiebreaker"), rates, null, true);
            
            modifierDoubleShot = CustomOption.Create(1014, Types.Modifier, cs(Palette.ImpostorRed, "Double Shot"), rates, null, true);

            // Block Role Pairings
            blockedRolePairings.Add((byte)RoleId.Lawyer, new [] { (byte)RoleId.Executioner, (byte)RoleId.GuardianAngel });
            blockedRolePairings.Add((byte)RoleId.Executioner, new [] { (byte)RoleId.Lawyer});
            blockedRolePairings.Add((byte)RoleId.GuardianAngel, new [] { (byte)RoleId.Lawyer});

            blockedRolePairings.Add((byte)RoleId.Scavenger, new [] { (byte)RoleId.Cleaner});
            blockedRolePairings.Add((byte)RoleId.Cleaner, new [] { (byte)RoleId.Scavenger});

            blockedRolePairings.Add((byte)RoleId.Swooper, new [] { (byte)RoleId.Phantom});
            blockedRolePairings.Add((byte)RoleId.Phantom, new [] { (byte)RoleId.Swooper});
        }
    }
}