using UnityEngine;

[CreateAssetMenu(fileName = "SettingToggleSO", menuName = "OcksTools/EasySettings/Toggle")]
public class SettingToggleSO : SettingSO<bool>
{
    public override void LoadFromString(string s) => Value = bool.Parse(s);

    public override string SaveToString() => Value.ToString();
}