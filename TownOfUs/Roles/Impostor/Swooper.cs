using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Swooper : RoleBase<Swooper>
    {
        private static CustomButton swooperButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite swoopButton;

        public static float cooldown { get => CustomOptionHolder.swooperCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.swooperDuration.getFloat(); }

        public float swoopTimer = 0f;
        public bool isInvisble = false;
        public Swooper()
        {
            RoleType = roleId = RoleId.Swooper;
            swoopTimer = 0f;
            isInvisble = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            swoopTimer -= Time.deltaTime;
            if (isInvisble && swoopTimer <= 0 && player == PlayerControl.LocalPlayer)
            {
                MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                invisibleWriter.Write(player.PlayerId);
                invisibleWriter.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                RPCProcedure.swooperSwoop(player.PlayerId, byte.MaxValue);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            swooperButton = new CustomButton(
                () =>
                {
                    MessageWriter invisibleWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwooperSwoop, Hazel.SendOption.Reliable, -1);
                    invisibleWriter.Write(local.player.PlayerId);
                    invisibleWriter.Write(byte.MinValue);
                    AmongUsClient.Instance.FinishRpcImmediately(invisibleWriter);
                    RPCProcedure.swooperSwoop(local.player.PlayerId, byte.MinValue);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Swooper) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    swooperButton.Timer = swooperButton.MaxTimer;
                    swooperButton.isEffectActive = false;
                    swooperButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F,
                true, duration, () => { swooperButton.Timer = swooperButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        { 
            swooperButton.MaxTimer = cooldown;
            swooperButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Swooper>();
        }

        public static Sprite getButtonSprite()
            => swoopButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SwoopButton.png", 100f);
    }
}