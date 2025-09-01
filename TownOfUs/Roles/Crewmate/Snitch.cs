using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Snitch : RoleBase<Snitch>
    {
        public static Color color = new Color(0.83f, 0.69f, 0.22f, 1f);

        public static int taskCountForReveal { get => Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat()); }
        public static bool includeBenignNeutral { get => CustomOptionHolder.snitchIncludeBenignNeutral.getBool(); }
        public static bool includeEvilNeutral { get => CustomOptionHolder.snitchIncludeEvilNeutral.getBool(); }
        public static bool includeKillingNeutral { get => CustomOptionHolder.snitchIncludeKillingNeutral.getBool(); }

        public static List<Arrow> localArrows = new List<Arrow>();
        public List<Arrow> snitchArrows = new List<Arrow>();
        public Snitch()
        {
            RoleType = roleId = RoleId.Snitch;
            if (snitchArrows != null)
            {
                foreach (Arrow arrow in snitchArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            snitchArrows = new List<Arrow>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (localArrows == null || snitchArrows == null) return;
            foreach (Arrow arrow in localArrows) arrow.arrow.SetActive(false);
            foreach (Arrow arrow in snitchArrows) arrow.arrow.SetActive(false);

            if (!(livingPlayers.Count > 0)) return;
            var local = PlayerControl.LocalPlayer;
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(player.Data);
            int numberOfTasks = playerTotal - playerCompleted;

            if (!local.Data.IsDead && (local.Data.Role.IsImpostor || local.isBenignNeutral() && includeBenignNeutral || local.isEvilNeutral() && includeEvilNeutral || local.isKillingNeutral() && includeKillingNeutral))
            {
                int arrowIndex = 0;
                foreach (var snitch in allPlayers)
                {
                    var (complete, total) = TasksHandler.taskInfo(snitch.Data);
                    int remaining = total - complete;
                    if (remaining > taskCountForReveal || snitch.Data.IsDead) continue;
                    if (arrowIndex >= localArrows.Count) localArrows.Add(new Arrow(Color.blue));
                    if (arrowIndex < localArrows.Count && localArrows[arrowIndex] != null)
                    {
                        localArrows[arrowIndex].arrow.SetActive(true);
                        localArrows[arrowIndex].Update(snitch.transform.position, Color.blue);
                    }
                    arrowIndex++;
                }
            }
            else if (local == player && numberOfTasks == 0)
            {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool arrowForImp = p.Data.Role.IsImpostor;
                    bool arrowForBNeut = p.isBenignNeutral() && includeBenignNeutral;
                    bool arrowForENeut = p.isEvilNeutral() && includeEvilNeutral;
                    bool arrowForKNeut = p.isKillingNeutral() && includeKillingNeutral;
                    Color color = (arrowForImp || arrowForKNeut) ? Palette.ImpostorRed : Color.gray;

                    if (!p.Data.IsDead && (arrowForImp || arrowForBNeut || arrowForENeut || arrowForKNeut))
                    {
                        if (arrowIndex >= snitchArrows.Count) snitchArrows.Add(new Arrow(color));
                        if (arrowIndex < snitchArrows.Count && snitchArrows[arrowIndex] != null)
                        {
                            snitchArrows[arrowIndex].arrow.SetActive(true);
                            snitchArrows[arrowIndex].Update(p.transform.position, color);
                        }
                        arrowIndex++;
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (player == this.player)
            {
                if (localArrows != null)
                    foreach (Arrow arrow in localArrows)
                        if (arrow?.arrow != null)
                            UnityEngine.Object.Destroy(arrow.arrow);
                localArrows = [];
            }
        }

        public static void Clear()
        {
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            players = new List<Snitch>();
        }
    }
}