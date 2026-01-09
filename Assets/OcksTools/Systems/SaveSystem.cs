using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SaveSystem;

public class SaveSystem : SingleInstance<SaveSystem>
{
    public SaveMethod SaveMethod_ = SaveMethod.TXTFile;
    public int test = 0;
    public bool TestBool = false;
    public KeyCode testkeybind = 0;
    public static OXEventLayered<SaveProfile> SaveAllData = new OXEventLayered<SaveProfile>();
    public static OXEventLayered<SaveProfile> LoadAllData = new OXEventLayered<SaveProfile>();

    public static SaveProfile SaveProfileGlobal = null;
    public static Dictionary<string, SaveProfile> SaveProfiles = new Dictionary<string, SaveProfile>();

    public static string ActiveDir;
    public static SaveProfile ActiveProf;
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private void Start()
    {
        LoadGame();
    }
    [HideInInspector]
    public bool LoadedData = false;
    public void LoadGame(string dict = "Profile1")
    {
        var prof = Profile(dict);
        LoadedData = true;
        ActiveDir = dict;
        ActiveProf = prof;

        InputManager.AssembleTheCodes();
        var s = SoundSystem.Instance;
        List<string> list = new List<string>();
        Dictionary<string, string> dic = new Dictionary<string, string>();


        GetDataFromFile(GlobalProfile());
        GetDataFromFile(prof);

        dic = prof.GetDict("keybinds", new Dictionary<string, string>());
        List<KeyCode> shungite = new List<KeyCode>();
        if (dic.Count > 0)
        {
            foreach (var a in dic)
            {
                list = Converter.StringToList(a.Value);
                shungite.Clear();
                foreach (var key in list)
                {
                    shungite.Add(InputManager.namekeys[key]);
                }
                InputManager.gamekeys[a.Key] = new List<KeyCode>(shungite);
            }
        }

        if (s != null)
        {
            s.MasterVolume = float.Parse(prof.GetString("snd_mas", "1"));
            s.SFXVolume = float.Parse(prof.GetString("snd_sfx", "1"));
            s.MusicVolume = float.Parse(prof.GetString("snd_mus", "1"));
        }

        test = int.Parse(prof.GetString("test_num", "0"));
        TestBool = bool.Parse(prof.GetString("test_bool", "False"));
        testkeybind = InputManager.namekeys[prof.GetString("test_keybind", "NONE")];
        //ConsoleLol.Instance.ConsoleLog(Prefix(i) + "test_num");
        LoadAllData.Invoke(prof);
    }
    public void SaveGame(string dict = "Profile1")
    {
        var prof = Profile(dict);
        var s = SoundSystem.Instance;
        List<string> list = new List<string>();
        Dictionary<string, string> dic = new Dictionary<string, string>();

        dic.Clear();
        foreach (var a in InputManager.gamekeys)
        {
            list.Clear();
            foreach (var b in a.Value)
            {
                list.Add(InputManager.keynames[b]);
            }
            dic.Add(a.Key, Converter.ListToString(list));
        }
        prof.SetDict("keybinds", dic);
        //PlayerPrefs.SetInt("UnitySelectMonitor", index); // sets the monitor that unity uses

        if (s != null)
        {
            prof.SetString("snd_mas", s.MasterVolume.ToString());
            prof.SetString("snd_sfx", s.SFXVolume.ToString());
            prof.SetString("snd_mus", s.MusicVolume.ToString());
        }

        prof.SetString("test_num", test.ToString());
        prof.SetString("test_bool", TestBool.ToString());
        prof.SetString("test_keybind", InputManager.keynames[testkeybind]);

        SaveAllData.Invoke(prof);

        SaveDataToFile(GlobalProfile());
        SaveDataToFile(prof);
    }


    public void SaveDataToFile(SaveProfile prof)
    {
        var f = FileSystem.Instance;
        f.AssembleFilePaths();
        switch (SaveMethod_)
        {
            case SaveMethod.TXTFile:
                f.WriteFile(PathOfProfile(prof), Converter.DictionaryToString(prof.SavedData, Environment.NewLine, ": "), true);
                break;
            case SaveMethod.OXFile:
                var ox = prof.GetOX();
                ox.WriteFile(PathOfProfile(prof), true);
                break;
        }
    }

    public string PathOfProfile(SaveProfile prof)
    {
        string str = ".txt";
        switch (SaveMethod_)
        {
            case SaveMethod.OXFile: str = ".ox"; break;
        }
        var f = FileSystem.Instance;
        if (prof.IsGlobal)
        {
            return $"{f.GameDirectory}\\Global_Data{str}";
        }
        switch (prof.Name)
        {
            case "ox_profile": return $"{f.UniversalDirectory}\\Player_Data.txt";
            case "console": return $"{f.GameDirectory}\\Console_Data{str}";
            default: return $"{f.GameDirectory}\\Data_{prof.Name}{str}";
        }
    }

    public void GetDataFromFile(SaveProfile prof)
    {
        var f = FileSystem.Instance;
        f.AssembleFilePaths();
        var fp = PathOfProfile(prof);
        var des = prof.SavedData;
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
                var ox = prof.GetOX();
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
    public static SaveProfile Profile(string name)
    {
        if (SaveProfiles.ContainsKey(name))
        {
            return SaveProfiles[name];
        }
        var p = new SaveProfile(name, Instance.SaveMethod_);
        SaveProfiles.Add(name, p);
        return p;
    }
    public static SaveProfile GlobalProfile()
    {
        if (SaveProfileGlobal != null) return SaveProfileGlobal;
        var p = new SaveProfile("GlobalProfile", Instance.SaveMethod_);
        p.IsGlobal = true;
        SaveProfileGlobal = p;
        return p;
    }
}

public class SaveProfile
{
    public string Name = "-";
    public SaveMethod SaveMethod = SaveMethod.TXTFile;
    public bool IsGlobal = false;
    public Dictionary<string, string> SavedData = new Dictionary<string, string>();
    public SaveProfile(string name, SaveMethod sm)
    {
        Name = name;
        SaveMethod = sm;
    }
    public void SetString(string key, string data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data);
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data);
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data);
                }
                break;
        }
    }


    public void SetDict(string key, Dictionary<string, string> data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, Converter.EscapedDictionaryToString(data));
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", Converter.EscapedDictionaryToString(data));
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", Converter.EscapedDictionaryToString(data));
                }
                break;
        }
    }



    public void SetList(string key, List<string> data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, Converter.EscapedListToString(data));
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", Converter.EscapedListToString(data));
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", Converter.EscapedListToString(data));
                }
                break;
        }
    }
    public void SetObject<A>(string key, A data)
    {
        SetString(key, data.ToString());
    }

    public void SetList<A>(string key, List<A> data)
    {
        SetList(key, data.AListToStringList());
    }

    public void SetDict<A, B>(string key, Dictionary<A, B> data)
    {
        SetDict(key, data.ABDictionaryToStringDictionary());
    }



    public string GetString(string key, string defaul = "")
    {
        //use this method to properly query data 
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                var ox = GetOX();
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
                if (SavedData.ContainsKey(key))
                {
                    return SavedData[key];
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    return PlayerPrefs.GetString($"_Global_{key}", defaul);
                }
                else
                {
                    return PlayerPrefs.GetString($"={Name}_{key}", defaul);
                }
        }
        return ""; //code never reaches here but it makes the compiler shut up
    }

    public List<string> GetList(string key, List<string> defaul = null)
    {
        //use this method to properly query data 
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                var ox = GetOX();
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
                if (SavedData.ContainsKey(key))
                {
                    var cd2 = Converter.EscapedStringToList(SavedData[key]);
                    return cd2.Count > 0 ? cd2 : defaul;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    var cd = Converter.EscapedStringToList(PlayerPrefs.GetString($"_Global_{key}", ""));
                    return cd.Count > 0 ? cd : defaul;
                }
                else
                {
                    var cd = Converter.EscapedStringToList(PlayerPrefs.GetString($"={Name}_{key}", ""));
                    return cd.Count > 0 ? cd : defaul;
                }
        }
        return null; //code never reaches here but it makes the compiler shut up
    }



    public Dictionary<string, string> GetDict(string key, Dictionary<string, string> defaul = null)
    {
        //use this method to properly query data 
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                var ox = GetOX();
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
                if (SavedData.ContainsKey(key))
                {
                    var cd2 = Converter.EscapedStringToDictionary(SavedData[key]);
                    return cd2.Count > 0 ? cd2 : defaul;
                }
                else
                {
                    return defaul;
                }
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    var cd = Converter.EscapedStringToDictionary(PlayerPrefs.GetString($"_Global_{key}", ""));
                    return cd.Count > 0 ? cd : defaul;
                }
                else
                {
                    var cd = Converter.EscapedStringToDictionary(PlayerPrefs.GetString($"={Name}_{key}", ""));
                    return cd.Count > 0 ? cd : defaul;
                }
        }
        return null; //code never reaches here but it makes the compiler shut up
    }

    public A GetObject<A>(string key, A defaul = default)
    {
        return GetString(key, defaul.ToString()).StringToObject<A>();
    }
    public List<A> GetList<A>(string key, List<A> defaul = null)
    {
        return GetList(key, defaul.AListToStringList()).StringListToAList<A>();
    }
    public Dictionary<A, B> GetDict<A, B>(string key, Dictionary<A, B> defaul = null)
    {
        return GetDict(key, defaul.ABDictionaryToStringDictionary()).StringDictionaryToABDictionary<A, B>();
    }



    private OXFile OXFile = null;
    public OXFile GetOX()
    {
        if (OXFile != null)
        {
            return OXFile;
        }
        OXFile = new OXFile();
        return OXFile;
    }

}