using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Grenadier : RoleBase<Grenadier>
    {
        private static CustomButton grenadierFlashButton;
        public static Color color = Palette.ImpostorRed;
        public static Color flash = new Color32(153, 153, 153, byte.MaxValue);
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> flashedPlayers = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        private static Sprite buttonSprite;

        public static float cooldown { get => CustomOptionHolder.grenadierCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.grenadierDuration.getFloat() + 0.5f; }
        public static int radius { get => CustomOptionHolder.grenadierFlashRadius.getSelection(); }
        public static bool indicateFlashedCrewmates { get => CustomOptionHolder.grenadierIndicateCrewmates.getBool(); }

        public static float flashTimer = 0f;

        public Grenadier()
        {
            RoleType = roleId = RoleId.Grenadier;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        { 
            grenadierFlashButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.grenadierFlash(false);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Grenadier) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;

                    return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer;
                    grenadierFlashButton.isEffectActive = false;
                    grenadierFlashButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Grenadier.getButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F,
                true, Grenadier.duration, () => { grenadierFlashButton.Timer = grenadierFlashButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        {
            grenadierFlashButton.MaxTimer = cooldown;
            grenadierFlashButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Grenadier>();
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FlashButton.png", 100f);
    }
}