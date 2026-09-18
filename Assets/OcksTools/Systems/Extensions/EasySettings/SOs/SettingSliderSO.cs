using UnityEngine;



[CreateAssetMenu(fileName = "SettingSliderSO", menuName = "OcksTools/EasySettings/Slider")]
public class SettingSliderSO : SettingSO<float>
{

    public override void LoadFromString(string s) => Value = float.Parse(s);

    public override string SaveToString() => Value.ToString();
}
