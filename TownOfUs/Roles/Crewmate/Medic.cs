using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Medic : RoleBase<Medic>
    {
        private static CustomButton medicShieldButton;
        public static Color color = new Color(0f, 0.4f, 0f, 1f);
        public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);
        private static Sprite shieldButton;

        public static int showShielded { get => CustomOptionHolder.medicShowShielded.getSelection(); } // 0 - Everyone, 1 - Shielded, 2 - Medic, 3 - Medic+Shielded, 4 - Nobody
        public static int getsNotification { get => CustomOptionHolder.medicGetsNotification.getSelection(); } // 0 - Everyone, 1 - Shielded, 2 - Medic, 3 - Medic+Shielded, 4 - Nobody
        public static bool getInfoOnReport { get => CustomOptionHolder.medicGetsInfo.getBool(); }
        public static float reportNameDuration { get => CustomOptionHolder.medicReportNameDuration.getFloat(); }
        public static float reportColorDuration { get => CustomOptionHolder.medicReportColorDuration.getFloat(); }

        public bool usedShield;
        public PlayerControl shielded;
        public PlayerControl currentTarget;
        public Medic()
        {
            RoleType = roleId = RoleId.Medic;
            shielded = null;
            currentTarget = null;
            usedShield = false;
        }

        public static bool isShielded(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && x.shielded == player && player?.Data.IsDead == false);
        public static List<Medic> GetMedic(PlayerControl shielded) => players.Where(x => x.shielded == shielded).ToList();
        public static bool shieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
            bool isMorphedGlitch = Glitch.players.Any(x => x.player == target && Glitch.morphPlayer != null && Glitch.morphTimer > 0f);
            bool isMorphedSwooper = Swooper.players.Any(x => x.player == target && x.isInvisble);
            bool isMorphedVenerer = Venerer.players.Any(x => x.player == target && x.morphTimer > 0f);

            if ((isShielded(target) && !isMorphedMorphling && !isMorphedGlitch && !isMorphedSwooper && !isMorphedVenerer) || (isMorphedMorphling && isShielded(Morphling.getRole(target).morphTarget)) || (isMorphedGlitch && isShielded(Glitch.morphPlayer)))
            {
                foreach (var medic in GetMedic(target))
                {
                    if (medic == null || medic.player == null) return false;
                    hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || showShielded == 1 & PlayerControl.LocalPlayer == medic.shielded // Shielded
                    || showShielded == 2 & PlayerControl.LocalPlayer == medic.player // Medic
                    || showShielded == 3 && (PlayerControl.LocalPlayer == medic.shielded || PlayerControl.LocalPlayer == medic.player);
                }
            }
            return hasVisibleShield;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                if (!usedShield) Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            medicShieldButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.medicSetShielded(local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Medic) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && !local.usedShield && PlayerControl.LocalPlayer.CanMove; },
                () => { medicShieldButton.Timer = medicShieldButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            medicShieldButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<Medic>();
        }

        public static Sprite getButtonSprite()
            => shieldButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ShieldButton.png", 115f);
    }
}