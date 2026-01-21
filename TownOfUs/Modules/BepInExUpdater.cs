using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace TownOfUs.Modules;

public class BepInExUpdater : MonoBehaviour
{
    public const string RequiredBepInExVersion = "6.0.0-be.752";
    public const string BepInExDownloadUrl32 = "https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.752%2Bdd0655f.zip";
    public const string BepInExDownloadUrl64 = "https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.752%2Bdd0655f.zip";
    public static string BepInExDownloadUrl => Environment.Is64BitProcess ? BepInExDownloadUrl64 : BepInExDownloadUrl32;
    public static bool UpdateRequired => !Paths.BepInExVersion.ToString().StartsWith(RequiredBepInExVersion);

    public void Awake()
    {
        TownOfUsPlugin.Logger.LogMessage("BepInEx Update Required...");
        TownOfUsPlugin.Logger.LogMessage($"{Paths.BepInExVersion}, {RequiredBepInExVersion} ");
        this.StartCoroutine(CoUpdate());
    }

    [HideFromIl2Cpp]
    public IEnumerator CoUpdate()
    {
        Task.Run(() => MessageBox(GetForegroundWindow(), $"Required BepInEx update ({RequiredBepInExVersion} {(Environment.Is64BitProcess ? "x64" : "x86")}) is downloading, please wait...","The Other Roles", 0));
        
        var zipPath = Path.Combine(Paths.GameRootPath, ".bepinex_update");

        var task = Helpers.DownloadFile(BepInExDownloadUrl, zipPath);
        while (!task.IsCompleted)
            yield return null;

        if (task.Exception != null)
            TownOfUsPlugin.Logger.LogError(task.Exception);

        var tempPath = Path.Combine(Path.GetTempPath(), "TheOtherUpdater.exe");
        var asm = Assembly.GetExecutingAssembly();
        var exeName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("TheOtherUpdater.exe"));
        
        using(var resource = asm.GetManifestResourceStream(exeName))
        {
            using(var file = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                resource!.CopyTo(file);
            } 
        }
        
        var startInfo = new ProcessStartInfo(tempPath, $"--game-path \"{Paths.GameRootPath}\" --zip \"{zipPath}\"");
        startInfo.UseShellExecute = false;
        Process.Start(startInfo);
        Application.Quit();
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    [DllImport("user32.dll")]
    public static extern int MessageBoxTimeout(IntPtr hwnd, String text, String title, uint type, Int16 wLanguageId, Int32 milliseconds);
}

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class StopLoadingMainMenu
{
    public static bool Prefix()
    {
        return !BepInExUpdater.UpdateRequired;
    }
}