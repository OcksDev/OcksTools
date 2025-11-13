using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractContainerTester : MonoBehaviour
{
    private GISContainer pp;
    void Start()
    {
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        yield return new WaitUntil(() => { return SaveSystem.Instance.LoadedData; });
        yield return new WaitForFixedUpdate();
        pp = GetComponent<GISContainer>();
        var g = GISLol.Instance;
        if(pp.slots.Count < 1)
        {
            var x = new GISItem(g.Items[1].Name);
            x.Amount = 69;
            pp.AbstractAdd(x);
            x = new GISItem(g.Items[4].Name);
            x.Amount = 690;
            pp.AbstractAdd(x);
            x = new GISItem(g.Items[3].Name);
            x.Amount = 169;
            pp.AbstractAdd(x);
        }
        string e = "";
        foreach (var s in pp.slots)
        {
            e += GISLol.Instance.ItemDict[s.Held_Item.Name].Name + ": " + s.Held_Item.Amount + Environment.NewLine;
        }
    }

}