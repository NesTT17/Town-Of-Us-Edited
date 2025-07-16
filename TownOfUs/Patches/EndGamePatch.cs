using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;

namespace TownOfUs.Patches {
    enum CustomGameOverReason {
        JesterWin = 10,
        LoversWin = 11,
        TeamVampireWin = 12,
        ScavengerWin = 13,
        ExecutionerWin = 14,
        FallenAngelWin = 15,
        JuggernautWin = 16,
        MercenaryWin = 17,
        DoomsayerWin = 18,
        WerewolfWin = 19,
        GlitchWin = 20,
    }

    enum WinCondition {
        Default,
        JesterWin,
        LoversTeamWin,
        LoversSoloWin,
        VampireWin,
        ScavengerWin,
        ExecutionerWin,
        FallenAngelWin,
        JuggernautWin,
        MercenaryWin,
        DoomsayerWin,
        WerewolfWin,
        GlitchWin,
        AdditionalLawyerBonusWin,
        AdditionalAlivePursuerWin,
        AdditionalGuardianAngelBonusWin,
        AdditionalAliveSurvivorWin,
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static float timer = 0;

        public static void clear() {
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo
        {
            private string mDeathReason = "";
            public string PlayerName { get; set; }
            public bool IsGATarget { get; set; }
            public bool IsFATarget { get; set; }
            public bool IsExeTarget { get; set; }
            public bool IsLawyerTarget { get; set; }

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
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public bool IsGuesser { get; set; }
            public int? Kills { get; set; }
            public bool IsAlive { get; set; }
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

        public static void UpdatePlayerSummary(PlayerControl player)
        {
            AdditionalTempData.PlayerRoleInfo playerControlRole = AdditionalTempData.playerRoles.Find(x => x.PlayerName.Equals(player.Data.PlayerName));
            if (playerControlRole == null) return;

            playerControlRole.IsGATarget = GuardianAngel.guardianAngel != null && GuardianAngel.target != null && player.PlayerId == GuardianAngel.target.PlayerId;
            playerControlRole.IsFATarget = FallenAngel.fallenAngel != null && FallenAngel.target != null && player.PlayerId == FallenAngel.target.PlayerId;
            playerControlRole.IsExeTarget = Executioner.executioner != null && Executioner.target != null && player.PlayerId == Executioner.target.PlayerId;
            playerControlRole.IsLawyerTarget = Lawyer.lawyer != null && Lawyer.target != null && player.PlayerId == Lawyer.target.PlayerId;

            List<RoleInfo> roles = PlayerGameInfo.GetRoles(player);
            (int tasksCompleted, int tasksTotal) = TasksHandler.taskInfo(player.Data);

            List<DeadPlayer> killedPlayers = GameHistory.GetKilledPlayers(player);
            int? killCount = killedPlayers.Count == 0 ? null : killedPlayers.Count;

            bool isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(player.PlayerId);
            playerControlRole.PlayerName = player.Data.PlayerName;
            playerControlRole.DeathReason = player.getDeathReasonString();
            playerControlRole.Roles = roles;
            playerControlRole.Modifiers = PlayerGameInfo.GetModifiers(player.PlayerId);
            playerControlRole.TasksTotal = tasksTotal;
            playerControlRole.TasksCompleted = tasksCompleted;
            playerControlRole.IsGuesser = isGuesser;
            playerControlRole.Kills ??= killCount;
            playerControlRole.IsAlive = !player.Data.IsDead;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
            AdditionalTempData.clear();

            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                UpdatePlayerSummary(playerControl);
            }

            // Remove neutrals from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();
            if (Jester.jester != null) notWinners.Add(Jester.jester);
            if (Dracula.dracula != null) notWinners.Add(Dracula.dracula);
            if (Vampire.vampire != null) notWinners.Add(Vampire.vampire);
            notWinners.AddRange(Dracula.formerDraculas);
            if (Scavenger.scavenger != null) notWinners.Add(Scavenger.scavenger);
            if (Executioner.executioner != null) notWinners.Add(Executioner.executioner);
            if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
            if (Pursuer.pursuer != null) notWinners.Add(Pursuer.pursuer);
            if (GuardianAngel.guardianAngel != null) notWinners.Add(GuardianAngel.guardianAngel);
            if (FallenAngel.fallenAngel != null) notWinners.Add(FallenAngel.fallenAngel);
            if (Survivor.survivor != null) notWinners.Add(Survivor.survivor);
            if (Juggernaut.juggernaut != null) notWinners.Add(Juggernaut.juggernaut);
            if (Mercenary.mercenary != null) notWinners.Add(Mercenary.mercenary);
            if (Doomsayer.doomsayer != null) notWinners.Add(Doomsayer.doomsayer);
            if (Werewolf.werewolf != null) notWinners.Add(Werewolf.werewolf);
            if (Glitch.glitch != null) notWinners.Add(Glitch.glitch);
            if (Amnesiac.amnesiac != null) notWinners.Add(Amnesiac.amnesiac);

            List<CachedPlayerData> winnersToRemove = new List<CachedPlayerData>();
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator()) {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);
            
            bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool loversWin = Lovers.existingAndAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin || (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.existingWithKiller())); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            bool teamVampireWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamVampireWin && ((Dracula.dracula != null && !Dracula.dracula.Data.IsDead) || (Vampire.vampire != null && !Vampire.vampire.Data.IsDead));
            bool scavengerWin = Scavenger.scavenger != null && gameOverReason == (GameOverReason)CustomGameOverReason.ScavengerWin;
            bool executionerWin = Executioner.executioner != null && gameOverReason == (GameOverReason)CustomGameOverReason.ExecutionerWin;
            bool fallenAngelWin = FallenAngel.fallenAngel != null && gameOverReason == (GameOverReason)CustomGameOverReason.FallenAngelWin;
            bool juggernautWin = Juggernaut.juggernaut != null && gameOverReason == (GameOverReason)CustomGameOverReason.JuggernautWin;
            bool mercenaryWin = Mercenary.mercenary != null && gameOverReason == (GameOverReason)CustomGameOverReason.MercenaryWin;
            bool doomsayerWin = Doomsayer.doomsayer != null && gameOverReason == (GameOverReason)CustomGameOverReason.DoomsayerWin;
            bool werewolfWin = Werewolf.werewolf != null && gameOverReason == (GameOverReason)CustomGameOverReason.WerewolfWin;
            bool glitchWin = Glitch.glitch != null && gameOverReason == (GameOverReason)CustomGameOverReason.GlitchWin;

            // Jester win
            if (jesterWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Jester.jester.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
            // Lovers win conditions
            else if (loversWin) {
                // Double win for lovers, crewmates also win
                if (!Lovers.existingWithKiller()) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (p == Pursuer.pursuer && !Pursuer.pursuer.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (p == Survivor.survivor && !Survivor.survivor.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (!p.isEvil())
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover1.Data));
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover2.Data));
                }
            }
            // Dracuka win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamVampireWin) {
                // Dracula wins if nobody except dracula is alive
                AdditionalTempData.winCondition = WinCondition.VampireWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Dracula.dracula.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                // If there is a vampire. The Vampire also wins
                if (Vampire.vampire != null) {
                    CachedPlayerData wpdVampire = new CachedPlayerData(Vampire.vampire.Data);
                    wpdVampire.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdVampire);
                }
                foreach (var player in Dracula.formerDraculas) {
                    CachedPlayerData wpdFormerDracula = new CachedPlayerData(player.Data);
                    wpdFormerDracula.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdFormerDracula);
                }
            }
            // Scavenger win
            else if (scavengerWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Scavenger.scavenger.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ScavengerWin;
            }
            // Executioner win
            else if (executionerWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Executioner.executioner.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ExecutionerWin;
            }
            // Fallen Angel win
            else if (fallenAngelWin) {
                AdditionalTempData.winCondition = WinCondition.FallenAngelWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(FallenAngel.fallenAngel.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                // If there is a FA target. The FA target also wins
                if (FallenAngel.target != null) {
                    CachedPlayerData wpdFATarget = new CachedPlayerData(FallenAngel.target.Data);
                    wpdFATarget.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdFATarget);
                }
            }
            // Juggernaut win
            else if (juggernautWin) {
                AdditionalTempData.winCondition = WinCondition.JuggernautWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Juggernaut.juggernaut.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
            }
            // Mercenary win
            else if (mercenaryWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Mercenary.mercenary.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MercenaryWin;
            }
            // Doomsayer win
            else if (doomsayerWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Doomsayer.doomsayer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
            }
            // Werewolf win
            else if (werewolfWin) {
                AdditionalTempData.winCondition = WinCondition.WerewolfWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Werewolf.werewolf.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
            }
            // Glitch win
            else if (glitchWin) {
                AdditionalTempData.winCondition = WinCondition.GlitchWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Glitch.glitch.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
            }

            // Possible Additional winner: Lawyer
            if (Lawyer.lawyer != null && Lawyer.target != null && (!Lawyer.target.Data.IsDead || Lawyer.target == Jester.jester) && !Pursuer.notAckedExiled) {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator()) {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null) { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins together with the client
                } 
            }

            // Possible Additional winner: Pursuer
            if (Pursuer.pursuer != null && !Pursuer.pursuer.Data.IsDead && !Pursuer.notAckedExiled) {
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Pursuer.pursuer.Data.PlayerName))
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Pursuer.pursuer.Data));
                AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
            }

            // Possible Additional winner: Guardian Angel
            if (GuardianAngel.guardianAngel != null && GuardianAngel.target != null) {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator()) {
                    if (winner.PlayerName == GuardianAngel.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null) { // The Guardian Angel wins if the client is winning
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == GuardianAngel.guardianAngel.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(GuardianAngel.guardianAngel.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalGuardianAngelBonusWin); // The Guardian Angel wins together with the client
                } 
            }

            // Possible Additional winner: Survivor
            if (Survivor.survivor != null && !Survivor.survivor.Data.IsDead) {
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Survivor.survivor.Data.PlayerName))
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Survivor.survivor.Data));
                AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAliveSurvivorWin);
            }

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - TownOfUs.startTime).TotalMilliseconds) / 1000;

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
                DeadPlayer.CustomDeathReason.SheriffMissfire => $" - checked {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}",
                DeadPlayer.CustomDeathReason.Shift => $" - {Helpers.cs(Color.yellow, "shifted")} {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.Guess => killerName == p.Data.PlayerName ? $" - failed guess" : $" - guessed by {Helpers.cs(killerColor, killerName)}",
                DeadPlayer.CustomDeathReason.LoverSuicide  => $" - {Helpers.cs(Lovers.color, "lover died")}",
                DeadPlayer.CustomDeathReason.LawyerSuicide => $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}",
                DeadPlayer.CustomDeathReason.GuardianAngelSuicide => $" - {Helpers.cs(GuardianAngel.color, "bad Guardian Angel")}",
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
            
            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                textRenderer.text = "Jester Wins";
                textRenderer.color = Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
            } else if (AdditionalTempData.winCondition == WinCondition.Default) {
                switch (OnGameEndPatch.gameOverReason) {
                    case GameOverReason.ImpostorDisconnect:
                        textRenderer.text = "Last Crewmate Disconnected";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsByKill:
                        textRenderer.text = "Impostors Win - By Kill";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsBySabotage:
                        textRenderer.text = "Impostors Win - By Sabotage";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsByVote:
                        textRenderer.text = "Impostors Win - By Vote, Guess or DC";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.CrewmatesByTask:
                        textRenderer.text = "Crew Wins - Task Win";
                        textRenderer.color = Color.white;
                        break;
                    case GameOverReason.CrewmateDisconnect:
                        textRenderer.text = "Crew Wins - No Evil Killers Left";
                        textRenderer.color = Color.white;
                        break;
                    case GameOverReason.CrewmatesByVote:
                        textRenderer.text = "Crew Wins - No Evil Killers Left";
                        textRenderer.color = Color.white;
                        break;
                }
            } else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin) {
                textRenderer.text = "Lovers And Crewmates Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin) {
                textRenderer.text = "Lovers Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } else if (AdditionalTempData.winCondition == WinCondition.VampireWin) {
                textRenderer.text = "Team Vampires Wins";
                textRenderer.color = Dracula.color;
                __instance.BackgroundBar.material.SetColor("_Color", Dracula.color);
            } else if (AdditionalTempData.winCondition == WinCondition.ScavengerWin) {
                textRenderer.text = "Scavenger Wins";
                textRenderer.color = Scavenger.color;
                __instance.BackgroundBar.material.SetColor("_Color", Scavenger.color);
            } else if (AdditionalTempData.winCondition == WinCondition.ExecutionerWin) {
                textRenderer.text = "Executioner Wins";
                textRenderer.color = Executioner.color;
                __instance.BackgroundBar.material.SetColor("_Color", Executioner.color);
            } else if (AdditionalTempData.winCondition == WinCondition.FallenAngelWin) {
                textRenderer.text = "Fallen Angel Wins";
                textRenderer.color = FallenAngel.color;
                __instance.BackgroundBar.material.SetColor("_Color", FallenAngel.color);
            } else if (AdditionalTempData.winCondition == WinCondition.JuggernautWin) {
                textRenderer.text = "Juggernaut Wins";
                textRenderer.color = Juggernaut.color;
                __instance.BackgroundBar.material.SetColor("_Color", Juggernaut.color);
            } else if (AdditionalTempData.winCondition == WinCondition.MercenaryWin) {
                textRenderer.text = "Mercenary Wins";
                textRenderer.color = Mercenary.color;
                __instance.BackgroundBar.material.SetColor("_Color", Mercenary.color);
            } else if (AdditionalTempData.winCondition == WinCondition.DoomsayerWin) {
                textRenderer.text = "Doomsayer Wins";
                textRenderer.color = Doomsayer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Doomsayer.color);
            } else if (AdditionalTempData.winCondition == WinCondition.WerewolfWin) {
                textRenderer.text = "Werewolf Wins";
                textRenderer.color = Werewolf.color;
                __instance.BackgroundBar.material.SetColor("_Color", Werewolf.color);
            } else if (AdditionalTempData.winCondition == WinCondition.GlitchWin) {
                textRenderer.text = "Glitch Wins";
                textRenderer.color = Glitch.color;
                __instance.BackgroundBar.material.SetColor("_Color", Glitch.color);
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions) {
                if (cond == WinCondition.AdditionalLawyerBonusWin) {
                    textRenderer.text += $"\n{Helpers.cs(Lawyer.color, "The Lawyer wins with the client")}";
                } else if (cond == WinCondition.AdditionalAlivePursuerWin) {
                    textRenderer.text += $"\n{Helpers.cs(Pursuer.color, "The Pursuer survived")}";
                } else if (cond == WinCondition.AdditionalGuardianAngelBonusWin) {
                    textRenderer.text += $"\n{Helpers.cs(GuardianAngel.color, "The Guardian Angel wins with the client")}";
                } else if (cond == WinCondition.AdditionalAliveSurvivorWin) {
                    textRenderer.text += $"\n{Helpers.cs(Survivor.color, "The Survivor survived")}";
                }
            }

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
                string gaTarget = data.IsGATarget ? $"{Helpers.cs(GuardianAngel.color, " ★")}" : "";
                string faTarget = data.IsFATarget ? $"{Helpers.cs(FallenAngel.color, " ★")}" : "";
                string exeTarget = data.IsExeTarget ? $"{Helpers.cs(Executioner.color, " X")}" : "";
                string lawyerTarget = data.IsLawyerTarget ? $"{Helpers.cs(Lawyer.color, " §")}" : "";
                string roles = data.Roles.Count > 0 ? string.Join(" -> ", data.Roles.Select(x => Helpers.cs(x.color, x.name))) : "";
                string modifiers = data.Modifiers.Count > 0 ? string.Join(" ", data.Modifiers.Select(x => Helpers.cs(x.color, $"({x.name}) "))) : "";
                string deathReason = data.DeathReason.IsNullOrWhiteSpace() ? "" : data.DeathReason;
                string guesserInfo = data.IsGuesser ? " (Gs)" : "";
                string taskInfo = data.TasksTotal > 0 ? (data.TasksCompleted == data.TasksTotal ? $" - Tasks: {Helpers.cs(Color.green, $"{data.TasksCompleted}/{data.TasksTotal}")}" : $" - Tasks: {Helpers.cs(Color.red, $"{data.TasksCompleted}/{data.TasksTotal}")}") : "";
                string killInfo = data.Kills != null ? $" - <color=#FF0000FF>Kills: {data.Kills}</color>" : "";
                summaryText.Add($"{name}{gaTarget}{faTarget}{exeTarget}{lawyerTarget}{deathReason} | {modifiers}{roles}{guesserInfo}{taskInfo}{killInfo}");

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
            if (CheckAndEndGameForDoomsayerWin(__instance)) return false;
            if (CheckAndEndGameForMercenaryWin(__instance)) return false;
            if (CheckAndEndGameForExecutionerWin(__instance)) return false;
            if (CheckAndEndGameForScavengerWin(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForGlitchWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWerewolfWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJuggernautWin(__instance, statistics)) return false;
            if (CheckAndEndGameForFallenAngelWin(__instance, statistics)) return false;
            if (CheckAndEndGameForDraculaWin(__instance, statistics)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForDoomsayerWin(ShipStatus __instance) {
            if (Doomsayer.triggerDoomsayerWin) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.DoomsayerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForMercenaryWin(ShipStatus __instance) {
            if (Mercenary.triggerMercenaryWin) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MercenaryWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForExecutionerWin(ShipStatus __instance) {
            if (Executioner.triggerExecutionerWin) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ExecutionerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForScavengerWin(ShipStatus __instance) {
            if (Scavenger.triggerScavengerWin) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ScavengerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance) {
            if (Jester.triggerJesterWin) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance) {
            if (MapUtilities.Systems == null) return false;
            var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null) {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) {
                systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null) {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks) {
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForGlitchWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.GlitchAlive >= statistics.TotalAlive - statistics.GlitchAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamVampireAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.GlitchHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.GlitchWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForWerewolfWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.WerewolfAlive >= statistics.TotalAlive - statistics.WerewolfAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamVampireAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.JuggernautAlive == 0 && statistics.GlitchAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.WerewolfHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.WerewolfWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJuggernautWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.JuggernautAlive >= statistics.TotalAlive - statistics.JuggernautAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamVampireAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.JuggernautHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JuggernautWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForFallenAngelWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.FallenAngelAlive >= statistics.TotalAlive - statistics.FallenAngelAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamVampireAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.FallenAngelHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.FallenAngelWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForDraculaWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamVampireAlive >= statistics.TotalAlive - statistics.TeamVampireAlive && statistics.TeamImpostorsAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.TeamVampireHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamVampireWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3) {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamVampireAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0 && !Helpers.stopGameEndForKillers() && !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2)) {
                GameOverReason endReason;
                switch (GameData.LastDeathReason) {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorsByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorsByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorsByVote;
                        break;
                }
                GameManager.Instance.RpcEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamVampireAlive == 0 && statistics.FallenAngelAlive == 0 && statistics.JuggernautAlive == 0 && statistics.WerewolfAlive == 0 && statistics.GlitchAlive == 0) {
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByVote, false);
                return true;
            }
            return false;
        }

        private static void EndGameForSabotage(ShipStatus __instance) {
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);
            return;
        }

    }

    internal class PlayerStatistics {
        public int TeamImpostorsAlive {get;set;}
        public int TeamVampireAlive {get;set;}
        public int FallenAngelAlive {get;set;}
        public int JuggernautAlive {get;set;}
        public int WerewolfAlive {get;set;}
        public int GlitchAlive {get;set;}
        public int TeamLoversAlive {get;set;}
        public int TotalAlive {get;set;}
        public bool TeamImpostorHasAliveLover {get;set;}
        public bool TeamVampireHasAliveLover {get;set;}
        public bool FallenAngelHasAliveLover {get;set;}
        public bool JuggernautHasAliveLover {get;set;}
        public bool WerewolfHasAliveLover {get;set;}
        public bool GlitchHasAliveLover {get;set;}

        public PlayerStatistics(ShipStatus __instance) {
            GetPlayerCounts();
        }

        private bool isLover(NetworkedPlayerInfo p) {
            return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId);
        }

        private void GetPlayerCounts() {
            int numImpostorsAlive = 0;
            int numVampireAlive = 0;
            int numFAAlive = 0;
            int numJuggAlive = 0;
            int numWereAlive = 0;
            int numGlitchAlive = 0;
            int numLoversAlive = 0;
            int numTotalAlive = 0;
            bool impLover = false;
            bool vampireLover = false;
            bool faLover = false;
            bool juggLover = false;
            bool wereLover = false;
            bool glitchLover = false;

            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        bool lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.Role.IsImpostor) {
                            numImpostorsAlive++;
                            if (lover) impLover = true;
                        }
                        if (Dracula.dracula != null && Dracula.dracula.PlayerId == playerInfo.PlayerId) {
                            numVampireAlive++;
                            if (lover) vampireLover = true;
                        }
                        if (Vampire.vampire != null && Vampire.vampire.PlayerId == playerInfo.PlayerId) {
                            numVampireAlive++;
                            if (lover) vampireLover = true;
                        }
                        if (FallenAngel.fallenAngel != null && FallenAngel.fallenAngel.PlayerId == playerInfo.PlayerId) {
                            numFAAlive++;
                            if (lover) faLover = true;
                        }
                        if (Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == playerInfo.PlayerId) {
                            numJuggAlive++;
                            if (lover) juggLover = true;
                        }
                        if (Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == playerInfo.PlayerId) {
                            numWereAlive++;
                            if (lover) wereLover = true;
                        }
                        if (Glitch.glitch != null && Glitch.glitch.PlayerId == playerInfo.PlayerId) {
                            numGlitchAlive++;
                            if (lover) glitchLover = true;
                        }
                    }
                }
            }

            TeamImpostorsAlive = numImpostorsAlive;
            TeamVampireAlive = numVampireAlive;
            FallenAngelAlive = numFAAlive;
            JuggernautAlive = numJuggAlive;
            WerewolfAlive = numWereAlive;
            GlitchAlive = numGlitchAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorHasAliveLover = impLover;
            TeamVampireHasAliveLover = vampireLover;
            FallenAngelHasAliveLover = faLover;
            JuggernautHasAliveLover = juggLover;
            WerewolfHasAliveLover = wereLover;
            GlitchHasAliveLover = glitchLover;
        }
    }
}
