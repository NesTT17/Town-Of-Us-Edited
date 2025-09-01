using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Veteran : RoleBase<Veteran>
    {
        private static CustomButton veteranAlertButton;
        private static TMP_Text veteranAlertButtonText;
        public static Color color = new Color(0.6f, 0.5f, 0.25f, 1f);
        private static Sprite alertButton;

        public static float cooldown { get => CustomOptionHolder.veteranCooldown.getFloat(); }
        public static float alertDuration { get => CustomOptionHolder.veteranAlertDuration.getFloat(); }

        public int remainingAlerts = 0;
        public bool isAlertActive = false;
        public Veteran()
        {
            RoleType = roleId = RoleId.Veteran;
            remainingAlerts = Mathf.RoundToInt(CustomOptionHolder.veteranAlertNumber.getFloat());
            isAlertActive = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            veteranAlertButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VeteranAlert, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.veteranAlert(PlayerControl.LocalPlayer.PlayerId);
                    local.remainingAlerts--;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Veteran) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (veteranAlertButtonText != null) veteranAlertButtonText.text = $"{local.remainingAlerts}";
                    return PlayerControl.LocalPlayer.CanMove && local.remainingAlerts > 0;
                },
                () =>
                {
                    veteranAlertButton.Timer = veteranAlertButton.MaxTimer;
                    veteranAlertButton.isEffectActive = false;
                    veteranAlertButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, alertDuration, () => { veteranAlertButton.Timer = veteranAlertButton.MaxTimer; }
            );
            veteranAlertButtonText = GameObject.Instantiate(veteranAlertButton.actionButton.cooldownTimerText, veteranAlertButton.actionButton.cooldownTimerText.transform.parent);
            veteranAlertButtonText.text = "";
            veteranAlertButtonText.enableWordWrapping = false;
            veteranAlertButtonText.transform.localScale = Vector3.one * 0.5f;
            veteranAlertButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            veteranAlertButton.MaxTimer = cooldown;
            veteranAlertButton.EffectDuration = alertDuration;
        }

        public static void Clear()
        {
            players = new List<Veteran>();
        }

        public static Sprite getButtonSprite()
            => alertButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.AlertButton.png", 100f);
    }
}