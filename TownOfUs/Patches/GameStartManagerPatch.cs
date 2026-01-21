using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.Patches
{
    public class GameStartManagerPatch
    {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        public static float timer = 600f;
        internal static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix(AmongUsClient __instance)
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    Helpers.shareGameVersion();
                }
                GameStartManagerUpdatePatch.sendGamemode = true;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f;
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            public static float startingTimer = 0;
            private static bool update = false;
            private static string currentText = "";
            private static GameObject copiedStartButton;
            public static bool sendGamemode = true;

            public static void Prefix(GameStartManager __instance)
            {
                if (!GameData.Instance) return; // No instance
                __instance.MinPlayers = 1;
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent)
                {
                    versionSent = true;
                    Helpers.shareGameVersion();
                    if (AmongUsClient.Instance.AmHost)
                    {
                        GameManager.Instance.LogicOptions.SyncOptions();
                        CustomOption.ShareOptionSelections();
                    }
                }

                // Display message to the host
                if (AmongUsClient.Instance.AmHost)
                {
                    __instance.GameStartText.transform.localPosition = Vector3.zero;
                    __instance.GameStartText.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                    if (!__instance.GameStartText.text.StartsWith("Starting"))
                    {
                        __instance.GameStartText.text = String.Empty;
                        __instance.GameStartTextParent.SetActive(false);
                    }

                    if (__instance.startState != GameStartManager.StartingStates.Countdown)
                        copiedStartButton?.Destroy();

                    // Make starting info available to clients:
                    if (startingTimer <= 0 && __instance.startState == GameStartManager.StartingStates.Countdown)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGameStarting, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setGameStarting();
                        // Activate Stop-Button
                        copiedStartButton = GameObject.Instantiate(__instance.StartButton.gameObject, __instance.StartButton.gameObject.transform.parent);
                        copiedStartButton.transform.localPosition = __instance.StartButton.transform.localPosition;
                        copiedStartButton.SetActive(true);
                        var startButtonText = copiedStartButton.GetComponentInChildren<TMPro.TextMeshPro>();
                        startButtonText.text = "";
                        startButtonText.fontSize *= 0.8f;
                        startButtonText.fontSizeMax = startButtonText.fontSize;
                        startButtonText.gameObject.transform.localPosition = Vector3.zero;
                        PassiveButton startButtonPassiveButton = copiedStartButton.GetComponent<PassiveButton>();
                        void StopStartFunc()
                        {
                            __instance.ResetStartState();
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StopStart, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            copiedStartButton.Destroy();
                            startingTimer = 0;
                            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) =>
                        {
                            startButtonText.text = "";
                        })));
                    }
                }

                // Client update with handshake infos
                else
                {
                    __instance.GameStartText.transform.localPosition = Vector3.zero;
                    __instance.GameStartText.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                    if (!__instance.GameStartText.text.StartsWith("Starting"))
                    {
                        __instance.GameStartText.text = String.Empty;
                        __instance.GameStartTextParent.SetActive(false);
                    }

                    if (!__instance.GameStartText.text.StartsWith("Starting") || !CustomOptionHolder.anyPlayerCanStopStart.getBool())
                        copiedStartButton?.Destroy();

                    if (CustomOptionHolder.anyPlayerCanStopStart.getBool() && copiedStartButton == null && __instance.startState == GameStartManager.StartingStates.Countdown)
                    {
                        // Activate Stop-Button
                        copiedStartButton = GameObject.Instantiate(__instance.StartButton.gameObject, __instance.StartButton.gameObject.transform.parent);
                        copiedStartButton.transform.localPosition = __instance.StartButton.transform.localPosition;
                        copiedStartButton.SetActive(true);
                        var startButtonText = copiedStartButton.GetComponentInChildren<TMPro.TextMeshPro>();
                        startButtonText.text = "";
                        startButtonText.fontSize *= 0.8f;
                        startButtonText.fontSizeMax = startButtonText.fontSize;
                        startButtonText.gameObject.transform.localPosition = Vector3.zero;
                        PassiveButton startButtonPassiveButton = copiedStartButton.GetComponent<PassiveButton>();

                        void StopStartFunc()
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StopStart, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            copiedStartButton.Destroy();
                            __instance.GameStartText.text = String.Empty;
                            startingTimer = 0;
                            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) =>
                        {
                            startButtonText.text = "";
                        })));
                    }
                }

                // Start Timer
                if (startingTimer > 0)
                {
                    startingTimer -= Time.deltaTime;
                }
                // Lobby timer
                if (!GameData.Instance || !__instance.PlayerCounter) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                if (!AmongUsClient.Instance) return;

                if (AmongUsClient.Instance.AmHost && sendGamemode && PlayerControl.LocalPlayer != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)TOUEdMapOptions.gameMode);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareGamemode((byte)TOUEdMapOptions.gameMode);
                    sendGamemode = false;
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame
        {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost)
                {                    
                    if (CustomOptionHolder.dynamicMap.getBool() && continueStart)
                    {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        // 5 = Fungle
                        byte chosenMapId = 0;
                        List<float> probabilities =
                        [
                            CustomOptionHolder.dynamicMapEnableSkeld.getSelection() / 10f,
                            CustomOptionHolder.dynamicMapEnableMira.getSelection() / 10f,
                            CustomOptionHolder.dynamicMapEnablePolus.getSelection() / 10f,
                            CustomOptionHolder.dynamicMapEnableAirShip.getSelection() / 10f,
                            CustomOptionHolder.dynamicMapEnableFungle.getSelection() / 10f,
                        ];

                        // if any map is at 100%, remove all maps that are not!
                        if (probabilities.Contains(1.0f))
                        {
                            for (int i = 0; i < probabilities.Count; i++)
                            {
                                if (probabilities[i] != 1.0) probabilities[i] = 0;
                            }
                        }

                        float sum = probabilities.Sum();
                        if (sum == 0) return continueStart;  // All maps set to 0, why are you doing this???
                        for (int i = 0; i < probabilities.Count; i++)
                        {  // Normalize to [0,1]
                            probabilities[i] /= sum;
                        }
                        float selection = (float)rnd.NextDouble();
                        float cumsum = 0;
                        for (byte i = 0; i < probabilities.Count; i++)
                        {
                            cumsum += probabilities[i];
                            if (cumsum > selection)
                            {
                                chosenMapId = i;
                                break;
                            }
                        }

                        if (chosenMapId >= 3) chosenMapId++;  // Skip dlekS

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                        writer.Write(chosenMapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.dynamicMapOption(chosenMapId);
                    }
                }
                return continueStart;
            }
        }
        
        public class PlayerVersion
        {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid)
            {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}