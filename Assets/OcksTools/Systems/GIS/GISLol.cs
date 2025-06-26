using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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
    bool nono = false;
    public void LoadTempForAll()
    {
        if (nono) return;
        nono = true;
        foreach(var con in All_Containers)
        {
            if (con.Value != null && !con.Value.IsAbstract) con.Value.LoadTempContents();
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
        SaveSystem.SaveAllData.Append(SaveAll);
    }


    private void Start()
    {
        MouseFollower.SetActive(true);

        if (UseLanguageFile)
        {
            var l = LanguageFileSystem.Instance;
            var file = new OXLanguageFileIndex();
            file.FileName = "Items";
            Dictionary<string, string> dat = new Dictionary<string, string>();
            foreach (var item in Items)
            {
                dat.Add(item.Name, item.GetLangData());
            }
            file.DefaultString = Converter.EscapedDictionaryToString(dat, Environment.NewLine, ": ");
            l.ReadFile(file);
            foreach(var a in l.GetDict("Items"))
            {
                ItemDict[a.Key].SetLangData(a.Value);
                Debug.Log(ItemDict[a.Key].DisplayName + ": " + ItemDict[a.Key].Description);
            }
        }
    }

    private void Update()
    {
        Mouse_Displayer.item = Mouse_Held_Item;
        var za = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        za.z = 0;
        MouseFollower.transform.position = za;
        if (InputManager.IsKeyDown("reload"))
        {
            LoadTempForAll();
        }
        if (InputManager.IsKeyDown(KeyCode.X))
        {
            Mouse_Held_Item = new GISItem();
        }
        nono = false;
    }

    public void SaveAll(string dict)
    {
        foreach (var c in All_Containers)
        {
            if (c.Value != null && c.Value.SaveLoadData)
            {
                c.Value.SaveContents(dict);
            }
        }
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

    public string Name;
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
        Amount = 1;
        Name = base_type;
    }
    public GISItem(GISItem sexnut)
    {
        setdefaultvals();
        Amount = sexnut.Amount;
        Name = sexnut.Name;
        Container = sexnut.Container;
    }
    private void setdefaultvals()
    {
        Data = GetDefaultData();
        Amount = 0;
        Name = "Empty";
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

        if (Name == sexnut.Name) comp = true;
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

        Data["Index"] = Name.ToString();
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

        Name = Data["Index"];
        if (wanker.ContainsKey("Count"))
        {
            Amount = int.Parse(Data["Count"]);
        }
        else
        {
            Amount = Name != "Empty" ? 1 : 0;
        }


    }

}

[Serializable]
public class GISItem_Data
{
    //this is what holds all of the base data for a general item of it's type.
    //EX: All "coal" items refer back to this for things like icon and name
    public string Name;
    public string DisplayName;
    public Sprite Sprite;
    public string Description;
    public int MaxAmount;
    public ItemType Type = ItemType.None;
    public GISItem_Data()
    {
        Sprite = null;
        Name = "Void";
        Description = "Nothing";
        MaxAmount = 0;
        Type = ItemType.None;
    }
    public GISItem_Data(GISItem_Data data)
    {
        Sprite = data.Sprite;
        Name = data.Name;
        Description = data.Description;
        MaxAmount = data.MaxAmount;
    }
    public enum ItemType
    {
        None,
        Weapon,
        Food,
        CraftingMaterial,
        Ammo,
        Gem,
        Key,
    }
    public string GetLangData()
    {
        List<string> d = new List<string>() 
        { 
            DisplayName,
            Description,
        };

        return Converter.EscapedListToString(d, "<->");
    }
    public void SetLangData(string d)
    {
        var dat = Converter.EscapedStringToList(d, "<->");
        DisplayName = dat[0];
        Description = dat[1];
    }
}


public class GISDisplayData
{
    public Sprite[] Images;
    public string Count;

    public GISDisplayData(GISItem gissy)
    {
        Images = new Sprite[1] { GISLol.Instance.ItemDict[gissy.Name].Sprite };
        Count = gissy.Amount > 0 ? "x" + gissy.Amount : "";
    }
}