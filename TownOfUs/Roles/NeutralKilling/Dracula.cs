using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Dracula : RoleBase<Dracula>
    {
        private static CustomButton draculaBiteButton;
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static Sprite biteButton;

        public static float biteCooldown { get => CustomOptionHolder.draculaAndVampireBiteCooldown.getFloat(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.draculaHasImpostorVision.getBool(); }
        public static bool canVent { get => CustomOptionHolder.draculaCanVent.getBool(); }
        public static bool canCreateVampire = false;
        public static int maxVampires { get => Mathf.RoundToInt(CustomOptionHolder.draculaMaxVampires.getFloat()); }
        public static bool canConvertBenignNeutral { get => CustomOptionHolder.draculaCanConvertBenignNeutral.getBool(); }
        public static bool canConvertEvilNeutral { get => CustomOptionHolder.draculaCanConvertEvilNeutral.getBool(); }
        public static bool canConvertKillingNeutral { get => CustomOptionHolder.draculaCanConvertKillingNeutral.getBool(); }
        public static bool canConvertImpostor { get => CustomOptionHolder.draculaCanConvertImpostor.getBool(); }
        public static int convertedVampires = 0;

        public PlayerControl fakeVampire;
        public PlayerControl currentTarget;
        public bool wasTeamRed;
        public bool wasAgent;
        public bool wasImpostor;
        public Dracula()
        {
            RoleType = roleId = RoleId.Dracula;
            wasTeamRed = wasAgent = wasImpostor = false;
            fakeVampire = null;
        }

        public void removeCurrentDracula()
        {
            currentTarget = null;
            fakeVampire = null;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                List<PlayerControl> untargetablePlayers = new List<PlayerControl>();
                if (canConvertBenignNeutral && canConvertEvilNeutral && canConvertKillingNeutral && canConvertImpostor)
                    if (getVampire(player) != null) untargetablePlayers.Add(getVampire(player).player);
                currentTarget = Helpers.setTarget(false, true, untargetablePlayers);
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public override void ResetRole(bool isShifted)
        {
            var vampire = getVampire(player);
            if (vampire != null && vampire.player != null && !vampire.player.Data.IsDead)
                RPCProcedure.vampirePromotes(vampire.player.PlayerId);
        }

        public static Vampire getVampire(PlayerControl dracula)
        {
            return Vampire.players.FirstOrDefault(x => x.dracula != null && x.dracula.player == dracula);
        }

        public static void makeButtons(HudManager hm)
        {
            draculaBiteButton = new CustomButton(
                () =>
                {
                    if (canCreateVampire)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                        if (Mayor.players.Any(x => x.player == local.currentTarget))
                        {
                            Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, local.currentTarget);
                            draculaBiteButton.Timer = draculaBiteButton.MaxTimer;
                            local.currentTarget = null;
                            return;
                        }

                        if (VampireHunter.players.Any(x => x.player == local.currentTarget))
                        {
                            Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer);
                            draculaBiteButton.Timer = draculaBiteButton.MaxTimer;
                            local.currentTarget = null;
                            return;
                        }

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraculaCreatesVampire, Hazel.SendOption.Reliable, -1);
                        writer.Write(local.currentTarget.PlayerId);
                        writer.Write(local.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.draculaCreatesVampire(local.currentTarget.PlayerId, local.player.PlayerId);
                        draculaBiteButton.Timer = draculaBiteButton.MaxTimer;
                        local.currentTarget = null;
                    }
                    else
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
                            draculaBiteButton.Timer = draculaBiteButton.MaxTimer;
                            local.currentTarget = null;
                            return;
                        }

                        if (murder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                            draculaBiteButton.Timer = draculaBiteButton.MaxTimer;
                            local.currentTarget = null;
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Dracula) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { draculaBiteButton.Timer = draculaBiteButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            draculaBiteButton.MaxTimer = biteCooldown;
        }

        public static void Clear()
        {
            convertedVampires = 0;
            canCreateVampire = CustomOptionHolder.draculaCanCreateVampire.getBool();
            players = new List<Dracula>();
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