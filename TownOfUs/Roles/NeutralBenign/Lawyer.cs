using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Lawyer : RoleBase<Lawyer>
    {
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);

        public static bool targetKnows { get => CustomOptionHolder.lawyerTargetKnows.getBool(); }
        public static bool winsAfterMeetings = true;
        public static int neededMeetings { get => Mathf.RoundToInt(CustomOptionHolder.lawyerNeededMeetings.getFloat()); }
        public static float vision { get => CustomOptionHolder.lawyerVision.getFloat(); }
        public static bool lawyerKnowsRole { get => CustomOptionHolder.lawyerKnowsRole.getBool(); }
        public static bool targetCanBeJester = false;

        public static int meetings = 0;
        public static PlayerControl target;
        public static bool triggerLawyerWin = false;
        public static bool targetWasGuessed = false;
        public Lawyer()
        {
            RoleType = roleId = RoleId.Lawyer;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        { 
            if (PlayerControl.LocalPlayer == player && !player.Data.IsDead)
                meetings++;
        }
        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;

            // Meeting win
            if (winsAfterMeetings && neededMeetings == meetings && target != null && !target.Data.IsDead)
            {
                winsAfterMeetings = false; // Avoid sending mutliple RPCs until the host finshes the game
                MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerWin, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                RPCProcedure.lawyerWin();
                return;
            }

            // Promote to Pursuer
            if (target != null && target.Data.Disconnected && !player.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer(player.PlayerId);
                return;
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            player.clearAllTasks();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player) target = null;
        }
        public override void ResetRole(bool isShifted)
        {
            if (target != null && player == PlayerControl.LocalPlayer) {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void clearTarget()
        {
            target = null;
            targetWasGuessed = false;
        }

        public static void Clear()
        {
            clearTarget();
            triggerLawyerWin = false;
            meetings = 0;
            winsAfterMeetings = CustomOptionHolder.lawyerWinsAfterMeetings.getBool();
            players = new List<Lawyer>();
            targetCanBeJester = false;
        }
    }
}