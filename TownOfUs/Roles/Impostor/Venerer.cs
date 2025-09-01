using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Venerer : RoleBase<Venerer>
    {
        private static CustomButton venererAbilityButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite noAbilitiesButton;
        private static Sprite camoButton;
        private static Sprite speedButton;
        private static Sprite freezeButton;

        public static float cooldown { get => CustomOptionHolder.venererCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.venererDuration.getFloat(); }
        public static float speedMultiplier { get => CustomOptionHolder.venererSpeedMultiplier.getFloat(); }
        public static float freezeMultiplier { get => CustomOptionHolder.venererFreezeMultiplier.getFloat(); }

        public int numberOfKills = 0;
        public float morphTimer = 0f;
        public Venerer()
        {
            RoleType = roleId = RoleId.Venerer;
            numberOfKills = 0;
            morphTimer = 0f;
        }

        public void resetMorph()
        {
            morphTimer = 0f;
            if (player == null) return;
            player.setDefaultLook();
        }

        public override void OnMeetingStart()
        {
            resetMorph();
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            if (player != PlayerControl.LocalPlayer) return;
            numberOfKills++;
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            venererAbilityButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VenererCamo, SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.venererCamo(PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Venerer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (local.numberOfKills == 1) venererAbilityButton.Sprite = getcamoButton();
                    if (local.numberOfKills == 2) venererAbilityButton.Sprite = getspeedButton();
                    if (local.numberOfKills >= 3) venererAbilityButton.Sprite = getfreezeButton();
                    return local.numberOfKills > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    venererAbilityButton.Timer = venererAbilityButton.MaxTimer;
                    venererAbilityButton.isEffectActive = false;
                    venererAbilityButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getnoAbilitiesButton(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F,
                true, duration, () => { venererAbilityButton.Timer = venererAbilityButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        {
            venererAbilityButton.MaxTimer = cooldown;
            venererAbilityButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Venerer>();
        }
        
        public static Sprite getnoAbilitiesButton()
            => noAbilitiesButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.NoAbilitiesButton.png", 100f);

        public static Sprite getcamoButton()
            => camoButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamouflageButton.png", 100f);

        public static Sprite getspeedButton()
            => speedButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SpeedButton.png", 100f);

        public static Sprite getfreezeButton()
            => freezeButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FreezeButton.png", 100f);
    }
}