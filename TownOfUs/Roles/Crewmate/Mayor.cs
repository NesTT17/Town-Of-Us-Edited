using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Mayor : RoleBase<Mayor>
    {
        private static CustomButton mayorBodyguardButton;
        private static TMP_Text mayorBodyguardButtonText;
        public static Color color = new Color(0.44f, 0.31f, 0.66f, 1f);
        private static Sprite bodyguardButton;

        public static float cooldown { get => CustomOptionHolder.mayorBodyguardCooldown.getFloat(); }
        public static float bodyguardDuration { get => CustomOptionHolder.mayorBodyguardDuration.getFloat(); }

        public int remainingBodyguards = 0;
        public bool isBodyguardActive = false;
        public Mayor()
        {
            RoleType = roleId = RoleId.Mayor;
            remainingBodyguards = 1;
            isBodyguardActive = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (!player.Data.IsDead) remainingBodyguards++;
        }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            mayorBodyguardButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MayorBodyguard, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mayorBodyguard(PlayerControl.LocalPlayer.PlayerId);
                    local.remainingBodyguards--;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (mayorBodyguardButtonText != null) mayorBodyguardButtonText.text = $"{local.remainingBodyguards}";
                    return PlayerControl.LocalPlayer.CanMove && local.remainingBodyguards > 0;
                },
                () =>
                { 
                    mayorBodyguardButton.Timer = mayorBodyguardButton.MaxTimer;
                    mayorBodyguardButton.isEffectActive = false;
                    mayorBodyguardButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, bodyguardDuration, () => { mayorBodyguardButton.Timer = mayorBodyguardButton.MaxTimer; }
            );
            mayorBodyguardButtonText = GameObject.Instantiate(mayorBodyguardButton.actionButton.cooldownTimerText, mayorBodyguardButton.actionButton.cooldownTimerText.transform.parent);
            mayorBodyguardButtonText.text = "";
            mayorBodyguardButtonText.enableWordWrapping = false;
            mayorBodyguardButtonText.transform.localScale = Vector3.one * 0.5f;
            mayorBodyguardButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            mayorBodyguardButton.MaxTimer = cooldown;
            mayorBodyguardButton.EffectDuration = bodyguardDuration;
        }

        public static void Clear()
        {
            players = new List<Mayor>();
        }
        
        public static Sprite getButtonSprite()
            => bodyguardButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BodyguardButton.png", 100f);
    }
}