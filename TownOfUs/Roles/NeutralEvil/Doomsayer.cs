using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Doomsayer : RoleBase<Doomsayer>
    {
        private static CustomButton doomsayerObserveButton;
        public static Color color = new Color(0f, 1f, 0.5f, 1f);
        private static Sprite buttonSprite;

        public static float cooldown { get => CustomOptionHolder.doomsayerObserveCooldown.getFloat(); }
        public static bool canObserve { get => !CustomOptionHolder.doomsayerCantObserve.getBool(); }
        public static int guessesToWin { get => Mathf.RoundToInt(CustomOptionHolder.doomsayerGuessesToWin.getFloat()); }
        public static bool triggerDoomsayerWin = false;

        public int numOfGuesses;
        public PlayerControl currentTarget;
        public Doomsayer()
        {
            RoleType = roleId = RoleId.Doomsayer;
            numOfGuesses = 0;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
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
            doomsayerObserveButton = new CustomButton(
                () =>
                {
                    var msg = GetInfo(local.currentTarget);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{msg}");

                    // Ghost Info
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer;
                },
                () => { return canObserve && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Doomsayer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { doomsayerObserveButton.Timer = doomsayerObserveButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
        }
        public static void setButtonCooldowns()
        {
            doomsayerObserveButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Doomsayer>();
            triggerDoomsayerWin = false;
        }

        public static string GetInfo(PlayerControl target)
        {
            try
            {
                var roleList = allRoleInfos().OrderBy(_ => rnd.Next()).ToList();
                var roleInfoTarget = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();

                var AllMessage = new List<string>();
                roleList.Remove(RoleInfo.doomsayer);
                roleList.Remove(roleInfoTarget);

                var formation = 6;
                var x = rnd.Next(0, formation);
                var message = new StringBuilder();
                var tempNumList = Enumerable.Range(0, roleList.Count).ToList();
                var temp = (tempNumList.Count > formation ? tempNumList.Take(formation) : tempNumList).OrderBy(_ => rnd.Next()).ToList();

                message.AppendLine($"{target?.Data?.PlayerName}'s Observe Info:\n");

                for (int num = 0, tempNum = 0; num < formation; num++, tempNum++)
                {
                    var info = roleList[temp[tempNum]];

                    message.Append(num == x ? roleInfoTarget.name : info.name);
                    message.Append(num < formation - 1 ? ", " : ';');
                }

                AllMessage.Add(message.ToString());

                return $"{message}";
            }
            catch
            {
                return "Doomsayer Error";
            }
        }

        public static List<RoleInfo> allRoleInfos()
        {
            var allRoleInfo = new List<RoleInfo>();
            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            foreach (var roleInfo in RoleInfo.allRoleInfos)
            {
                if (roleInfo.factionId == FactionId.Modifier) continue;
                if (roleInfo.roleId == RoleId.Pestilence) continue;
                if (roleInfo.roleId == RoleId.Mayor) continue;
                if (roleData.nonKillingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.nonKillingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.killingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.killingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleInfo.roleId == RoleId.Vampire && (!CustomOptionHolder.draculaCanCreateVampire.getBool() || CustomOptionHolder.draculaSpawnRate.getSelection() == 0)) continue;
                else if (roleInfo.roleId == RoleId.VampireHunter && CustomOptionHolder.draculaSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Pursuer && CustomOptionHolder.lawyerSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Survivor && CustomOptionHolder.guardianAngelSpawnRate.getSelection() == 0) continue;
                if (Snitch.exists && HandleGuesser.guesserCantGuessSnitch)
                {
                    if (Snitch.allPlayers.Any(x => TasksHandler.taskInfo(x.Data).Item2 - TasksHandler.taskInfo(x.Data).Item1 <= 0) && roleInfo.roleId == RoleId.Snitch) continue;
                }
                allRoleInfo.Add(roleInfo);
            }
            return allRoleInfo;
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.ObserveButton.png", 100f);
    }
}