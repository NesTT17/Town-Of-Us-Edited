using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class ButtonBarry : ModifierBase<ButtonBarry>
    {
        private static CustomButton buttonBarryButton;
        public static Color color = Color.yellow;
        private static Sprite buttonSprite;

        public bool usedButton = false;
        public ButtonBarry()
        {
            ModType = modId = RoleId.ButtonBarry;
            usedButton = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            buttonBarryButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                    local.usedButton = true;
                    Helpers.handleVampireKillOnBodyReport();
                    RPCProcedure.uncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, Byte.MaxValue);

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(Byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    buttonBarryButton.Timer = 1f;
                },
                () => { return PlayerControl.LocalPlayer.hasModifier(RoleId.ButtonBarry) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !local.usedButton && PlayerControl.LocalPlayer.CanMove; },
                () => { buttonBarryButton.Timer = buttonBarryButton.MaxTimer; },
                getButtonSprite(), new Vector3(0f, 0f, 0f), hm, null, true
            );
        }
        public static void setButtonCooldowns()
        {
            buttonBarryButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<ButtonBarry>();
        }
        
        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.EmergencyButton.png", 100f);
    }
}