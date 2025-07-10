global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;
global using Il2CppInterop.Runtime.Injection;

global using TownOfUs.Modules;
global using TownOfUs.Objects;
global using TownOfUs.Patches;
global using TownOfUs.Utilities;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Il2CppSystem.Security.Cryptography;
using Il2CppSystem.Text;
using Reactor.Networking.Attributes;
using AmongUs.Data;
using Reactor.Networking;
using TownOfUs.Modules.CustomHats;

namespace TownOfUs
{
    [BepInProcess("Among Us.exe")]
    [ReactorModFlags(ModFlags.RequireOnAllClients)]
    [BepInPlugin(Id, "Town Of Us", VersionString)]
    public class TownOfUsPlugin : BasePlugin
    {
        public const string Id = "me.nestt.townofus";
        public const string VersionString = "1.1.3";

        public static Version Version = Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public Harmony Harmony { get; } = new Harmony(Id);
        public static AssetLoader bundledAssets;
        public static TownOfUsPlugin Instance;
        public static int optionsPage = 2;

        public static ConfigEntry<bool> GhostsSeeInformation { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeModifier { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes{ get; set; }
        public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> ShowChatNotifications { get; set; }
        public static ConfigEntry<bool> ExtendedColorblindMode { get; set; }

        public static IRegionInfo[] defaultRegions;

        // This is part of the Mini.RegionInstaller, Licensed under GPLv3
        // file="RegionInstallPlugin.cs" company="miniduikboot">
        public static void UpdateRegions() {
            ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
            var regions = new IRegionInfo[] {
                new StaticHttpRegionInfo("Modded EU (MEU)", StringNames.NoTranslation, "au-eu.duikbo.at", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://au-eu.duikbo.at", 443, false) })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Modded NA (MNA)", StringNames.NoTranslation, "www.aumods.us", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://www.aumods.us", 443, false) })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Modded Asia (MAS)", StringNames.NoTranslation, "au-as.duikbo.at", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new ServerInfo("Http-1", "https://au-as.duikbo.at", 443, false) })).CastFast<IRegionInfo>(),
            };
            
            IRegionInfo currentRegion = serverManager.CurrentRegion;
            Logger.LogInfo($"Adding {regions.Length} regions");
            foreach (IRegionInfo region in regions) {
                if (region == null) 
                    Logger.LogError("Could not add region");
                else {
                    if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase)) 
                        currentRegion = region;               
                    serverManager.AddOrUpdateRegion(region);
                }
            }

            // AU remembers the previous region that was set, so we need to restore it
            if (currentRegion != null) {
                Logger.LogDebug("Resetting previous region");
                serverManager.SetRegion(currentRegion);
            }
        }

        public override void Load() {
            Logger = Log;

            if (IL2CPPChainloader.Instance.Plugins.Count > 2) {
                Logger.LogFatal($"Incompatible mods detected, unload Town Of Us");
                Harmony.UnpatchID("me.nestt.townofus");
                return;
            }

            Instance = this;
            bundledAssets = new();

            GhostsSeeInformation = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeModifier = Config.Bind("Custom", "Ghosts See Modifier", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", true);
            ShowChatNotifications = Config.Bind("Custom", "Show Chat Notifications", true);
            ExtendedColorblindMode = Config.Bind("Custom", "Extended Colorblind Mode", false);

            UpdateRegions();

            Harmony.PatchAll();

            CustomColors.Load();
            CustomOptionHolder.Load();
            CustomHatManager.LoadHats();
            MainMenuPatch.addSceneChangeCallbacks();
            AddToKillDistanceSetting.addKillDistance();

            AddComponent<ModUpdater>();

            Logger.LogInfo("Loading TOU completed!");
        }
    }
}