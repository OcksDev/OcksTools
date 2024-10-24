using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompTeste : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OXComponent.StoreComponentsInChildrenRecursive<SpriteRenderer>(gameObject);
        StartCoroutine(wankis());
    }
    public IEnumerator wankis()
    {
        yield return new WaitForSeconds(0.2f);
        var a = OXComponent.GetComponentsInChildren<SpriteRenderer>(gameObject);
        Console.Log("Length a: " + a.Count);
        var b = OXComponent.GetComponentsInChildrenRecursive<SpriteRenderer>(gameObject);
        Console.Log("Length b: " + b.Count);
    }
}
