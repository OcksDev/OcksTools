using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public List<Pool> Spawnables = new List<Pool>();
    List<string> parentdata = new List<string>();
    public static SpawnSystem Instance;
    public Dictionary<string, Pool> SpawnableDict = new Dictionary<string, Pool>();
    private void Awake()
    {
        Instance = this;
        foreach(var a in Spawnables)
        {
            SpawnableDict.Add(a.Name, a);
        }
    }
    public GameObject Spawn(string nerd)
    {
        var a  = Instantiate(SpawnableDict[nerd].Object);
        string my_id = Tags.GenerateID();

        var data = new ObjectData(a,my_id);

        Tags.AddObjectToTag(data, my_id, "Spawns");
        return a;
    }


}

public class ObjectData : UnityEngine.Object
{
    public GameObject Object;
    public string ID;
    public ObjectData(GameObject a, string id)
    {
        Object = a;
        ID = id;
    }
}

[Serializable]
public class Pool
{
    public string Name = "";
    public GameObject Object;
    /*public int PoolSize = 0;
    public bool UsePoolForObject = true;
    [HideInInspector]
    public Queue<GameObject> PoolLol = new Queue<GameObject>();

    public GameObject PullObject()
    {
        if (PoolLol.Count == 0) SpawnSystem.Instance.SpawnPoolObject(this);
        var e = PoolLol.Dequeue();
        e.SetActive(true);
        return e;
    }
    public void ReturnObject(GameObject ret)
    {
        ret.SetActive(false);
        PoolLol.Enqueue(ret);
    }*/
}