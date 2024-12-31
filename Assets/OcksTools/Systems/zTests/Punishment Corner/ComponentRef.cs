using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
/*
public class ComponentRef : MonoBehaviour
{
    public List<string> Components = new List<string>();
    void Awake()
    {
        OXComponent.StoreComponents(gameObject, Components);
    }
    private void OnDestroy()
    {
        OXComponent.ClearOf(gameObject);
    }
}


public class OXComponent
{
    public static Dictionary<GameObject, Dictionary<string, Component>> StoredComps = new Dictionary<GameObject, Dictionary<string, Component>>();
    public static void StoreComponents(GameObject nerd, List<string> boners)
    {
        var weenor = Converter.ListToDictionary(boners); // this makes it slightly faster, trust me
        //add new values below as needed
        if (weenor.ContainsKey("Entity")) StoreComponent<EntityOXS>(nerd);
        if (weenor.ContainsKey("SpawnData")) StoreComponent<SpawnData>(nerd);
        if (weenor.ContainsKey("NavMeshEntity")) StoreComponent<NavMeshEntity>(nerd);
        if (weenor.ContainsKey("Text")) StoreComponent<TextMeshProUGUI>(nerd);
    }

    public static void StoreComponent<T>(GameObject sus) where T : Component
    {
        StoreComponent(sus.GetComponent<T>());
        
    }
    public static void StoreComponent<T>(T sus) where T : Component
    {
        if (!StoredComps.ContainsKey(sus.gameObject))
        {
            StoredComps.Add(sus.gameObject, new Dictionary<string, Component>());
        }
        string name = typeof(T).Name;
        if (!StoredComps[sus.gameObject].ContainsKey(name))
        {
            if(sus != null) StoredComps[sus.gameObject].Add(name, sus);
        }
    }
    public static void StoreComponentsInChildren<T>(GameObject sus) where T : Component
    {
        var e = sus.transform.childCount;
        for (int i = 0; i < e; i++)
        {
            StoreComponent<T>(sus.transform.GetChild(i).gameObject);
        }
        StoreComponent<T>(sus);
    }
    public static void StoreComponentsInChildrenRecursive<T>(GameObject sus) where T : Component
    {
        var e = sus.transform.childCount;
        for (int i = 0; i < e; i++)
        {
            StoreComponentsInChildrenRecursive<T>(sus.transform.GetChild(i).gameObject);
        }
        StoreComponent<T>(sus);
    }
    public static void StoreComponentsInChildren(GameObject sus, List<string> pred)
    {
        var e = sus.transform.childCount;
        for (int i = 0; i < e; i++)
        {
            StoreComponents(sus.transform.GetChild(i).gameObject, pred);
        }
        StoreComponents(sus, pred);
    }
    public static void StoreComponentsInChildrenRecursive(GameObject sus, List<string> pred)
    {
        var e = sus.transform.childCount;
        for (int i = 0; i < e; i++)
        {
            StoreComponentsInChildrenRecursive(sus.transform.GetChild(i).gameObject, pred);
        }
        StoreComponents(sus, pred);
    }

    public static T GetComponent<T>(GameObject sussy, string name, bool storeifnull = true) where T : Component
    {
        try
        {
            return (T)StoredComps[sussy][name];
        }
        catch
        {

        }
        /*
        if (StoredComps.TryGetValue(sussy, out Dictionary<string, Component> comps))
        {
            if(comps.TryGetValue(typeof(T).Name, out Component thin))
            {
                return thin;
            }
        }
        if (storeifnull)
        {
            StoreComponent(sussy.GetComponent<T>());
            Debug.LogError("No");
            if (StoredComps.TryGetValue(sussy, out Dictionary<string, Component> comps2))
            {
                if (comps2.TryGetValue(typeof(T).Name, out Component thin))
                {
                    return (T)thin;
                }
            }
        }
        return null;
    }
    public static List<T> GetComponentsInChildren<T>(GameObject sussy, string name, bool storeifnull = true) where T : Component
    {
        List<T> founds = new List<T>();
        var e = sussy.transform.childCount;
        T comp;
        comp = GetComponent<T>(sussy, name, storeifnull);
        if (comp != null)
            founds.Add(comp);
        for (int i = 0; i < e; i++)
        {
            comp = GetComponent<T>(sussy.transform.GetChild(i).gameObject, name, storeifnull);
            if (comp != null)
                founds.Add(comp);
        }
        return founds;
    }
    public static List<T> GetComponentsInChildrenRecursive<T>(GameObject sussy, string name, bool storeifnull = true) where T : Component
    {
        List<T> founds = new List<T>();
        var e = sussy.transform.childCount;
        T comp;
        for (int i = 0; i < e; i++)
        {
            var weenis = GetComponentsInChildrenRecursive<T>(sussy.transform.GetChild(i).gameObject, name, storeifnull);
            if(weenis.Count > 0)
            founds = RandomFunctions.CombineLists(founds, weenis);
        }
        comp = GetComponent<T>(sussy, name, storeifnull);
        if (comp != null)
            founds.Add(comp);
        return founds;
    }
    public static void CleanUp()
    {
        for(int i = 0; i < StoredComps.Count; i++)
        {
            var wee = StoredComps.ElementAt(i);
            if (wee.Key == null)
            {
                StoredComps.Remove(wee.Key);
                i--;
            }
        }
        Dictionary<GameObject,int> recheck = new Dictionary<GameObject,int> ();
        for(int i = 0; i < StoredComps.Count; i++)
        {
            var kp = StoredComps.ElementAt(i);
            for (int j = 0; j < StoredComps[kp.Key].Count; j++)
            {
                var wee = kp.Value.ElementAt(j);
                if (wee.Value == null)
                {
                    if (!recheck.ContainsKey(kp.Key)) recheck.Add(kp.Key, 0);
                    kp.Value.Remove(wee.Key);
                    i--;
                }
            }
        }
        for (int i = 0; i < recheck.Count; i++)
        {
            var wee = recheck.ElementAt(i);
            if (wee.Key == null)
            {
                StoredComps.Remove(wee.Key);
                recheck.Remove(wee.Key);
                i--;
            }
        }

    }
    public static void ClearOf(GameObject sus)
    {
        if (StoredComps.ContainsKey(sus)) StoredComps.Remove(sus);
    }
}
*/