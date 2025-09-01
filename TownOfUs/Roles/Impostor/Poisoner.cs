using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Poisoner : RoleBase<Poisoner>
    {
        public static CustomButton poisonerButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite poisonButton;

        public static float delay { get => CustomOptionHolder.poisonerKillDelay.getFloat(); }
        public static float cooldown { get => CustomOptionHolder.poisonerCooldown.getFloat(); }

        public PlayerControl currentTarget;
        public PlayerControl poisoned;
        public bool enabledKillButton;
        public Poisoner()
        {
            RoleType = roleId = RoleId.Poisoner;
            currentTarget = null;
            poisoned = null;
            enabledKillButton = false;
        }

        public override void OnMeetingStart()
        {
            poisoned = null;
        }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer != player) return;
            if (!enabledKillButton && PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected && x != null) < 5)
                enabledKillButton = true;
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                PlayerControl target = null;
                if (Agent.exists)
                {
                    if (Agent.impostorsCanKillAnyone) target = Helpers.setTarget(false, true);
                    else
                    {
                        var untargetables = new List<PlayerControl>();
                        untargetables.AddRange(Agent.allPlayers);
                        untargetables.AddRange(Dracula.players.Where(x => x.wasTeamRed).Select(x => x.player));
                        untargetables.AddRange(Vampire.players.Where(x => x.wasTeamRed).Select(x => x.player));
                        target = Helpers.setTarget(true, true, untargetables);
                    }
                }
                else
                {
                    var untargetables = new List<PlayerControl>();
                    untargetables.AddRange(Dracula.players.Where(x => x.wasImpostor).Select(x => x.player));
                    untargetables.AddRange(Vampire.players.Where(x => x.wasImpostor).Select(x => x.player));
                    target = Helpers.setTarget(true, true, untargetables);
                }
                currentTarget = target;
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            poisonerButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    local.poisoned = local.currentTarget;
                    PlayerControl.LocalPlayer.killTimer = delay;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.poisoned.PlayerId);
                    writer.Write((byte)0);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.poisonerSetPoisoned(local.poisoned.PlayerId, 0, PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Poisoner) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    poisonerButton.Timer = poisonerButton.MaxTimer;
                    poisonerButton.isEffectActive = false;
                    poisonerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.Q,
                true, delay, () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, local.currentTarget, true, false, true);
                    if (murder == MurderAttemptResult.PerformKill || murder == MurderAttemptResult.BlankKill)
                    {
                        if (murder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, false);
                        }
                        poisonerButton.Timer = poisonerButton.MaxTimer;
                        PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }
                    else if (murder == MurderAttemptResult.ReverseKill)
                    {
                        Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer, false, false);
                    }
                    else if (murder == MurderAttemptResult.SuppressKill)
                    {
                        poisonerButton.Timer = poisonerButton.MaxTimer;
                        PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue, PlayerControl.LocalPlayer.PlayerId);
                }
            );
        }
        public static void setButtonCooldowns()
        {
            poisonerButton.MaxTimer = cooldown;
            poisonerButton.EffectDuration = delay;
        }

        public static void Clear()
        {
            players = new List<Poisoner>();
        }
        
        public static Sprite getButtonSprite()
            => poisonButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PoisonButton.png", 100f);
    }
}