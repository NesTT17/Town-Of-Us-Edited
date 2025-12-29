using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Juggernaut : RoleBase<Juggernaut>
    {
        private static CustomButton juggernautKillButton;
        public static Color color = new Color(0.55f, 0f, 0.3f, 1f);

        public static float initialCooldown { get => CustomOptionHolder.juggernautCooldown.getFloat(); }
        public static float cooldownReduce { get => CustomOptionHolder.juggernautCooldownReduce.getFloat(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.juggernautHasImpostorVision.getBool(); }
        public static bool canVent { get => CustomOptionHolder.juggernautCanVent.getBool(); }

        public int numOfKills = 0;
        public PlayerControl currentTarget;
        public Juggernaut()
        {
            RoleType = roleId = RoleId.Juggernaut;
            numOfKills = 0;
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
            juggernautKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, local.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        return;
                    }
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer);
                        return;
                    }
                    if (murder == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        juggernautKillButton.Timer = juggernautKillButton.MaxTimer - (local.numOfKills * cooldownReduce);
                        if (juggernautKillButton.Timer < 0) juggernautKillButton.Timer = 0;
                        local.currentTarget = null;
                        return;
                    }

                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        local.numOfKills++;
                        Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        juggernautKillButton.Timer = juggernautKillButton.MaxTimer - (local.numOfKills * cooldownReduce);
                        if (juggernautKillButton.Timer < 0) juggernautKillButton.Timer = 0;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Juggernaut) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    juggernautKillButton.Timer = juggernautKillButton.MaxTimer - (local.numOfKills * cooldownReduce);
                    if (juggernautKillButton.Timer < 0) juggernautKillButton.Timer = 0;
                },
                hm.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            juggernautKillButton.MaxTimer = initialCooldown;
        }

        public static void Clear()
        {
            players = new List<Juggernaut>();
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