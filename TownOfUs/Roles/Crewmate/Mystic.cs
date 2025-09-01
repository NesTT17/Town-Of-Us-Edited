using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Mystic : RoleBase<Mystic>
    {
        public static Color color = new Color(0.3f, 0.6f, 0.9f, 1f);

        public static float duration { get => CustomOptionHolder.mysticArrowDuration.getFloat(); }

        public List<Arrow> localArrows = new List<Arrow>();
        public Mystic()
        {
            RoleType = roleId = RoleId.Mystic;
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player || localArrows == null) return;
            if (player.Data.IsDead) {
                foreach (Arrow arrow in localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                localArrows = new List<Arrow>();
                return;
            }

            var validBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().Where(x => GameHistory.deadPlayers.Any(y => y.player.PlayerId == x.ParentId && y.timeOfDeath.AddSeconds(duration) > System.DateTime.UtcNow));
            bool arrowUpdate = localArrows.Count != validBodies.Count();
            int index = 0;

            if (arrowUpdate) {
                foreach (Arrow arrow in localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in validBodies) {
                if (arrowUpdate) {
                    localArrows.Add(new Arrow(Palette.EnabledColor));
                    localArrows[index].arrow.SetActive(true);
                }
                if (localArrows[index] != null) localArrows[index].Update(db.transform.position);
                index++;
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm) { }
        public static void setButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<Mystic>();
        }
    }
}