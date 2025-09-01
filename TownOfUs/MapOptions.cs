using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs
{
    static class TOUMapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool hidePlayerNames = false;
        public static bool allowParallelMedBayScans = false;
        public static bool shieldFirstKill = false;
        public static bool enableBetterPolus = false;

        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeModifier = true;
        public static bool ghostsSeeInformation = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool showLighterDarker = true;
        public static bool ShowChatNotifications = true;

        public static CustomGamemodes gameMode = CustomGamemodes.Classic;

        // Updating values
        public static int meetingsCount = 0;
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;

        public static void clearAndReloadMapOptions()
        {
            meetingsCount = 0;
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            enableBetterPolus = CustomOptionHolder.enableBetterPolus.getBool();
            firstKillPlayer = null;
        }

        public static void reloadPluginOptions()
        {
            ghostsSeeRoles = TownOfUsPlugin.GhostsSeeRoles.Value;
            ghostsSeeModifier = TownOfUsPlugin.GhostsSeeModifier.Value;
            ghostsSeeInformation = TownOfUsPlugin.GhostsSeeInformation.Value;
            ghostsSeeVotes = TownOfUsPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TownOfUsPlugin.ShowRoleSummary.Value;
            showLighterDarker = TownOfUsPlugin.ShowLighterDarker.Value;
            ShowChatNotifications = TownOfUsPlugin.ShowChatNotifications.Value;
        }

        public static void resetPoolables() {
            foreach (PoolablePlayer p in playerIcons.Values) {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
        }
    }
}