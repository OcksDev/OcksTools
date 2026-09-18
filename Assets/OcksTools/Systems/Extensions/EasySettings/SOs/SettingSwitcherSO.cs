using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "SettingSwitcherSO", menuName = "OcksTools/EasySettings/Switcher")]
public class SettingSwitcherSO : SettingSO<int>
{
    public List<string> Items = new List<string>();

    public override void LoadFromString(string s) => Data.Value = int.Parse(s);

    public override string SaveToString() => Data.Value.ToString();
}
