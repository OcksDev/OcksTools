using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class SpawnSystem : MonoBehaviour
{
    public List<Pool> Spawnables = new List<Pool>();
    List<string> parentdata = new List<string>();
    public static SpawnSystem Instance;
    public static Dictionary<string, Pool> SpawnableDict = new Dictionary<string, Pool>();
    public static Action<string> SpawnShareMethod;
    private void Awake()
    {
        Instance = this;
        foreach(var a in Spawnables)
        {
            SpawnableDict.Add(a.Name, a);
        }
    }
    public static GameObject BasicSpawn(string nerd, Vector3 pos = default, Quaternion rot = default, Transform parent = null)
    {
        if(parent != null)
        {
            return Instantiate(SpawnableDict[nerd].Object, pos, rot, parent);
        }
        else
        {
            return Instantiate(SpawnableDict[nerd].Object, pos, rot);
        }
    }
    public static GameObject Spawn(SpawnData sp)
    {
        var a = BasicSpawn(sp.nerd, sp.pos, sp.rot, sp.parent);
        sp.GameObject = a;

        Tags.AddObjectToTag(sp, sp.ID, "Spawns");
        Tags.AddObjectToTag(a, sp.ID, "Exist");

        if (sp.share && SpawnShareMethod != null)
        {
            SpawnShareMethod(sp.ConvertToString());
        }

        return a;
    }
    public static void Kill(GameObject nerd)
    {
        Tags.ClearAllOf(Tags.GetIDOf(nerd));
        Destroy(nerd);
    }
    public static SpawnData GetData(GameObject nerd)
    {
        return Tags.GetFromTag<SpawnData>("Spawns", Tags.GetIDOf(nerd));
    }
}


public class SpawnData
{
    public string nerd;
    public GameObject GameObject;
    public string ID;
    public Vector3 pos;
    public Quaternion rot;
    public Transform parent;
    public bool share;
    public Dictionary<string, string> data;
    public SpawnData(string nerd)
    {
        this.nerd = nerd;
        ID = Tags.GenerateID();
    }
    public SpawnData(string nerd,int i)
    {
        //parse data from nerd
        FromString(nerd);
    }

    public SpawnData Position(Vector3 pos)
    {
        this.pos = pos;
        return this;
    }
    public SpawnData Rotation(Quaternion pos)
    {
        this.rot = pos;
        return this;
    }
    public SpawnData Parent(Transform p)
    {
        this.parent = p;
        return this;
    }
    public SpawnData MultiplayerShare()
    {
        share = true; 
        return this;
    }
    public SpawnData Data(Dictionary<string,string> d)
    {
        this.data = d;
        return this;
    }


    public string ConvertToString()
    {
        Dictionary<string,string> da = new Dictionary<string,string>();
        da.Add("ID", ID);
        da.Add("pos", pos.ToString());
        da.Add("rot", rot.ToString());
        //da.Add("par", Tags.GetIDOf(parent.gameObject)); //tbd
        da.Add("dat", Converter.EscapedDictionaryToString(data));

        // deliberately not saving share

        return Converter.EscapedDictionaryToString(da);
    }

    public void FromString(string a)
    {
        Dictionary<string,string> da = Converter.EscapedStringToDictionary(a);
        ID = da["ID"];
        pos = Converter.StringToVector3(da["pos"]);
        rot = Converter.StringToQuaternion(da["rot"]);
        data = Converter.EscapedStringToDictionary(da["dat"]);
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