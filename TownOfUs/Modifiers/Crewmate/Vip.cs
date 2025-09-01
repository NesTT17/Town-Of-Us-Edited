using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Vip : ModifierBase<Vip>
    {
        public static Color color = Palette.CrewmateBlue;

        public Vip()
        {
            ModType = modId = RoleId.Vip;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Vip>();
        }
    }
}