using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXRand : MonoBehaviour
{
    public static OXRand Instance;
    public List<SpecialRands> SpecialRandoms = new List<SpecialRands>();
    private void Awake()
    {
        Instance = this;
        foreach(var a in SpecialRandoms)
        {
            specialrands.Add(a.Name, a);
        }
        SetOrigin(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    Dictionary<string, System.Random> rands =new Dictionary<string, System.Random>();
    Dictionary<string, SpecialRands> specialrands =new Dictionary<string, SpecialRands>();
    public int GlobalSeed;
    
    public void GetRandom(string name, int min, int max)
    {
        if (!rands.ContainsKey(name)) rands.Add(name, new System.Random(rands["Global"].Next(int.MinValue, int.MaxValue)));
        rands[name].Next(min, max);
    }
    public void SetOrigin(int seed)
    {
        rands.Clear();
        GlobalSeed = seed;
        rands.Add("Global", new System.Random(seed));
    }
}
[System.Serializable]
public class SpecialRands
{
    public string Name;
    public bool CanBeSaved = false;
    public bool UseSetSeed = false;
    public int StartingSeed;
}
