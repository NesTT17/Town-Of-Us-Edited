using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class GuardianAngel : RoleBase<GuardianAngel>
    {
        private static CustomButton guardianAngelProtectButton;
        private static TMP_Text guardianAngelProtectButtonText;
        public static Color color = new Color(0.7f, 1f, 1f, 1f);
        private static Sprite protectButton;

        public static float cooldown { get => CustomOptionHolder.guardianAngelProtectCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.guardianAngelProtectDuration.getFloat(); }
        public static bool knowsRole { get => CustomOptionHolder.guardianAngelKnowsRole.getBool(); }
        public static bool targetKnows { get => CustomOptionHolder.guardianAngelTargetKnows.getBool(); }
        public static int whoSeesProtect { get => CustomOptionHolder.guardianAngelWhoSeesProtect.getSelection(); }
        public static int oddsEvilTarget { get => CustomOptionHolder.guardianAngelOddsEvilTarget.getSelection(); }

        public static PlayerControl target;
        public static bool targetWasGuessed = false;

        public int remainingProtects = 0;
        public bool isProtectActive = false;
        public GuardianAngel()
        {
            RoleType = roleId = RoleId.GuardianAngel;
            remainingProtects = Mathf.RoundToInt(CustomOptionHolder.guardianAngelNumberOfProtects.getFloat());
            isProtectActive = false;
            target = null;
        }

        public static bool isProtected(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && target == player && x.isProtectActive && player?.Data.IsDead == false);
        public static List<GuardianAngel> GetGuardianAngel(PlayerControl player) => players.Where(x => target == player).ToList();
        public static bool protectVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
            bool isMorphedGlitch = Glitch.players.Any(x => x.player == target && Glitch.morphPlayer != null && Glitch.morphTimer > 0f);
            bool isMorphedSwooper = Swooper.players.Any(x => x.player == target && x.isInvisble);
            bool isMorphedVenerer = Venerer.players.Any(x => x.player == target && x.morphTimer > 0f);

            if ((isProtected(target) && !isMorphedMorphling && !isMorphedGlitch && !isMorphedSwooper && !isMorphedVenerer) || (isMorphedMorphling && isProtected(Morphling.getRole(target).morphTarget)) || (isMorphedGlitch && isProtected(Glitch.morphPlayer)))
            {
                foreach (var guardianAngel in GetGuardianAngel(target))
                {
                    if (guardianAngel == null || guardianAngel.player == null) return false;
                    hasVisibleShield = whoSeesProtect == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || whoSeesProtect == 1 & PlayerControl.LocalPlayer == target // Target
                    || whoSeesProtect == 2 & PlayerControl.LocalPlayer == guardianAngel.player // GA
                    || whoSeesProtect == 3 && (PlayerControl.LocalPlayer == target || PlayerControl.LocalPlayer == guardianAngel.player);
                }
            }
            return hasVisibleShield;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player) target = null;
        }
        
        public override void ResetRole(bool isShifted)
        {
            if (target != null && player == PlayerControl.LocalPlayer) {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void makeButtons(HudManager hm)
        {
            guardianAngelProtectButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelProtect, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelProtect(PlayerControl.LocalPlayer.PlayerId);
                    local.remainingProtects--;
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.GuardianAngel) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (guardianAngelProtectButtonText != null) guardianAngelProtectButtonText.text = $"{local.remainingProtects}";
                    return PlayerControl.LocalPlayer.CanMove && local.remainingProtects > 0;
                },
                () =>
                {
                    guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer;
                    guardianAngelProtectButton.isEffectActive = false;
                    guardianAngelProtectButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, duration, () => { guardianAngelProtectButton.Timer = guardianAngelProtectButton.MaxTimer; }
            );
            guardianAngelProtectButtonText = GameObject.Instantiate(guardianAngelProtectButton.actionButton.cooldownTimerText, guardianAngelProtectButton.actionButton.cooldownTimerText.transform.parent);
            guardianAngelProtectButtonText.text = "";
            guardianAngelProtectButtonText.enableWordWrapping = false;
            guardianAngelProtectButtonText.transform.localScale = Vector3.one * 0.5f;
            guardianAngelProtectButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            guardianAngelProtectButton.MaxTimer = cooldown;
            guardianAngelProtectButton.EffectDuration = duration;
        }

        public static void clearTarget()
        {
            target = null;
            targetWasGuessed = false;
        }

        public static void Clear()
        {
            clearTarget();
            players = new List<GuardianAngel>();
        }

        public static Sprite getButtonSprite()
            => protectButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ProtectButton.png", 100f);
    }
}