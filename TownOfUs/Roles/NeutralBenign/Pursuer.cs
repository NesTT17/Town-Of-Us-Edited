using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Pursuer : RoleBase<Pursuer>
    {
        private static CustomButton pursuerButton;
        private static TMP_Text pursuerButtonText;
        public static Color color = Lawyer.color;
        private static Sprite blankButton;

        public static float cooldown { get => CustomOptionHolder.pursuerCooldown.getFloat(); }
        public static int blanksNumber { get => Mathf.RoundToInt(CustomOptionHolder.pursuerBlanksNumber.getFloat()); }

        public int blanks = 0;
        public PlayerControl target;
        public bool notAckedExiled = false;
        public static List<PlayerControl> blankedList = new List<PlayerControl>();
        public Pursuer()
        {
            RoleType = roleId = RoleId.Pursuer;
            blanks = 0;
            notAckedExiled = false;
            target = null;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                target = Helpers.setTarget();
                Helpers.setPlayerOutline(target, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            player.clearAllTasks();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            pursuerButton = new CustomButton(
                () =>
                {
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.target);
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.target);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.target)) return;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.target.PlayerId);
                    writer.Write(Byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setBlanked(local.target.PlayerId, Byte.MaxValue);
                    local.target = null;
                    local.blanks++;
                    pursuerButton.Timer = pursuerButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Pursuer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (pursuerButtonText != null) pursuerButtonText.text = $"{local.blanks}";
                    return blanksNumber > local.blanks && local.target && PlayerControl.LocalPlayer.CanMove;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                getTargetSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
            pursuerButtonText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonText.text = "";
            pursuerButtonText.enableWordWrapping = false;
            pursuerButtonText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            pursuerButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            blankedList = new List<PlayerControl>();
            players = new List<Pursuer>();
        }
        
        public static Sprite getTargetSprite()
            => blankButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PursuerButton.png", 115f);
    }
}