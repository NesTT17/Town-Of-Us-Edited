using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Sleuth : ModifierBase<Sleuth>
    {
        public static Color color = Color.yellow;

        public List<PlayerControl> reported;
        public Sleuth()
        {
            ModType = modId = RoleId.Sleuth;
            reported = new List<PlayerControl>();
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
            players = new List<Sleuth>();
        }

    }
}