using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Agent : RoleBase<Agent>
    {
        public static Color color = Palette.ImpostorRed;

        public static bool impostorsCanKillAnyone { get => CustomOptionHolder.agentImpostorsCanKillAnyone.getBool(); }
        public static bool canEnterVents { get => CustomOptionHolder.agentCanEnterVents.getBool(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.agentHasImpostorVision.getBool(); }

        public Agent()
        {
            RoleType = roleId = RoleId.Agent;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Agent>();
        }
    }
}