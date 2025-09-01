using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Morphling : RoleBase<Morphling>
    {
        private static CustomButton morphlingButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite sampleButton;
        private static Sprite morphButton;

        public static float cooldown { get => CustomOptionHolder.morphlingCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.morphlingDuration.getFloat(); }

        public PlayerControl currentTarget;
        public PlayerControl sampledTarget;
        public PlayerControl morphTarget;
        public float morphTimer = 0f;
        public Morphling()
        {
            RoleType = roleId = RoleId.Morphling;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
        }

        public void resetMorph()
        {
            morphTarget = null;
            morphTimer = 0f;
            if (player == null) return;
            player.setDefaultLook();
        }

        public override void OnMeetingStart()
        {
            resetMorph();
        }
        public override void OnMeetingEnd()
        {
            sampledTarget = null;
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void ResetRole(bool isShifted)
        {
            resetMorph();
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            resetMorph();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            morphlingButton = new CustomButton(
                () =>
                {
                    if (local.sampledTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MorphlingMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(local.sampledTarget.PlayerId);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.morphlingMorph(local.sampledTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                        local.sampledTarget = null;
                        morphlingButton.EffectDuration = duration;
                        morphlingButton.shakeOnEnd = true;
                    }
                    else if (local.currentTarget != null)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;
                        local.sampledTarget = local.currentTarget;
                        morphlingButton.Sprite = getMorphSprite();
                        morphlingButton.shakeOnEnd = false;
                        morphlingButton.EffectDuration = 1f;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Morphling) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return (local.currentTarget || local.sampledTarget) && PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive() && Camouflager.camouflageTimer < 0f; },
                () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    morphlingButton.Sprite = getSampleSprite();
                },
                getSampleSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F,
                true, duration, () =>
                {
                    if (local.sampledTarget == null)
                    {
                        morphlingButton.Timer = morphlingButton.MaxTimer;
                        morphlingButton.Sprite = getSampleSprite();
                    }
                }
            );
        }
        public static void setButtonCooldowns()
        {
            morphlingButton.MaxTimer = cooldown;
            morphlingButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Morphling>();
        }
        
        public static Sprite getSampleSprite()
            => sampleButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SampleButton.png", 115f);
        public static Sprite getMorphSprite()
            => morphButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MorphButton.png", 115f);
    }
}