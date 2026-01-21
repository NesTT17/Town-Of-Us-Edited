using BepInEx.Unity.IL2CPP.Utils;
using System.Collections;
using System.IO;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;
using static TownOfUs.Modules.CustomHats.CustomHatManager;

namespace TownOfUs.Modules.CustomHats;

public class HatsLoader : MonoBehaviour
{
    private bool isRunning;

    public void FetchHats()
    {
        if (isRunning || BepInExUpdater.UpdateRequired) return;
        this.StartCoroutine(CoFetchHats());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFetchHats()
    {
        isRunning = true;
        var dh = new DownloadHandlerBuffer();
        var www = new UnityWebRequest();
        www.downloadHandler = dh;
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        TownOfUsPlugin.Logger.LogMessage($"Download manifest at: {RepositoryUrl}/{ManifestFileName}");
        www.SetUrl($"{RepositoryUrl}/{ManifestFileName}");
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            TownOfUsPlugin.Logger.LogError(www.error);
            yield break;
        }

        var response = JsonSerializer.Deserialize<SkinsConfigFile>(www.downloadHandler.text, new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        });
        www.downloadHandler.Dispose();
        www.Dispose();

        if (!Directory.Exists(HatsDirectory)) Directory.CreateDirectory(HatsDirectory);

        UnregisteredHats.AddRange(SanitizeHats(response));
        var toDownload = GenerateDownloadList(UnregisteredHats);

        TownOfUsPlugin.Logger.LogMessage($"I'll download {toDownload.Count} hat files");

        foreach (var fileName in toDownload)
        {
            yield return CoDownloadHatAsset(fileName);
        }

        isRunning = false;
    }

    private static IEnumerator CoDownloadHatAsset(string fileName)
    {
        fileName = fileName.Replace(" ", "%20");
        TownOfUsPlugin.Logger.LogMessage($"downloading hat: {fileName}");
        string url = $"{RepositoryUrl}/hats/{fileName}";
        var filePath = Path.Combine(HatsDirectory, fileName);
        filePath = filePath.Replace("%20", " ");

        var task = Helpers.DownloadFile(url, filePath);

        while (!task.IsCompleted)
            yield return null;

        if (task.Exception != null)
            TownOfUsPlugin.Logger.LogError(task.Exception);
    }
}