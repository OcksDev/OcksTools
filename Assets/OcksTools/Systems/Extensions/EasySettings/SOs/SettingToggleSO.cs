using UnityEngine;

[CreateAssetMenu(fileName = "SettingToggleSO", menuName = "OcksTools/EasySettings/Toggle")]
public class SettingToggleSO : SettingSO<bool>
{
    public override void LoadFromProfile(SaveProfile dict, string key) => Data.Value = dict.GetBool(key, Data.Value);

    public override void SaveToProfile(SaveProfile dict, string key) => dict.SetBool(key, Data.Value);
    public override SType Type => SType.Toggle;
}
