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
    private Dictionary<string, OXFile> OXFiles = new Dictionary<string, OXFile>();
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

        dic = GetDict("keybinds", new Dictionary<string,string>(), dict);
        List<KeyCode> shungite = new List<KeyCode>();
        if (dic.Count > 0)
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
        SetDict("keybinds", dic, dict);
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

    public string GetString(string key, string defaul = "", string dict = "def")
    {
        //use this method to properly query data 
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                if (!ox.Data.ContainsKey(key)) return defaul;
                var x = ox.Data[key].DataString;
                if (x != null && x != "")
                {
                    return x;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    return d[key];
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                return PlayerPrefs.GetString($"{dict}_{key}", defaul);
        }
        return ""; //code never reaches here but it makes the compiler shut up
    }
    
    public List<string> GetList(string key, List<string> defaul = null, string dict = "def")
    {
        //use this method to properly query data 
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                if (!ox.Data.ContainsKey(key)) return defaul;
                var x = ox.Data[key].DataListString;
                if (x != null && x.Count > 0)
                {
                    return x;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    var cd2 = Converter.EscapedStringToList(d[key]);
                    return cd2.Count > 0 ? cd2 : defaul;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                var cd = Converter.EscapedStringToList(PlayerPrefs.GetString($"{dict}_{key}", ""));
                return cd.Count > 0?cd:defaul;
        }
        return null; //code never reaches here but it makes the compiler shut up
    }
    public Dictionary<string,string> GetDict(string key, Dictionary<string, string> defaul = null, string dict = "def")
    {
        //use this method to properly query data 
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                if (!ox.Data.ContainsKey(key)) return defaul;
                var x = ox.Data[key].DataDictStringString;
                if (x != null && x.Count > 0)
                {
                    return x;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    var cd2 = Converter.EscapedStringToDictionary(d[key]);
                    return cd2.Count > 0 ? cd2 : defaul;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                var cd = Converter.EscapedStringToDictionary(PlayerPrefs.GetString($"{dict}_{key}", ""));
                return cd.Count > 0?cd:defaul;
        }
        return null; //code never reaches here but it makes the compiler shut up
    }


    public void SetString(string key, string data, string dict = "def")
    {
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                ox.Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    d[key] = data;
                }
                else
                {
                    d.Add(key, data);
                }
                break;
            case SaveMethod.PlayerPrefs:
                PlayerPrefs.SetString($"{dict}_{key}", data);
                break;
        }
    }
    public void SetList(string key, List<string> data, string dict = "def")
    {
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                ox.Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    d[key] = Converter.EscapedListToString(data);
                }
                else
                {
                    d.Add(key, Converter.EscapedListToString(data));
                }
                break;
            case SaveMethod.PlayerPrefs:
                PlayerPrefs.SetString($"{dict}_{key}", Converter.EscapedListToString(data));
                break;
        }
    }
    
    public void SetDict(string key, Dictionary<string,string> data, string dict = "def")
    {
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile:
                var ox = GetDictOX(dict);
                ox.Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                var d = GetDict(dict);
                if (d.ContainsKey(key))
                {
                    d[key] = Converter.EscapedDictionaryToString(data);
                }
                else
                {
                    d.Add(key, Converter.EscapedDictionaryToString(data));
                }
                break;
            case SaveMethod.PlayerPrefs:
                PlayerPrefs.SetString($"{dict}_{key}", Converter.EscapedDictionaryToString(data));
                break;
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
                var ox = GetDictOX(dict);
                ox.WriteFile(DictNameToFilePath(dict), true);
                break;
        }
    }

    public string DictNameToFilePath(string e)
    {
        string str = ".txt";
        switch (SaveMethod_)
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
    public OXFile GetDictOX(string name = "def")
    {
        if (OXFiles.ContainsKey(name))
        {
            return OXFiles[name];
        }
        else
        {
            OXFiles.Add(name, new OXFile());
            return OXFiles[name];
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
                var ox = GetDictOX(dict);
                ox.ReadFile(fp);
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
