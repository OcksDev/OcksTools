using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "SettingSwitcherSO", menuName = "OcksTools/EasySettings/Switcher")]
public class SettingSwitcherSO : SettingSO<int>
{
    public List<string> Items = new List<string>();

    public override void LoadFromProfile(SaveProfile dict, string key) => Data.Value = dict.GetInt(key, Data.Value);

    public override void SaveToProfile(SaveProfile dict, string key) => dict.SetInt(key, Data.Value);
    public override SType Type => SType.Switcher;
}
