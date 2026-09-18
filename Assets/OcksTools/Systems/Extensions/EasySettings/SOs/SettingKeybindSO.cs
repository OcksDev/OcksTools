using System;
using UnityEngine;



[CreateAssetMenu(fileName = "SettingKeybindSO", menuName = "OcksTools/EasySettings/Keybind")]
public class SettingKeybindSO : SettingSO<KeyCode>
{
    public override void LoadFromString(string s) => Value = Enum.Parse<KeyCode>(s);

    public override string SaveToString() => Value.ToString();
}
