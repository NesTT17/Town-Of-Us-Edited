using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Shifter : ModifierBase<Shifter>
    {
        private static CustomButton shifterButton;
        public static Color color = Palette.CrewmateBlue;
        private static Sprite shiftButton;

        public static bool shiftsMedicShield { get => CustomOptionHolder.modifierShifterShiftsMedicShield.getBool(); }

        public static PlayerControl futureShift;
        public PlayerControl currentTarget;
        public Shifter()
        {
            ModType = modId = RoleId.Shifter;
            currentTarget = null;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                if (futureShift == null) Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            shifterButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetFutureShifted, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureShifted(local.currentTarget.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.hasModifier(RoleId.Shifter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && futureShift == null && PlayerControl.LocalPlayer.CanMove; },
                () => { shifterButton.Timer = shifterButton.MaxTimer; },
                getButtonSprite(), new Vector3(0f, 1f, 0f), hm, null, true
            );
        }
        public static void setButtonCooldowns()
        {
            shifterButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            futureShift = null;
            players = new List<Shifter>();
        }

        public static Sprite getButtonSprite()
            => shiftButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShiftButton.png", 115f);
    }
}