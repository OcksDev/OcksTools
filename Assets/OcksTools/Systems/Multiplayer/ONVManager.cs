using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ONVManager : MonoBehaviour
{

    public static Dictionary<string, Dictionary<string, string>> OcksVars = new Dictionary<string, Dictionary<string, string>>();

    public static List<OcksNetworkVar> UndefinedVars = new List<OcksNetworkVar>();
    public static ONVManager Instance;
    public void Awake()
    {
        Instance = this;
        foreach(var a in UndefinedVars)
        {
            StartCoroutine(Gaming(a));
        }
        UndefinedVars.Clear();
    }
    public IEnumerator Gaming(OcksNetworkVar ONV)
    {
        yield return new WaitUntil(()=>ONV.NetOb.IsSpawned);
        ONV.FinishSetup();
    }
}
