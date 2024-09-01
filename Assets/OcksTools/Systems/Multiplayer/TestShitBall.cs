using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestShitBall : MonoBehaviour
{
    public OcksNetworkVar Ocky;

    public void Start()
    {
        var a = GetComponent<NetworkObject>();
        Ocky = new OcksNetworkVar(a, "Banana");
        Debug.Log(Ocky.Value);
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        yield return new WaitForSeconds(0.5f);
        //Debug.Log("AAAA" + Ocky.sex.name);
    }
}
