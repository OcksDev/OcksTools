using System;
using UnityEngine;



[CreateAssetMenu(fileName = "SettingKeybindSO", menuName = "OcksTools/EasySettings/Keybind")]
public class SettingKeybindSO : SettingSO<KeyCode>
{
    [HideInInspector]
    public bool CurrentlySelecting = false;
    public override void LoadFromString(string s) => Data.Value = Enum.Parse<KeyCode>(s);

    public override string SaveToString() => Data.Value.ToString();
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
