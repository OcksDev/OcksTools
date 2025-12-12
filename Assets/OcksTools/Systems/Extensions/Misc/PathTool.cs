using System.Collections.Generic;
using UnityEngine;

public class PathTool : MonoBehaviour
{
    public bool AutoEndConnect = true;
    public List<PT_Point> Positions = new List<PT_Point>();
    public bool DrawGizmo = true;
    private void Awake()
    {
        DrawGizmo = false;
        CalcFullPath();
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!DrawGizmo) return;
        CalcFullPath();
        for (int i = 0; i < Pather.Positions.Count - 1; i++)
        {
            Gizmos.color = new Color32(125, 255, 255, 125);
            Gizmos.DrawLine(Pather.Positions[i], Pather.Positions[i + 1]);
        }
    }
#endif
    public void CalcFullPath()
    {
        Pather = new OXPath();
        Pather.AutoEndConnect = false;
        Pather.Positions.Clear();
        var pos_working = new List<PT_Point>(Positions);
        if (AutoEndConnect)
        {
            pos_working.Add(Positions[0]);
        }
        for (int i = 0; i < pos_working.Count; i++)
        {
            var a = pos_working[i];
            if (a.PointType == PT_Point.PT_PointType.Bezier)
            {
                List<Vector3> points = new List<Vector3>();
                points.Add(pos_working[i - 1].Location.position);
                int bz = pos_working[i].BezierSegments;
                for (int j = i; j < pos_working.Count; j++)
                {
                    if (pos_working[j].PointType == PT_Point.PT_PointType.Bezier)
                    {
                        points.Add(pos_working[j].Location.position);
                        i++;
                    }
                    else
                    {
                        points.Add(pos_working[j].Location.position);
                        break;
                    }
                }
                points = new OXBezier(points, bz).CalculateCurve();
                points.RemoveAt(0);
                foreach (var b in points)
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
    private List<float> floats = new List<float>();
    private float total_dist = 0;

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
        distance = OXFunctions.Mod(distance, total_dist);
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
