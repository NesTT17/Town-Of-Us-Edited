using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Radar : ModifierBase<Radar>
    {
        public static Color color = Color.yellow;

        public Arrow localArrow;
        public Radar()
        {
            ModType = modId = RoleId.Radar;
            if (localArrow?.arrow != null)
                Object.Destroy(localArrow.arrow);
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate()
        {
            if (player == null || PlayerControl.LocalPlayer != player || MeetingHud.Instance)
                return;
            if (player.Data.IsDead)
            {
                if (localArrow.arrow != null)
                    Object.Destroy(localArrow.arrow);
                localArrow = null;
                return;
            }

            PlayerControl closestPlayer = null;
            float closestDistance = float.MaxValue;
            Vector2 refPosition = PlayerControl.LocalPlayer.GetTruePosition();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player.PlayerId == PlayerControl.LocalPlayer.PlayerId || !player.Collider.enabled)
                    continue;

                float distance = Vector2.Distance(refPosition, player.GetTruePosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                if (localArrow == null)
                {
                    localArrow = new Arrow(color);
                    localArrow.arrow.SetActive(true);
                }
                localArrow.Update(closestPlayer.transform.position);
            }
            else
            {
                if (localArrow != null && localArrow.arrow != null)
                {
                    localArrow.arrow.SetActive(false);
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Radar>();
        }
    }
}