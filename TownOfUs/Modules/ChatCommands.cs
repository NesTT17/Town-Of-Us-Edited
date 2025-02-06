using System;
using HarmonyLib;
using System.Linq;
using Hazel;

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
                if (sourcePlayer != null && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == true && __instance != null) __instance.NameText.color = Palette.ImpostorRed;
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
    }
}
