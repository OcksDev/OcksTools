using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    public List<string> Items = new List<string>();
    public int index = 0;
    public SettingInput SI;
    public void IndexUp()
    {
        index = RandomFunctions.Mod(index + 1, Items.Count);
        SI.WriteValue();
    }
    public void IndexDown()
    {
        index = RandomFunctions.Mod(index - 1, Items.Count);
        SI.WriteValue();
    }
}
