using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CoreScripts;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Detective : RoleBase<Detective>
    {
        private static CustomButton detectiveExamineButton;
        public static Color color = new Color(0.3f, 0.3f, 1f, 1f);
        private static Sprite buttonSprite;

        public static float initialCooldown { get => CustomOptionHolder.detectiveInitialCooldown.getFloat(); }
        public static float cooldown { get => CustomOptionHolder.detectiveCooldown.getFloat(); }
        public static float bloodTime { get => CustomOptionHolder.detectiveBloodTime.getFloat(); }
        public static bool getInfoOnReport { get => CustomOptionHolder.detectiveGetInfoOnReport.getBool(); }
        public static float reportRoleDuration { get => CustomOptionHolder.detectiveReportRoleDuration.getFloat(); }
        public static float reportFactionDuration { get => CustomOptionHolder.detectiveReportFactionDuration.getFloat(); }
        public static bool getExamineInfo { get => CustomOptionHolder.detectiveGetExamineInfo.getBool(); }

        public static PlayerControl examined;
        public PlayerControl currentTarget;
        public Detective()
        {
            RoleType = roleId = RoleId.Detective;
            currentTarget = null;
        }

        public static string GetInfo(PlayerControl target)
        {
            try
            {
                var allRoleInfo = Helpers.onlineRoleInfo().OrderBy(_ => TownOfUs.rnd.Next()).ToList();
                var roleInfoTarget = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
                var AllMessage = new List<string>();
                allRoleInfo.Remove(RoleInfo.detective);
                allRoleInfo.Remove(roleInfoTarget);

                var formation = 6;
                var x = TownOfUs.rnd.Next(0, formation);
                var message = new StringBuilder();
                var tempNumList = Enumerable.Range(0, allRoleInfo.Count).ToList();
                var temp = (tempNumList.Count > formation ? tempNumList.Take(formation) : tempNumList).OrderBy(_ => TownOfUs.rnd.Next()).ToList();
                message.AppendLine($"{target.Data.PlayerName} Examine Info: \n");

                for (int num = 0, tempNum = 0; num < formation; num++, tempNum++)
                {
                    var info = allRoleInfo[temp[tempNum]];

                    message.Append(num == x ? roleInfoTarget.name : info.name);
                    message.Append(num < formation - 1 ? ", " : ';');
                }

                AllMessage.Add(message.ToString());

                return $"{message}";
            }
            catch { return "Detective Error"; }
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (player == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveResetExamine, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.detectiveResetExamine();
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
            detectiveExamineButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DetectiveExamine, SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.detectiveExamine(local.currentTarget.PlayerId);
                    detectiveExamineButton.Timer = cooldown;

                    if (local.currentTarget != null)
                    {
                        if (GameHistory.deadPlayers == null)
                        {
                            if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green);
                        }
                        else
                        {
                            if (GameHistory.deadPlayers.Any(x => x.killerIfExisting.PlayerId == local.currentTarget.PlayerId))
                            {
                                float timeSinceDeath = ((float)(DateTime.UtcNow - GameHistory.deadPlayers.Where(x => x.killerIfExisting.PlayerId == local.currentTarget.PlayerId).FirstOrDefault().timeOfDeath).TotalMilliseconds);
                                if (timeSinceDeath < bloodTime * 1000)
                                {
                                    if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.red);
                                }
                                else
                                {
                                    if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green);
                                }
                            }
                            else
                            {
                                if (!PlayerControl.LocalPlayer.isFlashedByGrenadier()) Helpers.showFlash(Color.green);
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Detective) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { detectiveExamineButton.Timer = detectiveExamineButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        { 
            detectiveExamineButton.MaxTimer = initialCooldown;
        }

        public static void Clear()
        {
            examined = null;
            players = new List<Detective>();
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ExamineButton.png", 100f);
    }
}