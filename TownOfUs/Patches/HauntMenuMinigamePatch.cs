using System.Linq;
using System;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    public static class HauntMenuMinigamePatch
    {

        // Show the role name instead of just Crewmate / Impostor
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
        public static void Postfix(HauntMenuMinigame __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            var target = __instance.HauntTarget;

            string factionInfo = "";
            if (target.Data.Role.IsImpostor) factionInfo = "Impostor";
            else if (target.isAnyNeutral()) factionInfo = "Neutral";
            else factionInfo = "Crewmate";

            var roleInfo = RoleInfo.getRoleInfoForPlayer(target, false);
            string roleString = (roleInfo.Count > 0 && ghostsSeeRoles) ? roleInfo[0].name : "";
            if (__instance.HauntTarget.Data.IsDead)
            {
                __instance.FilterText.text = !ghostsSeeRoles ? factionInfo : roleString + " Ghost";
                return;
            }
            __instance.FilterText.text = !ghostsSeeRoles ? factionInfo : roleString;
            return;
        }

        // The impostor filter now includes neutral roles
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.MatchesFilter))]
        public static void MatchesFilterPostfix(HauntMenuMinigame __instance, PlayerControl pc, ref bool __result)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            if (__instance.filterMode == HauntMenuMinigame.HauntFilters.Impostor)
            {
                var info = RoleInfo.getRoleInfoForPlayer(pc, false);
                __result = pc.isKiller() && !pc.Data.IsDead;
            }
        }

        // Moves the haunt menu a bit further down
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.FixedUpdate))]
        public static void UpdatePostfix(HauntMenuMinigame __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !PlayerControl.LocalPlayer.isRole(RoleId.Poisoner))
                __instance.gameObject.transform.localPosition = new UnityEngine.Vector3(-6f, -1.1f, __instance.gameObject.transform.localPosition.z);
            return;
        }

        // Shows the "haunt evil roles button"
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Start))]
        public static bool StartPrefix(HauntMenuMinigame __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal || !ghostsSeeRoles) return true;
            __instance.FilterButtons[0].gameObject.SetActive(true);
            int numActive = 0;
            int numButtons = __instance.FilterButtons.Count((PassiveButton s) => s.isActiveAndEnabled);
            float edgeDist = 0.6f * (float)numButtons;
            for (int i = 0; i < __instance.FilterButtons.Length; i++)
            {
                PassiveButton passiveButton = __instance.FilterButtons[i];
                if (passiveButton.isActiveAndEnabled)
                {
                    passiveButton.transform.SetLocalX(FloatRange.SpreadToEdges(-edgeDist, edgeDist, numActive, numButtons));
                    numActive++;
                }
            }
            return false;
        }
    }
}