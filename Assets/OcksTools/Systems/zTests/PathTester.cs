using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTester : MonoBehaviour
{
    public PathTool tool;
    public GameObject gm;
    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> abana = new List<Vector3>();
        abana.Add(Vector3.zero);
        abana.Add(Vector3.up);
        abana.Add(Vector3.zero);
        var a = new OXBezier(abana, 10).CalculateCurve();
        foreach(var b in a)
        {
            Debug.Log(b);
        }

    }
    float z = 0f;
    private void Update()
    {
        z += Time.deltaTime * 0.35f;
        gm.transform.position = tool.Pather.GetPos_Percent(z);
    }

}
