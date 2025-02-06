using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using System.Linq;
using Reactor.Utilities.Extensions;
using AmongUs.Data;

namespace TownOfUs.Patches {
    public class GameStartManagerPatch  {
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch {
            public static void Postfix(AmongUsClient __instance) {
                GameStartManagerUpdatePatch.sendGamemode       = true;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix(GameStartManager __instance) {
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            public static float startingTimer = 0;
            private static bool update = false;
            private static string currentText = "";
            private static GameObject copiedStartButton;
            public static bool sendGamemode = true;

            public static void Prefix(GameStartManager __instance) {
                if (!GameData.Instance ) return; // No instance
                __instance.MinPlayers = 1;
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance) {
                // Display message to the host
                if (AmongUsClient.Instance.AmHost) {
                    __instance.GameStartText.transform.localPosition = Vector3.zero;
                    __instance.GameStartText.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                    if (!__instance.GameStartText.text.StartsWith("Starting")) {
                        __instance.GameStartText.text = String.Empty;
                        __instance.GameStartTextParent.SetActive(false);
                    }

                    if (__instance.startState != GameStartManager.StartingStates.Countdown)
                        copiedStartButton?.Destroy();
                    // Make starting info available to clients:
                    if (startingTimer <= 0 && __instance.startState == GameStartManager.StartingStates.Countdown) {
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
                        void StopStartFunc() {
                            __instance.ResetStartState();
                            copiedStartButton.Destroy();
                            startingTimer = 0;
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) => {
                            startButtonText.text = "";
                        })));
                    }
                }
                
                // Client update with handshake infos
                else {
                    __instance.GameStartText.transform.localPosition = Vector3.zero;
                    __instance.GameStartText.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                    if (!__instance.GameStartText.text.StartsWith("Starting")) {
                        __instance.GameStartText.text = String.Empty;
                        __instance.GameStartTextParent.SetActive(false);
                    }

                    if (!__instance.GameStartText.text.StartsWith("Starting") || !CustomOptionHolder.anyPlayerCanStopStart.getBool())
                        copiedStartButton?.Destroy();
                    if (CustomOptionHolder.anyPlayerCanStopStart.getBool() && copiedStartButton == null && __instance.GameStartText.text.StartsWith("Starting")) {

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

                        void StopStartFunc() {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StopStart, Hazel.SendOption.Reliable, AmongUsClient.Instance.HostId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            copiedStartButton.Destroy();
                            __instance.GameStartText.text = String.Empty;
                            startingTimer = 0;
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) => {
                            startButtonText.text = "";
                        })));
                    }
                }
                // Start Timer
                if (startingTimer > 0) {
                    startingTimer -= Time.deltaTime;
                }
                // Lobby timer
                if (!GameData.Instance || !__instance.PlayerCounter) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                // Show real players in lobby
                int playerCounter = 0;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (player != null)
                        playerCounter++;
                
                __instance.PlayerCounter.text = $"{playerCounter}/{GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers}";

                // Replace Lobby code
                string initialGameRoomName = __instance.GameRoomNameCode.text;
                if (DataManager.Settings.Gameplay.StreamerMode && AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame) __instance.GameRoomNameCode.text = "Town Of Us";
                else __instance.GameRoomNameCode.text = initialGameRoomName;

                if (!AmongUsClient.Instance) return;

                if (AmongUsClient.Instance.AmHost && sendGamemode && PlayerControl.LocalPlayer != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte) TOUMapOptions.gameMode);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareGamemode((byte)TOUMapOptions.gameMode);
                    sendGamemode = false;
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance) {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost) {
                    if (CustomOptionHolder.dynamicMap.getBool() && continueStart) {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        // 5 = Submerged
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
                        if (probabilities.Contains(1.0f)) {
                            for (int i=0; i < probabilities.Count; i++) {
                                if (probabilities[i] != 1.0) probabilities[i] = 0;
                            }
                        }

                        float sum = probabilities.Sum();
                        if (sum == 0) return continueStart;  // All maps set to 0, why are you doing this???
                        for (int i = 0; i < probabilities.Count; i++) {  // Normalize to [0,1]
                            probabilities[i] /= sum;
                        }
                        float selection = (float)TownOfUs.rnd.NextDouble();
                        float cumsum = 0;
                        for (byte i = 0; i < probabilities.Count; i++) {
                            cumsum += probabilities[i];
                            if (cumsum > selection) {
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
    }
}
