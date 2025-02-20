using System;
using HarmonyLib;
using System.Linq;
using Hazel;
using UnityEngine;

namespace TownOfUs.Modules {
    [HarmonyPatch]
    public static class ChatCommands {
        public static bool isLover(this PlayerControl player) => !(player == null) && (player == Lovers.lover1 || player == Lovers.lover2);

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat {
            public static void Postfix(HudManager __instance) {
                if (!__instance.Chat.isActiveAndEnabled && (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || (PlayerControl.LocalPlayer.isLover() && Lovers.enableChat)))
                    __instance.Chat.SetVisible(true);
                if (Blackmailer.blackmailed != null && Blackmailer.blackmailed == PlayerControl.LocalPlayer && MeetingHud.Instance) {
                    __instance.Chat.banButton.gameObject.SetActive(false);
                    __instance.Chat.backButton.gameObject.SetActive(false);
                    __instance.Chat.quickChatButton.gameObject.SetActive(false);
                    __instance.Chat.openKeyboardButton.gameObject.SetActive(false);
                    __instance.Chat.freeChatField.SetVisible(false);
                    __instance.Chat.quickChatField.SetVisible(false);
                }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public static class SetBubbleName { 
            public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName) {
                PlayerControl sourcePlayer = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data != null && x.Data.PlayerName.Equals(playerName));
                if (sourcePlayer != null && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == true && (Vampire.vampire != null && Vampire.wasTeamRed && sourcePlayer.PlayerId == Vampire.vampire.PlayerId || Dracula.dracula != null && Dracula.wasTeamRed && sourcePlayer.PlayerId == Dracula.dracula.PlayerId) && __instance != null) __instance.NameText.color = Palette.ImpostorRed;
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class AddChat {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer) {
                if (__instance != FastDestroyableSingleton<HudManager>.Instance.Chat)
                    return true;
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                return localPlayer == null || (MeetingHud.Instance != null || LobbyBehaviour.Instance != null || (localPlayer.Data.IsDead || localPlayer.isLover() && Lovers.enableChat) || (int)sourcePlayer.PlayerId == (int)PlayerControl.LocalPlayer.PlayerId);
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
        public static class ChatControllerUpdatePatch {
            public static void Postfix(ChatController __instance) {
                if (!__instance.freeChatField.textArea.hasFocus) return;
                __instance.freeChatField.textArea.AllowPaste = true;
                __instance.freeChatField.textArea.AllowSymbols = true;
                __instance.freeChatField.textArea.AllowEmail = true;
                __instance.freeChatField.textArea.allowAllCharacters = true;
            }
        }

        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Start))]
        public static class TextBoxTMPStartPatch {
            public static void Postfix(TextBoxTMP __instance) {
                __instance.allowAllCharacters = true;
                __instance.AllowEmail = true; 
                __instance.AllowPaste = true;
                __instance.AllowSymbols = true;
            }
        }

        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Update))]
        public static class TextBoxTMPUpdatePatch {
            public static void Postfix(TextBoxTMP __instance) {
                if (!__instance.hasFocus) return;
                if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C)) {
                    ClipboardHelper.PutClipboardString(__instance.text);
                }
            }
        }
    }
}