global using HarmonyLib;
global using Hazel;

global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;
global using Il2CppInterop.Runtime.Injection;

global using TownOfUs.CustomGameModes;
global using TownOfUs.Modules;
global using TownOfUs.Objects;
global using TownOfUs.Patches;
global using TownOfUs.Utilities;

global using static TownOfUs.GameHistory;
global using static TownOfUs.HudManagerStartPatch;
global using static TownOfUs.Patches.GameStartManagerPatch;
global using static TownOfUs.TownOfUs;
global using static TownOfUs.TOUEdMapOptions;

using AmongUs.Data;
using AmongUs.Data.Player;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using System;
using TownOfUs.Modules.CustomHats;

namespace TownOfUs
{
    [BepInProcess("Among Us.exe")]
    [ReactorModFlags(ModFlags.RequireOnAllClients)]
    [BepInPlugin(Id, "Town Of Us Edited", VersionString)]
    public class TownOfUsPlugin : BasePlugin
    {
        public const string Id = "com.nestt.townofus.edited";
        public const string VersionString = "1.4.0";

        public static Version Version = Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public Harmony Harmony { get; } = new Harmony(Id);
        public static AssetLoader bundledAssets;
        public static TownOfUsPlugin Instance;
        public static int optionsPage = 2;

        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeModifier { get; set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeInformation { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<bool> EnableSoundEffects { get; set; }
        public static ConfigEntry<bool> ShowVentsOnMap { get; set; }
        public static ConfigEntry<bool> ShowChatNotifications { get; set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; set; }

        /* --- This is part of the Mini.RegionInstaller, Licensed under GPLv3 
            file="RegionInstallPlugin.cs" company="miniduikboot"> --- */
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
            
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeModifier = Config.Bind("Custom", "Ghosts See Modifier", true);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeInformation = Config.Bind("Custom", "Ghosts See Information", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            EnableSoundEffects = Config.Bind("Custom", "Enable Sound Effects", true);
            ShowVentsOnMap = Config.Bind("Custom", "Show vent positions on minimap", false);
            ShowChatNotifications = Config.Bind("Custom", "Show Chat Notifications", true);
            ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");

            defaultRegions = ServerManager.DefaultRegions;
            // Removes vanilla Servers
            ServerManager.DefaultRegions = new Il2CppReferenceArray<IRegionInfo>(new IRegionInfo[0]);
            UpdateRegions();

            Harmony.PatchAll();

            CustomColors.Load();
            CustomOptionHolder.Load();
            SoundEffectsManager.Load();
            CustomHatManager.LoadHats();
            if (BepInExUpdater.UpdateRequired)
            {
                AddComponent<BepInExUpdater>();
                return;
            }
            AddComponent<ModUpdater>();
            MainMenuPatch.addSceneChangeCallbacks();
            AddToKillDistanceSetting.addKillDistance();
            Logger.LogInfo("Loading TOU Ed completed!");
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
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
    }
}