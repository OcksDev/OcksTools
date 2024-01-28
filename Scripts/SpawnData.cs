using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnData : MonoBehaviour
{
    public string Type = "";
    public List<string> Data= new List<string>();
    public List<string> Hidden_Data= new List<string>();
    public bool IsReal; // a boolean for the ages
    private void Start()
    {
        if (Hidden_Data.Count == 0) Hidden_Data = RandomFunctions.Instance.GenerateBlankHiddenData();

        Tags.DefineReference(gameObject, Hidden_Data[0]);
    }

    private void OnDestroy()
    {
        Tags.ClearAllOf(Hidden_Data[0]);
    }
}
