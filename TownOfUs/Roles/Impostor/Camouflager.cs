using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Camouflager : RoleBase<Camouflager>
    {
        private static CustomButton camouflagerCamoButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite camoButton;

        public static float cooldown { get => CustomOptionHolder.camouflagerCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.camouflagerDuration.getFloat(); }

        public static float camouflageTimer = 0f;
        public Camouflager()
        {
            RoleType = roleId = RoleId.Camouflager;
        }

        public override void OnMeetingStart()
        {
            resetCamouflage();
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            camouflagerCamoButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CamouflagerCamouflage, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.camouflagerCamouflage();
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Camouflager) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    camouflagerCamoButton.Timer = camouflagerCamoButton.MaxTimer;
                    camouflagerCamoButton.isEffectActive = false;
                    camouflagerCamoButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F,
                true, duration, () => { camouflagerCamoButton.Timer = camouflagerCamoButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        {
            camouflagerCamoButton.MaxTimer = cooldown;
            camouflagerCamoButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            camouflageTimer = 0f;
            players = new List<Camouflager>();
        }

        public static void resetCamouflage()
        {
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (Swooper.players.Any(x => x.player.PlayerId == p.PlayerId && x.isInvisble))
                    continue;
                p.setDefaultLook();
            }
        }

        public static Sprite getButtonSprite()
            => camoButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamoButton.png", 115f);
    }
}