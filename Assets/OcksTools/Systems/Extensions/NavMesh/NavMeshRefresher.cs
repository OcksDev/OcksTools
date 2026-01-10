using NavMeshPlus.Components;
using UnityEngine;

public class NavMeshRefresher : MonoBehaviour
{
    private int ij = 0;
    private NavMeshSurface navMeshSurface2D;
    public bool ActiveRefresh = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        navMeshSurface2D = GetComponent<NavMeshSurface>();
        BuildNavMesh();
    }

    // Update is called once per frame
    private void FixedUpdate()
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
        if (useasync)
        {
            navMeshSurface2D.BuildNavMeshAsync();
        }
        else
        {
            navMeshSurface2D.BuildNavMesh();
        }
    }

}
