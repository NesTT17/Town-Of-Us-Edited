using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
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
public static class PlayerPhysicsFixedUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        bool shouldInvert = Drunk.drunk != null && PlayerControl.LocalPlayer == Drunk.drunk;
        if (__instance.AmOwner &&
            AmongUsClient.Instance &&
            AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
            !PlayerControl.LocalPlayer.Data.IsDead &&
            shouldInvert &&
            GameData.Instance &&
            __instance.myPlayer.CanMove) __instance.body.velocity *= -1;
    }
}

[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
public static class SpeedPatch {
    public static void Postfix(CustomNetworkTransform __instance) {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return;
        
        venererUpdate(__instance);
    }

    static void venererUpdate(CustomNetworkTransform __instance) {
        if (Venerer.venerer != null) {
            if (Venerer.venerer == __instance.gameObject.GetComponent<PlayerControl>() && !__instance.AmOwner && Venerer.numberOfKills >= 2 && Venerer.morphTimer > 0f)
                __instance.body.velocity *= Venerer.speedMultiplier;
            if (Venerer.venerer != __instance.gameObject.GetComponent<PlayerControl>() && !__instance.AmOwner && Venerer.numberOfKills >= 3 && Venerer.morphTimer > 0f)
                __instance.body.velocity *= Venerer.freezeMultiplier;
        }
    }
}