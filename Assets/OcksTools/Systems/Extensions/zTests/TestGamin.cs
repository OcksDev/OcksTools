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
        //Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new GISItem()));
    }
    [Button]
    public void RandomVal()
    {
        var d = new WeightedAverage();
        d.Add(75f, "75%");
        d.Add(25f, "25%");
        d.Add(1f, "1%");
        Debug.Log(WeightedAverageHandler.DrawFromWeights<string>(d));
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