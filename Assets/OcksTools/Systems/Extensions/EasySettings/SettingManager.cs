using System.Collections;
using UnityEngine;

public class SettingManager : SingleInstance<SettingManager>
{
    public CompileableDictionaryAlt<string, SettingData> StoredData;
    public static bool HasCalledForLateData = false;
    public override void Awake2()
    {
        StoredData.Compile((x) =>
        {
            x.DupeData();
            x.SaveCurrentToDefault();
            return x.Name;
        });
        SaveSystem.SaveAllData.Append(SaveAll);
        SaveSystem.LoadAllData.Append(LoadAll);
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SaveSystem.Instance.LoadedData);
        foreach (var a in StoredData)
        {
            a.Value.LateDataFind();
        }
        HasCalledForLateData = true;
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
        dict = SaveSystem.GlobalProfile();
        foreach (var kvp in StoredData)
        {
            if (kvp.Value.GetShouldSkip()) continue;
            kvp.Value.SaveToProfile(dict, kvp.Key);
        }
    }
    public void LoadAll(SaveProfile dict)
    {
        dict = SaveSystem.GlobalProfile();
        foreach (var kvp in StoredData)
        {
            if (kvp.Value.GetShouldSkip()) continue;
            // Each setting falls back to its current value if the key is missing.
            kvp.Value.LoadFromProfile(dict, kvp.Key);
            kvp.Value.ApplyModiferValue();
        }
    }
}
