using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Vampire : RoleBase<Vampire>
    {
        private static CustomButton vampireBiteButton;
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static Sprite biteButton;

        public static float cooldown { get => CustomOptionHolder.draculaAndVampireBiteCooldown.getFloat(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.vampireHasImpostorVision.getBool(); }
        public static bool canVent { get => CustomOptionHolder.vampireCanVent.getBool(); }

        public PlayerControl currentTarget;
        public bool wasTeamRed;
        public bool wasAgent;
        public bool wasImpostor;
        public Dracula dracula;
        public Vampire()
        {
            RoleType = roleId = RoleId.Vampire;
            dracula = null;
            currentTarget = null;
            wasTeamRed = wasAgent = wasImpostor = false;
        }

        private void vampireSetTarget()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                var untargetablePlayers = new List<PlayerControl>();
                if (dracula != null) untargetablePlayers.Add(dracula.player);
                currentTarget = Helpers.setTarget(false, true, untargetablePlayers);
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }

        private void vampireCheckPromotion()
        {
            if (player.Data.IsDead == true) return;
            if (dracula == null || dracula.player == null || dracula.player.Data.IsDead || dracula?.player.Data?.Disconnected == true)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampirePromotes, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampirePromotes(PlayerControl.LocalPlayer.PlayerId);
            }
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            vampireSetTarget();
            vampireCheckPromotion();
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            vampireBiteButton = new CustomButton(
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
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer);
                        return;
                    }
                    if (murder == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        vampireBiteButton.Timer = vampireBiteButton.MaxTimer;
                        local.currentTarget = null;
                        return;
                    }

                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        vampireBiteButton.Timer = vampireBiteButton.MaxTimer;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Vampire) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { vampireBiteButton.Timer = vampireBiteButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            vampireBiteButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Vampire>();
        }

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public static Sprite getButtonSprite()
            => biteButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.BiteButton.png", 100f);
    }
}