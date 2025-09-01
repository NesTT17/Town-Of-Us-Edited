using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Executioner : RoleBase<Executioner>
    {
        public static Color color = new Color(0.55f, 0.25f, 0.02f, 1f);

        public static float vision { get => CustomOptionHolder.executionerVision.getFloat(); }
        public static bool canCallEmergency { get => CustomOptionHolder.executionerCanCallEmergency.getBool(); }

        public static PlayerControl target;
        public static bool triggerExecutionerWin = false;
        public static bool targetWasGuessed = false;
        public Executioner()
        {
            RoleType = roleId = RoleId.Executioner;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            // Promote to Amnesiac
            if (target != null && target.Data.Disconnected && !player.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToAmnesiac, SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerPromotesToAmnesiac(player.PlayerId);
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
            triggerExecutionerWin = false;
            players = new List<Executioner>();
        }
    }
}