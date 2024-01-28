using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshRefresher : MonoBehaviour
{
    private int ij = 0;
    // Start is called before the first frame update
    void OnEnable()
    {
        BuildNavMesh();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ij++;
        if (ij >= 100)
        {
            BuildNavMesh();
            ij = 0;
        }
    }
    public void BuildNavMesh()
    {
        var surfaces = GameObject.Find("NavMesh").GetComponent<NavMeshSurface2d>();
        surfaces.BuildNavMesh();
    }

}
