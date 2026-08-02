using System;
using System.Collections;
using UnityEngine;

public class AbstractContainerTester : MonoBehaviour
{
    private GISContainerSimple pp;
    private void Start()
    {
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        yield return new WaitUntil(() => { return SaveSystem.Instance.LoadedData; });
        yield return new WaitForFixedUpdate();
        pp = GetComponent<GISContainerSimple>();
        var g = GISLol.Instance;
        if (pp.items.Count < 1)
        {
            var x = new GISItem(g.ItemDefs.Items[1].Name);
            x.Amount.SetValue(69);
            pp.AbstractAdd(x);
            x = new GISItem(g.ItemDefs.Items[4].Name);
            x.Amount.SetValue(690);
            pp.AbstractAdd(x);
            x = new GISItem(g.ItemDefs.Items[3].Name);
            x.Amount.SetValue(169);
            pp.AbstractAdd(x);
        }
        string e = "";
        foreach (var s in pp.items)
        {
            e += GISLol.Instance.ItemDict[s.Name].Name + ": " + s.Amount + Environment.NewLine;
        }
    }

}