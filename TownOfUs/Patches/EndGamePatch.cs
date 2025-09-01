using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;

namespace TownOfUs.Patches {
    enum CustomGameOverReason
    {
        ImpostorWin = 10,
        CrewmateWin = 11,
        OxygenWin = 12,
        ReactorWin = 13,
        TaskWin = 14,
        JesterWin = 15,
        JuggernautWin = 16,
        LawyerSoloWin = 17,
        PlaguebearerWin = 18,
        PestilenceWin = 19,
        TeamVampiresWin = 20,
        ExecutionerWin = 21,
        ScavengerWin = 22,
        MercenaryWin = 23,
        GlitchWin = 24,
        ArsonistWin = 25,
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
        LawyerSoloWin,
        PlaguebearerWin,
        PestilenceWin,
        TeamVampiresWin,
        ExecutionerWin,
        ScavengerWin,
        MercenaryWin,
        GlitchWin,
        ArsonistWin,
        AdditionalLawyerBonusWin,
        AdditionalLawyerStolenWin,
        AdditionalAlivePursuerWin,
        AdditionalAliveSurvivorWin,
        AdditionalGABonusWin,
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static float timer = 0;

        public static void clear() {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo {
            private string mDeathReason = "";
            public string PlayerName { get; set; }
            public bool isGATarget { get; set; }
            public bool isExeTarget { get; set; }
            public bool isLawyerTarget { get; set; }
            public bool isFormerThief { get; set; }

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
            public List<RoleInfo> Modifiers { get; set; }
            public int TasksCompleted  {get;set;}
            public int TasksTotal  {get;set;}
            public int? Kills {get; set;}
            public bool IsGuesser {get; set;}
            public bool IsAlive { get; set; }
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    public class EndGameManagerStartPatch
    {
        // Implement a method to record the roles assigned to each player at the end of a game.
        // Store the role history in a session-based collection.
        private static Dictionary<byte, List<RoleId>> _sessionRoleHistory = new Dictionary<byte, List<RoleId>>();
        public static void RecordSessionRoles()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var playerId = player.PlayerId;
                var roleType = RoleInfo.getRoleInfoForPlayer(player, false)?.FirstOrDefault().roleId ?? RoleId.NoRole;

                if (!_sessionRoleHistory.ContainsKey(playerId))
                {
                    _sessionRoleHistory[playerId] = new List<RoleId>();
                }

                if (_sessionRoleHistory[playerId].Count > 100)
                {
                    _sessionRoleHistory[playerId].RemoveRange(0, _sessionRoleHistory[playerId].Count - 100);
                }
                _sessionRoleHistory[playerId].Add(roleType);
            }
        }

        public static void Prefix()
        {
            RecordSessionRoles(); // Record roles at the end of each game
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch {
        public static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;        
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
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
                List<RoleInfo> roles = PlayerGameInfo.getRoles(player);
                (int tasksCompleted, int tasksTotal) = TasksHandler.taskInfo(player.Data);
                List<DeadPlayer> killedPlayers = GetKilledPlayers(player);
                int? killCount = killedPlayers.Count == 0 ? null : killedPlayers.Count;
                bool isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(player.PlayerId);

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = player.Data.DefaultOutfit.PlayerName,
                    isGATarget = GuardianAngel.target != null && player == GuardianAngel.target,
                    isExeTarget = Executioner.target != null && player == Executioner.target,
                    isLawyerTarget = Lawyer.target != null && player == Lawyer.target,
                    isFormerThief = Thief.formerThiefs != null && Thief.formerThiefs.Contains(player),
                    DeathReason = player.getDeathReasonString(),
                    Roles = roles,
                    Modifiers = PlayerGameInfo.getModifiers(player.PlayerId),
                    TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted,
                    Kills = killCount,
                    IsGuesser = isGuesser,
                    IsAlive = !player.Data.IsDead
                });
            }

            // Remove neutrals from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = [
                .. Jester.allPlayers,
                .. Juggernaut.allPlayers,
                .. Lawyer.allPlayers,
                .. Pursuer.allPlayers,
                .. Plaguebearer.allPlayers,
                .. Pestilence.allPlayers,
                .. Dracula.allPlayers,
                .. Vampire.allPlayers,
                .. Survivor.allPlayers,
                .. GuardianAngel.allPlayers,
                .. Amnesiac.allPlayers,
                .. Executioner.allPlayers,
                .. Scavenger.allPlayers,
                .. Mercenary.allPlayers,
                .. Glitch.allPlayers,
                .. Arsonist.allPlayers,
                .. Thief.allPlayers,
            ];

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
            bool jesterWin = Jester.exists && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool juggernautWin = gameOverReason == (GameOverReason)CustomGameOverReason.JuggernautWin;
            bool lawyerSoloWin = Lawyer.exists && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
            bool plaguebearerWin = gameOverReason == (GameOverReason)CustomGameOverReason.PlaguebearerWin;
            bool pestilenceWin = gameOverReason == (GameOverReason)CustomGameOverReason.PestilenceWin;
            bool teamVampiresWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamVampiresWin;
            bool executionerWin = Executioner.exists && gameOverReason == (GameOverReason)CustomGameOverReason.ExecutionerWin;
            bool scavengerWin = Scavenger.exists && gameOverReason == (GameOverReason)CustomGameOverReason.ScavengerWin;
            bool mercenaryWin = Mercenary.exists && gameOverReason == (GameOverReason)CustomGameOverReason.MercenaryWin;
            bool glitchWin = gameOverReason == (GameOverReason)CustomGameOverReason.GlitchWin;
            bool arsonistWin = gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;

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
                foreach (var p in Jester.players)
                {
                    if (!p.triggerJesterWin || p.player == null) continue;
                    CachedPlayerData wpd = new(p.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
            // Juggernaut win
            else if (juggernautWin)
            {
                AdditionalTempData.winCondition = WinCondition.JuggernautWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Juggernaut.exists)
                {
                    foreach (var juggernaut in Juggernaut.allPlayers)
                    {
                        CachedPlayerData wpd = new(juggernaut.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }
            // Lawyer Solo win
            else if (lawyerSoloWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Lawyer.allPlayers)
                {
                    CachedPlayerData wpd = new(p.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.LawyerSoloWin;
            }
            // Plaguebearer win
            else if (plaguebearerWin)
            {
                AdditionalTempData.winCondition = WinCondition.PlaguebearerWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Plaguebearer.exists)
                {
                    foreach (var plaguebearer in Plaguebearer.allPlayers)
                    {
                        CachedPlayerData wpd = new(plaguebearer.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }
            // Pestilence win
            else if (pestilenceWin)
            {
                AdditionalTempData.winCondition = WinCondition.PestilenceWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Pestilence.exists)
                {
                    foreach (var pestilence in Pestilence.allPlayers)
                    {
                        CachedPlayerData wpd = new(pestilence.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }
            // Team Vampires Win
            else if (teamVampiresWin)
            {
                AdditionalTempData.winCondition = WinCondition.TeamVampiresWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Dracula.exists)
                {
                    foreach (var dracula in Dracula.allPlayers)
                    {
                        CachedPlayerData wpd = new(dracula.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
                foreach (var vampire in Vampire.allPlayers)
                {
                    CachedPlayerData wpdVampire = new(vampire.Data)
                    {
                        IsImpostor = false
                    };
                    EndGameResult.CachedWinners.Add(wpdVampire);
                }
            }
            // Executioner Solo win
            else if (executionerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Executioner.allPlayers)
                {
                    CachedPlayerData wpd = new(p.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.ExecutionerWin;
            }
            // Scavenger win
            else if (scavengerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var scavenger in Scavenger.players)
                {
                    if (!scavenger.triggerScavengerWin) continue;
                    CachedPlayerData wpd = new(scavenger.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.ScavengerWin;
            }
            // Mercenary win
            else if (mercenaryWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Mercenary.players)
                {
                    if (p.brilders < Mercenary.brildersRequires || p.player == null) continue;
                    CachedPlayerData wpd = new(p.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.MercenaryWin;
            }
            // Glitch win
            else if (glitchWin)
            {
                AdditionalTempData.winCondition = WinCondition.GlitchWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Glitch.exists)
                {
                    foreach (var glitch in Glitch.allPlayers)
                    {
                        CachedPlayerData wpd = new(glitch.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }
            // Arsonist win
            else if (arsonistWin)
            {
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Arsonist.exists)
                {
                    foreach (var arsonist in Arsonist.allPlayers)
                    {
                        CachedPlayerData wpd = new(arsonist.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }

            // Possible Additional winner: Lawyer
            if (Lawyer.target != null && (!Lawyer.target.Data.IsDead || Lawyer.target.isRole(RoleId.Jester)))
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
                {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null)
                { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => Lawyer.allPlayers.Any(lawyer => lawyer.Data.PlayerName == x.PlayerName)))
                    {
                        if (Lawyer.hasAlivePlayers)
                        {
                            EndGameResult.CachedWinners.Remove(winningClient);
                            foreach (var p in Lawyer.allPlayers)
                            {
                                CachedPlayerData wpd = new(p.Data);
                                EndGameResult.CachedWinners.Add(wpd);
                            }
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin); // The Lawyer replaces the client's victory
                        }
                        else
                        {
                            foreach (var p in Lawyer.allPlayers)
                            {
                                CachedPlayerData wpd = new(p.Data);
                                EndGameResult.CachedWinners.Add(wpd);
                            }
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins with the client
                        }
                    }
                }
            }

            // Possible Additional winner: Pursuer
            if (!oxygenWin && !reactorWin)
            {
                foreach (var pursuer in Pursuer.players)
                {
                    if (pursuer.player != null && !pursuer.player.Data.IsDead && !pursuer.notAckedExiled)
                    {
                        if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == pursuer.player.Data.PlayerName))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(pursuer.player.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
                    }
                }
            }

            // Possible additional winner: Survivor
            if (!oxygenWin && !reactorWin)
            {
                foreach (var survivor in Survivor.players)
                {
                    if (survivor.player != null && !survivor.player.Data.IsDead)
                    {
                        if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == survivor.player.Data.PlayerName))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(survivor.player.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAliveSurvivorWin);
                    }
                }
            }

            // Possible Additional winner: Guardian Angel
            if (GuardianAngel.target != null)
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator())
                {
                    if (winner.PlayerName == GuardianAngel.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null)
                { // The GuardianAngel wins if the client is winning (and alive, but if he wasn't the GuardianAngel shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => GuardianAngel.allPlayers.Any(ga => ga.Data.PlayerName == x.PlayerName)))
                    {
                        foreach (var p in GuardianAngel.allPlayers)
                        {
                            CachedPlayerData wpd = new(p.Data);
                            EndGameResult.CachedWinners.Add(wpd);
                        }
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalGABonusWin); // The GuardianAngel wins with the client

                    }
                }
            }

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - startTime).TotalMilliseconds) / 1000;

            // Reset Settings
            RPCProcedure.resetVariables();
        }

        public static string getDeathReasonString(this PlayerControl p) {
            if (!p.Data.IsDead) return "";
            
            DeadPlayer deadPlayer = GameHistory.deadPlayers.Find(x => x.player.PlayerId == p.PlayerId);
            if(deadPlayer == null) return "";

            Color  killerColor = default;
            string killerName = "";
            if (deadPlayer.killerIfExisting)
            {
                RoleInfo roleInfo    = RoleInfo.getRoleInfoForPlayer(deadPlayer.killerIfExisting, false).FirstOrDefault();
                killerColor = roleInfo?.color ?? default(Color);
                killerName  = deadPlayer.killerIfExisting.Data.PlayerName;

            }

            return deadPlayer.deathReason switch
            {
                DeadPlayer.CustomDeathReason.Disconnect => $"{Helpers.cs(Color.red, $" - disconnected")}",
                DeadPlayer.CustomDeathReason.Exile      => $" - voted out",
                DeadPlayer.CustomDeathReason.Kill => $" - killed by {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.Guess => (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName) ? " - failed guess" : $" - guessed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}",
                DeadPlayer.CustomDeathReason.LawyerSuicide => $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}",
                DeadPlayer.CustomDeathReason.GASuicide => $" - {Helpers.cs(GuardianAngel.color, "bad Guardian Angel")}",
                DeadPlayer.CustomDeathReason.Shift => $" - shifted {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.Bomb => $" - bombed by {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}",
                _ => "",
            };
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance) {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>()) {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate(CachedPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList<CachedPlayerData>();
            for (int i = 0; i < list.Count; i++) {
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
                if (CachedPlayerData2.IsDead) {
                    poolablePlayer.SetBodyAsGhost();
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                } else {
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
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
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
            else if (AdditionalTempData.winCondition == WinCondition.LawyerSoloWin)
            {
                textRenderer.text = "Lawyer Wins";
                textRenderer.color = Lawyer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lawyer.color);
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
            else if (AdditionalTempData.winCondition == WinCondition.TeamVampiresWin)
            {
                textRenderer.text = "Team Vampires Wins";
                textRenderer.color = Dracula.color;
                __instance.BackgroundBar.material.SetColor("_Color", Dracula.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ExecutionerWin)
            {
                textRenderer.text = "Executioner Wins";
                textRenderer.color = Executioner.color;
                __instance.BackgroundBar.material.SetColor("_Color", Executioner.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ScavengerWin)
            {
                textRenderer.text = "Scavenger Wins";
                textRenderer.color = Scavenger.color;
                __instance.BackgroundBar.material.SetColor("_Color", Scavenger.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.MercenaryWin)
            {
                textRenderer.text = "Mercenary Wins";
                textRenderer.color = Mercenary.color;
                __instance.BackgroundBar.material.SetColor("_Color", Mercenary.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.GlitchWin)
            {
                textRenderer.text = "Glitch Wins";
                textRenderer.color = Glitch.color;
                __instance.BackgroundBar.material.SetColor("_Color", Glitch.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
            {
                textRenderer.text = "Arsonist Wins";
                textRenderer.color = Arsonist.color;
                __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions)
            {
                switch (cond)
                {
                    case WinCondition.AdditionalLawyerStolenWin:
                        textRenderer.text += $"\n{Helpers.cs(Lawyer.color, "The Lawyer stole the win from the client")}";
                        break;
                    case WinCondition.AdditionalLawyerBonusWin:
                        textRenderer.text += $"\n{Helpers.cs(Lawyer.color, "The Lawyer wins with the client")}";
                        break;
                    case WinCondition.AdditionalAlivePursuerWin:
                        textRenderer.text += $"\n{Helpers.cs(Pursuer.color, "The Pursuer survived")}";
                        break;
                    case WinCondition.AdditionalAliveSurvivorWin:
                        textRenderer.text += $"\n{Helpers.cs(Survivor.color, "The Survivor survived")}";
                        break;
                    case WinCondition.AdditionalGABonusWin:
                        textRenderer.text += $"\n{Helpers.cs(GuardianAngel.color, "The Guardian Angel wins with the target")}";
                        break;
                }
            }

            if (showRoleSummary) {
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

                foreach (AdditionalTempData.PlayerRoleInfo data in AdditionalTempData.playerRoles) {
                    var summaryText = new List<string>();

                    string name = Helpers.cs(data.IsAlive ? Color.white : new Color(.7f, .7f, .7f), data.PlayerName);
                    string gaTargetInfo = data.isGATarget ? Helpers.cs(GuardianAngel.color, " ★") : "";
                    string exeTargetInfo = data.isExeTarget ? Helpers.cs(Executioner.color, " X") : "";
                    string lawyerTargetInfo = data.isLawyerTarget ? Helpers.cs(Lawyer.color, " §") : "";
                    string formerThiefInfo = data.isFormerThief ? Helpers.cs(Thief.color, " $") : "";
                    string roles = data.Roles.Count > 0 ? string.Join(" -> ", data.Roles.Select(x => Helpers.cs(x.color, x.name))) : "";
                    string modifiers = data.Modifiers.Count > 0 ? string.Join(" ", data.Modifiers.Select(x => Helpers.cs(x.color, $"({x.name}) "))) : "";
                    string deathReason = data.DeathReason.IsNullOrWhiteSpace() ? "" : data.DeathReason;
                    string taskInfo = data.TasksTotal > 0 ? (data.TasksCompleted == data.TasksTotal ? $" - Tasks: {Helpers.cs(Color.green, $"{data.TasksCompleted}/{data.TasksTotal}")}" : $" - Tasks: {Helpers.cs(Color.red, $"{data.TasksCompleted}/{data.TasksTotal}")}") : "";
                    string killInfo = data.Kills != null ? $" - <color=#FF0000FF>Kills: {data.Kills}</color>" : "";
                    string guesserInfo = data.IsGuesser ? " (Guesser)" : "";
                    summaryText.Add($"{name}{gaTargetInfo}{exeTargetInfo}{lawyerTargetInfo}{formerThiefInfo}{deathReason} | {modifiers}{roles}{guesserInfo}{taskInfo}{killInfo}");

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
    class CheckEndCriteriaPatch {
        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForExecutionerWin(__instance)) return false;
            if (CheckAndEndGameForLawyerMeetingWin(__instance)) return false;
            if (CheckAndEndGameForScavengerWin(__instance)) return false;
            if (CheckAndEndGameForMercenaryWin(__instance)) return false;

            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPlaguebearerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPestilenceWin(__instance, statistics)) return false;
            if (CheckAndEndGameForArsonistWin(__instance, statistics)) return false;
            if (CheckAndEndGameForTeamVampiresWin(__instance, statistics)) return false;
            if (CheckAndEndGameForGlitchWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJuggernautWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForMercenaryWin(ShipStatus __instance)
        {
            if (Mercenary.players.Any(x => x.player && x.brilders >= Mercenary.brildersRequires))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MercenaryWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForScavengerWin(ShipStatus __instance)
        {
            if (Scavenger.players.Any(x => x.player && x.triggerScavengerWin))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ScavengerWin, false);
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

        private static bool CheckAndEndGameForLawyerMeetingWin(ShipStatus __instance)
        {
            if (Lawyer.triggerLawyerWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LawyerSoloWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
        {
            if (Jester.players.Any(x => x.player && x.triggerJesterWin))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
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

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TaskWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamArsonistAlive >= statistics.TotalAlive - statistics.TeamArsonistAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForGlitchWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamGlitchAlive >= statistics.TotalAlive - statistics.TeamGlitchAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.GlitchWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForTeamVampiresWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamVampiresAlive >= statistics.TotalAlive - statistics.TeamVampiresAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamVampiresWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForPestilenceWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamPestilenceAlive >= statistics.TotalAlive - statistics.TeamPestilenceAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PestilenceWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForPlaguebearerWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamPlaguebearerAlive >= statistics.TotalAlive - statistics.TeamPlaguebearerAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PlaguebearerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJuggernautWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJuggernautAlive >= statistics.TotalAlive - statistics.TeamJuggernautAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JuggernautWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJuggernautAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0 && !Helpers.stopGameEndForKillers())
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ImpostorWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamPlaguebearerAlive == 0 && statistics.TeamPestilenceAlive == 0 && statistics.TeamVampiresAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamArsonistAlive == 0) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.CrewmateWin, false);
                return true;
            }
            return false;
        }

        private static void EndGameForOxygenSabotage(ShipStatus __instance) {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.OxygenWin, false);
            return;
        }

        private static void EndGameForReactorSabotage(ShipStatus __instance) {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ReactorWin, false);
            return;
        }
    }

    internal class PlayerStatistics {
        public int TeamArsonistAlive { get; set; }
        public int TeamGlitchAlive { get; set; }
        public int TeamVampiresAlive { get; set; }
        public int TeamPestilenceAlive { get; set; }
        public int TeamPlaguebearerAlive {get;set;}
        public int TeamJuggernautAlive {get;set;}
        public int TeamImpostorsAlive {get;set;}
        public int TotalAlive {get;set;}

        public PlayerStatistics(ShipStatus __instance) {
            GetPlayerCounts();
        }

        private void GetPlayerCounts() {
            int numArsonistAlive = Arsonist.livingPlayers.Count;
            int numGlitchAlive = Glitch.livingPlayers.Count;
            int numVampiresAlive = Dracula.livingPlayers.Count + Vampire.livingPlayers.Count;
            int numPestilenceAlive = Pestilence.livingPlayers.Count;
            int numPlaguebearerAlive = Plaguebearer.livingPlayers.Count;
            int numJuggernautAlive = Juggernaut.livingPlayers.Count;
            int numImpostorsAlive = 0;
            int numTotalAlive = 0;

            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        if (playerInfo.Role.IsImpostor) {
                            numImpostorsAlive++;
                        }
                    }
                }
            }

            TeamArsonistAlive = numArsonistAlive;
            TeamGlitchAlive = numGlitchAlive;
            TeamVampiresAlive = numVampiresAlive;
            TeamPestilenceAlive = numPestilenceAlive;
            TeamPlaguebearerAlive = numPlaguebearerAlive;
            TeamJuggernautAlive = numJuggernautAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TotalAlive = numTotalAlive;
        }
    }
}