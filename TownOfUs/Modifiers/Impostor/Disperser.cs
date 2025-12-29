using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Disperser : ModifierBase<Disperser>
    {
        private static CustomButton disperserButton;
        private static TMP_Text disperserButtonText;
        public static Color color = Palette.ImpostorRed;
        private static Sprite buttonSprite;

        public static bool dispersesToVent { get => CustomOptionHolder.modifierDisperserDispersesToVent.getBool(); }

        public int numOfKills = 0;
        public int remainingDisperses = 0;
        public float rechargedKillsNumber;
        public float rechargeKills;
        public Disperser()
        {
            ModType = modId = RoleId.Disperser;
            remainingDisperses = Mathf.RoundToInt(CustomOptionHolder.modifierDisperserInitialDisperses.getFloat());
            rechargedKillsNumber = Mathf.RoundToInt(CustomOptionHolder.modifierDisperserRechargeKillsNumber.getFloat());
            rechargeKills = Mathf.RoundToInt(CustomOptionHolder.modifierDisperserRechargeKillsNumber.getFloat());
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == null || PlayerControl.LocalPlayer != player) return;
            if (numOfKills == rechargeKills)
            {
                rechargeKills += rechargedKillsNumber;
                remainingDisperses++;
            }
        }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            numOfKills++;
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            disperserButton = new CustomButton(
                () =>
                {
                    local.remainingDisperses--;
                    disperserButton.Timer = disperserButton.MaxTimer;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DisperserDisperse, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.disperserDisperse();
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.hasModifier(RoleId.Disperser) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (disperserButtonText != null) disperserButtonText.text = $"{local.remainingDisperses}";
                    return PlayerControl.LocalPlayer.CanMove && local.remainingDisperses > 0;
                },
                () => { disperserButton.Timer = disperserButton.MaxTimer; },
                getButtonSprite(), new Vector3(0f, 0f, 0f), hm, null, true
            );
            disperserButtonText = GameObject.Instantiate(disperserButton.actionButton.cooldownTimerText, disperserButton.actionButton.cooldownTimerText.transform.parent);
            disperserButtonText.text = "";
            disperserButtonText.enableWordWrapping = false;
            disperserButtonText.transform.localScale = Vector3.one * 0.5f;
            disperserButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            disperserButton.MaxTimer = 10f;
        }

        public static void Clear()
        {
            players = new List<Disperser>();
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DisperseButton.png", 100f);
    }
}