using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Blind : ModifierBase<Blind>
    {
        public static Color color = Palette.CrewmateBlue;

        public Blind()
        {
            ModType = modId = RoleId.Blind;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
                DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Blind>();
        }
    }
}