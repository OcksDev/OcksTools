using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public class Tags : MonoBehaviour
{
    public static Dictionary<string, Dictionary<string, UnityEngine.Object>> AllTags = new Dictionary<string, Dictionary<string, UnityEngine.Object>>()
    {
        {"Exist", new Dictionary<string, UnityEngine.Object>()}
    };
    public static Dictionary<string, Dictionary<UnityEngine.Object, string>> AllTagsReverse = new Dictionary<string, Dictionary<UnityEngine.Object, string>>()
    {
        {"Exist", new Dictionary<UnityEngine.Object, string>()}
    };
    [HideInInspector]
    [DoNotSerialize]
    public static Dictionary<string, GameObject> refs = new Dictionary<string, GameObject>();
    public List<OXTagRefThing> RefedObjects = new List<OXTagRefThing>();
    /*
     * Tags Help:
     * To check if a gameobject has a certain tag: see if the tag list contains the string ID of the gameobject(s)
     * string ID of a gamobject should be stored in SpawnData.Data[0]
     * 
     */

    public static Tags Instance;

    public void Awake()
    {
        if(Instance == null) Instance= this;
        foreach(var a in RefedObjects)
        {
            a.Zoink();
        }
    }

    public static void CreateTag(string tag)
    {
        AllTags.Add(tag, new Dictionary<string, UnityEngine.Object>());
        AllTagsReverse.Add(tag, new Dictionary<UnityEngine.Object, string>());
    }
    public static string GetIDOf(UnityEngine.Object a)
    {
        var aa = AllTagsReverse["Exist"];
        return aa.ContainsKey(a) ? aa[a] :"";
    }
    public static void AddObjectToTag(UnityEngine.Object a, string namee, string tag)
    {
        if(!AllTags.ContainsKey(tag) || !AllTagsReverse.ContainsKey(tag)) CreateTag(tag);
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
            OXComponent.ClearOf(gm);
        }
        foreach(var a in AllTags)
        {
            if (AllTags[a.Key].ContainsKey(key)) AllTags[a.Key].Remove(key);
        }
        foreach(var a in AllTagsReverse)
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
        if (refs.ContainsKey(name))
        {
            refs[name] = ob;
        }
        else
        {
            refs.Add(name, ob);
        }
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