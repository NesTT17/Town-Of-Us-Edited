using AmongUs.GameOptions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    public static class CredentialPatch
    {
        public static string fullCredentialsVersion =
@$"<size=130%><color=#018001FF>Town Of Us</color></size> v{TownOfUsPlugin.Version.ToString()}";
        public static string fullCredentials =
@$"<size=60%>Edited by <color=#018001FF>NesTT</color>
Modded <color=#018001FF>Donners</color>, <color=#018001FF>Term</color>, <color=#018001FF>-H</color> and <color=#018001FF>MyDragonBreath</color>
Formerly: <color=#018001FF>Slushiegoose</color> & <color=#018001FF>Polus.gg</color></size>";
        public static string mainMenuCredentials =
@$"Edited by <color=#018001FF>NesTT</color>
Modded <color=#018001FF>Donners</color>, <color=#018001FF>Term</color>, <color=#018001FF>-H</color> and <color=#018001FF>MyDragonBreath</color>";
        public static string contributorsCredentials =
@$"<size=60%> <color=#018001FF>Formerly: Slushiegoose & Polus.gg</color></size>";

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        internal static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TextAlignmentOptions.Top;
                var position = __instance.GetComponent<AspectPosition>();
                position.Alignment = AspectPosition.EdgeAlignments.Top;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    string gameModeText = $"";
                    if (HandleGuesser.isGuesserGm) gameModeText = $"Guesser";
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";

                    string betterPolusText = $"";
                    if (CustomOptionHolder.enableBetterPolus.getBool()) betterPolusText = $"\n<size=70%> + <color=#5E4CA6FF>BetterPolus v1.2.2</color> by Brybry</size>";

                    __instance.text.text = $"<size=130%><color=#018001FF>Town Of Us</color></size> v{TownOfUsPlugin.Version.ToString()}{betterPolusText}\n{gameModeText}" + __instance.text.text;
                    position.DistanceFromEdge = new Vector3(2.25f, 0.11f, 0);
                }
                else
                {
                    string gameModeText = $"";
                    if (TOUEdMapOptions.gameMode == CustomGamemodes.Guesser) gameModeText = $"Guesser";
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText);

                    string betterPolusText = $"";
                    if (CustomOptionHolder.enableBetterPolus.getBool()) betterPolusText = $"\n<size=70%> + <color=#5E4CA6FF>BetterPolus v1.2.2</color> by Brybry</size>";

                    __instance.text.text = $"{fullCredentialsVersion}{betterPolusText}\n{fullCredentials}\n {__instance.text.text}";
                    position.DistanceFromEdge = new Vector3(0f, 0.1f, 0);
                    try
                    {
                        var GameModeText = GameObject.Find("GameModeText")?.GetComponent<TextMeshPro>();
                        GameModeText.text = gameModeText == "" ? (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek ? "Van. HideNSeek" : "Classic") : gameModeText;
                        var ModeLabel = GameObject.Find("ModeLabel")?.GetComponentInChildren<TextMeshPro>();
                        ModeLabel.text = "Game Mode";
                    }
                    catch { }
                }
                position.AdjustPosition();
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            private static PingTracker instance;

            static void Postfix(PingTracker __instance)
            {
                var torLogo = new GameObject("bannerLogo_TOU");
                torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
                torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);

                renderer = torLogo.AddComponent<SpriteRenderer>();
                loadSprites();
                renderer.sprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.Banner.png", 100f);

                instance = __instance;
                loadSprites();
                renderer.sprite = bannerSprite;
                var credentialObject = new GameObject("credentialsTOU");
                var credentials = credentialObject.AddComponent<TextMeshPro>();
                credentials.SetText($"v{TownOfUsPlugin.Version.ToString()}\n<size=30f%>\n</size>{mainMenuCredentials}\n<size=30%>\n</size>{contributorsCredentials}");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.05f;
                credentials.transform.SetParent(torLogo.transform);
                credentials.transform.localPosition = Vector3.down * 1.25f;
            }

            public static void loadSprites()
            {
                if (bannerSprite == null) bannerSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.Banner.png", 100f);
            }
        }
    }
}