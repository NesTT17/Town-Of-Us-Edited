using System.Collections.Generic;

namespace TownOfUs
{
    static class TOUMapOptions {
        // Set values
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeModifier = true;
        public static bool ghostsSeeInformation = true;
        public static bool ghostsSeeVotes = true;
        public static bool showLighterDarker = true;
        public static bool showChatNotifications = true;
        public static bool extendedColorblindMode = false;
        public static CustomGamemodes gameMode = CustomGamemodes.Classic;

        public static bool camoComms = false;
        public static bool hidePlayerNames = false;
        public static bool allowParallelMedBayScans = false;
        public static bool shieldFirstKill = false;
        public static int blockSkippingInEmergencyMeetings = 0; // 0 - off, 1 - emergency, 2 - always
        
        // Updating values
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;

        public static void clearAndReloadMapOptions() {
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            camoComms = CustomOptionHolder.camoComms.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            firstKillPlayer = null;
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getSelection();
        }

        public static void reloadPluginOptions() {
            ghostsSeeRoles = TownOfUsPlugin.GhostsSeeRoles.Value;
            ghostsSeeModifier = TownOfUsPlugin.GhostsSeeModifier.Value;
            ghostsSeeInformation = TownOfUsPlugin.GhostsSeeInformation.Value;
            ghostsSeeVotes = TownOfUsPlugin.GhostsSeeVotes.Value;
            showLighterDarker = TownOfUsPlugin.ShowLighterDarker.Value;
            showChatNotifications = TownOfUsPlugin.ShowChatNotifications.Value;
            extendedColorblindMode = TownOfUsPlugin.ExtendedColorblindMode.Value;
        }
    }
}