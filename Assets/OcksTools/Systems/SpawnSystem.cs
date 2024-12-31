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

    private void Start()
    {
        StartCoroutine(SpawnPools());
    }

    public IEnumerator SpawnPools()
    {
        // dictates the amount of objects to spawn every 50th of a second, in other words spawning amount*50 objects per second.
        int simulateousspawns = 1;
        int sim = 0;
        for (int i = 0; i < Spawnables.Count; i++)
        {
            if (!Spawnables[i].UsePoolForObject) continue;
            for (int z = 0; z < Spawnables[i].PoolSize; z++)
            {
                sim++;
                if (sim >= simulateousspawns) { yield return new WaitForFixedUpdate(); sim = 0; }
                SpawnPoolObject(Spawnables[i]);
            }
        }
    }

    public GameObject SpawnObject(string name, GameObject parent, Vector3 pos, Quaternion rot, bool SendToEveryone = false, string data = "", string hidden_data = "")
    {
        //object parenting over multiplayer is untested
        List<string> dadalol = Converter.StringToList(data);
        List<string> hidden_dadalol = Converter.StringToList(hidden_data);
        if (hidden_data == "")
        {
            hidden_dadalol = RandomFunctions.Instance.GenerateBlankHiddenData();
        }

        //object parenting using Tags, should work over multiplayer, untested
        if (hidden_dadalol[2] != "-" && Tags.AllTags["Exist"].ContainsKey(hidden_dadalol[2]))
        {
            parent = (GameObject)Tags.AllTags["Exist"][hidden_dadalol[2]];
        }
        if (Tags.AllTagsReverse["Exist"].ContainsKey(parent))
        {
            hidden_dadalol[2] = Tags.AllTagsReverse["Exist"][parent];
        }

        //incase you want to run some stuff here based on the object that is going to be spawned
        switch (name)
        {
            case "":
                break;
        }

        GameObject f; //Instantiate(Pools[refe].Object, pos, rot, parent.transform);

        if (SpawnableDict[name].UsePoolForObject)
        {
            f = SpawnableDict[name].PullObject();
            f.transform.position = pos;
            f.transform.rotation = rot;
            f.transform.parent = parent.transform;
        }
        else
        {
            f = Instantiate(SpawnableDict[name].Object, pos, rot, parent.transform);
        }

        var d = f.GetComponent<SpawnData>();
        if (d != null)
        {
            //Requires objects to have SpawnData.cs to allow for data sending
            d.Data = dadalol;
            d.Hidden_Data = hidden_dadalol;
            d.IsReal = hidden_dadalol[1] == "true";
            d.Start();
        }

        if (SendToEveryone)
        {
            // This code works, its just commented out by default because it requires Ocks Tools Multiplayer to be added
            //used for syncing the spawn of a local gameobject over the network instead of being a networked object

            //known issue: object parent is not preserved when spawning a local object over multiplayer


            //ServerGamer.Instance.SpawnObjectServerRpc(refe, pos, rot, ClientID, RandomFunctions.Instance.ListToString(dadalol), RandomFunctions.Instance.ListToString(hidden_dadalol));
        }
        return f;
    }
    public void SpawnPoolObject(Pool pp)
    {
        var e = Instantiate(pp.Object, transform.position, Quaternion.identity, transform);
        e.SetActive(false);
        pp.PoolLol.Enqueue(e);
    }

    public void ReturnObject(GameObject sex, string pool)
    {
        SpawnableDict[pool].ReturnObject(sex);
    }
    public GameObject QuickSpawnObject(string name)
    {
        if (SpawnableDict[name].UsePoolForObject)
        {
            return SpawnableDict[name].PullObject();
        }
        else
        {
            return Instantiate(SpawnableDict[name].Object);
        }
    }
}

[Serializable]
public class Pool
{
    public string Name = "";
    public GameObject Object;
    public int PoolSize = 0;
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
    }
}