using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Jester : RoleBase<Jester>
    {
        public static Color color = new Color(1f, 0.75f, 0.8f, 1f);

        public static bool canCallEmergency { get => CustomOptionHolder.jesterCanCallEmergency.getBool(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.jesterHasImpostorVision.getBool(); }
        
        public bool triggerJesterWin = false;
        public Jester()
        {
            RoleType = roleId = RoleId.Jester;
            triggerJesterWin = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            if (players != null) players.Do(x => x.triggerJesterWin = false);
            players = new List<Jester>();
        }
    }
}