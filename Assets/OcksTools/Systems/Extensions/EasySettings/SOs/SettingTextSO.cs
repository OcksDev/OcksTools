using UnityEngine;



[CreateAssetMenu(fileName = "SettingTextSO", menuName = "OcksTools/EasySettings/Text")]
public class SettingTextSO : SettingSO<string>
{

    public override void LoadFromString(string s) => Data.Value = s;

    public override string SaveToString() => Data.Value;
    public override SType Type => SType.Text;
}
