using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXBezier
{
    private List<Vector3> initposes;
    public List<Vector3> OutputPositions;
    public int Segments = 0;
    public OXBezier(List<Vector3> li, int segemtn)
    {
        initposes = li;
        Segments = segemtn;
        OutputPositions = new List<Vector3>();
    }

    public static Vector3? GetPosition(List<Vector3> points, float perc)
    {
        if (points.Count <= 1)
        {
            return null;
        }
        return RecursiveGet(points, perc)[0];
    }
    private static List<Vector3> RecursiveGet(List<Vector3> points, float perc)
    {
        List<Vector3> banan = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            banan.Add(Vector3.Lerp(points[i], points[i + 1], perc));
        }
        if (banan.Count == 1)
        {
            return banan;
        }
        else
        {
            return RecursiveGet(banan, perc);
        }
    }

    public List<Vector3> CalculateCurve()
    {
        OutputPositions.Clear();
        if (initposes.Count <= 1)
        {
            return null;
        }

        OutputPositions.Add(initposes[0]);

        float spacing = 1f / Segments;
        for (int i = 1; i < Segments; i++)
        {
            OutputPositions.Add(GetPosition(initposes, spacing * i).Value);
        }

        OutputPositions.Add(initposes[initposes.Count - 1]);

        return OutputPositions;
    }
}
