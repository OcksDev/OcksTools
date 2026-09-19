using UnityEngine;



[CreateAssetMenu(fileName = "SettingSliderSO", menuName = "OcksTools/EasySettings/Slider")]
public class SettingSliderSO : SettingSO<float>
{

    public override void LoadFromString(string s) => Data.Value = float.Parse(s);

    public override string SaveToString() => Data.Value.ToString();
    public override SType Type => SType.Slider;
}
