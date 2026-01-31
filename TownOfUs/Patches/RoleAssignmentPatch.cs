using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using AmongUs.GameOptions;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(RoleOptionsCollectionV10), nameof(RoleOptionsCollectionV10.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class GameOptionsDataGetAdjustedNumImpostorsPatch
    {
        public static void Postfix(ref int __result)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
            {
                // Ignore Vanilla impostor limits in TOR Games.
                __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
            }
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        private static int crewValues;
        private static int neutValues;
        private static int neutKillValues;
        private static int impValues;
        private static List<Tuple<byte, byte>> playerRoleMap = new List<Tuple<byte, byte>>();
        public static bool isGuesserGamemode { get { return gameMode == CustomGamemodes.Guesser; } }
        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVariables, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return; // Don't assign Roles in Hide N Seek
            assignRoles();
        }

        private static void assignRoles()
        {
            var data = getRoleAssignmentData();
            assignSpecialRoles(data); // Assign special roles first as they assign a role to multiple players and the chances are independent of the ticket system
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data); // Assign roles that should always be in the game next
            assignDependentRoles(data); // Assign roles that may have a dependent role
            assignChanceRoles(data); // Assign roles that may or may not be in the game last
            assignRoleTargets(data); // Assign targets for Lawyer, Executioner & Guardian angel
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

            var crewmateMin = CustomOptionHolder.crewmateRolesCountMin.getSelection();
            var crewmateMax = CustomOptionHolder.crewmateRolesCountMax.getSelection();
            var neutralMin = CustomOptionHolder.nonKillingNeutralRolesCountMin.getSelection();
            var neutralMax = CustomOptionHolder.nonKillingNeutralRolesCountMax.getSelection();
            var neutralKillingMin = CustomOptionHolder.killingNeutralRolesCountMin.getSelection();
            var neutralKillingMax = CustomOptionHolder.killingNeutralRolesCountMax.getSelection();
            var impostorMin = CustomOptionHolder.impostorRolesCountMin.getSelection();
            var impostorMax = CustomOptionHolder.impostorRolesCountMax.getSelection();

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (neutralKillingMin > neutralKillingMax) neutralKillingMin = neutralKillingMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
            if (CustomOptionHolder.crewmateRolesFill.getBool())
            {
                crewmateMax = crewmates.Count - neutralMin - neutralKillingMin;
                crewmateMin = crewmates.Count - neutralMax - neutralKillingMax;
            }

            // Get the maximum allowed count of each role type based on the minimum and maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
            int neutralKillingCountSettings = rnd.Next(neutralKillingMin, neutralKillingMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);
            // If fill crewmates is enabled, make sure crew + neutral >= crewmates s.t. everyone has a role!
            while (crewCountSettings + neutralCountSettings + neutralKillingCountSettings < crewmates.Count && CustomOptionHolder.crewmateRolesFill.getBool())
                crewCountSettings++;

            // Potentially lower the actual maximum to the assignable players
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            int maxNeutralKillingRoles = Mathf.Min(crewmates.Count, neutralKillingCountSettings);
            int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> neutralKillingSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> impSettings = new Dictionary<byte, int>();

            crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Seer, CustomOptionHolder.seerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Medic, CustomOptionHolder.medicSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Investigator, CustomOptionHolder.investigatorSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Spy, CustomOptionHolder.spySpawnRate.getSelection());
            if (!isGuesserGamemode) crewSettings.Add((byte)RoleId.Vigilante, CustomOptionHolder.vigilanteSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Mystic, CustomOptionHolder.mysticSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Politician, CustomOptionHolder.politicianSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Oracle, CustomOptionHolder.oracleSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Lookout, CustomOptionHolder.lookoutSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Plumber, CustomOptionHolder.plumberSpawnRate.getSelection());

            neutralSettings.Add((byte)RoleId.Jester, CustomOptionHolder.jesterSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Survivor, CustomOptionHolder.survivorSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Pursuer, CustomOptionHolder.pursuerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Amnesiac, CustomOptionHolder.amnesiacSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Thief, CustomOptionHolder.thiefSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.GuardianAngel, CustomOptionHolder.guardianAngelSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.getSelection());
            neutralSettings.Add((byte)RoleId.Cannibal, CustomOptionHolder.cannibalSpawnRate.getSelection());

            neutralKillingSettings.Add((byte)RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.getSelection());
            neutralKillingSettings.Add((byte)RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.getSelection());
            neutralKillingSettings.Add((byte)RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.getSelection());
            neutralKillingSettings.Add((byte)RoleId.Plaguebearer, CustomOptionHolder.plaguebearerSpawnRate.getSelection());
            neutralKillingSettings.Add((byte)RoleId.Glitch, CustomOptionHolder.glitchSpawnRate.getSelection());

            impSettings.Add((byte)RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.getSelection());
            if (!isGuesserGamemode) impSettings.Add((byte)RoleId.Assassin, CustomOptionHolder.assassinSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Miner, CustomOptionHolder.minerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Janitor, CustomOptionHolder.janitorSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Undertaker, CustomOptionHolder.undertakerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Poisoner, CustomOptionHolder.poisonerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Venerer, CustomOptionHolder.venererSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Scavenger, CustomOptionHolder.scavengerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Escapist, CustomOptionHolder.escapistSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Deceiver, CustomOptionHolder.deceiverSpawnRate.getSelection());

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                neutralKillingSettings = neutralKillingSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNeutralRoles = maxNeutralRoles,
                maxNeutralKillingRoles = maxNeutralKillingRoles,
                maxImpostorRoles = maxImpostorRoles
            };
        }

        private static void assignSpecialRoles(RoleAssignmentData data)
        {
        }

        private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {
            // Assign Dracula
            if ((CustomOptionHolder.vampireHunterSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaSpawnRate.getSelection() == 10) || CustomOptionHolder.vampireHunterSpawnRate.getSelection() == 0)
                data.neutralKillingSettings.Add((byte)RoleId.Dracula, CustomOptionHolder.draculaSpawnRate.getSelection());
            
            crewValues = data.crewSettings.Values.ToList().Sum();
            neutKillValues = data.neutralKillingSettings.Values.ToList().Sum();
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> ensuredNeutralKillingRoles = data.neutralKillingSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) || (data.crewmates.Count > 0 && ((data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) || (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) || (data.maxNeutralKillingRoles > 0 && ensuredNeutralKillingRoles.Count > 0))))
            {
                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralKillingRoles > 0 && ensuredNeutralKillingRoles.Count > 0) rolesToAssign.Add(RoleType.NeutralKilling, ensuredNeutralKillingRoles);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral || roleType == RoleType.NeutralKilling ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                    {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = 0;
                        if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = 0;
                        if (data.neutralKillingSettings.ContainsKey(blockedRoleId)) data.neutralKillingSettings[blockedRoleId] = 0;
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = 0;
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
                    case RoleType.Neutral: data.maxNeutralRoles--; neutValues -= 10; break;
                    case RoleType.NeutralKilling: data.maxNeutralKillingRoles--; neutKillValues -= 10; break;
                    case RoleType.Impostor: data.maxImpostorRoles--; impValues -= 10; break;
                }
            }
        }

        private static void assignDependentRoles(RoleAssignmentData data)
        {
            // Roles that prob have a dependent role
            bool draculaFlag = CustomOptionHolder.vampireHunterSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaSpawnRate.getSelection() > 0;

            if (!draculaFlag) return;

            int crew = data.crewmates.Count < data.maxCrewmateRoles ? data.crewmates.Count : data.maxCrewmateRoles; // Max number of crew loops
            int neut = data.crewmates.Count < data.maxNeutralKillingRoles ? data.crewmates.Count : data.maxNeutralKillingRoles; // Max number of kneut loops
            int crewSteps = crew / data.crewSettings.Keys.Count(); // Avarage crewvalues deducted after each loop 
            int neutSteps = neut / data.neutralKillingSettings.Keys.Count(); // Avarage kneutValues deducted after each loop 

            // set to false if needed, otherwise we can skip the loop
            bool isDracula = !draculaFlag;

            // --- Simulate Crew & Neut ticket system ---
            while (crew > 0 && !isDracula)
            {
                if (!isDracula && rnd.Next(neutKillValues) < CustomOptionHolder.draculaSpawnRate.getSelection()) isDracula = true;
                neut--;
                neutKillValues -= neutSteps;
            }

            // --- Assign Main Roles if they won the lottery ---
            if (isDracula && Dracula.dracula == null && data.crewmates.Count > 0 && data.maxNeutralKillingRoles > 0 && draculaFlag)
            {
                // Set Dracula cause he won the lottery
                byte dracula = setRoleToRandomPlayer((byte)RoleId.Dracula, data.crewmates);
                data.crewmates.ToList().RemoveAll(x => x.PlayerId == dracula);
                data.maxNeutralKillingRoles--;
            }

            // --- Assign Dependent Roles if main role exists ---
            if (Dracula.dracula != null)
            {
                // Vampire Hunter
                if (CustomOptionHolder.vampireHunterSpawnRate.getSelection() == 10 && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                {
                    // Force Vampire Hunter
                    byte vampireHunter = setRoleToRandomPlayer((byte)RoleId.VampireHunter, data.crewmates);
                    data.crewmates.ToList().RemoveAll(x => x.PlayerId == vampireHunter);
                    data.maxCrewmateRoles--;
                }
                else if (CustomOptionHolder.vampireHunterSpawnRate.getSelection() < 10)
                {
                    // Dont force, add Vampire Hunter to the ticket system
                    data.crewSettings.Add((byte)RoleId.VampireHunter, CustomOptionHolder.vampireHunterSpawnRate.getSelection());
                }
            }

            if (!data.neutralKillingSettings.ContainsKey((byte)RoleId.Dracula)) data.neutralKillingSettings.Add((byte)RoleId.Dracula, 0);
        }

        private static void assignChanceRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> neutralKillingTickets = data.neutralKillingSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value > 0 && x.Value < 10).Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) || (data.crewmates.Count > 0 && ((data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) || (data.maxNeutralRoles > 0 && neutralTickets.Count > 0) || (data.maxNeutralKillingRoles > 0 && neutralKillingTickets.Count > 0))))
            {
                Dictionary<RoleType, List<byte>> rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, neutralTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralKillingRoles > 0 && neutralKillingTickets.Count > 0) rolesToAssign.Add(RoleType.NeutralKilling, neutralKillingTickets);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(RoleType.Impostor, impostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral || roleType == RoleType.NeutralKilling ? data.crewmates : data.impostors;
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
                        neutralTickets.RemoveAll(x => x == blockedRoleId);
                        neutralKillingTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case RoleType.Crewmate: data.maxCrewmateRoles--; break;
                    case RoleType.Neutral: data.maxNeutralRoles--; break;
                    case RoleType.NeutralKilling: data.maxNeutralKillingRoles--; break;
                    case RoleType.Impostor: data.maxImpostorRoles--; break;
                }
            }
        }

        public static void assignRoleTargets(RoleAssignmentData data)
        {
            // Set Lawyer Target
            if (Lawyer.lawyer != null)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && (p.isKiller() || (Lawyer.targetCanBeJester && p.IsJester(out _))))
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0)
                {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.lawyerPromotes();
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerSetTarget, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerSetTarget(target.PlayerId);
                }
            }

            // Set Executioner Target
            if (Executioner.executioner != null)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && !p.isEvil() && p != Swapper.swapper)
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0)
                {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.executionerPromotes();
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerSetTarget, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerSetTarget(target.PlayerId);
                }
            }

            // Set Guardian Angel Target
            if (GuardianAngel.guardianAngel != null)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 && !p.isEvil() && p != Executioner.target)
                        possibleTargets.Add(p);

                if (possibleTargets.Count == 0)
                {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.guardianAngelPromotes();
                }
                else
                {
                    var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelSetTarget, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelSetTarget(target.PlayerId);
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
            if (isGuesserGamemode && !CustomOptionHolder.guesserGamemodeHaveModifier.getBool())
                players.RemoveAll(x => GuesserGM.isGuesser(x.PlayerId));
            int modifierCount = Mathf.Min(players.Count, modifierCountSettings);

            if (modifierCount == 0) return;

            List<RoleId> allModifiers = new List<RoleId>();
            List<RoleId> ensuredModifiers = new List<RoleId>();
            List<RoleId> chanceModifiers = new List<RoleId>();
            allModifiers.AddRange(new List<RoleId>
            {
                RoleId.Bait,
                RoleId.Blind,
                RoleId.ButtonBarry,
                RoleId.Shy,
                RoleId.Flash,
                RoleId.Mini,
                RoleId.Indomitable,
                RoleId.Multitasker,
                RoleId.Radar,
                RoleId.Sleuth,
                RoleId.Tiebreaker,
                RoleId.Torch,
                RoleId.Vip,
                RoleId.Drunk,
                RoleId.Immovable,
                RoleId.DoubleShot,
                RoleId.Ruthless,
                RoleId.Underdog,
                RoleId.Saboteur,
                RoleId.Frosty,
                RoleId.Satelite,
                RoleId.SixthSense,
                RoleId.Taskmaster,
                RoleId.Disperser,
                RoleId.Poucher,
            });

            if (rnd.Next(1, 101) <= CustomOptionHolder.modifierLover.getSelection() * 10)
            { 
                // Assign lover
                bool isEvilLover = rnd.Next(1, 101) <= CustomOptionHolder.modifierLoverImpLoverRate.getSelection() * 10;
                byte firstLoverId;
                List<PlayerControl> impPlayer = new List<PlayerControl>(players);
                List<PlayerControl> crewPlayer = new List<PlayerControl>(players);
                impPlayer.RemoveAll(x => !x.isKiller());
                crewPlayer.RemoveAll(x => x.isKiller() || x == Lawyer.lawyer || x == Executioner.executioner || x == GuardianAngel.guardianAngel);

                if (isEvilLover) firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, impPlayer);
                else firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer);
                byte secondLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer, 1);

                players.RemoveAll(x => x.PlayerId == firstLoverId || x.PlayerId == secondLoverId);
                modifierCount--;
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

        public static void assignGuesserGamemode()
        {
            List<PlayerControl> crewPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> neutralBenignPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> neutralEvilPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> neutralKillingPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            List<PlayerControl> impPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
            neutralBenignPlayer.RemoveAll(x => !x.isNeutralBenign());
            neutralEvilPlayer.RemoveAll(x => !x.isNeutralEvil() || x == Doomsayer.doomsayer);
            neutralKillingPlayer.RemoveAll(x => !x.isNeutralKilling());
            crewPlayer.RemoveAll(x => x.isEvil());
            try
            {
                assignGuesserGamemodeToPlayers(crewPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewmateNumber.getFloat()));
            }
            catch { }
            try
            {
                assignGuesserGamemodeToPlayers(neutralBenignPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNeutralBenignNumber.getFloat()));
            }
            catch { }
            try
            {
                assignGuesserGamemodeToPlayers(neutralEvilPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNeutralEvilNumber.getFloat()));
            }
            catch { }
            try
            {
                assignGuesserGamemodeToPlayers(neutralKillingPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNeutralKillingNumber.getFloat()));
            }
            catch { }
            try
            {
                assignGuesserGamemodeToPlayers(impPlayer, Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeImpostorNumber.getFloat()));
            }
            catch { }
        }

        private static void assignGuesserGamemodeToPlayers(List<PlayerControl> playerList, int count)
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

        private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
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

        private static byte setModifierToRandomPlayer(byte modifierId, List<PlayerControl> playerList, byte flag = 0)
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

        private static void assignModifiersToPlayers(List<RoleId> modifiers, List<PlayerControl> playerList, int modifierCount)
        {
            modifiers = modifiers.OrderBy(x => rnd.Next()).ToList(); // randomize list

            while (modifierCount < modifiers.Count)
            {
                var index = rnd.Next(0, modifiers.Count);
                modifiers.RemoveAt(index);
            }

            byte playerId;

            List<PlayerControl> crewPlayer = new List<PlayerControl>(playerList);
            crewPlayer.RemoveAll(x => x.isEvil());
            
            List<PlayerControl> impPlayer = new List<PlayerControl>(playerList);
            impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);

            if (modifiers.Contains(RoleId.Bait))
            {
                var baitCount = 0;
                while (baitCount < modifiers.FindAll(x => x == RoleId.Bait).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Bait, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    baitCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Bait);
            }

            if (modifiers.Contains(RoleId.Blind))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Blind, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Blind);
            }

            if (modifiers.Contains(RoleId.Indomitable))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Indomitable, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Indomitable);
            }

            if (modifiers.Contains(RoleId.Multitasker))
            {
                var multitaskerCount = 0;
                while (multitaskerCount < modifiers.FindAll(x => x == RoleId.Multitasker).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Multitasker, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    multitaskerCount++;
                }
                modifiers.RemoveAll(x => x == RoleId.Multitasker);
            }

            if (modifiers.Contains(RoleId.Torch))
            {
                var torchCount = 0;
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
                var torchCount = 0;
                while (torchCount < modifiers.FindAll(x => x == RoleId.Vip).Count)
                {
                    playerId = setModifierToRandomPlayer((byte)RoleId.Vip, crewPlayer);
                    crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                    playerList.RemoveAll(x => x.PlayerId == playerId);
                    torchCount++;
                }

                modifiers.RemoveAll(x => x == RoleId.Vip);
            }

            if (modifiers.Contains(RoleId.DoubleShot))
            {
                var dImpPlayer = new List<PlayerControl>(impPlayer);
                dImpPlayer.RemoveAll(x => !HandleGuesser.isGuesser(x.PlayerId));
                playerId = setModifierToRandomPlayer((byte)RoleId.DoubleShot, dImpPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.DoubleShot);
            }

            if (modifiers.Contains(RoleId.Ruthless))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Ruthless, impPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Ruthless);
            }

            if (modifiers.Contains(RoleId.Underdog))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Underdog, impPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Underdog);
            }

            if (modifiers.Contains(RoleId.Saboteur))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Saboteur, impPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Saboteur);
            }

            if (modifiers.Contains(RoleId.Frosty))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Frosty, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Frosty);
            }

            if (modifiers.Contains(RoleId.Taskmaster))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Taskmaster, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Taskmaster);
            }

            if (modifiers.Contains(RoleId.Disperser))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Disperser, impPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Disperser);
            }
            
            if (modifiers.Contains(RoleId.Poucher))
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Poucher, impPlayer);
                impPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                modifiers.RemoveAll(x => x == RoleId.Poucher);
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
                    selection = CustomOptionHolder.modifierBait.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierBaitQuantity.getQuantity();
                    break;
                case RoleId.Blind:
                    selection = CustomOptionHolder.modifierBlind.getSelection(); break;
                case RoleId.ButtonBarry:
                    selection = CustomOptionHolder.modifierButtonBarry.getSelection(); break;
                case RoleId.Shy:
                    selection = CustomOptionHolder.modifierShy.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierShyQuantity.getQuantity();
                    break;
                case RoleId.Flash:
                    selection = CustomOptionHolder.modifierFlash.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierFlashQuantity.getQuantity();
                    break;
                case RoleId.Mini:
                    selection = CustomOptionHolder.modifierMini.getSelection(); break;
                case RoleId.Indomitable:
                    selection = CustomOptionHolder.modifierIndomitable.getSelection(); break;
                case RoleId.Lover:
                    selection = CustomOptionHolder.modifierLover.getSelection(); break;
                case RoleId.Multitasker:
                    selection = CustomOptionHolder.modifierMultitasker.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierMultitaskerQuantity.getQuantity();
                    break;
                case RoleId.Radar:
                    selection = CustomOptionHolder.modifierRadar.getSelection(); break;
                case RoleId.Sleuth:
                    selection = CustomOptionHolder.modifierSleuth.getSelection(); break;
                case RoleId.Tiebreaker:
                    selection = CustomOptionHolder.modifierTieBreaker.getSelection(); break;
                case RoleId.Torch:
                    selection = CustomOptionHolder.modifierTorch.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierTorchQuantity.getQuantity();
                    break;
                case RoleId.Vip:
                    selection = CustomOptionHolder.modifierVip.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierVipQuantity.getQuantity();
                    break;
                case RoleId.Drunk:
                    selection = CustomOptionHolder.modifierDrunk.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierDrunkQuantity.getQuantity();
                    break;
                case RoleId.Immovable:
                    selection = CustomOptionHolder.modifierImmovable.getSelection();
                    if (multiplyQuantity) selection *= CustomOptionHolder.modifierImmovableQuantity.getQuantity();
                    break;
                case RoleId.DoubleShot:
                    selection = CustomOptionHolder.modifierDoubleShot.getSelection(); break;
                case RoleId.Ruthless:
                    selection = CustomOptionHolder.modifierRuthless.getSelection(); break;
                case RoleId.Underdog:
                    selection = CustomOptionHolder.modifierUnderdog.getSelection(); break;
                case RoleId.Saboteur:
                    selection = CustomOptionHolder.modifierSaboteur.getSelection(); break;
                case RoleId.Frosty:
                    selection = CustomOptionHolder.modifierFrosty.getSelection(); break;
                case RoleId.Satelite:
                    selection = CustomOptionHolder.modifierSatelite.getSelection(); break;
                case RoleId.SixthSense:
                    selection = CustomOptionHolder.modifierSixthSense.getSelection(); break;
                case RoleId.Taskmaster:
                    selection = CustomOptionHolder.modifierTaskmaster.getSelection(); break;
                case RoleId.Disperser:
                    selection = CustomOptionHolder.modifierDisperser.getSelection(); break;
                case RoleId.Poucher:
                    selection = CustomOptionHolder.modifierPoucher.getSelection(); break;
            }

            return selection;
        }

        public class RoleAssignmentData
        {
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> neutralSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> neutralKillingSettings = new Dictionary<byte, int>();
            public Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            public int maxCrewmateRoles { get; set; }
            public int maxNeutralRoles { get; set; }
            public int maxNeutralKillingRoles { get; set; }
            public int maxImpostorRoles { get; set; }
        }

        private enum RoleType
        {
            Crewmate = 0,
            Neutral = 1,
            NeutralKilling = 2,
            Impostor = 3
        }
    }
}