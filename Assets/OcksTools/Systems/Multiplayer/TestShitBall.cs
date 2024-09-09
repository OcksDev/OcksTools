using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TestShitBall : MonoBehaviour
{
    [HideInInspector]
    public OcksNetworkVar Ocky;
    public string DataBalling = "";
    public string ReadVal = "";
    public TextMeshProUGUI Display;

    public void Start()
    {
        var a = GetComponent<NetworkObject>();
        DataBalling = "Banana";
        Ocky = new OcksNetworkVar(a, DataBalling);
        StartCoroutine(sex());
    }
    public IEnumerator sex()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Ocky.SetValue(DataBalling);
            ReadVal = Ocky.GetValue();
            Display.text = ReadVal;
        }
    }
}
