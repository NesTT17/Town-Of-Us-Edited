using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Werewolf : RoleBase<Werewolf>
    {
        private static CustomButton werewolfRampageButton;
        private static CustomButton werewolfKillButton;
        public static Color color = new Color(0.66f, 0.4f, 0.16f, 1f);
        private static Sprite rampageButton;

        public static float cooldown { get => CustomOptionHolder.werewolfRampageCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.werewolfRampageDuration.getFloat(); }
        public static float killCooldown { get => CustomOptionHolder.werewolfKillCooldown.getFloat(); }
        public static bool canVent { get => CustomOptionHolder.werewolfCanVent.getBool(); }

        public bool isRampageActive;
        public PlayerControl currentTarget;
        public Werewolf()
        {
            RoleType = roleId = RoleId.Werewolf;
            isRampageActive = false;
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
            werewolfRampageButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WerewolfRampage, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.werewolfRampage(PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Werewolf) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer;
                    werewolfRampageButton.isEffectActive = false;
                    werewolfRampageButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, hm, KeyCode.F,
                true, duration, () => { werewolfRampageButton.Timer = werewolfRampageButton.MaxTimer; }
            );

            werewolfKillButton = new CustomButton(
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
                        Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer);
                        return;
                    }
                    if (murder == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        werewolfKillButton.Timer = werewolfKillButton.MaxTimer;
                        local.currentTarget = null;
                        return;
                    }

                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        werewolfKillButton.Timer = werewolfKillButton.MaxTimer;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Werewolf) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget != null && local.isRampageActive && PlayerControl.LocalPlayer.CanMove; },
                () => { werewolfKillButton.Timer = werewolfKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            werewolfRampageButton.MaxTimer = cooldown;
            werewolfRampageButton.EffectDuration = duration;
            werewolfKillButton.MaxTimer = killCooldown;
        }

        public static void Clear()
        {
            players = new List<Werewolf>();
        }
        
        public static Sprite getButtonSprite()
            => rampageButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RampageButton.png", 100f);
    }
}