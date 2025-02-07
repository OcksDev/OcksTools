using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnData : MonoBehaviour
{
    public Dictionary<string,string> Data= new Dictionary<string, string>();
    public Dictionary<string, string> Hidden_Data = new Dictionary<string, string>();
    public bool IsReal; // a boolean for the ages
    public void Start()
    {
        if (Hidden_Data.Count == 0) Hidden_Data = RandomFunctions.GenerateBlankHiddenData();

        Tags.DefineTagReference(gameObject, Hidden_Data["ID"]);
    }

    private void OnDestroy()
    {
        Tags.ClearAllOf(Hidden_Data["ID"]);
    }
}
