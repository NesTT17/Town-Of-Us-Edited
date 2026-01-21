using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.Data.Legacy;

namespace TownOfUs.Modules
{
    public class CustomColors
    {
        protected static Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new List<CustomColor>();

            /* Custom Colors, starting with id (for ORDER) 18 */
            colors.Add(new CustomColor
            {
                longname = "Tamarind", //18
                color = new Color32(48, 28, 34, byte.MaxValue),
                shadow = new Color32(30, 11, 16, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Army", // 19
                color = new Color32(39, 45, 31, byte.MaxValue),
                shadow = new Color32(11, 30, 24, byte.MaxValue)
            });
            // 20
            colors.Add(new CustomColor
            {
                longname = "Olive",
                color = new Color32(154, 140, 61, byte.MaxValue),
                shadow = new Color32(104, 95, 40, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Turquoise",
                color = new Color32(22, 132, 176, byte.MaxValue),
                shadow = new Color32(15, 89, 117, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Mint",
                color = new Color32(111, 192, 156, byte.MaxValue),
                shadow = new Color32(65, 148, 111, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Lavender",
                color = new Color32(173, 126, 201, byte.MaxValue),
                shadow = new Color32(131, 58, 203, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Nougat",
                color = new Color32(160, 101, 56, byte.MaxValue),
                shadow = new Color32(115, 15, 78, byte.MaxValue)
            });
            // 25
            colors.Add(new CustomColor
            {
                longname = "Peach",
                color = new Color32(255, 164, 119, byte.MaxValue),
                shadow = new Color32(238, 128, 100, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Wasabi",
                color = new Color32(112, 143, 46, byte.MaxValue),
                shadow = new Color32(72, 92, 29, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Hot Pink",
                color = new Color32(255, 51, 102, byte.MaxValue),
                shadow = new Color32(232, 0, 58, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Petrol",
                color = new Color32(0, 99, 105, byte.MaxValue),
                shadow = new Color32(0, 61, 54, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Lemon",
                color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue),
                shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue)
            });
            // 30
            colors.Add(new CustomColor
            {
                longname = "Signal\nOrange",
                color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Teal",
                color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue),
                shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue)
            });

            colors.Add(new CustomColor
            {
                longname = "Blurple",
                color = new Color32(61, 44, 142, byte.MaxValue),
                shadow = new Color32(25, 14, 90, byte.MaxValue)
            });

            colors.Add(new CustomColor
            {
                longname = "Sunrise",
                color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue),
                shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Ice",
                color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue),
                shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue)
            });
            // 35
            colors.Add(new CustomColor
            {
                longname = "Fuchsia", //35 Color Credit: LaikosVK
                color = new Color32(164, 17, 129, byte.MaxValue),
                shadow = new Color32(104, 3, 79, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Royal\nGreen", //36
                color = new Color32(9, 82, 33, byte.MaxValue),
                shadow = new Color32(0, 46, 8, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Slime",
                color = new Color32(244, 255, 188, byte.MaxValue),
                shadow = new Color32(167, 239, 112, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Navy", //38
                color = new Color32(9, 43, 119, byte.MaxValue),
                shadow = new Color32(0, 13, 56, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Darkness", //39
                color = new Color32(36, 39, 40, byte.MaxValue),
                shadow = new Color32(10, 10, 10, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Ocean", //40
                color = new Color32(55, 159, 218, byte.MaxValue),
                shadow = new Color32(62, 92, 158, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Sundown", // 41
                color = new Color32(252, 194, 100, byte.MaxValue),
                shadow = new Color32(197, 98, 54, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Gold", // 42
                color = new Color32(255, 207, 0, byte.MaxValue),
                shadow = new Color32(191, 143, 0, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Magenta", // 43
                color = new Color32(255, 0, 127, byte.MaxValue),
                shadow = new Color32(191, 0, 95, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Melon", // 44
                color = new Color32(168, 50, 62, byte.MaxValue),
                shadow = new Color32(101, 30, 37, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Plum", // 45
                color = new Color32(79, 0, 127, byte.MaxValue),
                shadow = new Color32(55, 0, 95, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Lilac", // 46
                color = new Color32(186, 161, 255, byte.MaxValue),
                shadow = new Color32(93, 81, 128, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Sky Blue", // 47
                color = new Color32(61, 129, 255, byte.MaxValue),
                shadow = new Color32(31, 65, 128, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Azure", // 48
                color = new Color32(1, 166, 255, byte.MaxValue),
                shadow = new Color32(17, 104, 151, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Macau", // 49
                color = new Color32(0, 97, 93, byte.MaxValue),
                shadow = new Color32(0, 65, 61, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Jungle", // 50
                color = new Color32(0, 47, 0, byte.MaxValue),
                shadow = new Color32(0, 23, 0, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Chocolate", // 51
                color = new Color32(60, 48, 44, byte.MaxValue),
                shadow = new Color32(30, 24, 22, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Lime", // 52
                color = new Color32(80, 240, 57, byte.MaxValue),
                shadow = new Color32(21, 168, 66, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Aqua", // 53
                color = new Color32(61, 255, 181, byte.MaxValue),
                shadow = new Color32(31, 128, 91, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Chartreuse", // 54
                color = new Color32(207, 255, 0, byte.MaxValue),
                shadow = new Color32(143, 191, 61, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Tawny", // 55
                color = new Color32(205, 63, 0, byte.MaxValue),
                shadow = new Color32(141, 31, 0, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Snow", // 56
                color = new Color32(255, 255, 255, byte.MaxValue),
                shadow = new Color32(163, 194, 223, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Nacho", // 57
                color = new Color32(242, 166, 38, byte.MaxValue),
                shadow = new Color32(185, 87, 25, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Grass", // 58
                color = new Color32(59, 130, 90, byte.MaxValue),
                shadow = new Color32(9, 86, 73, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Mandarin", // 59
                color = new Color32(255, 149, 79, byte.MaxValue),
                shadow = new Color32(230, 52, 76, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Glass", // 60
                color = new Color32(149, 202, 220, byte.MaxValue),
                shadow = new Color32(79, 125, 161, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Ash", // 61
                color = new Color32(11, 14, 19, byte.MaxValue),
                shadow = new Color32(4, 5, 7, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Silver", // 62
                color = new Color32(54, 252, 169, byte.MaxValue),
                shadow = new Color32(30, 189, 191, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Charcoal", // 63
                color = new Color32(128, 6, 178, byte.MaxValue),
                shadow = new Color32(78, 16, 145, byte.MaxValue)
            });
            colors.Add(new CustomColor
            {
                longname = "Denim", // 64
                color = new Color32(54, 47, 188, byte.MaxValue),
                shadow = new Color32(21, 21, 129, byte.MaxValue)
            });
            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/

            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors)
            {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
            }


            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }

        protected internal struct CustomColor
        {
            public string longname;
            public Color32 color;
            public Color32 shadow;
        }

        [HarmonyPatch]
        public static class CustomColorPatches
        {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
            private class ColorStringPatch
            {
                [HarmonyPriority(Priority.Last)]
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
                {
                    if ((int)name >= 50000)
                    {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null)
                        {
                            __result = text;
                            return false;
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
            private class ChatNotificationColorsPatch
            {
                public static bool Prefix(ChatNotification __instance, PlayerControl sender, string text)
                {
                    if (ShipStatus.Instance && !showChatNotifications)
                    {
                        return false;
                    }
                    __instance.timeOnScreen = 5f;
                    __instance.gameObject.SetActive(true);
                    __instance.SetCosmetics(sender.Data);
                    string str;
                    Color color;
                    try
                    {
                        str = ColorUtility.ToHtmlStringRGB(Palette.TextColors[__instance.player.ColorId]);
                        color = Palette.TextOutlineColors[__instance.player.ColorId];
                    }
                    catch
                    {
                        Color32 c = Palette.PlayerColors[__instance.player.ColorId];
                        str = ColorUtility.ToHtmlStringRGB(c);

                        color = c.r + c.g + c.b > 180 ? Palette.Black : Palette.White;
                    }
                    __instance.playerColorText.text = __instance.player.ColorBlindName;
                    __instance.playerNameText.text = "<color=#" + str + ">" + (string.IsNullOrEmpty(sender.Data.PlayerName) ? "..." : sender.Data.PlayerName);
                    __instance.playerNameText.outlineColor = color;
                    __instance.chatText.text = text;
                    return false;
                }
            }

            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch
            {
                public static void Postfix(PlayerTab __instance)
                {
                    for (var i = 0; i < __instance.ColorChips.Count; i++)
                    {
                        var colorChip = __instance.ColorChips[i];
                        colorChip.transform.localScale *= 0.76f;
                        var x = __instance.XRange.min + (i % 8 * 0.5f);
                        var y = __instance.YStart - (i / 8 * 0.5f);
                        colorChip.transform.localPosition = new(x, y, 2f);
                    }
                }
            }

            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch
            { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
                {
                    if (!LegacySaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix()
                {
                    if (!needsPatch) return;
                    LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }

            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch
            {
                private static bool isTaken(PlayerControl player, uint color)
                {
                    foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                            return true;
                    return false;
                }
                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
                { 
                    // Fix incorrect color assignment
                    uint color = (uint)bodyColor;
                    if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                    {
                        int num = 0;
                        while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color)))
                        {
                            color = (color + 1) % CustomColors.pickableColors;
                        }
                    }
                    __instance.RpcSetColor((byte)color);
                    return false;
                }
            }
        }
    }
}