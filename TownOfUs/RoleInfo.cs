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
        public bool isImpostor => color == Palette.ImpostorRed;
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


        // Special Roles
        public static RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.color, "Shoot the Impostors", "Shoot the Impostors", RoleId.Sheriff, FactionId.Crewmate);
        public static RoleInfo jester = new RoleInfo("Jester", Jester.color, "Get voted out", "Get voted out", RoleId.Jester, FactionId.NeutralEvil);
        public static RoleInfo survivor = new RoleInfo("Survivor", Survivor.color, "Stay alive to win", "Stay alive to win", RoleId.Survivor, FactionId.NeutralBenign);
        public static RoleInfo juggernaut = new RoleInfo("Juggernaut", Juggernaut.color, "With each kill your kill cooldown decreases", "With each kill your kill cooldown decreases", RoleId.Juggernaut, FactionId.NeutralKilling);
        public static RoleInfo seer = new RoleInfo("Seer", Seer.color, "Reveal alliances of other players to find the Impostors", "Reveal alliances of other players to find the Impostors", RoleId.Seer, FactionId.Crewmate);
        public static RoleInfo engineer = new RoleInfo("Engineer", Engineer.color, "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer, FactionId.Crewmate);
        public static RoleInfo snitch = new RoleInfo("Snitch", Snitch.color, "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch, FactionId.Crewmate);
        public static RoleInfo veteran = new RoleInfo("Veteran", Veteran.color, "Protect yourself from other", "Protect yourself from others", RoleId.Veteran, FactionId.Crewmate);
        public static RoleInfo camouflager = new RoleInfo("Camouflager", Camouflager.color, "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager, FactionId.Impostor);
        public static RoleInfo morphling = new RoleInfo("Morphling", Morphling.color, "Change your look to not get caught", "Change your look", RoleId.Morphling, FactionId.Impostor);
        public static RoleInfo pursuer = new RoleInfo("Pursuer", Pursuer.color, "Blank the Impostors", "Blank the Impostors", RoleId.Pursuer, FactionId.NeutralBenign);
        public static RoleInfo amnesiac = new RoleInfo("Amnesiac", Amnesiac.color, "Steal roles from the dead", "You forgot", RoleId.Amnesiac, FactionId.NeutralBenign);
        public static RoleInfo thief = new RoleInfo("Thief", Thief.color, "Steal a killers role by killing them", "Steal a killers role", RoleId.Thief, FactionId.NeutralBenign);
        public static RoleInfo lawyer = new RoleInfo("Lawyer", Lawyer.color, "Defend your client", "Defend your client", RoleId.Lawyer, FactionId.NeutralBenign);
        public static RoleInfo executioner = new RoleInfo("Executioner", Executioner.color, "Vote out your target", "Vote out your target", RoleId.Executioner, FactionId.NeutralEvil);
        public static RoleInfo medic = new RoleInfo("Medic", Medic.color, "Protect someone with your shield", "Protect other players", RoleId.Medic, FactionId.Crewmate);
        public static RoleInfo swapper = new RoleInfo("Swapper", Swapper.color, "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper, FactionId.Crewmate);
        public static RoleInfo investigator = new RoleInfo("Investigator", Investigator.color, "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Find the impostors by examining footprints", RoleId.Investigator, FactionId.Crewmate);
        public static RoleInfo spy = new RoleInfo("Spy", Spy.color, "Gain extra information on the Admin Table and Vitals", "Gain extra information on the Admin Table and Vitals", RoleId.Spy, FactionId.Crewmate);
        public static RoleInfo vigilante = new RoleInfo("Vigilante", Vigilante.color, "Guess and shoot", "Guess and shoot", RoleId.Vigilante, FactionId.Crewmate);
        public static RoleInfo assassin = new RoleInfo("Assassin", Assassin.color, "Guess and shoot", "Guess and shoot", RoleId.Assassin, FactionId.Impostor);
        public static RoleInfo tracker = new RoleInfo("Tracker", Tracker.color, "Track the <color=#FF1919FF>Impostors</color> down", "Track the Impostors down", RoleId.Tracker, FactionId.Crewmate);
        public static RoleInfo trapper = new RoleInfo("Trapper", Trapper.color, "Place traps around the map", "Place traps around the map", RoleId.Trapper, FactionId.Crewmate);
        public static RoleInfo detective = new RoleInfo("Detective", Detective.color, "Examine suspicious players to find evildoers", "Examine suspicious players to find evildoers", RoleId.Detective, FactionId.Crewmate);
        public static RoleInfo mystic = new RoleInfo("Mystic", Mystic.color, "Know when and where kills happen", "Know when and where kills happen", RoleId.Mystic, FactionId.Crewmate);
        public static RoleInfo guardianAngel = new RoleInfo("Guardian Angel", GuardianAngel.color, "Protect your target with your life", "Protect your target with your life", RoleId.GuardianAngel, FactionId.NeutralBenign);
        public static RoleInfo swooper = new RoleInfo("Swooper", Swooper.color, "Turn invisible and sneakily kill", "Turn invisible and sneakily kill", RoleId.Swooper, FactionId.Impostor);
        public static RoleInfo arsonist = new RoleInfo("Arsonist", Arsonist.color, "Let them burn", "Let them burn", RoleId.Arsonist, FactionId.NeutralKilling);
        public static RoleInfo werewolf = new RoleInfo("Werewolf", Werewolf.color, "Rampage to kill everyone", "Rampage to kill everyone", RoleId.Werewolf, FactionId.NeutralKilling);
        public static RoleInfo miner = new RoleInfo("Miner", Miner.color, "Place vents around the map", "Place vents around the map", RoleId.Miner, FactionId.Impostor);
        public static RoleInfo janitor = new RoleInfo("Janitor", Janitor.color, "Clean up dead bodies", "Clean up dead bodies", RoleId.Janitor, FactionId.Impostor);
        public static RoleInfo undertaker = new RoleInfo("Undertaker", Undertaker.color, "Drag bodies around to hide them from being reported", "Drag bodies around to hide them from being reported", RoleId.Undertaker, FactionId.Impostor); 
        public static RoleInfo grenadier = new RoleInfo("Grenadier", Grenadier.color, "Blind the crewmates to get sneaky kills", "Blind the crewmates to get sneaky kills", RoleId.Grenadier, FactionId.Impostor);
        public static RoleInfo blackmailer = new RoleInfo("Blackmailer", Blackmailer.color, "Blackmail those who seek to hurt you", "Blackmail those who seek to hurt you", RoleId.Blackmailer, FactionId.Impostor);
        public static RoleInfo politician = new RoleInfo("Politician", Politician.color, "Spread your campaign to become the Mayor", "Spread your campaign to become the Mayor", RoleId.Politician, FactionId.Crewmate);
        public static RoleInfo mayor = new RoleInfo("Mayor", Mayor.color, "Lead the town to victory", "Lead the town to victory", RoleId.Mayor, FactionId.Crewmate);
        public static RoleInfo dracula = new RoleInfo("Dracula", Dracula.color, "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Dracula, FactionId.NeutralKilling);
        public static RoleInfo vampire = new RoleInfo("Vampire", Vampire.color, "Help your Dracula to kill everyone", "Help your Dracula to kill everyone", RoleId.Vampire, FactionId.NeutralKilling);
        public static RoleInfo poisoner = new RoleInfo("Poisoner", Poisoner.color, "Kill the Crewmates with your poison", "Kill the Crewmates with your poison", RoleId.Poisoner, FactionId.Impostor);
        public static RoleInfo venerer = new RoleInfo("Venerer", Venerer.color, "Kill players to unlock ability perks", "Kill players to unlock ability perks", RoleId.Venerer, FactionId.Impostor);
        public static RoleInfo plaguebearer = new RoleInfo("Plaguebearer", Plaguebearer.color, "Infect everyone to become Pestilence", "Infect everyone to become Pestilence", RoleId.Plaguebearer, FactionId.NeutralKilling);
        public static RoleInfo pestilence = new RoleInfo("Pestilence", Pestilence.color, "Kill everyone with your unstoppable abilities", "Kill everyone with your unstoppable abilities", RoleId.Pestilence, FactionId.NeutralKilling);
        public static RoleInfo doomsayer = new RoleInfo("Doomsayer", Doomsayer.color, "Win by guessing players' roles", "Win by guessing players' roles", RoleId.Doomsayer, FactionId.NeutralEvil);
        public static RoleInfo glitch = new RoleInfo("Glitch", Glitch.color, "Murder everyone to win", "Murder everyone to win", RoleId.Glitch, FactionId.NeutralKilling);
        public static RoleInfo cannibal = new RoleInfo("Cannibal", Cannibal.color, "Eat corpses to win", "Eat dead bodies", RoleId.Cannibal, FactionId.NeutralEvil);
        public static RoleInfo scavenger = new RoleInfo("Scavenger", Scavenger.color, "Hunt your bounty down", "Hunt your bounty down", RoleId.Scavenger, FactionId.Impostor);
        public static RoleInfo escapist = new RoleInfo("Escapist", Escapist.color, "Teleport to get away from bodies", "Teleport to get away from bodies", RoleId.Escapist, FactionId.Impostor);
        public static RoleInfo vampireHunter = new RoleInfo("Vampire Hunter", VampireHunter.color, "Stake the vampires", "Stake the vampires", RoleId.VampireHunter, FactionId.Crewmate);
        public static RoleInfo oracle = new RoleInfo("Oracle", Oracle.color, "Make the <color=#FF1919FF>Impostors</color> confess their sins", "Get another player to confess on your passing", RoleId.Oracle, FactionId.Crewmate);
        public static RoleInfo lookout = new RoleInfo("Lookout", Lookout.color, "Zoomout of the map and observe", "Zoomout of the map and observe", RoleId.Lookout, FactionId.Crewmate);
        public static RoleInfo plumber = new RoleInfo("Plumber", Plumber.color, "Maintain a clean vent system", "Maintain a clean vent system", RoleId.Plumber, FactionId.Crewmate);
        public static RoleInfo deceiver = new RoleInfo("Deceiver", Deceiver.color, "You can leace decoys to deceive others", "You can leace decoys to deceive others", RoleId.Deceiver, FactionId.Impostor);

        // Ghost Roles
        public static RoleInfo banshee = new RoleInfo("Banshee", Banshee.color, "Scare the living", "Scare the living", RoleId.Banshee, FactionId.Ghost);
        public static RoleInfo poltergeist = new RoleInfo("Poltergeist", Poltergeist.color, "Where them corpses at?", "Where them corpses at?", RoleId.Poltergeist, FactionId.Ghost);

        // Default Roles
        public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed, "Sabotage and kill everyone", "Sabotage and kill everyone", RoleId.Impostor, FactionId.Impostor);
        public static RoleInfo crewmate = new RoleInfo("Crewmate", Palette.CrewmateBlue, "Find the Impostors", "Find the Impostors", RoleId.Crewmate, FactionId.Crewmate);

        // Modifier
        public static RoleInfo bait = new RoleInfo("Bait", Bait.color, "Bait your enemies", "Bait your enemies", RoleId.Bait, FactionId.Modifier);
        public static RoleInfo blind = new RoleInfo("Blind", Blind.color, "Your report button doesn't lights up", "Your report button doesn't lights up", RoleId.Blind, FactionId.Modifier);
        public static RoleInfo buttonBarry = new RoleInfo("Button Barry", ButtonBarry.color, "Call emergency from anywhere", "Call emergency from anywherer", RoleId.ButtonBarry, FactionId.Modifier);
        public static RoleInfo shy = new RoleInfo("Shy", Shy.color, "You're hard to see when not moving", "You're hard to see when not moving", RoleId.Shy, FactionId.Modifier);
        public static RoleInfo flash = new RoleInfo("Flash", Flash.color, "Superspeed", "Superspeed", RoleId.Flash, FactionId.Modifier);
        public static RoleInfo mini = new RoleInfo("Mini", Mini.color, "No one will harm you until you grow up", "No one will harm you", RoleId.Mini, FactionId.Modifier);
        public static RoleInfo indomitable = new RoleInfo("Indomitable", Indomitable.color, "Your role can't be guessed", "Your role can't be guessed", RoleId.Indomitable, FactionId.Modifier);
        public static RoleInfo lover = new RoleInfo("Lover", Lovers.color, $"You are in love", $"You are in love", RoleId.Lover, FactionId.Modifier);
        public static RoleInfo multitasker = new RoleInfo("Multitasker", Multitasker.color, "Your tasks are transparent", "Your tasks are transparent", RoleId.Multitasker, FactionId.Modifier);
        public static RoleInfo radar = new RoleInfo("Radar", Radar.color, "Be on high alert", "Be on high alert", RoleId.Radar, FactionId.Modifier);
        public static RoleInfo sleuth = new RoleInfo("Sleuth", Sleuth.color, "Reporting a body reveals the victim's role", "Reporting a body reveals the victim's role", RoleId.Sleuth, FactionId.Modifier);
        public static RoleInfo tiebreaker = new RoleInfo("Tiebreaker", Tiebreaker.color, "Your vote breaks the tie", "Break the tie", RoleId.Tiebreaker, FactionId.Modifier);
        public static RoleInfo torch = new RoleInfo("Torch", Torch.color, "You have increased vision, unaffected by lights-out", "You have increased vision, unaffected by lights-out", RoleId.Torch, FactionId.Modifier);
        public static RoleInfo vip = new RoleInfo("VIP", Vip.color, "You are the VIP", "Everyone is notified when you die", RoleId.Vip, FactionId.Modifier);
        public static RoleInfo drunk = new RoleInfo("Drunk", Drunk.color, "Your movement is inverted", "Your movement is inverted", RoleId.Drunk, FactionId.Modifier);
        public static RoleInfo immovable = new RoleInfo("Immovable", Immovable.color, "You will not get teleported", "You will not get teleported", RoleId.Immovable, FactionId.Modifier);
        public static RoleInfo doubleShot = new RoleInfo("Double Shot", DoubleShot.color, "You have extra life while guessing", "You have extra life while guessing", RoleId.DoubleShot, FactionId.Modifier);
        public static RoleInfo ruthless = new RoleInfo("Ruthless", Ruthless.color, "Play without rules", "Play without rules", RoleId.Ruthless, FactionId.Modifier);
        public static RoleInfo underdog = new RoleInfo("Underdog", Underdog.color, "When you're alone, your kill cooldown is shortened", "When you're alone, your kill cooldown is shortened", RoleId.Underdog, FactionId.Modifier);
        public static RoleInfo saboteur = new RoleInfo("Saboteur", Saboteur.color, "You have reduced sabotage cooldowns", "You have reduced sabotage cooldowns", RoleId.Saboteur, FactionId.Modifier);
        public static RoleInfo frosty = new RoleInfo("Frosty", Frosty.color, "Leave behind an icy surprise", "Leave behind an icy surprise", RoleId.Frosty, FactionId.Modifier);
        public static RoleInfo satelite = new RoleInfo("Satelite", Satelite.color, "Detect dead bodies", "Detect dead bodies", RoleId.Satelite, FactionId.Modifier);
        public static RoleInfo sixthSense = new RoleInfo("Sixth Sense", SixthSense.color, "Know when someone interacts with you", "Know when someone interacts with you", RoleId.SixthSense, FactionId.Modifier);
        public static RoleInfo taskMaster = new RoleInfo("Taskmaster", Taskmaster.color, "Get tasks done fast", "Get tasks done fast", RoleId.Taskmaster, FactionId.Modifier);
        public static RoleInfo disperser = new RoleInfo("Disperser", Disperser.color, "Separate the Crewmates", "Separate the Crewmates", RoleId.Disperser, FactionId.Modifier);
        public static RoleInfo poucher = new RoleInfo("Poucher", Poucher.color, "Keep info on the players you kill", "Keep info on the players you kill", RoleId.Poucher, FactionId.Modifier);

        public static List<RoleInfo> allRoleInfos = new()
        {
            crewmate,
            detective,
            engineer,
            investigator,
            lookout,
            mayor,
            medic,
            mystic,
            oracle,
            politician,
            plumber,
            seer,
            sheriff,
            snitch,
            spy,
            swapper,
            tracker,
            trapper,
            vampireHunter,
            veteran,
            vigilante,

            amnesiac,
            guardianAngel,
            lawyer,
            pursuer,
            survivor,
            thief,

            cannibal,
            doomsayer,
            executioner,
            jester,

            arsonist,
            dracula,
            glitch,
            juggernaut,
            pestilence,
            plaguebearer,
            vampire,
            werewolf,

            impostor,
            assassin,
            blackmailer,
            camouflager,
            deceiver,
            escapist,
            grenadier,
            janitor,
            miner,
            morphling,
            poisoner,
            scavenger,
            swooper,
            venerer,
            undertaker,

            bait,
            blind,
            frosty,
            indomitable,
            multitasker,
            taskMaster,
            torch,
            vip,

            buttonBarry,
            drunk,
            flash,
            immovable,
            lover,
            mini,
            radar,
            satelite,
            shy,
            sixthSense,
            sleuth,
            tiebreaker,

            disperser,
            doubleShot,
            poucher,
            ruthless,
            saboteur,
            underdog,

            banshee,
            poltergeist,
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true)
        {
            List<RoleInfo> infos = new List<RoleInfo>();
            if (p == null) return infos;

            if (showModifier)
            {
                if (Bait.bait.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bait);
                if (p == Blind.blind) infos.Add(blind);
                if (p == ButtonBarry.buttonBarry) infos.Add(buttonBarry);
                if (Shy.shy.Any(x => x.PlayerId == p.PlayerId)) infos.Add(shy);
                if (Flash.flash.Any(x => x.PlayerId == p.PlayerId)) infos.Add(flash);
                if (p == Mini.mini) infos.Add(mini);
                if (p == Indomitable.indomitable) infos.Add(indomitable);
                if (p == Lovers.lover1 || p == Lovers.lover2) infos.Add(lover);
                if (Multitasker.multitasker.Any(x => x.PlayerId == p.PlayerId)) infos.Add(multitasker);
                if (p == Radar.radar) infos.Add(radar);
                if (p == Sleuth.sleuth) infos.Add(sleuth);
                if (p == Tiebreaker.tiebreaker) infos.Add(tiebreaker);
                if (Torch.torch.Any(x => x.PlayerId == p.PlayerId)) infos.Add(torch);
                if (Vip.vip.Any(x => x.PlayerId == p.PlayerId)) infos.Add(vip);
                if (Drunk.drunk.Any(x => x.PlayerId == p.PlayerId)) infos.Add(drunk);
                if (Immovable.immovable.Any(x => x.PlayerId == p.PlayerId)) infos.Add(immovable);
                if (p == DoubleShot.doubleShot) infos.Add(doubleShot);
                if (p == Ruthless.ruthless) infos.Add(ruthless);
                if (p == Underdog.underdog) infos.Add(underdog);
                if (p == Saboteur.saboteur) infos.Add(saboteur);
                if (p == Frosty.frosty) infos.Add(frosty);
                if (p == Satelite.satelite) infos.Add(satelite);
                if (p == SixthSense.sixthSense) infos.Add(sixthSense);
                if (p == Taskmaster.taskMaster) infos.Add(taskMaster);
                if (p == Disperser.disperser) infos.Add(disperser);
                if (p == Poucher.poucher) infos.Add(poucher);
            }
            int count = infos.Count;

            // Special roles
            if (p.IsSheriff(out _)) infos.Add(sheriff);
            if (p.IsJester(out _)) infos.Add(jester);
            if (p.IsSurvivor(out _)) infos.Add(survivor);
            if (p == Juggernaut.juggernaut) infos.Add(juggernaut);
            if (p == Seer.seer) infos.Add(seer);
            if (p == Engineer.engineer) infos.Add(engineer);
            if (p == Snitch.snitch) infos.Add(snitch);
            if (p.IsVeteran(out _)) infos.Add(veteran);
            if (p == Camouflager.camouflager) infos.Add(camouflager);
            if (p == Morphling.morphling) infos.Add(morphling);
            if (p.IsPursuer(out _)) infos.Add(pursuer);
            if (p.IsAmnesiac(out _)) infos.Add(amnesiac);
            if (p.IsThief(out _)) infos.Add(thief);
            if (p == Lawyer.lawyer) infos.Add(lawyer);
            if (p == Executioner.executioner) infos.Add(executioner);
            if (p == Medic.medic) infos.Add(medic);
            if (p == Swapper.swapper) infos.Add(swapper);
            if (p == Investigator.investigator) infos.Add(investigator);
            if (p == Spy.spy) infos.Add(spy);
            if (p == Vigilante.vigilante) infos.Add(vigilante);
            if (p == Assassin.assassin) infos.Add(assassin);
            if (p == Tracker.tracker) infos.Add(tracker);
            if (p == Trapper.trapper) infos.Add(trapper);
            if (p == Detective.detective) infos.Add(detective);
            if (p == Mystic.mystic) infos.Add(mystic);
            if (p == GuardianAngel.guardianAngel) infos.Add(guardianAngel);
            if (p == Swooper.swooper) infos.Add(swooper);
            if (p == Arsonist.arsonist) infos.Add(arsonist);
            if (p == Werewolf.werewolf) infos.Add(werewolf);
            if (p == Miner.miner) infos.Add(miner);
            if (p == Janitor.janitor) infos.Add(janitor);
            if (p == Undertaker.undertaker) infos.Add(undertaker);
            if (p == Grenadier.grenadier) infos.Add(grenadier);
            if (p == Blackmailer.blackmailer) infos.Add(blackmailer);
            if (p == Politician.politician) infos.Add(politician);
            if (p == Mayor.mayor) infos.Add(mayor);
            if (p == Dracula.dracula || Dracula.formerDraculas != null && Dracula.formerDraculas.Any(x => x.PlayerId == p.PlayerId)) infos.Add(dracula);
            if (p == Vampire.vampire) infos.Add(vampire);
            if (p == Poisoner.poisoner) infos.Add(poisoner);
            if (p == Venerer.venerer) infos.Add(venerer);
            if (p == Plaguebearer.plaguebearer) infos.Add(plaguebearer);
            if (p == Pestilence.pestilence) infos.Add(pestilence);
            if (p == Doomsayer.doomsayer) infos.Add(doomsayer);
            if (p == Glitch.glitch) infos.Add(glitch);
            if (p == Cannibal.cannibal) infos.Add(cannibal);
            if (p == Scavenger.scavenger) infos.Add(scavenger);
            if (p == Escapist.escapist) infos.Add(escapist);
            if (p == VampireHunter.vampireHunter) infos.Add(vampireHunter);
            if (p == Oracle.oracle) infos.Add(oracle);
            if (p == Lookout.lookout) infos.Add(lookout);
            if (p == Plumber.plumber) infos.Add(plumber);
            if (p == Deceiver.deceiver) infos.Add(deceiver);

            // Ghost Roles
            if (p == Banshee.banshee) infos.Add(banshee);
            if (p == Poltergeist.poltergeist) infos.Add(poltergeist);

            // Default roles
            if (infos.Count == count) infos.Add(p.Data.Role.IsImpostor ? impostor : crewmate);

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true, bool suppressGhostInfo = false)
        {
            string roleName;
            roleName = String.Join(" ", getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
            if (Lawyer.target != null && p.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target) 
                roleName += useColors ? Helpers.cs(Lawyer.color, " §") : " §";
            if (Executioner.target != null && p.PlayerId == Executioner.target.PlayerId && PlayerControl.LocalPlayer != Executioner.target)
                roleName += useColors ? Helpers.cs(Executioner.color, " X") : " X";
            if (GuardianAngel.target != null && p.PlayerId == GuardianAngel.target.PlayerId && PlayerControl.LocalPlayer != GuardianAngel.target)
                roleName += useColors ? Helpers.cs(GuardianAngel.color, " ★") : " ★";

            if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId))
            {
                int remainingShots = HandleGuesser.remainingShots(p.PlayerId);
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(p.Data);
                if (!Helpers.isEvil(p) && playerCompleted < HandleGuesser.tasksToUnlock || remainingShots == 0)
                    roleName += Helpers.cs(Color.gray, " (Gs)");
                else
                    roleName += Helpers.cs(Color.white, " (Gs)");
            }

            if (!suppressGhostInfo && p != null)
            {
                if (p == Cannibal.cannibal && (PlayerControl.LocalPlayer == Cannibal.cannibal || Helpers.shouldShowGhostInfo())) roleName = roleName + Helpers.cs(Cannibal.color, $" ({Cannibal.cannibalNumberToWin - Cannibal.eatenBodies} left)");
                if (Helpers.shouldShowGhostInfo())
                {
                    if (p.IsVeteran(out Veteran veteran) && veteran.alertActive) roleName = Helpers.cs(Veteran.color, "! ") + roleName;
                    if (Pursuer.blankedList.Contains(p)) roleName = Helpers.cs(Pursuer.color, "(blanked) ") + roleName;
                    if (Arsonist.dousedPlayers.Contains(p)) roleName = Helpers.cs(Arsonist.color, "♨ ") + roleName;
                    if (Werewolf.werewolf == p && Werewolf.isRampageActive) roleName = Helpers.cs(Werewolf.color, "! ") + roleName;
                    if (Blackmailer.blackmailed == p && !MeetingHud.Instance) roleName = Helpers.cs(Blackmailer.color, "(blackmailed) ") + roleName;
                    if (Mayor.mayor != null && Mayor.mayor == p && Mayor.isBodyguardActive) roleName = Helpers.cs(Mayor.color, "! ") + roleName;
                    if (p == Dracula.fakeVampire) roleName = Helpers.cs(Dracula.color, "(fake Vamp) ") + roleName;
                    if (Poisoner.poisoner != null && !Poisoner.poisoner.Data.IsDead && Poisoner.poisoned == p && !p.Data.IsDead) roleName = Helpers.cs(Poisoner.color, $"(bitten {(int)poisonButton.Timer + 1}) ") + roleName;
                    if (Glitch.hackedPlayer != null && !Glitch.hackedPlayer.Data.IsDead && Glitch.hackedPlayer == p) roleName = Helpers.cs(Glitch.color, "(hacked) ") + roleName;
                    if (Scavenger.bounty == p && !MeetingHud.Instance) roleName = Helpers.cs(Scavenger.color, "(bounty) ") + roleName;

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
                                case DeadPlayer.CustomDeathReason.LawyerSuicide:
                                    deathReasonString = $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Guess:
                                    if (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName)
                                        deathReasonString = $" - failed guess";
                                    else
                                        deathReasonString = $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.Arson:
                                    deathReasonString = $" - burnt by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                                    break;
                                case DeadPlayer.CustomDeathReason.LoverSuicide:
                                    deathReasonString = $" - {Helpers.cs(Lovers.color, "lover died")}";
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