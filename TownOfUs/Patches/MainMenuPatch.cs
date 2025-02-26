using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using AmongUs.Data;
using Assets.InnerNet;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch {
        public static void addSceneChangeCallbacks() {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) => {
                if (!scene.name.Equals("MatchMaking", StringComparison.Ordinal)) return;
                TOUMapOptions.gameMode = CustomGamemodes.Classic;
                // Add buttons For Guesser Mode, Hide N Seek in this scene.
                // find "HostLocalGameButton"
                var template = GameObject.FindObjectOfType<HostLocalGameButton>();
                var gameButton = template.transform.FindChild("CreateGameButton");
                var gameButtonPassiveButton = gameButton.GetComponentInChildren<PassiveButton>();

                var guesserButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                guesserButton.transform.localPosition += new Vector3(0f, -0.5f);
                var guesserButtonText = guesserButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var guesserButtonPassiveButton = guesserButton.GetComponentInChildren<PassiveButton>();
                
                guesserButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                guesserButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TOUMapOptions.gameMode = CustomGamemodes.Guesser;
                    template.OnClick();
                }));

                template.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                    guesserButtonText.SetText("TOU Guesser");
                })));
            }));
        }

        public static void Postfix(MainMenuManager __instance) {
            Application.targetFrameRate = 165;
        }

        private static void Postfix()
        {
            CustomNameplateLoader.LaunchNameplateFetcher();
        }
    }
}