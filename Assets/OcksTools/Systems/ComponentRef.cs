using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    public static Dictionary<GameObject, Dictionary<string, MonoBehaviour>> StoredComps = new Dictionary<GameObject, Dictionary<string, MonoBehaviour>>();
    public static void StoreComponents(GameObject nerd, List<string> boners)
    {
        var weenor = Converter.ListToDictionary(boners); // this makes it slightly faster, trust me
        //add new values below as needed
        if (weenor.ContainsKey("Entity")) StoreComponent<EntityOXS>(nerd);
        if (weenor.ContainsKey("SpawnData")) StoreComponent<SpawnData>(nerd);
        if (weenor.ContainsKey("NavMeshEntity")) StoreComponent<NavMeshEntity>(nerd);
        if (weenor.ContainsKey("Text")) StoreComponent<TextMeshProUGUI>(nerd);
    }

    private static void StoreComponent<T>(GameObject sus) where T : MonoBehaviour
    {
        if (!StoredComps.ContainsKey(sus))
        {
            StoredComps.Add(sus, new Dictionary<string, MonoBehaviour>());
        }
        string name = typeof(T).Name;
        if (!StoredComps[sus].ContainsKey(name))
        {
            StoredComps[sus].Add(name, sus.GetComponent<T>());
        }
    }

    public static T GetComponent<T>(GameObject sussy) where T : MonoBehaviour
    {
        if (StoredComps.TryGetValue(sussy, out Dictionary<string, MonoBehaviour> comps))
        {
            if(comps.TryGetValue(typeof(T).Name, out MonoBehaviour thin))
            {
                return (T)thin;
            }
        }
        return null;
    }
    public static List<T> GetComponentsInChildren<T>(GameObject sussy) where T : MonoBehaviour
    {
        List<T> founds = new List<T>();
        var e = sussy.transform.childCount;
        T comp;
        comp = GetComponent<T>(sussy);
        if (comp != null)
            founds.Add(comp);
        for (int i = 0; i < e; i++)
        {
            comp = GetComponent<T>(sussy.transform.GetChild(i).gameObject);
            if (comp != null)
                founds.Add(comp);
        }
        return null;
    }
    public static List<T> GetComponentsInChildrenRecursive<T>(GameObject sussy) where T : MonoBehaviour
    {
        List<T> founds = new List<T>();
        var e = sussy.transform.childCount;
        T comp;
        int amnt = 0;
        GameObject held;
        comp = GetComponent<T>(sussy);
        if (comp != null)
            founds.Add(comp);
        for (int i = 0; i < e; i++)
        {
            held = sussy.transform.GetChild(i).gameObject;
            amnt = held.transform.childCount;
            comp = GetComponent<T>(held);
            if (comp != null)
                founds.Add(comp);
            if(amnt > 0)
            {
                var weenis = GetComponentsInChildrenRecursive<T>(held);
                founds = RandomFunctions.CombineLists(founds, weenis);
            }
        }
        return null;
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
    }
    public static void ClearOf(GameObject sus)
    {
        if (StoredComps.ContainsKey(sus)) StoredComps.Remove(sus);
    }
}
