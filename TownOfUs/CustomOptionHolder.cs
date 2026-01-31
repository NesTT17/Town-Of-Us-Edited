using System.Collections.Generic;
using UnityEngine;
using Types = TownOfUs.Modules.CustomOptionType;

namespace TownOfUs
{
    public class CustomOptionHolder
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] ratesModifier = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
        public static string[] presets = new string[] { "Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5" };
        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();
        public static string cs(Color c, string s) => Helpers.cs(c, s);

        public static CustomOption presetSelection;
        public static CustomOption crewmateRolesCountMin;
        public static CustomOption crewmateRolesCountMax;
        public static CustomOption crewmateRolesFill;
        public static CustomOption nonKillingNeutralRolesCountMin;
        public static CustomOption nonKillingNeutralRolesCountMax;
        public static CustomOption killingNeutralRolesCountMin;
        public static CustomOption killingNeutralRolesCountMax;
        public static CustomOption impostorRolesCountMin;
        public static CustomOption impostorRolesCountMax;
        public static CustomOption modifiersCountMin;
        public static CustomOption modifiersCountMax;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption blockSkippingInEmergencyMeetings;
        public static CustomOption noVoteIsSelfVote;
        public static CustomOption anyPlayerCanStopStart;
        public static CustomOption hidePlayerNames;
        public static CustomOption allowParallelMedBayScans;
        public static CustomOption finishTasksBeforeHauntingOrZoomingOut;
        public static CustomOption killPlayersInVent;
        public static CustomOption abilityPlayersInVent;
        public static CustomOption randomSpawnPositions;
        public static CustomOption enableBetterPolus;
        public static CustomOption shieldFirstKill;

        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableAirShip;
        public static CustomOption dynamicMapEnableFungle;
        
        //Guesser Gamemode
        public static CustomOption guesserGamemodeCrewmateNumber;
        public static CustomOption guesserGamemodeNeutralBenignNumber;
        public static CustomOption guesserGamemodeNeutralEvilNumber;
        public static CustomOption guesserGamemodeNeutralKillingNumber;
        public static CustomOption guesserGamemodeImpostorNumber;

        public static CustomOption guesserGamemodeCrewmateGuesserCanGuessCrewmateRoles;
        public static CustomOption guesserGamemodeCrewmateGuesserCanGuessNeutralBenign;
        public static CustomOption guesserGamemodeCrewmateGuesserCanGuessNeutralEvil;
        public static CustomOption guesserGamemodeCrewmateGuesserCanGuessNeutralKilling;

        public static CustomOption guesserGamemodeImpostorGuesserCanGuessNeutralBenign;
        public static CustomOption guesserGamemodeImpostorGuesserCanGuessNeutralEvil;
        public static CustomOption guesserGamemodeImpostorGuesserCanGuessNeutralKilling;
        public static CustomOption guesserGamemodeImpostorGuesserCanGuessImpostorRoles;

        public static CustomOption guesserGamemodeNeutralBenignGuesserCanGuessCrewmateRoles;
        public static CustomOption guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign;
        public static CustomOption guesserGamemodeNeutralBenignGuesserCanGuessNeutralEvil;
        public static CustomOption guesserGamemodeNeutralBenignGuesserCanGuessNeutralKilling;
        public static CustomOption guesserGamemodeNeutralBenignGuesserCanGuessImpostorRoles;

        public static CustomOption guesserGamemodeNeutralEvilGuesserCanGuessCrewmateRoles;
        public static CustomOption guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign;
        public static CustomOption guesserGamemodeNeutralEvilGuesserCanGuessNeutralEvil;
        public static CustomOption guesserGamemodeNeutralEvilGuesserCanGuessNeutralKilling;
        public static CustomOption guesserGamemodeNeutralEvilGuesserCanGuessImpostorRoles;

        public static CustomOption guesserGamemodeNeutralKillingGuesserCanGuessCrewmateRoles;
        public static CustomOption guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign;
        public static CustomOption guesserGamemodeNeutralKillingGuesserCanGuessNeutralEvil;
        public static CustomOption guesserGamemodeNeutralKillingGuesserCanGuessNeutralKilling;
        public static CustomOption guesserGamemodeNeutralKillingGuesserCanGuessImpostorRoles;

        public static CustomOption guesserGamemodeHaveModifier;
        public static CustomOption guesserGamemodeNumberOfShots;
        public static CustomOption guesserGamemodeHasMultipleShotsPerMeeting;
        public static CustomOption guesserGamemodeKillsThroughShield;
        public static CustomOption guesserGamemodeCantGuessSnitchIfTaksDone;
        public static CustomOption guesserGamemodeCrewGuesserNumberOfTasks;
        public static CustomOption guesserGamemodeVampireGetsGuesser;

        public static CustomOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffCanKillNeutralBenign;
        public static CustomOption sheriffCanKillNeutralEvil;
        public static CustomOption sheriffCanKillNeutralKilling;
        public static CustomOption sheriffWhoDiesOnMissfire;

        public static CustomOption survivorSpawnRate;
        public static CustomOption survivorSafeguardCooldown;
        public static CustomOption survivorSafeguardDuration;
        public static CustomOption survivorCooldownReset;
        public static CustomOption survivorNumberOfSafeguards;

        public static CustomOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterHasImpostorVision;
        public static CustomOption jesterCanEnterVents;
        public static CustomOption jesterWinEndsGame;

        public static CustomOption juggernautSpawnRate;
        public static CustomOption juggernautCooldown;
        public static CustomOption juggernautCooldownDecrease;
        public static CustomOption juggernautHasImpostorVision;
        public static CustomOption juggernautCanVent;

        public static CustomOption seerSpawnRate;
        public static CustomOption seerRevealCooldown;
        public static CustomOption seerSheriffShowsEvil;
        public static CustomOption seerVeteranShowsEvil;
        public static CustomOption seerBenignNeutralsShowsEvil;
        public static CustomOption seerEvilNeutralsShowsEvil;
        public static CustomOption seerKillingNeutralsShowsEvil;

        public static CustomOption engineerSpawnRate;
        public static CustomOption engineerNumberOfRepairs;
        public static CustomOption engineerCanRecharge;
        public static CustomOption engineerRechargeTasksNumber;

        public static CustomOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchMode;
        public static CustomOption snitchTargets;

        public static CustomOption veteranSpawnRate;
        public static CustomOption veteranAlertCooldown;
        public static CustomOption veteranAlertDuration;
        public static CustomOption veteranNumberOfAlerts;
        public static CustomOption veteranCanRecharge;
        public static CustomOption veteranRechargeTasksNumber;

        public static CustomOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;
        public static CustomOption camouflagerCanVent;

        public static CustomOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;
        public static CustomOption morphlingCanVent;

        public static CustomOption pursuerSpawnRate;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;

        public static CustomOption amnesiacSpawnRate;
        public static CustomOption amnesiacShowArrows;
        public static CustomOption amnesiacDelay;
        public static CustomOption amnesiacResetRole;
        public static CustomOption amnesiacCanRememberOnMeetingIfGuesser;

        public static CustomOption thiefSpawnRate;
        public static CustomOption thiefCooldown;
        public static CustomOption thiefHasImpVision;
        public static CustomOption thiefCanUseVents;
        public static CustomOption thiefCanKillSheriff;
        public static CustomOption thiefCanKillVeteran;
        public static CustomOption thiefCanStealWithGuess;

        public static CustomOption lawyerSpawnRate;
        public static CustomOption lawyerTargetCanBeJester;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption lawyerTargetKnows;
        public static CustomOption lawyerCanCallEmergency;
        public static CustomOption lawyerBecomeOnTargetDeath;

        public static CustomOption executionerSpawnRate;
        public static CustomOption executionerVision;
        public static CustomOption executionerKnowsRole;
        public static CustomOption executionerCanCallEmergency;
        public static CustomOption executionerBecomeOnTargetDeath;
        public static CustomOption executionerWinEndsGame;
        
        public static CustomOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicGetsNotification;
        public static CustomOption medicSetOrShowShieldAfterMeeting;
        public static CustomOption medicShieldUnbreakable;
        public static CustomOption medicInfoReport;
        public static CustomOption medicReportNameDuration;
        public static CustomOption medicReportColorDuration;
        
        public static CustomOption swapperSpawnRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;
        public static CustomOption swapperSwapsNumber;
        public static CustomOption swapperRechargeTasksNumber;
        
        public static CustomOption investigatorSpawnRate;
        public static CustomOption investigatorAnonymousFootprints;
        public static CustomOption investigatorFootprintIntervall;
        public static CustomOption investigatorFootprintDuration;
        
        public static CustomOption spySpawnRate;
        public static CustomOption spyCooldown;
        public static CustomOption spyDuration;
        public static CustomOption spyToolsNumber;
        public static CustomOption spyRechargeTasksNumber;
        public static CustomOption spyNoMove;

        public static CustomOption vigilanteSpawnRate;
        public static CustomOption vigilanteKills;
        public static CustomOption vigilanteMultiKill;
        public static CustomOption vigilanteGuessCrewmateRoles;
        public static CustomOption vigilanteGuessNeutralBenign;
        public static CustomOption vigilanteGuessNeutralEvil;
        public static CustomOption vigilanteGuessNeutralKilling;
        public static CustomOption vigilanteKillsThroughShield;
        
        public static CustomOption assassinSpawnRate;
        public static CustomOption assassinKills;
        public static CustomOption assassinMultiKill;
        public static CustomOption assassinGuessNeutralBenign;
        public static CustomOption assassinGuessNeutralEvil;
        public static CustomOption assassinGuessNeutralKilling;
        public static CustomOption assassinGuessImpostorRoles;
        public static CustomOption assassinKillsThroughShield;
        public static CustomOption assassinCantGuessSnitch;

        public static CustomOption trackerSpawnRate;
        public static CustomOption trackerNumberOfTracks;
        public static CustomOption trackerCooldown;
        public static CustomOption trackerUpdateIntervall;
        public static CustomOption trackerResetTargetAfterMeeting;
        
        public static CustomOption trapperSpawnRate;
        public static CustomOption trapperTrapCooldown;
        public static CustomOption trapperTrapsRemoveOnNewRound;
        public static CustomOption trapperMaxTraps;
        public static CustomOption trapperMinAmountOfTimeInTrap;
        public static CustomOption trapperTrapSize;
        public static CustomOption trapperMinAmountOfPlayersInTrap;

        public static CustomOption detectiveSpawnRate;
        public static CustomOption detectiveCooldown;
        public static CustomOption detectiveReportInfo;
        public static CustomOption detectiveReportRoleDuration;
        public static CustomOption detectiveReportFactionDuration;
        
        public static CustomOption mysticSpawnRate;
        public static CustomOption mysticArrowDuration;

        public static CustomOption guardianAngelSpawnRate;
        public static CustomOption guardianAngelCooldown;
        public static CustomOption guardianAngelDuration;
        public static CustomOption guardianAngelCooldownReset;
        public static CustomOption guardianAngelNumberOfProtects;
        public static CustomOption guardianAngelShowProtected;
        public static CustomOption guardianAngelVision;
        public static CustomOption guardianAngelKnowsRole;
        public static CustomOption guardianAngelCanCallEmergency;
        public static CustomOption guardianAngelTargetKnows;
        public static CustomOption guardianAngelBecomeOnTargetDeath;

        public static CustomOption swooperSpawnRate;
        public static CustomOption swooperCooldown;
        public static CustomOption swooperDuration;
        public static CustomOption swooperCanvent;
        
        public static CustomOption arsonistSpawnRate;
        public static CustomOption arsonistDouseCooldown;
        public static CustomOption arsonistIgniteCooldown;
        public static CustomOption arsonistTriggerBoth;
        public static CustomOption arsonistHasImpostorVision;
        public static CustomOption arsonistCanvent;

        public static CustomOption werewolfSpawnRate;
        public static CustomOption werewolfRampageCooldown;
        public static CustomOption werewolfRampageDuration;
        public static CustomOption werewolfRampageKillCooldown;
        public static CustomOption werewolfHasImpostorVision;
        public static CustomOption werewolfCanVent;

        public static CustomOption minerSpawnRate;
        public static CustomOption minerPlaceVentCooldown;
        
        public static CustomOption janitorSpawnRate;
        public static CustomOption janitorCooldown;

        public static CustomOption undertakerSpawnRate;
        public static CustomOption undertakerDragCooldown;
        public static CustomOption undertakerDragingAfterVelocity;
        public static CustomOption undertakerCanVent;
        public static CustomOption undertakerCanDragAndVent;
        
        public static CustomOption grenadierSpawnRate;
        public static CustomOption grenadierCooldown;
        public static CustomOption grenadierDuration;
        public static CustomOption grenadierFlashRadius;
        public static CustomOption grenadierIndicateCrewmates;
        
        public static CustomOption blackmailerSpawnRate;
        public static CustomOption blackmailerCooldown;
        public static CustomOption blackmailerBlockTargetVote;
        public static CustomOption blackmailerBlockTargetAbility;

        public static CustomOption politicianSpawnRate;
        public static CustomOption politicianCooldown;
        public static CustomOption mayorCooldown;
        public static CustomOption mayorDuration;

        public static CustomOption draculaSpawnRate;
        public static CustomOption draculaKillCooldown;
        public static CustomOption draculaCanUseVents;
        public static CustomOption draculaHaveImpostorVision;
        public static CustomOption draculaCanCreateVampire;
        public static CustomOption draculaNumOfCreatedVampires;
        public static CustomOption draculaCanCreateVampireFromNeutralBenign;
        public static CustomOption draculaCanCreateVampireFromNeutralEvil;
        public static CustomOption draculaCanCreateVampireFromNeutralKilling;
        public static CustomOption draculaCanCreateVampireFromImpostor;
        public static CustomOption vampireCanKill;
        public static CustomOption vampireKillCooldown;
        public static CustomOption vampireCanUseVents;
        public static CustomOption vampireHaveImpostorVision;
        
        public static CustomOption poisonerSpawnRate;
        public static CustomOption poisonerKillDelay;
        public static CustomOption poisonerCooldown;
        public static CustomOption poisonerTrapCooldown;
        public static CustomOption poisonerTrapDuration;
        public static CustomOption poisonerNumberOfTraps;
        public static CustomOption poisonerCanVent;

        public static CustomOption venererSpawnRate;
        public static CustomOption venererCooldown;
        public static CustomOption venererDuration;
        public static CustomOption venererSpeedMultiplier;
        public static CustomOption venererFreezeMultiplier;

        public static CustomOption plaguebearerSpawnRate;
        public static CustomOption plaguebearerCooldown;
        public static CustomOption pestilenceKillCooldown;
        public static CustomOption pestilenceHasImpostorVision;
        public static CustomOption pestilenceCanVent;

        public static CustomOption doomsayerSpawnRate;
        public static CustomOption doomsayerCooldown;
        public static CustomOption doomsayerHasMultipleShotsPerMeeting;
        public static CustomOption doomsayerCantGuessSnitch;
        public static CustomOption doomsayerCanKillsThroughShield;
        public static CustomOption doomsayerCanGuessNeutralBenign;
        public static CustomOption doomsayerCanGuessNeutralEvil;
        public static CustomOption doomsayerCanGuessNeutralKilling;
        public static CustomOption doomsayerCanGuessImpostor;
        public static CustomOption doomsayerKillToWin;
        public static CustomOption doomsayerWinEndsGame;
        
        public static CustomOption glitchSpawnRate;
        public static CustomOption glitchMimicCooldown;
        public static CustomOption glitchMimicDuration;
        public static CustomOption glitchHackCooldown;
        public static CustomOption glitchHackDuration;
        public static CustomOption glitchKillCooldown;
        public static CustomOption glitchCanVent;
        public static CustomOption glitchHasImpostorVision;

        public static CustomOption cannibalSpawnRate;
        public static CustomOption cannibalCooldown;
        public static CustomOption cannibalNumberToWin;
        public static CustomOption cannibalCanUseVents;
        public static CustomOption cannibalShowArrows;
        public static CustomOption cannibalEatWithGuess;
        public static CustomOption cannibalWinEndsGame;
        
        public static CustomOption scavengerSpawnRate;
        public static CustomOption scavengerBountyDuration;
        public static CustomOption scavengerReducedCooldown;
        public static CustomOption scavengerPunishmentTime;
        public static CustomOption scavengerShowArrow;
        public static CustomOption scavengerArrowUpdateIntervall;

        public static CustomOption escapistSpawnRate;
        public static CustomOption escapistCooldown;
        public static CustomOption escapistCanVent;
        
        public static CustomOption vampireHunterSpawnRate;
        public static CustomOption vampireHunterCooldown;
        public static CustomOption vampireHunterMaxFailedStakes;
        public static CustomOption vampireHunterCanStakeRoundOne;
        public static CustomOption vampireHunterSelfKillOnFailStakes;
        public static CustomOption vampireHunterBecome;
        
        public static CustomOption oracleSpawnRate;
        public static CustomOption oracleConfessCooldown;
        public static CustomOption oracleRevealAccuracy;
        public static CustomOption oracleBenignNeutralsShowsEvil;
        public static CustomOption oracleEvilNeutralsShowsEvil;
        public static CustomOption oracleKillingNeutralsShowsEvil;
        
        public static CustomOption lookoutSpawnRate;
        public static CustomOption lookoutCooldown;
        public static CustomOption lookoutDuration;
        public static CustomOption lookoutOrthographicSize;
        public static CustomOption lookoutRemainingZooms;
        public static CustomOption lookoutCanRecharge;
        public static CustomOption lookoutRechargeTasksNumber;

        public static CustomOption plumberSpawnRate;
        public static CustomOption plumberFlushCooldown;
        public static CustomOption plumberNumberOfFlushs;
        public static CustomOption plumberCanRecharge;
        public static CustomOption plumberRechargeTasksNumber;
        public static CustomOption plumberSealCooldown;
        public static CustomOption plumberNumberOfVents;

        public static CustomOption modifierBait;
        public static CustomOption modifierBaitQuantity;
        public static CustomOption modifierBaitReportDelayMin;
        public static CustomOption modifierBaitReportDelayMax;
        public static CustomOption modifierBaitShowKillFlash;

        public static CustomOption modifierBlind;

        public static CustomOption modifierButtonBarry;

        public static CustomOption modifierShy;
        public static CustomOption modifierShyQuantity;
        public static CustomOption modifierShyHoldDuration;
        public static CustomOption modifierShyFadeDuration;
        public static CustomOption modifierShyMinVisibility;

        public static CustomOption modifierFlash;
        public static CustomOption modifierFlashQuantity;
        public static CustomOption modifierFlashSpeed;
        
        public static CustomOption modifierMini;
        public static CustomOption modifierMiniGrowingUpDuration;
        public static CustomOption modifierMiniGrowingUpInMeeting;

        public static CustomOption modifierIndomitable;
        
        public static CustomOption modifierLover;
        public static CustomOption modifierLoverImpLoverRate;
        public static CustomOption modifierLoverBothDie;
        public static CustomOption modifierLoverEnableChat;

        public static CustomOption modifierMultitasker;
        public static CustomOption modifierMultitaskerQuantity;

        public static CustomOption modifierRadar;

        public static CustomOption modifierSleuth;

        public static CustomOption modifierTieBreaker;

        public static CustomOption modifierTorch;
        public static CustomOption modifierTorchQuantity;
        public static CustomOption modifierTorchVision;
        
        public static CustomOption modifierVip;
        public static CustomOption modifierVipQuantity;
        public static CustomOption modifierVipShowColor;

        public static CustomOption modifierDrunk;
        public static CustomOption modifierDrunkQuantity;
        public static CustomOption modifierDrunkDuration;
        
        public static CustomOption modifierImmovable;
        public static CustomOption modifierImmovableQuantity;

        public static CustomOption modifierDoubleShot;

        public static CustomOption modifierRuthless;

        public static CustomOption modifierUnderdog;
        public static CustomOption modifierUnderdogCooldownAddition;
        public static CustomOption modifierUnderdogIncreaseCooldown;
        
        public static CustomOption modifierSaboteur;
        public static CustomOption modifierSaboteurReduceSaboCooldown;
        
        public static CustomOption modifierFrosty;
        public static CustomOption modifierFrostyDuration;
        public static CustomOption modifierFrostyStartSpeed;

        public static CustomOption modifierSatelite;
        public static CustomOption modifierSateliteTrackingCooldown;
        public static CustomOption modifierSateliteTrackingDuration;

        public static CustomOption modifierSixthSense;

        public static CustomOption modifierTaskmaster;
        
        public static CustomOption modifierDisperser;
        public static CustomOption modifierDisperserNumOfDisperses;
        public static CustomOption modifierDisperserDisperseToVents;
        public static CustomOption modifierDisperserCanRecharge;
        public static CustomOption modifierDisperserRechargeKillsNumber;

        public static CustomOption modifierPoucher;

        public static CustomOption bansheeSpawnRate;
        public static CustomOption bansheeCooldown;
        public static CustomOption bansheeDuration;
        
        public static CustomOption poltergeistSpawnRate;
        public static CustomOption poltergeistCooldown;
        public static CustomOption poltergeistRadius;

        public static CustomOption deceiverSpawnRate;
        public static CustomOption deceiverPlaceCooldown;
        public static CustomOption deceiverDecoyDelayedDisplay;
        public static CustomOption deceiverDecoyPermanent;
        public static CustomOption deceiverResetPlaceAfterMeeting;
        public static CustomOption deceiverDecoyDuration;
        public static CustomOption deceiverSwapCooldown;
        public static CustomOption deceiverShowDecoy;

        public static void Load()
        {
            CustomOption.vanillaSettings = TownOfUsPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

            #region General Settings
            // Role Options
            presetSelection = CustomOption.Create(0, Types.General, "Preset", presets, null, true);

            crewmateRolesCountMin = CustomOption.Create(300, Types.General, "Minimum Crewmate Roles", 15f, 0f, 15f, 1f, null, true, heading: "Role Count Settings");
            crewmateRolesCountMax = CustomOption.Create(301, Types.General, "Maximum Crewmate Roles", 15f, 0f, 15f, 1f);
            nonKillingNeutralRolesCountMin = CustomOption.Create(302, Types.General, "Minimum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f);
            nonKillingNeutralRolesCountMax = CustomOption.Create(303, Types.General, "Maximum Neutral Non-Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMin = CustomOption.Create(304, Types.General, "Minimum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            killingNeutralRolesCountMax = CustomOption.Create(305, Types.General, "Maximum Neutral Killing Roles", 15f, 0f, 15f, 1f);
            impostorRolesCountMin = CustomOption.Create(306, Types.General, "Minimum Impostor Roles", 3f, 0f, 3f, 1f);
            impostorRolesCountMax = CustomOption.Create(307, Types.General, "Maximum Impostor Roles", 3f, 0f, 3f, 1f);
            modifiersCountMin = CustomOption.Create(308, Types.General, "Minimum Modifiers", 15f, 0f, 15f, 1f);
            modifiersCountMax = CustomOption.Create(309, Types.General, "Maximum Modifiers", 15f, 0f, 15f, 1f);
            crewmateRolesFill = CustomOption.Create(310, Types.General, "Fill Crewmate Roles\n(Ignores Min/Max)", false);

            maxNumberOfMeetings = CustomOption.Create(1, Types.General, "Number Of Meetings", 10, 0, 15, 1, null, true, heading: "Gameplay Settings");
            blockSkippingInEmergencyMeetings = CustomOption.Create(2, Types.General, "Block Skipping In Emergency Meetings", new string[] { "Off", "Emergency", "Always" });
            noVoteIsSelfVote = CustomOption.Create(3, Types.General, "No Vote Is Self Vote", false, blockSkippingInEmergencyMeetings);
            anyPlayerCanStopStart = CustomOption.Create(4, Types.General, "Any Player Can Stop The Start", false);
            hidePlayerNames = CustomOption.Create(5, Types.General, "Hide Player Names", false);
            allowParallelMedBayScans = CustomOption.Create(6, Types.General, "Allow Parallel MedBay Scans", false);
            finishTasksBeforeHauntingOrZoomingOut = CustomOption.Create(7, Types.General, "Finish Tasks Before Haunting Or Zooming Out", true);
            killPlayersInVent = CustomOption.Create(8, Types.General, "Allow Kill Player In Vent", false);
            abilityPlayersInVent = CustomOption.Create(9, Types.General, "Allow Use Ability On Player In Vent", false);
            randomSpawnPositions = CustomOption.Create(10, Types.General, "Random Spawn Positions", new string[] { "Off", "1st Round", "Always" });
            enableBetterPolus = CustomOption.Create(11, Types.General, "Enable Better Polus", false);
            shieldFirstKill = CustomOption.Create(12, Types.General, "Shield Last Game First Kill", false);

            dynamicMap = CustomOption.Create(500, Types.General, "Play On A Random Map", false, null, true, heading: "Random Maps");
            dynamicMapEnableSkeld = CustomOption.Create(501, Types.General, "Skeld Rotation", rates, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(502, Types.General, "Mira Rotation", rates, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(503, Types.General, "Polus Rotation", rates, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(504, Types.General, "Airship Rotation", rates, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(506, Types.General, "Fungle Rotation", rates, dynamicMap, false);
            #endregion

            #region Guesser Gamemode
            guesserGamemodeCrewmateNumber = CustomOption.Create(2000, Types.Guesser, "Number Of Crewmate Guessers", 15f, 0f, 15f, 1f, null, true, heading: "Number Of Guessers");
            guesserGamemodeNeutralBenignNumber = CustomOption.Create(2001, Types.Guesser, "Number Of Neutral Benign Guessers", 15f, 0f, 15f, 1f);
            guesserGamemodeNeutralEvilNumber = CustomOption.Create(2002, Types.Guesser, "Number Of Neutral Evil Guessers", 15f, 0f, 15f, 1f);
            guesserGamemodeNeutralKillingNumber = CustomOption.Create(2003, Types.Guesser, "Number Of Neutral Killing Guessers", 15f, 0f, 15f, 1f);
            guesserGamemodeImpostorNumber = CustomOption.Create(2004, Types.Guesser, "Number Of Impostor Guessers", 15f, 0f, 15f, 1f);

            guesserGamemodeCrewmateGuesserCanGuessCrewmateRoles = CustomOption.Create(2005, Types.Guesser, "Crewmate Guesser Can Guess Crewmate Roles", false, null, true, heading: "Crewmate Guessers Settings");
            guesserGamemodeCrewmateGuesserCanGuessNeutralBenign = CustomOption.Create(2006, Types.Guesser, "Crewmate Guesser Can Guess Neutral Benign Roles", false);
            guesserGamemodeCrewmateGuesserCanGuessNeutralEvil = CustomOption.Create(2007, Types.Guesser, "Crewmate Guesser Can Guess Neutral Evil Roles", false);
            guesserGamemodeCrewmateGuesserCanGuessNeutralKilling = CustomOption.Create(2008, Types.Guesser, "Crewmate Guesser Can Guess Neutral Killing Roles", false);

            guesserGamemodeNeutralBenignGuesserCanGuessCrewmateRoles = CustomOption.Create(2009, Types.Guesser, "Neutral Benign Guesser Can Guess Crewmate Roles", false, null, true, heading: "Neutral Benign Guessers Settings");
            guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign = CustomOption.Create(2010, Types.Guesser, "Neutral Benign Guesser Can Guess Neutral Benign Roles", false);
            guesserGamemodeNeutralBenignGuesserCanGuessNeutralEvil = CustomOption.Create(2011, Types.Guesser, "Neutral Benign Guesser Can Guess Neutral Evil Roles", false);
            guesserGamemodeNeutralBenignGuesserCanGuessNeutralKilling = CustomOption.Create(2012, Types.Guesser, "Neutral Benign Guesser Can Guess Neutral Killing Roles", false);
            guesserGamemodeNeutralBenignGuesserCanGuessImpostorRoles = CustomOption.Create(2013, Types.Guesser, "Neutral Benign Guesser Can Guess Impostor Roles", false);

            guesserGamemodeNeutralEvilGuesserCanGuessCrewmateRoles = CustomOption.Create(2014, Types.Guesser, "Neutral Evil Guesser Can Guess Crewmate Roles", false, null, true, heading: "Neutral Evil Guessers Settings");
            guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign = CustomOption.Create(2015, Types.Guesser, "Neutral Evil Guesser Can Guess Neutral Benign Roles", false);
            guesserGamemodeNeutralEvilGuesserCanGuessNeutralEvil = CustomOption.Create(2016, Types.Guesser, "Neutral Evil Guesser Can Guess Neutral Evil Roles", false);
            guesserGamemodeNeutralEvilGuesserCanGuessNeutralKilling = CustomOption.Create(2017, Types.Guesser, "Neutral Evil Guesser Can Guess Neutral Killing Roles", false);
            guesserGamemodeNeutralEvilGuesserCanGuessImpostorRoles = CustomOption.Create(2018, Types.Guesser, "Neutral Evil Guesser Can Guess Impostor Roles", false);

            guesserGamemodeNeutralKillingGuesserCanGuessCrewmateRoles = CustomOption.Create(2019, Types.Guesser, "Neutral Killing Guesser Can Guess Crewmate Roles", false, null, true, heading: "Neutral Killing Guessers Settings");
            guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign = CustomOption.Create(2020, Types.Guesser, "Neutral Killing Guesser Can Guess Neutral Benign Roles", false);
            guesserGamemodeNeutralKillingGuesserCanGuessNeutralEvil = CustomOption.Create(2021, Types.Guesser, "Neutral Killing Guesser Can Guess Neutral Evil Roles", false);
            guesserGamemodeNeutralKillingGuesserCanGuessNeutralKilling = CustomOption.Create(2022, Types.Guesser, "Neutral Killing Guesser Can Guess Neutral Killing Roles", false);
            guesserGamemodeNeutralKillingGuesserCanGuessImpostorRoles = CustomOption.Create(2023, Types.Guesser, "Neutral Killing Guesser Can Guess Impostor Roles", false);

            guesserGamemodeImpostorGuesserCanGuessNeutralBenign = CustomOption.Create(2024, Types.Guesser, "Impostor Guesser Can Guess Neutral Benign Roles", false, null, true, heading: "Impostor Guessers Settings");
            guesserGamemodeImpostorGuesserCanGuessNeutralEvil = CustomOption.Create(2025, Types.Guesser, "Impostor Guesser Can Guess Neutral Evil Roles", false);
            guesserGamemodeImpostorGuesserCanGuessNeutralKilling = CustomOption.Create(2026, Types.Guesser, "Impostor Guesser Can Guess Neutral Killing Roles", false);
            guesserGamemodeImpostorGuesserCanGuessImpostorRoles = CustomOption.Create(2027, Types.Guesser, "Impostor Guesser Can Guess Impostor Roles", false);

            guesserGamemodeHaveModifier = CustomOption.Create(2028, Types.Guesser, "Guessers Can Have A Modifier", false, null, true, heading: "General Guessers Settings");
            guesserGamemodeNumberOfShots = CustomOption.Create(2029, Types.Guesser, "Guesser Number Of Shots", 5f, 1f, 15f, 1f);
            guesserGamemodeHasMultipleShotsPerMeeting = CustomOption.Create(2030, Types.Guesser, "Guesser Can Shoot Multiple Times Per Meeting", false);
            guesserGamemodeCrewGuesserNumberOfTasks = CustomOption.Create(2031, Types.Guesser, "Number Of Tasks Needed To Unlock Shooting\nFor Crew Guesser", 0f, 0f, 15f, 1f);
            guesserGamemodeKillsThroughShield = CustomOption.Create(2032, Types.Guesser, "Guesses Ignore The Medic Shield", false);
            guesserGamemodeCantGuessSnitchIfTaksDone = CustomOption.Create(2033, Types.Guesser, "Guesser Can't Guess Snitch When Tasks Completed", false);
            guesserGamemodeVampireGetsGuesser = CustomOption.Create(2034, Types.Guesser, "New Vampire Can Assassinate", false);
            #endregion

            #region Crewmate Roles
            detectiveSpawnRate = CustomOption.Create(195, Types.Crewmate, cs(Detective.color, "Detective"), rates, null, true);
            detectiveCooldown = CustomOption.Create(196, Types.Crewmate, "Examine Cooldown", 30f, 10f, 60f, 2.5f, detectiveSpawnRate);
            detectiveReportInfo = CustomOption.Create(197, Types.Crewmate, "Show Body Info On Report", false, detectiveSpawnRate);
            detectiveReportRoleDuration = CustomOption.Create(198, Types.Crewmate, "Time Where Detective Will Have Role", 15f, 0f, 60f, 2.5f, detectiveReportInfo);
            detectiveReportFactionDuration = CustomOption.Create(199, Types.Crewmate, "Time Where Detective Will Have Faction", 30f, 0f, 60f, 2.5f, detectiveReportInfo);

            engineerSpawnRate = CustomOption.Create(85, Types.Crewmate, cs(Engineer.color, "Engineer"), rates, null, true);
            engineerNumberOfRepairs = CustomOption.Create(86, Types.Crewmate, "Number Of Repairs", 1f, 1f, 3f, 1f, engineerSpawnRate);
            engineerCanRecharge = CustomOption.Create(87, Types.Crewmate, "Can Recharge Repairs", false, engineerSpawnRate);
            engineerRechargeTasksNumber = CustomOption.Create(88, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 10f, 1f, engineerSpawnRate);

            investigatorSpawnRate = CustomOption.Create(165, Types.Crewmate, cs(Investigator.color, "Investigator"), rates, null, true);
            investigatorAnonymousFootprints = CustomOption.Create(166, Types.Crewmate, "Anonymous Footprints", false, investigatorSpawnRate);
            investigatorFootprintIntervall = CustomOption.Create(167, Types.Crewmate, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);
            investigatorFootprintDuration = CustomOption.Create(168, Types.Crewmate, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, investigatorSpawnRate);

            lookoutSpawnRate = CustomOption.Create(395, Types.Crewmate, cs(Lookout.color, "Lookout"), rates, null, true);
            lookoutCooldown = CustomOption.Create(396, Types.Crewmate, "Zoom Out Cooldown", 30f, 10f, 60f, 2.5f, lookoutSpawnRate);
            lookoutDuration = CustomOption.Create(397, Types.Crewmate, "Zoom Out Duration", 10f, 1f, 20f, 0.5f, lookoutSpawnRate);
            lookoutOrthographicSize = CustomOption.Create(398, Types.Crewmate, "Zoom Out Distance", 6f, 4f, 12f, 1f, lookoutSpawnRate);
            lookoutRemainingZooms = CustomOption.Create(399, Types.Crewmate, "Number Of Zooms", 3f, 1f, 5f, 1f, lookoutSpawnRate);
            lookoutCanRecharge = CustomOption.Create(400, Types.Crewmate, "Can Recharge", false, lookoutSpawnRate);
            lookoutRechargeTasksNumber = CustomOption.Create(401, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 10f, 1f, lookoutCanRecharge);

            medicSpawnRate = CustomOption.Create(150, Types.Crewmate, cs(Medic.color, "Medic"), rates, null, true);
            medicShowShielded = CustomOption.Create(151, Types.Crewmate, "Show Shielded Player", new string[] { "Everyone", "Shielded + Medic", "Medic", "Shielded", "Nobody" }, medicSpawnRate);
            medicGetsNotification = CustomOption.Create(154, Types.Crewmate, "Who Gets Notification On Murder Attempt", new string[] { "Everyone", "Shielded + Medic", "Medic", "Shielded", "Nobody" }, medicSpawnRate);
            medicSetOrShowShieldAfterMeeting = CustomOption.Create(152, Types.Crewmate, "Shield Will Be Activated", new string[] { "Instantly", "Instantly, Visible\nAfter Meeting", "After Meeting" }, medicSpawnRate);
            medicShieldUnbreakable = CustomOption.Create(153, Types.Crewmate, "Shield Is Unbreakable", true, medicSpawnRate);
            medicInfoReport = CustomOption.Create(155, Types.Crewmate, "Gets Info On Body Medic", false, medicSpawnRate);
            medicReportNameDuration = CustomOption.Create(156, Types.Crewmate, "Time Where Medic Reports Will Have Name", 0, 0, 60, 0.5f, medicInfoReport);
            medicReportColorDuration = CustomOption.Create(157, Types.Crewmate, "Time Where Medic Reports Will Have Color Type", 0, 0, 120, 0.5f, medicInfoReport);

            mysticSpawnRate = CustomOption.Create(200, Types.Crewmate, cs(Mystic.color, "Mystic"), rates, null, true);
            mysticArrowDuration = CustomOption.Create(201, Types.Crewmate, "Arrow Duration", 0.5f, 0.125f, 1f, 0.125f, mysticSpawnRate);

            oracleSpawnRate = CustomOption.Create(380, Types.Crewmate, cs(Oracle.color, "Oracle"), rates, null, true);
            oracleConfessCooldown = CustomOption.Create(381, Types.Crewmate, "Confess Cooldown", 30f, 10f, 60f, 2.5f, oracleSpawnRate);
            oracleRevealAccuracy = CustomOption.Create(382, Types.Crewmate, "Reveal Accuracy", rates, oracleSpawnRate);
            oracleBenignNeutralsShowsEvil = CustomOption.Create(383, Types.Crewmate, "Neutral Benign Roles Shows Evil", false, oracleSpawnRate);
            oracleEvilNeutralsShowsEvil = CustomOption.Create(384, Types.Crewmate, "Neutral Evil Roles Shows Evil", false, oracleSpawnRate);
            oracleKillingNeutralsShowsEvil = CustomOption.Create(385, Types.Crewmate, "Neutral Killing Roles Shows Evil", false, oracleSpawnRate);

            politicianSpawnRate = CustomOption.Create(270, Types.Crewmate, cs(Politician.color, "Politician"), rates, null, true);
            politicianCooldown = CustomOption.Create(271, Types.Crewmate, "Campaign Cooldown", 30f, 10f, 60f, 2.5f, politicianSpawnRate);
            mayorCooldown = CustomOption.Create(272, Types.Crewmate, "Mayor Bodyguard Cooldown", 30f, 10f, 60f, 2.5f, politicianSpawnRate);
            mayorDuration = CustomOption.Create(273, Types.Crewmate, "Mayor Bodyguard Duration", 10f, 1f, 20f, 0.5f, politicianSpawnRate);

            plumberSpawnRate = CustomOption.Create(405, Types.Crewmate, cs(Plumber.color, "Plumber"), rates, null, true);
            plumberFlushCooldown = CustomOption.Create(408, Types.Crewmate, "Flush Cooldown", 30f, 10f, 60f, 2.5f, plumberSpawnRate);
            plumberNumberOfFlushs = CustomOption.Create(409, Types.Crewmate, "Number Of Flushes", 3f, 1f, 5f, 1f, plumberSpawnRate);
            plumberCanRecharge = CustomOption.Create(410, Types.Crewmate, "Can Recharge Flushes", false, plumberSpawnRate);
            plumberRechargeTasksNumber = CustomOption.Create(411, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 10f, 1f, plumberCanRecharge);
            plumberSealCooldown = CustomOption.Create(406, Types.Crewmate, "Seal Vent Cooldown", 30f, 10f, 60f, 2.5f, plumberSpawnRate);
            plumberNumberOfVents = CustomOption.Create(407, Types.Crewmate, "Max Number Of Sealed Vents", 1f, 1f, 3f, 1f, plumberSpawnRate);

            seerSpawnRate = CustomOption.Create(75, Types.Crewmate, cs(Seer.color, "Seer"), rates, null, true);
            seerRevealCooldown = CustomOption.Create(76, Types.Crewmate, "Reveal Cooldown", 30f, 10f, 60f, 2.5f, seerSpawnRate);
            seerSheriffShowsEvil = CustomOption.Create(77, Types.Crewmate, "Sheriff Shows Evil", false, seerSpawnRate);
            seerVeteranShowsEvil = CustomOption.Create(81, Types.Crewmate, "Veteran Shows Evil", false, seerSpawnRate);
            seerBenignNeutralsShowsEvil = CustomOption.Create(78, Types.Crewmate, "Neutral Benign Roles Shows Evil", false, seerSpawnRate);
            seerEvilNeutralsShowsEvil = CustomOption.Create(79, Types.Crewmate, "Neutral Evil Roles Shows Evil", false, seerSpawnRate);
            seerKillingNeutralsShowsEvil = CustomOption.Create(80, Types.Crewmate, "Neutral Killing Roles Shows Evil", false, seerSpawnRate);

            sheriffSpawnRate = CustomOption.Create(50, Types.Crewmate, cs(Sheriff.color, "Sheriff"), rates, null, true);
            sheriffCooldown = CustomOption.Create(51, Types.Crewmate, "Shoot Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            sheriffCanKillNeutralBenign = CustomOption.Create(52, Types.Crewmate, "Can Kill Neutral Benign Roles", false, sheriffSpawnRate);
            sheriffCanKillNeutralEvil = CustomOption.Create(53, Types.Crewmate, "Can Kill Neutral Evil Roles", false, sheriffSpawnRate);
            sheriffCanKillNeutralKilling = CustomOption.Create(54, Types.Crewmate, "Can Kill Neutral Killing Roles", false, sheriffSpawnRate);
            sheriffWhoDiesOnMissfire = CustomOption.Create(55, Types.Crewmate, "Who Dies On Missfire", new string[] { "Target", "Sheriff", "Both" }, sheriffSpawnRate);

            snitchSpawnRate = CustomOption.Create(90, Types.Crewmate, cs(Snitch.color, "Snitch"), rates, null, true);
            snitchLeftTasksForReveal = CustomOption.Create(91, Types.Crewmate, "Task Count Where The Snitch Will Be Revealed", 5f, 0f, 25f, 1f, snitchSpawnRate);
            snitchMode = CustomOption.Create(92, Types.Crewmate, "Information Mode", new string[] { "Chat", "Map", "Chat & Map", "Arrows" }, snitchSpawnRate);
            snitchTargets = CustomOption.Create(93, Types.Crewmate, "Targets", new string[] { "All Evil Players", "Killing Players" }, snitchSpawnRate);

            spySpawnRate = CustomOption.Create(170, Types.Crewmate, cs(Spy.color, "Spy"), rates, null, true);
            spyCooldown = CustomOption.Create(171, Types.Crewmate, "Ability Cooldown", 30f, 10f, 60f, 2.5f, spySpawnRate);
            spyDuration = CustomOption.Create(172, Types.Crewmate, "Ability Duration", 10f, 1f, 20f, 0.5f, spySpawnRate);
            spyToolsNumber = CustomOption.Create(173, Types.Crewmate, "Max Mobile Gadget Charges", 5f, 1f, 30f, 1f, spySpawnRate);
            spyRechargeTasksNumber = CustomOption.Create(174, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 5f, 1f, spySpawnRate);
            spyNoMove = CustomOption.Create(175, Types.Crewmate, "Cant Move During Mobile Gadget Duration", true, spySpawnRate);

            swapperSpawnRate = CustomOption.Create(160, Types.Crewmate, cs(Swapper.color, "Swapper"), rates, null, true);
            swapperCanCallEmergency = CustomOption.Create(161, Types.Crewmate, "Swapper Can Call Emergency Meeting", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(162, Types.Crewmate, "Swapper Can Only Swap Others", false, swapperSpawnRate);
            swapperSwapsNumber = CustomOption.Create(163, Types.Crewmate, "Initial Swap Charges", 1f, 0f, 5f, 1f, swapperSpawnRate);
            swapperRechargeTasksNumber = CustomOption.Create(164, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 10f, 1f, swapperSpawnRate);

            trackerSpawnRate = CustomOption.Create(180, Types.Crewmate, cs(Tracker.color, "Tracker"), rates, null, true);
            trackerCooldown = CustomOption.Create(184, Types.Crewmate, "Track Cooldown", 30f, 2.5f, 60f, 2.5f, trackerSpawnRate);
            trackerNumberOfTracks = CustomOption.Create(181, Types.Crewmate, "Number Of Tracks", 1f, 1f, 5f, 1f, trackerSpawnRate);
            trackerUpdateIntervall = CustomOption.Create(182, Types.Crewmate, "Tracker Update Intervall", 3f, 0.5f, 30f, 0.5f, trackerSpawnRate);
            trackerResetTargetAfterMeeting = CustomOption.Create(183, Types.Crewmate, "Tracker Reset Target After Meeting", false, trackerSpawnRate);

            trapperSpawnRate = CustomOption.Create(185, Types.Crewmate, cs(Trapper.color, "Trapper"), rates, null, true);
            trapperTrapCooldown = CustomOption.Create(186, Types.Crewmate, "Trap Cooldown", 30f, 10f, 60f, 2.5f, trapperSpawnRate);
            trapperTrapsRemoveOnNewRound = CustomOption.Create(187, Types.Crewmate, "Traps Remove After Each Round", false, trapperSpawnRate);
            trapperMaxTraps = CustomOption.Create(188, Types.Crewmate, "Maximum Number Of Traps", 3f, 1f, 5f, 1f, trapperSpawnRate);
            trapperMinAmountOfTimeInTrap = CustomOption.Create(189, Types.Crewmate, "Min Amount Of Time In Trap To Register", 1f, 0f, 15f, 0.5f, trapperSpawnRate);
            trapperTrapSize = CustomOption.Create(190, Types.Crewmate, "Trap Size", 0.25f, 0.125f, 1f, 0.125f, trapperSpawnRate);
            trapperMinAmountOfPlayersInTrap = CustomOption.Create(191, Types.Crewmate, "Minimum Number Of Roles Required To Trigger Trap", 3f, 1f, 5f, 1f, trapperSpawnRate);

            vampireHunterSpawnRate = CustomOption.Create(370, Types.Crewmate, cs(VampireHunter.color, "Vampire Hunter"), rates, null, true);
            vampireHunterCooldown = CustomOption.Create(371, Types.Crewmate, "Stake Cooldown", 30f, 10f, 60f, 2.5f, vampireHunterSpawnRate);
            vampireHunterMaxFailedStakes = CustomOption.Create(372, Types.Crewmate, "Max Failed Stakes", 1f, 1f, 5f, 1f, vampireHunterSpawnRate);
            vampireHunterCanStakeRoundOne = CustomOption.Create(373, Types.Crewmate, "Can Stake Round One", false, vampireHunterSpawnRate);
            vampireHunterSelfKillOnFailStakes = CustomOption.Create(374, Types.Crewmate, "Self Kill On Failure To Kill A Vamp With All Stakes", false, vampireHunterSpawnRate);
            vampireHunterBecome = CustomOption.Create(375, Types.Crewmate, "Becomes On All Vampire Deaths", new string[] { "Sheriff", "Veteran", "Crewmate" }, vampireHunterSpawnRate);

            veteranSpawnRate = CustomOption.Create(95, Types.Crewmate, cs(Veteran.color, "Veteran"), rates, null, true);
            veteranAlertCooldown = CustomOption.Create(96, Types.Crewmate, "Alert Cooldown", 30f, 10f, 60f, 2.5f, veteranSpawnRate);
            veteranAlertDuration = CustomOption.Create(97, Types.Crewmate, "Alert Duration", 10f, 1f, 20f, 0.5f, veteranSpawnRate);
            veteranNumberOfAlerts = CustomOption.Create(98, Types.Crewmate, "Initial Number Of Alerts", 5f, 1f, 15f, 1f, veteranSpawnRate);
            veteranCanRecharge = CustomOption.Create(94, Types.Crewmate, "Can Recharge Alerts", false, veteranSpawnRate);
            veteranRechargeTasksNumber = CustomOption.Create(99, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 10f, 1f, veteranCanRecharge);

            vigilanteSpawnRate = CustomOption.Create(3000, Types.Crewmate, cs(Vigilante.color, "Vigilante"), rates, null, true);
            vigilanteKills = CustomOption.Create(3001, Types.Crewmate, "Number Of Shots", 1f, 1f, 15f, 1f, vigilanteSpawnRate);
            vigilanteMultiKill = CustomOption.Create(3002, Types.Crewmate, "Can Kill More Than Once Per Meeting", false, vigilanteSpawnRate);
            vigilanteGuessCrewmateRoles = CustomOption.Create(3003, Types.Crewmate, "Can Guess Crewmate Roles", false, vigilanteSpawnRate);
            vigilanteGuessNeutralBenign = CustomOption.Create(3004, Types.Crewmate, "Can Guess Neutral Benign Roles", false, vigilanteSpawnRate);
            vigilanteGuessNeutralEvil = CustomOption.Create(3005, Types.Crewmate, "Can Guess Neutral Evil Roles", false, vigilanteSpawnRate);
            vigilanteGuessNeutralKilling = CustomOption.Create(3006, Types.Crewmate, "Can Guess Neutral Killing Roles", false, vigilanteSpawnRate);
            vigilanteKillsThroughShield = CustomOption.Create(3014, Types.Crewmate, "Guesses Ignore Medic Shield", false, vigilanteSpawnRate);
            #endregion

            #region Neutral Benign Roles
            amnesiacSpawnRate = CustomOption.Create(115, Types.NeutralBenign, cs(Amnesiac.color, "Amnesiac"), rates, null, true);
            amnesiacShowArrows = CustomOption.Create(116, Types.NeutralBenign, "Show Arrows To Dead Bodies", false, amnesiacSpawnRate);
            amnesiacDelay = CustomOption.Create(117, Types.NeutralBenign, "Arrow Appears Delay", 3f, 0f, 5f, 0.25f, amnesiacShowArrows);
            amnesiacResetRole = CustomOption.Create(118, Types.NeutralBenign, "Reset Role When Taken", false, amnesiacSpawnRate);
            amnesiacCanRememberOnMeetingIfGuesser = CustomOption.Create(119, Types.NeutralBenign, "Can Remember On Meeting (If Guesser)", false, amnesiacSpawnRate);

            guardianAngelSpawnRate = CustomOption.Create(205, Types.NeutralBenign, cs(GuardianAngel.color, "Guardian Angel"), rates, null, true);
            guardianAngelVision = CustomOption.Create(211, Types.NeutralBenign, "Vision Multiplier", 1f, 0.25f, 3f, 0.25f, guardianAngelSpawnRate);
            guardianAngelCooldown = CustomOption.Create(206, Types.NeutralBenign, "Protect Cooldown", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            guardianAngelDuration = CustomOption.Create(207, Types.NeutralBenign, "Protect Duration", 10f, 1f, 20f, 0.5f, guardianAngelSpawnRate);
            guardianAngelCooldownReset = CustomOption.Create(208, Types.NeutralBenign, "Kill Cooldown Reset On Murder Attempt", 30f, 10f, 60f, 2.5f, guardianAngelSpawnRate);
            guardianAngelNumberOfProtects = CustomOption.Create(209, Types.NeutralBenign, "Number Of Protects", 5f, 1f, 15f, 1f, guardianAngelSpawnRate);
            guardianAngelShowProtected = CustomOption.Create(210, Types.NeutralBenign, "Show Protected Player", new string[] { "Everyone", "Protected + GA", "Guardian Angel", "Protected", "Nobody" }, guardianAngelSpawnRate);
            guardianAngelKnowsRole = CustomOption.Create(212, Types.NeutralBenign, "Knows Target Role", false, guardianAngelSpawnRate);
            guardianAngelCanCallEmergency = CustomOption.Create(213, Types.NeutralBenign, "Can Call Emergency", false, guardianAngelSpawnRate);
            guardianAngelTargetKnows = CustomOption.Create(214, Types.NeutralBenign, "Target Knows GA Exists", false, guardianAngelSpawnRate);
            guardianAngelBecomeOnTargetDeath = CustomOption.Create(215, Types.NeutralBenign, "Become On Target Death", new string[] { "Amnesiac", "Pursuer", "Survivor", "Thief", "Crewmate" }, guardianAngelSpawnRate);

            lawyerSpawnRate = CustomOption.Create(130, Types.NeutralBenign, cs(Lawyer.color, "Lawyer"), rates, null, true);
            lawyerTargetCanBeJester = CustomOption.Create(131, Types.NeutralBenign, "Target Can Be The Jester", false, lawyerSpawnRate);
            lawyerVision = CustomOption.Create(132, Types.NeutralBenign, "Vision Multiplier", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate);
            lawyerKnowsRole = CustomOption.Create(133, Types.NeutralBenign, "Knows Target Role", false, lawyerSpawnRate);
            lawyerTargetKnows = CustomOption.Create(134, Types.NeutralBenign, "Target Knows Lawyer Exists", false, lawyerSpawnRate);
            lawyerCanCallEmergency = CustomOption.Create(136, Types.NeutralBenign, "Can Call Emergency", false, lawyerSpawnRate);
            lawyerBecomeOnTargetDeath = CustomOption.Create(135, Types.NeutralBenign, "Become On Target Death", new string[] { "Amnesiac", "Pursuer", "Survivor", "Thief", "Crewmate" }, lawyerSpawnRate);

            pursuerSpawnRate = CustomOption.Create(110, Types.NeutralBenign, cs(Pursuer.color, "Pursuer"), rates, null, true);
            pursuerCooldown = CustomOption.Create(111, Types.NeutralBenign, "Blank Cooldown", 30f, 10f, 60f, 2.5f, pursuerSpawnRate);
            pursuerBlanksNumber = CustomOption.Create(112, Types.NeutralBenign, "Number Of Blanks", 5f, 1f, 15f, 1f, pursuerSpawnRate);

            survivorSpawnRate = CustomOption.Create(65, Types.NeutralBenign, cs(Survivor.color, "Survivor"), rates, null, true);
            survivorSafeguardCooldown = CustomOption.Create(66, Types.NeutralBenign, "Safeguard Cooldown", 30f, 10f, 60f, 2.5f, survivorSpawnRate);
            survivorSafeguardDuration = CustomOption.Create(67, Types.NeutralBenign, "Safeguard Duration", 10f, 1f, 20f, 0.5f, survivorSpawnRate);
            survivorCooldownReset = CustomOption.Create(68, Types.NeutralBenign, "Kill Cooldown Reset On Murder Attempt", 30f, 10f, 60f, 2.5f, survivorSpawnRate);
            survivorNumberOfSafeguards = CustomOption.Create(69, Types.NeutralBenign, "Number Of Safeguards", 5f, 1f, 15f, 1f, survivorSpawnRate);

            thiefSpawnRate = CustomOption.Create(120, Types.NeutralBenign, cs(Thief.color, "Thief"), rates, null, true);
            thiefCooldown = CustomOption.Create(121, Types.NeutralBenign, "Steal Cooldown", 30f, 10f, 60f, 2.5f, thiefSpawnRate);
            thiefCanKillSheriff = CustomOption.Create(122, Types.NeutralBenign, "Can Kill Sheriff", true, thiefSpawnRate);
            thiefCanKillVeteran = CustomOption.Create(126, Types.NeutralBenign, "Can Kill Veteran", true, thiefSpawnRate);
            thiefHasImpVision = CustomOption.Create(123, Types.NeutralBenign, "Has Impostor Vision", true, thiefSpawnRate);
            thiefCanUseVents = CustomOption.Create(124, Types.NeutralBenign, "Can Use Vents", true, thiefSpawnRate);
            thiefCanStealWithGuess = CustomOption.Create(125, Types.NeutralBenign, "Can Guess To Steal A Role (If Guesser)", false, thiefSpawnRate);
            #endregion

            #region Neutral Evil Roles
            cannibalSpawnRate = CustomOption.Create(345, Types.NeutralEvil, cs(Cannibal.color, "Cannibal"), rates, null, true);
            cannibalCooldown = CustomOption.Create(346, Types.NeutralEvil, "Eat Cooldown", 15f, 10f, 60f, 2.5f, cannibalSpawnRate);
            cannibalNumberToWin = CustomOption.Create(347, Types.NeutralEvil, "Number Of Corpses Needed To Be Eaten", 4f, 1f, 10f, 1f, cannibalSpawnRate);
            cannibalCanUseVents = CustomOption.Create(348, Types.NeutralEvil, "Can Use Vents", true, cannibalSpawnRate);
            cannibalShowArrows = CustomOption.Create(349, Types.NeutralEvil, "Show Arrows Pointing Towards The Corpses", true, cannibalSpawnRate);
            cannibalEatWithGuess = CustomOption.Create(350, Types.NeutralEvil, "Can Guess To Eat Body (If Guesser)", false, cannibalSpawnRate);
            cannibalWinEndsGame = CustomOption.Create(351, Types.NeutralEvil, "Win Ends Game", false, cannibalSpawnRate);

            doomsayerSpawnRate = CustomOption.Create(320, Types.NeutralEvil, cs(Doomsayer.color, "Doomsayer"), rates, null, true);
            doomsayerCooldown = CustomOption.Create(321, Types.NeutralEvil, "Observe Cooldown", 30f, 10f, 60f, 2.5f, doomsayerSpawnRate);
            doomsayerHasMultipleShotsPerMeeting = CustomOption.Create(322, Types.NeutralEvil, "Has Multiple Shots Per Meeting", false, doomsayerSpawnRate);
            doomsayerCantGuessSnitch = CustomOption.Create(329, Types.NeutralEvil, "Can't Guess Snitch When Tasks Completed", false, doomsayerSpawnRate);
            doomsayerCanKillsThroughShield = CustomOption.Create(330, Types.NeutralEvil, "Guesses Ignore Medic Shield", false, doomsayerSpawnRate);
            doomsayerCanGuessNeutralBenign = CustomOption.Create(323, Types.NeutralEvil, "Can Guess Neutral Benign Roles", false, doomsayerSpawnRate);
            doomsayerCanGuessNeutralEvil = CustomOption.Create(324, Types.NeutralEvil, "Can Guess Neutral Evil Roles", false, doomsayerSpawnRate);
            doomsayerCanGuessNeutralKilling = CustomOption.Create(325, Types.NeutralEvil, "Can Guess Neutral Killing Roles", false, doomsayerSpawnRate);
            doomsayerCanGuessImpostor = CustomOption.Create(326, Types.NeutralEvil, "Can Guess Impostor Roles", false, doomsayerSpawnRate);
            doomsayerKillToWin = CustomOption.Create(327, Types.NeutralEvil, "Number Of Guesses To Win", 3f, 2f, 5f, 1f, doomsayerSpawnRate);
            doomsayerWinEndsGame = CustomOption.Create(328, Types.NeutralEvil, "Win Ends Game", false, doomsayerSpawnRate);

            executionerSpawnRate = CustomOption.Create(140, Types.NeutralEvil, cs(Executioner.color, "Executioner"), rates, null, true);
            executionerVision = CustomOption.Create(141, Types.NeutralEvil, "Vision Multiplier", 1f, 0.25f, 3f, 0.25f, executionerSpawnRate);
            executionerKnowsRole = CustomOption.Create(142, Types.NeutralEvil, "Knows Target Role", false, executionerSpawnRate);
            executionerCanCallEmergency = CustomOption.Create(143, Types.NeutralEvil, "Can Call Emergency", false, executionerSpawnRate);
            executionerBecomeOnTargetDeath = CustomOption.Create(144, Types.NeutralEvil, "Become On Target Death", new string[] { "Amnesiac", "Pursuer", "Survivor", "Thief", "Crewmate" }, executionerSpawnRate);
            executionerWinEndsGame = CustomOption.Create(145, Types.NeutralEvil, "Win Ends Game", false, executionerSpawnRate);

            jesterSpawnRate = CustomOption.Create(60, Types.NeutralEvil, cs(Jester.color, "Jester"), rates, null, true);
            jesterCanCallEmergency = CustomOption.Create(61, Types.NeutralEvil, "Can Call Emergency", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(62, Types.NeutralEvil, "Has Impostor Vision", false, jesterSpawnRate);
            jesterCanEnterVents = CustomOption.Create(63, Types.NeutralEvil, "Can Hide In Vent", false, jesterSpawnRate);
            jesterWinEndsGame = CustomOption.Create(64, Types.NeutralEvil, "Win Ends Game", false, jesterSpawnRate);
            #endregion

            #region Neutral Killing Roles
            arsonistSpawnRate = CustomOption.Create(225, Types.NeutralKilling, cs(Arsonist.color, "Arsonist"), rates, null, true);
            arsonistDouseCooldown = CustomOption.Create(226, Types.NeutralKilling, "Douse Cooldown", 30f, 10f, 60f, 2.5f, arsonistSpawnRate);
            arsonistIgniteCooldown = CustomOption.Create(227, Types.NeutralKilling, "Ignite Cooldown", 30f, 10f, 60f, 2.5f, arsonistSpawnRate);
            arsonistTriggerBoth = CustomOption.Create(228, Types.NeutralKilling, "Trigger Both Cooldowns", true, arsonistSpawnRate);
            arsonistHasImpostorVision = CustomOption.Create(229, Types.NeutralKilling, "Has Impostor Vision", false, arsonistSpawnRate);
            arsonistCanvent = CustomOption.Create(230, Types.NeutralKilling, "Can Use Vents", false, arsonistSpawnRate);

            draculaSpawnRate = CustomOption.Create(275, Types.NeutralKilling, cs(Dracula.color, "Dracula"), rates, null, true);
            draculaKillCooldown = CustomOption.Create(276, Types.NeutralKilling, "Bite Cooldown", 30f, 10f, 60f, 2.5f, draculaSpawnRate);
            draculaCanUseVents = CustomOption.Create(277, Types.NeutralKilling, "Can Use Vents", false, draculaSpawnRate);
            draculaHaveImpostorVision = CustomOption.Create(278, Types.NeutralKilling, "Has Impostor Vision", false, draculaSpawnRate);
            draculaCanCreateVampire = CustomOption.Create(279, Types.NeutralKilling, "Can Create Vampire", false, draculaSpawnRate);
            draculaNumOfCreatedVampires = CustomOption.Create(280, Types.NeutralKilling, "Maximum Number Of Vampires", 1f, 1f, 5f, 1f, draculaCanCreateVampire);
            draculaCanCreateVampireFromNeutralBenign = CustomOption.Create(281, Types.NeutralKilling, "Can Create Vampire From Neutral Benign Role", false, draculaCanCreateVampire);
            draculaCanCreateVampireFromNeutralEvil = CustomOption.Create(286, Types.NeutralKilling, "Can Create Vampire From Neutral Evil Role", false, draculaCanCreateVampire);
            draculaCanCreateVampireFromNeutralKilling = CustomOption.Create(287, Types.NeutralKilling, "Can Create Vampire From Neutral Killing Role", false, draculaCanCreateVampire);
            draculaCanCreateVampireFromImpostor = CustomOption.Create(288, Types.NeutralKilling, "Can Create Vampire From Impostor Role", false, draculaCanCreateVampire);
            vampireCanKill = CustomOption.Create(282, Types.NeutralKilling, "Vampire Can Bite", false, draculaCanCreateVampire);
            vampireKillCooldown = CustomOption.Create(283, Types.NeutralKilling, "Vampire Bite Cooldown", 30f, 10f, 60f, 2.5f, vampireCanKill);
            vampireCanUseVents = CustomOption.Create(284, Types.NeutralKilling, "Vampire Can Use Vents", false, draculaCanCreateVampire);
            vampireHaveImpostorVision = CustomOption.Create(285, Types.NeutralKilling, "Vampire Has Impostor Vision", false, draculaCanCreateVampire);

            glitchSpawnRate = CustomOption.Create(335, Types.NeutralKilling, cs(Glitch.color, "Glitch"), rates, null, true);
            glitchMimicCooldown = CustomOption.Create(336, Types.NeutralKilling, "Mimic Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchMimicDuration = CustomOption.Create(337, Types.NeutralKilling, "Mimic Duration", 10f, 1f, 20f, 0.5f, glitchSpawnRate);
            glitchHackCooldown = CustomOption.Create(338, Types.NeutralKilling, "Hack Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchHackDuration = CustomOption.Create(339, Types.NeutralKilling, "Hack Duration", 10f, 1f, 20f, 0.5f, glitchSpawnRate);
            glitchKillCooldown = CustomOption.Create(340, Types.NeutralKilling, "Kill Cooldown", 30f, 10f, 60f, 2.5f, glitchSpawnRate);
            glitchHasImpostorVision = CustomOption.Create(341, Types.NeutralKilling, "Has Impostor Vision", false, glitchSpawnRate);
            glitchCanVent = CustomOption.Create(342, Types.NeutralKilling, "Can Use Vents", false, glitchSpawnRate);

            juggernautSpawnRate = CustomOption.Create(70, Types.NeutralKilling, cs(Juggernaut.color, "Juggernaut"), rates, null, true);
            juggernautCooldown = CustomOption.Create(71, Types.NeutralKilling, "Initial Kill Cooldown", 30f, 10f, 60f, 2.5f, juggernautSpawnRate);
            juggernautCooldownDecrease = CustomOption.Create(72, Types.NeutralKilling, "Kill Cooldown Decrease Per Kill", 10f, 1f, 20f, 0.5f, juggernautSpawnRate);
            juggernautHasImpostorVision = CustomOption.Create(73, Types.NeutralKilling, "Has Impostor Vision", false, juggernautSpawnRate);
            juggernautCanVent = CustomOption.Create(74, Types.NeutralKilling, "Can Use Vents", false, juggernautSpawnRate);

            plaguebearerSpawnRate = CustomOption.Create(315, Types.NeutralKilling, cs(Plaguebearer.color, "Plaguebearer"), rates, null, true);
            plaguebearerCooldown = CustomOption.Create(316, Types.NeutralKilling, "Infect Cooldown", 30f, 10f, 60f, 2.5f, plaguebearerSpawnRate);
            pestilenceKillCooldown = CustomOption.Create(317, Types.NeutralKilling, "Pestilence Kill Cooldown", 30f, 10f, 60f, 2.5f, plaguebearerSpawnRate);
            pestilenceHasImpostorVision = CustomOption.Create(318, Types.NeutralKilling, "Pestilence Has Impostor Vision", false, plaguebearerSpawnRate);
            pestilenceCanVent = CustomOption.Create(319, Types.NeutralKilling, "Pestilence Can Use Vents", false, plaguebearerSpawnRate);

            werewolfSpawnRate = CustomOption.Create(235, Types.NeutralKilling, cs(Werewolf.color, "Werewolf"), rates, null, true);
            werewolfRampageCooldown = CustomOption.Create(236, Types.NeutralKilling, "Rampage Cooldown", 30f, 10f, 60f, 2.5f, werewolfSpawnRate);
            werewolfRampageDuration = CustomOption.Create(237, Types.NeutralKilling, "Rampage Duration", 10f, 1f, 20f, 0.5f, werewolfSpawnRate);
            werewolfRampageKillCooldown = CustomOption.Create(238, Types.NeutralKilling, "Kill Cooldown", 3f, 0.5f, 10f, 0.5f, werewolfSpawnRate);
            werewolfHasImpostorVision = CustomOption.Create(239, Types.NeutralKilling, "Has Impostor Vision While Rampaged", false, werewolfSpawnRate);
            werewolfCanVent = CustomOption.Create(240, Types.NeutralKilling, "Can Use Vents While Rampaged", false, werewolfSpawnRate);
            #endregion

            #region Impostor Roles
            assassinSpawnRate = CustomOption.Create(3007, Types.Impostor, cs(Assassin.color, "Assassin"), rates, null, true);
            assassinKills = CustomOption.Create(3008, Types.Impostor, "Number Of Shots", 1f, 1f, 15f, 1f, assassinSpawnRate);
            assassinMultiKill = CustomOption.Create(3009, Types.Impostor, "Can Kill More Than Once Per Meeting", false, assassinSpawnRate);
            assassinGuessNeutralBenign = CustomOption.Create(3010, Types.Impostor, "Can Guess Neutral Benign Roles", false, assassinSpawnRate);
            assassinGuessNeutralEvil = CustomOption.Create(3011, Types.Impostor, "Can Guess Neutral Evil Roles", false, assassinSpawnRate);
            assassinGuessNeutralKilling = CustomOption.Create(3012, Types.Impostor, "Can Guess Neutral Killing Roles", false, assassinSpawnRate);
            assassinGuessImpostorRoles = CustomOption.Create(3013, Types.Impostor, "Can Guess Impostor Roles", false, assassinSpawnRate);
            assassinKillsThroughShield = CustomOption.Create(3015, Types.Impostor, "Guesses Ignore Medic Shield", false, assassinSpawnRate);
            assassinCantGuessSnitch = CustomOption.Create(3016, Types.Impostor, "Can't Guess Snitch When Tasks Completed", false, assassinSpawnRate);

            blackmailerSpawnRate = CustomOption.Create(265, Types.Impostor, cs(Blackmailer.color, "Blackmailer"), rates, null, true);
            blackmailerCooldown = CustomOption.Create(266, Types.Impostor, "Blackmail Cooldown", 30f, 2.5f, 60f, 2.5f, blackmailerSpawnRate);
            blackmailerBlockTargetVote = CustomOption.Create(267, Types.Impostor, "Blackmail Target Cant Vote", false, blackmailerSpawnRate);
            blackmailerBlockTargetAbility = CustomOption.Create(268, Types.Impostor, "Blackmail Target Cant Use Meeting Abilities", false, blackmailerSpawnRate);

            camouflagerSpawnRate = CustomOption.Create(100, Types.Impostor, cs(Camouflager.color, "Camouflager"), rates, null, true);
            camouflagerCooldown = CustomOption.Create(101, Types.Impostor, "Camo Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);
            camouflagerDuration = CustomOption.Create(102, Types.Impostor, "Camo Duration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate);
            camouflagerCanVent = CustomOption.Create(103, Types.Impostor, "Can Use Vents", true, camouflagerSpawnRate);

            deceiverSpawnRate = CustomOption.Create(425, Types.Impostor, cs(Deceiver.color, "Deceiver"), rates, null, true);
            deceiverPlaceCooldown = CustomOption.Create(426, Types.Impostor, "Place Cooldown", 30f, 10f, 60f, 2.5f, deceiverSpawnRate);
            deceiverDecoyDelayedDisplay = CustomOption.Create(427, Types.Impostor, "Decoy Delayed Activation", 10f, 3f, 20f, 0.5f, deceiverSpawnRate);
            deceiverResetPlaceAfterMeeting = CustomOption.Create(429, Types.Impostor, "Reset Deceiver After Meeting", false, deceiverSpawnRate);
            deceiverDecoyPermanent = CustomOption.Create(428, Types.Impostor, "Decoy Not Permanence", false, deceiverSpawnRate);
            deceiverDecoyDuration = CustomOption.Create(430, Types.Impostor, "Decoy Duration", 60f, 25f, 120f, 2.5f, deceiverDecoyPermanent);
            deceiverSwapCooldown = CustomOption.Create(431, Types.Impostor, "Swap Cooldown", 10f, 3f, 20f, 0.5f, deceiverSpawnRate);
            deceiverShowDecoy = CustomOption.Create(432, Types.Impostor, "Who Can See the Decoy", new string[] { "Only Deceiver", "Only Impostors", "All Players" }, deceiverSpawnRate);

            escapistSpawnRate = CustomOption.Create(365, Types.Impostor, cs(Escapist.color, "Escapist"), rates, null, true);
            escapistCooldown = CustomOption.Create(366, Types.Impostor, "Mark Cooldown", 30f, 10f, 60f, 2.5f, escapistSpawnRate);
            escapistCanVent = CustomOption.Create(367, Types.Impostor, "Can Use Vents", true, escapistSpawnRate);

            grenadierSpawnRate = CustomOption.Create(260, Types.Impostor, cs(Grenadier.color, "Grenadier"), rates, null, true);
            grenadierCooldown = CustomOption.Create(261, Types.Impostor, "Flash Cooldown", 30f, 10f, 60f, 2.5f, grenadierSpawnRate);
            grenadierDuration = CustomOption.Create(262, Types.Impostor, "Flash Duration", 10f, 1f, 20f, 0.5f, grenadierSpawnRate);
            grenadierFlashRadius = CustomOption.Create(263, Types.Impostor, "Flash Radius", new string[] { "All Map", "0.25x", "0.5x", "0.75x", "1x", "1.25x", "1.5x", "1.75x", "2x", "2.25x", "2.5x", "2.75x", "3x" }, grenadierSpawnRate);
            grenadierIndicateCrewmates = CustomOption.Create(264, Types.Impostor, "Indicate Flashed Crewmates", false, grenadierSpawnRate);

            janitorSpawnRate = CustomOption.Create(250, Types.Impostor, cs(Janitor.color, "Janitor"), rates, null, true);
            janitorCooldown = CustomOption.Create(251, Types.Impostor, "Clean Cooldown", 30f, 10f, 60f, 2.5f, janitorSpawnRate);

            minerSpawnRate = CustomOption.Create(245, Types.Impostor, cs(Miner.color, "Miner"), rates, null, true);
            minerPlaceVentCooldown = CustomOption.Create(246, Types.Impostor, "Place Vent Cooldown", 10f, 2.5f, 30f, 2.5f, minerSpawnRate);

            morphlingSpawnRate = CustomOption.Create(105, Types.Impostor, cs(Morphling.color, "Morphling"), rates, null, true);
            morphlingCooldown = CustomOption.Create(106, Types.Impostor, "Morph Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);
            morphlingDuration = CustomOption.Create(107, Types.Impostor, "Morph Duration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate);
            morphlingCanVent = CustomOption.Create(108, Types.Impostor, "Can Use Vents", true, morphlingSpawnRate);

            poisonerSpawnRate = CustomOption.Create(290, Types.Impostor, cs(Poisoner.color, "Poisoner"), rates, null, true);
            poisonerCooldown = CustomOption.Create(291, Types.Impostor, "Poison Cooldown", 30f, 10f, 60f, 2.5f, poisonerSpawnRate);
            poisonerKillDelay = CustomOption.Create(292, Types.Impostor, "Poison Kill Delay", 10f, 1f, 20f, 0.5f, poisonerSpawnRate);
            poisonerTrapCooldown = CustomOption.Create(294, Types.Impostor, "Blind Trap Cooldown", 10f, 3f, 20f, 0.5f, poisonerSpawnRate);
            poisonerTrapDuration = CustomOption.Create(289, Types.Impostor, "Blind Duration", 10f, 3f, 20f, 0.5f, poisonerSpawnRate);
            poisonerNumberOfTraps = CustomOption.Create(249, Types.Impostor, "Number Of Blind Traps", 3f, 1f, 15f, 1f, poisonerSpawnRate);
            poisonerCanVent = CustomOption.Create(293, Types.Impostor, "Can Use Vents", true, poisonerSpawnRate);

            scavengerSpawnRate = CustomOption.Create(355, Types.Impostor, cs(Scavenger.color, "Scavenger"), rates, null, true);
            scavengerBountyDuration = CustomOption.Create(356, Types.Impostor, "Duration After Which Bounty Changes", 60f, 10f, 180f, 10f, scavengerSpawnRate);
            scavengerReducedCooldown = CustomOption.Create(357, Types.Impostor, "Cooldown After Killing Bounty", 2.5f, 0f, 30f, 2.5f, scavengerSpawnRate);
            scavengerPunishmentTime = CustomOption.Create(358, Types.Impostor, "Additional Cooldown After Killing Others", 20f, 0f, 60f, 2.5f, scavengerSpawnRate);
            scavengerShowArrow = CustomOption.Create(359, Types.Impostor, "Show Arrow Pointing Towards The Bounty", true, scavengerSpawnRate);
            scavengerArrowUpdateIntervall = CustomOption.Create(360, Types.Impostor, "Arrow Update Intervall", 15f, 2.5f, 60f, 2.5f, scavengerShowArrow);

            swooperSpawnRate = CustomOption.Create(220, Types.Impostor, cs(Swooper.color, "Swooper"), rates, null, true);
            swooperCooldown = CustomOption.Create(221, Types.Impostor, "Swoop Cooldown", 30f, 10f, 60f, 2.5f, swooperSpawnRate);
            swooperDuration = CustomOption.Create(222, Types.Impostor, "Swoop Duration", 10f, 1f, 20f, 0.5f, swooperSpawnRate);
            swooperCanvent = CustomOption.Create(223, Types.Impostor, "Can Use Vents", true, swooperSpawnRate);

            venererSpawnRate = CustomOption.Create(295, Types.Impostor, cs(Venerer.color, "Venerer"), rates, null, true);
            venererCooldown = CustomOption.Create(296, Types.Impostor, "Ability Cooldown", 30f, 10f, 60f, 2.5f, venererSpawnRate);
            venererDuration = CustomOption.Create(297, Types.Impostor, "Ability Duration", 10f, 1f, 20f, 0.5f, venererSpawnRate);
            venererSpeedMultiplier = CustomOption.Create(298, Types.Impostor, "Speed Multiplier", 1.25f, 1.125f, 2.5f, 0.125f, venererSpawnRate);
            venererFreezeMultiplier = CustomOption.Create(299, Types.Impostor, "Freeze Multiplier", 0.25f, 0f, 0.875f, 0.125f, venererSpawnRate);

            undertakerSpawnRate = CustomOption.Create(255, Types.Impostor, cs(Undertaker.color, "Undertaker"), rates, null, true);
            undertakerDragCooldown = CustomOption.Create(256, Types.Impostor, "Drag Cooldown", 30f, 10f, 60f, 2.5f, undertakerSpawnRate);
            undertakerDragingAfterVelocity = CustomOption.Create(257, Types.Impostor, "Speed While Dragging", 0.75f, 0.5f, 1.5f, 0.125f, undertakerSpawnRate);
            undertakerCanVent = CustomOption.Create(258, Types.Impostor, "Can Use Vents", true, undertakerSpawnRate);
            undertakerCanDragAndVent = CustomOption.Create(259, Types.Impostor, "Can Use Vents While Dragging", false, undertakerSpawnRate);
            #endregion

            #region Ghost Roles
            poltergeistSpawnRate = CustomOption.Create(420, Types.CrewGhost, cs(Poltergeist.color, "Poltergeist"), rates, null, true);
            poltergeistCooldown = CustomOption.Create(421, Types.CrewGhost, "Drag Cooldown", 30f, 10f, 60f, 2.5f, poltergeistSpawnRate);
            poltergeistRadius = CustomOption.Create(422, Types.CrewGhost, "Drag Radius", 0.75f, 0.5f, 2f, 0.125f, poltergeistSpawnRate);

            bansheeSpawnRate = CustomOption.Create(415, Types.ImpGhost, cs(Banshee.color, "Banshee"), rates, null, true);
            bansheeCooldown = CustomOption.Create(416, Types.ImpGhost, "Scare Cooldown", 30f, 10f, 60f, 2.5f, bansheeSpawnRate);
            bansheeDuration = CustomOption.Create(417, Types.ImpGhost, "Scare Duration", 10f, 1f, 20f, 0.5f, bansheeSpawnRate);
            #endregion

            #region Modifiers
            modifierBait = CustomOption.Create(1000, Types.Modifier, cs(Bait.color, "Bait"), rates, null, true);
            modifierBaitQuantity = CustomOption.Create(1004, Types.Modifier, "Modifier Amount", ratesModifier, modifierBait);
            modifierBaitReportDelayMin = CustomOption.Create(1001, Types.Modifier, "Report Delay Min", 0f, 0f, 10f, 0.125f, modifierBait);
            modifierBaitReportDelayMax = CustomOption.Create(1002, Types.Modifier, "Report Delay Max", 0.5f, 0f, 10f, 0.5f, modifierBait);
            modifierBaitShowKillFlash = CustomOption.Create(1003, Types.Modifier, "Warn the Killer With a Flash", true, modifierBait);

            modifierBlind = CustomOption.Create(1005, Types.Modifier, cs(Blind.color, "Blind"), rates, null, true);

            modifierFrosty = CustomOption.Create(1060, Types.Modifier, cs(Frosty.color, "Frosty"), rates, null, true);
            modifierFrostyDuration = CustomOption.Create(1061, Types.Modifier, "Chill Duration", 10f, 1f, 20f, 1f, modifierFrosty);
            modifierFrostyStartSpeed = CustomOption.Create(1062, Types.Modifier, "Chill Start Speed", 0.5f, 0.125f, 0.875f, 0.125f, modifierFrosty);

            modifierIndomitable = CustomOption.Create(1018, Types.Modifier, cs(Indomitable.color, "Indomitable"), rates, null, true);

            modifierMultitasker = CustomOption.Create(1030, Types.Modifier, cs(Multitasker.color, "Multitasker"), rates, null, true);
            modifierMultitaskerQuantity = CustomOption.Create(1031, Types.Modifier, "Modifier Amount", ratesModifier, modifierMultitasker);

            modifierTaskmaster = CustomOption.Create(1069, Types.Modifier, cs(Taskmaster.color, "Taskmaster"), rates, null, true);

            modifierTorch = CustomOption.Create(1035, Types.Modifier, cs(Torch.color, "Torch"), rates, null, true);
            modifierTorchQuantity = CustomOption.Create(1036, Types.Modifier, "Modifier Amount", ratesModifier, modifierTorch);
            modifierTorchVision = CustomOption.Create(1037, Types.Modifier, "Vision Multiplier", 1.5f, 1f, 3f, 0.125f, modifierTorch);

            modifierVip = CustomOption.Create(1040, Types.Modifier, cs(Vip.color, "VIP"), rates, null, true);
            modifierVipQuantity = CustomOption.Create(1041, Types.Modifier, "Modifier Amount", ratesModifier, modifierVip);
            modifierVipShowColor = CustomOption.Create(1042, Types.Modifier, "Show Killer Team Color", true, modifierVip);

            modifierButtonBarry = CustomOption.Create(1006, Types.Modifier, cs(ButtonBarry.color, "Button Barry"), rates, null, true);

            modifierDrunk = CustomOption.Create(1045, Types.Modifier, cs(Drunk.color, "Drunk"), rates, null, true);
            modifierDrunkQuantity = CustomOption.Create(1046, Types.Modifier, "Modifier Amount", ratesModifier, modifierDrunk);
            modifierDrunkDuration = CustomOption.Create(1047, Types.Modifier, "Number Of Meetings Inverted", 3f, 1f, 15f, 1f, modifierDrunk);

            modifierFlash = CustomOption.Create(1015, Types.Modifier, cs(Flash.color, "Flash"), rates, null, true);
            modifierFlashQuantity = CustomOption.Create(1016, Types.Modifier, "Modifier Amount", ratesModifier, modifierFlash);
            modifierFlashSpeed = CustomOption.Create(1017, Types.Modifier, "Speed Multiplier", 1.25f, 1f, 3f, 0.125f, modifierFlash);

            modifierImmovable = CustomOption.Create(1048, Types.Modifier, cs(Immovable.color, "Immovable"), rates, null, true);
            modifierImmovableQuantity = CustomOption.Create(1049, Types.Modifier, "Modifier Amount", ratesModifier, modifierImmovable);

            modifierLover = CustomOption.Create(1025, Types.Modifier, cs(Color.yellow, "Lovers"), rates, null, true);
            modifierLoverImpLoverRate = CustomOption.Create(1026, Types.Modifier, "Chance That One Lover Is Killer", rates, modifierLover);
            modifierLoverBothDie = CustomOption.Create(1027, Types.Modifier, "Both Lovers Die", true, modifierLover);
            modifierLoverEnableChat = CustomOption.Create(1028, Types.Modifier, "Enable Lover Chat", true, modifierLover);

            modifierMini = CustomOption.Create(1020, Types.Modifier, cs(Mini.color, "Mini"), rates, null, true);
            modifierMiniGrowingUpDuration = CustomOption.Create(1021, Types.Modifier, "Mini Growing Up Duration", 400f, 100f, 1500f, 100f, modifierMini);
            modifierMiniGrowingUpInMeeting = CustomOption.Create(1022, Types.Modifier, "Mini Grows Up In Meeting", true, modifierMini);

            modifierRadar = CustomOption.Create(1023, Types.Modifier, cs(Radar.color, "Radar"), rates, null, true);

            modifierSatelite = CustomOption.Create(1065, Types.Modifier, cs(Satelite.color, "Satelite"), rates, null, true);
            modifierSateliteTrackingCooldown = CustomOption.Create(1066, Types.Modifier, "Corpses Tracking Cooldown", 30f, 10f, 60f, 2.5f, modifierSatelite);
            modifierSateliteTrackingDuration = CustomOption.Create(1067, Types.Modifier, "Corpses Tracking Duration", 10f, 1f, 20f, 0.5f, modifierSatelite);

            modifierSixthSense = CustomOption.Create(1068, Types.Modifier, cs(SixthSense.color, "Sixth Sense"), rates, null, true);

            modifierShy = CustomOption.Create(1010, Types.Modifier, cs(Shy.color, "Shy"), rates, null, true);
            modifierShyQuantity = CustomOption.Create(1011, Types.Modifier, "Modifier Amount", ratesModifier, modifierShy);
            modifierShyHoldDuration = CustomOption.Create(1012, Types.Modifier, "Time Until Fading Starts", 3f, 1f, 10f, 0.5f, modifierShy);
            modifierShyFadeDuration = CustomOption.Create(1013, Types.Modifier, "Fade Duration", 1f, 0.25f, 10f, 0.25f, modifierShy);
            modifierShyMinVisibility = CustomOption.Create(1014, Types.Modifier, "Minimum Visibility", new string[] { "0%", "10%", "20%", "30%", "40%", "50%" }, modifierShy);

            modifierSleuth = CustomOption.Create(1024, Types.Modifier, cs(Sleuth.color, "Sleuth"), rates, null, true);

            modifierTieBreaker = CustomOption.Create(1029, Types.Modifier, cs(Tiebreaker.color, "Tiebreaker"), rates, null, true);

            modifierDisperser = CustomOption.Create(1070, Types.Modifier, cs(Disperser.color, "Disperser"), rates, null, true);
            modifierDisperserNumOfDisperses = CustomOption.Create(1071, Types.Modifier, "Number Of Disperses", 1f, 1f, 3f, 1f, modifierDisperser);
            modifierDisperserDisperseToVents = CustomOption.Create(1072, Types.Modifier, "Disperse Players To Vents", false, modifierDisperser);
            modifierDisperserCanRecharge = CustomOption.Create(1073, Types.Modifier, "Can Recharge", false, modifierDisperser);
            modifierDisperserRechargeKillsNumber = CustomOption.Create(1074, Types.Modifier, "Number Of Kills Needed For Recharging", 2f, 1f, 10f, 1f, modifierDisperserCanRecharge);

            modifierDoubleShot = CustomOption.Create(1050, Types.Modifier, cs(DoubleShot.color, "Double Shot"), rates, null, true);

            modifierPoucher = CustomOption.Create(1075, Types.Modifier, cs(Poucher.color, "Poucher"), rates, null, true);

            modifierRuthless = CustomOption.Create(1051, Types.Modifier, cs(Ruthless.color, "Ruthless"), rates, null, true);

            modifierSaboteur = CustomOption.Create(1058, Types.Modifier, cs(Saboteur.color, "Saboteur"), rates, null, true);
            modifierSaboteurReduceSaboCooldown = CustomOption.Create(1059, Types.Modifier, "Reduced Sabotage Bonus", 10f, 1f, 20f, 1f, modifierSaboteur);

            modifierUnderdog = CustomOption.Create(1055, Types.Modifier, cs(Underdog.color, "Underdog"), rates, null, true);
            modifierUnderdogCooldownAddition = CustomOption.Create(1056, Types.Modifier, "Kill Cooldown Bonus", 15f, 2.5f, 20f, 2.5f, modifierUnderdog);
            modifierUnderdogIncreaseCooldown = CustomOption.Create(1057, Types.Modifier, "Increased Kill Cooldown When 2+ Imps", false, modifierUnderdog);
            #endregion

            blockedRolePairings.Add((byte)RoleId.Survivor, new[] { (byte)RoleId.Pursuer });
            blockedRolePairings.Add((byte)RoleId.Pursuer, new[] { (byte)RoleId.Survivor });

            blockedRolePairings.Add((byte)RoleId.Lawyer, new[] { (byte)RoleId.Executioner });
            blockedRolePairings.Add((byte)RoleId.Executioner, new[] { (byte)RoleId.Lawyer });
        }
    }
}