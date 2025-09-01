using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Seer : RoleBase<Seer>
    {
        private static CustomButton seerRevealButton;
        public static Color color = new Color(1f, 0.8f, 0.5f, 1f);
        private static Sprite revealButton;

        public static float cooldown { get => CustomOptionHolder.seerRevealCooldown.getFloat(); }
        public static bool benignNeutralsShowsEvil { get => CustomOptionHolder.seerBenignNeutralsShowsEvil.getBool(); }
        public static bool evilNeutralsShowsEvil { get => CustomOptionHolder.seerEvilNeutralsShowsEvil.getBool(); }
        public static bool killingNeutralsShowsEvil { get => CustomOptionHolder.seerKillingNeutralsShowsEvil.getBool(); }

        public PlayerControl currentTarget;
        public List<PlayerControl> revealedPlayers;
        public Seer()
        {
            RoleType = roleId = RoleId.Seer;
            revealedPlayers = new List<PlayerControl>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget(false, false, revealedPlayers);
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            seerRevealButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;
                    local.revealedPlayers.Add(local.currentTarget);
                    seerRevealButton.Timer = seerRevealButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Seer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { seerRevealButton.Timer = seerRevealButton.MaxTimer; },
                getRevealButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        
        public static void setButtonCooldowns()
        {
            seerRevealButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Seer>();
        }
        
        public static Sprite getRevealButtonSprite()
            => revealButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RevealButton.png", 100f);
    }
}