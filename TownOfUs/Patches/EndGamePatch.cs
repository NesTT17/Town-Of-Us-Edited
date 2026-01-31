using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TownOfUs.Patches
{
    enum CustomGameOverReason
    {
        ImpostorWin = 10,
        CrewmateWin = 11,
        OxygenWin = 12,
        ReactorWin = 13,
        TaskWin = 14,
        JesterWin = 15,
        JuggernautWin = 16,
        ExecutionerWin = 17,
        ArsonistWin = 18,
        WerewolfWin = 19,
        TeamVampiresWin = 20,
        DoomsayerWin = 21,
        PlaguebearerWin = 22,
        PestilenceWin = 23,
        GlitchWin = 24,
        MiniLose = 25,
        LoversWin = 26,
        CannibalWin = 27,
    }

    enum WinCondition
    {
        Default,
        ImpostorWin,
        CrewmateWin,
        OxygenWin,
        ReactorWin,
        TaskWin,
        JesterWin,
        JuggernautWin,
        ExecutionerWin,
        ArsonistWin,
        WerewolfWin,
        DraculaWin,
        DoomsayerWin,
        PlaguebearerWin,
        PestilenceWin,
        GlitchWin,
        MiniLose,
        LoversTeamWin,
        LoversSoloWin,
        CannibalWin,
        AdditionalAliveSurvivorWin,
        AdditionalAlivePursuerWin,
        AdditionalLawyerBonusWin,
        AdditionalGuardianAngelBonusWin,
    }

    static class AdditionalTempData
    {
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static List<Winners> otherWinners = new List<Winners>();
        public static float timer = 0;

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            otherWinners.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo
        {
            private string mDeathReason = "";
            public string PlayerName { get; set; }
            public bool IsFormerThief { get; set; }
            public bool IsLawyerClient { get; set; }
            public bool IsExeTarget { get; set; }
            public bool IsGATarget { get; set; }
            public string DeathReason
            {
                get => mDeathReason;
                set
                {
                    if (!mDeathReason.Equals("")) return;

                    mDeathReason = value;
                }
            }
            public List<RoleInfo> Roles { get; set; }
            public List<RoleInfo> GhostRoles { get; set; }
            public List<RoleInfo> Modifiers { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public bool IsGuesser { get; set; }
            public int Kills { get; set; }
            public int CorrectGuesses { get; set; }
            public int IncorrectGuesses { get; set; }
            public int CorrectShots { get; set; }
            public int Misfires { get; set; }
            public int AbilityKills { get; set; }
            public int Eats { get; set; }
            public int Cleans { get; set; }
            public bool IsAlive { get; set; }
        }

        internal class Winners
        {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles { get; set; }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch
    {
        public static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;

            // Reset zoomed out ghosts
            Helpers.toggleZoom(reset: true);
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            AdditionalTempData.clear();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                var roles = PlayerGameInfo.GetRoles(player);
                var ghostRoles = PlayerGameInfo.GetGhostRoles(player);
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(player.Data);
                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = player.Data.DefaultOutfit.PlayerName,
                    IsFormerThief = Thief.formerThieves.Contains(player),
                    IsLawyerClient = Lawyer.target != null && Lawyer.target.PlayerId == player.PlayerId,
                    IsExeTarget = Executioner.target != null && Executioner.target.PlayerId == player.PlayerId,
                    IsGATarget = GuardianAngel.target != null && GuardianAngel.target.PlayerId == player.PlayerId,
                    DeathReason = player.getDeathReasonString(),
                    Roles = roles,
                    GhostRoles = ghostRoles,
                    Modifiers = PlayerGameInfo.GetModifiers(player.PlayerId),
                    TasksCompleted = tasksCompleted,
                    TasksTotal = tasksTotal,
                    IsGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(player.PlayerId),
                    Kills = PlayerGameInfo.TotalKills(player.PlayerId),
                    CorrectGuesses = PlayerGameInfo.TotalCorrectGuesses(player.PlayerId),
                    IncorrectGuesses = PlayerGameInfo.TotalIncorrectGuesses(player.PlayerId),
                    CorrectShots = PlayerGameInfo.TotalCorrectShots(player.PlayerId),
                    Misfires = PlayerGameInfo.TotalMisfires(player.PlayerId),
                    AbilityKills = PlayerGameInfo.TotalAbilityKills(player.PlayerId),
                    Eats = PlayerGameInfo.TotalEaten(player.PlayerId),
                    Cleans = PlayerGameInfo.TotalCleaned(player.PlayerId),
                    IsAlive = !player.Data.IsDead
                });
            }

            // Remove neutrals from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();
            if (Juggernaut.juggernaut != null) notWinners.Add(Juggernaut.juggernaut);
            if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
            if (Executioner.executioner != null) notWinners.Add(Executioner.executioner);
            if (GuardianAngel.guardianAngel != null) notWinners.Add(GuardianAngel.guardianAngel);
            if (Arsonist.arsonist != null) notWinners.Add(Arsonist.arsonist);
            if (Werewolf.werewolf != null) notWinners.Add(Werewolf.werewolf);
            if (Dracula.dracula != null) notWinners.Add(Dracula.dracula);
            if (Vampire.vampire != null) notWinners.Add(Vampire.vampire);
            if (Doomsayer.doomsayer != null) notWinners.Add(Doomsayer.doomsayer);
            if (Plaguebearer.plaguebearer != null) notWinners.Add(Plaguebearer.plaguebearer);
            if (Pestilence.pestilence != null) notWinners.Add(Pestilence.pestilence);
            if (Glitch.glitch != null) notWinners.Add(Glitch.glitch);
            if (Cannibal.cannibal != null) notWinners.Add(Cannibal.cannibal);
            notWinners.AddRange(PlayerControl.AllPlayerControls.GetFastEnumerator().Where(player => player.IsJester(out _) || player.IsSurvivor(out _) || player.IsPursuer(out _) || player.IsAmnesiac(out _) || player.IsThief(out _)));
            notWinners.AddRange(Dracula.formerDraculas);

            List<CachedPlayerData> winnersToRemove = new List<CachedPlayerData>();
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

            bool impostorWin = gameOverReason == (GameOverReason)CustomGameOverReason.ImpostorWin;
            bool crewmateWins = gameOverReason is (GameOverReason)CustomGameOverReason.CrewmateWin or (GameOverReason)CustomGameOverReason.TaskWin;
            bool oxygenWin = gameOverReason == (GameOverReason)CustomGameOverReason.OxygenWin;
            bool reactorWin = gameOverReason == (GameOverReason)CustomGameOverReason.ReactorWin;
            bool jesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool juggernautWin = Juggernaut.juggernaut != null && gameOverReason == (GameOverReason)CustomGameOverReason.JuggernautWin;
            bool executionerWin = Executioner.executioner != null && gameOverReason == (GameOverReason)CustomGameOverReason.ExecutionerWin;
            bool arsonistWin = Arsonist.arsonist != null && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool werewolfWin = Werewolf.werewolf != null && gameOverReason == (GameOverReason)CustomGameOverReason.WerewolfWin;
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamVampiresWin && ((Dracula.dracula != null && !Dracula.dracula.Data.IsDead) || (Vampire.vampire != null && !Vampire.vampire.Data.IsDead));
            bool doomsayerWin = Doomsayer.doomsayer != null && gameOverReason == (GameOverReason)CustomGameOverReason.DoomsayerWin;
            bool plaguebearerWin = Plaguebearer.plaguebearer != null && gameOverReason == (GameOverReason)CustomGameOverReason.PlaguebearerWin;
            bool pestilenceWin = Pestilence.pestilence != null && gameOverReason == (GameOverReason)CustomGameOverReason.PestilenceWin;
            bool glitchWin = Glitch.glitch != null && gameOverReason == (GameOverReason)CustomGameOverReason.GlitchWin;
            bool miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            bool loversWin = Lovers.existingAndAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin || (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.existingWithKiller())); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            bool cannibalWin = Cannibal.cannibal != null && gameOverReason == (GameOverReason)CustomGameOverReason.CannibalWin;

            // Impostor win
            if (impostorWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    if (player.Data.Role.IsImpostor)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
                AdditionalTempData.winCondition = WinCondition.ImpostorWin;
            }
            // Oxygen win
            else if (oxygenWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    if (player.Data.Role.IsImpostor)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
                AdditionalTempData.winCondition = WinCondition.OxygenWin;
            }
            // Reactor win
            else if (reactorWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    if (player.Data.Role.IsImpostor)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
                AdditionalTempData.winCondition = WinCondition.ReactorWin;
            }
            // Crewmates win
            else if (crewmateWins)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    if (!player.isEvil())
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
                if (gameOverReason == (GameOverReason)CustomGameOverReason.TaskWin)
                {
                    AdditionalTempData.winCondition = WinCondition.TaskWin;
                }
                else
                {
                    AdditionalTempData.winCondition = WinCondition.CrewmateWin;
                }
            }
            // Jester win
            else if (jesterWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Jester.winningJesterPlayer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
            // Juggernaut Win
            else if (juggernautWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Juggernaut.juggernaut.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JuggernautWin;
            }
            // Executioner win
            else if (executionerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Executioner.executioner.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ExecutionerWin;
            }
            // Arsonist Win
            else if (arsonistWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Arsonist.arsonist.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }
            // Werewolf Win
            else if (werewolfWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Werewolf.werewolf.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.WerewolfWin;
            }
            // Dracula win condition
            else if (teamJackalWin)
            {
                // Dracula wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.DraculaWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Dracula.dracula.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                // If there is a vampire. The vampire also wins
                if (Vampire.vampire != null)
                {
                    CachedPlayerData wpdVampire = new CachedPlayerData(Vampire.vampire.Data);
                    wpdVampire.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdVampire);
                }
                foreach (var player in Dracula.formerDraculas)
                {
                    CachedPlayerData wpdFormerDracula = new CachedPlayerData(player.Data);
                    wpdFormerDracula.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdFormerDracula);
                }
            }
            // Doomsayer win
            else if (doomsayerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Doomsayer.doomsayer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
            }
            // Plaguebearer Win
            else if (plaguebearerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Plaguebearer.plaguebearer.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.PlaguebearerWin;
            }
            // Pestilence Win
            else if (pestilenceWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Pestilence.pestilence.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.PestilenceWin;
            }
            // Glitch Win
            else if (glitchWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Glitch.glitch.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.GlitchWin;
            }
            // Mini lose
            else if (miniLose)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Mini.mini.Data);
                wpd.IsYou = false; // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;
            }
            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (!Lovers.existingWithKiller())
                {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (!p.isEvil())
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover1.Data));
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover2.Data));
                }
            }
            // Cannibal win
            else if (cannibalWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Cannibal.cannibal.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.CannibalWin;
            }

            // Possible Additional winner: Survivor
            if (!miniLose && Survivor.survivors.Values.Count > 0)
            {
                foreach (Survivor survivor in Survivor.survivors.Values)
                {
                    if (survivor.survivor != null && !survivor.survivor.Data.IsDead)
                    {
                        if (!EndGameResult.CachedWinners.ToArray().Any(winner => winner.PlayerName == survivor.survivor.Data.PlayerName))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(survivor.survivor.Data));
                        if (!AdditionalTempData.additionalWinConditions.Contains(WinCondition.AdditionalAliveSurvivorWin))
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAliveSurvivorWin);
                    }
                }
            }

            // Possible Additional winner: Pursuer
            if (!miniLose && Pursuer.pursuers.Values.Count > 0)
            {
                foreach (Pursuer pursuer in Pursuer.pursuers.Values)
                {
                    if (pursuer.pursuer != null && !pursuer.pursuer.Data.IsDead)
                    {
                        if (!EndGameResult.CachedWinners.ToArray().Any(winner => winner.PlayerName == pursuer.pursuer.Data.PlayerName))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(pursuer.pursuer.Data));
                        if (!AdditionalTempData.additionalWinConditions.Contains(WinCondition.AdditionalAlivePursuerWin))
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
                    }
                }
            }

            // Possible Additional winner: Lawyer
            if (!miniLose && Lawyer.lawyer != null && Lawyer.target != null && (!Lawyer.target.Data.IsDead || Lawyer.target.IsJester(out _)))
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
                {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null)
                {
                    // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins together with the client
                }
            }

            // Possible Additional winner: Guardian Angel
            if (!miniLose && GuardianAngel.guardianAngel != null && GuardianAngel.target != null)
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
                {
                    if (winner.PlayerName == GuardianAngel.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null)
                {
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == GuardianAngel.guardianAngel.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(GuardianAngel.guardianAngel.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalGuardianAngelBonusWin);
                }
            }

            // Possible Additional winner: Jester
            if (Jester.jesters.Values.Count > 0 && !CustomOptionHolder.jesterWinEndsGame.getBool())
            {
                foreach (Jester jester in Jester.jesters.Values)
                {
                    if (jester.jester != null && jester.votedOut)
                    {
                        AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners
                        {
                            PlayerName = jester.jester.Data.PlayerName,
                            Roles = PlayerGameInfo.GetRoles(jester.jester.Data)
                        });
                    }
                }
            }

            // Possible Additional winner: Executioner
            if (Executioner.executioner != null && !CustomOptionHolder.executionerWinEndsGame.getBool())
            {
                if (Executioner.triggerExile)
                {
                    AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners
                    {
                        PlayerName = Executioner.executioner.Data.PlayerName,
                        Roles = PlayerGameInfo.GetRoles(Executioner.executioner.Data)
                    });
                }
            }

            // Possible Additional winner: Doomsayer
            if (Doomsayer.doomsayer != null && !CustomOptionHolder.doomsayerWinEndsGame.getBool())
            {
                if (Doomsayer.triggerGuessed)
                {
                    AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners
                    {
                        PlayerName = Doomsayer.doomsayer.Data.PlayerName,
                        Roles = PlayerGameInfo.GetRoles(Doomsayer.doomsayer.Data)
                    });
                }
            }
            
            // Possible Addition winner: Cannibal
            if (Cannibal.cannibal != null && !CustomOptionHolder.cannibalWinEndsGame.getBool())
            {
                if (Cannibal.triggerEaten)
                {
                    AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners
                    {
                        PlayerName = Cannibal.cannibal.Data.PlayerName,
                        Roles = PlayerGameInfo.GetRoles(Cannibal.cannibal.Data)
                    });
                }
            }

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - startTime).TotalMilliseconds) / 1000;

            // Reset Settings
            RPCProcedure.resetVariables();
        }

        public static string getDeathReasonString(this PlayerControl p)
        {
            if (!p.Data.IsDead) return "";

            DeadPlayer deadPlayer = GameHistory.deadPlayers.Find(x => x.player.PlayerId == p.PlayerId);
            if (deadPlayer == null) return "";

            Color killerColor = default;
            string killerName = "";
            if (deadPlayer.killerIfExisting)
            {
                RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(deadPlayer.killerIfExisting, false).FirstOrDefault();
                killerColor = roleInfo?.color ?? default(Color);
                killerName = deadPlayer.killerIfExisting.Data.PlayerName;

            }

            return deadPlayer.deathReason switch
            {
                DeadPlayer.CustomDeathReason.Disconnect => $" - disconnected",
                DeadPlayer.CustomDeathReason.Exile => $" - voted out",
                DeadPlayer.CustomDeathReason.Kill => $" - killed by {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.LawyerSuicide => $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}",
                DeadPlayer.CustomDeathReason.Guess => (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName) ? $" - failed guess" : $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}",
                DeadPlayer.CustomDeathReason.Arson => $" - burnt by {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.LoverSuicide => $" - {Helpers.cs(Lovers.color, "lover died")}",
                _ => "",
            };
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate (CachedPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList<CachedPlayerData>();
            for (int i = 0; i < list.Count; i++)
            {
                CachedPlayerData CachedPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                if (CachedPlayerData2.IsDead)
                {
                    poolablePlayer.SetBodyAsGhost();
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }
                poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

                AdditionalTempData.PlayerRoleInfo roleInfo = AdditionalTempData.playerRoles.FirstOrDefault(data => data.PlayerName == CachedPlayerData2.PlayerName);

                if (roleInfo != null && roleInfo.Roles.Count > 0)
                    poolablePlayer.cosmetics.nameText.text += $"\n{roleInfo.Roles.Select(x => Helpers.cs(x.color, x.name)).Last()}";
            }

            // Additional code
            __instance.WinText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y + 4f, __instance.WinText.transform.position.z);
            __instance.WinText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 4f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>(); 
            textRenderer.text = "";

            if (AdditionalTempData.winCondition == WinCondition.CrewmateWin)
            {
                textRenderer.text = "Crewmates Win";
                textRenderer.color = Palette.CrewmateBlue;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
            }
            else if (AdditionalTempData.winCondition == WinCondition.TaskWin)
            {
                textRenderer.text = "Crewmates Task Win";
                textRenderer.color = Palette.CrewmateBlue;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ImpostorWin)
            {
                textRenderer.text = "Impostors Win";
                textRenderer.color = Palette.ImpostorRed;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.ImpostorRed);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ReactorWin)
            {
                textRenderer.text = "Crewmates Were Consumed By Radiation";
                textRenderer.color = Palette.ImpostorRed;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.ImpostorRed);
            }
            else if (AdditionalTempData.winCondition == WinCondition.OxygenWin)
            {
                textRenderer.text = "Crewmates Forgot How To Breathe";
                textRenderer.color = Palette.ImpostorRed;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.ImpostorRed);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JesterWin)
            {
                textRenderer.text = "Jester Wins";
                textRenderer.color = Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JuggernautWin)
            {
                textRenderer.text = "Juggernaut Wins";
                textRenderer.color = Juggernaut.color;
                __instance.BackgroundBar.material.SetColor("_Color", Juggernaut.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ExecutionerWin)
            {
                textRenderer.text = "Executioner Wins";
                textRenderer.color = Executioner.color;
                __instance.BackgroundBar.material.SetColor("_Color", Executioner.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
            {
                textRenderer.text = "Arsonist Wins";
                textRenderer.color = Arsonist.color;
                __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.WerewolfWin)
            {
                textRenderer.text = "Werewolf Wins";
                textRenderer.color = Werewolf.color;
                __instance.BackgroundBar.material.SetColor("_Color", Werewolf.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.DraculaWin)
            {
                textRenderer.text = "Team Vampires Wins";
                textRenderer.color = Dracula.color;
                __instance.BackgroundBar.material.SetColor("_Color", Dracula.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.DoomsayerWin)
            {
                textRenderer.text = "Doomsayer Wins";
                textRenderer.color = Doomsayer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Doomsayer.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.PlaguebearerWin)
            {
                textRenderer.text = "Plaguebearer Wins";
                textRenderer.color = Plaguebearer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Plaguebearer.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.PestilenceWin)
            {
                textRenderer.text = "Pestilence Wins";
                textRenderer.color = Pestilence.color;
                __instance.BackgroundBar.material.SetColor("_Color", Pestilence.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.GlitchWin)
            {
                textRenderer.text = "Glitch Wins";
                textRenderer.color = Glitch.color;
                __instance.BackgroundBar.material.SetColor("_Color", Glitch.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.MiniLose)
            {
                textRenderer.text = "Mini died";
                textRenderer.color = Mini.color;
                __instance.BackgroundBar.material.SetColor("_Color", Mini.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin)
            {
                textRenderer.text = "Lovers And Crewmates Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin)
            {
                textRenderer.text = "Lovers Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.CannibalWin)
            {
                textRenderer.text = "Cannibal Win";
                textRenderer.color = Cannibal.color;
                __instance.BackgroundBar.material.SetColor("_Color", Cannibal.color);
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions)
            {
                switch (cond)
                {
                    case WinCondition.AdditionalAliveSurvivorWin:
                        textRenderer.text += $"\n{Helpers.cs(Survivor.color, "The Survivor stayed alive")}";
                        break;
                    case WinCondition.AdditionalAlivePursuerWin:
                        textRenderer.text += $"\n{Helpers.cs(Pursuer.color, "The Pursuer survived")}";
                        break;
                    case WinCondition.AdditionalLawyerBonusWin:
                        textRenderer.text += $"\n{Helpers.cs(Lawyer.color, "The Lawyer wins with the client")}";
                        break;
                    case WinCondition.AdditionalGuardianAngelBonusWin:
                        textRenderer.text += $"\n{Helpers.cs(GuardianAngel.color, "The Guardian Angel wins with the target")}";
                        break;
                }
            }

            if (showRoleSummary)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                var winnersText = new StringBuilder();
                var winnersCache = new StringBuilder();
                var losersText = new StringBuilder();
                var winnerCount = 0;
                var loserCount = 0;
                int minutes = (int)AdditionalTempData.timer / 60;
                int seconds = (int)AdditionalTempData.timer % 60;
                roleSummaryText.AppendLine($"<size=125%><u><b><color=#FAD934FF>Match Duration: {minutes:00}:{seconds:00}</color></b></u></size> \n");
                roleSummaryText.AppendLine("<size=125%><u><b>Game Stats:</b></u></size>");
                roleSummaryText.AppendLine();
                winnersText.AppendLine("<size=105%><color=#00FF00FF><b>★ - Winners List - ★</b></color></size>");
                losersText.AppendLine("<size=105%><color=#FF0000FF><b>◆ - Losers List - ◆</b></color></size>");

                foreach (AdditionalTempData.PlayerRoleInfo data in AdditionalTempData.playerRoles)
                {
                    var summaryText = new List<string>();

                    string name = Helpers.cs(data.IsAlive ? Color.white : new Color(.7f, .7f, .7f), data.PlayerName);
                    string formerThief = data.IsFormerThief ? Helpers.cs(Thief.color, " $") : "";
                    string lawyerClient = data.IsLawyerClient ? Helpers.cs(Lawyer.color, " §") : "";
                    string exeTarget = data.IsExeTarget ? Helpers.cs(Executioner.color, " X") : "";
                    string gaTarget = data.IsGATarget ? Helpers.cs(GuardianAngel.color, " ★") : "";
                    string deathReason = data.DeathReason.IsNullOrWhiteSpace() ? "" : data.DeathReason;
                    string guesserInfo = data.IsGuesser ? " (Guesser)" : "";

                    string roles = data.Roles.Count > 0 ? string.Join(" -> ", data.Roles.Select(x => Helpers.cs(x.color, x.name))) : "";
                    string ghostRoles = data.GhostRoles.Count > 0 ? $" (Ghost: {string.Join(" -> ", data.GhostRoles.Select(x => Helpers.cs(x.color, x.name)))})" : "";
                    string modifiers = data.Modifiers.Count > 0 ? string.Join(" ", data.Modifiers.Select(x => Helpers.cs(x.color, $"({x.name}) "))) : "";
                    string taskInfo = data.TasksTotal > 0 ? (data.TasksCompleted == data.TasksTotal ? $" - Tasks: {Helpers.cs(Color.green, $"{data.TasksCompleted}/{data.TasksTotal}")}" : $" - Tasks: {Helpers.cs(Color.red, $"{data.TasksCompleted}/{data.TasksTotal}")}") : "";
                    string abilityKillsInfo = data.AbilityKills > 0 ? $" ({data.AbilityKills} Ability)" : "";
                    string killInfo = data.Kills > 0 ? $" - <color=#FF0000FF>Kills: {data.Kills}{abilityKillsInfo}</color>" : "";
                    string misfireInfo = data.Misfires > 0 ? $" - {Helpers.cs(Palette.ImpostorRed, $"Misfires: {data.Misfires}")}" : "";
                    string correctShotsInfo = data.CorrectShots > 0 ? $" - {Helpers.cs(Color.green, $"Correct Shots: {data.CorrectShots}")}" : "";
                    string correctGuessInfo = data.CorrectGuesses > 0 ? $" - {Helpers.cs(Color.green, $"Correct Guesses: {data.CorrectGuesses}")}" : "";
                    string incorrectGuessInfo = data.IncorrectGuesses > 0 ? $" - {Helpers.cs(Palette.ImpostorRed, $"Incorrect Guesses: {data.IncorrectGuesses}")}" : "";
                    string scavengerEaten = data.Eats > 0 ? $" - {Helpers.cs(Color.white, $"Bodies Eaten: {data.Eats}")}" : "";
                    string bodiesClean = data.Cleans > 0 ? $" - {Helpers.cs(Palette.ImpostorRed, $"Cleaned Bodies: {data.Cleans}")}" : "";
                    summaryText.Add($"{name}{formerThief}{lawyerClient}{exeTarget}{gaTarget}{deathReason} | {modifiers}{roles}{ghostRoles}{guesserInfo}{taskInfo}{killInfo}{misfireInfo}{correctShotsInfo}{correctGuessInfo}{incorrectGuessInfo}{scavengerEaten}{bodiesClean}");

                    string dataString = $"<size=70%>{string.Join("", summaryText)}</size>";

                    if (data.PlayerName.IsWinner())
                    {
                        winnersText.AppendLine(dataString);
                        winnerCount++;
                    }
                    else
                    {
                        losersText.AppendLine(dataString);
                        loserCount++;
                    }
                }

                roleSummaryText.Append(winnersText);
                roleSummaryText.AppendLine();
                roleSummaryText.Append(losersText);

                if (AdditionalTempData.otherWinners.Count != 0)
                {
                    roleSummaryText.AppendLine("\n\n\nOther Winners:");
                    foreach (var data in AdditionalTempData.otherWinners)
                    {
                        string name = data.PlayerName;
                        string roles = data.Roles.Count > 0 ? string.Join(" -> ", data.Roles.Select(x => Helpers.cs(x.color, x.name))) : "";
                        roleSummaryText.AppendLine($"<size=70%>{name} | {roles}</size>");
                    }
                }

                TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.fontSizeMin = 1.5f;
                roleSummaryTextMesh.fontSizeMax = 1.5f;
                roleSummaryTextMesh.fontSize = 1.5f;

                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();

                Helpers.previousEndGameSummary = roleSummaryText.ToString();
            }
            AdditionalTempData.clear();
            AdditionalTempData.playerRoles.Clear();
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    class CheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForMiniLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForExecutionerWin(__instance)) return false;
            if (CheckAndEndGameForDoomsayerWin(__instance)) return false;
            if (CheckAndEndGameForCannibalWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForVampiresWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPlaguebearerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPestilenceWin(__instance, statistics)) return false;
            if (CheckAndEndGameForGlitchWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJuggernautWin(__instance, statistics)) return false;
            if (CheckAndEndGameForArsonistWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWerewolfWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
        {
            if (Mini.triggerMiniLose)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
        {
            if (Jester.triggerJesterWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForExecutionerWin(ShipStatus __instance)
        {
            if (Executioner.triggerExecutionerWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ExecutionerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForDoomsayerWin(ShipStatus __instance)
        {
            if (Doomsayer.triggerDoomsayerWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.DoomsayerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCannibalWin(ShipStatus __instance)
        {
            if (Cannibal.triggerCannibalWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.CannibalWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (MapUtilities.Systems == null) return false;
            var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForOxygenSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForReactorSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TaskWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForGlitchWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.GlitchAlive >= statistics.TotalAlive - statistics.GlitchAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.GlitchHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.GlitchWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForPestilenceWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.PestilenceAlive >= statistics.TotalAlive - statistics.PestilenceAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.GlitchAlive == 0 && !(statistics.PestilenceHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PestilenceWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForPlaguebearerWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.PlaguebearerAlive >= statistics.TotalAlive - statistics.PlaguebearerAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.PlaguebearerHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PlaguebearerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForVampiresWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamVampiresAlive >= statistics.TotalAlive - statistics.TeamVampiresAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.TeamVampiresHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamVampiresWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForWerewolfWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.WerewolfAlive >= statistics.TotalAlive - statistics.WerewolfAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.WerewolfHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.WerewolfWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.ArsonistAlive >= statistics.TotalAlive - statistics.ArsonistAlive && statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.ArsonistHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJuggernautWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.JuggernautAlive >= statistics.TotalAlive - statistics.JuggernautAlive && statistics.TeamImpostorsAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.JuggernautHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JuggernautWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0 && !(statistics.TeamImpostorsHasAliveLover && statistics.TeamLoversAlive == 2))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ImpostorWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive == 0 && statistics.JuggernautAlive == 0 && statistics.ArsonistAlive == 0 && statistics.WerewolfAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.GlitchAlive == 0 && statistics.PlaguebearerAlive == 0 && statistics.PestilenceAlive == 0)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.CrewmateWin, false);
                return true;
            }
            return false;
        }

        private static void EndGameForOxygenSabotage(ShipStatus __instance)
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.OxygenWin, false);
            return;
        }

        private static void EndGameForReactorSabotage(ShipStatus __instance)
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ReactorWin, false);
            return;
        }
    }

    internal class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int JuggernautAlive { get; set; }
        public int ArsonistAlive { get; set; }
        public int WerewolfAlive { get; set; }
        public int TeamVampiresAlive { get; set; }
        public int PlaguebearerAlive { get; set; }
        public int PestilenceAlive { get; set; }
        public int GlitchAlive { get; set; }
        public int TeamLoversAlive {get;set;}
        public int TotalAlive { get; set; }
        public bool TeamImpostorsHasAliveLover { get; set; }
        public bool JuggernautHasAliveLover { get; set; }
        public bool ArsonistHasAliveLover { get; set; }
        public bool WerewolfHasAliveLover { get; set; }
        public bool TeamVampiresHasAliveLover { get; set; }
        public bool PlaguebearerHasAliveLover { get; set; }
        public bool PestilenceHasAliveLover { get; set; }
        public bool GlitchHasAliveLover { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        private bool isLover(NetworkedPlayerInfo p)
        {
            return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId);
        }

        private void GetPlayerCounts()
        {
            int numImpostorsAlive = 0;
            int numJuggernautAlive = 0;
            int numArsonistAlive = 0;
            int numWerewolfAlive = 0;
            int numVampiresAlive = 0;
            int numPlaguebearerAlive = 0;
            int numPestilenceAlive = 0;
            int numGlitchAlive = 0;
            int numLoversAlive = 0;
            int numTotalAlive = 0;
            bool ImpostorsLiver = false;
            bool JuggernautLiver = false;
            bool ArsonistLiver = false;
            bool WerewolfLiver = false;
            bool VampiresLiver = false;
            bool PlaguebearerLiver = false;
            bool PestilenceLiver = false;
            bool GlitchLiver = false;

            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        bool lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.Role.IsImpostor)
                        {
                            numImpostorsAlive++;
                            if (lover) ImpostorsLiver = true;
                        }

                        if (Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == playerInfo.PlayerId)
                        {
                            numJuggernautAlive++;
                            if (lover) JuggernautLiver = true;
                        }

                        if (Arsonist.arsonist != null && Arsonist.arsonist.PlayerId == playerInfo.PlayerId)
                        {
                            numArsonistAlive++;
                            if (lover) ArsonistLiver = true;
                        }

                        if (Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == playerInfo.PlayerId)
                        {
                            numWerewolfAlive++;
                            if (lover) WerewolfLiver = true;
                        }

                        if (Dracula.dracula != null && Dracula.dracula.PlayerId == playerInfo.PlayerId)
                        {
                            numVampiresAlive++;
                            if (lover) VampiresLiver = true;
                        }

                        if (Vampire.vampire != null && Vampire.vampire.PlayerId == playerInfo.PlayerId)
                        {
                            numVampiresAlive++;
                            if (lover) VampiresLiver = true;
                        }
                        
                        if (Plaguebearer.plaguebearer != null && Plaguebearer.plaguebearer.PlayerId == playerInfo.PlayerId)
                        {
                            numPlaguebearerAlive++;
                            if (lover) PlaguebearerLiver = true;
                        }
                        
                        if (Pestilence.pestilence != null && Pestilence.pestilence.PlayerId == playerInfo.PlayerId)
                        {
                            numPestilenceAlive++;
                            if (lover) PestilenceLiver = true;
                        }
                        
                        if (Glitch.glitch != null && Glitch.glitch.PlayerId == playerInfo.PlayerId)
                        {
                            numGlitchAlive++;
                            if (lover) GlitchLiver = true;
                        }
                    }
                }
            }

            TeamImpostorsAlive = numImpostorsAlive;
            JuggernautAlive = numJuggernautAlive;
            ArsonistAlive = numArsonistAlive;
            WerewolfAlive = numWerewolfAlive;
            TeamVampiresAlive = numVampiresAlive;
            PlaguebearerAlive = numPlaguebearerAlive;
            PestilenceAlive = numPestilenceAlive;
            GlitchAlive = numGlitchAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorsHasAliveLover = ImpostorsLiver;
            JuggernautHasAliveLover = JuggernautLiver;
            ArsonistHasAliveLover = ArsonistLiver;
            WerewolfHasAliveLover = WerewolfLiver;
            TeamVampiresHasAliveLover = VampiresLiver;
            PlaguebearerHasAliveLover = PlaguebearerLiver;
            PestilenceHasAliveLover = PestilenceLiver;
            GlitchHasAliveLover = GlitchLiver;
        }
    }
}