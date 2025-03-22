using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StoredRandom : MonoBehaviour
{
    public bool AutoSaveAndLoad = true;
    public Dictionary<string, OXRandStore> randoms = new Dictionary<string, OXRandStore>();
    public static StoredRandom Instance;
    private void Awake()
    {
        Instance = this;
        if (AutoSaveAndLoad)
        {
            SaveSystem.SaveAllData.Append(SS_SaveRandData);
            SaveSystem.LoadAllData.Append(SS_LoadRandData);
        }
    }

    public void DefineRandom(string name, int start = -1)
    {
        if (randoms.ContainsKey(name))
        {
            randoms[name] = new OXRandStore(start, name);
        }
        else
        {
            randoms.Add(name, new OXRandStore(start, name));
        }
    }
    public void UnallocateRandom(string name)
    {
        randoms.Remove(name);
    }

    public System.Random GetRand(string a) 
    {
        randoms[a].Hits++;
        return randoms[a].Rand; 
    }   

    public string ExportToString()
    {
        List<string> list = new List<string>();
        foreach(var a in randoms)
        {
            list.Add(a.Value.ConvertToString());
        }
        Debug.Log("S: " + Converter.ListToString(list, "||"));
        return Converter.ListToString(list, "||");

    }
    public void ImportFromString(string aa)
    {
        randoms.Clear();
        if (aa == "")
        {
            return;
        }
        List<string> list = Converter.StringToList(aa, "||");
        foreach(var a in list)
        {
            var e = new OXRandStore(a);
            randoms.Add(e.Name, e);
        }
    }

    public void SS_SaveRandData(string dict)
    {
        SaveSystem.Instance.SetString("Randoms", ExportToString(), dict);
    }
    public void SS_LoadRandData(string dict)
    {
        ImportFromString(SaveSystem.Instance.GetString("Randoms", "", dict));
    }


}

public class OXRandStore
{
    public System.Random Rand;
    public int Hits = 0;
    public int Initial = 0;
    public string Name = "";
    public OXRandStore(int start, string name)
    {
        if (start != -1)
        {
            Rand = new System.Random(start);
        }
        else
        {
            start = new System.Random().Next(int.MinValue, int.MaxValue);
            Rand = new System.Random(start);
        }
        Initial = start;
        Name = name;
    }
    public OXRandStore(string initaldata)
    {
        StringToData(initaldata);
    }
    public string ConvertToString()
    {
        var aa = Converter.ListToString(new List<string>()
        {
            Name,
            Hits.ToString(),
            Initial.ToString(),
        }, ";;");
        Debug.Log(aa);
        return aa;
    }
    public void StringToData(string data)
    {
        var dat = Converter.StringToList(data, ";;");
        Name = dat[0];
        Hits = int.Parse(dat[1]);
        Initial = int.Parse(dat[2]);
        Rand = new System.Random(Initial);
        for(int i = 0; i < Hits; i++) // I cant think of a better way to do this lol
        {
            Rand.Next();
        }
    }
}