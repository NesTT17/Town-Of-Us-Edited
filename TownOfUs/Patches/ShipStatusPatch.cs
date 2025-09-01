using System.Linq;
using UnityEngine;

namespace TownOfUs.Patches {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch 
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] NetworkedPlayerInfo player) {
            if ((!__instance.Systems.ContainsKey(SystemTypes.Electrical) && !Helpers.isFungle()) || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;

            // If player is a role which has Impostor vision
            if (Helpers.hasImpVision(player)) {
                __result = GetNeutralLightRadius(__instance, true);
                return false;
            }

            // If player is Lighter with ability active
            if (Lighter.players.Any(x => x.player.PlayerId == player.PlayerId && x.lighterTimer > 0f)) {
                float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, unlerped);
            }
            // If player is Lawyer, apply Lawyer vision modifier
            else if (Lawyer.players.Any(x => x.player.PlayerId == player.PlayerId))
            {
                float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius * Lawyer.vision, unlerped);
                return false;
            }
            // If player is Executioner, apply Executioner vision modifier
            else if (Executioner.players.Any(x => x.player.PlayerId == player.PlayerId))
            {
                float unlerped = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius * Executioner.vision, unlerped);
                return false;
            }
            // Default light radius
            else
            {
                __result = GetNeutralLightRadius(__instance, false);
            }
            if (Torch.allPlayers.Any(x => x.PlayerId == player.PlayerId))
                __result = __instance.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod * 1;
            return false;
        }

        public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor) {
            if (isImpostor) return shipStatus.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
            float lerpValue = 1.0f;
            try {
                SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                lerpValue = switchSystem.Value / 255f;
            } catch { }

            return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
        public static void Postfix2(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }

        private static int originalNumCommonTasksOption = 0;
        private static int originalNumShortTasksOption = 0;
        private static int originalNumLongTasksOption = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static bool Prefix(ShipStatus __instance)
        {
            originalNumCommonTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks;
            originalNumShortTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks;
            originalNumLongTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks;

            var commonTaskCount = __instance.CommonTasks.Count;
            var normalTaskCount = __instance.ShortTasks.Count;
            var longTaskCount = __instance.LongTasks.Count;

            if (GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks > commonTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = commonTaskCount;
            if (GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks > normalTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = normalTaskCount;
            if (GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks > longTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = longTaskCount;
            
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static void Postfix3(ShipStatus __instance)
        {
            // Restore original settings after the tasks have been selected
            GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = originalNumCommonTasksOption;
            GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = originalNumShortTasksOption;
            GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = originalNumLongTasksOption;
        }
    }
}
