using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredRandom : MonoBehaviour
{
    public Dictionary<string, OXRandStore> randoms = new Dictionary<string, OXRandStore>();

    public void DefineRandom(string name, int start)
    {
        if (randoms.ContainsKey(name))
        {
            randoms[name] = new OXRandStore(start);
        }
        else
        {
            randoms.Add(name, new OXRandStore(start));
        }
    }

    public System.Random GetRand(string a) { return randoms[a].Rand; }


}

public class OXRandStore
{
    public System.Random Rand;
    public int Hits = 0;
    public int Initial = 0;
    public OXRandStore(int start)
    {
        Rand = new System.Random(start);
        Initial = start;
    }
    public string ConvertToString()
    {
        return Converter.ListToString(new List<string>()
        {
            Hits.ToString(),
            Initial.ToString(),
        }, ";;");
    }
    public void StringToData(string data)
    {
        var dat = Converter.StringToList(data, ";;");
        Hits = int.Parse(dat[0]);
        Initial = int.Parse(dat[1]);
        Rand = new System.Random(Initial);
        for(int i = 0; i < Hits; i++) // I cant think of a better way to do this lol
        {
            Rand.Next();
        }
    }
}