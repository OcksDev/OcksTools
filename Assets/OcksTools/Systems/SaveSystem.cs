using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public bool UseFileSystem = true;
    public SaveMethod SaveMethod_ = SaveMethod.TXTFile;
    public int test = 0;
    public bool TestBool = false;
    private OXFile oxfile;
    public static OXEvent<string> SaveAllData = new OXEvent<string>();
    public static OXEvent<string> LoadAllData = new OXEvent<string>();

    public Dictionary<string, Dictionary<string, string>> HoldingData = new Dictionary<string, Dictionary<string, string>>();

    public static SaveSystem Instance
    {
        get { return instance; }
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private void Awake()
    {
        if (Instance == null) instance = this;
        oxfile = new OXFile();
    }
    private void Start()
    {
        LoadGame();
    }
    [HideInInspector]
    public bool LoadedData = false;
    public void LoadGame(string dict = "def")
    {
        LoadedData = true;


        InputManager.AssembleTheCodes();
        var s = SoundSystem.Instance;
        List<string> list = new List<string>();
        Dictionary<string,string> dic = new Dictionary<string, string>();



        GetDataFromFile(dict);

        var ghghgg = GetString("keybinds", "fuck", dict);
        dic = Converter.StringToDictionary(ghghgg);
        List<KeyCode> shungite = new List<KeyCode>();
        if (ghghgg != "fuck")
        {
            foreach (var a in dic)
            {
                list = Converter.StringToList(a.Value);
                shungite.Clear();
                foreach(var key in list)
                {
                    shungite.Add(InputManager.namekeys[key]);
                }
                InputManager.gamekeys[a.Key] = new List<KeyCode>(shungite);
            }
        }

        if(s != null)
        {
            s.MasterVolume = float.Parse(GetString("snd_mas", "1", dict));
            s.SFXVolume = float.Parse(GetString("snd_sfx", "1", dict));
            s.MusicVolume = float.Parse(GetString("snd_mus", "1", dict));
        }

        test = int.Parse(GetString("test_num", "0", dict));
        TestBool = bool.Parse(GetString("test_bool", "False", dict));
        //ConsoleLol.Instance.ConsoleLog(Prefix(i) + "test_num");

        LoadAllData.Invoke(dict);
    }
    public void SaveGame(string dict = "def")
    {
        var s = SoundSystem.Instance;
        List<string> list = new List<string>();
        Dictionary<string, string> dic = new Dictionary<string, string>();

        dic.Clear();
        foreach (var a in InputManager.gamekeys)
        {
            list.Clear();
            foreach(var b in a.Value)
            {
                list.Add(InputManager.keynames[b]);
            }
            dic.Add(a.Key,Converter.ListToString(list));
        }
        SetString("keybinds", Converter.DictionaryToString(dic), dict);
        //PlayerPrefs.SetInt("UnitySelectMonitor", index); // sets the monitor that unity uses

        if (s != null)
        {
            SetString("snd_mas", s.MasterVolume.ToString(), dict);
            SetString("snd_sfx", s.SFXVolume.ToString(), dict);
            SetString("snd_mus", s.MusicVolume.ToString(), dict);
        }

        SetString("test_num", test.ToString(), dict);
        SetString("test_bool", TestBool.ToString(), dict);

        SaveAllData.Invoke(dict);

        SaveDataToFile(dict);
    }

    public string DictNameToFilePath(string e)
    {
        string str = ".txt";
        switch(SaveMethod_)
        {
            case SaveMethod.OXFile: str = ".ox"; break;
        }
        var f = FileSystem.Instance;
        switch (e)
        {
            case "def": return $"{f.GameDirectory}\\Game_Data{str}";
            case "ox_profile": return $"{f.UniversalDirectory}\\Player_Data.txt";
            default: return $"{f.GameDirectory}\\Data_{e}{str}";
        }
    }


    public Dictionary<string, string> GetDict(string name = "def")
    {
        if (HoldingData.ContainsKey(name))
        {
            return HoldingData[name];
        }
        else
        {
            HoldingData.Add(name, new Dictionary<string, string>());
            return HoldingData[name];
        }
    }

    public void SetString(string key, string data, string dict = "def")
    {
        var d = GetDict(dict);
        if (d.ContainsKey(key))
        {
            d[key] = data;
        }
        else
        {
            d.Add(key, data);
        }
    }

    public string GetString(string key, string defaul = "", string dict = "def")
    {
        var d = GetDict(dict);
        if (d.ContainsKey(key))
        {
            return d[key];
        }
        else
        {
            return defaul;
        }
    }

    public void SaveDataToFile(string dict = "def")
    {
        var f = FileSystem.Instance;
        f.AssembleFilePaths();
        switch (SaveMethod_)
        {
            case SaveMethod.TXTFile:
                f.WriteFile(DictNameToFilePath(dict), Converter.DictionaryToString(GetDict(dict), Environment.NewLine, ": "), true);
                break;
            case SaveMethod.OXFile:
                oxfile.Data.DataOXFiles.Clear();
                foreach(var a in GetDict(dict))
                {
                    oxfile.Data.Add(a.Key, a.Value);
                }
                oxfile.WriteFile(DictNameToFilePath(dict), true);
                break;
        }
    }


    public void GetDataFromFile(string dict = "def")
    {
        var f = FileSystem.Instance;
        f.AssembleFilePaths();
        var fp = DictNameToFilePath(dict);
        var des = GetDict(dict);
        des.Clear();
        if (!File.Exists(fp))
        {
            f.WriteFile(fp, "", false);
            return;
        }
        switch (SaveMethod_)
        {
            case SaveMethod.TXTFile:
                var s = Converter.StringToList(f.ReadFile(fp), Environment.NewLine);
                foreach (var d in s)
                {
                    if (d.IndexOf(": ") > -1)
                    {
                        des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
                    }
                }
                break;
            case SaveMethod.OXFile:
                oxfile.ReadFile(fp);
                foreach(var a in oxfile.Data.DataOXFiles)
                {
                    des.Add(a.Key, a.Value.DataString);
                }
                break;
        }
    }


    public enum SaveMethod
    {
        TXTFile,
        OXFile,
        PlayerPrefs,
    }

}
