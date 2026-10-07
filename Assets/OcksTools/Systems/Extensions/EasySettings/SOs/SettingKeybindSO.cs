using System;
using UnityEngine;



[CreateAssetMenu(fileName = "SettingKeybindSO", menuName = "OcksTools/EasySettings/Keybind")]
public class SettingKeybindSO : SettingSO<KeyCode>
{
    [HideInInspector]
    public bool CurrentlySelecting = false;

    public override void LoadFromProfile(SaveProfile dict, string key) => Data.Value = Enum.Parse<KeyCode>(dict.GetString(key, Data.Value.ToString()));

    public override void SaveToProfile(SaveProfile dict, string key) => dict.SetString(key, Data.Value.ToString());
    public override void ResetToDefault()
    {
        base.ResetToDefault();
        CurrentlySelecting = false;
    }
    public override void DupeData()
    {
        CurrentlySelecting = false;
        base.DupeData();
    }
    public override SType Type => SType.Keybind;
}
