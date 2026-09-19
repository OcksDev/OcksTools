using UnityEngine;

[CreateAssetMenu(fileName = "SettingToggleSO", menuName = "OcksTools/EasySettings/Toggle")]
public class SettingToggleSO : SettingSO<bool>
{
    public override void LoadFromString(string s) => Data.Value = bool.Parse(s);

    public override string SaveToString() => Data.Value.ToString();
    public override SType Type => SType.Toggle;
}