using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GISLol : MonoBehaviour
{
    public bool UseLanguageFile = true;
    public GISItem Mouse_Held_Item;
    public GISDisplay Mouse_Displayer;
    public GameObject MouseFollower;
    private static GISLol instance;
    public List<GISItem_Data> Items = new List<GISItem_Data>();
    public Dictionary<string,GISItem_Data> ItemDict = new Dictionary<string, GISItem_Data>();


    public Dictionary<string,GISContainer> All_Containers = new Dictionary<string, GISContainer>();


    public static GISLol Instance
    {
        get { return instance; }
    }

    public void LoadTempForAll()
    {
        foreach(var con in All_Containers)
        {
            if (con.Value != null) con.Value.LoadTempContents();
        }
    }

    private void Awake()
    {
        if (Instance == null) instance = this;
        Mouse_Held_Item = new GISItem();
        foreach(var item in Items)
        {
            ItemDict.Add(item.Name, item);
        }
        SaveSystem.SaveAllData += SaveAll;
    }


    private void Start()
    {
        MouseFollower.SetActive(true);

        if (UseLanguageFile)
        {
            
            var l = LanguageFileSystem.Instance;
            bool changed = false;
            for (int i = 0; i < Items.Count; i++)
            {
                var s = "Item_" + SaltName(Items[i].Name);
                var s2 = "ItemDesc_" + Items[i].Description;
                if (l.IndexValuePairs.ContainsKey(s))
                {
                    Items[i].Name = l.IndexValuePairs[s];
                }
                else
                {
                    changed = true;
                    l.IndexValuePairs.Add(s, Items[i].Name);
                }
                if (l.IndexValuePairs.ContainsKey(s2))
                {
                    Items[i].Description = l.IndexValuePairs[s2];
                }
                else
                {
                    changed = true;
                    l.IndexValuePairs.Add(s2, Items[i].Description);
                }
            }
            if (changed) l.UpdateTextFile();
            
        }
    }

    private void Update()
    {
        Mouse_Displayer.item = Mouse_Held_Item;
        var za = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        za.z = 0;
        MouseFollower.transform.position = za;

        if (InputManager.IsKeyDown(InputManager.gamekeys["reload"]))
        {
            LoadTempForAll();
        }
        if (InputManager.IsKeyDown(KeyCode.X))
        {
            Mouse_Held_Item = new GISItem();
        }
    }

    public void SaveAll()
    {
        foreach (var c in All_Containers)
        {
            if (c.Value != null && c.Value.SaveLoadData)
            {
                c.Value.SaveContents();
            }
        }
    }


    public string SaltName(string e)
    {
        e = e.Replace(" ", "");
        e = e.Replace(":", ";");
        return e;
    }
}


[System.Serializable]
public class GISItem
{
    /*
     * This class is for each item in the container, specifying individual data such as durability or amount
     * 
     * When adding new attributes for items, make sure to update the below functions:
     * GISItem.GISItem()
     * GISItem.GISItem(GISItem)
     * GISItem.Compare(GISItem)
     * GISContainer.LoadContents()
     */

    public string ItemIndex;
    public int Amount;
    public GISContainer Container;
    public List<GISContainer> Interacted_Containers = new List<GISContainer>();

    public GISItem()
    {
        setdefaultvals();
    }
    public GISItem(string base_type)
    {
        setdefaultvals();
        ItemIndex = base_type;
    }
    public GISItem(GISItem sexnut)
    {
        setdefaultvals();
        Amount = sexnut.Amount;
        ItemIndex = sexnut.ItemIndex;
        Container = sexnut.Container;
    }
    private void setdefaultvals()
    {
        Data = GetDefaultData();
        Amount = 0;
        ItemIndex = "Empty";
        Container = null;
    }
    public void Solidify()
    {
        AddConnection(Container);
        foreach (var c in Interacted_Containers)
        {
            if (c != null) c.SaveTempContents();
        }
    }

    public bool Compare(GISItem sexnut, bool usebase = false)
    {
        /* returns:
         * false - not the same
         * true - are the same
         */
        bool comp = false;

        if (ItemIndex == sexnut.ItemIndex) comp = true;
        if (!usebase && !comp)
        {
            //code to further compare goes here
        }
        return comp;
    }

    public void AddConnection(GISContainer gis)
    {
        if (!Interacted_Containers.Contains(gis))
        {
            Interacted_Containers.Add(gis);
        }
    }
    public void SetContainer(GISContainer gis)
    {
        if (gis == null)
        {
            Container = gis;
        }
    }

    public Dictionary<string, string> Data = new Dictionary<string, string>();

    public Dictionary<string, string> GetDefaultData()
    {
        var e = new Dictionary<string, string>()
        {
            { "Index", "Empty" },
            { "Count", "0" },
        };
        return e;
    }


    public string ItemToString()
    {
        string e = "";
        var def = GetDefaultData();

        Data["Index"] = ItemIndex.ToString();
        Data["Count"] = Amount.ToString();



        Dictionary<string, string> bb = new Dictionary<string, string>();
        foreach (var dat in Data)
        {
            if (!def.ContainsKey(dat.Key) || dat.Value != def[dat.Key])
            {
                bb.Add(dat.Key, dat.Value);
            }
        }
        if (bb.ContainsKey("Count") && bb["Count"] == "1") bb.Remove("Count");

        e = Converter.DictionaryToString(bb, "~|~", "~o~");

        return e;
    }
    public void StringToItem(string e)
    {
        setdefaultvals();

        var wanker = Converter.StringToDictionary(e, "~|~", "~o~");
        foreach (var ae in wanker)
        {
            if (Data.ContainsKey(ae.Key))
            {
                Data[ae.Key] = ae.Value;
            }
            else
            {
                Data.Add(ae.Key, ae.Value);
            }
        }

        ItemIndex = Data["Index"];
        if (wanker.ContainsKey("Count"))
        {
            Amount = int.Parse(Data["Count"]);
        }
        else
        {
            Amount = ItemIndex != "Empty" ? 1 : 0;
        }


    }

}

[Serializable]
public class GISItem_Data
{
    //this is what holds all of the base data for a general item of it's type.
    //EX: All "coal" items refer back to this for things like icon and name
    public string Name;
    public Sprite Sprite;
    public string Description;
    public int MaxAmount;
    public GISItem_Data()
    {
        Sprite = null;
        Name = "Void";
        Description = "Nothing";
        MaxAmount = 0;
    }
    public GISItem_Data(GISItem_Data data)
    {
        Sprite = data.Sprite;
        Name = data.Name;
        Description = data.Description;
        MaxAmount = data.MaxAmount;
    }
}