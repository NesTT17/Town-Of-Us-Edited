using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Scavenger : RoleBase<Scavenger>
    {
        private static CustomButton scavengerButton;
        private static TMP_Text scavengerButtonText;
        public static Color color = new Color(0.6f, 0.15f, 0.15f, 1f);
        private static Sprite devourButton;

        public static float cooldown { get => CustomOptionHolder.scavengerCooldown.getFloat(); }
        public static int scavengerNumberToWin { get => Mathf.RoundToInt(CustomOptionHolder.scavengerNumberToWin.getFloat()); }
        public static bool canVent { get => CustomOptionHolder.scavengerCanUseVents.getBool(); }
        public static bool showArrows { get => CustomOptionHolder.scavengerShowArrows.getBool(); }
        public static bool canEatWithGuess { get => CustomOptionHolder.scavengerCanEatWithGuess.getBool(); }

        public int eatenBodies = 0;
        public bool triggerScavengerWin = false;
        public List<Arrow> localArrows = new List<Arrow>();
        public Scavenger()
        {
            RoleType = roleId = RoleId.Scavenger;
            eatenBodies = 0;
            triggerScavengerWin = false;
            if (localArrows != null)
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        Object.Destroy(arrow.arrow);
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player || localArrows == null || !showArrows) return;
            if (player.Data.IsDead) {
                foreach (Arrow arrow in localArrows) Object.Destroy(arrow.arrow);
                localArrows = [];
                return;
            }

            DeadBody[] deadBodies = Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate) {
                foreach (Arrow arrow in localArrows) Object.Destroy(arrow.arrow);
                localArrows = [];
            }

            foreach (DeadBody db in deadBodies) {
                if (arrowUpdate) {
                    localArrows.Add(new Arrow(Palette.EnabledColor));
                    localArrows[index].arrow.SetActive(true);
                }
                if (localArrows[index] != null) localArrows[index].Update(db.transform.position);
                index++;
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public override void ResetRole(bool isShifted)
        {
            if (localArrows != null)
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        Object.Destroy(arrow.arrow);
        }

        public static void makeButtons(HudManager hm)
        {
            scavengerButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, Helpers.playerById(playerInfo.PlayerId));
                                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, Helpers.playerById(playerInfo.PlayerId));

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.cleanBody(playerInfo.PlayerId, PlayerControl.LocalPlayer.PlayerId);

                                    scavengerButton.Timer = scavengerButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Scavenger) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (scavengerButtonText != null) scavengerButtonText.text = $"{scavengerNumberToWin - local.eatenBodies}";
                    return hm.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
                },
                () => { scavengerButton.Timer = scavengerButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
            scavengerButtonText = GameObject.Instantiate(scavengerButton.actionButton.cooldownTimerText, scavengerButton.actionButton.cooldownTimerText.transform.parent);
            scavengerButtonText.text = "";
            scavengerButtonText.enableWordWrapping = false;
            scavengerButtonText.transform.localScale = Vector3.one * 0.5f;
            scavengerButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            scavengerButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            if (players != null) players.Do(x => x.triggerScavengerWin = false);
            players = new List<Scavenger>();
        }

        public static Sprite getButtonSprite()
            => devourButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.DevourButton.png", 100f);
    }
}