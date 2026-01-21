using System.Collections.Generic;
using System.Linq;

namespace TownOfUs
{
    public enum InfoType
    {
        AddKill,
        AddCorrectShot,
        AddMisfire,
        AddCorrectGuess,
        AddIncorrectGuess,
        AddAbilityKill,
        AddEat,
        AddClean,
    }

    public class PlayerGameInfo
    {
        public static readonly Dictionary<byte, PlayerGameInfo> Mapping = new();
        public int Kills = 0;
        public int CorrectGuesses = 0;
        public int IncorrectGuesses = 0;
        public int CorrectShots = 0;
        public int Misfires = 0;
        public int AbilityKills = 0;
        public int Eats = 0;
        public int Cleans = 0;
        public readonly List<RoleInfo> Roles = new();
        public readonly List<RoleInfo> Modifiers = new();

        public static int TotalKills(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.Kills : 0;
        }

        public static int TotalCorrectShots(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.CorrectShots : 0;
        }

        public static int TotalMisfires(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.Misfires : 0;
        }

        public static int TotalCorrectGuesses(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.CorrectGuesses : 0;
        }

        public static int TotalIncorrectGuesses(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.IncorrectGuesses : 0;
        }

        public static int TotalAbilityKills(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.AbilityKills : 0;
        }

        public static int TotalEaten(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.Eats : 0;
        }

        public static int TotalCleaned(byte playerId)
        {
            return Mapping.TryGetValue(playerId, out PlayerGameInfo gameInfo) ? gameInfo.Cleans : 0;
        }

        public static List<RoleInfo> GetRoles(PlayerControl player) => GetRoles(player.Data);
        public static List<RoleInfo> GetRoles(NetworkedPlayerInfo player)
        {
            if (Mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
            {
                if (roleInfo.Roles?.Count != 0)
                    return roleInfo.Roles;
            }
            else
            {
                Mapping.Add(player.PlayerId, roleInfo = new PlayerGameInfo());
            }

            if (player.Role.IsImpostor)
                roleInfo.Roles.Add(RoleInfo.impostor);
            else
                roleInfo.Roles.Add(RoleInfo.crewmate);

            return roleInfo.Roles;
        }

        public static void EraseHistory(PlayerControl player)
        {
            if (Mapping.TryGetValue(player.PlayerId, out PlayerGameInfo roleInfo))
                roleInfo.Roles.Clear();
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