using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Swapper : RoleBase<Swapper>
    {
        public static Color color = new Color(0.4f, 0.9f, 0.4f, 1f);
        private static Sprite spriteCheck;

        public static bool canCallEmergency { get => CustomOptionHolder.swapperCanCallEmergency.getBool(); }
        public static bool canOnlySwapOthers { get => CustomOptionHolder.swapperCanOnlySwapOthers.getBool(); }

        public static byte playerId1 = byte.MaxValue;
        public static byte playerId2 = byte.MaxValue;

        public Swapper()
        {
            RoleType = roleId = RoleId.Swapper;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            playerId1 = byte.MaxValue;
            playerId2 = byte.MaxValue;
            players = new List<Swapper>();
        }

        public static Sprite getCheckSprite()
            => spriteCheck ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwapperCheck.png", 150f);
    }
}