using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs
{
    public static class TOUEdMapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static BlockOptions blockSkippingInEmergencyMeetings = BlockOptions.Off;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool allowParallelMedBayScans = false;
        public static bool killPlayersInVent = false;
        public static bool abilityPlayersInVent = false;
        public static bool shieldFirstKill = false;
        public static CustomGamemodes gameMode = CustomGamemodes.Classic;

        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeModifier = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeInformation = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool enableSoundEffects = true;
        public static bool showVentsOnMap = true;
        public static bool showChatNotifications = true;
        
        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;

        public static void clearAndReloadMapOptions()
        {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>(); ;

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getFloat());
            blockSkippingInEmergencyMeetings = (BlockOptions)CustomOptionHolder.blockSkippingInEmergencyMeetings.getSelection();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            killPlayersInVent = CustomOptionHolder.killPlayersInVent.getBool();
            abilityPlayersInVent = CustomOptionHolder.abilityPlayersInVent.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            firstKillPlayer = null;
        }

        public static void reloadPluginOptions()
        {
            ghostsSeeRoles = TownOfUsPlugin.GhostsSeeRoles.Value;
            ghostsSeeModifier = TownOfUsPlugin.GhostsSeeModifier.Value;
            ghostsSeeTasks = TownOfUsPlugin.GhostsSeeTasks.Value;
            ghostsSeeInformation = TownOfUsPlugin.GhostsSeeInformation.Value;
            ghostsSeeVotes = TownOfUsPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TownOfUsPlugin.ShowRoleSummary.Value;
            enableSoundEffects = TownOfUsPlugin.EnableSoundEffects.Value;
            showVentsOnMap = TownOfUsPlugin.ShowVentsOnMap.Value;
            showChatNotifications = TownOfUsPlugin.ShowChatNotifications.Value;
        }
    }
}