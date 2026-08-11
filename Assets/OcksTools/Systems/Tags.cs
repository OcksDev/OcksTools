using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tags
{
    public static Dictionary<string, Dictionary<string, object>> AllTags = new Dictionary<string, Dictionary<string, object>>()
    {
        {"Exist", new Dictionary<string, object>()}
    };
    public static Dictionary<string, Dictionary<object, string>> AllTagsReverse = new Dictionary<string, Dictionary<object, string>>()
    {
        {"Exist", new Dictionary<object, string>()}
    };
    public static void CreateTag(string tag)
    {
        AllTags.Add(tag, new Dictionary<string, object>());
        AllTagsReverse.Add(tag, new Dictionary<object, string>());
    }
    public static string GetIDOf(object a, string tag = "Exist")
    {
        var aa = AllTagsReverse[tag];
        return aa.ContainsKey(a) ? aa[a] : "";
    }
    public static T GetFromTag<T>(string name, string tag = "Exist")
    {
        return (T)AllTags[tag][name];
    }
    public static bool IsInTag(string name, string tag = "Exist")
    {
        return AllTags[tag].ContainsKey(name);
    }
    public static void AddObjectToTag(object a, string namee, string tag)
    {
        if (!AllTags.ContainsKey(tag) || !AllTagsReverse.ContainsKey(tag)) CreateTag(tag);
        AllTags[tag].Add(namee, a);
        AllTagsReverse[tag].Add(a, namee);
    }

    public static void ClearAllOf(string key)
    {
        //should go and clear any instance of the ID found in any tag
        GameObject gm = null;

        if (AllTags["Exist"].ContainsKey(key))
        {
            gm = (GameObject)AllTags["Exist"][key];
        }
        ClearAllOf(key, gm);
    }

    public static void ClearAllOf(string key, GameObject gm)
    {
        foreach (var a in AllTags)
        {
            if (AllTags[a.Key].ContainsKey(key)) AllTags[a.Key].Remove(key);
        }
        if (gm == null) return;
        foreach (var a in AllTagsReverse)
        {
            if (AllTagsReverse[a.Key].ContainsKey(gm)) AllTagsReverse[a.Key].Remove(gm);
        }

    }
    public static void DefineTagReference(GameObject boner, string id)
    {
        if (!AllTags["Exist"].ContainsKey(id)) AllTags["Exist"].Add(id, boner);
        if (!AllTagsReverse["Exist"].ContainsKey(boner)) AllTagsReverse["Exist"].Add(boner, id);
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
        Tags.AddObjectToTag(go, id, tag);
    }
    public static void AddTag(this Component go, string tag, string id)
    {
        Tags.AddObjectToTag(go, id, tag);
    }
    public static string GetOXID(this Component go) => Tags.GetIDOf(go);
}