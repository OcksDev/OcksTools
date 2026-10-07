using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SaveSystem;

public class SaveSystem : SingleInstance<SaveSystem>
{
    public SaveMethod SaveMethod_ = SaveMethod.TXTFile;
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

        GetDataFromFile(GlobalProfile());
        GetDataFromFile(prof);

        LoadAllData.Invoke(prof);
    }
    public void SaveGame(string dict = "Profile1")
    {
        var prof = Profile(dict);

        SaveAllData.Invoke(prof);

        SaveDataToFile(GlobalProfile());
        SaveDataToFile(prof);
    }


    public void SaveDataToFile(SaveProfile prof)
    {
        var f = FileSystem.Instance;
        f.AssembleFilePaths();
        switch (prof.SaveMethod)
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
        switch (prof.SaveMethod)
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
        switch (prof.SaveMethod)
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
        SetList(key, data.AToString());
    }

    public void SetDict<A, B>(string key, Dictionary<A, B> data)
    {
        SetDict(key, data.ABToString());
    }
    public void SetDict(string key, Dictionary<string, int> data)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            GetOX().Data.Add(key, data);
        }
        else
        {
            // TXT / PlayerPrefs only store text, so go through the string version
            SetDict(key, data.ABToString());
        }
    }
    public void SetDict(string key, Dictionary<string, long> data)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            GetOX().Data.Add(key, data);
        }
        else
        {
            // TXT / PlayerPrefs only store text, so go through the string version
            SetDict(key, data.ABToString());
        }
    }
    public void SetDict(string key, Dictionary<string, double> data)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            GetOX().Data.Add(key, data);
        }
        else
        {
            // TXT / PlayerPrefs only store text, so go through the string version
            SetDict(key, data.ABToString());
        }
    }
    public void SetDict(string key, Dictionary<string, float> data)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            GetOX().Data.Add(key, data);
        }
        else
        {
            // TXT / PlayerPrefs only store text, so go through the string version
            SetDict(key, data.ABToString());
        }
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
        return GetList(key, defaul.AToString()).StringToA<A>();
    }
    public Dictionary<A, B> GetDict<A, B>(string key, Dictionary<A, B> defaul = null)
    {
        return GetDict(key, defaul.ABToString()).StringToAB<A, B>();
    }
    // defaul is intentionally required here (no "= null"), so existing GetDict(key) calls don't become ambiguous
    public Dictionary<string, int> GetDict(string key, Dictionary<string, int> defaul)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            var ox = GetOX();
            if (!ox.Data.ContainsKey(key)) return defaul;
            var node = ox.Data[key];
            if (node.Type == OXFileData.OXFileType.DictStringInt)
            {
                var x = node.DataDictStringInt;
                return x != null && x.Count > 0 ? x : defaul;
            }
            // otherwise it was saved earlier as a DictStringString (old generic SetDict), so fall through and convert
        }
        var s = GetDict(key, (Dictionary<string, string>)null);
        return s == null ? defaul : s.StringToAB<string, int>();
    }
    // defaul is intentionally required here (no "= null"), so existing GetDict(key) calls don't become ambiguous
    public Dictionary<string, long> GetDict(string key, Dictionary<string, long> defaul)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            var ox = GetOX();
            if (!ox.Data.ContainsKey(key)) return defaul;
            var node = ox.Data[key];
            if (node.Type == OXFileData.OXFileType.DictStringLong)
            {
                var x = node.DataDictStringLong;
                return x != null && x.Count > 0 ? x : defaul;
            }
            // otherwise it was saved earlier as a DictStringString (old generic SetDict), so fall through and convert
        }
        var s = GetDict(key, (Dictionary<string, string>)null);
        return s == null ? defaul : s.StringToAB<string, long>();
    }
    // defaul is intentionally required here (no "= null"), so existing GetDict(key) calls don't become ambiguous
    public Dictionary<string, double> GetDict(string key, Dictionary<string, double> defaul)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            var ox = GetOX();
            if (!ox.Data.ContainsKey(key)) return defaul;
            var node = ox.Data[key];
            if (node.Type == OXFileData.OXFileType.DictStringDouble)
            {
                var x = node.DataDictStringDouble;
                return x != null && x.Count > 0 ? x : defaul;
            }
            // otherwise it was saved earlier as a DictStringString (old generic SetDict), so fall through and convert
        }
        var s = GetDict(key, (Dictionary<string, string>)null);
        return s == null ? defaul : s.StringToAB<string, double>();
    }
    // defaul is intentionally required here (no "= null"), so existing GetDict(key) calls don't become ambiguous
    public Dictionary<string, float> GetDict(string key, Dictionary<string, float> defaul)
    {
        if (SaveMethod == SaveMethod.OXFile)
        {
            var ox = GetOX();
            if (!ox.Data.ContainsKey(key)) return defaul;
            var node = ox.Data[key];
            if (node.Type == OXFileData.OXFileType.DictStringFloat)
            {
                var x = node.DataDictStringFloat;
                return x != null && x.Count > 0 ? x : defaul;
            }
            // otherwise it was saved earlier as a DictStringString (old generic SetDict), so fall through and convert
        }
        var s = GetDict(key, (Dictionary<string, string>)null);
        return s == null ? defaul : s.StringToAB<string, float>();
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


    public void SetInt(string key, int data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }
    public int GetInt(string key, int def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataInt : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? int.Parse(d2) : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return int.Parse(a);
        }
        return default; // this line never runs lol
    }
    public void SetLong(string key, long data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }

    public long GetLong(string key, long def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataLong : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? long.Parse(d2) : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return long.Parse(a);
        }
        return default; // this line never runs lol
    }
    public void SetBool(string key, bool data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }

    public bool GetBool(string key, bool def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataBool : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? bool.Parse(d2) : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return bool.Parse(a);
        }
        return default; // this line never runs lol
    }

    public void SetFloat(string key, float data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }
    public float GetFloat(string key, float def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataFloat : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? float.Parse(d2) : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return float.Parse(a);
        }
        return default; // this line never runs lol
    }
    public void SetDouble(string key, double data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }
    public double GetDouble(string key, double def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataDouble : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? double.Parse(d2) : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return double.Parse(a);
        }
        return default; // this line never runs lol
    }
    public void SetVector2(string key, Vector2 data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }
    public Vector2 GetVector2(string key, Vector2 def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataVector2 : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToVector2() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToVector2();
        }
        return default; // this line never runs lol
    }
    public void SetVector3(string key, Vector3 data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }

    public Vector3 GetVector3(string key, Vector3 def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataVector3 : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToVector3() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToVector3();
        }
        return default; // this line never runs lol
    }
    public void SetVector2Int(string key, Vector2Int data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }
    }

    public Vector2Int GetVector2Int(string key, Vector2Int def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataVector2Int : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToVector2Int() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToVector2Int();
        }
        return default; // this line never runs lol
    }
    public void SetVector3Int(string key, Vector3Int data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }

    }
    public Vector3Int GetVector3Int(string key, Vector3Int def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataVector3Int : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToVector3Int() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToVector3Int();
        }
        return default; // this line never runs lol
    }
    public void SetQuaternion(string key, Quaternion data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }

    }
    public Quaternion GetQuaternion(string key, Quaternion def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataQuaternion : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToQuaternion() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToQuaternion();
        }
        return default; // this line never runs lol
    }
    public void SetColor(string key, Color data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                GetOX().Data.Add(key, data);
                break;
            case SaveMethod.TXTFile:
                SavedData.AddOrUpdate(key, data.ToString());
                break;
            case SaveMethod.PlayerPrefs:
                if (IsGlobal)
                {
                    PlayerPrefs.SetString($"_Global_{key}", data.ToString());
                }
                else
                {
                    PlayerPrefs.SetString($"={Name}_{key}", data.ToString());
                }
                break;
        }

    }
    public Color GetColor(string key, Color def_data)
    {
        switch (SaveMethod)
        {
            case SaveMethod.OXFile:
                return GetOX().Data.TryGetValue(key, out OXFileData d) ? d.DataColor : def_data;
            case SaveMethod.TXTFile:
                return SavedData.TryGetValue(key, out string d2) ? d2.StringToColor() : def_data;
            case SaveMethod.PlayerPrefs:
                string a = "";
                if (IsGlobal)
                {
                    a = PlayerPrefs.GetString($"_Global_{key}", "!!!");
                }
                else
                {
                    a = PlayerPrefs.GetString($"={Name}_{key}", "!!!");
                }
                if (a == "!!!") return def_data;
                return a.StringToColor();
        }
        return default; // this line never runs lol
    }
}