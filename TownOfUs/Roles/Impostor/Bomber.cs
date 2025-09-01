using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Bomber : RoleBase<Bomber>
    {
        private static CustomButton bomberButton;
        public static Color color = Palette.ImpostorRed;
        private static Sprite plantButtonSprite;
        private static Sprite detonateButtonSprite;
        public static Material bombMaterial = TownOfUsPlugin.bundledAssets.Get<Material>("bomb");

        public static int maxKills { get => Mathf.RoundToInt(CustomOptionHolder.bomberMaxKillsInDetonation.getFloat()); }
        public static float radius { get => CustomOptionHolder.bomberDetonateRadius.getFloat(); }

        public static Vector3 DetonatePoint;
        public static Bomb Bomb = new Bomb();
        public static bool placedBomb = false;
        public Bomber()
        {
            RoleType = roleId = RoleId.Bomber;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            if (player != PlayerControl.LocalPlayer) return;
            bomberButton.Timer = bomberButton.MaxTimer;
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            bomberButton = new CustomButton(
                () =>
                {
                    if (placedBomb)
                    {
                        bomberButton.Sprite = getPlantButtonSprite();

                        var playersToDie = Helpers.GetClosestPlayers(DetonatePoint, radius, false);
                        playersToDie = Helpers.Shuffle(playersToDie);
                        while (playersToDie.Count > maxKills) playersToDie.Remove(playersToDie[playersToDie.Count - 1]);
                        foreach (PlayerControl player in playersToDie)
                        {
                            Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, player);
                            Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, player);
                            Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, player, showAnimation: false);

                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                            writer.Write(player.PlayerId);
                            writer.Write((byte)DeadPlayer.CustomDeathReason.LawyerSuicide);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Bomb, PlayerControl.LocalPlayer);
                        }

                        bomberButton.Timer = bomberButton.MaxTimer;
                        PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        Bomb.ClearBomb();
                        placedBomb = false;
                    }
                    else
                    {
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        pos.z += 0.001f;
                        DetonatePoint = pos;
                        Bomb = BombExtensions.CreateBomb(pos);
                        bomberButton.Sprite = getDetonateButtonSprite();
                        bomberButton.Timer = 3f;
                        placedBomb = true;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Bomber) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    Bomb.ClearBomb();
                    bomberButton.Sprite = getPlantButtonSprite();
                    bomberButton.Timer = bomberButton.MaxTimer;
                },
                getPlantButtonSprite(), CustomButton.ButtonPositions.upperRowLeft, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            bomberButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - 2.5f;
        }

        public static void Clear()
        {
            Bomb = new Bomb();
            placedBomb = false;
            players = new List<Bomber>();
        }

        public static Sprite getPlantButtonSprite()
            => plantButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.PlantButton.png", 100f);
        public static Sprite getDetonateButtonSprite()
            => detonateButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DetonateButton.png", 100f);
    }
}