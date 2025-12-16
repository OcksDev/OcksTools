using System.Collections.Generic;

public class Achievement : SingleInstance<Achievement>
{
    public static List<AchievementData> Achievements = new List<AchievementData>();
    public static List<AchievementData> AchievementsDontSave = new List<AchievementData>();
    private static Dictionary<string, AchievementData> AchievementDict = new Dictionary<string, AchievementData>();
    private static Dictionary<string, AchievementData> AchievementDontSaveDict = new Dictionary<string, AchievementData>();

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
    public static void Grant(string name, bool saved = true)
    {
        var d = GetAchievementData(name);
        if (d != null)
        {
            d.Progress.IsCompleted = true;
        }
        else
        {
            Define(name, saved);
            GetAchievementData(name).Progress.IsCompleted = true;
        }
    }
    public static void Define(string name, bool saved = true)
    {
        if (AchievementDict.ContainsKey(name))
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
            Achievements.Add(d);
            AchievementDict.Add(name, d);
        }
        else
        {
            AchievementsDontSave.Add(d);
            AchievementDontSaveDict.Add(name, d);
        }
    }


    public override void Awake2()
    {
        SaveSystem.SaveAllData.Append(SaveAchievements);
        SaveSystem.LoadAllData_EarlyCall.Append(LoadAchievements);
        CompileDict();
    }
    public void SaveAchievements(string a)
    {
        SaveSystem.Instance.SetString("Achievements", Achievements.ListToString(), a);
    }
    public void LoadAchievements(string a)
    {
        Achievements = SaveSystem.Instance.GetString("Achievements", a).StringToList().AListToBList(x =>
        {
            AchievementData ad = new AchievementData();
            ad.FromString(x);
            return ad;
        }
        );
        CompileDict();
    }
    private void CompileDict()
    {
        AchievementDict.Clear();
        foreach (var ach in Achievements)
        {
            AchievementDict[ach.Name] = ach;
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
        Dictionary<string, string> banans = new Dictionary<string, string>
        {
            { "N", Name },
            { "C", Progress.IsCompleted.ToString() }
        };
        if (Progress.long_prog.HasValue) banans.Add("LP", Progress.long_prog.Value.ToString());
        if (Progress.double_prog.HasValue) banans.Add("DP", Progress.double_prog.Value.ToString());
        return banans.DictionaryToString("==", "++");
    }
    public void FromString(string data)
    {
        Dictionary<string, string> banans = data.StringToDictionary("==", "++");
        Name = banans["N"];
        Progress = new AchievementProgress(true)
        {
            IsCompleted = bool.Parse(banans["C"])
        };
        if (banans.ContainsKey("LP"))
            Progress.long_prog = long.Parse(banans["LP"]);
        if (banans.ContainsKey("DP"))
            Progress.double_prog = double.Parse(banans["DP"]);
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
    public bool SaveLoadMe = true;
    public AchievementProgress(bool saveme)
    {
        IsCompleted = false;
        long_prog = null;
        double_prog = null;
        SaveLoadMe = saveme;
    }
}