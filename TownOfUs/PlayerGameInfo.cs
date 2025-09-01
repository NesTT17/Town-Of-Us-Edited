using System.Collections.Generic;
using System.Linq;

namespace TownOfUs
{
    public class PlayerGameInfo
    {
        public static readonly Dictionary<byte, PlayerGameInfo> mapping = new();
        public readonly List<RoleInfo> roles = new();
        public readonly List<RoleInfo> modifiers = new();

        public static List<RoleInfo> getRoles(PlayerControl player)
        {
            if (mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
                return roleInfo.roles;

            mapping.Add(player.PlayerId, roleInfo = new PlayerGameInfo());

            if (player.Data.Role.IsImpostor)
                roleInfo.roles.Add(RoleInfo.impostor);
            else
                roleInfo.roles.Add(RoleInfo.crewmate);

            return roleInfo.roles;
        }

        public static List<RoleInfo> getRoles(NetworkedPlayerInfo player)
        {
            if (mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
                return roleInfo.roles;

            mapping.Add(player.PlayerId, roleInfo = new PlayerGameInfo());

            if (player.Role.IsImpostor)
                roleInfo.roles.Add(RoleInfo.impostor);
            else
                roleInfo.roles.Add(RoleInfo.crewmate);

            return roleInfo.roles;
        }

        public static void addRole(byte playerId, RoleInfo role)
        {
            if (!mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                mapping.Add(playerId, gameInfo = new PlayerGameInfo());

            if (!gameInfo.roles.Any(r => r.roleId == role.roleId))
                gameInfo.roles.Add(role);
        }

        public static List<RoleInfo> getModifiers(byte playerId)
        {
            if (!mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                mapping.Add(playerId, gameInfo = new PlayerGameInfo());
            return gameInfo.modifiers;
        }

        public static void addModifier(byte playerId, RoleInfo role)
        {
            if (!mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo))
                mapping.Add(playerId, gameInfo = new PlayerGameInfo());

            if (!gameInfo.modifiers.Any(r => r.roleId == role.roleId))
                gameInfo.modifiers.Add(role);
        }

        public static void clearAndReload()
        {
            mapping.Clear();
        }
    }
}