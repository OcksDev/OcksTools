using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tags
{
    public static Dictionary<string, OXTag> BigTags = new()
    {
        {"ID", new OXTag()}
    };
    public static OXTag From(string tag)
    {
        BigTags.AddIfUnique(tag, new OXTag());
        return BigTags[tag];
    }
    public static OXTag ID => BigTags["ID"];
    public static void ClearAllOf(string key)
    {
        //should go and clear any instance of the ID found in any tag
        object gm = null;
        if (ID.Has(key)) gm = ID.Get<object>(key);
        ClearAllOf(key, gm);
    }

    public static void ClearAllOf(string key, object gm)
    {
        foreach (var a in BigTags) { a.Value.ClearAllOf(key, gm); }
    }

    public static string GenerateID()
    {
        return Guid.NewGuid().ToString();
    }
}

public static class TagsExtensions
{
    public static void AddTag(this GameObject go, string tag, string id)
    {
        Tags.From(tag).Add(go, id);
    }
    public static void AddTag(this Component go, string tag, string id)
    {
        Tags.From(tag).Add(go, id);
    }
    public static string GetOXID(this Component go) => Tags.ID.Get(go);
}

public class OXTag
{
    public Dictionary<string, object> StToOb = new();
    public Dictionary<object, string> ObToSt = new();

    public T Get<T>(string name)
    {
        return (T)StToOb[name];
    }

    public string Get(object a)
    {
        return ObToSt[a];
    }
    public bool Has(string name)
    {
        return StToOb.ContainsKey(name);
    }
    public void Add(object a, string namee)
    {
        StToOb.Add(namee, a);
        ObToSt.Add(a, namee);
    }
    public void ClearAllOf(string key, object gm)
    {
        StToOb.Remove(key);
        if (gm == null) return;
        ObToSt.Remove(gm);
    }
}