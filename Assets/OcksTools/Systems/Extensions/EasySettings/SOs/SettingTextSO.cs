using UnityEngine;



[CreateAssetMenu(fileName = "SettingTextSO", menuName = "OcksTools/EasySettings/Text")]
public class SettingTextSO : SettingSO<string>
{
    public override void LoadFromProfile(SaveProfile dict, string key) => Data.Value = dict.GetString(key, Data.Value);

    public override void SaveToProfile(SaveProfile dict, string key) => dict.SetString(key, Data.Value);
    public override SType Type => SType.Text;
}
