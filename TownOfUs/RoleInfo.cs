using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs
{
    public class RoleInfo {
        public string name;
        public Color color;
        public string introDescription;
        public string shortDescription;
        public RoleId roleId;
        public FactionId factionId;

        public RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId, FactionId factionId) {
            this.name = name;
            this.color = color;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
            this.factionId = factionId;
        }
        
        // Crewmate Roles
        public static RoleInfo snitch = new RoleInfo("Snitch", Snitch.color, "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch, FactionId.Crewmate);
        public static RoleInfo engineer = new RoleInfo("Engineer",  Engineer.color, "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer, FactionId.Crewmate);
        public static RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.color, "Shoot the <color=#FF1919FF>Impostors</color>", "Shoot the Impostors", RoleId.Sheriff, FactionId.Crewmate);
        public static RoleInfo shifter = new RoleInfo("Shifter", Shifter.color, "Shift your role", "Shift your role", RoleId.Shifter, FactionId.Crewmate);
        public static RoleInfo spy = new RoleInfo("Spy", Spy.color, "Gets extra info on Admin Table and Vitals", "Gets extra info on Admin Table and Vitals", RoleId.Spy, FactionId.Crewmate);
        public static RoleInfo vigilante = new RoleInfo("Vigilante", Guesser.color, "Guess and shoot", "Guess and shoot", RoleId.Vigilante, FactionId.Crewmate);
        public static RoleInfo swapper = new RoleInfo("Swapper", Swapper.color, "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper, FactionId.Crewmate);
        public static RoleInfo mayor = new RoleInfo("Mayor", Mayor.color, "Reveal yourself when the time is right", "Reveal yourself when the time is right", RoleId.Mayor, FactionId.Crewmate);
        public static RoleInfo medic = new RoleInfo("Medic", Medic.color, "Protect someone with your shield", "Protect other players", RoleId.Medic, FactionId.Crewmate);
        public static RoleInfo investigator = new RoleInfo("Investigator", Investigator.color, "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints", RoleId.Investigator, FactionId.Crewmate);
        public static RoleInfo veteran = new RoleInfo("Veteran", Veteran.color, "Protect yourself from other", "Protect yourself from others", RoleId.Veteran, FactionId.Crewmate);
        public static RoleInfo seer = new RoleInfo("Seer", Seer.color, "Reveal alliances of other players to find the Impostors", "Reveal alliances of other players to find the Impostors", RoleId.Seer, FactionId.Crewmate);
        public static RoleInfo mystic = new RoleInfo("Mystic", Mystic.color, "Know when and where kills happen", "Know when and where kills happen", RoleId.Mystic, FactionId.Crewmate);
        public static RoleInfo detective = new RoleInfo("Detective", Detective.color, "Examine suspicious players to find evildoers", "Examine suspicious players to find evildoers", RoleId.Detective, FactionId.Crewmate);
        public static RoleInfo vampireHunter = new RoleInfo("Vampire Hunter", VampireHunter.color, "Stake the Vampires", "Stake the Vampires", RoleId.VampireHunter, FactionId.Crewmate);
        public static RoleInfo timeLord = new RoleInfo("Time Lord", TimeLord.color, "Save yourself with your time power", "Save yourself with your time power", RoleId.TimeLord, FactionId.Crewmate);
        public static RoleInfo oracle = new RoleInfo("Oracle", Oracle.color, "Make the <color=#FF1919FF>Impostors</color> confess their sins", "Get another player to confess on your passing", RoleId.Oracle, FactionId.Crewmate);

        // Neutral Roles
        public static RoleInfo jester = new RoleInfo("Jester", Jester.color, "Get voted out", "Get voted out", RoleId.Jester, FactionId.Neutral);
        public static RoleInfo scavenger = new RoleInfo("Scavenger", Scavenger.color, "Eat corpses to win", "Eat dead bodies", RoleId.Scavenger, FactionId.Neutral);
        public static RoleInfo executioner = new RoleInfo("Executioner", Executioner.color, "Vote out your target", "Vote out your target", RoleId.Executioner, FactionId.Neutral);
        public static RoleInfo lawyer = new RoleInfo("Lawyer", Lawyer.color, "Defend your client", "Defend your client", RoleId.Lawyer, FactionId.Neutral);
        public static RoleInfo pursuer = new RoleInfo("Pursuer", Pursuer.color, "Blank the Impostors", "Blank the Impostors", RoleId.Pursuer, FactionId.Neutral);
        public static RoleInfo guardianAngel = new RoleInfo("Guardian Angel", GuardianAngel.color, "Protect your target with your life", "Protect your target with your life", RoleId.GuardianAngel, FactionId.Neutral);
        public static RoleInfo survivor = new RoleInfo("Survivor", Survivor.color, "Your target was killed. Now, stay alive", "Your target was killed. Now, stay alive", RoleId.Survivor, FactionId.Neutral);
        public static RoleInfo amnesiac = new RoleInfo("Amnesiac", Amnesiac.color, "Find a dead body to remember a role", "Find a dead body to remember a role", RoleId.Amnesiac, FactionId.Neutral);
        public static RoleInfo mercenary = new RoleInfo("Mercenary", Mercenary.color, "Stop murders with your shield to win", "Stop murders with your shield to win", RoleId.Mercenary, FactionId.Neutral);
        public static RoleInfo doomsayer = new RoleInfo("Doomsayer", Doomsayer.color, "Win by guessing player's roles", "Win by guessing player's roles", RoleId.Doomsayer, FactionId.Neutral);

        // Neutral Killing Roles
        public static RoleInfo dracula = new RoleInfo("Dracula", Dracula.color, "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Dracula, FactionId.NeutralKiller);
        public static RoleInfo vampire = new RoleInfo("Vampire", Vampire.color, "Help your Dracula to kill everyone", "Help your Dracula to kill everyone", RoleId.Vampire, FactionId.NeutralKiller);
        public static RoleInfo juggernaut = new RoleInfo("Juggernaut", Juggernaut.color, "With each kill your kill cooldown decreases", "With each kill your kill cooldown decreases", RoleId.Juggernaut, FactionId.NeutralKiller);
        public static RoleInfo fallenAngel = new RoleInfo("Fallen Angel", FallenAngel.color, "Your target was killed. Now, take your revenge", "Your target was killed. Now, take your revenge", RoleId.FallenAngel, FactionId.NeutralKiller);
        public static RoleInfo werewolf = new RoleInfo("Werewolf", Werewolf.color, "Rampage and kill everyone", "Rampage and kill everyone", RoleId.Werewolf, FactionId.NeutralKiller);
        public static RoleInfo glitch = new RoleInfo("Glitch", Glitch.color, "Murder everyone to win", "Murder everyone to win", RoleId.Glitch, FactionId.NeutralKiller);
        
        // Impostor Roles
        public static RoleInfo morphling = new RoleInfo("Morphling", Morphling.color, "Change your look to not get caught", "Change your look", RoleId.Morphling, FactionId.Impostor);
        public static RoleInfo camouflager = new RoleInfo("Camouflager", Camouflager.color, "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager, FactionId.Impostor);
        public static RoleInfo assassin = new RoleInfo("Assassin", Palette.ImpostorRed, "Guess and shoot", "Guess and shoot", RoleId.Assassin, FactionId.Impostor);
        public static RoleInfo poisoner = new RoleInfo("Poisoner", Poisoner.color, "Poison the crewmates", "Poison the crewmates", RoleId.Poisoner, FactionId.Impostor);
        public static RoleInfo swooper = new RoleInfo("Swooper", Swooper.color, "Turn invisible and sneakily kill", "Turn invisible and sneakily kill", RoleId.Swooper, FactionId.Impostor);
        public static RoleInfo blackmailer = new RoleInfo("Blackmailer", Blackmailer.color, "Blackmail those who seek to hurt you", "Blackmail those who seek to hurt you", RoleId.Blackmailer, FactionId.Impostor);
        public static RoleInfo escapist = new RoleInfo("Escapist", Escapist.color, "Recall to a marked location and back", "Recall to a marked location and back", RoleId.Escapist, FactionId.Impostor);
        public static RoleInfo miner = new RoleInfo("Miner", Miner.color, "Use your miner vents to surprise others", "Surprise your enemies", RoleId.Miner, FactionId.Impostor);
        public static RoleInfo cleaner = new RoleInfo("Cleaner", Cleaner.color, "Kill everyone and leave no traces", "Clean up dead bodies", RoleId.Cleaner, FactionId.Impostor);
        public static RoleInfo phantom = new RoleInfo("Phantom", Phantom.color, "Use your ghost form to kill crewmates", "Use your ghost form to kill crewmates", RoleId.Phantom, FactionId.Impostor);
        public static RoleInfo grenadier = new RoleInfo("Grenadier", Grenadier.color, "Blind the crewmates to get sneaky kills", "Blind the crewmates to get sneaky kills", RoleId.Grenadier, FactionId.Impostor);
        public static RoleInfo venerer = new RoleInfo("Venerer", Venerer.color, "Kill players to unlock ability perks", "Kill players to unlock ability perks", RoleId.Venerer, FactionId.Impostor);
        public static RoleInfo bountyHunter = new RoleInfo("Bounty Hunter", BountyHunter.color, "Hunt your bounty down", "Hunt your bounty down", RoleId.BountyHunter, FactionId.Impostor);
        public static RoleInfo bomber = new RoleInfo("Bomber", Bomber.color, "Plant bombs to kill crewmates", "Plant bombs to kill crewmates", RoleId.Bomber, FactionId.Impostor);
        public static RoleInfo medusa = new RoleInfo("Medusa", Medusa.color, "Petrify players", "Petrify players", RoleId.Medusa, FactionId.Impostor);
        public static RoleInfo archer = new RoleInfo("Archer", Archer.color, "Make range kills", "Pick bow with F nand right click to shoot", RoleId.Archer, FactionId.Impostor);

        // Modifiers
        public static RoleInfo lover = new RoleInfo("Lover", Lovers.color, $"You are in love", $"You are in love", RoleId.Lover, FactionId.Modifier);
        public static RoleInfo blind = new RoleInfo("Blind", Palette.CrewmateBlue, "Your report button doesn't ligths up", "Your report button doesn't ligths up", RoleId.Blind, FactionId.Modifier);
        public static RoleInfo bait = new RoleInfo("Bait", Palette.CrewmateBlue, "Bait your enemies", "Bait your enemies", RoleId.Bait, FactionId.Modifier);
        public static RoleInfo sleuth = new RoleInfo("Sleuth", Color.yellow, "Learn the roles of bodies you report", "Learn the roles of bodies you report", RoleId.Sleuth, FactionId.Modifier);
        public static RoleInfo tiebreaker = new RoleInfo("Tiebreaker", Color.yellow, "Your vote breaks the tie", "Break the tie", RoleId.Tiebreaker, FactionId.Modifier);
        public static RoleInfo buttonBarry = new RoleInfo("Button Barry", Color.yellow, "Call meeting from anywhere", "Call meeting from anywhere", RoleId.ButtonBarry, FactionId.Modifier);
        public static RoleInfo indomitable = new RoleInfo("Indomitable", Palette.CrewmateBlue, "Your role cannot be guessed", "Your role cannot be guessed", RoleId.Indomitable, FactionId.Modifier);
        public static RoleInfo drunk = new RoleInfo("Drunk", Color.yellow, "Your movement is inverted", "Your movement is inverted", RoleId.Drunk, FactionId.Modifier);
        public static RoleInfo sunglasses = new RoleInfo("Sunglasses", Palette.CrewmateBlue, "Your vision is reduced", "Your vision is reduced", RoleId.Sunglasses, FactionId.Modifier);
        public static RoleInfo torch = new RoleInfo("Torch", Palette.CrewmateBlue, "You can see in lights sabotage", "You can see in lights sabotage", RoleId.Torch, FactionId.Modifier);
        public static RoleInfo doubleShot = new RoleInfo("Double Shot", Palette.ImpostorRed, "You have extra life while assassinating", "You have extra life while assassinating", RoleId.DoubleShot, FactionId.Modifier);
        public static RoleInfo disperser = new RoleInfo("Disperser", Palette.ImpostorRed, "Separate the Crew", "Separate the Crew", RoleId.Disperser, FactionId.Modifier);
        public static RoleInfo armored = new RoleInfo("Armored", Color.yellow, "You are protected from one murder attempt", "You are protected from one murder attempt", RoleId.Armored, FactionId.Modifier);
        public static RoleInfo underdog = new RoleInfo("Underdog", Palette.ImpostorRed, "When you're alone, your kill cooldown is shortened", "When you're alone, your kill cooldown is shortened", RoleId.Underdog, FactionId.Modifier);
        public static RoleInfo teamist = new RoleInfo("Teamist", Palette.ImpostorRed, "When you aren't alone, your kill cooldown is shortened", "When you aren't alone, your kill cooldown is shortened", RoleId.Teamist, FactionId.Modifier);

        public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed, "Sabotage and kill everyone", "Sabotage and kill everyone", RoleId.Impostor, FactionId.Impostor);
        public static RoleInfo crewmate = new RoleInfo("Crewmate", Palette.CrewmateBlue, "Find the Impostors", "Find the Impostors", RoleId.Crewmate, FactionId.Crewmate);

        public static List<RoleInfo> allRoleInfos = new List<RoleInfo>() {
            crewmate,
            detective,
            engineer,
            investigator,
            mayor,
            medic,
            mystic,
            oracle,
            seer,
            sheriff,
            shifter,
            snitch,
            spy,
            swapper,
            timeLord,
            vampireHunter,
            veteran,
            vigilante,

            amnesiac,
            doomsayer,
            executioner,
            guardianAngel,
            jester,
            lawyer,
            mercenary,
            pursuer,
            scavenger,
            survivor,

            dracula,
            fallenAngel,
            glitch,
            juggernaut,
            vampire,
            werewolf,

            impostor,
            archer,
            assassin,
            bomber,
            bountyHunter,
            blackmailer,
            camouflager,
            cleaner,
            escapist,
            grenadier,
            medusa,
            miner,
            morphling,
            phantom,
            poisoner,
            swooper,
            venerer,

            bait,
            blind,
            indomitable,
            sunglasses,
            torch,

            armored,
            buttonBarry,
            drunk,
            lover,
            sleuth,
            tiebreaker,

            disperser,
            doubleShot,
            teamist,
            underdog,
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true) {
            List<RoleInfo> infos = new List<RoleInfo>();
            if (p == null) return infos;

            // Modifier
            if (showModifier) {
                if (p == Lovers.lover1 || p == Lovers.lover2) infos.Add(lover);
                if (p == Blind.blind) infos.Add(blind);
                if (p == Bait.bait) infos.Add(bait);
                if (p == Tiebreaker.tiebreaker) infos.Add(tiebreaker);
                if (p == Sleuth.sleuth) infos.Add(sleuth);
                if (p == ButtonBarry.buttonBarry) infos.Add(buttonBarry);
                if (p == Indomitable.indomitable) infos.Add(indomitable);
                if (p == Drunk.drunk) infos.Add(drunk);
                if (p == Sunglasses.sunglasses) infos.Add(sunglasses);
                if (p == Torch.torch) infos.Add(torch);
                if (p == DoubleShot.doubleShot) infos.Add(doubleShot);
                if (p == Disperser.disperser) infos.Add(disperser);
                if (p == Armored.armored) infos.Add(armored);
                if (p == Underdog.underdog) infos.Add(underdog);
                if (p == Teamist.teamist) infos.Add(teamist);
            }
            int count = infos.Count;  // Save count after modifiers are added so that the role count can be checked

            // Special Roles
            if (p == Morphling.morphling) infos.Add(morphling);
            if (p == Camouflager.camouflager) infos.Add(camouflager);
            if (p == Snitch.snitch) infos.Add(snitch);
            if (p == Engineer.engineer) infos.Add(engineer);
            if (p == Sheriff.sheriff) infos.Add(sheriff);
            if (p == Jester.jester) infos.Add(jester);
            if (p == Shifter.shifter) infos.Add(shifter);
            if (p == Spy.spy) infos.Add(spy);
            if (p == Guesser.niceGuesser) infos.Add(vigilante);
            if (p == Guesser.evilGuesser) infos.Add(assassin);
            if (p == Swapper.swapper) infos.Add(swapper);
            if (p == Mayor.mayor) infos.Add(mayor);
            if (p == Medic.medic) infos.Add(medic);
            if (p == Dracula.dracula || (Dracula.formerDraculas != null && Dracula.formerDraculas.Any(x => x.PlayerId == p.PlayerId))) infos.Add(dracula);
            if (p == Vampire.vampire) infos.Add(vampire);
            if (p == Poisoner.poisoner) infos.Add(poisoner);
            if (p == Scavenger.scavenger) infos.Add(scavenger);
            if (p == Executioner.executioner) infos.Add(executioner);
            if (p == Lawyer.lawyer) infos.Add(lawyer);
            if (p == Pursuer.pursuer) infos.Add(pursuer);
            if (p == GuardianAngel.guardianAngel) infos.Add(guardianAngel);
            if (p == FallenAngel.fallenAngel) infos.Add(fallenAngel);
            if (p == Survivor.survivor) infos.Add(survivor);
            if (p == Amnesiac.amnesiac) infos.Add(amnesiac);
            if (p == Investigator.investigator) infos.Add(investigator);
            if (p == Veteran.veteran) infos.Add(veteran);
            if (p == VampireHunter.veteran) infos.Add(veteran);
            if (p == Seer.seer) infos.Add(seer);
            if (p == Juggernaut.juggernaut) infos.Add(juggernaut);
            if (p == Swooper.swooper) infos.Add(swooper);
            if (p == Mercenary.mercenary) infos.Add(mercenary);
            if (p == Blackmailer.blackmailer) infos.Add(blackmailer);
            if (p == Escapist.escapist) infos.Add(escapist);
            if (p == Miner.miner) infos.Add(miner);
            if (p == Cleaner.cleaner) infos.Add(cleaner);
            if (p == Phantom.phantom) infos.Add(phantom);
            if (p == Grenadier.grenadier) infos.Add(grenadier);
            if (p == Doomsayer.doomsayer) infos.Add(doomsayer);
            if (p == Mystic.mystic) infos.Add(mystic);
            if (p == Werewolf.werewolf) infos.Add(werewolf);
            if (p == Detective.detective) infos.Add(detective);
            if (p == Glitch.glitch) infos.Add(glitch);
            if (p == Venerer.venerer) infos.Add(venerer);
            if (p == BountyHunter.bountyHunter) infos.Add(bountyHunter);
            if (p == Bomber.bomber) infos.Add(bomber);
            if (p == VampireHunter.vampireHunter) infos.Add(vampireHunter);
            if (p == TimeLord.timeLord) infos.Add(timeLord);
            if (p == Oracle.oracle) infos.Add(oracle);
            if (p == Medusa.medusa) infos.Add(medusa);
            if (p == Archer.archer) infos.Add(archer);

            // Default roles
            if (infos.Count == count) infos.Add(p.Data.Role.IsImpostor ? impostor : crewmate);
            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true, bool suppressGhostInfo = false) {
            string roleName;
            roleName = String.Join(" ", getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
            if (Executioner.target != null && p.PlayerId == Executioner.target.PlayerId && PlayerControl.LocalPlayer != Executioner.target) 
                roleName += (useColors ? Helpers.cs(Executioner.color, " X") : " X");
            if (Lawyer.target != null && p.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target) 
                roleName += (useColors ? Helpers.cs(Lawyer.color, " §") : " §");
            if (GuardianAngel.target != null && p.PlayerId == GuardianAngel.target.PlayerId && PlayerControl.LocalPlayer != GuardianAngel.target) 
                roleName += (useColors ? Helpers.cs(GuardianAngel.color, " ★") : " ★");
            if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId))
                roleName += " (Gs)";

            if (!suppressGhostInfo && p != null) {
                if (p == Shifter.shifter && (PlayerControl.LocalPlayer == Shifter.shifter || Helpers.shouldShowGhostInfo()) && Shifter.futureShift != null)
                    roleName += Helpers.cs(Color.yellow, " ← " + Shifter.futureShift.Data.PlayerName);
                if (p == Scavenger.scavenger && (PlayerControl.LocalPlayer == Scavenger.scavenger || Helpers.shouldShowGhostInfo()))
                    roleName = roleName + Helpers.cs(Scavenger.color, $" ({Scavenger.scavengerNumberToWin - Scavenger.eatenBodies} left)");
                if (Helpers.shouldShowGhostInfo()) {
                    if (p == Dracula.fakeVampire)
                        roleName = Helpers.cs(Dracula.color, "(fake Vamp) ") + roleName;
                    if (Poisoner.poisoner != null && !Poisoner.poisoner.Data.IsDead && Poisoner.poisoned == p && !p.Data.IsDead)
                        roleName = Helpers.cs(Poisoner.color, $"(poisoned {(int)HudManagerStartPatch.poisonerButton.Timer + 1}) ") + roleName;
                    if (Veteran.veteran != null && !Veteran.veteran.Data.IsDead && Veteran.veteran == p && Veteran.isAlertActive)
                        roleName = Helpers.cs(Veteran.color, "! ") + roleName;
                    if (VampireHunter.veteran != null && !VampireHunter.veteran.Data.IsDead && VampireHunter.veteran == p && VampireHunter.isAlertActive)
                        roleName = Helpers.cs(Veteran.color, "! ") + roleName;
                    if  (Glitch.hackedPlayer != null && !Glitch.hackedPlayer.Data.IsDead && Glitch.hackedPlayer == p)
                        roleName = Helpers.cs(Glitch.color, "(hacked) ") + roleName;
                    if (BountyHunter.bounty == p && !MeetingHud.Instance)
                        roleName = Helpers.cs(BountyHunter.color, "(bounty) ") + roleName;
                    if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == p)
                        roleName = roleName + $" ({Doomsayer.guessesToWin - Doomsayer.guessedToWin} left)";
                    // Death Reason on Ghosts
                    if (p.Data.IsDead)
                    {
                        string deathReasonString = "";
                        var deadPlayer = GameHistory.deadPlayers.FirstOrDefault(x => x.player.PlayerId == p.PlayerId);

                        Color killerColor = new();
                        if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                        {
                            killerColor = RoleInfo.getRoleInfoForPlayer(deadPlayer.killerIfExisting, false).FirstOrDefault().color;
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
                                case DeadPlayer.CustomDeathReason.SheriffMissfire:
                                    deathReasonString = $" - checked {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Shift:
                                    deathReasonString = $" - {Helpers.cs(Color.yellow, "shifted")} {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Guess:
                                    if (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName)
                                        deathReasonString = $" - failed guess";
                                    else
                                        deathReasonString = $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.LoverSuicide:
                                    deathReasonString = $" - {Helpers.cs(Lovers.color, "lover died")}";
                                    break;
                                case DeadPlayer.CustomDeathReason.LawyerSuicide:
                                    deathReasonString = $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}";
                                    break;
                                case DeadPlayer.CustomDeathReason.GuardianAngelSuicide:
                                    deathReasonString = $" - {Helpers.cs(GuardianAngel.color, "bad Guardian Angel")}";
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