using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Politician : RoleBase<Politician>
    {
        private static CustomButton politicianCampaignButton;
        public static Color color = new Color(0.4f, 0f, 0.6f, 1f);
        private static Sprite campaignButton;

        public static float cooldown { get => CustomOptionHolder.politicianCooldown.getFloat(); }

        public PlayerControl currentTarget;
        public static List<byte> campaignedPlayers = new List<byte>();
        public static int campaignedAlive => campaignedPlayers.Count(x => Helpers.playerById(x) != null && Helpers.playerById(x).Data != null && !Helpers.playerById(x).Data.IsDead && !Helpers.playerById(x).Data.Disconnected);
        public Politician()
        {
            RoleType = roleId = RoleId.Politician;
        }

        public static bool isCampaigned(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && campaignedPlayers.Contains(player.PlayerId) && player?.Data.IsDead == false);
        public static void campaignPlayer(PlayerControl source, PlayerControl target)
        {
            if (campaignedPlayers.Contains(source.PlayerId) && !campaignedPlayers.Contains(target.PlayerId)) campaignedPlayers.Add(target.PlayerId);
            else if (campaignedPlayers.Contains(target.PlayerId) && !campaignedPlayers.Contains(source.PlayerId)) campaignedPlayers.Add(source.PlayerId);
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (!campaignedPlayers.Contains(player.PlayerId)) campaignedPlayers.Add(player.PlayerId);
            if (player == PlayerControl.LocalPlayer)
            {
                List<PlayerControl> untargetablePlayers = [];
                foreach (var playerId in campaignedPlayers)
                {
                    PlayerControl player = Helpers.playerById(playerId);
                    untargetablePlayers.Add(player);
                }
                currentTarget = Helpers.setTarget(false, false, untargetablePlayers);
                Helpers.setPlayerOutline(currentTarget, color);

                if (campaignedPlayers.Count(x => Helpers.playerById(x) != null && Helpers.playerById(x).Data != null && !Helpers.playerById(x).Data.IsDead && !Helpers.playerById(x).Data.Disconnected) >= PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && x != null).Count() && !player.Data.IsDead)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.PoliticianTurnMayor, SendOption.Reliable, -1);
                    writer.Write(player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.politicianTurnMayor(player.PlayerId);
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            politicianCampaignButton = new CustomButton(
                () =>
                {
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    politicianCampaignButton.Timer = politicianCampaignButton.MaxTimer;
                    local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Politician) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { politicianCampaignButton.Timer = politicianCampaignButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            politicianCampaignButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            campaignedPlayers = new List<byte>();
            players = new List<Politician>();
        }
        
        public static Sprite getButtonSprite()
            => campaignButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CampaignButton.png", 100f);
    }
}