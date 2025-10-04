using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshRefresher : MonoBehaviour
{
   /* private int ij = 0;
    private NavMeshSurface2d navMeshSurface2D;
    public bool ActiveRefresh = false;
    // Start is called before the first frame update
    void OnEnable()
    {
        navMeshSurface2D = GetComponent<NavMeshSurface2d>();
        BuildNavMesh();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ActiveRefresh)
        {
            ij++;
            if (ij >= 100)
            {
                BuildNavMesh();
                ij = 0;
            }
        }
    }
    public void BuildNavMesh(bool useasync = true)
    {
        if(useasync)
        {
            navMeshSurface2D.BuildNavMeshAsync();
        }
        else
        {
            navMeshSurface2D.BuildNavMesh();
        }
    }
   */
}
