using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using AmongUs.GameOptions;
using static TownOfUs.TownOfUs;

namespace TownOfUs.Patches {
    [HarmonyPatch(typeof(RoleOptionsCollectionV08), nameof(RoleOptionsCollectionV08.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch{
        public static void Postfix(ref int __result) {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class GameOptionsDataGetAdjustedNumImpostorsPatch {
        public static void Postfix(ref int __result) {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) {  // Ignore Vanilla impostor limits in TOR Games.
                __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
            } 
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Validate))]
    class GameOptionsDataValidatePatch {
        public static void Postfix(GameOptionsData __instance) {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;
            __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch {
        private static int crewValues;
        private static int impValues;
        private static bool isEvilGuesser;
        private static List<Tuple<byte, byte>> playerRoleMap = new List<Tuple<byte, byte>>();
        public static bool isGuesserGamemode { get { return TOUMapOptions.gameMode == CustomGamemodes.Guesser; } }
        public static void Postfix() {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return; // Don't assign Roles in Hide N Seek
            assignRoles();
        }

        private static void assignRoles() {
            var data = getRoleAssignmentData();
            assignSpecialRoles(data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data); // Assign roles that should always be in the game next
            assignDependentRoles(data); // Assign roles that may have a dependent role
            assignChanceRoles(data); // Assign roles that may or may not be in the game last
            assignRoleTargets(data); // Assign targets for Lawyer & Prosecutor
            if (isGuesserGamemode) assignGuesserGamemode();
            assignModifiers(); // Assign modifier
            initEndGameSummary();
        }

        // Our end game summary initializer and buffer creator
        private static void initEndGameSummary()
        {
            MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.InitGameSummary, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(w);            
            RPCProcedure.initEndGameSummary();
        }

        public static RoleAssignmentData getRoleAssignmentData() {
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var crewmateMin = crewmates.Count;
            var crewmateMax = crewmates.Count;
            var neutralMin = CustomOptionHolder.nonKillingNeutralRolesCountMin.getSelection();
            var neutralMax = CustomOptionHolder.nonKillingNeutralRolesCountMax.getSelection();
            var kneutralMin = CustomOptionHolder.killingNeutralRolesCountMin.getSelection();
            var kneutralMax = CustomOptionHolder.killingNeutralRolesCountMax.getSelection();
            var impostorMin = impostors.Count;
            var impostorMax = impostors.Count;

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (kneutralMin > kneutralMax) kneutralMin = kneutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
            crewmateMax = crewmates.Count - (neutralMin + kneutralMin);
            crewmateMin = crewmates.Count - (neutralMax + kneutralMax);
           
            // Get the maximum allowed count of each role type based on the minimum and maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
            int kneutralCountSettings = rnd.Next(kneutralMin, kneutralMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);
            // If fill crewmates is enabled, make sure crew + neutral >= crewmates s.t. everyone has a role!
            while (crewCountSettings + neutralCountSettings + kneutralCountSettings < crewmates.Count)
                crewCountSettings++;

            // Potentially lower the actual maximum to the assignable players
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            int kmaxNeutralRoles = Mathf.Min(crewmates.Count, kneutralCountSettings);
            int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> kneutralSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            
            impSettings.Add((byte)RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Poisoner, CustomOptionHolder.poisonerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Escapist, CustomOptionHolder.escapistSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Miner, CustomOptionHolder.minerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Phantom, CustomOptionHolder.phantomSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Venerer, CustomOptionHolder.venererSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.getSelection());
            
            neutralSettings.Add((byte)RoleId.Jester, CustomOptionHolder.jesterSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Scavenger, CustomOptionHolder.scavengerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.getSelection());
            if (rnd.Next(1, 101) < 15) {
                neutralSettings.Add((byte)RoleId.GuardianAngel, CustomOptionHolder.guardianAngelSpawnRate.getSelection());
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianToFa, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianToFa();
            } else {
                neutralSettings.Add((byte)RoleId.GuardianAngel, CustomOptionHolder.guardianAngelSpawnRate.getSelection());
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianToSurv, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianToSurv();
            }
            neutralSettings.Add((byte)RoleId.Amnesiac, CustomOptionHolder.amnesiacSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Mercenary, CustomOptionHolder.mercenarySpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.getSelection());

            kneutralSettings.Add((byte)RoleId.Dracula, CustomOptionHolder.draculaSpawnRate.getSelection());
            kneutralSettings.Add((byte)RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.getSelection());
            kneutralSettings.Add((byte)RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.getSelection());
            kneutralSettings.Add((byte)RoleId.Glitch, CustomOptionHolder.glitchSpawnRate.getSelection());
            
            crewSettings.Add((byte)RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Shifter, CustomOptionHolder.shifterSpawnRate.getSelection());
            if (!Helpers.isFungle()) crewSettings.Add((byte)RoleId.Spy, CustomOptionHolder.spySpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Medic, CustomOptionHolder.medicSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Investigator, CustomOptionHolder.investigatorSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Seer, CustomOptionHolder.seerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Mystic, CustomOptionHolder.mysticSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.getSelection());

            return new RoleAssignmentData {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                kneutralSettings = kneutralSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNeutralRoles = maxNeutralRoles,
                kmaxNeutralRoles = kmaxNeutralRoles,
                maxImpostorRoles = maxImpostorRoles
            };
        }

        private static void assignSpecialRoles(RoleAssignmentData data) {
        }

        private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data) {
            if (!isGuesserGamemode) {
                // Assign Guesser (chance to be impostor based on setting)
                isEvilGuesser = rnd.Next(1, 101) <= CustomOptionHolder.guesserIsImpGuesserRate.getSelection() * 10;
                if ((CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0 &&
                    CustomOptionHolder.guesserSpawnRate.getSelection() == 10) ||
                    CustomOptionHolder.guesserSpawnBothRate.getSelection() == 0) {
                    if (isEvilGuesser) data.impSettings.Add((byte)RoleId.Assassin, CustomOptionHolder.guesserSpawnRate.getSelection());
                    else data.crewSettings.Add((byte)RoleId.Vigilante, CustomOptionHolder.guesserSpawnRate.getSelection());
                }
            }
            
            crewValues = data.crewSettings.Values.ToList().Sum();
            impValues = data.impSettings.Values.ToList().Sum();
        }

        private static void assignEnsuredRoles(RoleAssignmentData data) {
            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> kensuredNeutralRoles = data.kneutralSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) || 
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) || 
                    (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) || 
                    (data.kmaxNeutralRoles > 0 && kensuredNeutralRoles.Count > 0)
                ))) {
                    
                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
                if (data.crewmates.Count > 0 && data.kmaxNeutralRoles > 0 && kensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.NeutralKiller, kensuredNeutralRoles);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);
                
                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count())); 
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral || roleType == RoleType.NeutralKiller ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId)) {
                    foreach(var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId]) {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = 0;
                        if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = 0;
                        if (data.kneutralSettings.ContainsKey(blockedRoleId)) data.kneutralSettings[blockedRoleId] = 0;
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = 0;
                        // Remove blocked roles even if the chance was 100%
                        foreach(var ensuredRolesList in rolesToAssign.Values) {
                            ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                        }
                    }
                }

                // Adjust the role limit
                switch (roleType) {
                    case RoleType.Crewmate: data.maxCrewmateRoles--; crewValues -= 10; break;
                    case RoleType.Neutral: data.maxNeutralRoles--; break;
                    case RoleType.NeutralKiller: data.kmaxNeutralRoles--; break;
                    case RoleType.Impostor: data.maxImpostorRoles--; impValues -= 10;  break;
                }
            }
        }

        private static void assignDependentRoles(RoleAssignmentData data) {
            // Roles that prob have a dependent role
            bool guesserFlag = CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0 
                && CustomOptionHolder.guesserSpawnRate.getSelection() > 0;
                
            if (isGuesserGamemode) guesserFlag = false;
            if (!guesserFlag) return; // assignDependentRoles is not needed

            int crew = data.crewmates.Count < data.maxCrewmateRoles ? data.crewmates.Count : data.maxCrewmateRoles; // Max number of crew loops
            int imp = data.impostors.Count < data.maxImpostorRoles ? data.impostors.Count : data.maxImpostorRoles; // Max number of imp loops
            int crewSteps = crew / data.crewSettings.Keys.Count(); // Avarage crewvalues deducted after each loop 
            int impSteps = imp / data.impSettings.Keys.Count(); // Avarage impvalues deducted after each loop

            // set to false if needed, otherwise we can skip the loop
            bool isGuesser = !guesserFlag;

            // --- Simulate Crew & Imp ticket system ---
            while (crew > 0 && (!isEvilGuesser && !isGuesser)) {
                if (!isEvilGuesser && !isGuesser && rnd.Next(crewValues) < CustomOptionHolder.guesserSpawnRate.getSelection()) isGuesser = true;
                crew--;
                crewValues -= crewSteps;
            }
            while (imp > 0 && (isEvilGuesser && !isGuesser)) { 
                if (rnd.Next(impValues) < CustomOptionHolder.guesserSpawnRate.getSelection()) isGuesser = true;
                imp--;
                impValues -= impSteps;
            }

            // --- Assign Main Roles if they won the lottery ---
            if (!isGuesserGamemode) {
                if (!isEvilGuesser && isGuesser && Guesser.niceGuesser == null && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && guesserFlag) { // Set Nice Guesser cause he won the lottery
                    byte niceGuesser = setRoleToRandomPlayer((byte)RoleId.Vigilante, data.crewmates);
                    data.crewmates.ToList().RemoveAll(x => x.PlayerId == niceGuesser);
                    data.maxCrewmateRoles--;
                }
                else if (isEvilGuesser && isGuesser && Guesser.evilGuesser == null && data.impostors.Count > 0 && data.maxImpostorRoles > 0 && guesserFlag) { // Set Evil Guesser cause he won the lottery
                    byte evilGuesser = setRoleToRandomPlayer((byte)RoleId.Assassin, data.impostors);
                    data.impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                    data.maxImpostorRoles--;
                }
            }

            // --- Assign Dependent Roles if main role exists ---
            if (!isGuesserGamemode) {
                if (!isEvilGuesser && Guesser.niceGuesser != null) { // Other Guesser (evil)
                    if (CustomOptionHolder.guesserSpawnBothRate.getSelection() == 10 && data.impostors.Count > 0 && data.maxImpostorRoles > 0) { // Force other guesser (evil)
                        byte bothGuesser = setRoleToRandomPlayer((byte)RoleId.Assassin, data.impostors);
                        data.impostors.ToList().RemoveAll(x => x.PlayerId == bothGuesser);
                        data.maxImpostorRoles--;
                    }
                    else if (CustomOptionHolder.guesserSpawnBothRate.getSelection() < 10) // Dont force, add Guesser (evil) to the ticket system
                        data.impSettings.Add((byte)RoleId.Assassin, CustomOptionHolder.guesserSpawnBothRate.getSelection());
                }
                else if (isEvilGuesser && Guesser.evilGuesser != null) { // ELSE other Guesser (nice)
                    if (CustomOptionHolder.guesserSpawnBothRate.getSelection() == 10 && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0) { // Force other guesser (nice)
                        byte bothGuesser = setRoleToRandomPlayer((byte)RoleId.Vigilante, data.crewmates);
                        data.crewmates.ToList().RemoveAll(x => x.PlayerId == bothGuesser);
                        data.maxCrewmateRoles--;
                    }
                    else if (CustomOptionHolder.guesserSpawnBothRate.getSelection() < 10) // Dont force, add Guesser (nice) to the ticket system
                        data.crewSettings.Add((byte)RoleId.Vigilante, CustomOptionHolder.guesserSpawnBothRate.getSelection());
                }
            }
        }

        private static void assignChanceRoles(RoleAssignmentData data) {
            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> kneutralTickets = data.kneutralSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) || 
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) || 
                    (data.maxNeutralRoles > 0 && neutralTickets.Count > 0) || 
                    (data.kmaxNeutralRoles > 0 && kneutralTickets.Count > 0)
                ))) {
                
                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, neutralTickets);
                if (data.crewmates.Count > 0 && data.kmaxNeutralRoles > 0 && kneutralTickets.Count > 0) rolesToAssign.Add(RoleType.NeutralKiller, kneutralTickets);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(RoleType.Impostor, impostorTickets);
                
                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral || roleType == RoleType.NeutralKiller ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                setRoleToRandomPlayer(roleId, players);
                rolesToAssign[roleType].RemoveAll(x => x == roleId);

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId)) {
                    foreach(var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId]) {
                        // Remove tickets of blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == blockedRoleId);
                        neutralTickets.RemoveAll(x => x == blockedRoleId);
                        kneutralTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType) {
                    case RoleType.Crewmate: data.maxCrewmateRoles--; break;
                    case RoleType.Neutral: data.maxNeutralRoles--;break;
                    case RoleType.NeutralKiller: data.kmaxNeutralRoles--; break;
                    case RoleType.Impostor: data.maxImpostorRoles--;break;
                }
            }
        }

        private static void assignRoleTargets(RoleAssignmentData data) {
            // Set Executioner Target
            if (Executioner.executioner != null) {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && !p.Data.Role.IsImpostor && !p.isAnyNeutral() && p != Swapper.swapper && p != Sheriff.sheriff && p != Veteran.veteran && p != Mayor.mayor)
                        possibleTargets.Add(p);
                
                if (possibleTargets.Count == 0) {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.executionerPromotesToPursuer();
                } else {
                    var target = possibleTargets[TownOfUs.rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerSetTarget(target.PlayerId);
                }
            }

            // Set Lawyer Target
            if (Lawyer.lawyer != null) {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && (p.isKiller() || (Lawyer.targetCanBeJester && p == Jester.jester)))
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0) {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.lawyerPromotesToPursuer();
                } else {
                    var target = possibleTargets[TownOfUs.rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerSetTarget(target.PlayerId);
                }
            }

            // Set Guardian Angel Target
            if (GuardianAngel.guardianAngel != null) {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && !p.isEvil())
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0) {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.guardianAngelPromotes();
                } else {
                    var target = possibleTargets[TownOfUs.rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelSetTarget(target.PlayerId);
                }
            }
        }

        private static void assignModifiers() {
            var modifierMin = CustomOptionHolder.modifiersCountMin.getSelection();
            var modifierMax = CustomOptionHolder.modifiersCountMax.getSelection();
            if (modifierMin > modifierMax) modifierMin = modifierMax;
            int modifierCountSettings = rnd.Next(modifierMin, modifierMax + 1);
            List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList();
            int modifierCount = Mathf.Min(players.Count, modifierCountSettings);

            if (modifierCount == 0) return;

            List<RoleId> allModifiers = new List<RoleId>();
            List<RoleId> ensuredModifiers = new List<RoleId>();
            List<RoleId> chanceModifiers = new List<RoleId>();
            allModifiers.AddRange(new List<RoleId> {
                RoleId.Blind,
                RoleId.Bait,
                RoleId.Sleuth,
                RoleId.Tiebreaker,
                RoleId.ButtonBarry,
                RoleId.Indomitable,
                RoleId.Drunk,
                RoleId.Sunglasses,
                RoleId.Torch,
                RoleId.DoubleShot,
                RoleId.Disperser,
                RoleId.Armored,
            });

            if (rnd.Next(1, 101) <= CustomOptionHolder.modifierLover.getSelection() * 10) { // Assign lover
                bool isEvilLover = rnd.Next(1, 101) <= CustomOptionHolder.modifierLoverImpLoverRate.getSelection() * 10;
                byte firstLoverId;
                List<PlayerControl> impPlayer = new List<PlayerControl>(players);
                List<PlayerControl> crewPlayer = new List<PlayerControl>(players);
                impPlayer.RemoveAll(x => !x.isKiller());
                crewPlayer.RemoveAll(x => x.isEvil());

                if (isEvilLover) firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, impPlayer);
                else firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer);
                byte secondLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer, 1);

                players.RemoveAll(x => x.PlayerId == firstLoverId || x.PlayerId == secondLoverId);
                modifierCount--;
            }

            foreach (RoleId m in allModifiers) {
                if (getSelectionForRoleId(m) == 10) ensuredModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true) / 10));
                else chanceModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true)));
            }

            assignModifiersToPlayers(ensuredModifiers, players, modifierCount); // Assign ensured modifier

            modifierCount -= ensuredModifiers.Count;
            if (modifierCount <= 0) return;
            int chanceModifierCount = Mathf.Min(modifierCount, chanceModifiers.Count);
            List<RoleId> chanceModifierToAssign = new List<RoleId>();
            while (chanceModifierCount > 0 && chanceModifiers.Count > 0) {
                var index = rnd.Next(0, chanceModifiers.Count);
                RoleId modifierId = chanceModifiers[index];
                chanceModifierToAssign.Add(modifierId);

                int modifierSelection = getSelectionForRoleId(modifierId);
                while (modifierSelection > 0) {
                    chanceModifiers.Remove(modifierId);
                    modifierSelection--;
                }
                chanceModifierCount--;
            }

            assignModifiersToPlayers(chanceModifierToAssign, players, modifierCount); // Assign chance modifier
        }

        private static void assignGuesserGamemode() {
            List<PlayerControl> impPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> kneutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> neutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> crewPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
            kneutralPlayer.RemoveAll(x => !Helpers.isNeutralKiller(x));
            neutralPlayer.RemoveAll(x => !Helpers.isNeutral(x));
            crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || Helpers.isAnyNeutral(x));
            assignGuesserGamemodeToPlayers(crewPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewNumber.getFloat()));
            assignGuesserGamemodeToPlayers(neutralPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNNeutralNumber.getFloat()));
            assignGuesserGamemodeToPlayers(kneutralPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeKNeutralNumber.getFloat()));
            assignGuesserGamemodeToPlayers(impPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeImpNumber.getFloat()));
        }

        private static void assignGuesserGamemodeToPlayers(List<PlayerControl> playerList, int count) {
            for (int i = 0; i < count && playerList.Count > 0; i++) {
                var index = rnd.Next(0, playerList.Count);
                byte playerId = playerList[index].PlayerId;
                playerList.RemoveAt(index);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGuesserGm, Hazel.SendOption.Reliable, -1);
                writer.Write(playerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setGuesserGm(playerId);
            }
        }

        private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true) {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            if (removePlayer) playerList.RemoveAt(index);

            playerRoleMap.Add(new Tuple<byte, byte>(playerId, roleId));

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId);
            return playerId;
        }

        private static byte setModifierToRandomPlayer(byte modifierId, List<PlayerControl> playerList, byte flag = 0) {
            if (playerList.Count == 0) return Byte.MaxValue;
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            playerList.RemoveAt(index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetModifier, Hazel.SendOption.Reliable, -1);
            writer.Write(modifierId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setModifier(modifierId, playerId, flag);
            return playerId;
        }

        private static void assignModifiersToPlayers(List<RoleId> modifiers, List<PlayerControl> playerList, int modifierCount) {
            modifiers = modifiers.OrderBy(x => rnd.Next()).ToList(); // randomize list

            while (modifierCount < modifiers.Count) {
                var index = rnd.Next(0, modifiers.Count);
                modifiers.RemoveAt(index);
            }

            byte playerId;

            List<PlayerControl> crewPlayer = new List<PlayerControl>(playerList);
            crewPlayer.RemoveAll(x => x.isEvil());

            List<PlayerControl> impPlayer = new List<PlayerControl>(playerList);
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);

            if (modifiers.Contains(RoleId.Blind)) {
                var crewPlayerBlind = new List<PlayerControl>(crewPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Blind, crewPlayerBlind);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Blind);
            }

            if (modifiers.Contains(RoleId.Bait)) {
                var crewPlayerBait = new List<PlayerControl>(crewPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Bait, crewPlayerBait);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Bait);
            }

            if (modifiers.Contains(RoleId.Indomitable)) {
                var crewPlayerIndomitable = new List<PlayerControl>(crewPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Indomitable, crewPlayerIndomitable);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Indomitable);
            }

            if (modifiers.Contains(RoleId.Sunglasses)) {
                var crewPlayerSunglasses = new List<PlayerControl>(crewPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Sunglasses, crewPlayerSunglasses);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Sunglasses);
            }
            
            if (modifiers.Contains(RoleId.Torch)) {
                var crewPlayerTorch = new List<PlayerControl>(crewPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Torch, crewPlayerTorch);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Torch);
            }

            if (modifiers.Contains(RoleId.DoubleShot)) {
                var impPlayerDoubleShot = new List<PlayerControl>(impPlayer);
                impPlayerDoubleShot.RemoveAll(x => !HandleGuesser.isGuesser(x.PlayerId));
                playerId = setModifierToRandomPlayer((byte)RoleId.DoubleShot, impPlayerDoubleShot);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.DoubleShot);
            }

            if (modifiers.Contains(RoleId.Disperser)) {
                var impPlayerDisperser = new List<PlayerControl>(impPlayer);
                playerId = setModifierToRandomPlayer((byte)RoleId.Disperser, impPlayerDisperser);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Disperser);
            }

            foreach (RoleId modifier in modifiers) {
                if (playerList.Count == 0) break;
                playerId = setModifierToRandomPlayer((byte)modifier, playerList);
                playerList.RemoveAll(x => x.PlayerId == playerId);
            }
        }

        private static int getSelectionForRoleId(RoleId roleId, bool multiplyQuantity = false) {
            int selection = 0;
            switch (roleId) {
                case RoleId.Lover:
                    selection = CustomOptionHolder.modifierLover.getSelection(); break;
                case RoleId.Blind:
                    selection = CustomOptionHolder.modifierBlind.getSelection(); break;
                case RoleId.Bait:
                    selection = CustomOptionHolder.modifierBait.getSelection(); break;
                case RoleId.Sleuth:
                    selection = CustomOptionHolder.modifierSleuth.getSelection(); break;
                case RoleId.Tiebreaker:
                    selection = CustomOptionHolder.modifierTieBreaker.getSelection(); break;
                case RoleId.ButtonBarry:
                    selection = CustomOptionHolder.modifierButtonBarry.getSelection(); break;
                case RoleId.Indomitable:
                    selection = CustomOptionHolder.modifierIndomitable.getSelection(); break;
                case RoleId.Drunk:
                    selection = CustomOptionHolder.modifierDrunk.getSelection(); break;
                case RoleId.Sunglasses:
                    selection = CustomOptionHolder.modifierSunglasses.getSelection(); break;
                case RoleId.Torch:
                    selection = CustomOptionHolder.modifierTorch.getSelection(); break;
                case RoleId.DoubleShot:
                    selection = CustomOptionHolder.modifierDoubleShot.getSelection(); break;
                case RoleId.Disperser:
                    selection = CustomOptionHolder.modifierDisperser.getSelection(); break;
                case RoleId.Armored:
                    selection = CustomOptionHolder.modifierArmored.getSelection(); break;
            }
                 
            return selection;
        }

        public class RoleAssignmentData {
            public List<PlayerControl> crewmates {get;set;}
            public List<PlayerControl> impostors {get;set;}
            public Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> kneutralSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            public int maxCrewmateRoles {get;set;}
            public int maxNeutralRoles {get;set;}
            public int kmaxNeutralRoles {get;set;}
            public int maxImpostorRoles {get;set;}
        }
        
        private enum RoleType {
            Crewmate = 0,
            Neutral = 1,
            NeutralKiller = 2,
            Impostor = 3
        }

    }
}
