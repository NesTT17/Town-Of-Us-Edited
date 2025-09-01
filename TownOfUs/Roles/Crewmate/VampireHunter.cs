using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class VampireHunter : RoleBase<VampireHunter>
    {
        private static CustomButton vampireHunterStakeButton;
        public static Color color = new Color(0.7f, 0.7f, 0.9f, 1f);
        private static Sprite stakeButton;

        public static float stakeCooldown { get => CustomOptionHolder.vampireHunterStakeCooldown.getFloat(); }
        public static int maxFailedStakes { get => Mathf.RoundToInt(CustomOptionHolder.vampireHunterMaxFailedStakes.getFloat()); }
        public static bool canStakeRoundOne { get => CustomOptionHolder.vampireHunterCanStakeRoundOne.getBool(); }
        public static bool selfKillAfterFinalStake { get => CustomOptionHolder.vampireHunterSelfKillAfterFinalStake.getBool(); }
        public static int promotesTo { get => CustomOptionHolder.vampireHunterPromote.getSelection(); }

        public bool canStake;
        public bool suicideFlag;
        public int currentFailedStakes;
        public PlayerControl currentTarget;
        public VampireHunter()
        {
            RoleType = roleId = RoleId.VampireHunter;
            currentFailedStakes = 0;
            suicideFlag = false;
            if (!canStakeRoundOne) canStake = false;
            else canStake = true;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (!canStake) canStake = true;
            if ((Dracula.livingPlayers.Count + Vampire.livingPlayers.Count) == 0)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.VampireHunterPromotes, SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampireHunterPromote(player.PlayerId);
            }
        }
        public override void FixedUpdate()
        {
            if (suicideFlag)
            {
                suicideFlag = false;
                Helpers.MurderPlayer(player, player, false);
            }
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
            vampireHunterStakeButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    if (Dracula.players.Any(x => x.player == local.currentTarget) || Vampire.players.Any(x => x.player == local.currentTarget))
                    {
                        Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, local.currentTarget);
                        vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer;
                        local.currentTarget = null;
                    }
                    else
                    {
                        local.currentFailedStakes++;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.VampireHunter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (local.currentFailedStakes == maxFailedStakes && !local.player.Data.IsDead) local.suicideFlag = true;
                    return local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { vampireHunterStakeButton.Timer = vampireHunterStakeButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            vampireHunterStakeButton.MaxTimer = stakeCooldown;
        }

        public static void Clear()
        {
            players = new List<VampireHunter>();
        }
        
        public static Sprite getButtonSprite()
            => stakeButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.StakeButton.png", 100f);
    }
}