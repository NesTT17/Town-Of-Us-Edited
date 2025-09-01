using AmongUs.GameOptions;
using Reactor.Utilities.Extensions;
using System;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    class CreateGameOptionsPatch
    {
        [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.OpenTab))]
        public static void Postfix(CreateGameOptions __instance, bool isGeneral)
        {
            if (!isGeneral) return;  // Only run in general tab!
            if (__instance.modeButtons.Count > 2) return;  // already initialized!

            var template = __instance.modeButtons[0];

            int i = 0;
            foreach (var modeButton in __instance.modeButtons)
            {
                modeButton.transform.localScale = new Vector3(0.5f, 1, 1);
                modeButton.transform.localPosition = new Vector3(-0.71f + i * 1.42f, 0, -3);
                int modeSel = i;
                modeButton.OnClick.AddListener((Action)(() => { __instance.SelectMode(modeSel); }));
                for (int j = 0; j < modeButton.transform.childCount; j++)
                {
                    var child = modeButton.transform.GetChild(j);
                    for (int k = 0; k < child.childCount; k++)
                    {
                        var innerChild = child.GetChild(k);
                        innerChild.localScale = new Vector3(2, 1, 1);
                        if (i == 1)
                        {
                            var text = innerChild.GetComponent<TextMeshPro>();
                            if (text != null)
                            {
                                text.fontSizeMax = 2.5f;
                            }
                        }
                    }
                }
                i++;
            }

            var buttonList = __instance.modeButtons.ToList();
            for (int l = 2; l <= 3; l++)
            {
                var newButton = GameObject.Instantiate(template, template.transform.parent);
                newButton.transform.localPosition = new Vector3(-0.71f + 1.42f * (l), 0f, -3f);

                string tooltip = "A TOU Game Mode";

                for (int j = 0; j < newButton.transform.childCount; j++)
                {
                    var child = newButton.transform.GetChild(j);
                    for (int k = 0; k < child.childCount; k++)
                    {
                        var innerChild = child.GetChild(k);
                        var text = innerChild.GetComponent<TextMeshPro>();
                        var trnsl = innerChild.GetComponent<TextTranslatorTMP>();
                        if (trnsl != null)
                        {
                            trnsl.Destroy();
                        }
                        if (text != null)
                        {

                            string newText = "";
                            float textSize = 2f;
                            if (l == 2)
                            {
                                newText = "TOU Guesser";
                                tooltip = "Everyone can be a Guesser in this mode. Don't reveal your role or you might die!";
                            }
                            else if (l == 3)
                            {
                                newText = "TOU All Any";
                                tooltip = "TOU Classic with the removed restriction on the uniqueness of the role!";
                                textSize = 1.5f;
                            }
                            newButton.name = newText + "Option";
                            newButton.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(p => { text.SetText(newText); })));
                            text.fontSizeMax = textSize;
                        }

                    }
                }

                int mode = l;

                newButton.OnClick.RemoveAllListeners();
                newButton.OnClick.AddListener((Action)(() =>
                {
                    __instance.SelectMode(mode);
                }));
                newButton.OnMouseOver.RemoveAllListeners();
                newButton.OnMouseOver.AddListener((Action)(() =>
                {
                    __instance.tooltip.text = tooltip;
                }));
                buttonList.Add(newButton);
            }

            __instance.modeButtons = buttonList.ToArray();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.SelectMode))]
        public static bool Prefix(CreateGameOptions __instance, int i, bool saveSetting)
        {

            foreach (var modeButton in __instance.modeButtons)
            {
                modeButton.SelectButton(modeButton == __instance.modeButtons[i]);
            }

            if (i < 2)
            {
                TOUMapOptions.gameMode = CustomGamemodes.Classic;
                return true;
            }

            __instance.SetGameMode(GameModes.Normal);
            CustomGamemodes gm = (CustomGamemodes)(i - 1);
            if (gm == CustomGamemodes.Guesser)
            {
                TOUMapOptions.gameMode = CustomGamemodes.Guesser;
            }
            else if (gm == CustomGamemodes.AllAny)
            {
                TOUMapOptions.gameMode = CustomGamemodes.AllAny;
            }

            if (saveSetting)
            {
                __instance.SetGameMode(GameModes.Normal);
            }
            __instance.SwitchOptions(GameModes.Normal);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConfirmCreatePopUp), nameof(ConfirmCreatePopUp.SetupInfo))]
        public static void Postfix2(ConfirmCreatePopUp __instance)
        {
            if (TOUMapOptions.gameMode == CustomGamemodes.Classic) return;
            string modeText = "TOU Guesser";
            if (TOUMapOptions.gameMode == CustomGamemodes.AllAny) modeText = "TOU All Any";

            __instance.modeText.text = modeText;
        }
    }
}