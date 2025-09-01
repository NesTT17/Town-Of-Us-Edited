using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using System.Linq;

namespace TownOfUs.Modules
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch
    {
        public static void addSceneChangeCallbacks()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                if (!scene.name.Equals("MatchMaking", StringComparison.Ordinal)) return;
                gameMode = CustomGamemodes.Classic;
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
                guesserButtonPassiveButton.OnClick.AddListener((System.Action)(() =>
                {
                    gameMode = CustomGamemodes.Guesser;
                    template.OnClick();
                }));

                var allAnyButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                allAnyButton.transform.localPosition += new Vector3(1.7f, -0.5f);
                var allAnyButtonText = allAnyButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var allAnyButtonPassiveButton = allAnyButton.GetComponentInChildren<PassiveButton>();
                
                allAnyButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                allAnyButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    gameMode = CustomGamemodes.AllAny;
                    template.OnClick();
                }));


                template.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    guesserButtonText.SetText("TOU Guesser");
                    allAnyButtonText.SetText("TOU All Any");
                })));
            }));
        }
    }

    [HarmonyPatch]
    public class MainMenuButtonHoverAnimation
    {

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Start_Postfix(MainMenuManager __instance)
        {
            var mainButtons = GameObject.Find("Main Buttons");
            mainButtons.ForEachChild((Il2CppSystem.Action<GameObject>)Init);
            static void Init(GameObject obj)
            {
                if (obj.name is "BottomButtonBounds" or "Divider") return;
                if (AllButtons.ContainsKey(obj)) return;
                SetButtonStatus(obj, false);
                var pb = obj.GetComponent<PassiveButton>();
                pb.OnMouseOver.AddListener((Action)(() => SetButtonStatus(obj, true)));
                pb.OnMouseOut.AddListener((Action)(() => SetButtonStatus(obj, false)));
            }
        }

        private static Dictionary<GameObject, (Vector3, bool)> AllButtons = new();
        private static void SetButtonStatus(GameObject obj, bool active)
        {
            AllButtons.TryAdd(obj, (obj.transform.position, active));
            AllButtons[obj] = (AllButtons[obj].Item1, active);
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate)), HarmonyPostfix]
        private static void Update_Postfix(MainMenuManager __instance)
        {
            if (GameObject.Find("MainUI") == null) return;

            foreach (var kvp in AllButtons.Where(x => x.Key != null && x.Key.active))
            {
                var button = kvp.Key;
                var pos = button.transform.position;
                var targetPos = kvp.Value.Item1 + new Vector3(kvp.Value.Item2 ? 0.35f : 0f, 0f, 0f);
                if (kvp.Value.Item2 && pos.x > (kvp.Value.Item1.x + 0.2f)) continue;
                button.transform.position = kvp.Value.Item2
                    ? Vector3.Lerp(pos, targetPos, Time.deltaTime * 2f)
                    : Vector3.MoveTowards(pos, targetPos, Time.deltaTime * 2f);
            }
        }
    }
}