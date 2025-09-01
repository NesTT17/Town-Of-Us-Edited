using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static bool initialized = false;
        public static CustomButton zoomOutButton;

        public static void setCustomButtonCooldowns()
        {
            if (!initialized)
            {
                try
                {
                    createButtonsPostfix(HudManager.Instance);
                }
                catch
                {
                    TownOfUsPlugin.Logger.LogWarning("Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                    return;
                }
            }
            Sheriff.setButtonCooldowns();
            Seer.setButtonCooldowns();
            Engineer.setButtonCooldowns();
            Juggernaut.setButtonCooldowns();
            Veteran.setButtonCooldowns();
            Camouflager.setButtonCooldowns();
            Morphling.setButtonCooldowns();
            Pursuer.setButtonCooldowns();
            Politician.setButtonCooldowns();
            Mayor.setButtonCooldowns();
            Plaguebearer.setButtonCooldowns();
            Pestilence.setButtonCooldowns();
            Werewolf.setButtonCooldowns();
            Dracula.setButtonCooldowns();
            Vampire.setButtonCooldowns();
            VampireHunter.setButtonCooldowns();
            Medic.setButtonCooldowns();
            Survivor.setButtonCooldowns();
            GuardianAngel.setButtonCooldowns();
            Amnesiac.setButtonCooldowns();
            Poisoner.setButtonCooldowns();
            Scavenger.setButtonCooldowns();
            Janitor.setButtonCooldowns();
            Swooper.setButtonCooldowns();
            Oracle.setButtonCooldowns();
            Mercenary.setButtonCooldowns();
            Blackmailer.setButtonCooldowns();
            Grenadier.setButtonCooldowns();
            Glitch.setButtonCooldowns();
            Transporter.setButtonCooldowns();
            Arsonist.setButtonCooldowns();
            Thief.setButtonCooldowns();
            Lighter.setButtonCooldowns();
            Detective.setButtonCooldowns();
            Bomber.setButtonCooldowns();
            Venerer.setButtonCooldowns();
            
            Shifter.setButtonCooldowns();
            ButtonBarry.setButtonCooldowns();
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            zoomOutButton.MaxTimer = 0f;
        }

        public static void Postfix(HudManager __instance)
        {
            initialized = false;

            try
            {
                createButtonsPostfix(__instance);
            }
            catch { }
        }

        public static void createButtonsPostfix(HudManager __instance)
        {
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            Sheriff.makeButtons(__instance);
            Seer.makeButtons(__instance);
            Engineer.makeButtons(__instance);
            Juggernaut.makeButtons(__instance);
            Veteran.makeButtons(__instance);
            Camouflager.makeButtons(__instance);
            Morphling.makeButtons(__instance);
            Pursuer.makeButtons(__instance);
            Politician.makeButtons(__instance);
            Mayor.makeButtons(__instance);
            Plaguebearer.makeButtons(__instance);
            Pestilence.makeButtons(__instance);
            Werewolf.makeButtons(__instance);
            Dracula.makeButtons(__instance);
            Vampire.makeButtons(__instance);
            VampireHunter.makeButtons(__instance);
            Medic.makeButtons(__instance);
            Survivor.makeButtons(__instance);
            GuardianAngel.makeButtons(__instance);
            Amnesiac.makeButtons(__instance);
            Poisoner.makeButtons(__instance);
            Scavenger.makeButtons(__instance);
            Janitor.makeButtons(__instance);
            Swooper.makeButtons(__instance);
            Oracle.makeButtons(__instance);
            Mercenary.makeButtons(__instance);
            Blackmailer.makeButtons(__instance);
            Grenadier.makeButtons(__instance);
            Glitch.makeButtons(__instance);
            Transporter.makeButtons(__instance);
            Arsonist.makeButtons(__instance);
            Thief.makeButtons(__instance);
            Lighter.makeButtons(__instance);
            Detective.makeButtons(__instance);
            Bomber.makeButtons(__instance);
            Venerer.makeButtons(__instance);
            
            Shifter.makeButtons(__instance);
            ButtonBarry.makeButtons(__instance);

            // Zoom Button
            zoomOutButton = new CustomButton(
                () =>
                {
                    Helpers.toggleZoom();
                    zoomOutButton.Timer = 0f;
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { zoomOutButton.Timer = 0f; },
                null, new Vector3(-15f, -15f, 0f), __instance, KeyCode.KeypadPlus
            );
            zoomOutButton.Timer = 0f;
        }
    }
}