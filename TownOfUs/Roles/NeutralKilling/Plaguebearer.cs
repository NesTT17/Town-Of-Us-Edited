using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Plaguebearer : RoleBase<Plaguebearer>
    {
        private static CustomButton plaguebearerInfectButton;
        public static Color color = new Color(0.9f, 1f, 0.7f, 1f);
        private static Sprite infectButton;

        public static float cooldown { get => CustomOptionHolder.plaguebearerCooldown.getFloat(); }

        public PlayerControl currentTarget;
        public static List<byte> infectedPlayers = new List<byte>();
        public static int infectedAlive => infectedPlayers.Count(x => Helpers.playerById(x) != null && Helpers.playerById(x).Data != null && !Helpers.playerById(x).Data.IsDead && !Helpers.playerById(x).Data.Disconnected);
        public Plaguebearer()
        {
            RoleType = roleId = RoleId.Plaguebearer;
        }

        public static bool isInfected(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && infectedPlayers.Contains(player.PlayerId) && player?.Data.IsDead == false);
        public static void infectPlayer(PlayerControl source, PlayerControl target)
        {
            if (infectedPlayers.Contains(source.PlayerId) && !infectedPlayers.Contains(target.PlayerId)) infectedPlayers.Add(target.PlayerId);
            else if (infectedPlayers.Contains(target.PlayerId) && !infectedPlayers.Contains(source.PlayerId)) infectedPlayers.Add(source.PlayerId);
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        { 
            if (infectedAlive >= livingPlayers.Count && !player.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.PlaguebearerTurnPestilence, SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.plaguebearerTurnPestilence(player.PlayerId);
            }
        }
        public override void FixedUpdate()
        {
            if (!infectedPlayers.Contains(player.PlayerId)) infectedPlayers.Add(player.PlayerId);
            if (player == PlayerControl.LocalPlayer)
            {
                List<PlayerControl> untargetablePlayers = [];
                foreach (var playerId in infectedPlayers)
                {
                    PlayerControl player = Helpers.playerById(playerId);
                    untargetablePlayers.Add(player);
                }
                currentTarget = Helpers.setTarget(false, false, untargetablePlayers);
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            plaguebearerInfectButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    plaguebearerInfectButton.Timer = plaguebearerInfectButton.MaxTimer;
                    local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Plaguebearer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { plaguebearerInfectButton.Timer = plaguebearerInfectButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            plaguebearerInfectButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            infectedPlayers = new List<byte>();
            players = new List<Plaguebearer>();
        }
        
        public static Sprite getButtonSprite()
            => infectButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.InfectButton.png", 100f);
    }
}