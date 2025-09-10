using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGamin : MonoBehaviour
{
    [Button]
    public void TestButton()
    {
        Debug.Log("RAHHH");
        Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new GISItem()));
    }

    [HorizontalLine]
    public int zzzz = 1;
}

public class bamam
{
    public int x;
    public bamam(int x)
    {
        this.x = x;
    }
}