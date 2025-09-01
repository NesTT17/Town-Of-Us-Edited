using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs
{
    public class RoleInfo
    {
        public string name;
        public Color color;
        public string introDescription;
        public string shortDescription;
        public RoleId roleId;
        public FactionId factionId;
        public bool isImpostor => factionId == FactionId.Impostor;
        public static Dictionary<RoleId, RoleInfo> roleInfoById = new();

        public RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId, FactionId factionId)
        {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
            this.factionId = factionId;
            roleInfoById.TryAdd(roleId, this);
        }

        #region Special Roles
        public static RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.color, "Shoot the Impostors", "Shoot the impostors", RoleId.Sheriff, FactionId.Crewmate);
        public static RoleInfo seer = new RoleInfo("Seer", Seer.color, "Reveal alliances of other players to find the Impostors", "Reveal alliances of other players to find the Impostors", RoleId.Seer, FactionId.Crewmate);
        public static RoleInfo engineer = new RoleInfo("Engineer", Engineer.color, "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer, FactionId.Crewmate);
        public static RoleInfo jester = new RoleInfo("Jester", Jester.color, "Get voted out", "Get voted out", RoleId.Jester, FactionId.EvilNeutral);
        public static RoleInfo juggernaut = new RoleInfo("Juggernaut", Juggernaut.color, "With each kill your kill cooldown decreases", "With each kill your kill cooldown decreases", RoleId.Juggernaut, FactionId.KillingNeutral);
        public static RoleInfo snitch = new RoleInfo("Snitch", Snitch.color, "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch, FactionId.Crewmate);
        public static RoleInfo veteran = new RoleInfo("Veteran", Veteran.color, "Protect yourself from other", "Protect yourself from others", RoleId.Veteran, FactionId.Crewmate);
        public static RoleInfo camouflager = new RoleInfo("Camouflager", Camouflager.color, "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager, FactionId.Impostor);
        public static RoleInfo morphling = new RoleInfo("Morphling", Morphling.color, "Change your look to not get caught", "Change your look", RoleId.Morphling, FactionId.Impostor);
        public static RoleInfo lawyer = new RoleInfo("Lawyer", Lawyer.color, "Defend your client", "Defend your client", RoleId.Lawyer, FactionId.BenignNeutral);
        public static RoleInfo pursuer = new RoleInfo("Pursuer", Pursuer.color, "Blank the Impostors", "Blank the Impostors", RoleId.Pursuer, FactionId.BenignNeutral);
        public static RoleInfo politician = new RoleInfo("Politician", Politician.color, "Spread your campaign to become the Mayor", "Spread your campaign to become the Mayor", RoleId.Politician, FactionId.Crewmate);
        public static RoleInfo mayor = new RoleInfo("Mayor", Mayor.color, "Lead the town to victory", "Lead the town to victory", RoleId.Mayor, FactionId.Crewmate);
        public static RoleInfo plaguebearer = new RoleInfo("Plaguebearer", Plaguebearer.color, "Infect everyone to become Pestilence", "Infect everyone to become Pestilence", RoleId.Plaguebearer, FactionId.KillingNeutral);
        public static RoleInfo pestilence = new RoleInfo("Pestilence", Pestilence.color, "Kill everyone with your unstoppable abilities", "Kill everyone with your unstoppable abilities", RoleId.Pestilence, FactionId.KillingNeutral);
        public static RoleInfo werewolf = new RoleInfo("Werewolf", Werewolf.color, "Rampage and kill everyone", "Rampage and kill everyone", RoleId.Werewolf, FactionId.KillingNeutral);
        public static RoleInfo dracula = new RoleInfo("Dracula", Dracula.color, "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Dracula, FactionId.KillingNeutral);
        public static RoleInfo vampire = new RoleInfo("Vampire", Vampire.color, "Help your Dracula to kill everyone", "Help your Dracula to kill everyone", RoleId.Vampire, FactionId.KillingNeutral);
        public static RoleInfo swapper = new RoleInfo("Swapper", Swapper.color, "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper, FactionId.Crewmate);
        public static RoleInfo vampireHunter = new RoleInfo("Vampire Hunter", VampireHunter.color, "Stake the Vampires", "Stake the Vampires", RoleId.VampireHunter, FactionId.Crewmate);
        public static RoleInfo medic = new RoleInfo("Medic", Medic.color, "Protect someone with your shield", "Protect other players", RoleId.Medic, FactionId.Crewmate);
        public static RoleInfo survivor = new RoleInfo("Survivor", Survivor.color, "Stay alive to win", "Stay alive to win", RoleId.Survivor, FactionId.BenignNeutral);
        public static RoleInfo guardianAngel = new RoleInfo("Guardian Angel", GuardianAngel.color, "Protect your target with your life", "Protect your target with your life", RoleId.GuardianAngel, FactionId.BenignNeutral);
        public static RoleInfo amnesiac = new RoleInfo("Amnesiac", Amnesiac.color, "Find a dead body to remember a role", "Find a dead body to remember a role", RoleId.Amnesiac, FactionId.BenignNeutral);
        public static RoleInfo executioner = new RoleInfo("Executioner", Executioner.color, "Vote out your target", "Vote out your target", RoleId.Executioner, FactionId.EvilNeutral);
        public static RoleInfo spy = new RoleInfo("Spy", Spy.color, "Gets extra info on Admin Table and Vitals", "Gets extra info on Admin Table and Vitals", RoleId.Spy, FactionId.Crewmate);
        public static RoleInfo poisoner = new RoleInfo("Poisoner", Poisoner.color, "Poison the crewmates", "Poison the crewmates", RoleId.Poisoner, FactionId.Impostor);
        public static RoleInfo scavenger = new RoleInfo("Scavenger", Scavenger.color, "Eat corpses to win", "Eat dead bodies", RoleId.Scavenger, FactionId.EvilNeutral);
        public static RoleInfo investigator = new RoleInfo("Investigator", Investigator.color, "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints", RoleId.Investigator, FactionId.Crewmate);
        public static RoleInfo janitor = new RoleInfo("Janitor", Janitor.color, "Kill everyone and leave no traces", "Clean up dead bodies", RoleId.Janitor, FactionId.Impostor);
        public static RoleInfo swooper = new RoleInfo("Swooper", Swooper.color, "Turn invisible and sneakily kill", "Turn invisible and sneakily kill", RoleId.Swooper, FactionId.Impostor);
        public static RoleInfo oracle = new RoleInfo("Oracle", Oracle.color, "Make the <color=#FF1919FF>Impostors</color> confess their sins", "Get another player to confess on your passing", RoleId.Oracle, FactionId.Crewmate);
        public static RoleInfo mercenary = new RoleInfo("Mercenary", Mercenary.color, "Stop abilities with your shield to gain brilders", "Stop abilities with your shield to gain brilders", RoleId.Mercenary, FactionId.EvilNeutral);
        public static RoleInfo blackmailer = new RoleInfo("Blackmailer", Blackmailer.color, "Blackmail those who seek to hurt you", "Blackmail those who seek to hurt you", RoleId.Blackmailer, FactionId.Impostor);
        public static RoleInfo grenadier = new RoleInfo("Grenadier", Grenadier.color, "Blind the crewmates to get sneaky kills", "Blind the crewmates to get sneaky kills", RoleId.Grenadier, FactionId.Impostor);
        public static RoleInfo mystic = new RoleInfo("Mystic", Mystic.color, "Know when and where kills happen", "Know when and where kills happen", RoleId.Mystic, FactionId.Crewmate);
        public static RoleInfo glitch = new RoleInfo("Glitch", Glitch.color, "Murder everyone to win", "Murder everyone to win", RoleId.Glitch, FactionId.KillingNeutral);
        public static RoleInfo transporter = new RoleInfo("Transporter", Transporter.color, "Choose two players to swap locations", "Choose two players to swap locations", RoleId.Transporter, FactionId.Crewmate);
        public static RoleInfo agent = new RoleInfo("Agent", Agent.color, "Confuse the <color=#FF1919FF>Impostors</color>", "Confuse the Impostors", RoleId.Agent, FactionId.Crewmate);
        public static RoleInfo thief = new RoleInfo("Thief", Thief.color, "Steal a killers role by killing them", "Steal a killers role by killing them", RoleId.Thief, FactionId.BenignNeutral);
        public static RoleInfo lighter = new RoleInfo("Lighter", Lighter.color, "Your light never goes out", "Your light never goes out", RoleId.Lighter, FactionId.Crewmate);
        public static RoleInfo detective = new RoleInfo("Detective", Detective.color, "Examine suspicious players to find evildoers", "Examine suspicious players to find evildoers", RoleId.Detective, FactionId.Crewmate);
        public static RoleInfo bomber = new RoleInfo("Bomber", Bomber.color, "Plant bombs to kill crewmates", "Plant bombs to kill crewmates", RoleId.Bomber, FactionId.Impostor);
        public static RoleInfo venerer = new RoleInfo("Venerer", Venerer.color, "Kill players to unlock ability perks", "Kill players to unlock ability perks", RoleId.Venerer, FactionId.Impostor);
        public static RoleInfo bountyHunter = new RoleInfo("Bounty Hunter", BountyHunter.color, "Hunt your bounty down", "Hunt your bounty down", RoleId.BountyHunter, FactionId.Impostor);
        #endregion

        #region Modifiers
        public static RoleInfo bait = new RoleInfo("Bait", Bait.color, "Bait your enemies", "Bait your enemies", RoleId.Bait, FactionId.Modifier);
        public static RoleInfo shifter = new RoleInfo("Shifter", Shifter.color, "Shift your role", "Shift your role", RoleId.Shifter, FactionId.Modifier);
        public static RoleInfo buttonBarry = new RoleInfo("Button Barry", ButtonBarry.color, "Call meeting from anywhere", "Call meeting from anywhere", RoleId.ButtonBarry, FactionId.Modifier);
        public static RoleInfo blind = new RoleInfo("Blind", Blind.color, "Your report button doesn't ligths up", "Your report button doesn't ligths up", RoleId.Blind, FactionId.Modifier);
        public static RoleInfo sleuth = new RoleInfo("Sleuth", Sleuth.color, "Learn the roles of bodies you report", "Learn the roles of bodies you report", RoleId.Sleuth, FactionId.Modifier);
        public static RoleInfo indomitable = new RoleInfo("Indomitable", Indomitable.color, "Your role cannot be guessed", "Your role cannot be guessed", RoleId.Indomitable, FactionId.Modifier);
        public static RoleInfo drunk = new RoleInfo("Drunk", Drunk.color, "Your movement is inverted", "Your movement is inverted", RoleId.Drunk, FactionId.Modifier);
        public static RoleInfo torch = new RoleInfo("Torch", Palette.CrewmateBlue, "You can see in lights sabotage", "You can see in lights sabotage", RoleId.Torch, FactionId.Modifier);
        public static RoleInfo vip = new RoleInfo("Vip", Vip.color, "You are the VIP", "Everyone knows when you due", RoleId.Vip, FactionId.Modifier);
        public static RoleInfo radar = new RoleInfo("Radar", Radar.color, "Be on high alert", "Be on high alert", RoleId.Radar, FactionId.Modifier);
        #endregion

        #region Vanilla Roles
        public static RoleInfo crewmate = new RoleInfo("Crewmate", Palette.CrewmateBlue, "Find the Impostors", "Find the Impostors", RoleId.Crewmate, FactionId.Crewmate);
        public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed, "Sabotage and kill everyone", "Sabotage and kill everyone", RoleId.Impostor, FactionId.Impostor);
        #endregion

        public static List<RoleInfo> allRoleInfos = new List<RoleInfo>()
        {
            crewmate,
            agent,
            detective,
            engineer,
            investigator,
            lighter,
            medic,
            mystic,
            oracle,
            politician,
            mayor,
            seer,
            sheriff,
            snitch,
            spy,
            swapper,
            transporter,
            vampireHunter,
            veteran,

            amnesiac,
            guardianAngel,
            survivor,
            lawyer,
            pursuer,
            thief,

            executioner,
            jester,
            mercenary,
            scavenger,

            dracula,
            vampire,
            glitch,
            juggernaut,
            pestilence,
            plaguebearer,
            werewolf,

            impostor,
            blackmailer,
            bomber,
            bountyHunter,
            camouflager,
            grenadier,
            janitor,
            morphling,
            poisoner,
            swooper,
            venerer,

            bait,
            blind,
            indomitable,
            shifter,
            torch,
            vip,

            buttonBarry,
            drunk,
            radar,
            sleuth,
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true, bool onlyModifiers = false)
        {
            List<RoleInfo> infos = new List<RoleInfo>();
            if (p == null) return infos;

            // Modifier
            if (showModifier)
            {
                if (p.hasModifier(RoleId.Bait)) infos.Add(bait);
                if (p.hasModifier(RoleId.Shifter)) infos.Add(shifter);
                if (p.hasModifier(RoleId.ButtonBarry)) infos.Add(buttonBarry);
                if (p.hasModifier(RoleId.Blind)) infos.Add(blind);
                if (p.hasModifier(RoleId.Sleuth)) infos.Add(sleuth);
                if (p.hasModifier(RoleId.Indomitable)) infos.Add(indomitable);
                if (p.hasModifier(RoleId.Drunk)) infos.Add(drunk);
                if (p.hasModifier(RoleId.Torch)) infos.Add(torch);
                if (p.hasModifier(RoleId.Vip)) infos.Add(vip);
                if (p.hasModifier(RoleId.Radar)) infos.Add(radar);
            }
            int count = infos.Count;
            if (onlyModifiers) return infos;

            // Special roles
            if (p.isRole(RoleId.Sheriff)) infos.Add(sheriff);
            if (p.isRole(RoleId.Seer)) infos.Add(seer);
            if (p.isRole(RoleId.Engineer)) infos.Add(engineer);
            if (p.isRole(RoleId.Jester)) infos.Add(jester);
            if (p.isRole(RoleId.Juggernaut)) infos.Add(juggernaut);
            if (p.isRole(RoleId.Snitch)) infos.Add(snitch);
            if (p.isRole(RoleId.Veteran)) infos.Add(veteran);
            if (p.isRole(RoleId.Camouflager)) infos.Add(camouflager);
            if (p.isRole(RoleId.Morphling)) infos.Add(morphling);
            if (p.isRole(RoleId.Lawyer)) infos.Add(lawyer);
            if (p.isRole(RoleId.Pursuer)) infos.Add(pursuer);
            if (p.isRole(RoleId.Politician)) infos.Add(politician);
            if (p.isRole(RoleId.Mayor)) infos.Add(mayor);
            if (p.isRole(RoleId.Plaguebearer)) infos.Add(plaguebearer);
            if (p.isRole(RoleId.Pestilence)) infos.Add(pestilence);
            if (p.isRole(RoleId.Werewolf)) infos.Add(werewolf);
            if (p.isRole(RoleId.Dracula)) infos.Add(dracula);
            if (p.isRole(RoleId.Vampire)) infos.Add(vampire);
            if (p.isRole(RoleId.Swapper)) infos.Add(swapper);
            if (p.isRole(RoleId.VampireHunter)) infos.Add(vampireHunter);
            if (p.isRole(RoleId.Medic)) infos.Add(medic);
            if (p.isRole(RoleId.Survivor)) infos.Add(survivor);
            if (p.isRole(RoleId.GuardianAngel)) infos.Add(guardianAngel);
            if (p.isRole(RoleId.Amnesiac)) infos.Add(amnesiac);
            if (p.isRole(RoleId.Executioner)) infos.Add(executioner);
            if (p.isRole(RoleId.Spy)) infos.Add(spy);
            if (p.isRole(RoleId.Poisoner)) infos.Add(poisoner);
            if (p.isRole(RoleId.Scavenger)) infos.Add(scavenger);
            if (p.isRole(RoleId.Investigator)) infos.Add(investigator);
            if (p.isRole(RoleId.Janitor)) infos.Add(janitor);
            if (p.isRole(RoleId.Swooper)) infos.Add(swooper);
            if (p.isRole(RoleId.Oracle)) infos.Add(oracle);
            if (p.isRole(RoleId.Mercenary)) infos.Add(mercenary);
            if (p.isRole(RoleId.Blackmailer)) infos.Add(blackmailer);
            if (p.isRole(RoleId.Grenadier)) infos.Add(grenadier);
            if (p.isRole(RoleId.Mystic)) infos.Add(mystic);
            if (p.isRole(RoleId.Glitch)) infos.Add(glitch);
            if (p.isRole(RoleId.Transporter)) infos.Add(transporter);
            if (p.isRole(RoleId.Agent)) infos.Add(agent);
            if (p.isRole(RoleId.Thief)) infos.Add(thief);
            if (p.isRole(RoleId.Lighter)) infos.Add(lighter);
            if (p.isRole(RoleId.Detective)) infos.Add(detective);
            if (p.isRole(RoleId.Bomber)) infos.Add(bomber);
            if (p.isRole(RoleId.Venerer)) infos.Add(venerer);
            if (p.isRole(RoleId.BountyHunter)) infos.Add(bountyHunter);

            // Vanilla Roles
            if (infos.Count == count) infos.Add(p.Data.Role.IsImpostor ? impostor : crewmate);

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true, bool suppressGhostInfo = false)
        {
            string roleName;
            roleName = String.Join(" ", getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
            if (Lawyer.target != null && p.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target)
                roleName += useColors ? Helpers.cs(Lawyer.color, " §") : " §";
            if (GuardianAngel.target != null && p.PlayerId == GuardianAngel.target.PlayerId && PlayerControl.LocalPlayer != GuardianAngel.target)
                roleName += useColors ? Helpers.cs(GuardianAngel.color, " ★") : " ★";
            if (Executioner.target != null && p.PlayerId == Executioner.target.PlayerId && PlayerControl.LocalPlayer != Executioner.target)
                roleName += useColors ? Helpers.cs(Executioner.color, " X") : " X";
            if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId))
            {
                int remainingShots = HandleGuesser.remainingShots(p.PlayerId);
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(p.Data);
                var color = Color.white;
                string guesserModifier = " (Gs)";
                if ((!Helpers.isEvil(p) && playerCompleted < HandleGuesser.tasksToUnlock) || remainingShots == 0) color = Color.gray;
                roleName += useColors ? Helpers.cs(color, guesserModifier) : guesserModifier;
            }

            if (!suppressGhostInfo && p != null)
            {
                foreach (var scavenger in Scavenger.players)
                    {
                        if (p == scavenger.player && (PlayerControl.LocalPlayer == p || Helpers.shouldShowGhostInfo()))
                            roleName += Helpers.cs(Scavenger.color, $" ({Scavenger.scavengerNumberToWin - scavenger.eatenBodies} left)");
                    }
                if (Helpers.shouldShowGhostInfo())
                {
                    if (p.isRole(RoleId.Lawyer)) roleName += $" ({Lawyer.neededMeetings - Lawyer.meetings} meetings left)";
                    if (Pursuer.blankedList.Contains(p) && !p.Data.IsDead) roleName = Helpers.cs(Pursuer.color, "(blanked) ") + roleName;
                    if (p.isRole(RoleId.Werewolf) && Werewolf.players.Any(x => x.player == p && x.isRampageActive)) roleName = "! " + roleName;
                    if (p.isRole(RoleId.Veteran) && Veteran.players.Any(x => x.player == p && x.isAlertActive)) roleName = "! " + roleName;
                    if (p.isRole(RoleId.Mayor) && Mayor.players.Any(x => x.player == p && x.isBodyguardActive)) roleName = "! " + roleName;
                    if (p.isRole(RoleId.Survivor) && Survivor.players.Any(x => x.player == p && x.isSafeguardActive)) roleName = "! " + roleName;
                    if (p.isRole(RoleId.GuardianAngel) && GuardianAngel.players.Any(x => x.player == p && x.isProtectActive)) roleName = "! " + roleName;
                    if (Dracula.exists && Dracula.players.Any(x => x.fakeVampire == p)) roleName = "(fake Vamp) " + roleName;
                    if (Glitch.exists && Glitch.hackedPlayer != null && !Glitch.hackedPlayer.Data.IsDead && Glitch.hackedPlayer == p)
                        roleName = Helpers.cs(Glitch.color, "(hacked) ") + roleName;
                    // Death Reason on Ghosts
                    if (p.Data.IsDead)
                    {
                        string deathReasonString = "";
                        var deadPlayer = deadPlayers.FirstOrDefault(x => x.player.PlayerId == p.PlayerId);

                        Color killerColor = new();
                        if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                        {
                            killerColor = getRoleInfoForPlayer(deadPlayer.killerIfExisting, false).FirstOrDefault().color;
                        }

                        if (deadPlayer != null)
                        {
                            switch (deadPlayer.deathReason)
                            {
                                case DeadPlayer.CustomDeathReason.Disconnect:
                                    deathReasonString = " - disconnected";
                                    break;
                                case DeadPlayer.CustomDeathReason.Exile:
                                    deathReasonString = " - voted out";
                                    break;
                                case DeadPlayer.CustomDeathReason.Kill:
                                    deathReasonString = $" - killed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Guess:
                                    if (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName) deathReasonString = " - failed guess";
                                    else deathReasonString = $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.LawyerSuicide:
                                    deathReasonString = $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}";
                                    break;
                                case DeadPlayer.CustomDeathReason.GASuicide:
                                    deathReasonString = $" - {Helpers.cs(GuardianAngel.color, "bad Guardian Angel")}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Shift:
                                    deathReasonString = $" - shifted {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Bomb:
                                    deathReasonString = $" - bombed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                            }
                            roleName = roleName + deathReasonString;
                        }
                    }
                }
            }
            return roleName;
        }
    }
}