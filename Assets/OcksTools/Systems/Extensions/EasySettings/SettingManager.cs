using UnityEngine;

public class SettingManager : SingleInstance<SettingManager>
{
    public CompileableDictionaryAlt<string, SettingData> StoredData;

    public override void Awake2()
    {
        StoredData.Compile((x) =>
        {
            x.SaveCurrentToDefault();
            x.DupeData();
            return x.Name;
        });
        SaveSystem.SaveAllData.Append(SaveAll);
        SaveSystem.LoadAllData.Append(LoadAll);
    }

    public static T GetValue<T>(string name)
    {
        if (Instance.StoredData.TryGetValue(name, out var d)) return d.GetValue<T>();
        else
        {
            Debug.LogWarning($"Trying to read setting '{name}' but it does not exist.");
            return default;
        }
    }


    public void SaveAll(SaveProfile dict)
    {
        foreach (var kvp in StoredData)
        {
            dict.SetString(kvp.Key, kvp.Value.SaveToString());
        }
    }
    public void LoadAll(SaveProfile dict)
    {
        foreach (var kvp in StoredData)
        {
            string s = dict.GetString(kvp.Key, "-=-");
            if (s != "-=-")
            {
                kvp.Value.LoadFromString(s);
            }
        }
    }
}
