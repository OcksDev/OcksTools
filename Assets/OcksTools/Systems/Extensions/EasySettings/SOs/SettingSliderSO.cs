using UnityEngine;



[CreateAssetMenu(fileName = "SettingSliderSO", menuName = "OcksTools/EasySettings/Slider")]
public class SettingSliderSO : SettingSO<float>
{
    public override void LoadFromProfile(SaveProfile dict, string key) => Data.Value = dict.GetFloat(key, Data.Value);

    public override void SaveToProfile(SaveProfile dict, string key) => dict.SetFloat(key, Data.Value);
    public override SType Type => SType.Slider;
}
