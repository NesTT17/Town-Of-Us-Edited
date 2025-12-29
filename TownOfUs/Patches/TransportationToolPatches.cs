using System;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    public static class TransportationToolPatches
    {
        /* 
         * Moving Plattform / Zipline / Ladders move the player out of bounds, thus we want to disable functions of the mod if the player is currently using one of these.
         * Save the players anti tp position before using it.
         * 
         * Zipline can also break camo, fix that one too.
         */

        public static bool isUsingTransportation(PlayerControl pc)
        {
            return pc.inMovingPlat || pc.onLadder;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), new Type[] { typeof(PlayerControl), typeof(bool) })]
        public static void prefix3(ZiplineBehaviour __instance, PlayerControl player, bool fromTop)
        {
            Immovable.local.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), [typeof(PlayerControl), typeof(bool)])]
        public static void postfix(ZiplineBehaviour __instance, PlayerControl player, bool fromTop)
        {
            // Fix camo:
            __instance.StartCoroutine(Effects.Lerp(fromTop ? __instance.downTravelTime : __instance.upTravelTime, new System.Action<float>((p) =>
            {
                HandZiplinePoolable hand;
                __instance.playerIdHands.TryGetValue(player.PlayerId, out hand);
                if (hand != null)
                {
                    if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive())
                    {
                        foreach (var morphling in Morphling.players)
                        {
                            if (player == morphling.player && morphling.morphTimer > 0)
                            {
                                hand.SetPlayerColor(morphling.morphTarget.CurrentOutfit, PlayerMaterial.MaskType.None, 1f);
                                // Also set hat color, cause the line destroys it...
                                player.RawSetHat(morphling.morphTarget.Data.DefaultOutfit.HatId, morphling.morphTarget.Data.DefaultOutfit.ColorId);
                            }
                        }
                        foreach (var glitch in Glitch.players)
                        {
                            if (player == glitch.player && Glitch.morphTimer > 0)
                            {
                                hand.SetPlayerColor(Glitch.morphPlayer.CurrentOutfit, PlayerMaterial.MaskType.None, 1f);
                                // Also set hat color, cause the line destroys it...
                                player.RawSetHat(Glitch.morphPlayer.Data.DefaultOutfit.HatId, Glitch.morphPlayer.Data.DefaultOutfit.ColorId);
                            }
                        }
                        hand.SetPlayerColor(player.CurrentOutfit, PlayerMaterial.MaskType.None, player.cosmetics.GetPhantomRoleAlpha());
                    }
                    else
                    {
                        PlayerMaterial.SetColors(6, hand.handRenderer);
                    }
                }
            })));
        }

        // Save the position of the player prior to starting the climb / gap platform
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void prefix()
        {
            Immovable.local.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void postfix2(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
        {
            // Fix camo:
            var player = __instance.myPlayer;
            __instance.StartCoroutine(Effects.Lerp(5.0f, new System.Action<float>((p) =>
            {
                if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive())
                {
                    foreach (var morphling in Morphling.players)
                    {
                        if (player == morphling.player && morphling.morphTimer > 0.1f)
                        {
                            player.RawSetHat(morphling.morphTarget.Data.DefaultOutfit.HatId, morphling.morphTarget.Data.DefaultOutfit.ColorId);
                        }
                    }
                    foreach (var glitch in Glitch.players)
                    {
                        if (player == glitch.player && Glitch.morphTimer > 0.1f)
                        {
                            player.RawSetHat(Glitch.morphPlayer.Data.DefaultOutfit.HatId, Glitch.morphPlayer.Data.DefaultOutfit.ColorId);
                        }
                    }
                }
            })));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
        public static void prefix2()
        {
            Immovable.local.position = PlayerControl.LocalPlayer.transform.position;
        }
    }
}