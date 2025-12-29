using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public class Chameleon : ModifierBase<Chameleon>
    {
        public static Color color = Color.yellow;

        public static float holdDuration { get => CustomOptionHolder.modifierChameleonHoldDuration.getFloat(); }
        public static float fadeDuration { get => CustomOptionHolder.modifierChameleonFadeDuration.getFloat(); }
        public static float minVisibility { get => CustomOptionHolder.modifierChameleonMinVisibility.getSelection() / 10f; }

        public Dictionary<byte, float> lastMoved;
        public Chameleon()
        {
            ModType = modId = RoleId.Chameleon;
            lastMoved = new Dictionary<byte, float>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void HudUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm) { }
        public static void setButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<Chameleon>();
        }

        public float visibility(byte playerId)
        {
            float visibility = 1f;
            if (lastMoved != null && lastMoved.ContainsKey(playerId))
            {
                var tStill = Time.time - lastMoved[playerId];
                if (tStill > holdDuration)
                {
                    if (tStill - holdDuration > fadeDuration) visibility = minVisibility;
                    else visibility = (1 - (tStill - holdDuration) / fadeDuration) * (1 - minVisibility) + minVisibility;
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsDead && visibility < 0.1f)
            {  // Ghosts can always see!
                visibility = 0.1f;
            }
            return visibility;
        }

        public void update()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                var swooperRole = RoleBase<Swooper>.getRole(PlayerControl.LocalPlayer);
                if (swooperRole != null && swooperRole.isInvisble) return; // Dont make Swooper visible...

                // check movement by animation
                PlayerPhysics playerPhysics = PlayerControl.LocalPlayer.MyPhysics;
                var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
                if (currentPhysicsAnim != playerPhysics.Animations.group.IdleAnim)
                {
                    lastMoved[PlayerControl.LocalPlayer.PlayerId] = Time.time;
                }

                // calculate and set visibility
                float visibilty = visibility(PlayerControl.LocalPlayer.PlayerId);
                float petVisibility = visibilty;
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    visibilty = 0.5f;
                    petVisibility = 1f;
                }

                try
                {
                    PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.color = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.color.SetAlpha(visibilty);
                    if (DataManager.Settings.Accessibility.ColorBlindMode) PlayerControl.LocalPlayer.cosmetics.colorBlindText.color = PlayerControl.LocalPlayer.cosmetics.colorBlindText.color.SetAlpha(visibilty);
                    PlayerControl.LocalPlayer.SetHatAndVisorAlpha(visibilty);
                    PlayerControl.LocalPlayer.cosmetics.skin.layer.color = PlayerControl.LocalPlayer.cosmetics.skin.layer.color.SetAlpha(visibilty);
                    PlayerControl.LocalPlayer.cosmetics.nameText.color = PlayerControl.LocalPlayer.cosmetics.nameText.color.SetAlpha(visibilty);
                    foreach (var rend in PlayerControl.LocalPlayer.cosmetics.currentPet.renderers)
                        rend.color = rend.color.SetAlpha(petVisibility);
                    foreach (var shadowRend in PlayerControl.LocalPlayer.cosmetics.currentPet.shadows)
                        shadowRend.color = shadowRend.color.SetAlpha(petVisibility);
                }
                catch { }
            }
        }
    }
}