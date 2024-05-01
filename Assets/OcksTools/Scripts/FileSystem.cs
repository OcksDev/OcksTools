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
    public string GameFolderName = "OcksTools";
    private string GameName = "Ocks Tools v?";
    [HideInInspector]
    public string GameVer = "v1.1.0";
    public string DirectoryLol = "";
    public string OcksDirectry = "";
    public string UniversalDirectory = "";
    public string GameDirectory = "";
    public List<DownloadDataHandler> DDH = new List<DownloadDataHandler>();
    public List<string> FileLocations = new List<string>();
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

        WriteFile(FileLocations[0], "", false);
        GameName = $"Ocks Tools {GameVer}";
        var s = ReadFile(FileLocations[0]);
        if (!s.Contains(GameName))
        {
            s += $"{GameName}\n";
            WriteFile(FileLocations[0], s, true);
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

        FileLocations = new List<string>()
        {
            $"{OcksDirectry}\\Ocks_Games_Owned.txt",
            $"{GameDirectory}\\Game_Data.txt",
            $"{UniversalDirectory}\\Player_Data.txt",
        };
    }
    public void WriteFile(string FileName, string data, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);
        if ((!e && !CanOverride) || (CanOverride))
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
    public int DownloadFile(int type, string filelocation)
    {
        int index = DDH.Count;
        DDH.Add(new DownloadDataHandler());
        switch (type)
        {
            default:
            case 0:
                StartCoroutine(GetAudioClip(filelocation, index));
                break;
            case 1:
                StartCoroutine(GetImage(filelocation, index));
                break;

        }
        return index;
    }

    public string BoolArrayToString(bool[] arr)
    {
        string op = arr.Length + ":";
        List<string> chars = new List<string>(){
"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","!","*"
        };

        int rollover = 0;
        int f = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            switch (rollover)
            {
                case 0:
                    f += arr[i] ? 1 : 0;
                    break;
                case 1:
                    f += arr[i] ? 2 : 0;
                    break;
                case 2:
                    f += arr[i] ? 4 : 0;
                    break;
                case 3:
                    f += arr[i] ? 8 : 0;
                    break;
                case 4:
                    f += arr[i] ? 16 : 0;
                    break;
                case 5:
                    f += arr[i] ? 32 : 0;
                    rollover = -1;
                    op += chars[f];
                    f = 0;
                    break;
            }
            rollover++;
        }
        if (rollover != 0)
        {
            op += chars[f];
        }
        return op;
    }

    public bool[] StringToBoolArray(string e)
    {
        bool[] arr = new bool[int.Parse(e.Substring(0, e.IndexOf(":")))];
        e = e.Substring(e.IndexOf(":") + 1);
        List<string> chars = new List<string>(){
"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","!","*"
        };

        int rollover = 0;
        int f = 0;
        for (int i = 0; i < e.Length; i++)
        {
            f = chars.IndexOf(e[i].ToString());
            if ((f - 32) >= 0)
            {
                f -= 32;
                arr[(i * 6) + 5] = true;
            }
            if ((f - 16) >= 0)
            {
                f -= 16;
                arr[(i * 6) + 4] = true;
            }
            if ((f - 8) >= 0)
            {
                f -= 8;
                arr[(i * 6) + 3] = true;
            }
            if ((f - 4) >= 0)
            {
                f -= 4;
                arr[(i * 6) + 2] = true;
            }
            if ((f - 2) >= 0)
            {
                f -= 2;
                arr[(i * 6) + 1] = true;
            }
            if ((f - 1) >= 0)
            {
                arr[(i * 6)] = true;
            }
        }
        return arr;
    }

    IEnumerator GetAudioClip(string fileName, int index = 0)
    {
        DDH[index].ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(
            fileName, AudioType.MPEG);
        //Debug.Log("SexPath: "+ DirectoryLol + "/" + fileName);
        yield return webRequest.SendWebRequest();
        try
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
            clip.name = fileName;
            DDH[index].Clip = clip;
        }
        catch
        {
            DDH[index].ErrorLol = true;
        }
        DDH[index].CompletedDownload = true;
    }

    IEnumerator GetImage(string fileName, int index = 0)
    {
        DDH[index].ErrorLol = false;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(
            fileName);

        yield return webRequest.SendWebRequest();
        try
        {
            Texture sex = DownloadHandlerTexture.GetContent(webRequest);
            sex.name = fileName;
            DDH[index].Texture = sex;
        }
        catch
        {
            DDH[index].ErrorLol = true;
        }
        DDH[index].CompletedDownload = true;
    }

}


public class DownloadDataHandler
{
    public bool ErrorLol = false;
    public bool CompletedDownload = false;
    public Texture Texture;
    public AudioClip Clip;
}
