using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tags : MonoBehaviour
{
    public static Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();
    public static Dictionary<GameObject, string> gameobject_dict = new Dictionary<GameObject, string>();
    public static Dictionary<string, SpawnData> SDs = new Dictionary<string, SpawnData>();
    public static List<string> tag1 = new List<string>();

    /*
     * Tags Help:
     * To check if a gameobject has a certain tag: see if the tag list contains the string ID of the gameobject(s)
     * string ID of a gamobject should be stored in SpawnData.Data[0]
     * 
     */

    public static void ClearAllOf(string key)
    {
        //should go and clear any instance of the ID found in any tag
        if (dict.ContainsKey(key) && gameobject_dict.ContainsKey(dict[key])) gameobject_dict.Remove(dict[key]);
        if (dict.ContainsKey(key)) dict.Remove(key);
        if (SDs.ContainsKey(key)) SDs.Remove(key);
        if (tag1.Contains(key)) tag1.Remove(key);
    }
    public static void GarbageCleanup()
    {
        //should locate any null gameobject references and remove them.
        int i = 0;
        while(i < dict.Count)
        {
            var d = dict.ElementAt(i);
            if (d.Value == null)
            {
                ClearAllOf(d.Key);
                i--;
            }
            i++;
        }
    }

    public static void DefineReference(GameObject boner, string id)
    {
        if (!dict.ContainsKey(id)) dict.Add(id, boner);
        if (!gameobject_dict.ContainsKey(boner)) gameobject_dict.Add(boner, id);
    }
}
