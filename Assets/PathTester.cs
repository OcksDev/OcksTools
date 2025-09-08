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
    }
    float z = 0f;
    private void Update()
    {
        z += Time.deltaTime * 0.35f;
        gm.transform.position = tool.GetPos_Percent(z);
    }

}
