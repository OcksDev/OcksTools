using System.Collections.Generic;
using UnityEngine;

public class GlobalRefs : SingleInstance<GlobalRefs>
{
    [HideInInspector]
    public static Dictionary<string, GameObject> refs = new Dictionary<string, GameObject>();
    public List<OXObjectRefThing> RefedObjects = new List<OXObjectRefThing>();
    public override void Awake2()
    {
        foreach (var a in RefedObjects)
        {
            a.Zoink();
        }
    }
    public static void Set(string name, GameObject ob) => refs.AddOrUpdate(name, ob);
    public static GameObject Get(string name) => refs[name];
    public GameObject this[string key]
    {
        get => Get(key);
        set => Set(key, value);
    }
}

[System.Serializable]
public class OXObjectRefThing
{
    public string Name;
    public GameObject Object;
    public void Zoink()
    {
        if (Name == "") Name = Object.name;
        GlobalRefs.Set(Name, Object);
    }
}
