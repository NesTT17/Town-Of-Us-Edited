using UnityEngine;

namespace TownOfUs.Utilities
{
    public static class HandleGuesser
    {
        private static Sprite targetSprite;
        public static bool isGuesserGm = false;
        public static int tasksToUnlock = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewGuesserNumberOfTasks.getFloat());

        public static Sprite getTargetSprite()
        {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static bool isGuesser(byte playerId)
        {
            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer.PlayerId == playerId) return true;
            return isGuesserGm ? GuesserGM.isGuesser(playerId) : Guesser.isGuesser(playerId);
        }

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer.PlayerId == playerId) return 100;
            return isGuesserGm ? GuesserGM.remainingShots(playerId, shoot) : Guesser.remainingShots(playerId, shoot);
        }

        public static void clearAndReload()
        {
            Guesser.clearAndReload();
            GuesserGM.clearAndReload();
            isGuesserGm = gameMode == CustomGamemodes.Guesser;
        }
    }
}