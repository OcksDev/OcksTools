using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FileSystem : MonoBehaviour
{
    /* IMPORTANT:
     *    This is not the save system!
     * 
     * This class is made for interacting with disk files.
     * It can be used for saving player data, akin to how SaveSystem handles it, but its main purpose is for file interaction.
     * 
     * anyway, on with your coding
     */





    private static FileSystem instance;
    private string GameFolderName = "OcksTools";
    private string GameName = "Ocks Tools v?";
    public string GameVer = "v1.1.0";
    [HideInInspector]
    public string DirectoryLol = "";
    [HideInInspector]
    public string OcksDirectry = "";
    [HideInInspector]
    public string UniversalDirectory = "";
    [HideInInspector]
    public string GameDirectory = "";
    [HideInInspector]
    public Dictionary<string, string> FileLocations = new Dictionary<string, string>();
    public static FileSystem Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (Instance == null) instance = this;
        AssembleFilePaths();
        CreateFolder(OcksDirectry);
        CreateFolder(GameDirectory);
        CreateFolder(UniversalDirectory);

        WriteFile(FileLocations["OcksGames"], "", false);
        GameName = $"Ocks Tools {GameVer}";
        var s = ReadFile(FileLocations["OcksGames"]);
        if (!s.Contains(GameName))
        {
            s += $"{GameName}\n";
            WriteFile(FileLocations["OcksGames"], s, true);
        }
        var pp = new bool[37];
        for(int i = 0; i < pp.Length; i++)
        {
            pp[i] = UnityEngine.Random.Range(0, 2) == 0 ? true:false ;
        }
    }
    private void Start()
    {
        //ConsoleLol.instance.ConsoleLog("Current File Location: " + DirectoryLol);
        //ConsoleLol.instance.ConsoleLog("Game Data Location: " + GameDirectory);

        WriteFile($"{GameDirectory}\\Test.txt", "Test Data Lol", false);


    }
    public void AssembleFilePaths()
    {
        DirectoryLol = Directory.GetCurrentDirectory();
        OcksDirectry = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Ocks";
        GameDirectory = OcksDirectry + "\\" + GameFolderName;
        UniversalDirectory = OcksDirectry + "\\Universal";

        //this feature might be deprecated soon
        FileLocations = new Dictionary<string, string>()
        {
            {"OcksGames",$"{OcksDirectry}\\Ocks_Games_Owned.txt"},
            {"OXFileTest",$"{GameDirectory}\\Testing.ox"},
        };
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

    public void DeleteFile(string file)
    {
        File.Delete(file);
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

    IEnumerator GetAudioClip(DownloadDataHandler DDH, string fileName)
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
        DDH.CompletedDownload = true;
    }

    IEnumerator GetImage(DownloadDataHandler DDH, string fileName)
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
        DDH.CompletedDownload = true;
    }

    public static void WEE(string a)
    {
        Debug.Log(a);
    }
}


public class DownloadDataHandler
{
    public bool ErrorLol = false;
    public bool CompletedDownload = false;
    public Texture Texture;
    public Sprite Sprite;
    public AudioClip Clip;
}
