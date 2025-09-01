using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Blackmailer : RoleBase<Blackmailer>
    {
        private static CustomButton blackmailerButton;
        public static Color color = Palette.ImpostorRed;
        public static Color blackmailedColor = new Color(0.3f, 0f, 0f);
        private static Sprite overlaySprite;
        private static Sprite blackmailButton;

        public static float cooldown { get => CustomOptionHolder.blackmailerCooldown.getFloat(); }
        public static bool blockTargetVote { get => CustomOptionHolder.blackmailerBlockTargetVote.getBool(); }
        public static bool blockTargetAbility { get => CustomOptionHolder.blackmailerBlockTargetAbility.getBool(); }

        public static bool alreadyShook = false;

        public PlayerControl blackmailed;
        public PlayerControl currentTarget;
        public Blackmailer()
        {
            RoleType = roleId = RoleId.Blackmailer;
            currentTarget = null;
            blackmailed = null;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            blackmailed = null;
            alreadyShook = false;
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget(untargetablePlayers: new List<PlayerControl>() { blackmailed });
                Helpers.setPlayerOutline(currentTarget, blackmailedColor);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            blackmailerButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BlackmailPlayer, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.blackmailPlayer(local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                    blackmailerButton.Timer = blackmailerButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Blackmailer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { blackmailerButton.Timer = blackmailerButton.MaxTimer; },
                getBlackmailButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            blackmailerButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            alreadyShook = false;
            players = new List<Blackmailer>();
        }
        
        public static Sprite getBlackmailOverlaySprite()
            => overlaySprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailOverlay.png", 100f);
        public static Sprite getBlackmailButtonSprite()
            => blackmailButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailButton.png", 100f);
    }
}