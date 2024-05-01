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
        if(pp.slots.Count < 1)
        {
            var x = new GISItem(1);
            x.Amount = 69;
            pp.AbstractAdd(x);
            x = new GISItem(4);
            x.Amount = 690;
            pp.AbstractAdd(x);
            x = new GISItem(3);
            x.Amount = 169;
            pp.AbstractAdd(x);
        }
        string e = "";
        foreach (var s in pp.slots)
        {
            e += GISLol.Instance.Items[s.Held_Item.ItemIndex].Name + ": " + s.Held_Item.Amount + Environment.NewLine;
        }
        Debug.Log(e);
    }

}