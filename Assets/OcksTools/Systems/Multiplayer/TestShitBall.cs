using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TestShitBall : MonoBehaviour
{
    [HideInInspector]
    public OcksNetworkVar Ocky;
    public OcksNetworkVar Ocky2;
    public string DataBalling = "";
    public string DataBalling2 = "";
    public TextMeshProUGUI Display;
    public TextMeshProUGUI Display2;

    public void Start()
    {
        var a = GetComponent<NetworkObject>();
        DataBalling = "Banana";
        DataBalling2 = "";
        Ocky = new OcksNetworkVar(a, "Bobby", DataBalling);
        Ocky2 = new OcksNetworkVar(a, "TimmyA", DataBalling2);
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        yield return new WaitForSeconds(1f);
        if (!Ocky.NetOb.IsOwner)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                Display.text = Ocky.GetValue();
                Display2.text = Ocky2.GetValue();
            }
        }
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Ocky.SetValue(DataBalling);
            Ocky2.SetValue(DataBalling2);
            Display.text = Ocky.GetValue();
            Display2.text = Ocky2.GetValue();
        }
    }
}
