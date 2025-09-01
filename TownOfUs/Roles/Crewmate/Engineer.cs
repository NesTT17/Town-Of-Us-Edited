using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Engineer : RoleBase<Engineer>
    {
        private static CustomButton engineerRepairButton;
        private static TMP_Text engineerRepairButtonText;
        public static Color color = new Color(1f, 0.65f, 0.04f, 1f);
        private static Sprite repairButton;

        public int remainingFixes = 1;

        public Engineer()
        {
            RoleType = roleId = RoleId.Engineer;
            remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        {
            engineerRepairButton = new CustomButton(
                () =>
                {
                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedRepair();
                    local.remainingFixes--;

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.engineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Engineer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (engineerRepairButtonText != null) engineerRepairButtonText.text = $"{local.remainingFixes}";
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && local.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { engineerRepairButton.Timer = engineerRepairButton.MaxTimer; },
                getRepairButtonSprite(), CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.F
            );
            engineerRepairButtonText = GameObject.Instantiate(engineerRepairButton.actionButton.cooldownTimerText, engineerRepairButton.actionButton.cooldownTimerText.transform.parent);
            engineerRepairButtonText.text = "";
            engineerRepairButtonText.enableWordWrapping = false;
            engineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
            engineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            engineerRepairButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<Engineer>();
        }

        public static Sprite getRepairButtonSprite()
            => repairButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.RepairButton.png", 115f);
    }
}