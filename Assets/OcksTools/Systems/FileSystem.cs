using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
    public const string GameVer = "v1.2.0";
    public const string GameFolderName = "OcksTools";
    public const string GameTrueName = "Ocks Tools";


    private string GameName = "?";
    [ReadOnly]
    public string DirectoryLol = "";
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
        DirectoryLol = Directory.GetCurrentDirectory();
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
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);
        if (CanOverride || !e)
        {
            File.WriteAllText(FileName, data);
        }


        //Environment.NewLine
    }
    public string ReadFile(string FileName)
    {
        //string fullpath = Path.Combine(DirectoryLol, FileName);
        return File.ReadAllText(FileName);
    }
    public void AppendFile(string FileName, string data)
    {
        //string fullpath = Path.Combine(DirectoryLol, FileName);
        File.AppendAllText(FileName, data);
    }
    public void CreateFolder(string FolderName)
    {
        //string fullpath = Path.Combine(DirectoryLol, FolderName);
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
    public static string ConvertWindowsAppDataToLinux(string windowsPath)
    {
        if (string.IsNullOrWhiteSpace(windowsPath))
            return windowsPath;

        windowsPath = windowsPath.Replace('/', '\\');

        var match = Regex.Match(
            windowsPath,
            @"^C:\\Users\\[^\\]+\\AppData\\Roaming\\(.+)$",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new ArgumentException("Invalid Windows AppData Roaming path format.");

        string remainingPath = match.Groups[1].Value;

        string linuxUser = Environment.UserName;

        string linuxBasePath = $"/home/{linuxUser}/.local/share";

        string linuxPath = Path.Combine(linuxBasePath, remainingPath)
            .Replace('\\', '/');

        return linuxPath;
    }

    public DownloadDataHandler DownloadFile(int type, string filelocation)
    {
        var DDH = new DownloadDataHandler();
        switch (type)
        {
            default:
            case 0:
                //image file
                StartCoroutine(GetImage(DDH, filelocation));
                break;
            case 1:
                //audio file
                StartCoroutine(GetAudioClip(DDH, filelocation));
                break;

        }
        return DDH;
    }

    private IEnumerator GetAudioClip(DownloadDataHandler DDH, string fileName)
    {
        DDH.ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(
            fileName, AudioType.MPEG);
        //Debug.Log("SexPath: "+ DirectoryLol + "/" + fileName);
        yield return webRequest.SendWebRequest();
        try
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
            clip.name = fileName;
            DDH.Clip = clip;
        }
        catch
        {
            DDH.ErrorLol = true;
        }
        DDH.CompletedDownload.SetValue(true);
    }

    private IEnumerator GetImage(DownloadDataHandler DDH, string fileName)
    {
        DDH.ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(
            fileName);

        yield return webRequest.SendWebRequest();
        try
        {
            Texture sex = DownloadHandlerTexture.GetContent(webRequest);
            sex.name = fileName;
            DDH.Texture = sex;
            DDH.Sprite = Converter.Texture2DToSprite((Texture2D)sex);
        }
        catch
        {
            DDH.ErrorLol = true;
        }
        DDH.CompletedDownload.SetValue(true);
    }

    public static void WEE(string a)
    {
        Debug.Log(a);
    }

}


public class DownloadDataHandler
{
    public bool ErrorLol = false;
    public Reactable<bool> CompletedDownload = new Reactable<bool>(false);
    public Texture Texture;
    public Sprite Sprite;
    public AudioClip Clip;
}
