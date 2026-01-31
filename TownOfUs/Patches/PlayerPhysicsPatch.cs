using System;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.Awake))]
public static class PlayerPhysiscs_Awake_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerPhysics __instance)
    {
        if (!__instance.body) return;
        __instance.body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        updateUndertakerMoveSpeed(__instance);
        updateVenererMoveSpeed(__instance);
        updateFlashMoveSpeed(__instance);
        updateFrostyChillSpeed(__instance);
        updateBansheeScaredPlayerSpeed(__instance);
    }

    static void updateUndertakerMoveSpeed(PlayerPhysics playerPhysics)
    {
        if (Undertaker.undertaker == null || Undertaker.undertaker != PlayerControl.LocalPlayer) return;
        if (Undertaker.dragedBody != null)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                playerPhysics.body.velocity *= Undertaker.velocity;
        }
    }

    static void updateVenererMoveSpeed(PlayerPhysics playerPhysics)
    {
        if (Venerer.venerer == null || Venerer.numberOfKills < 2) return;
        if (Venerer.numberOfKills >= 2 && PlayerControl.LocalPlayer == Venerer.venerer)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove && Venerer.abilityTimer > 0f)
                playerPhysics.body.velocity *= Venerer.speedMultiplier;
        }
        if (Venerer.numberOfKills >= 3 && PlayerControl.LocalPlayer != Venerer.venerer && !PlayerControl.LocalPlayer.Data.IsDead && Venerer.abilityTimer > 0f)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                playerPhysics.body.velocity *= Venerer.freezeMultiplier;
        }
    }

    static void updateFlashMoveSpeed(PlayerPhysics playerPhysics)
    {
        if (Flash.flash != null && Flash.flash.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId) && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                playerPhysics.body.velocity *= Flash.speed;
        }
    }

    static void updateFrostyChillSpeed(PlayerPhysics playerPhysics)
    {
        if (Frosty.chilled != null && Frosty.chilled == PlayerControl.LocalPlayer && Frosty.isChilled)
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - Frosty.lastChilled;
            var duration = Frosty.duration * 1000f;
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
            {
                if ((float)timeSpan.TotalMilliseconds < duration)
                {
                    playerPhysics.body.velocity *= 1 - (duration - (float)timeSpan.TotalMilliseconds) * (1 - Frosty.startSpeed) / duration;
                }
                else
                {
                    Frosty.isChilled = false;
                }
            }
        }
    }

    static void updateBansheeScaredPlayerSpeed(PlayerPhysics playerPhysics)
    {
        if (Banshee.banshee != null && Banshee.scareVictim != null && Banshee.scareVictim == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && Banshee.scareTimer > 0f)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                playerPhysics.body.velocity *= 0f;
        }
    }
}