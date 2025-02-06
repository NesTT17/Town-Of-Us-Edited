using System.Collections.Generic;
using System;
using UnityEngine;

namespace TownOfUs {
    public class DeadPlayer
    {
        public enum CustomDeathReason {
            Exile,
            Kill,
            Disconnect,
            SheriffMissfire,
            Shift,
            Guess,
            LoverSuicide,
            LawyerSuicide,
            GuardianAngelSuicide,
        };

        public PlayerControl player;
        public DateTime timeOfDeath;
        public CustomDeathReason deathReason;
        public PlayerControl killerIfExisting;
        public bool wasCleaned;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, CustomDeathReason deathReason, PlayerControl killerIfExisting) {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
            this.wasCleaned = false;
        }
    }

    static class GameHistory {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new List<Tuple<Vector3, bool>>();
        public static List<DeadPlayer> deadPlayers = new List<DeadPlayer>();
        
        public static List<DeadPlayer> GetKilledPlayers(PlayerControl player)
        {
            List<DeadPlayer> killedPlayers = [];
            foreach (DeadPlayer deadPlayerData in deadPlayers)
            {
                PlayerControl killer      = deadPlayerData.killerIfExisting;
                if (!killer) continue;
                
                PlayerControl deadPlayer  = deadPlayerData.player;
                bool          i_am_killer = killer     == player;
                bool          not_suicide = deadPlayer != player;
                if (i_am_killer && not_suicide)
                {
                    killedPlayers.Add(deadPlayerData);
                }
            }

            return killedPlayers;
        }

        public static void clearGameHistory() {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            deadPlayers = new List<DeadPlayer>();
        }

        public static void overrideDeathReasonAndKiller(PlayerControl player, DeadPlayer.CustomDeathReason deathReason, PlayerControl killer)
        {
            DeadPlayer target = deadPlayers.Find(x => x.player.PlayerId == player.PlayerId);
            if (target != null)
            {
                target.deathReason = deathReason;
                if (killer)
                {
                    target.killerIfExisting = killer;
                }
            }
            else if (player)
            {
                // Create dead player if needed:
                var dp = new DeadPlayer(player, DateTime.UtcNow, deathReason, killer);
                deadPlayers.Add(dp);
            }
        }
    }
}