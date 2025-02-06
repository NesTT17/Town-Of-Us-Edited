using TownOfUs.CustomGameModes;
using UnityEngine;

namespace TownOfUs.Utilities
{
    public static class HandleGuesser {
        public static bool isGuesserGm = false;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool killsThroughShield = true;
        public static bool guesserCantGuessSnitch = false;
        
        private static Sprite targetSprite;
        public static Sprite getTargetSprite()
            => targetSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.TargetIcon.png", 150f);
        
        public static bool isGuesser(byte playerId) {
            if (isGuesserGm) return GuesserGM.isGuesser(playerId);
            return Guesser.isGuesser(playerId);
        }

        public static void clear(byte playerId) {
            if (isGuesserGm) GuesserGM.clear(playerId);
            else Guesser.clear(playerId);
        }

        public static int remainingShots(byte playerId, bool shoot = false) {
            if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
            return Guesser.remainingShots(playerId, shoot);
        }

        public static void clearAndReload() {
            Guesser.clearAndReload();
            GuesserGM.clearAndReload();
            isGuesserGm = TOUMapOptions.gameMode == CustomGamemodes.Guesser;
            if (isGuesserGm) {
                guesserCantGuessSnitch = CustomOptionHolder.guesserGamemodeCantGuessSnitchIfTaksDone.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserGamemodeHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserGamemodeKillsThroughShield.getBool();
            } else {
                guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserKillsThroughShield.getBool();
            }
        }
    }
}