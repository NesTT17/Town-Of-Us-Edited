using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Lighter : RoleBase<Lighter>
    {
        private static CustomButton lighterButton;
        public static Color color = new Color32(238, 229, 190, byte.MaxValue);
        private static Sprite buttonSprite;

        public static float lighterModeLightsOnVision { get => CustomOptionHolder.lighterModeLightsOnVision.getFloat(); }
        public static float lighterModeLightsOffVision { get => CustomOptionHolder.lighterModeLightsOffVision.getFloat(); }
        public static float cooldown { get => CustomOptionHolder.lighterCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.lighterDuration.getFloat(); }

        public float lighterTimer = 0f;
        public Lighter()
        {
            RoleType = roleId = RoleId.Lighter;
            lighterTimer = 0f;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { lighterTimer -= Time.deltaTime; }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            lighterButton = new CustomButton(
                () => { local.lighterTimer = duration; },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    lighterButton.Timer = lighterButton.MaxTimer;
                    lighterButton.isEffectActive = false;
                    lighterButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, duration, () => { lighterButton.Timer = lighterButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        {
            lighterButton.MaxTimer = cooldown;
            lighterButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Lighter>();
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.LighterButton.png", 115f);
    }
}