using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShitBall : MonoBehaviour
{
    public OcksNetworkVar Ocky;

    public void Start()
    {
        Ocky = new OcksNetworkVar(gameObject, "Banana");
        Debug.Log(Ocky.Value);
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("AAAA" + Ocky.sex.name);
    }
}
