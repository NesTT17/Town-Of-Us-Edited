using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Bait : ModifierBase<Bait>
    {
        public static Color color = Palette.CrewmateBlue;

        public static bool showKillFlash { get => CustomOptionHolder.modifierBaitShowKillFlash.getBool(); }

        public bool reported = true;
        public float reportDelay = 0f;
        public Bait()
        {
            ModType = modId = RoleId.Bait;
            reportDelay = CustomOptionHolder.modifierBaitReportDelay.getFloat();
            reported = false;
        }

        public override void OnMeetingStart()
        {
            reported = true;
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            if (PlayerControl.LocalPlayer.Data.IsDead && !reported)
            {
                reportDelay -= Time.fixedDeltaTime;
                DeadPlayer deadPlayer = GameHistory.deadPlayers?.Where(x => x.player?.PlayerId == player.PlayerId)?.FirstOrDefault();
                if (deadPlayer.killerIfExisting != null && reportDelay <= 0f)
                {
                    Helpers.handleVampireKillOnBodyReport();

                    byte reporter = deadPlayer.killerIfExisting.PlayerId;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, SendOption.Reliable, -1);
                    writer.Write(reporter);
                    writer.Write(player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedCmdReportDeadBody(reporter, player.PlayerId);
                    reported = true;
                }
            }
        }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null)
            {
                reported = false;
                if (showKillFlash && killer != player && killer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.isFlashedByGrenadier())
                    Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
            }
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }


        public static void Clear()
        {
            players = new List<Bait>();
        }

    }
}