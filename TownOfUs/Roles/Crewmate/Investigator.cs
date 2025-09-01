using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Investigator : RoleBase<Investigator>
    {
        public static Color color = new Color(0f, 0.7f, 0.7f, 1f);

        public static float footprintIntervall { get => CustomOptionHolder.investigatorFootprintIntervall.getFloat(); }
        public static float footprintDuration { get => CustomOptionHolder.investigatorFootprintDuration.getFloat(); }
        public static bool anonymousFootprints { get => CustomOptionHolder.investigatorAnonymousFootprints.getBool(); }

        public float timer = 6.2f;
        public Investigator()
        {
            RoleType = roleId = RoleId.Investigator;
            timer = 6.2f;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            if (player.Data.IsDead) return;

            timer -= Time.fixedDeltaTime;
            if (timer <= 0f)
            {
                timer = footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent)
                    {
                        FootprintHolder.Instance.MakeFootprint(player);
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Investigator>();
        }
    }
}