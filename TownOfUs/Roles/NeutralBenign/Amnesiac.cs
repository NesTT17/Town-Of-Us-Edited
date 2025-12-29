using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Amnesiac : RoleBase<Amnesiac>
    {
        private static CustomButton amnesiacRememberButton;
        public static Color color = new Color(0.5f, 0.7f, 1f, 1f);
        private static Sprite rememberButton;
        private static Sprite rememberMeetingButton;

        public static bool showArrows { get => CustomOptionHolder.amnesiacShowArrows.getBool(); }
        public static float delay { get => CustomOptionHolder.amnesiacDelay.getFloat(); }
        public static bool resetRole { get => CustomOptionHolder.amnesiacResetRoleWhenTaken.getBool(); }
        public static bool rememberMeeting { get => CustomOptionHolder.amnesiacCanRememberOnMeetingIfGuesser.getBool(); }

        public List<Arrow> localArrows = new List<Arrow>();
        public Amnesiac()
        {
            RoleType = roleId = RoleId.Amnesiac;
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
            if (PlayerControl.LocalPlayer != player || localArrows == null || !showArrows) return;
            if (player.Data.IsDead) {
                foreach (Arrow arrow in localArrows) Object.Destroy(arrow.arrow);
                localArrows = [];
                return;
            }

            DeadBody[] deadBodies = Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = localArrows.Count != deadBodies.Count();
            int index = 0;
            
            if (arrowUpdate) {
                foreach (Arrow arrow in localArrows) Object.Destroy(arrow.arrow);
                localArrows = [];
            }

            foreach (DeadBody db in deadBodies) {
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

        public static void makeButtons(HudManager hm)
        {
            amnesiacRememberButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, Helpers.playerById(playerInfo.PlayerId));
                                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, Helpers.playerById(playerInfo.PlayerId));

                                    if (local.localArrows != null)
                                    {
                                        foreach (Arrow arrow in local.localArrows)
                                            if (arrow?.arrow != null)
                                                UnityEngine.Object.Destroy(arrow.arrow);
                                    }
                                    local.localArrows = new List<Arrow>();

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AmnesiacRemember, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.amnesiacRemember(playerInfo.PlayerId, PlayerControl.LocalPlayer.PlayerId);

                                    amnesiacRememberButton.Timer = amnesiacRememberButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Amnesiac) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return hm.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { amnesiacRememberButton.Timer = amnesiacRememberButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            amnesiacRememberButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<Amnesiac>();
        }

        public static Sprite getButtonSprite()
            => rememberButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RememberButton.png", 100f);
            
        public static Sprite getMeetingButtonSprite()
            => rememberMeetingButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MeetingRemember.png", 100f);
    }
}