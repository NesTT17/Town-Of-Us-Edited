using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Janitor : RoleBase<Janitor>
    {
        public static CustomButton janitorButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite cleanButton;

        public static float cooldown { get => CustomOptionHolder.janitorCooldown.getFloat(); }

        public Janitor()
        {
            RoleType = roleId = RoleId.Janitor;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && janitorButton != null)
                janitorButton.Timer = janitorButton.MaxTimer;
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            janitorButton = new CustomButton(
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

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.cleanBody(playerInfo.PlayerId, PlayerControl.LocalPlayer.PlayerId);

                                    PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                                    janitorButton.Timer = janitorButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Janitor) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return hm.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { janitorButton.Timer = janitorButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            janitorButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Janitor>();
        }

        public static Sprite getButtonSprite()
            => cleanButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CleanButton.png", 100f);
    }
}