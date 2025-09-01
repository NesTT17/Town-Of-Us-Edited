using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Thief : RoleBase<Thief>
    {
        private static CustomButton thiefKillButton;
        public static Color color = new Color32(71, 99, 45, byte.MaxValue);

        public static float cooldown { get => CustomOptionHolder.thiefCooldown.getFloat(); }
        public static bool canKillMayor { get => CustomOptionHolder.thiefCanKillMayor.getBool(); }
        public static bool canKillSheriff { get => CustomOptionHolder.thiefCanKillSheriff.getBool(); }
        public static bool canKillVampireHunter { get => CustomOptionHolder.thiefCanKillVampireHunter.getBool(); }
        public static bool canKillVeteran { get => CustomOptionHolder.thiefCanKillVeteran.getBool(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.thiefHasImpVision.getBool(); }
        public static bool canUseVents { get => CustomOptionHolder.thiefCanUseVents.getBool(); }
        public static bool canStealWithGuess { get => CustomOptionHolder.thiefCanStealWithGuess.getBool(); }

        public static List<PlayerControl> formerThiefs = new List<PlayerControl>();

        public bool suicideFlag = false;
        public PlayerControl currentTarget;
        public Thief()
        {
            RoleType = roleId = RoleId.Thief;
            suicideFlag = false;
            currentTarget = null;
        }

        public static bool isFailedThiefKill(PlayerControl target, PlayerControl killer, RoleInfo targetRole)
        {
            return isRole(killer) && !target.isKiller() && !new List<RoleInfo> { canKillMayor ? RoleInfo.mayor : null, canKillSheriff ? RoleInfo.sheriff : null, canKillVampireHunter ? RoleInfo.vampireHunter : null, canKillVeteran ? RoleInfo.veteran : null }.Contains(targetRole);
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        { 
            thiefKillButton = new CustomButton(
                () =>
                {
                    PlayerControl thief = PlayerControl.LocalPlayer;
                    PlayerControl target = local.currentTarget;
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    var result = Helpers.checkMuderAttempt(thief, target);
                    if (result == MurderAttemptResult.BlankKill)
                    {
                        thiefKillButton.Timer = thiefKillButton.MaxTimer;
                        return;
                    }
                    if (local.suicideFlag)
                    {
                        // Suicide
                        Helpers.MurderPlayer(thief, thief, false);
                    }

                    // Steal role if survived.
                    if (!PlayerControl.LocalPlayer.Data.IsDead && result == MurderAttemptResult.PerformKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ThiefStealsRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.thiefStealsRole(target.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                    }

                    // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                    if (result == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(thief, target, true);
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Thief) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            thiefKillButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            formerThiefs = new List<PlayerControl>();
            players = new List<Thief>();
        }
    }
}