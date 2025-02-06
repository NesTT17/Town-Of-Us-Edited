using System.Collections.Generic;
using System.Linq;

namespace TownOfUs
{
    public class PlayerGameInfo {
        public static readonly Dictionary<byte, PlayerGameInfo> Mapping = new();
        public readonly List<RoleInfo> Roles = new();
        public readonly List<RoleInfo> Modifiers = new();

        public static List<RoleInfo> GetRoles(PlayerControl player)
        {
            if (Mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
                return roleInfo.Roles;

            Mapping.Add(player.PlayerId, roleInfo = new PlayerGameInfo());

            if (player.Data.Role.IsImpostor)
                roleInfo.Roles.Add(RoleInfo.impostor);
            else
                roleInfo.Roles.Add(RoleInfo.crewmate);

            return roleInfo.Roles;
        }

        public static List<RoleInfo> GetRoles(NetworkedPlayerInfo player)
        {
            if (Mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
                return roleInfo.Roles;

            Mapping.Add(player.PlayerId, roleInfo = new PlayerGameInfo());

            if (player.Role.IsImpostor)
                roleInfo.Roles.Add(RoleInfo.impostor);
            else
                roleInfo.Roles.Add(RoleInfo.crewmate);

            return roleInfo.Roles;
        }

        public static void AddRole(byte playerId, RoleInfo role)
        {
            if (!Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                Mapping.Add(playerId, gameInfo = new PlayerGameInfo());

            if (!gameInfo.Roles.Any(r => r.roleId == role.roleId))
                gameInfo.Roles.Add(role);
        }

        public static List<RoleInfo> GetModifiers(byte playerId)
        {
            if (!Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                Mapping.Add(playerId, gameInfo = new PlayerGameInfo());
            return gameInfo.Modifiers;
        }

        public static void AddModifier(byte playerId, RoleInfo role)
        {
            if (!Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                Mapping.Add(playerId, gameInfo = new PlayerGameInfo());

            if (!gameInfo.Modifiers.Any(r => r.roleId == role.roleId))
                gameInfo.Modifiers.Add(role);
        }

        public static void clearAndReload()
        {
            Mapping.Clear();
        }
    }
}