using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : SingleInstance<SpawnSystem>
{
    public List<Pool> Spawnables = new List<Pool>();
    List<string> parentdata = new List<string>();
    public static Dictionary<string, Pool> SpawnableDict = new Dictionary<string, Pool>();
    public static Action<string> SpawnShareMethod;
    public override void Awake2()
    {
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
        GameObject a = null;
        if (sp._dospawn)
        {
            a = BasicSpawn(sp.nerd, sp._pos, sp._rot, sp._parent);
            sp.GameObject = a;
        }
        else
        {
            a = sp.GameObject;
        }
        Tags.AddObjectToTag(a, sp._IDValue, "Exist");
        Tags.AddObjectToTag(sp, sp._IDValue, "Spawns");

        if (sp._share && SpawnShareMethod != null)
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
    public string _IDValue;
    public Vector3 _pos;
    public Quaternion _rot = Quaternion.identity;
    public Transform _parent;
    public string _parentrefid = "";
    public bool _share;
    public bool _dospawn = true;
    public Dictionary<string, string> data = new Dictionary<string, string>();
    public SpawnData(string nerd)
    {
        this.nerd = nerd;
        _IDValue = Tags.GenerateID();
    }
    public SpawnData(string nerd,int i)
    {
        //parse data from nerd
        FromString(nerd);
    }

    public SpawnData Position(Vector3 pos)
    {
        this._pos = pos;
        return this;
    }
    public SpawnData Rotation(Quaternion pos)
    {
        this._rot = pos;
        return this;
    }
    public SpawnData Parent(Transform p)
    {
        this._parent = p;
        return this;
    }
    public SpawnData Parent(GameObject p)
    {
        this._parent = p.transform;
        return this;
    }
    public SpawnData Parent(string id)
    {
        this._parent = Tags.GetFromTag<GameObject>("Exist", id).transform;
        return this;
    }
    public SpawnData ParentFromRef(string refd)
    {
        this._parent = Tags.refs[refd].transform;
        _parentrefid = refd;
        return this;
    }
    public SpawnData MultiplayerShare()
    {
        _share = true; 
        return this;
    }
    public SpawnData DontSpawn(GameObject a)
    {
        _dospawn = false;
        GameObject = a;
        return this;
    }
    public SpawnData Data(Dictionary<string,string> d)
    {
        this.data = d;
        return this;
    }
    public SpawnData ID(string i)
    {
        this._IDValue = i;
        return this;
    }


    public string ConvertToString()
    {
        Dictionary<string,string> da = new Dictionary<string,string>();
        da.Add("nerd", nerd);
        da.Add("ID", _IDValue);
        if(_pos != default)da.Add("pos", _pos.ToString());
        if (_rot != Quaternion.identity) da.Add("rot", _rot.ToString());
        if (_parent != null)
        {
            if (_parentrefid == "")
            {
                da.Add("par", Tags.GetIDOf(_parent.gameObject));
            }
            else
            {
                da.Add("par", "");
                da.Add("par_id", _parentrefid);
            }
        }
        if(data.Count > 0)da.Add("dat", Converter.EscapedDictionaryToString(data, "!", "?"));

        // deliberately not saving share

        return Converter.EscapedDictionaryToString(da, ":", ";");
    }

    public void FromString(string a)
    {
        Dictionary<string,string> da = Converter.EscapedStringToDictionary(a, ":", ";");
        nerd = da["nerd"];
        _IDValue = da["ID"];
        if (da.ContainsKey("pos")) _pos = Converter.StringToVector3(da["pos"]);
        if (da.ContainsKey("rot")) _rot = Converter.StringToQuaternion(da["rot"]);
        if (da.ContainsKey("dat")) data = Converter.EscapedStringToDictionary(da["dat"], "!", "?");
        if (da.ContainsKey("par"))
        {

            if (!da.ContainsKey("par_id"))
            {
                _parent = Tags.GetFromTag<GameObject>("Exist", da["par"]).transform;
            }
            else
            {
                _parent = Tags.refs[da["par_id"]].transform;
            }
        }
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