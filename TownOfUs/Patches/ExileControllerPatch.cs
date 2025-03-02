using HarmonyLib;
using System.Linq;
using System;
using Hazel;

namespace TownOfUs.Patches {
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)]ref NetworkedPlayerInfo exiled) {
            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;

            // Miner Vents
            if (Miner.miner != null && MinerVent.hasLimitReached())
                MinerVent.ConvertToVents();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch {
            public static void Postfix(ExileController __instance) {
                WrapUpPostfix(__instance.initData.networkedPlayer?.Object);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch {
            public static void Postfix(AirshipExileController __instance) {
                WrapUpPostfix(__instance.initData.networkedPlayer?.Object);
            }
        }

        static void WrapUpPostfix(PlayerControl exiled) {
            // Jester win condition
            if (exiled != null && Jester.jester != null && Jester.jester.PlayerId == exiled.PlayerId) {
                Jester.triggerJesterWin = true;
            }
            // Executioner win condition
            else if (exiled != null && Executioner.executioner != null && Executioner.target != null && Executioner.target.PlayerId == exiled.PlayerId && !Executioner.executioner.Data.IsDead) {
                Executioner.triggerExecutionerWin = true;
            }

            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Executioner promotion trigger
            if (Executioner.executioner != null && Executioner.target != null && !Executioner.executioner.Data.IsDead && Executioner.targetWasGuessed) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotesToPursuer();
            }

            // Guardian Angel promotion trigger
            if (GuardianAngel.guardianAngel != null && GuardianAngel.target != null && !GuardianAngel.guardianAngel.Data.IsDead && GuardianAngel.targetWasGuessed) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.guardianAngelPromotes();
            }

            // Reset Mercenary Shield
            if (Mercenary.shielded != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryResetShield, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.mercenaryResetShield();
            }

            // Blackmailer reset blackmailed
            if (Blackmailer.blackmailed != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UnblackmailPlayer, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.unblackmailPlayer();
            }

            // Trapper Remove traps
            if (Trapper.trapper != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanTraps, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.cleanTraps();
            }

            // Tracker Remove tracks
            if (Tracker.tracker != null && Tracker.resetAfterMeeting) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TrackerResetTrack, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.trackerResetTrack();
            }

            // Detective reset examined
            if (Detective.detective != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveResetExamine, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.detectiveResetExamine();
            }

            // Force Bounty Hunter Bounty Update
            if (BountyHunter.bountyHunter != null && BountyHunter.bountyHunter == PlayerControl.LocalPlayer)
                BountyHunter.bountyUpdateTimer = 0f;
            
            // Vampire Hunter promotion trigger
            if (VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == PlayerControl.LocalPlayer) {
                int aliveVampires = PlayerControl.AllPlayerControls.ToArray().Where(x => x != null && !x.Data.Disconnected && !x.Data.IsDead && (x.PlayerId == Dracula.dracula.PlayerId || x.PlayerId == Vampire.vampire.PlayerId)).Count();
                if (aliveVampires == 0) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireHunterPromotes, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.vampireHunterPromotes();
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch {
        static void Postfix(ref string __result, [HarmonyArgument(0)]StringNames id) {
            try {
                if (ExileController.Instance != null && ExileController.Instance.initData != null) {
                    PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP) {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.name).ToArray());
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS) {
                        if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId) __result = "";
                    }
                    if (Tiebreaker.isTiebreak) __result += " (Tiebreaker)";
                    Tiebreaker.isTiebreak = false;
                }
            } catch {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}