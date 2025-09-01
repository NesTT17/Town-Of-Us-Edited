using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Sheriff : RoleBase<Sheriff>
    {
        private static CustomButton sheriffKillButton;
        public static Color color = Color.yellow;
        private static Sprite shootButton;

        public static float cooldown { get => CustomOptionHolder.sheriffShootCooldown.getFloat(); }
        public static bool missfireShootKillsTarget { get => CustomOptionHolder.sheriffMissfireShootKillsTarget.getBool(); }
        public static bool canKillBenignNeutrals { get => CustomOptionHolder.sheriffCanKillBenignNeutrals.getBool(); }
        public static bool canKillEvilNeutrals { get => CustomOptionHolder.sheriffCanKillEvilNeutrals.getBool(); }
        public static bool canKillKillingNeutrals { get => CustomOptionHolder.sheriffCanKillKillingNeutrals.getBool(); }
        public static bool agentCanDieToSheriff { get => CustomOptionHolder.agentCanDieToSheriff.getBool(); }

        public PlayerControl currentTarget;
        public Sheriff()
        {
            RoleType = roleId = RoleId.Sheriff;
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

        public static void makeButtons(HudManager __instance)
        {
            sheriffKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murderAttemptResult = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, local.currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        return;
                    }

                    if (murderAttemptResult == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                        local.currentTarget = null;
                        return;
                    }

                    if (murderAttemptResult == MurderAttemptResult.PerformKill || murderAttemptResult == MurderAttemptResult.ReverseKill)
                    {
                        bool missfire = false;

                        if (local.currentTarget.Data.Role.IsImpostor || agentCanDieToSheriff && local.currentTarget.isRole(RoleId.Agent) || local.currentTarget.isBenignNeutral() && canKillBenignNeutrals || local.currentTarget.isEvilNeutral() && canKillEvilNeutrals || local.currentTarget.isKillingNeutral() && canKillKillingNeutrals) missfire = false;
                        else missfire = true;

                        if (missfire)
                        {
                            if (murderAttemptResult == MurderAttemptResult.PerformKill)
                            {
                                if (missfireShootKillsTarget) Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, false);
                                Helpers.MurderPlayer(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer, false);
                            }
                            else if (murderAttemptResult == MurderAttemptResult.ReverseKill)
                            {
                                Helpers.MurderPlayer(local.currentTarget, PlayerControl.LocalPlayer, true);
                            }
                        }
                        else Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        
                        sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                getShootButtonSprite(), CustomButton.ButtonPositions.upperRowRight, __instance, KeyCode.Q
            );
        }

        public static void setButtonCooldowns()
        {
            sheriffKillButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Sheriff>();
        }

        public static Sprite getShootButtonSprite()
            => shootButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShootButton.png", 100f); 
    }
}