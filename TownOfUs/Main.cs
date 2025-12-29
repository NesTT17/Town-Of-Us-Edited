global using AmongUs.Data;
global using AmongUs.Data.Legacy;
global using AmongUs.Data.Player;
global using AmongUs.GameOptions;

global using BepInEx;
global using BepInEx.Unity.IL2CPP;
global using BepInEx.Configuration;
global using BepInEx.Unity.IL2CPP.Utils.Collections;

global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Injection;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;

global using Reactor;
global using Reactor.Utilities;
global using Reactor.Networking;
global using Reactor.Utilities.Extensions;
global using Reactor.Networking.Attributes;
global using Reactor.Networking.Extensions;

global using TownOfUs.CustomGameModes;
global using TownOfUs.Modules;
global using TownOfUs.Objects;
global using TownOfUs.Roles;
global using TownOfUs.Modifiers;
global using TownOfUs.Patches;
global using TownOfUs.Utilities;

global using static TownOfUs.GameHistory;
global using static TownOfUs.HudManagerStartPatch;
global using static TownOfUs.TOUMapOptions;
global using static TownOfUs.TownOfUs;

global using TMPro;
global using Hazel;
global using Twitch;
global using InnerNet;
global using HarmonyLib;

using System;
using TownOfUs.Modules.Debugger.Components;
using static TownOfUs.Modules.Debugger.Embedded.ReactorCoroutines.Coroutines;
using TownOfUs.Modules.CustomHats;

namespace TownOfUs
{
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [ReactorModFlags(ModFlags.RequireOnAllClients)]
    [BepInPlugin(Id, "Town Of Us", VersionString)]
    public class TownOfUsPlugin : BasePlugin
    {
        public const string Id = "townofus.reworked";
        public const string VersionString = "1.3.2";

        public static Version Version = Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public Harmony Harmony { get; } = new Harmony(Id);
        public static AssetLoader bundledAssets;
        public static TownOfUsPlugin Instance;
        public static int optionsPage = 2;

        public static bool DebuggerLoaded => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
        public static string RobotName { get; set; } = "Bot";
        public static bool Persistence { get; set; } = true;
        public static Debugger Debugger { get; set; } = null;

        public static ConfigEntry<bool> GhostsSeeInformation { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeModifier { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> ShowChatNotifications { get; set; }

        /* This is part of the Mini.RegionInstaller, Licensed under GPLv3
            file="RegionInstallPlugin.cs" company="miniduikboot"> */
        public static IRegionInfo[] defaultRegions;
        public static void UpdateRegions()
        {
            ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
            var regions = new IRegionInfo[] {
                new StaticHttpRegionInfo("Modded EU (MEU)", StringNames.NoTranslation, "au-eu.duikbo.at", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://au-eu.duikbo.at", 443, false) })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Modded NA (MNA)", StringNames.NoTranslation, "www.aumods.us", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://www.aumods.us", 443, false) })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Modded Asia (MAS)", StringNames.NoTranslation, "au-as.duikbo.at", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://au-as.duikbo.at", 443, false) })).CastFast<IRegionInfo>(),
            };

            IRegionInfo currentRegion = serverManager.CurrentRegion;
            Logger.LogInfo($"Adding {regions.Length} regions");
            foreach (IRegionInfo region in regions)
            {
                if (region == null)
                    Logger.LogError("Could not add region");
                else
                {
                    if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                        currentRegion = region;
                    serverManager.AddOrUpdateRegion(region);
                }
            }

            // AU remembers the previous region that was set, so we need to restore it
            if (currentRegion != null)
            {
                Logger.LogDebug("Resetting previous region");
                serverManager.SetRegion(currentRegion);
            }
        }

        public override void Load()
        {
            Logger = Log;
            Instance = this;
            bundledAssets = new();

            ClassInjector.RegisterTypeInIl2Cpp<Debugger>();
            ClassInjector.RegisterTypeInIl2Cpp<Component>();
            AddComponent<Component>();
            Debugger = this.AddComponent<Debugger>();

            GhostsSeeInformation = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeModifier = Config.Bind("Custom", "Ghosts See Modifier", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", true);
            ShowChatNotifications = Config.Bind("Custom", "Show Chat Notifications", true);

            defaultRegions = ServerManager.DefaultRegions;
            UpdateRegions();

            Harmony.PatchAll();

            CustomOptionHolder.Load();
            CustomColors.Load();
            CustomHatManager.LoadHats();
            MainMenuPatch.addSceneChangeCallbacks();
            AddToKillDistanceSetting.addKillDistance();
            
            AddComponent<ModUpdater>();

            Logger.LogInfo("Successfully loaded TOU!");
        }
    }

    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
    
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (!EOSManager.Instance.isKWSMinor)
            {
                DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
            }
        }
    }
}