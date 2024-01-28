using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEntity : MonoBehaviour
{
    private NavMeshAgent beans;
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        beans = GetComponent<NavMeshAgent>();

        beans.updateRotation= false;
        beans.updateUpAxis= false;
    }

    // Update is called once per frame
    void Update()
    {
        beans.SetDestination(target.transform.position);
    }
}
