using System.Collections.Generic;
using UnityEngine;
using Types = TownOfUs.Modules.CustomOption.CustomOptionType;
namespace TownOfUs
{
    public class CustomOptionHolder
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] ratesModifier = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
        public static string[] presets = new string[] { "Preset 1", "Preset 2", "Preset 3" };
        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();
        public static string cs(Color c, string s) => Helpers.cs(c, s);

        public static CustomOption presetSelection;
        public static CustomOption nonKillingNeutralRolesCountMin;
        public static CustomOption nonKillingNeutralRolesCountMax;
        public static CustomOption killingNeutralRolesCountMin;
        public static CustomOption killingNeutralRolesCountMax;
        public static CustomOption modifiersCountMin;
        public static CustomOption modifiersCountMax;

        public static CustomOption isDraftMode;
        public static CustomOption draftModeAmountOfChoices;
        public static CustomOption draftModeTimeToChoose;
        public static CustomOption draftModeShowRoles;
        public static CustomOption draftModeHideCrewmateRoles;
        public static CustomOption draftModeHideNeutralBenignRoles;
        public static CustomOption draftModeHideNeutralEvilRoles;
        public static CustomOption draftModeHideNeutralKillingRoles;
        public static CustomOption draftModeHideImpRoles;

        public static CustomOption anyPlayerCanStopStart;
        public static CustomOption maxNumberOfMeetings;
        public static CustomOption hidePlayerNames;
        public static CustomOption allowParallelMedBayScans;
        public static CustomOption shieldFirstKill;
        public static CustomOption enableBetterPolus;
        public static CustomOption initialCooldown;
        public static CustomOption randomSpawnPositions;

        public static CustomOption guesserGamemodeCrewNumber;
        public static CustomOption guesserGamemodeBenignNeutralNumber;
        public static CustomOption guesserGamemodeEvilNeutralNumber;
        public static CustomOption guesserGamemodeKillingNeutralNumber;
        public static CustomOption guesserGamemodeImpNumber;
        public static CustomOption guesserGamemodeNumberOfShots;
        public static CustomOption guesserGamemodeHasMultipleShotsPerMeeting;
        public static CustomOption guesserGamemodeKillsThroughShield;
        public static CustomOption guesserGamemodeEvilCanKillAgent;
        public static CustomOption guesserGamemodeCantGuessSnitchIfTaksDone;
        public static CustomOption guesserGamemodeNewVampCanAssassinate;
        public static CustomOption guesserGamemodeCrewGuesserNumberOfTasks;
        
        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableAirShip;
        public static CustomOption dynamicMapEnableFungle;

        public static CustomRoleOption sheriffSpawnRate;
        public static CustomOption sheriffMissfireShootKillsTarget;
        public static CustomOption sheriffShootCooldown;
        public static CustomOption sheriffCanKillBenignNeutrals;
        public static CustomOption sheriffCanKillEvilNeutrals;
        public static CustomOption sheriffCanKillKillingNeutrals;
        
        public static CustomRoleOption seerSpawnRate;
        public static CustomOption seerRevealCooldown;
        public static CustomOption seerBenignNeutralsShowsEvil;
        public static CustomOption seerEvilNeutralsShowsEvil;
        public static CustomOption seerKillingNeutralsShowsEvil;
        
        public static CustomRoleOption engineerSpawnRate;
        public static CustomOption engineerNumberOfFixes;
        
        public static CustomRoleOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterHasImpostorVision;
        
        public static CustomRoleOption juggernautSpawnRate;
        public static CustomOption juggernautCooldown;
        public static CustomOption juggernautCooldownReduce;
        public static CustomOption juggernautHasImpostorVision;
        public static CustomOption juggernautCanVent;
        
        public static CustomRoleOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchIncludeBenignNeutral;
        public static CustomOption snitchIncludeEvilNeutral;
        public static CustomOption snitchIncludeKillingNeutral;
        
        public static CustomRoleOption veteranSpawnRate;
        public static CustomOption veteranCooldown;
        public static CustomOption veteranAlertDuration;
        public static CustomOption veteranAlertNumber;

        public static CustomModifierOption modifierBait;
        public static CustomOption modifierBaitReportDelay;
        public static CustomOption modifierBaitShowKillFlash;
        
        public static CustomRoleOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;
        
        public static CustomRoleOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;
        
        public static CustomRoleOption lawyerSpawnRate;
        public static CustomOption lawyerTargetKnows;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption lawyerWinsAfterMeetings;
        public static CustomOption lawyerNeededMeetings;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;

        public static CustomRoleOption politicianSpawnRate;
        public static CustomOption politicianCooldown;
        public static CustomOption mayorBodyguardCooldown;
        public static CustomOption mayorBodyguardDuration;
        
        public static CustomRoleOption plaguebearerSpawnRate;
        public static CustomOption plaguebearerCooldown;
        public static CustomOption pestilenceCooldown;
        public static CustomOption pestilenceHasImpostorVision;
        public static CustomOption pestilenceCanVent;
        
        public static CustomRoleOption werewolfSpawnRate;
        public static CustomOption werewolfRampageCooldown;
        public static CustomOption werewolfRampageDuration;
        public static CustomOption werewolfKillCooldown;
        public static CustomOption werewolfCanVent;

        public static CustomRoleOption draculaSpawnRate;
        public static CustomOption draculaAndVampireBiteCooldown;
        public static CustomOption draculaHasImpostorVision;
        public static CustomOption draculaCanVent;
        public static CustomOption draculaCanCreateVampire;
        public static CustomOption draculaMaxVampires;
        public static CustomOption draculaCanConvertBenignNeutral;
        public static CustomOption draculaCanConvertEvilNeutral;
        public static CustomOption draculaCanConvertKillingNeutral;
        public static CustomOption draculaCanConvertImpostor;
        public static CustomOption vampireHasImpostorVision;
        public static CustomOption vampireCanVent;
        
        public static CustomRoleOption swapperSpawnRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;

        public static CustomRoleOption vampireHunterSpawnRate;
        public static CustomOption vampireHunterStakeCooldown;
        public static CustomOption vampireHunterMaxFailedStakes;
        public static CustomOption vampireHunterCanStakeRoundOne;
        public static CustomOption vampireHunterSelfKillAfterFinalStake;
        public static CustomOption vampireHunterPromote;
        
        public static CustomRoleOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicGetsNotification;
        public static CustomOption medicGetsInfo;
        public static CustomOption medicReportNameDuration;
        public static CustomOption medicReportColorDuration;

        public static CustomRoleOption guardianAngelSpawnRate;
        public static CustomOption guardianAngelProtectCooldown;
        public static CustomOption guardianAngelProtectDuration;
        public static CustomOption guardianAngelNumberOfProtects;
        public static CustomOption guardianAngelKnowsRole;
        public static CustomOption guardianAngelTargetKnows;
        public static CustomOption guardianAngelWhoSeesProtect;
        public static CustomOption guardianAngelOddsEvilTarget;
        public static CustomOption survivorSafeguardCooldown;
        public static CustomOption survivorSafeguardDuration;
        public static CustomOption survivorNumberOfSafeguards;
        
        public static CustomRoleOption amnesiacSpawnRate;
        public static CustomOption amnesiacShowArrows;
        public static CustomOption amnesiacDelay;
        public static CustomOption amnesiacResetRoleWhenTaken;
        public static CustomOption amnesiacCanRememberOnMeetingIfGuesser;

        public static CustomRoleOption executionerSpawnRate;
        public static CustomOption executionerVision;
        public static CustomOption executionerCanCallEmergency;
        
        public static CustomRoleOption spySpawnRate;
        
        public static CustomRoleOption poisonerSpawnRate;
        public static CustomOption poisonerKillDelay;
        public static CustomOption poisonerCooldown;
        
        public static CustomRoleOption scavengerSpawnRate;
        public static CustomOption scavengerCooldown;
        public static CustomOption scavengerNumberToWin;
        public static CustomOption scavengerCanUseVents;
        public static CustomOption scavengerShowArrows;
        public static CustomOption scavengerCanEatWithGuess;
        
        public static CustomRoleOption investigatorSpawnRate;
        public static CustomOption investigatorAnonymousFootprints;
        public static CustomOption investigatorFootprintIntervall;
        public static CustomOption investigatorFootprintDuration;
        
        public static CustomRoleOption janitorSpawnRate;
        public static CustomOption janitorCooldown;
        
        public static CustomRoleOption swooperSpawnRate;
        public static CustomOption swooperCooldown;
        public static CustomOption swooperDuration;
        
        public static CustomModifierOption modifierShifter;
        public static CustomOption modifierShifterShiftsMedicShield;
        
        public static CustomRoleOption oracleSpawnRate;
        public static CustomOption oracleConfessCooldown;
        public static CustomOption oracleRevealAccuracy;
        public static CustomOption oracleBenignNeutralsShowsEvil;
        public static CustomOption oracleEvilNeutralsShowsEvil;
        public static CustomOption oracleKillingNeutralsShowsEvil;
        
        public static CustomRoleOption mercenarySpawnRate;
        public static CustomOption mercenaryShowShielded;
        public static CustomOption mercenaryGetsNotification;
        public static CustomOption mercenaryBrildersRequired;
        public static CustomOption mercenaryArmorCooldown;
        public static CustomOption mercenaryArmorDuration;

        public static CustomRoleOption blackmailerSpawnRate;
        public static CustomOption blackmailerCooldown;
        public static CustomOption blackmailerBlockTargetVote;
        public static CustomOption blackmailerBlockTargetAbility;

        public static CustomRoleOption grenadierSpawnRate;
        public static CustomOption grenadierCooldown;
        public static CustomOption grenadierDuration;
        public static CustomOption grenadierFlashRadius;
        public static CustomOption grenadierIndicateCrewmates;
        
        public static CustomRoleOption mysticSpawnRate;
        public static CustomOption mysticArrowDuration;

        public static CustomRoleOption glitchSpawnRate;
        public static CustomOption glitchMimicCooldown;
        public static CustomOption glitchMimicDuration;
        public static CustomOption glitchHackCooldown;
        public static CustomOption glitchHackDuration;
        public static CustomOption glitchKillCooldown;
        public static CustomOption glitchCanVent;
        public static CustomOption glitchHasImpostorVision;
        
        public static CustomRoleOption transporterSpawnRate;
        public static CustomOption transporterCooldown;
        public static CustomOption transporterNumberOfTransports;
        
        public static CustomModifierOption modifierButtonBarry;
        
        public static CustomModifierOption modifierBlind;
        
        public static CustomModifierOption modifierSleuth;
        
        public static CustomModifierOption modifierIndomitable;
        
        public static CustomModifierOption modifierDrunk;
        
        public static CustomModifierOption modifierTorch;
        
        public static CustomRoleOption arsonistSpawnRate;
        public static CustomOption arsonistDouseCooldown;
        public static CustomOption arsonistIgniteCooldown;
        public static CustomOption arsonistCanVent;
        public static CustomOption arsonistHasImpostorVision;
        public static CustomOption arsonistMaxAliveDoused;
        public static CustomOption arsonistRemoveIgniteCooldown;

        public static CustomRoleOption agentSpawnRate;
        public static CustomOption agentCanDieToSheriff;
        public static CustomOption agentImpostorsCanKillAnyone;
        public static CustomOption agentCanEnterVents;
        public static CustomOption agentHasImpostorVision;
        
        public static CustomRoleOption thiefSpawnRate;
        public static CustomOption thiefCooldown;
        public static CustomOption thiefHasImpVision;
        public static CustomOption thiefCanUseVents;
        public static CustomOption thiefCanKillMayor;
        public static CustomOption thiefCanKillSheriff;
        public static CustomOption thiefCanKillVampireHunter;
        public static CustomOption thiefCanKillVeteran;
        public static CustomOption thiefCanStealWithGuess;
        
        public static CustomModifierOption modifierVip;
        
        public static CustomModifierOption modifierRadar;

        public static CustomRoleOption lighterSpawnRate;
        public static CustomOption lighterModeLightsOnVision;
        public static CustomOption lighterModeLightsOffVision;
        public static CustomOption lighterCooldown;
        public static CustomOption lighterDuration;

        public static CustomRoleOption detectiveSpawnRate;
        public static CustomOption detectiveInitialCooldown;
        public static CustomOption detectiveCooldown;
        public static CustomOption detectiveBloodTime;
        public static CustomOption detectiveGetInfoOnReport;
        public static CustomOption detectiveReportRoleDuration;
        public static CustomOption detectiveReportFactionDuration;
        public static CustomOption detectiveGetExamineInfo;
        
        public static CustomRoleOption bomberSpawnRate;
        public static CustomOption bomberMaxKillsInDetonation;
        public static CustomOption bomberDetonateRadius;
        
        public static CustomRoleOption venererSpawnRate;
        public static CustomOption venererCooldown;
        public static CustomOption venererDuration;
        public static CustomOption venererSpeedMultiplier;
        public static CustomOption venererFreezeMultiplier;
        
        public static CustomRoleOption bountyHunterSpawnRate;
        public static CustomOption bountyHunterBountyDuration;
        public static CustomOption bountyHunterReducedCooldown;
        public static CustomOption bountyHunterPunishmentTime;
        public static CustomOption bountyHunterShowArrow;
        public static CustomOption bountyHunterArrowUpdateIntervall;
        
        public static CustomOption sheriffBlockGameEnd;
        public static CustomOption vampireHunterBlockGameEnd;
        public static CustomOption veteranBlockGameEnd;
        public static CustomOption mayorBlockGameEnd;
        public static CustomOption swapperBlockGameEnd;

        public static void Load()
        {
            CustomOption.vanillaSettings = TownOfUsPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

            // Role Options
            presetSelection = CustomOption.Create(0, Types.General, "Preset", presets, null, true);

            nonKillingNeutralRolesCountMin = CustomOption.Create(300, Types.General, "Minimum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f, null, true, heading: "Role Count Settings");
            nonKillingNeutralRolesCountMax = CustomOption.Create(301, Types.General, "Maximum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMin = CustomOption.Create(302, Types.General, "Minimum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMax = CustomOption.Create(303, Types.General, "Maximum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            modifiersCountMin = CustomOption.Create(304, Types.General, "Minimum Modifiers", 15f, 0f, 15f, 1f);
            modifiersCountMax = CustomOption.Create(305, Types.General, "Maximum Modifiers", 15f, 0f, 15f, 1f);

            isDraftMode = CustomOption.Create(900, Types.General, "Enable Draft Mode", false, null, true);
            draftModeAmountOfChoices = CustomOption.Create(901, Types.General, "Amount Of Choices", 3f, 2f, 6f, 1f, isDraftMode);
            draftModeTimeToChoose = CustomOption.Create(902, Types.General, "Time To Choose", 5f, 3f, 20f, 1f, isDraftMode);
            draftModeShowRoles = CustomOption.Create(903, Types.General, "Show Chosen Roles", false, isDraftMode);
            draftModeHideCrewmateRoles = CustomOption.Create(904, Types.General, "Hide Crewmate Roles", false, draftModeShowRoles);
            draftModeHideNeutralBenignRoles = CustomOption.Create(905, Types.General, "Hide Neutral Benign Roles", false, draftModeShowRoles);
            draftModeHideNeutralEvilRoles = CustomOption.Create(906, Types.General, "Hide Neutral Evil Roles", false, draftModeShowRoles);
            draftModeHideNeutralKillingRoles = CustomOption.Create(907, Types.General, "Hide Neutral Killing Roles", false, draftModeShowRoles);
            draftModeHideImpRoles = CustomOption.Create(908, Types.General, "Hide Impostor Roles", false, draftModeShowRoles);

            maxNumberOfMeetings = CustomOption.Create(1, Types.General, "Number Of Meetings", 10, 0, 15, 1, null, true, heading: "Gameplay Settings");
            initialCooldown = CustomOption.Create(7, Types.General, "Initial Buttons Cooldown", 15f, 10f, 30f, 2.5f);
            anyPlayerCanStopStart = CustomOption.Create(2, Types.General, "Any Player Can Stop The Start", false, null, false);
            hidePlayerNames = CustomOption.Create(3, Types.General, "Hide Player Names", false);
            allowParallelMedBayScans = CustomOption.Create(4, Types.General, "Allow Parallel MedBay Scans", false);
            shieldFirstKill = CustomOption.Create(5, Types.General, "Shield Last Game First Kill", false);
            enableBetterPolus = CustomOption.Create(6, Types.General, "Enable Better Polus", false);
            randomSpawnPositions = CustomOption.Create(8, Types.General, "Random Spawn Positions", new string[] { "Off", "1st Round", "Always" });

            sheriffBlockGameEnd = CustomOption.Create(800, Types.General, "Sheriff Blocks Game End", false, null, true, heading: "Extended End Game");
            mayorBlockGameEnd = CustomOption.Create(802, Types.General, "Mayor Blocks Game End", false);
            vampireHunterBlockGameEnd = CustomOption.Create(804, Types.General, "Vampire Hunter Blocks Game End", false);
            veteranBlockGameEnd = CustomOption.Create(801, Types.General, "Veteran Blocks Game End", false);
            swapperBlockGameEnd = CustomOption.Create(803, Types.General, "Swapper Blocks Game End", false);

            dynamicMap = CustomOption.Create(500, Types.General, "Play On A Random Map", false, null, true, heading: "Random Maps");
            dynamicMapEnableSkeld = CustomOption.Create(501, Types.General, "Skeld", rates, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(502, Types.General, "Mira", rates, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(503, Types.General, "Polus", rates, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(504, Types.General, "Airship", rates, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(506, Types.General, "Fungle", rates, dynamicMap, false);

            guesserGamemodeCrewNumber = CustomOption.Create(20001, Types.General, "Number of Crew Guessers", 15f, 0f, 15f, 1f, null, true, heading: "Amount of Guessers");
            guesserGamemodeBenignNeutralNumber = CustomOption.Create(20002, Types.General, "Number of Neutral Benign Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeEvilNeutralNumber = CustomOption.Create(20009, Types.General, "Number of Neutral Evil Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeKillingNeutralNumber = CustomOption.Create(20010, Types.General, "Number of Neutral Killing Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeImpNumber = CustomOption.Create(20003, Types.General, "Number of Impostor Guessers", 15f, 0f, 15f, 1f, null);
            guesserGamemodeNumberOfShots = CustomOption.Create(20004, Types.General, "Guesser Number Of Shots", 3f, 1f, 15f, 1f, null, true, heading: "Guesser Settings");
            guesserGamemodeHasMultipleShotsPerMeeting = CustomOption.Create(20005, Types.General, "Guesser Can Shoot Multiple Times Per Meeting", false, null);
            guesserGamemodeEvilCanKillAgent = CustomOption.Create(20012, Types.General, "Evil Guesser Can Guess Agent", false);
            guesserGamemodeKillsThroughShield = CustomOption.Create(20006, Types.General, "Guesses Ignore The Medic Shield", true, null);
            guesserGamemodeCrewGuesserNumberOfTasks = CustomOption.Create(20011, Types.General, "Completed Tasks To Unlock Crew Guesser", 4f, 0f, 15f, 1f);
            guesserGamemodeCantGuessSnitchIfTaksDone = CustomOption.Create(20007, Types.General, "Guesser Can't Guess Snitch When Tasks Completed", true, null);
            guesserGamemodeNewVampCanAssassinate = CustomOption.Create(20008, Types.General, "New Vampire Can Assassinate", true);

            #region Crewmate Roles
            agentSpawnRate = new CustomRoleOption(350, Types.Crewmate, "Agent", Agent.color);
            agentCanDieToSheriff = CustomOption.Create(351, Types.Crewmate, "Agent Can Die To Sheriff", false, agentSpawnRate);
            agentImpostorsCanKillAnyone = CustomOption.Create(352, Types.Crewmate, "Impostors Can Kill Anyone If There Is An Agent", false, agentSpawnRate);
            agentCanEnterVents = CustomOption.Create(353, Types.Crewmate, "Agent Can Enter Vents", false, agentSpawnRate);
            agentHasImpostorVision = CustomOption.Create(354, Types.Crewmate, "Agent Has Impostor Vision", false, agentSpawnRate);

            detectiveSpawnRate = new CustomRoleOption(370, Types.Crewmate, "Detective", Detective.color);
            detectiveInitialCooldown = CustomOption.Create(371, Types.Crewmate, "Initial Examine Cooldown", 30f, 10f, 60f, 2.5f, detectiveSpawnRate);
            detectiveCooldown = CustomOption.Create(372, Types.Crewmate, "Examine Cooldown", 10f, 1f, 20f, 0.5f, detectiveSpawnRate);
            detectiveBloodTime = CustomOption.Create(373, Types.Crewmate, "How Long Players Stay Bloody For", 30f, 10f, 60f, 2.5f, detectiveSpawnRate);
            detectiveGetInfoOnReport = CustomOption.Create(374, Types.Crewmate, "Show Detective Reports", false, detectiveSpawnRate);
            detectiveReportRoleDuration = CustomOption.Create(375, Types.Crewmate, "Time Where Detective Will Have Role", 15f, 0f, 60f, 2.5f, detectiveGetInfoOnReport);
            detectiveReportFactionDuration = CustomOption.Create(376, Types.Crewmate, "Time Where Detective Will Have Faction", 30f, 0f, 60f, 2.5f, detectiveGetInfoOnReport);
            detectiveGetExamineInfo = CustomOption.Create(377, Types.Crewmate, "Show Examine Reports", false, detectiveSpawnRate);

            engineerSpawnRate = new CustomRoleOption(115, Types.Crewmate, "Engineer", Engineer.color);
            engineerNumberOfFixes = CustomOption.Create(116, Types.Crewmate, "Number Of Repairs", 1f, 1f, 5f, 1f, engineerSpawnRate);

            investigatorSpawnRate = new CustomRoleOption(265, Types.Crewmate, "Investigator", Investigator.color);
            investigatorAnonymousFootprints = CustomOption.Create(267, Types.Crewmate, "Anonymous Footprints", false, investigatorSpawnRate);
            investigatorFootprintIntervall = CustomOption.Create(268, Types.Crewmate, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);
            investigatorFootprintDuration = CustomOption.Create(269, Types.Crewmate, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);

            lighterSpawnRate = new CustomRoleOption(365, Types.Crewmate, "Lighter", Lighter.color);
            lighterModeLightsOnVision = CustomOption.Create(366, Types.Crewmate, "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f, 0.25f, lighterSpawnRate);
            lighterModeLightsOffVision = CustomOption.Create(367, Types.Crewmate, "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f, 0.25f, lighterSpawnRate);
            lighterCooldown = CustomOption.Create(368, Types.Crewmate, "Lighter Cooldown", 30f, 10f, 60f, 2.5f, lighterSpawnRate);
            lighterDuration = CustomOption.Create(369, Types.Crewmate, "Lighter Duration", 10f, 1f, 20f, 0.5f, lighterSpawnRate);

            medicSpawnRate = new CustomRoleOption(210, Types.Crewmate, "Medic", Medic.color);
            medicShowShielded = CustomOption.Create(211, Types.Crewmate, "Show Shielded Player", new string[] { "Everyone", "Shielded", "Medic", "Shielded + Medic", "Nobody" }, medicSpawnRate);
            medicGetsNotification = CustomOption.Create(212, Types.Crewmate, "Show Murder Attempt", new string[] { "Everyone", "Shielded", "Medic", "Shielded + Medic", "Nobody" }, medicSpawnRate);
            medicGetsInfo = CustomOption.Create(213, Types.Crewmate, "Gets Dead Body Info On Report", false, medicSpawnRate);
            medicReportNameDuration = CustomOption.Create(214, Types.Crewmate, "Time Where Medic Reports Will Have Name", 0f, 0f, 30f, 0.25f, medicGetsInfo);
            medicReportColorDuration = CustomOption.Create(215, Types.Crewmate, "Time Where Medic Reports Will Have Color Type", 0f, 0f, 60f, 0.25f, medicGetsInfo);

            mysticSpawnRate = new CustomRoleOption(320, Types.Crewmate, "Mystic", Mystic.color);
            mysticArrowDuration = CustomOption.Create(321, Types.Crewmate, "Arrow Duration", 0.5f, 0.125f, 1f, 0.125f, mysticSpawnRate);

            oracleSpawnRate = new CustomRoleOption(280, Types.Crewmate, "Oracle", Oracle.color);
            oracleConfessCooldown = CustomOption.Create(281, Types.Crewmate, "Confess Cooldown", 30f, 10f, 60f, 2.5f, oracleSpawnRate);
            oracleRevealAccuracy = CustomOption.Create(282, Types.Crewmate, "Reveal Accuracy", 100f, 0f, 100f, 10f, oracleSpawnRate);
            oracleBenignNeutralsShowsEvil = CustomOption.Create(283, Types.Crewmate, "Neutral Benign Roles Shows Evil", false, oracleSpawnRate);
            oracleEvilNeutralsShowsEvil = CustomOption.Create(284, Types.Crewmate, "Neutral Evil Roles Shows Evil", false, oracleSpawnRate);
            oracleKillingNeutralsShowsEvil = CustomOption.Create(285, Types.Crewmate, "Neutral Killing Roles Shows Evil", false, oracleSpawnRate);

            politicianSpawnRate = new CustomRoleOption(165, Types.Crewmate, "Politician", Politician.color, 1);
            politicianCooldown = CustomOption.Create(166, Types.Crewmate, "Politician Campaign Cooldown", 30f, 10f, 60f, 2.5f, politicianSpawnRate);
            mayorBodyguardCooldown = CustomOption.Create(167, Types.Crewmate, "Mayor Bodyguard Cooldown", 30f, 10f, 60f, 2.5f, politicianSpawnRate);
            mayorBodyguardDuration = CustomOption.Create(168, Types.Crewmate, "Mayor Bodyguard Duration", 10f, 1f, 20f, 0.5f, politicianSpawnRate);

            seerSpawnRate = new CustomRoleOption(110, Types.Crewmate, "Seer", Seer.color);
            seerRevealCooldown = CustomOption.Create(111, Types.Crewmate, "Seer Reveal Cooldown", 30f, 10f, 60f, 2.5f, seerSpawnRate);
            seerBenignNeutralsShowsEvil = CustomOption.Create(112, Types.Crewmate, "Neutral Benign Roles Shows Evil", false, seerSpawnRate);
            seerEvilNeutralsShowsEvil = CustomOption.Create(113, Types.Crewmate, "Neutral Evil Roles Shows Evil", false, seerSpawnRate);
            seerKillingNeutralsShowsEvil = CustomOption.Create(114, Types.Crewmate, "Neutral Killing Roles Shows Evil", false, seerSpawnRate);

            sheriffSpawnRate = new CustomRoleOption(100, Types.Crewmate, "Sheriff", Sheriff.color);
            sheriffMissfireShootKillsTarget = CustomOption.Create(101, Types.Crewmate, "Missfire Shoot Kills Target", false, sheriffSpawnRate);
            sheriffShootCooldown = CustomOption.Create(102, Types.Crewmate, "Sheriff Shoot Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            sheriffCanKillBenignNeutrals = CustomOption.Create(103, Types.Crewmate, "Sheriff Can Kill Neutral Benign Roles", false, sheriffSpawnRate);
            sheriffCanKillEvilNeutrals = CustomOption.Create(104, Types.Crewmate, "Sheriff Can Kill Neutral Evil Roles", false, sheriffSpawnRate);
            sheriffCanKillKillingNeutrals = CustomOption.Create(105, Types.Crewmate, "Sheriff Can Kill Neutral Killing Roles", false, sheriffSpawnRate);

            snitchSpawnRate = new CustomRoleOption(130, Types.Crewmate, "Snitch", Snitch.color);
            snitchLeftTasksForReveal = CustomOption.Create(131, Types.Crewmate, "Task Count Left Where The Snitch Will Be Revealed", 5f, 0f, 25f, 1f, snitchSpawnRate);
            snitchIncludeBenignNeutral = CustomOption.Create(132, Types.Crewmate, "Snitch Reveal Neutral Benign Roles", false, snitchSpawnRate);
            snitchIncludeEvilNeutral = CustomOption.Create(133, Types.Crewmate, "Snitch Reveal Neutral Evil Roles", false, snitchSpawnRate);
            snitchIncludeKillingNeutral = CustomOption.Create(134, Types.Crewmate, "Snitch Reveal Neutral Killing Roles", false, snitchSpawnRate);

            spySpawnRate = new CustomRoleOption(245, Types.Crewmate, "Spy", Spy.color);

            swapperSpawnRate = new CustomRoleOption(195, Types.Crewmate, "Swapper", Swapper.color, 1);
            swapperCanCallEmergency = CustomOption.Create(196, Types.Crewmate, "Swapper Can Call Emergency", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(197, Types.Crewmate, "Swapper Can Only Swap Others", false, swapperSpawnRate);

            transporterSpawnRate = new CustomRoleOption(335, Types.Crewmate, "Transporter", Transporter.color);
            transporterCooldown = CustomOption.Create(336, Types.Crewmate, "Transport Cooldown", 30f, 10f, 60f, 2.5f, transporterSpawnRate);
            transporterNumberOfTransports = CustomOption.Create(337, Types.Crewmate, "Number Of Transports", 5f, 1f, 15f, 1f, transporterSpawnRate);

            vampireHunterSpawnRate = new CustomRoleOption(200, Types.Crewmate, "Vampire Hunter", VampireHunter.color, 1);
            vampireHunterStakeCooldown = CustomOption.Create(201, Types.Crewmate, "Stake Cooldown", 30f, 10f, 60f, 2.5f, vampireHunterSpawnRate);
            vampireHunterMaxFailedStakes = CustomOption.Create(202, Types.Crewmate, "Max Failed Stakes", 1f, 1f, 5f, 1f, vampireHunterSpawnRate);
            vampireHunterCanStakeRoundOne = CustomOption.Create(203, Types.Crewmate, "Can Stake Round One", false, vampireHunterSpawnRate);
            vampireHunterSelfKillAfterFinalStake = CustomOption.Create(204, Types.Crewmate, "Self Kill After Final Stake", false, vampireHunterSpawnRate);
            vampireHunterPromote = CustomOption.Create(205, Types.Crewmate, "Vampire Hunter Promotes To", new string[] { "Crewmate", "Sheriff", "Veteran" }, vampireHunterSpawnRate);

            veteranSpawnRate = new CustomRoleOption(140, Types.Crewmate, "Veteran", Veteran.color);
            veteranCooldown = CustomOption.Create(141, Types.Crewmate, "Alert Cooldown", 30f, 10f, 60f, 2.5f, veteranSpawnRate);
            veteranAlertDuration = CustomOption.Create(142, Types.Crewmate, "Alert Duration", 10f, 1f, 20f, 0.5f, veteranSpawnRate);
            veteranAlertNumber = CustomOption.Create(143, Types.Crewmate, "Number Of Alerts", 5f, 1f, 15f, 1f, veteranSpawnRate);
            #endregion

            #region Neutral Benign Roles
            amnesiacSpawnRate = new CustomRoleOption(235, Types.NeutralBenign, "Amnesiac", Amnesiac.color);
            amnesiacShowArrows = CustomOption.Create(236, Types.NeutralBenign, "Show Arrows To Dead Bodies", false, amnesiacSpawnRate);
            amnesiacDelay = CustomOption.Create(237, Types.NeutralBenign, "Arrow Appears Delay", 3f, 0f, 5f, 0.25f, amnesiacShowArrows);
            amnesiacResetRoleWhenTaken = CustomOption.Create(238, Types.NeutralBenign, "Reset Role When Taken", false, amnesiacSpawnRate);
            amnesiacCanRememberOnMeetingIfGuesser = CustomOption.Create(239, Types.NeutralBenign, "Can Remember On Meeting (If Guesser)", false, amnesiacSpawnRate);

            guardianAngelSpawnRate = new CustomRoleOption(220, Types.NeutralBenign, "Guardian Angel", GuardianAngel.color, 1);
            guardianAngelProtectCooldown = CustomOption.Create(221, Types.NeutralBenign, "GA Protect Cooldown", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            guardianAngelProtectDuration = CustomOption.Create(222, Types.NeutralBenign, "GA Protect Duration", 10f, 1f, 20f, 0.5f, guardianAngelSpawnRate);
            guardianAngelNumberOfProtects = CustomOption.Create(228, Types.NeutralBenign, "GA Number Of Protects", 5f, 1f, 15f, 1f, guardianAngelSpawnRate);
            guardianAngelKnowsRole = CustomOption.Create(223, Types.NeutralBenign, "GA Knows Target Role", false, guardianAngelSpawnRate);
            guardianAngelTargetKnows = CustomOption.Create(224, Types.NeutralBenign, "Target Knows GA Exists", false, guardianAngelSpawnRate);
            guardianAngelOddsEvilTarget = CustomOption.Create(230, Types.NeutralBenign, "Odds Of Target Being Evil", rates, guardianAngelSpawnRate);
            guardianAngelWhoSeesProtect = CustomOption.Create(225, Types.NeutralBenign, "Who Sees Protect", new string[] { "Everyone", "Protected", "GA", "Protected + GA", "Nobody" }, guardianAngelSpawnRate);
            survivorSafeguardCooldown = CustomOption.Create(226, Types.NeutralBenign, "Survivor Safeguard Cooldown", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            survivorSafeguardDuration = CustomOption.Create(227, Types.NeutralBenign, "Survivor Safeguard Duration", 10f, 1f, 20f, 0.5f, guardianAngelSpawnRate);
            survivorNumberOfSafeguards = CustomOption.Create(229, Types.NeutralBenign, "Survivor Number Of Safeguards", 5f, 1f, 15f, 1f, guardianAngelSpawnRate);

            lawyerSpawnRate = new CustomRoleOption(155, Types.NeutralBenign, "Lawyer", Lawyer.color, 1);
            lawyerTargetKnows = CustomOption.Create(156, Types.NeutralBenign, "Target Knows Lawyer Exists", true, lawyerSpawnRate);
            lawyerVision = CustomOption.Create(157, Types.NeutralBenign, "Lawyer Vision Multiplier", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate);
            lawyerKnowsRole = CustomOption.Create(158, Types.NeutralBenign, "Lawyer Knows Target Role", false, lawyerSpawnRate);
            lawyerWinsAfterMeetings = CustomOption.Create(159, Types.NeutralBenign, "Lawyer Needed Meeting To Win", false, lawyerSpawnRate);
            lawyerNeededMeetings = CustomOption.Create(160, Types.NeutralBenign, "Number Of Meetings Required", 5f, 1f, 15f, 1f, lawyerWinsAfterMeetings);
            pursuerCooldown = CustomOption.Create(161, Types.NeutralBenign, "Pursuer Blank Cooldown", 30f, 5f, 60f, 2.5f, lawyerSpawnRate);
            pursuerBlanksNumber = CustomOption.Create(162, Types.NeutralBenign, "Pursuer Number Of Blanks", 5f, 1f, 20f, 1f, lawyerSpawnRate);

            thiefSpawnRate = new CustomRoleOption(355, Types.NeutralBenign, "Thief", Thief.color);
            thiefCooldown = CustomOption.Create(356, Types.NeutralBenign, "Thief Cooldown", 30f, 10f, 60f, 2.5f, thiefSpawnRate);
            thiefHasImpVision = CustomOption.Create(357, Types.NeutralBenign, "Thief Has Impostor Vision", false, thiefSpawnRate);
            thiefCanUseVents = CustomOption.Create(358, Types.NeutralBenign, "Thief Can Use Vents", false, thiefSpawnRate);
            thiefCanKillMayor = CustomOption.Create(359, Types.NeutralBenign, "Thief Can Kill Mayor", false, thiefSpawnRate);
            thiefCanKillSheriff = CustomOption.Create(360, Types.NeutralBenign, "Thief Can Kill Sheriff", false, thiefSpawnRate);
            thiefCanKillVampireHunter = CustomOption.Create(361, Types.NeutralBenign, "Thief Can Kill Vampire Hunter", false, thiefSpawnRate);
            thiefCanKillVeteran = CustomOption.Create(362, Types.NeutralBenign, "Thief Can Kill Veteran", false, thiefSpawnRate);
            thiefCanStealWithGuess = CustomOption.Create(363, Types.NeutralBenign, "Thief Can Guess To Steal A Role (If Guesser)", false, thiefSpawnRate);
            #endregion

            #region Neutral Evil Roles
            executionerSpawnRate = new CustomRoleOption(240, Types.NeutralEvil, "Executioner", Executioner.color, 1);
            executionerVision = CustomOption.Create(241, Types.NeutralEvil, "Executioner Vision Multiplier", 1f, 0.25f, 3f, 0.25f, executionerSpawnRate);
            executionerCanCallEmergency = CustomOption.Create(242, Types.NeutralEvil, "Can Call Emergency", false, executionerSpawnRate);

            jesterSpawnRate = new CustomRoleOption(120, Types.NeutralEvil, "Jester", Jester.color);
            jesterHasImpostorVision = CustomOption.Create(121, Types.NeutralEvil, "Has Impostor Vision", false, jesterSpawnRate);
            jesterCanCallEmergency = CustomOption.Create(122, Types.NeutralEvil, "Can Call Emergency", false, jesterSpawnRate);

            mercenarySpawnRate = new CustomRoleOption(295, Types.NeutralEvil, "Mercenary", Mercenary.color);
            mercenaryShowShielded = CustomOption.Create(299, Types.Crewmate, "Show Shielded Player", new string[] { "Everyone", "Shielded", "Mercenary", "Shielded + Mercenary", "Nobody" }, mercenarySpawnRate);
            mercenaryGetsNotification = CustomOption.Create(306, Types.Crewmate, "Show Murder Attempt", new string[] { "Everyone", "Shielded", "Mercenary", "Shielded + Mercenary", "Nobody" }, mercenarySpawnRate);
            mercenaryBrildersRequired = CustomOption.Create(296, Types.NeutralEvil, "Brilders Required To Win", 3f, 1f, 15f, 1f, mercenarySpawnRate);
            mercenaryArmorCooldown = CustomOption.Create(297, Types.NeutralEvil, "Armor Cooldown", 30f, 10f, 60f, 2.5f, mercenarySpawnRate);
            mercenaryArmorDuration = CustomOption.Create(298, Types.NeutralEvil, "Armor Duration", 10f, 1f, 20f, 0.5f, mercenarySpawnRate);

            scavengerSpawnRate = new CustomRoleOption(255, Types.NeutralEvil, "Scavenger", Scavenger.color);
            scavengerCooldown = CustomOption.Create(256, Types.NeutralEvil, "Scavenger Cooldown", 15f, 10f, 60f, 2.5f, scavengerSpawnRate);
            scavengerNumberToWin = CustomOption.Create(257, Types.NeutralEvil, "Number Of Corpses Needed To Be Eaten", 4f, 1f, 10f, 1f, scavengerSpawnRate);
            scavengerCanUseVents = CustomOption.Create(258, Types.NeutralEvil, "Scavenger Can Use Vents", true, scavengerSpawnRate);
            scavengerShowArrows = CustomOption.Create(259, Types.NeutralEvil, "Show Arrows Pointing Towards The Corpses", true, scavengerSpawnRate);
            scavengerCanEatWithGuess = CustomOption.Create(260, Types.NeutralEvil, "Can Guess To Eat a Corpse (If Guesser)", true, scavengerSpawnRate);
            #endregion

            #region Neutral Killing Roles
            arsonistSpawnRate = new CustomRoleOption(340, Types.NeutralKilling, "Arsonist", Arsonist.color, 1);
            arsonistDouseCooldown = CustomOption.Create(341, Types.NeutralKilling, "Douse Cooldown", 12.5f, 5f, 60f, 2.5f, arsonistSpawnRate);
            arsonistIgniteCooldown = CustomOption.Create(342, Types.NeutralKilling, "Ignite Cooldown", 12.5f, 5f, 60f, 2.5f, arsonistSpawnRate);
            arsonistRemoveIgniteCooldown = CustomOption.Create(346, Types.NeutralKilling, "Remove Ignite Cd If Last Killer", false, arsonistSpawnRate);
            arsonistMaxAliveDoused = CustomOption.Create(343, Types.NeutralKilling, "Max Alive Doused", 5f, 2f, 15f, 1f, arsonistSpawnRate);
            arsonistHasImpostorVision = CustomOption.Create(344, Types.NeutralKilling, "Arsonist Has Impostor Vision", false, arsonistSpawnRate);
            arsonistCanVent = CustomOption.Create(345, Types.NeutralKilling, "Arsonist Can Vent", false, arsonistSpawnRate);

            draculaSpawnRate = new CustomRoleOption(180, Types.NeutralKilling, "Dracula", Dracula.color, 1);
            draculaAndVampireBiteCooldown = CustomOption.Create(181, Types.NeutralKilling, "Dracula And Vampire Bite Cooldown", 30f, 10f, 60f, 2.5f, draculaSpawnRate);
            draculaHasImpostorVision = CustomOption.Create(182, Types.NeutralKilling, "Dracula Has Impostor Vision", false, draculaSpawnRate);
            draculaCanVent = CustomOption.Create(183, Types.NeutralKilling, "Dracula Can Vent", false, draculaSpawnRate);
            draculaCanCreateVampire = CustomOption.Create(184, Types.NeutralKilling, "Dracula Can Create Vampires", false, draculaSpawnRate);
            draculaMaxVampires = CustomOption.Create(185, Types.NeutralKilling, "Max Vampires That Dracula Can Convert", 1f, 1f, 5f, 1f, draculaCanCreateVampire);
            draculaCanConvertBenignNeutral = CustomOption.Create(186, Types.NeutralKilling, "Dracula Can Convert Neutral Benign Roles", false, draculaCanCreateVampire);
            draculaCanConvertEvilNeutral = CustomOption.Create(187, Types.NeutralKilling, "Dracula Can Convert Neutral Evil Roles", false, draculaCanCreateVampire);
            draculaCanConvertKillingNeutral = CustomOption.Create(188, Types.NeutralKilling, "Dracula Can Convert Neutral Killing Roles", false, draculaCanCreateVampire);
            draculaCanConvertImpostor = CustomOption.Create(189, Types.NeutralKilling, "Dracula Can Convert Impostor Roles", false, draculaCanCreateVampire);
            vampireHasImpostorVision = CustomOption.Create(190, Types.NeutralKilling, "Vampire Has Impostor Vision", false, draculaCanCreateVampire);
            vampireCanVent = CustomOption.Create(191, Types.NeutralKilling, "Vampire Can Vent", false, draculaCanCreateVampire);

            glitchSpawnRate = new CustomRoleOption(325, Types.NeutralKilling, "Glitch", Glitch.color, 1);
            glitchMimicCooldown = CustomOption.Create(326, Types.NeutralKilling, "Mimic Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchMimicDuration = CustomOption.Create(327, Types.NeutralKilling, "Mimic Duration", 10f, 1f, 20f, 0.5f, glitchSpawnRate);
            glitchHackCooldown = CustomOption.Create(328, Types.NeutralKilling, "Hack Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchHackDuration = CustomOption.Create(329, Types.NeutralKilling, "Hack Duration", 10f, 1f, 20f, 0.5f, glitchSpawnRate);
            glitchKillCooldown = CustomOption.Create(330, Types.NeutralKilling, "Kill Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchCanVent = CustomOption.Create(331, Types.NeutralKilling, "Glitch Can Use Vents", false, glitchSpawnRate);
            glitchHasImpostorVision = CustomOption.Create(332, Types.NeutralKilling, "Glitch Has Impostor Vision", false, glitchSpawnRate);

            juggernautSpawnRate = new CustomRoleOption(125, Types.NeutralKilling, "Juggernaut", Juggernaut.color, 1);
            juggernautCooldown = CustomOption.Create(126, Types.NeutralKilling, "Initial Kill Cooldown", 30f, 10f, 60f, 2.5f, juggernautSpawnRate);
            juggernautCooldownReduce = CustomOption.Create(127, Types.NeutralKilling, "Cooldown Reduce On Kill", 10f, 1f, 20f, 0.5f, juggernautSpawnRate);
            juggernautHasImpostorVision = CustomOption.Create(128, Types.NeutralKilling, "Juggernaut Has Impostor Vision", false, juggernautSpawnRate);
            juggernautCanVent = CustomOption.Create(129, Types.NeutralKilling, "Juggernaut Can Use Vents", false, juggernautSpawnRate);

            plaguebearerSpawnRate = new CustomRoleOption(170, Types.NeutralKilling, "Plaguebearer", Plaguebearer.color, 1);
            plaguebearerCooldown = CustomOption.Create(171, Types.NeutralKilling, "Plaguebearer Infect Cooldown", 30f, 10f, 60f, 2.5f, plaguebearerSpawnRate);
            pestilenceCooldown = CustomOption.Create(172, Types.NeutralKilling, "Pestilence Kill Cooldown", 15f, 2.5f, 60f, 2.5f, plaguebearerSpawnRate);
            pestilenceHasImpostorVision = CustomOption.Create(173, Types.NeutralKilling, "Pestilence Has Impostor Vision", false, plaguebearerSpawnRate);
            pestilenceCanVent = CustomOption.Create(174, Types.NeutralKilling, "Pestilence Can Vent", false, plaguebearerSpawnRate);

            werewolfSpawnRate = new CustomRoleOption(175, Types.NeutralKilling, "Werewolf", Werewolf.color, 1);
            werewolfRampageCooldown = CustomOption.Create(176, Types.NeutralKilling, "Rampage Cooldown", 30f, 10f, 60f, 2.5f, werewolfSpawnRate);
            werewolfRampageDuration = CustomOption.Create(177, Types.NeutralKilling, "Rampage Duration", 10f, 1f, 20f, 0.5f, werewolfSpawnRate);
            werewolfKillCooldown = CustomOption.Create(178, Types.NeutralKilling, "Werewolf Kill Cooldown", 3f, 1f, 20f, 0.5f, werewolfSpawnRate);
            werewolfCanVent = CustomOption.Create(179, Types.NeutralKilling, "Werewolf Can Vent While Rampaged", false, werewolfSpawnRate);
            #endregion

            #region Impostor Roles
            blackmailerSpawnRate = new CustomRoleOption(310, Types.Impostor, "Blackmailer", Blackmailer.color);
            blackmailerCooldown = CustomOption.Create(311, Types.Impostor, "Blackmail Cooldown", 30f, 2.5f, 60f, 2.5f, blackmailerSpawnRate);
            blackmailerBlockTargetVote = CustomOption.Create(312, Types.Impostor, "Blackmail Target Cant Vote", false, blackmailerSpawnRate);
            blackmailerBlockTargetAbility = CustomOption.Create(313, Types.Impostor, "Blackmail Target Cant Use Meeting Abilities", false, blackmailerSpawnRate);

            bomberSpawnRate = new CustomRoleOption(380, Types.Impostor, "Bomber", Bomber.color, 1);
            bomberMaxKillsInDetonation = CustomOption.Create(382, Types.Impostor, "Max Kills In Detonation", 5f, 1f, 15f, 1f, bomberSpawnRate);
            bomberDetonateRadius = CustomOption.Create(383, Types.Impostor, "Detonate Radius", 0.25f, 0.125f, 1f, 0.125f, bomberSpawnRate);

            bountyHunterSpawnRate = new CustomRoleOption(320, Types.Impostor, "Bounty Hunter", BountyHunter.color);
            bountyHunterBountyDuration = CustomOption.Create(321, Types.Impostor, "Duration After Which Bounty Changes", 60f, 10f, 180f, 10f, bountyHunterSpawnRate);
            bountyHunterReducedCooldown = CustomOption.Create(322, Types.Impostor, "Cooldown After Killing Bounty", 2.5f, 0f, 30f, 2.5f, bountyHunterSpawnRate);
            bountyHunterPunishmentTime = CustomOption.Create(323, Types.Impostor, "Additional Cooldown After Killing Others", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate);
            bountyHunterShowArrow = CustomOption.Create(324, Types.Impostor, "Show Arrow Pointing Towards The Bounty", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateIntervall = CustomOption.Create(325, Types.Impostor, "Arrow Update Intervall", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow);

            camouflagerSpawnRate = new CustomRoleOption(145, Types.Impostor, "Camouflager", Camouflager.color, 1);
            camouflagerCooldown = CustomOption.Create(146, Types.Impostor, "Camo Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);
            camouflagerDuration = CustomOption.Create(147, Types.Impostor, "Camo Duration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate);

            grenadierSpawnRate = new CustomRoleOption(315, Types.Impostor, "Grenadier", Grenadier.color, 1);
            grenadierCooldown = CustomOption.Create(316, Types.Impostor, "Grenadier Cooldown", 30f, 10f, 60f, 2.5f, grenadierSpawnRate);
            grenadierDuration = CustomOption.Create(317, Types.Impostor, "Flash Duration", 10f, 1f, 20f, 0.5f, grenadierSpawnRate);
            grenadierFlashRadius = CustomOption.Create(318, Types.Impostor, "Flash Radius", new string[] { "All Map", "0.25x", "0.5x", "0.75x", "1x", "1.25x", "1.5x", "1.75x", "2x", "2.25x", "2.5x", "2.75x", "3x" }, grenadierSpawnRate);
            grenadierIndicateCrewmates = CustomOption.Create(319, Types.Impostor, "Indicate Flashed Crewmates", false, grenadierSpawnRate);

            janitorSpawnRate = new CustomRoleOption(270, Types.Impostor, "Janitor", Janitor.color);
            janitorCooldown = CustomOption.Create(271, Types.Impostor, "Janitor Cooldown", 30f, 10f, 60f, 2.5f, janitorSpawnRate);

            morphlingSpawnRate = new CustomRoleOption(150, Types.Impostor, "Morphling", Morphling.color);
            morphlingCooldown = CustomOption.Create(151, Types.Impostor, "Morph Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);
            morphlingDuration = CustomOption.Create(152, Types.Impostor, "Morph Duration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate);

            poisonerSpawnRate = new CustomRoleOption(250, Types.Impostor, "Poisoner", Poisoner.color);
            poisonerKillDelay = CustomOption.Create(251, Types.Impostor, "Poison Kills Delay", 10f, 1f, 20f, 1f, poisonerSpawnRate);
            poisonerCooldown = CustomOption.Create(252, Types.Impostor, "Poison Cooldown", 30f, 10f, 60f, 2.5f, poisonerSpawnRate);

            swooperSpawnRate = new CustomRoleOption(275, Types.Impostor, "Swooper", Swooper.color);
            swooperCooldown = CustomOption.Create(276, Types.Impostor, "Swoop Cooldown", 30f, 10f, 60f, 2.5f, swooperSpawnRate);
            swooperDuration = CustomOption.Create(277, Types.Impostor, "Swoop Duration", 10f, 1f, 20f, 0.5f, swooperSpawnRate);

            venererSpawnRate = new CustomRoleOption(385, Types.Impostor, "Venerer", Venerer.color);
            venererCooldown = CustomOption.Create(386, Types.Impostor, "Ability Cooldown", 30f, 10f, 60f, 2.5f, venererSpawnRate);
            venererDuration = CustomOption.Create(387, Types.Impostor, "Ability Duration", 10f, 1f, 20f, 0.5f, venererSpawnRate);
            venererSpeedMultiplier = CustomOption.Create(388, Types.Impostor, "Sprint Multiplier", 1.125f, 1.125f, 2.5f, 0.125f, venererSpawnRate);
            venererFreezeMultiplier = CustomOption.Create(389, Types.Impostor, "Freeze Multiplier", 0.5f, 0f, 0.875f, 0.125f, venererSpawnRate);
            #endregion

            #region Modifiers
            // Crewmate Modifiers
            modifierBait = new CustomModifierOption(1000, Types.Modifier, "Bait", Bait.color);
            modifierBaitReportDelay = CustomOption.Create(1001, Types.Modifier, "Bait Report Delay", 0f, 0f, 10f, 1f, modifierBait);
            modifierBaitShowKillFlash = CustomOption.Create(1002, Types.Modifier, "Warn The Killer With A Flash", false, modifierBait);

            modifierBlind = new CustomModifierOption(1003, Types.Modifier, "Blind", Blind.color);

            modifierIndomitable = new CustomModifierOption(1007, Types.Modifier, "Indomitable", Indomitable.color);

            modifierShifter = new CustomModifierOption(1005, Types.Modifier, "Shifter", Shifter.color, 1);
            modifierShifterShiftsMedicShield = CustomOption.Create(1006, Types.Modifier, "Can Shift Medic Shield", false, modifierShifter);

            modifierTorch = new CustomModifierOption(1009, Types.Modifier, "Torch", Torch.color);

            modifierVip = new CustomModifierOption(1011, Types.Modifier, "Vip", Vip.color);
            // Global Modifiers
            modifierButtonBarry = new CustomModifierOption(1010, Types.Modifier, "Button Barry", ButtonBarry.color);
            modifierDrunk = new CustomModifierOption(1008, Types.Modifier, "Drunk", Drunk.color);
            modifierRadar = new CustomModifierOption(1012, Types.Modifier, "Radar", Radar.color);
            modifierSleuth = new CustomModifierOption(1004, Types.Modifier, "Sleuth", Sleuth.color);
            // Impostor Modifiers
            #endregion
        }
    }
}