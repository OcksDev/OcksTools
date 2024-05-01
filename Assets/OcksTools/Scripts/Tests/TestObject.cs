using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour
{
    float s = 0;
    public bool DoSpawn = false;
    private void OnEnable()
    {
        Debug.Log("AH");
        s = 0;
    }
    private void Update()
    {
        s += Time.deltaTime;
        if(s >= 1)
        {
            if (DoSpawn)
            {
                Destroy(gameObject);
            }
            else
            {
                SpawnSystem.Instance.ReturnObject(gameObject, 0);
            }
        }
    }
}
