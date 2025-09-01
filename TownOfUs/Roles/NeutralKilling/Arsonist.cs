using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Arsonist : RoleBase<Arsonist>
    {
        private static CustomButton arsonistDouseButton;
        private static CustomButton arsonistIgniteButton;
        public static Color color = new Color(1f, 0.3f, 0f);
        private static Sprite douseSprite;
        private static Sprite igniteSprite;

        public static float douseCooldown { get => CustomOptionHolder.arsonistDouseCooldown.getFloat(); }
        public static float igniteCooldown { get => CustomOptionHolder.arsonistIgniteCooldown.getFloat(); }
        public static int maxAliveDoused { get => Mathf.RoundToInt(CustomOptionHolder.arsonistMaxAliveDoused.getFloat()); }
        public static bool hasImpostorVision { get => CustomOptionHolder.arsonistHasImpostorVision.getBool(); }
        public static bool canVent { get => CustomOptionHolder.arsonistCanVent.getBool(); }
        public static bool removeIgniteCooldown { get => CustomOptionHolder.arsonistRemoveIgniteCooldown.getBool(); }

        public static List<PlayerControl> dousedPlayers;

        public PlayerControl currentDouseTarget;
        public PlayerControl currentIgniteTarget;
        public bool triggerBothCooldown;
        public bool canDouse = dousedPlayers.Count(x => x != null && !x.Data.IsDead && !x.Data.Disconnected) < maxAliveDoused;
        public Arsonist()
        {
            RoleType = roleId = RoleId.Arsonist;
            currentDouseTarget = null;
            currentIgniteTarget = null;
            triggerBothCooldown = true;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                if (PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && !x.Data.Disconnected && !x.Data.IsDead && x != player && x.isKiller()) > 0) return;
                if (!removeIgniteCooldown) return;
                if (triggerBothCooldown) triggerBothCooldown = false;
            }
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                List<PlayerControl> untargetables = new List<PlayerControl>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (!dousedPlayers.Contains(player))
                        untargetables.Add(player);
                currentDouseTarget = Helpers.setTarget(false, false, dousedPlayers);
                currentIgniteTarget = Helpers.setTarget(false, true, untargetables);
                Helpers.setPlayerOutline(currentDouseTarget, color);
                Helpers.setPlayerOutline(currentIgniteTarget, Palette.ImpostorRed);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            arsonistDouseButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentDouseTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentDouseTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentDouseTarget)) return;
                    dousedPlayers.Add(local.currentDouseTarget);
                    arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                    if (local.triggerBothCooldown) arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentDouseTarget && local.canDouse && PlayerControl.LocalPlayer.CanMove; },
                () => { arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer; },
                getDouseButtonSprite(), CustomButton.ButtonPositions.upperRowCenter, hm, KeyCode.F
            );

            arsonistIgniteButton = new CustomButton(
                () =>
                {
                    foreach (PlayerControl target in dousedPlayers)
                    {
                        MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, target);
                        if (murder == MurderAttemptResult.SuppressKill)
                        {
                            Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            continue;
                        }

                        if (murder == MurderAttemptResult.BlankKill)
                        {
                            Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            if (local.triggerBothCooldown) arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                            arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                            continue;
                        }

                        if (murder == MurderAttemptResult.PerformKill)
                        {
                            Helpers.MurderPlayer(PlayerControl.LocalPlayer, target, false);
                            if (local.triggerBothCooldown) arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                            arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                            continue;
                        }

                        if (murder == MurderAttemptResult.ReverseKill)
                        {
                            Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentDouseTarget);
                            Helpers.checkMurderAttemptAndKill(target, PlayerControl.LocalPlayer);
                            if (local.triggerBothCooldown) arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer;
                            arsonistDouseButton.Timer = arsonistDouseButton.MaxTimer;
                            continue;
                        }
                    }
                    dousedPlayers.Clear();
                    dousedPlayers = new List<PlayerControl>();
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentIgniteTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { arsonistIgniteButton.Timer = arsonistIgniteButton.MaxTimer; },
                getIgniteButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );
        }
        public static void setButtonCooldowns()
        {
            arsonistDouseButton.MaxTimer = douseCooldown;
            arsonistIgniteButton.MaxTimer = igniteCooldown;
        }

        public static void Clear()
        {
            dousedPlayers = new List<PlayerControl>();
            players = new List<Arsonist>();
        }

        public static Sprite getDouseButtonSprite()
            => douseSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DouseButton.png", 115f);

        public static Sprite getIgniteButtonSprite()
            => igniteSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.IgniteButton.png", 115f);
    }
}