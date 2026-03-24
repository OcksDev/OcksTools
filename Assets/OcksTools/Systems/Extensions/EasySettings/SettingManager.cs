using System.Collections.Generic;

public class SettingManager : SingleInstance<SettingManager>
{
    public Dictionary<string, ResettableValue<string>> Data = new();

    private void Awake()
    {
        SaveSystem.SaveAllData.Append(SaveSettings);
        SaveSystem.SaveAllData.Append(LoadSettings);
    }
    public void SaveSettings(SaveProfile dict)
    {

    }
    public void LoadSettings(SaveProfile dict)
    {

    }
}

