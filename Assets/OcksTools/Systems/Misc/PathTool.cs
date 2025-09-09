using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTool : MonoBehaviour
{
    public bool AutoEndConnect = true;
    public List<PT_Point> Positions = new List<PT_Point>();
    private void Awake()
    {
        Pather = new OXPath();
        Pather.AutoEndConnect = false;
        Pather.Positions.Clear();
        if (AutoEndConnect)
        {
            Positions.Add(Positions[0]);
        }
        for(int i = 0; i < Positions.Count; i++)
        {
            var a = Positions[i];
            if (a.PointType == PT_Point.PT_PointType.Bezier)
            {
                List<Vector3> points = new List<Vector3>();
                points.Add(Positions[i - 1].Location.position);
                int bz = Positions[i].BezierSegments;
                for (int j = i; j < Positions.Count; j++)
                {
                    if (Positions[j].PointType == PT_Point.PT_PointType.Bezier)
                    {
                        points.Add(Positions[j].Location.position);
                        i++;
                    }
                    else
                    {
                        points.Add(Positions[j].Location.position);
                        break;
                    }
                }
                points = new OXBezier(points,bz).CalculateCurve();
                points.RemoveAt(0);
                foreach(var b in points)
                {
                    Pather.Positions.Add(b);
                }
            }
            else
            {
                Pather.Positions.Add(a.Location.position);
            }
        }

        Pather.CalculateStats();
    }

    public OXPath Pather;
}

[System.Serializable]
public class PT_Point
{
    public Transform Location;
    public PT_PointType PointType = PT_PointType.Point;
    public int BezierSegments = 10;

    public enum PT_PointType
    {
        Point,
        Bezier,
    }
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
