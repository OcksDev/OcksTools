using System.Collections.Generic;

public class OXRand : SingleInstance<OXRand>
{
    public List<SpecialRands> SpecialRandoms = new List<SpecialRands>();
    public override void Awake2()
    {
        foreach (var a in SpecialRandoms)
        {
            specialrands.Add(a.Name, a);
        }
        SetOrigin(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    private Dictionary<string, System.Random> rands = new Dictionary<string, System.Random>();
    private Dictionary<string, SpecialRands> specialrands = new Dictionary<string, SpecialRands>();
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
