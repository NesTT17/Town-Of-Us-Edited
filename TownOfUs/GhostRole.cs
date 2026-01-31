using System;
using System.Collections.Generic;
using System.Linq;

namespace TownOfUs
{
    public class GhostRole
    {
        public enum AssignType { Crewmate, Neutral, Impostor }

        public static Dictionary<AssignType, List<Assignment>> GhostRoles = new();
        public static List<PlayerControl> GhostPlayer = new();

        public static void clearAndReload()
        {
            GhostRoles.Clear();
            GhostPlayer.Clear();

            GhostRoles[AssignType.Crewmate] = new List<Assignment>
            {
                new Assignment(RoleId.Poltergeist, CustomOptionHolder.poltergeistSpawnRate.getSelection()),
            };

            GhostRoles[AssignType.Neutral] = new List<Assignment>
            {
                
            };

            GhostRoles[AssignType.Impostor] = new List<Assignment>
            {
                new Assignment(RoleId.Banshee, CustomOptionHolder.bansheeSpawnRate.getSelection()),
            };
        }

        public class Assignment
        {
            public RoleId RoleId { get; set; }
            public int SpawnRate { get; set; }
            public bool Assigned { get; set; }

            public Assignment(RoleId roleId, int Rate)
            {
                RoleId = roleId;
                SpawnRate = Rate;
            }
        }

        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
        public static class AssignRoleOnDeathPatch
        {
            public static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] bool specialRolesAllowed)
            {
                if (!player.Data.IsDead || player == null || !specialRolesAllowed) return false;
                return true;
            }

            public static void Postfix([HarmonyArgument(0)] PlayerControl player)
            {
                if (GhostPlayer.Contains(player)) return;

                if (!player.isEvil()) AssignRole(player, AssignType.Crewmate);
                else if (player.Data.Role.IsImpostor) AssignRole(player, AssignType.Impostor);
                else if (player.isAnyNeutral()) AssignRole(player, AssignType.Neutral);
            }
        }

        private static void AssignRole(PlayerControl player, AssignType assignType)
        {
            if (!GhostRoles.TryGetValue(assignType, out var roles) || roles.All(x => x.SpawnRate == 0)) return;
            roles = roles.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var role in roles.Where(x => x.SpawnRate == 10 && !x.Assigned).ToList())
            {
                AssignRoleToPlayer(player, role);
                return;
            }

            int maxCount = roles.Count;
            int count = 0;
            while (count < maxCount)
            {
                bool assigned = false;
                foreach (var role in roles.Where(x => x.SpawnRate is > 0 and < 10 && !x.Assigned).ToList())
                {
                    if (rnd.Next(1, 101) <= role.SpawnRate * (10 + count))
                    {
                        AssignRoleToPlayer(player, role);
                        assigned = true;
                        break;
                    }
                }
                if (assigned) break;
                count++;
            }
        }

        private static void AssignRoleToPlayer(PlayerControl player, Assignment role)
        {
            var write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGhostRole, SendOption.Reliable, -1);
            write.Write(player.PlayerId);
            write.Write((byte)role.RoleId);
            AmongUsClient.Instance.FinishRpcImmediately(write);
            RPCProcedure.setGhostRole(player.PlayerId, (byte)role.RoleId);

            GhostPlayer.Add(player);
            role.Assigned = true;
        }
    }
}