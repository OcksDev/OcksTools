using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileSystem : SingleInstance<FileSystem>
{
    /* IMPORTANT:
     *    This is not the save system!
     * 
     * This class is made for interacting with disk files.
     * It can be used for saving player data, akin to how SaveSystem handles it, but its main purpose is for file interaction.
     * 
     * anyway, on with your coding
     */



    //change these to match your game
    public const string GameVer = "v1.2.5";
    public const string GameFolderName = "OcksTools";
    public const string GameTrueName = "Ocks Tools";


    private string GameName = "?";
    [ReadOnly]
    public string WorkingDirectory = "";
    [ReadOnly]
    public string OcksDirectry = "";
    [ReadOnly]
    public string UniversalDirectory = "";
    [ReadOnly]
    public string GameDirectory = "";
    [HideInInspector]
    public Dictionary<string, string> FileLocations = new Dictionary<string, string>();

    public static OXEvent LocationEvent = new OXEvent();

    public override void Awake2()
    {
        AssembleFilePaths();
        CreateFolder(OcksDirectry);
        CreateFolder(GameDirectory);
        CreateFolder(UniversalDirectory);

        WriteFile(FileLocations["OcksGames"], "", false);
        GameName = $"{GameTrueName} {GameVer}";
        var s = ReadFile(FileLocations["OcksGames"]);
        if (!s.Contains(GameName))
        {
            s += $"{GameName}\n";
            WriteFile(FileLocations["OcksGames"], s, true);
        }
    }
    private void Start()
    {
        //ConsoleLol.instance.ConsoleLog("Current File Location: " + DirectoryLol);
        //ConsoleLol.instance.ConsoleLog("Game Data Location: " + GameDirectory);

        //WriteFile($"{GameDirectory}\\Test.txt", "Test Data Lol", false);

        AssembleFilePaths();
    }
    public void AssembleFilePaths()
    {
        WorkingDirectory = Directory.GetCurrentDirectory();
        OcksDirectry = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Ocks";
        GameDirectory = OcksDirectry + "\\" + GameFolderName;
        UniversalDirectory = OcksDirectry + "\\Universal";

        FileLocations = new Dictionary<string, string>()
        {
            {"OcksGames",$"{OcksDirectry}\\Ocks_Games_Owned.txt"},
            {"OXFileTest",$"{GameDirectory}\\Testing.ox"},
        };
        LocationEvent.Invoke();
    }
    public void WriteFile(string FileName, string data, bool CanOverride)
    {
        bool e = File.Exists(FileName);
        if (CanOverride || !e)
        {
            File.WriteAllText(FileName, data);
        }


        //Environment.NewLine
    }
    public string ReadFile(string FileName)
    {
        return File.ReadAllText(FileName);
    }
    public void AppendFile(string FileName, string data)
    {
        File.AppendAllText(FileName, data);
    }
    public void CreateFolder(string FolderName)
    {
        Directory.CreateDirectory(FolderName);
    }
    public string[] ReadFilePathsInFolder(string FolderPath)
    {
        return Directory.GetFiles(FolderPath);
    }
    public string[] ReadFolderPathsInFolder(string FolderPath)
    {
        return Directory.GetDirectories(FolderPath);
    }
    public DateTime ReadFileLastWriteTime(string file)
    {
        return File.GetLastWriteTime(file);
    }
    public void DeleteFile(string file)
    {
        File.Delete(file);
    }
    public void DeleteFolder(string FolderPath)
    {
        Directory.Delete(FolderPath);
    }
    public DownloadDataHandler<Texture> LoadTexture(string filelocation)
    {
        var DDH = new DownloadDataHandler<Texture>();
        StartCoroutine(GetImage(DDH, filelocation));
        return DDH;
    }

    public DownloadDataHandler<AudioClip> LoadAudio(string filelocation)
    {
        var DDH = new DownloadDataHandler<AudioClip>();
        StartCoroutine(GetAudioClip(DDH, filelocation));
        return DDH;
    }

    private IEnumerator GetAudioClip(DownloadDataHandler<AudioClip> DDH, string fileName)
    {
        DDH.ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(
            fileName, AudioType.MPEG);
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Download failed: {webRequest.error}");
            DDH.ErrorLol = true;
        }
        else
            try
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                clip.name = fileName;
                DDH.FileContent = clip;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred while processing audio file: {e.Message}");
                DDH.ErrorLol = true;
            }
        DDH.CompletedDownload.SetValue(true);
        webRequest.Dispose();
    }

    private IEnumerator GetImage(DownloadDataHandler<Texture> DDH, string fileName)
    {
        DDH.ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(
            fileName);

        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Download failed: {webRequest.error}");
            DDH.ErrorLol = true;
        }
        else
            try
            {
                Texture sex = DownloadHandlerTexture.GetContent(webRequest);
                sex.name = fileName;
                DDH.FileContent = sex;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred while processing texture file: {e.Message}");
                DDH.ErrorLol = true;
            }
        DDH.CompletedDownload.SetValue(true);
        webRequest.Dispose();
    }

    public static void WEE(string a)
    {
        Debug.Log(a);
    }

}

public class DownloadDataHandler<T>
{
    public bool ErrorLol = false;
    public Reactable<bool> CompletedDownload = new Reactable<bool>(false);
    public T FileContent;
    public IEnumerator WaitForDownload()
    {
        yield return new WaitUntil(() => CompletedDownload.GetValue());
    }
}
