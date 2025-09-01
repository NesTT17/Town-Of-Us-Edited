using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Mercenary : RoleBase<Mercenary>
    {
        private static CustomButton mercenaryProtectButton;
        private static CustomButton mercenaryDonArmorButton;
        private static TMP_Text mercenaryDonArmorButtonText;
        public static Color color = new Color(0.65f, 0.61f, 0.58f, 1f);
        private static Sprite protectButton;
        private static Sprite donArmorButton;

        public static int brildersRequires { get => Mathf.RoundToInt(CustomOptionHolder.mercenaryBrildersRequired.getFloat()); }
        public static int showShielded { get => CustomOptionHolder.mercenaryShowShielded.getSelection(); } // 0 - Everyone, 1 - Shielded, 2 - Mercenary, 3 - Mercenary+Shielded, 4 - Nobody
        public static int getsNotification { get => CustomOptionHolder.mercenaryGetsNotification.getSelection(); } // 0 - Everyone, 1 - Shielded, 2 - Mercenary, 3 - Mercenary+Shielded, 4 - Nobody
        public static float cooldown { get => CustomOptionHolder.mercenaryArmorCooldown.getFloat(); }
        public static float duration { get => CustomOptionHolder.mercenaryArmorDuration.getFloat(); }

        public int brilders;
        public PlayerControl currentTarget;
        public bool usedAbility;
        public PlayerControl shielded;
        public PlayerControl exShielded;
        public bool isArmorActive;
        public Mercenary()
        {
            RoleType = roleId = RoleId.Mercenary;
            brilders = 0;
            currentTarget = null;
            usedAbility = false;
            shielded = null;
            exShielded = null;
            isArmorActive = false;
        }
        
        public static bool isShielded(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && x.shielded == player && player?.Data.IsDead == false);
        public static List<Mercenary> GetMercenary(PlayerControl shielded) => players.Where(x => x.shielded == shielded).ToList();
        public static bool shieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
            bool isMorphedGlitch = Glitch.players.Any(x => x.player == target && Glitch.morphPlayer != null && Glitch.morphTimer > 0f);
            bool isMorphedSwooper = Swooper.players.Any(x => x.player == target && x.isInvisble);
            bool isMorphedVenerer = Venerer.players.Any(x => x.player == target && x.morphTimer > 0f);

            if ((isShielded(target) && !isMorphedMorphling && !isMorphedGlitch && !isMorphedSwooper && !isMorphedVenerer) || (isMorphedMorphling && isShielded(Morphling.getRole(target).morphTarget)) || (isMorphedGlitch && isShielded(Glitch.morphPlayer)))
            {
                foreach (var mercenary in GetMercenary(target))
                {
                    if (mercenary == null || mercenary.player == null) return false;
                    hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone
                    || showShielded == 1 & PlayerControl.LocalPlayer == mercenary.shielded // Shielded
                    || showShielded == 2 & PlayerControl.LocalPlayer == mercenary.player // Mercenary
                    || showShielded == 3 && (PlayerControl.LocalPlayer == mercenary.shielded || PlayerControl.LocalPlayer == mercenary.player);
                }
            }
            return hasVisibleShield;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        { 
            if (shielded != null) shielded = null;
            if (usedAbility) usedAbility = false;
        }
        public override void FixedUpdate()
        { 
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                if (!usedAbility) Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            mercenaryProtectButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenarySetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenarySetShielded(local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Mercenary) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && !local.usedAbility && PlayerControl.LocalPlayer.CanMove; },
                () => { mercenaryProtectButton.Timer = mercenaryProtectButton.MaxTimer; },
                getProtectButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.F
            );

            mercenaryDonArmorButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryDonArmor, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryDonArmor(PlayerControl.LocalPlayer.PlayerId);
                    local.brilders--;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Mercenary) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (mercenaryDonArmorButtonText != null) mercenaryDonArmorButtonText.text = $"{local.brilders}/{brildersRequires}";
                    return PlayerControl.LocalPlayer.CanMove && local.brilders > 0;
                },
                () =>
                {
                    mercenaryDonArmorButton.Timer = mercenaryDonArmorButton.MaxTimer;
                    mercenaryDonArmorButton.isEffectActive = false;
                    mercenaryDonArmorButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                getDonArmorButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.G,
                true, duration, () => { mercenaryDonArmorButton.Timer = mercenaryDonArmorButton.MaxTimer; }
            );
            mercenaryDonArmorButtonText = GameObject.Instantiate(mercenaryDonArmorButton.actionButton.cooldownTimerText, mercenaryDonArmorButton.actionButton.cooldownTimerText.transform.parent);
            mercenaryDonArmorButtonText.text = "";
            mercenaryDonArmorButtonText.enableWordWrapping = false;
            mercenaryDonArmorButtonText.transform.localScale = Vector3.one * 0.5f;
            mercenaryDonArmorButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            
        }
        public static void setButtonCooldowns()
        {
            mercenaryProtectButton.Timer = 0f;
            mercenaryDonArmorButton.MaxTimer = cooldown;
            mercenaryDonArmorButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            players = new List<Mercenary>();
        }

        public static Sprite getProtectButtonSprite()
            => protectButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MercProtectButton.png", 100f);

        public static Sprite getDonArmorButtonSprite()
            => donArmorButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DonArmorButton.png", 100f);
    }
}