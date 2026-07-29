using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class Achievement : SingleInstance<Achievement>
{
    public List<AchievementData> Achievements = new List<AchievementData>();
    public List<AchievementData> AchievementsDontSave = new List<AchievementData>();
    private static Dictionary<string, AchievementData> AchievementDict = new Dictionary<string, AchievementData>();
    private static Dictionary<string, AchievementData> AchievementDontSaveDict = new Dictionary<string, AchievementData>();
    public static OXEventLayered<AchievementData> OnAchievementGet = new OXEventLayered<AchievementData>();

    public static AchievementData GetAchievementData(string name)
    {
        if (AchievementDict.ContainsKey(name))
        {
            return AchievementDict[name];
        }
        if (AchievementDontSaveDict.ContainsKey(name))
        {
            return AchievementDontSaveDict[name];
        }
        return null;
    }
    public static bool Get(string name)
    {
        var d = GetAchievementData(name);
        return d != null ? d.Progress.IsCompleted : false;
    }
    public static long GetProgressLong(string name)
    {
        var d = GetAchievementData(name);
        return d != null ? (d.Progress.long_prog.HasValue ? d.Progress.long_prog.Value : 0) : 0;
    }
    public static double GetProgressDouble(string name)
    {
        var d = GetAchievementData(name);
        return d != null ? (d.Progress.double_prog.HasValue ? d.Progress.double_prog.Value : 0) : 0;
    }
    public static void SetProgressLong(string name, long p)
    {
        var d = GetAchievementData(name);
        if (d != null)
        {
            d.Progress.long_prog = p;
        }
    }
    public static void SetProgressDouble(string name, double p)
    {
        var d = GetAchievementData(name);
        if (d != null)
        {
            d.Progress.double_prog = p;
        }
    }
    public static void Grant(string name, bool saved = true)
    {
        var d = GetAchievementData(name);
        if (d == null)
        {
            Define(name, saved);
            d = GetAchievementData(name);
        }
        d.Progress.IsCompleted = true;
        OnAchievementGet.Invoke(d);
    }
    public static void Revoke(string name)
    {
        if (AchievementDict.ContainsKey(name)) AchievementDict.Remove(name);
        if (AchievementDontSaveDict.ContainsKey(name)) AchievementDontSaveDict.Remove(name);
    }
    public static void Define(string name, bool saved = true)
    {
        if (AchievementDict.ContainsKey(name) || AchievementDontSaveDict.ContainsKey(name))
        {
            return;
        }
        var d = new AchievementData()
        {
            Name = name,
            Progress = new AchievementProgress(true)
        };
        if (saved)
        {
            Instance.Achievements.Add(d);
            AchievementDict.Add(name, d);
        }
        else
        {
            Instance.AchievementsDontSave.Add(d);
            AchievementDontSaveDict.Add(name, d);
        }
    }


    public override void Awake2()
    {
        SaveSystem.SaveAllData.Append(SaveAchievements);
        SaveSystem.LoadAllData.Append(-69, LoadAchievements);
        CompileDict();
    }
    public void SaveAchievements(SaveProfile a)
    {
        Dictionary<string, AchievementData> sex = new(AchievementDict);
        var q = sex.ToList();
        foreach (var item in q)
        {
            if (item.Value == null) sex.Remove(item.Key);
            if (!item.Value.Progress.HasAnything()) sex.Remove(item.Key);
        }
        a.SetString("Achievements", sex.DictionaryToString("$", "#"));
    }
    public void LoadAchievements(SaveProfile a)
    {
        var Achievements = a.GetString("Achievements").StringToDictionary("$", "#").ABToCD((x, y) => x, (x, y) =>
        {
            AchievementData ad = new AchievementData();
            ad.FromString(x, y);
            return ad;
        }
        );
        CompileDict();
        foreach (var b in Achievements)
        {
            AchievementDict.AddOrUpdate(b);
        }
    }
    private void CompileDict()
    {
        AchievementDict.Clear();
        foreach (var ach in Achievements)
        {
            AchievementDict.AddOrUpdate(ach.Name, ach);
        }
        foreach (var ach in AchievementsDontSave)
        {
            AchievementDontSaveDict.AddOrUpdate(ach.Name, ach);
        }
    }
}

[System.Serializable]
public class AchievementData
{
    public string Name;
    public string Description = "";
    public AchievementProgress Progress = new AchievementProgress(true);
    public override string ToString()
    {
        Dictionary<string, string> banans = new Dictionary<string, string>();
        if (!Progress.IsCompleted) banans.Add("C", "");
        if (Progress.long_prog.HasValue) banans.Add("L", Progress.long_prog.Value.ToString());
        if (Progress.double_prog.HasValue) banans.Add("D", Progress.double_prog.Value.ToString());
        return banans.DictionaryToString(";", "^");
    }
    public void FromString(string name, string data)
    {
        Dictionary<string, string> banans = data.StringToDictionary(";", "^");
        Name = name;
        Progress = new AchievementProgress(true)
        {
            IsCompleted = !banans.ContainsKey("C")
        };
        if (banans.ContainsKey("L"))
            Progress.long_prog = long.Parse(banans["L"]);
        if (banans.ContainsKey("D"))
            Progress.double_prog = double.Parse(banans["D"]);
    }
    public void ResetProgress()
    {
        Progress = new AchievementProgress(true)
        {
            IsCompleted = false,
            long_prog = null,
            double_prog = null
        };
    }
}
[System.Serializable]
public class AchievementProgress
{
    public bool IsCompleted = false;
    public long? long_prog;
    public double? double_prog;
    [UnityEngine.HideInInspector]
    public bool SaveLoadMe = true;
    public AchievementProgress(bool saveme)
    {
        IsCompleted = false;
        long_prog = null;
        double_prog = null;
        SaveLoadMe = saveme;
    }
    public bool HasAnything()
    {
        if (IsCompleted) return true;
        if (long_prog.HasValue && long_prog > 0) return true;
        if (double_prog.HasValue && double_prog > 0) return true;
        return false;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AchievementProgress))]
public class FuckassAchievementDrawer : AutoCompressedInspector
{
}
#endif