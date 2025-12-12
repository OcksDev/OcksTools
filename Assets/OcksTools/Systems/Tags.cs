using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tags : SingleInstance<Tags>
{
    public static Dictionary<string, Dictionary<string, object>> AllTags = new Dictionary<string, Dictionary<string, object>>()
    {
        {"Exist", new Dictionary<string, object>()}
    };
    public static Dictionary<string, Dictionary<object, string>> AllTagsReverse = new Dictionary<string, Dictionary<object, string>>()
    {
        {"Exist", new Dictionary<object, string>()}
    };
    [HideInInspector]
    public static Dictionary<string, GameObject> refs = new Dictionary<string, GameObject>();
    public List<OXTagRefThing> RefedObjects = new List<OXTagRefThing>();
    /*
     * Tags Help:
     * To check if a gameobject has a certain tag: see if the tag list contains the string ID of the gameobject(s)
     * string ID of a gamobject should be stored in SpawnData.Data[0]
     * 
     */


    public override void Awake2()
    {
        foreach (var a in RefedObjects)
        {
            a.Zoink();
        }
    }

    public static void CreateTag(string tag)
    {
        AllTags.Add(tag, new Dictionary<string, object>());
        AllTagsReverse.Add(tag, new Dictionary<object, string>());
    }

    public static bool ObjectHasTag(object objecty, string tag)
    {
        return AllTagsReverse[tag].ContainsKey(objecty);
    }

    public static bool ObjectHasTag(string objecty, string tag)
    {
        return AllTags[tag].ContainsKey(objecty);
    }
    public static string GetIDOf(object a, string tag = "Exist")
    {
        var aa = AllTagsReverse[tag];
        return aa.ContainsKey(a) ? aa[a] : "";
    }
    public static T GetFromTag<T>(string tag, string name)
    {
        return (T)AllTags[tag][name];
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
        foreach (var a in AllTags)
        {
            if (AllTags[a.Key].ContainsKey(key)) AllTags[a.Key].Remove(key);
        }
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

    public static void SetRef(string name, GameObject ob)
    {
        refs.AddOrUpdate(name, ob);
    }
    public static string GenerateID()
    {
        return Guid.NewGuid().ToString();
    }
}

[System.Serializable]
public class OXTagRefThing
{
    public string Name;
    public GameObject Object;
    public void Zoink()
    {
        if (Name == "") Name = Object.name;
        Tags.SetRef(Name, Object);
    }
}