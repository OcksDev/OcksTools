using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedAverageHandler
{
    public static T DrawFromWeights<T>(WeightedAverage nerds, int? seed = null)
    {
        float get = 0;
        if(seed != null)
        {
            get = (float)(new System.Random(seed.Value).NextDouble());
        }
        else
        {
            get = UnityEngine.Random.Range(0f, 1f);
        }
        var deez = nerds.lists;

        float sum = 0;
        float tot = 0;
        foreach (var dd in deez) sum += (float)dd[0];
        for (int i = 0; i < deez.Count;i++)
        {
            var x = ((float)deez[i][0]) / sum;
            deez[i][0] = x;
            tot += x;
            if (tot >= get)
            {
                return (T)deez[i][1];
            }
        }
        return (T)deez[deez.Count-1][1];
    }
}
public class WeightedAverage
{
    public List<List<object>> lists = new List<List<object>>();
    public WeightedAverage Add(float weight, object n)
    {
        lists.Add(new List<object>() { weight, n });
        return this;
    } 
}