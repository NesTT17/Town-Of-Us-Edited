using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class DoubleShot : ModifierBase<DoubleShot>
    {
        public static Color color = Palette.ImpostorRed;

        public bool usedExtraLife;
        public DoubleShot()
        {
            ModType = modId = RoleId.DoubleShot;
            usedExtraLife = false;
        }

        public override void OnMeetingStart() { }
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
            players = new List<DoubleShot>();
        }
    }
}