using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public bool UseFileSystem = true;
    //idk how needed this is tbh
    private string UniqueGamePrefix = "oxt";
    public int test = 0;


    public delegate void JustFuckingRunTheMethods();
    public static event JustFuckingRunTheMethods SaveAllData;
    public static event JustFuckingRunTheMethods LoadAllData;

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
        List<string> list = new List<string>();



        var s = SoundSystem.Instance;

        int x = 0;


        GetDataFromFile(dict);

        var ghghgg = GetString("keybinds", "fuck", dict);
        list = Converter.StringToList(ghghgg);
        if (ghghgg != "fuck")
        {
            foreach (var a in list)
            {
                try
                {
                    var sseexx = Converter.StringToList(a, "<K>");
                    InputManager.gamekeys[sseexx[0]] = InputManager.namekeys[sseexx[1]];
                    x++;
                }
                catch
                {
                }
            }
        }
        x = 0;

        if(s != null)
        {
            s.MasterVolume = float.Parse(GetString("snd_mas", "1", dict));
            s.SFXVolume = float.Parse(GetString("snd_sfx", "1", dict));
            s.MusicVolume = float.Parse(GetString("snd_mus", "1", dict));
        }

        test = int.Parse(GetString("test_num", "0", dict));
        //ConsoleLol.Instance.ConsoleLog(Prefix(i) + "test_num");

        LoadAllData?.Invoke();
    }
    public void SaveGame(string dict = "def")
    {
        /* Input Modes:
         * -1 = Save whatever is the currently selected file (by default is 0)
         * Any Other Value = Save curent data to a specfic file
         */

        List<string> list = new List<string>();
        var s = SoundSystem.Instance;

        list.Clear();
        foreach (var a in InputManager.gamekeys)
        {
            list.Add(a.Key + "<K>" + InputManager.keynames[a.Value]);
        }
        SetString("keybinds", Converter.ListToString(list), dict);
        //PlayerPrefs.SetInt("UnitySelectMonitor", index); // sets the monitor that unity uses

        if (s != null)
        {
            SetString("snd_mas", s.MasterVolume.ToString(), dict);
            SetString("snd_sfx", s.SFXVolume.ToString(), dict);
            SetString("snd_mus", s.MusicVolume.ToString(), dict);
        }

        SetString("test_num", test.ToString(), dict);

        SaveAllData?.Invoke();

        SaveDataToFile(dict);
    }

    public string DictNameToFilePath(string e)
    {
        var f = FileSystem.Instance;
        switch (e)
        {
            case "def": return $"{f.GameDirectory}\\Game_Data.txt";
            case "ox_profile": return $"{f.UniversalDirectory}\\Player_Data.txt";
            default: return $"{f.GameDirectory}\\Data_{e}.txt";
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
        f.WriteFile(DictNameToFilePath(dict), Converter.DictionaryToString(GetDict(dict), Environment.NewLine, ": "), true);
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
        var s = Converter.StringToList(f.ReadFile(fp), Environment.NewLine);
        foreach (var d in s)
        {
            if (d.IndexOf(": ") > -1)
            {
                des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
            }
        }
    }


    public string Prefix(int file)
    {
        return UniqueGamePrefix + "_";
    }


}
