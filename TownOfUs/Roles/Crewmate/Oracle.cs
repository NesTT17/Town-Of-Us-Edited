using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Oracle : RoleBase<Oracle>
    {
        private static CustomButton oracleButton;
        public static Color color = new Color(0.75f, 0f, 0.75f, 1f);
        private static Sprite confessButton;

        public static float accuracy { get => CustomOptionHolder.oracleRevealAccuracy.getFloat(); }
        public static float cooldown { get => CustomOptionHolder.oracleConfessCooldown.getFloat(); }
        public static bool benignNeutralsShowsEvil { get => CustomOptionHolder.oracleBenignNeutralsShowsEvil.getBool(); }
        public static bool evilNeutralsShowsEvil { get => CustomOptionHolder.oracleEvilNeutralsShowsEvil.getBool(); }
        public static bool killingNeutralsShowsEvil { get => CustomOptionHolder.oracleKillingNeutralsShowsEvil.getBool(); }

        public PlayerControl confessor;
        public PlayerControl currentTarget;
        public bool investigated;
        public FactionId revealedFactionId;
        public Oracle()
        {
            RoleType = roleId = RoleId.Oracle;
            confessor = null;
            currentTarget = null;
            investigated = false;
            revealedFactionId = FactionId.Crewmate;
        }
        
        public static List<Oracle> GetOracle(PlayerControl confessor) => players.Where(x => x.confessor == confessor).ToList();
        public static void update(MeetingHud __instance)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                foreach (var oracle in GetOracle(player))
                {
                    if (oracle.player != null && oracle.player.Data.IsDead && oracle.confessor != null)
                    {
                        foreach (var state in __instance.playerStates)
                        {
                            if (player.PlayerId != state.TargetPlayerId) continue;
                            if (player == oracle.confessor)
                            {
                                if (oracle.revealedFactionId == FactionId.Crewmate) state.NameText.text = state.NameText.text + $" <size=60%>(<color=#00FFFFFF>{accuracy}% Crew</color>) </size>";
                                else if (oracle.revealedFactionId == FactionId.Impostor) state.NameText.text = state.NameText.text + $" <size=60%>(<color=#FF0000FF>{accuracy}% Imp</color>) </size>";
                                else state.NameText.text = state.NameText.text + $" <size=60%>(<color=#808080FF>{accuracy}% Neut</color>) </size>";
                            }
                        }
                    }
                }
            }
        }

        public string GetInfo(PlayerControl target)
        {
            string msg = "";
            var evilPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && (x.Data.Role.IsImpostor || x.isBenignNeutral() && benignNeutralsShowsEvil || x.isEvilNeutral() && evilNeutralsShowsEvil || x.isKillingNeutral() && killingNeutralsShowsEvil)).ToList();
            var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && x != player && x != target).ToList();

            if (target.Data.IsDead || target.Data.Disconnected) msg = "Your confessor failed to survive so you received no confession";
            else if (allPlayers.Count < 2) msg = "Too few people alive to receive a confessional";

            if (evilPlayers.Count == 0) msg = $"{target.Data.PlayerName} confesses to knowing that there are no more evil players!";

            allPlayers.Shuffle();
            evilPlayers.Shuffle();

            var secondPlayer = allPlayers[0];
            var firstTwoEvil = false;

            foreach (var evilPlayer in evilPlayers)
            {
                if (evilPlayer == target || evilPlayer == secondPlayer) firstTwoEvil = true;
            }

            if (firstTwoEvil)
            {
                var thirdPlayer = allPlayers[1];
                msg = $"{target.Data.PlayerName} confesses to knowing that they, {secondPlayer.Data.PlayerName} and/or {thirdPlayer.Data.PlayerName} is evil!";
            }
            else
            {
                var thirdPlayer = evilPlayers[0];
                msg = $"{target.Data.PlayerName} confesses to knowing that they, {secondPlayer.Data.PlayerName} and/or {thirdPlayer.Data.PlayerName} is evil!";
            }

            return msg;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (player == PlayerControl.LocalPlayer && investigated) investigated = false;
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        { 
            oracleButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OracleConfess, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.oracleConfess(local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);

                    local.investigated = true;
                    local.currentTarget = null;
                    oracleButton.Timer = oracleButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Oracle) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && !local.investigated && PlayerControl.LocalPlayer.CanMove; },
                () => { oracleButton.Timer = oracleButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F,
                true, 0f, () => { oracleButton.Timer = oracleButton.MaxTimer; }
            );
        }
        public static void setButtonCooldowns()
        {
            oracleButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Oracle>();
        }

        public static Sprite getButtonSprite()
            => confessButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ConfessButton.png", 100f);
    }
}