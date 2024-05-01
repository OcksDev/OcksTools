using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public List<Pool> Pools = new List<Pool>();
    List<string> parentdata = new List<string>();
    public static SpawnSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnPools());
    }

    public IEnumerator SpawnPools()
    {
        int simulateousspawns = 1;
        int sim = 0;
        for (int i = 0; i < Pools.Count; i++)
        {
            for (int z = 0; z < Pools[i].PoolSize; z++)
            {
                sim++;
                if (sim >= simulateousspawns) { yield return new WaitForFixedUpdate(); sim = 0; }
                SpawnPoolObject(Pools[i]);
            }
        }
    }
    public void SpawnPoolObject(Pool pp)
    {
        var e = Instantiate(pp.Object, transform.position, Quaternion.identity, transform);
        e.SetActive(false);
        pp.PoolLol.Enqueue(e);
    }

    public void ReturnObject(GameObject sex, int pool)
    {
        Pools[pool].ReturnObject(sex);
    }
    public GameObject QuickSpawnObject(int i)
    {
        //only works for pooled objects
        return Pools[i].PullObject();
    }

    public GameObject SpawnObject(int refe, GameObject parent, Vector3 pos, Quaternion rot, bool SendToEveryone = false, string data = "", string hidden_data = "")
    {
        //object parenting over multiplayer is untested
        List<string> dadalol = RandomFunctions.Instance.StringToList(data);
        List<string> hidden_dadalol = RandomFunctions.Instance.StringToList(hidden_data);
        if (hidden_data == "")
        {
            hidden_dadalol = RandomFunctions.Instance.GenerateBlankHiddenData();
        }

        //object parenting using Tags, should work over multiplayer, untested
        if (hidden_dadalol[2] != "-" && Tags.dict.ContainsKey(hidden_dadalol[2]))
        {
            parent = Tags.dict[hidden_dadalol[2]];
        }
        if (Tags.gameobject_dict.ContainsKey(parent))
        {
            hidden_dadalol[2] = Tags.gameobject_dict[parent];
        }

        //incase you want to run some stuff here based on the object that is going to be spawned
        switch (refe)
        {
            case 0:
                break;
        }

        GameObject f; //Instantiate(Pools[refe].Object, pos, rot, parent.transform);

        if (Pools[refe].UsePoolForObject)
        {
            f = Pools[refe].PullObject();
            f.transform.position = pos;
            f.transform.rotation = rot;
            f.transform.parent = parent.transform;
        }
        else
        {
            f = Instantiate(Pools[refe].Object, pos, rot, parent.transform);
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
}

[Serializable]
public class Pool
{
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