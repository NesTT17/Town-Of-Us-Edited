using UnityEngine;

namespace TownOfUs.Utilities
{
    public static class HandleGuesser
    {
        private static Sprite targetSprite;
        public static bool isGuesserGm = false;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool killsThroughShield = true;
        public static bool guesserCantGuessSnitch = false;
        public static bool evilGuesserCanGuessAgent = true;
        public static int tasksToUnlock = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewGuesserNumberOfTasks.getFloat());

        public static Sprite getTargetSprite()
        {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static bool isGuesser(byte playerId)
        {
            if (isGuesserGm) return GuesserGM.isGuesser(playerId);
            return false;
        }

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
            return 0;
        }

        public static void clearAndReload() {
            GuesserGM.clearAndReload();
            isGuesserGm = gameMode == CustomGamemodes.Guesser;
            if (isGuesserGm)
            {
                guesserCantGuessSnitch = CustomOptionHolder.guesserGamemodeCantGuessSnitchIfTaksDone.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserGamemodeHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserGamemodeKillsThroughShield.getBool();
                tasksToUnlock = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewGuesserNumberOfTasks.getFloat());
                evilGuesserCanGuessAgent = CustomOptionHolder.guesserGamemodeEvilCanKillAgent.getBool();
            }
            else
            {
                guesserCantGuessSnitch = false;
                hasMultipleShotsPerMeeting = false;
                killsThroughShield = false;
                evilGuesserCanGuessAgent = true;
            }
        }
    }
}