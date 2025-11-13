using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitLol());
    }
    public IEnumerator WaitLol()
    {
        yield return new WaitForFixedUpdate();

        StoredRandom.Instance.DefineRandom("Gaming", 69);
        var cd = StoredRandom.Instance.ExportToString();
        Debug.Log($"D: {StoredRandom.Instance.GetRand("Gaming").Next()}");

        StoredRandom.Instance.ImportFromString(cd);
        Debug.Log($"E: {StoredRandom.Instance.GetRand("Gaming").Next()}");

        StoredRandom.Instance.UnallocateRandom("Gaming");

        if(!StoredRandom.Instance.randoms.ContainsKey("Shungite")) StoredRandom.Instance.DefineRandom("Shungite", 420);
        Debug.Log($"Shungite: {StoredRandom.Instance.GetRand("Shungite").Next()}");
    }
}
