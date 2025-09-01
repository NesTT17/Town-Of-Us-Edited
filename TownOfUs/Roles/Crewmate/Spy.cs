using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Spy : RoleBase<Spy>
    {
        public static Color color = new Color(0.8f, 0.64f, 0.8f, 1f);

        public Spy()
        {
            RoleType = roleId = RoleId.Spy;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Spy>();
        }
    }
}