using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Survivor : RoleBase<Survivor>
    {
        private static CustomButton survivorSafeguardButton;
        private static TMP_Text survivorSafeguardButtonText;
        public static Color color = new Color(1f, 0.9f, 0.3f, 1f);
        private static Sprite safeguardButton;

        public static float cooldown { get => CustomOptionHolder.survivorSafeguardCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.survivorSafeguardDuration.getFloat(); }

        public int remainingSafeguards = 0;
        public bool isSafeguardActive = false;
        public Survivor()
        {
            RoleType = roleId = RoleId.Survivor;
            remainingSafeguards = Mathf.RoundToInt(CustomOptionHolder.survivorNumberOfSafeguards.getFloat());
            isSafeguardActive = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            survivorSafeguardButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SurvivorSafeguard, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.survivorSafeguard(PlayerControl.LocalPlayer.PlayerId);
                    local.remainingSafeguards--;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Survivor) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (survivorSafeguardButtonText != null) survivorSafeguardButtonText.text = $"{local.remainingSafeguards}";
                    return local.remainingSafeguards > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer;
                    survivorSafeguardButton.isEffectActive = false;
                    survivorSafeguardButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, duration, () => { survivorSafeguardButton.Timer = survivorSafeguardButton.MaxTimer; }
            );
            survivorSafeguardButtonText = GameObject.Instantiate(survivorSafeguardButton.actionButton.cooldownTimerText, survivorSafeguardButton.actionButton.cooldownTimerText.transform.parent);
            survivorSafeguardButtonText.text = "";
            survivorSafeguardButtonText.enableWordWrapping = false;
            survivorSafeguardButtonText.transform.localScale = Vector3.one * 0.5f;
            survivorSafeguardButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            survivorSafeguardButton.MaxTimer = cooldown;
            survivorSafeguardButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Survivor>();
        }

        public static Sprite getButtonSprite()
            => safeguardButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SafeguardButton.png", 100f);
    }
}