using System;
using System.Collections.Generic;
using UnityEngine;

public class GISLol : SingleInstance<GISLol>
{
    public bool UseLanguageFile = true;
    public GISItem Mouse_Held_Item;
    public GISDisplay Mouse_Displayer;
    public GameObject MouseFollower;
    public List<GISItem_Data> Items = new List<GISItem_Data>();
    public Dictionary<string, GISItem_Data> ItemDict = new Dictionary<string, GISItem_Data>();
    public Dictionary<string, GISContainer> All_Containers = new Dictionary<string, GISContainer>();

    private bool nono = false;

    public void LoadTempForAll()
    {
        if (nono) return;
        nono = true;
        foreach (var con in All_Containers)
        {
            if (con.Value != null && !con.Value.IsAbstract) con.Value.LoadTempContents();
        }
    }

    public override void Awake2()
    {
        Mouse_Held_Item = new GISItem();
        foreach (var item in Items)
        {
            ItemDict.Add(item.Name, item);
        }
        SaveSystem.SaveAllData.Append(SaveAll);

        InputManager.CollectInputAllocs.Append("GIS", () =>
        {
            InputManager.CreateKeyAllocation("item_select", KeyCode.Mouse0);
            InputManager.CreateKeyAllocation("item_half", KeyCode.Mouse1);
            InputManager.CreateKeyAllocation("item_pick", KeyCode.Mouse2);
            InputManager.CreateKeyAllocation("item_alt", KeyCode.LeftShift);
            InputManager.CreateKeyAllocation("item_mod", KeyCode.LeftControl);
            InputManager.CreateKeyAllocation("item_change", KeyCode.LeftAlt);
        });
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
            foreach (var a in l.GetDict("Items"))
            {
                ItemDict[a.Key].SetLangData(a.Value);
                //Debug.Log(ItemDict[a.Key].DisplayName + ": " + ItemDict[a.Key].Description);
            }
        }
    }
    [HideInInspector]
    public int DragLock = 0;
    [HideInInspector]
    public int SameFixedStop = 0;
    [HideInInspector]
    public int SameFrameStop = 0;
    [HideInInspector]
    public Reactable<bool> MouseLeftClicking = new(false);
    [HideInInspector]
    public Reactable<bool> MouseRightClicking = new(false);
    [HideInInspector]
    public Reactable<bool> MouseLeftClickingDown = new(false);
    [HideInInspector]
    public Reactable<bool> MouseRightClickingDown = new(false);
    [HideInInspector]
    public List<GISSlot> DragSlotsLeft = new();
    [HideInInspector]
    public List<GISSlot> DragSlotsRight = new();
    [HideInInspector]
    public GISItem DragItemLeft = null;
    private void FixedUpdate()
    {
        if (SameFixedStop > 0) SameFixedStop--;
    }

    private void Update()
    {
        if (SameFrameStop > 0) SameFrameStop--;
        Mouse_Displayer.item.SetValue(Mouse_Held_Item);
        var za = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        za.z = 0;
        MouseFollower.transform.position = za;
#if UNITY_EDITOR
        if (InputManager.IsKeyDown("reload"))
        {
            LoadTempForAll();
        }
#endif
        MouseLeftClicking.SetValue(DragLock != 2 && InputManager.IsKey("shoot"));
        MouseRightClicking.SetValue(DragLock != 1 && InputManager.IsKey("alt_shoot"));
        MouseLeftClickingDown.SetValue(DragLock != 2 && InputManager.IsKeyDown("shoot"));
        MouseRightClickingDown.SetValue(DragLock != 1 && InputManager.IsKeyDown("alt_shoot"));
        if (MouseLeftClicking.HasChanged())
        {
            if (MouseLeftClicking)
            {
                //active?
                DragLock = 1;
            }
            else
            {
                DragSlotsLeft.Clear();
                DragLock = 0;
            }
        }
        if (MouseRightClicking.HasChanged())
        {
            if (MouseRightClicking)
            {
                //active?
                DragLock = 2;
            }
            else
            {
                DragSlotsRight.Clear();
                DragLock = 0;
            }
        }


        nono = false;
    }

    public void SaveAll(SaveProfile dict)
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
     * GISItem.setdefaultvals()
     * GISItem.GISItem(GISItem)
     * GISItem.Compare(GISItem)
     */

    public string Name;
    public Reactable<int> Amount;
    public GISContainer Container;
    public List<GISContainer> Interacted_Containers = new List<GISContainer>();
    public ComponentHolder Components = new ComponentHolder();
    public GISItem()
    {
        setdefaultvals();
    }
    public GISItem(string base_type)
    {
        setdefaultvals();
        Amount = new(1);
        Name = base_type;
    }
    public GISItem(GISItem sexnut)
    {
        setdefaultvals();
        Amount = new(sexnut.Amount);
        Name = sexnut.Name;
        Container = sexnut.Container;
        Components = new ComponentHolder(sexnut.Components);
    }
    public bool IsEmpty()
    {
        return Name == "Empty";
    }
    private void setdefaultvals()
    {
        Data = GetDefaultData();
        Amount = new(0);
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
        if (IsEmpty() || sexnut.IsEmpty()) return false;

        bool comp = Name == sexnut.Name;

        if (!usebase && comp)
        {
            comp = Components.Compare(sexnut.Components);
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
            { "Count", "1" },
            { "Extra", "" },
        };
        return e;
    }


    public override string ToString()
    {
        if (this == null) return "NULL";
        string e = "";
        var def = GetDefaultData();

        Data["Index"] = Name.ToString();
        Data["Count"] = Amount.ToString();

        Data["Extra"] = Components.CompToString();

        Dictionary<string, string> bb = def.DiffDictionary(Data);

        if (!bb.ContainsKey("Index")) bb = new Dictionary<string, string>(); //no need to store this, saves a large amount of space
        if (bb.ContainsKey("Count") && bb["Count"] == "1") bb.Remove("Count"); //no need to store this, saves a minimal amount of space
        e = Converter.EscapedDictionaryToString(bb, "~|~", "~o~");
        return e;
    }
    [ConversionMethod]
    public GISItem ConvertToItem(string e)
    {
        var a = new GISItem();
        a.StringToItem(e);
        return a;
    }

    public GISItem StringToItem(string e)
    {
        setdefaultvals();
        Data = Data.MergeDictionary(Converter.EscapedStringToDictionary(e, "~|~", "~o~"));

        Name = Data["Index"];
        if (!IsEmpty()) Amount = new(int.Parse(Data["Count"]));
        else Amount = new(0);

        Components.CompFromString(Data["Extra"]);

        return this;
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
