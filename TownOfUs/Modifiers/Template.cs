using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Template : ModifierBase<Template>
    {
        public static Color color = Palette.CrewmateBlue;

        public Template()
        {
            ModType = modId = RoleId.NoRole;
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
            players = new List<Template>();
        }

    }
}