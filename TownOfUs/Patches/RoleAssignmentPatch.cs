using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(RoleOptionsCollectionV08), nameof(RoleOptionsCollectionV08.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class LegacyGameOptionsGetAdjustedNumImpostorsPatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
            {  
                // Ignore Vanilla impostor limits in TOU Games.
                __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
            }
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public class RoleManagerSelectRolesPatch
    {
        private static int crewValues;
        private static int impValues;
        private static List<Tuple<byte, byte>> playerRoleMap = new List<Tuple<byte, byte>>();
        public static bool isGuesserGamemode { get { return gameMode == CustomGamemodes.Guesser; } }
        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVariables, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek || RoleDraftEx.isEnabled) return; // Don't assign Roles in Hide N Seek
            assignRoles();
        }

        private static void assignRoles()
        {
            var data = getRoleAssignmentData();
            assignSpecialRoles(data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data); // Assign roles that should always be in the game next
            assignDependentRoles(data); // Assign roles that may have a dependent role
            assignChanceRoles(data); // Assign roles that may or may not be in the game last
            assignRoleTargets(data); // Assign targets for Lawyer, Executioner & Guardian Angel
            if (isGuesserGamemode) assignGuesserGamemode();
            assignModifiers(); // Assign modifier
        }

        public static RoleAssignmentData getRoleAssignmentData()
        {
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var crewmateMin = crewmates.Count;
            var crewmateMax = crewmates.Count;
            var nonKillingNeutralMin = CustomOptionHolder.nonKillingNeutralRolesCountMin.getSelection();
            var nonKillingNeutralMax = CustomOptionHolder.nonKillingNeutralRolesCountMax.getSelection();
            var killingNeutralMin = CustomOptionHolder.killingNeutralRolesCountMin.getSelection();
            var killingNeutralMax = CustomOptionHolder.killingNeutralRolesCountMax.getSelection();
            var impostorMin = impostors.Count;
            var impostorMax = impostors.Count;

            if (RoleDraftEx.isEnabled)
            {
                nonKillingNeutralMin = nonKillingNeutralMax;
                killingNeutralMin = killingNeutralMax;
            }

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (nonKillingNeutralMin > nonKillingNeutralMax) nonKillingNeutralMin = nonKillingNeutralMax;
            if (killingNeutralMin > killingNeutralMax) killingNeutralMin = killingNeutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
            crewmateMax = crewmates.Count - (nonKillingNeutralMin + killingNeutralMin);
            crewmateMin = crewmates.Count - (nonKillingNeutralMax + killingNeutralMax);

            // Get the maximum allowed count of each role type based on the minimum and maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int nonKillingNeutralCountSettings = rnd.Next(nonKillingNeutralMin, nonKillingNeutralMax + 1);
            int killingNeutralCountSettings = rnd.Next(killingNeutralMin, killingNeutralMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);

            // If fill crewmates is enabled, make sure crew + neutral >= crewmates s.t. everyone has a role!
            while (crewCountSettings + nonKillingNeutralCountSettings + killingNeutralCountSettings < crewmates.Count)
                crewCountSettings++;

            // Potentially lower the actual maximum to the assignable players
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int maxNonKillingNeutralRoles = Mathf.Min(crewmates.Count, nonKillingNeutralCountSettings);
            int maxKillingNeutralRoles = Mathf.Min(crewmates.Count, killingNeutralCountSettings);
            int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (int rate, int count)> crewSettings = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int rate, int count)> nonKillingNeutralSettings = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int rate, int count)> killingNeutralSettings = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int rate, int count)> impSettings = new Dictionary<byte, (int, int)>();

            crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.data);
            crewSettings.Add((byte)RoleId.Seer, CustomOptionHolder.seerSpawnRate.data);
            crewSettings.Add((byte)RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.data);
            crewSettings.Add((byte)RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.data);
            crewSettings.Add((byte)RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.data);
            crewSettings.Add((byte)RoleId.Politician, CustomOptionHolder.politicianSpawnRate.data);
            crewSettings.Add((byte)RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.data);
            crewSettings.Add((byte)RoleId.Medic, CustomOptionHolder.medicSpawnRate.data);
            if (!Helpers.isFungle()) crewSettings.Add((byte)RoleId.Spy, CustomOptionHolder.spySpawnRate.data);
            crewSettings.Add((byte)RoleId.Investigator, CustomOptionHolder.investigatorSpawnRate.data);
            crewSettings.Add((byte)RoleId.Oracle, CustomOptionHolder.oracleSpawnRate.data);
            crewSettings.Add((byte)RoleId.Mystic, CustomOptionHolder.mysticSpawnRate.data);
            crewSettings.Add((byte)RoleId.Transporter, CustomOptionHolder.transporterSpawnRate.data);
            if (impostors.Count > 1) crewSettings.Add((byte)RoleId.Agent, CustomOptionHolder.agentSpawnRate.data);
            crewSettings.Add((byte)RoleId.Lighter, CustomOptionHolder.lighterSpawnRate.data);
            crewSettings.Add((byte)RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.data);
            
            nonKillingNeutralSettings.Add((byte)RoleId.Jester, CustomOptionHolder.jesterSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.GuardianAngel, CustomOptionHolder.guardianAngelSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Amnesiac, CustomOptionHolder.amnesiacSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Scavenger, CustomOptionHolder.scavengerSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Mercenary, CustomOptionHolder.mercenarySpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Thief, CustomOptionHolder.thiefSpawnRate.data);
            nonKillingNeutralSettings.Add((byte)RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.data);

            killingNeutralSettings.Add((byte)RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.data);
            killingNeutralSettings.Add((byte)RoleId.Plaguebearer, CustomOptionHolder.plaguebearerSpawnRate.data);
            killingNeutralSettings.Add((byte)RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.data);
            killingNeutralSettings.Add((byte)RoleId.Glitch, CustomOptionHolder.glitchSpawnRate.data);
            killingNeutralSettings.Add((byte)RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.data);
            
            impSettings.Add((byte)RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.data);
            impSettings.Add((byte)RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.data);
            impSettings.Add((byte)RoleId.Poisoner, CustomOptionHolder.poisonerSpawnRate.data);
            impSettings.Add((byte)RoleId.Janitor, CustomOptionHolder.janitorSpawnRate.data);
            impSettings.Add((byte)RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.data);
            impSettings.Add((byte)RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.data);
            impSettings.Add((byte)RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.data);
            impSettings.Add((byte)RoleId.Bomber, CustomOptionHolder.bomberSpawnRate.data);
            impSettings.Add((byte)RoleId.Venerer, CustomOptionHolder.venererSpawnRate.data);
            impSettings.Add((byte)RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.data);

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                nonKillingNeutralSettings = nonKillingNeutralSettings,
                killingNeutralSettings = killingNeutralSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNonKillingNeutralRoles = maxNonKillingNeutralRoles,
                maxKillingNeutralRoles = maxKillingNeutralRoles,
                maxImpostorRoles = maxImpostorRoles
            };
        }

        public static void assignSpecialRoles(RoleAssignmentData data)
        {
        }

        public static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {
            // Assign Vampires
            if ((CustomOptionHolder.vampireHunterSpawnRate.getSelection() > 0 &&
                CustomOptionHolder.draculaSpawnRate.getSelection() == 10) ||
                CustomOptionHolder.vampireHunterSpawnRate.getSelection() == 0)
                    data.killingNeutralSettings.Add((byte)RoleId.Dracula, CustomOptionHolder.draculaSpawnRate.data);
            
            crewValues = data.crewSettings.Values.Select(x => x.rate * x.count).ToList().Sum();
            impValues = data.impSettings.Values.Select(x => x.rate * x.count).ToList().Sum();
        }

        public static void assignEnsuredRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredNonKillingNeutralRoles = data.nonKillingNeutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredKillingNeutralRoles = data.killingNeutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                    (data.maxNonKillingNeutralRoles > 0 && ensuredNonKillingNeutralRoles.Count > 0) ||
                    (data.maxKillingNeutralRoles > 0 && ensuredKillingNeutralRoles.Count > 0)
                )))
            {

                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNonKillingNeutralRoles > 0 && ensuredNonKillingNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNonKillingNeutralRoles);
                if (data.crewmates.Count > 0 && data.maxKillingNeutralRoles > 0 && ensuredKillingNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.NeutralKiller, ensuredKillingNeutralRoles);
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

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                    {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                        if (data.nonKillingNeutralSettings.ContainsKey(blockedRoleId)) data.nonKillingNeutralSettings[blockedRoleId] = (0, 0);
                        if (data.killingNeutralSettings.ContainsKey(blockedRoleId)) data.killingNeutralSettings[blockedRoleId] = (0, 0);
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0, 0);
                        // Remove blocked roles even if the chance was 100%
                        foreach (var ensuredRolesList in rolesToAssign.Values)
                        {
                            ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                        }
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case RoleType.Crewmate: data.maxCrewmateRoles--; crewValues -= 10; break;
                    case RoleType.Neutral: data.maxNonKillingNeutralRoles--; break;
                    case RoleType.NeutralKiller: data.maxKillingNeutralRoles--; break;
                    case RoleType.Impostor: data.maxImpostorRoles--; impValues -= 10; break;
                }
            }
        }

        public static void assignDependentRoles(RoleAssignmentData data)
        {
            bool draculaFlag = CustomOptionHolder.vampireHunterSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaSpawnRate.getSelection() > 0;
            if (!draculaFlag) return;

            int crew = data.crewmates.Count < data.maxCrewmateRoles ? data.crewmates.Count : data.maxCrewmateRoles; // Max number of crew loops
            int crewSteps = crew / data.crewSettings.Keys.Count(); // Avarage crewvalues deducted after each loop 

            bool isDracula = !draculaFlag;

            int draculaCount = CustomOptionHolder.draculaSpawnRate.count;
            // --- Simulate Crew ticket system ---
            while (crew > 0 && !isDracula)
            {
                if (!isDracula && rnd.Next(crewValues) < CustomOptionHolder.draculaSpawnRate.getSelection() && data.crewmates.Count > 0 && data.maxKillingNeutralRoles > 0 && Dracula.players.Count < CustomOptionHolder.draculaSpawnRate.count)
                {
                    byte dracula = setRoleToRandomPlayer((byte)RoleId.Dracula, data.crewmates);
                    data.crewmates.ToList().RemoveAll(x => x.PlayerId == dracula);
                    data.maxKillingNeutralRoles--;
                    draculaCount--;
                    isDracula = draculaCount == 0;
                }
                crew--;
                crewValues -= crewSteps;
            }

            // --- Assign Dependent Roles if main role exists ---
            if (Dracula.exists)
            {
                if (CustomOptionHolder.vampireHunterSpawnRate.getSelection() == 10)
                {
                    int vampireHunterCount = CustomOptionHolder.vampireHunterSpawnRate.count;
                    while (vampireHunterCount > 0 && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && VampireHunter.players.Count < CustomOptionHolder.vampireHunterSpawnRate.count)
                    {
                        byte vampireHubter = setRoleToRandomPlayer((byte)RoleId.VampireHunter, data.crewmates);
                        data.crewmates.ToList().RemoveAll(x => x.PlayerId == vampireHubter);
                        data.maxCrewmateRoles--;
                        vampireHunterCount--;
                    }
                }
                else if (CustomOptionHolder.vampireHunterSpawnRate.getSelection() < 10)
                {
                    data.crewSettings.Add((byte)RoleId.VampireHunter, CustomOptionHolder.vampireHunterSpawnRate.data);
                }
            }

            if (!data.killingNeutralSettings.ContainsKey((byte)RoleId.Dracula)) data.killingNeutralSettings.Add((byte)RoleId.Dracula, (0, 0));
        }

        public static void assignChanceRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> nonKillingNeutralTickets = data.nonKillingNeutralSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> killingNeutralTickets = data.killingNeutralSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                    (data.maxNonKillingNeutralRoles > 0 && nonKillingNeutralTickets.Count > 0) ||
                    (data.maxKillingNeutralRoles > 0 && killingNeutralTickets.Count > 0)
                )))
            {

                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNonKillingNeutralRoles > 0 && nonKillingNeutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, nonKillingNeutralTickets);
                if (data.crewmates.Count > 0 && data.maxKillingNeutralRoles > 0 && killingNeutralTickets.Count > 0) rolesToAssign.Add(RoleType.NeutralKiller, killingNeutralTickets);
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

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                    {
                        // Remove tickets of blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == blockedRoleId);
                        nonKillingNeutralTickets.RemoveAll(x => x == blockedRoleId);
                        killingNeutralTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case RoleType.Crewmate: data.maxCrewmateRoles--; break;
                    case RoleType.Neutral: data.maxNonKillingNeutralRoles--; break;
                    case RoleType.NeutralKiller: data.maxKillingNeutralRoles--; break;
                    case RoleType.Impostor: data.maxImpostorRoles--; break;
                }
            }
        }

        public static void assignRoleTargets(RoleAssignmentData data)
        {
            if (Lawyer.exists)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && p.isKiller())
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0)
                {
                    foreach (var lawyer in Lawyer.allPlayers)
                    {
                        MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                        w.Write(lawyer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(w);
                        RPCProcedure.lawyerPromotesToPursuer(lawyer.PlayerId);
                    }
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerSetTarget(target.PlayerId);
                }
            }

            if (GuardianAngel.exists)
            {
                var possibleTargets = new List<PlayerControl>();
                if (rnd.Next(1, 101) <= (GuardianAngel.oddsEvilTarget * 10))
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers())
                            possibleTargets.Add(p);
                }
                else
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && !p.isEvil())
                            possibleTargets.Add(p);
                }

                if (possibleTargets.Count == 0)
                {
                    foreach (var ga in GuardianAngel.allPlayers)
                    {
                        MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                        w.Write(ga.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(w);
                        RPCProcedure.guardianAngelPromotes(ga.PlayerId);
                    }
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelSetTarget(target.PlayerId);
                }
            }

            if (Executioner.exists)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && !p.isEvil() && !p.isRole(RoleId.Politician) && !p.isRole(RoleId.Sheriff))
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0)
                {
                    foreach (var executioner in Executioner.allPlayers)
                    {
                        MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToAmnesiac, Hazel.SendOption.Reliable, -1);
                        w.Write(executioner.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(w);
                        RPCProcedure.executionerPromotesToAmnesiac(executioner.PlayerId);
                    }
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerSetTarget(target.PlayerId);
                }
            }
        }

        public static void assignModifiers()
        {
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
            allModifiers.AddRange(new List<RoleId>
            {
                RoleId.Bait,
                RoleId.Shifter,
                RoleId.ButtonBarry,
                RoleId.Blind,
                RoleId.Sleuth,
                RoleId.Indomitable,
                RoleId.Drunk,
                RoleId.Torch,
                RoleId.Vip,
                RoleId.Radar,
                RoleId.Disperser,
                RoleId.DoubleShot,
                RoleId.Immovable,
                RoleId.Tiebreaker,
                RoleId.Chameleon,
            });

            // Assign Lovers
            if (CustomOptionHolder.modifierLover.getSelection() > 0)
            {
                for (int i = 0; i < CustomOptionHolder.modifierLover.count; i++)
                {
                    List<PlayerControl> impPlayer = new(players);
                    List<PlayerControl> crewPlayer = new(players);
                    impPlayer.RemoveAll(x => !x.isKiller());
                    crewPlayer.RemoveAll(x => x.isKiller() || x.isRole(RoleId.Lawyer) || x.isRole(RoleId.GuardianAngel) || x.isRole(RoleId.Executioner));
                    var singleCrew = crewPlayer.FindAll(x => !x.isLovers());
                    var singleImps = impPlayer.FindAll(x => !x.isLovers());

                    if (rnd.Next(1, 101) <= CustomOptionHolder.modifierLover.getSelection() * 10)
                    {
                        int lover1 = -1;
                        int lover2 = -1;
                        int lover1Index = -1;
                        int lover2Index = -1;
                        if (singleImps.Count > 0 && singleCrew.Count > 0 && rnd.Next(1, 101) <= CustomOptionHolder.modifierLoverImpLoverRate.getSelection() * 10)
                        {
                            lover1Index = rnd.Next(0, singleImps.Count);
                            lover1 = singleImps[lover1Index].PlayerId;

                            lover2Index = rnd.Next(0, singleCrew.Count);
                            lover2 = singleCrew[lover2Index].PlayerId;
                        }

                        else if (singleCrew.Count >= 2)
                        {
                            lover1Index = rnd.Next(0, singleCrew.Count);
                            while (lover2Index == lover1Index || lover2Index < 0) lover2Index = rnd.Next(0, singleCrew.Count);

                            lover1 = singleCrew[lover1Index].PlayerId;
                            lover2 = singleCrew[lover2Index].PlayerId;
                        }

                        if (lover1 >= 0 && lover2 >= 0)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLovers, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)lover1);
                            writer.Write((byte)lover2);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.setLovers((byte)lover1, (byte)lover2);
                            modifierCount--;
                        }
                    }
                }
            }

            foreach (RoleId m in allModifiers)
            {
                if (getSelectionForRoleId(m) == 10) ensuredModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true) / 10));
                else chanceModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true)));
            }

            assignModifiersToPlayers(ensuredModifiers, players, modifierCount); // Assign ensured modifier

            modifierCount -= ensuredModifiers.Count;
            if (modifierCount <= 0) return;
            int chanceModifierCount = Mathf.Min(modifierCount, chanceModifiers.Count);
            List<RoleId> chanceModifierToAssign = new List<RoleId>();
            while (chanceModifierCount > 0 && chanceModifiers.Count > 0)
            {
                var index = rnd.Next(0, chanceModifiers.Count);
                RoleId modifierId = chanceModifiers[index];
                chanceModifierToAssign.Add(modifierId);

                int modifierSelection = getSelectionForRoleId(modifierId);
                while (modifierSelection > 0)
                {
                    chanceModifiers.Remove(modifierId);
                    modifierSelection--;
                }
                chanceModifierCount--;
            }

            assignModifiersToPlayers(chanceModifierToAssign, players, modifierCount); // Assign chance modifier
        }

        public static void assignGuesserGamemode() {
            List<PlayerControl> impPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> benignNeutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> evilNeutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> killingNeutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> crewPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
            benignNeutralPlayer.RemoveAll(x => !Helpers.isBenignNeutral(x));
            evilNeutralPlayer.RemoveAll(x => !Helpers.isEvilNeutral(x));
            killingNeutralPlayer.RemoveAll(x => !Helpers.isKillingNeutral(x));
            crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || Helpers.isAnyNeutral(x));
            assignGuesserGamemodeToPlayers(crewPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewNumber.getFloat()));
            assignGuesserGamemodeToPlayers(benignNeutralPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeBenignNeutralNumber.getFloat()));
            assignGuesserGamemodeToPlayers(evilNeutralPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeEvilNeutralNumber.getFloat()));
            assignGuesserGamemodeToPlayers(killingNeutralPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeKillingNeutralNumber.getFloat()));
            assignGuesserGamemodeToPlayers(impPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeImpNumber.getFloat()));
        }

        private static void assignGuesserGamemodeToPlayers(List<PlayerControl> playerList, int count, bool forceJackal = false, bool forceThief = false)
        {
            for (int i = 0; i < count && playerList.Count > 0; i++)
            {
                var index = rnd.Next(0, playerList.Count);
                byte playerId = playerList[index].PlayerId;
                playerList.RemoveAt(index);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGuesserGm, Hazel.SendOption.Reliable, -1);
                writer.Write(playerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setGuesserGm(playerId);
            }
        }

        public static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
        {
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

        public static byte setModifierToRandomPlayer(byte modifierId, List<PlayerControl> playerList, byte flag = 0)
        {
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

        public static void assignModifiersToPlayers(List<RoleId> modifiers, List<PlayerControl> playerList, int modifierCount)
        {
            modifiers = modifiers.OrderBy(x => rnd.Next()).ToList(); // randomize list

            while (modifierCount < modifiers.Count)
            {
                var index = rnd.Next(0, modifiers.Count);
                modifiers.RemoveAt(index);
            }

            byte playerId;
            List<PlayerControl> crewPlayer = new(playerList);
            crewPlayer.RemoveAll(x => x.isEvil());

            List<PlayerControl> impPlayer = new(playerList);
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);

            // Crewmate Modifiers
            if (modifiers.Contains(RoleId.Bait))
            {
                int baitCount = 0;
                while (baitCount < modifiers.FindAll(x => x == RoleId.Bait).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Bait, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    baitCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Bait);
            }

            if (modifiers.Contains(RoleId.Shifter))
            {
                int shifterCount = 0;
                while (shifterCount < modifiers.FindAll(x => x == RoleId.Shifter).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Shifter, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    shifterCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Shifter);
            }
            
            if (modifiers.Contains(RoleId.Blind))
            {
                int blindCount = 0;
                while (blindCount < modifiers.FindAll(x => x == RoleId.Blind).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Blind, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    blindCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Blind);
            }

            if (modifiers.Contains(RoleId.Indomitable))
            {
                int indomitableCount = 0;
                while (indomitableCount < modifiers.FindAll(x => x == RoleId.Indomitable).Count)
                {
                    var playerIndomitable = new List<PlayerControl>(crewPlayer);
                    playerIndomitable.RemoveAll(x => Politician.players.Any(y => y.player.PlayerId == x.PlayerId));
                    playerId = setModifierToRandomPlayer((byte)RoleId.Indomitable, playerIndomitable);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    indomitableCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Indomitable);
            }

            if (modifiers.Contains(RoleId.Torch))
            {
                int torchCount = 0;
                while (torchCount < modifiers.FindAll(x => x == RoleId.Torch).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Torch, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    torchCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Torch);
            }

            if (modifiers.Contains(RoleId.Vip))
            {
                int vipCount = 0;
                while (vipCount < modifiers.FindAll(x => x == RoleId.Vip).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Vip, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    vipCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Vip);
            }

            // Global modifiers
            if (modifiers.Contains(RoleId.ButtonBarry))
            {
                int buttonBarryCount = 0;
                while (buttonBarryCount < modifiers.FindAll(x => x == RoleId.ButtonBarry).Count)
                {
                    var playerButtonBarry = new List<PlayerControl>(playerList);
                    playerButtonBarry.RemoveAll(x => Glitch.players.Any(y => y.player.PlayerId == x.PlayerId));
                    playerId = setModifierToRandomPlayer((byte)RoleId.ButtonBarry, playerButtonBarry);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    buttonBarryCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.ButtonBarry);
            }

            // Impostor modifiers
            if (modifiers.Contains(RoleId.Disperser))
            {
                int disperserCount = 0;
                while (disperserCount < modifiers.FindAll(x => x == RoleId.Disperser).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Disperser, impPlayer);
                    impPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    disperserCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Disperser);
            }
            
            if (modifiers.Contains(RoleId.DoubleShot))
            {
                int doubleShotCount = 0;
                while (doubleShotCount < modifiers.FindAll(x => x == RoleId.DoubleShot).Count)
                {
                    List<PlayerControl> impPlayerGuesser = new(impPlayer);
                    impPlayerGuesser.RemoveAll(x => !HandleGuesser.isGuesser(x.PlayerId));
                    playerId = setModifierToRandomPlayer((byte)RoleId.DoubleShot, impPlayerGuesser);
                    impPlayer.RemoveAll(x => x.PlayerId == playerId);
                    impPlayerGuesser.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    doubleShotCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.DoubleShot);
            }

            foreach (RoleId modifier in modifiers)
            {
                if (playerList.Count == 0) break;
                playerId = setModifierToRandomPlayer((byte)modifier, playerList);
                playerList.RemoveAll(x => x.PlayerId == playerId);
            }
        }

        private static int getSelectionForRoleId(RoleId roleId, bool multiplyQuantity = false)
        {
            int selection = 0;
            switch (roleId)
            {
                case RoleId.Bait:
                    selection = CustomOptionHolder.modifierBait.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierBait.count;
                    break;
                case RoleId.Shifter:
                    selection = CustomOptionHolder.modifierShifter.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierShifter.count;
                    break;
                case RoleId.ButtonBarry:
                    selection = CustomOptionHolder.modifierButtonBarry.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierButtonBarry.count;
                    break;
                case RoleId.Blind:
                    selection = CustomOptionHolder.modifierBlind.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierBlind.count;
                    break;
                case RoleId.Sleuth:
                    selection = CustomOptionHolder.modifierSleuth.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierSleuth.count;
                    break;
                case RoleId.Indomitable:
                    selection = CustomOptionHolder.modifierIndomitable.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierIndomitable.count;
                    break;
                case RoleId.Drunk:
                    selection = CustomOptionHolder.modifierDrunk.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierDrunk.count;
                    break;
                case RoleId.Torch:
                    selection = CustomOptionHolder.modifierTorch.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierTorch.count;
                    break;
                case RoleId.Vip:
                    selection = CustomOptionHolder.modifierVip.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierVip.count;
                    break;
                case RoleId.Radar:
                    selection = CustomOptionHolder.modifierRadar.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierRadar.count;
                    break;
                case RoleId.Disperser:
                    selection = CustomOptionHolder.modifierDisperser.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierDisperser.count;
                    break;
                case RoleId.Lover:
                    selection = CustomOptionHolder.modifierLover.getSelection(); break;
                case RoleId.DoubleShot:
                    selection = CustomOptionHolder.modifierDoubleShot.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierDoubleShot.count;
                    break;
                case RoleId.Immovable:
                    selection = CustomOptionHolder.modifierImmovable.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierImmovable.count;
                    break;
                case RoleId.Tiebreaker:
                    selection = CustomOptionHolder.modifierTieBreaker.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierTieBreaker.count;
                    break;
                case RoleId.Chameleon:
                    selection = CustomOptionHolder.modifierChameleon.rate;
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierChameleon.count;
                    break;
            }

            return selection;
        }

        public class RoleAssignmentData
        {
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public Dictionary<byte, (int rate, int count)> impSettings = new Dictionary<byte, (int, int)>();
            public Dictionary<byte, (int rate, int count)> nonKillingNeutralSettings = new Dictionary<byte, (int, int)>();
            public Dictionary<byte, (int rate, int count)> killingNeutralSettings = new Dictionary<byte, (int, int)>();
            public Dictionary<byte, (int rate, int count)> crewSettings = new Dictionary<byte, (int, int)>();
            public int maxCrewmateRoles { get; set; }
            public int maxNonKillingNeutralRoles { get; set; }
            public int maxKillingNeutralRoles { get; set; }
            public int maxImpostorRoles { get; set; }
        }

        private enum RoleType
        {
            Crewmate = 0,
            Neutral = 1,
            NeutralKiller = 2,
            Impostor = 3
        }
    }
}