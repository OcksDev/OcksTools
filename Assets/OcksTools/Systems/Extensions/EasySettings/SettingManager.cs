using UnityEngine;

public class SettingManager : SingleInstance<SettingManager>
{
    public CompileableDictionaryAlt<string, SettingData> StoredData;

    public override void Awake2()
    {
        StoredData.Compile((x) =>
        {
            x.SaveCurrentToDefault();
            return x.Name;
        });
        //save/load stuff
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
}
