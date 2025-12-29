using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Immovable : ModifierBase<Immovable>
    {
        public static Color color = Color.yellow;

        public Vector3 position;
        public Immovable()
        {
            ModType = modId = RoleId.Immovable;
        }

        public override void OnMeetingStart()
        {
            position = PlayerControl.LocalPlayer.transform.position;
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm) { }
        public static void setButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<Immovable>();
        }

        public void setPosition()
        {
            if (position == Vector3.zero) return;  // Check if this has been set, otherwise first spawn on submerged will fail
            if (PlayerControl.LocalPlayer == player)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
            }
        }
    }
}