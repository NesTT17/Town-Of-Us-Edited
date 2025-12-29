using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Pestilence : RoleBase<Pestilence>
    {
        private static CustomButton pestilenceKillButton;
        public static Color color = new Color(0.3f, 0.3f, 0.3f, 1f);

        public static float cooldown { get => CustomOptionHolder.pestilenceCooldown.getFloat(); }
        public static bool canVent { get => CustomOptionHolder.pestilenceCanVent.getBool(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.pestilenceHasImpostorVision.getBool(); }

        public PlayerControl currentTarget;
        public Pestilence()
        {
            RoleType = roleId = RoleId.Pestilence;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget(false, true);
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            pestilenceKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, local.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill || murder == MurderAttemptResult.ReverseKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        return;
                    }
                    if (murder == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        pestilenceKillButton.Timer = pestilenceKillButton.MaxTimer;
                        local.currentTarget = null;
                        return;
                    }

                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        pestilenceKillButton.Timer = pestilenceKillButton.MaxTimer;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Pestilence) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { pestilenceKillButton.Timer = pestilenceKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            pestilenceKillButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Pestilence>();
        }

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }
    }
}