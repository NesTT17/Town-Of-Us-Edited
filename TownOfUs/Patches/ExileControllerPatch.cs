using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch
    {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
        {
            // Shifter shift
            if (Shifter.allPlayers.Count > 0 && AmongUsClient.Instance.AmHost && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                PlayerControl oldShifter = Shifter.allPlayers.FirstOrDefault();

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer?.Object);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer?.Object);
            }
        }

        static void WrapUpPostfix(PlayerControl exiled)
        {
            // Jester win condition
            if (exiled != null && Jester.exists && exiled.isRole(RoleId.Jester))
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JesterWin, SendOption.Reliable, -1);
                writer.Write(exiled.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.jesterWin(exiled.PlayerId);
            }
            // Executioner win condition
            else if (exiled != null && Executioner.livingPlayers.Count > 0 && exiled.PlayerId == Executioner.target.PlayerId)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerWin, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerWin();
            }

            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Custom role post-meeting functions
            OnMeetingEnd();

            // Clear bugged dead bodies
            MessageWriter cleanWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SystemCleanBody, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(cleanWriter);
            RPCProcedure.systemCleanBody();

            if (CustomOptionHolder.randomSpawnPositions.getSelection() == 2)
            {
                RPCProcedure.systemSpreadPlayers();
            }

            // Immovable set position
            if (PlayerControl.LocalPlayer.hasModifier(RoleId.Immovable))
            {
                var immovableModifier = ModifierBase<Immovable>.getModifier(PlayerControl.LocalPlayer);
                immovableModifier.setPosition();
            }

            Chameleon.local.lastMoved.Clear();
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]  // Set position of AntiTp players AFTER they have selected a spawn.
    class AirshipSpawnInPatch
    {
        static void Postfix()
        {
            Immovable.local.setPosition();
            Chameleon.local.lastMoved.Clear();
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.initData != null)
                {
                    PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.name).ToArray());
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS)
                    {
                        if (player.isRole(RoleId.Jester)) __result = "";
                    }
                    if (Tiebreaker.isTiebreak) __result += " (Tiebreaker)";
                    Tiebreaker.isTiebreak = false;
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}