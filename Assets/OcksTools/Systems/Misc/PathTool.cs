using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTool : MonoBehaviour
{
    public bool AutoEndConnect = true;
    public List<Transform> Positions = new List<Transform>();
    private void Awake()
    {
        Pather = new OXPath();
        Pather.Positions.Clear();
        foreach(var a in Positions)
        {
            Pather.Positions.Add(a.position);
        }
        Pather.CalculateStats();
    }

    public OXPath Pather;
}


public class OXPath
{
    public bool AutoEndConnect = true;
    public List<Vector3> Positions = new List<Vector3>();
    private List<Vector3> Positions_re = new List<Vector3>();
    List<float> floats = new List<float>();
    float total_dist = 0;

    private void Awake()
    {
        CalculateStats();
    }
    public void CalculateStats()
    {
        floats.Clear();
        total_dist = 0;
        Positions_re = new List<Vector3>(Positions);
        if (AutoEndConnect) Positions_re.Add(Positions[0]);
        for (int i = 0; i < Positions_re.Count - 1; i++)
        {
            floats.Add(total_dist);
            total_dist += RandomFunctions.Dist(Positions_re[i], Positions_re[i + 1]);
        }
        floats.Add(total_dist);
    }

    public Vector3 GetPos_Distance(float distance)
    {
        distance = RandomFunctions.Mod(distance, total_dist);
        int z = GetIndex(distance);
        distance -= floats[z];
        float perc = distance / (floats[z + 1] - floats[z]);
        return Vector3.Lerp(Positions_re[z], Positions_re[z + 1], perc);
    }

    public Vector3 GetPos_Percent(float perc)
    {
        return GetPos_Distance(perc * total_dist);
    }

    private int GetIndex(float distance)
    {
        for (int i = 1; i < floats.Count; i++)
        {
            if (floats[i] > distance) return i - 1;
        }
        return -1; //this should never run but compiler angry
    }
}
