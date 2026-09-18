using UnityEngine;



[CreateAssetMenu(fileName = "SettingTextSO", menuName = "OcksTools/EasySettings/Text")]
public class SettingTextSO : SettingSO<string>
{

    public override void LoadFromString(string s) => Value = s;

    public override string SaveToString() => Value;
}
