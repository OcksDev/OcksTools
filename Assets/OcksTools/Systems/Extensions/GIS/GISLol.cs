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

    public Dictionary<string, Func<string, GISItemComponentBase>> ComponentTransformers = new Dictionary<string, Func<string, GISItemComponentBase>>();
    public Dictionary<string, string> ClassToIdentifier = new Dictionary<string, string>();

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
     * GISItem.setdefaultvals()
     * GISItem.GISItem(GISItem)
     * GISItem.Compare(GISItem)
     */

    public string Name;
    public int Amount;
    public GISContainer Container;
    public List<GISContainer> Interacted_Containers = new List<GISContainer>();
    public Dictionary<string, GISItemComponentBase> Components = new Dictionary<string, GISItemComponentBase>();
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
        bool comp = Name == sexnut.Name;

        if (!usebase && !comp)
        {
            if (sexnut.Components.Count != Components.Count) comp = false;
            else
            {
                foreach (var c in Components)
                {
                    if (!sexnut.Components.ContainsKey(c.Key))
                    {
                        comp = false;
                        break;
                    }
                    else
                    {
                        if (!c.Value.Compare(sexnut.Components[c.Key]))
                        {
                            comp = false;
                            break;
                        }
                    }
                }
            }
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

        //cursed ass line of code lol
        Data["Extra"] = Components.ABDictionaryToCDDictionary((x) => x, (x) => x.GetString()).EscapedDictionaryToString();

        Dictionary<string, string> bb = def.MergeDictionary(Data);

        if (bb["Index"] == "Empty") bb = new Dictionary<string, string>(); //no need to store this, saves a large amount of space
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
        Amount = int.Parse(Data["Count"]);
        Components = Data["Extra"].EscapedStringToDictionary().ABDictionaryToCDDictionary((x, y) => x, (x, y) => ItemDataConvert(x, y));
        return this;
    }
    public static GISItemComponentBase ItemDataConvert(string wish, string data)
    {
        if (GISLol.Instance.ComponentTransformers.ContainsKey(wish))
        {
            return GISLol.Instance.ComponentTransformers[wish](data);
        }
        throw new Exception($"No item data conversion defined for {wish}");
    }
    public void AddComponent(GISItemComponentBase cum)
    {
        Components.Add(cum.GetIdentifier(), cum);
    }
    public T GetComponent<T>() where T : GISItemComponentBase
    {
        return (T)Components[GISLol.Instance.ClassToIdentifier[typeof(T).Name]];
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

/// <summary>
/// Components have to be initialized in order to register their FromString functions.
/// </summary>
public abstract class GISItemComponentBase
{
    /// <summary>
    /// Unique identifier for this component type. No two components should share the same identifier.
    /// </summary>
    public abstract string GetIdentifier();
    /// <summary>
    /// Converts the current component instance into its string representation for serialization.
    /// </summary>
    public abstract string GetString();

    /// <summary>
    /// Creates a new instance by parsing the specified string representation.
    /// IT DOES NOT set any values on the current instance!
    /// </summary>
    public abstract GISItemComponentBase FromString(string data);

    /// <summary>
    /// Determines if this component is equal to another component.
    /// </summary>
    public bool Compare(GISItemComponentBase data)
    {
        if (GetIdentifier() != data.GetIdentifier()) return false;
        return Compare2(data);
    }
    public abstract bool Compare2(GISItemComponentBase data);
}
/// <summary>
/// Components have to be initialized in order to register their FromString functions.
/// </summary>
public abstract class GISItemComponent<T> : GISItemComponentBase where T : GISItemComponent<T>
{
    public void Init()
    {
        GISLol.Instance.ComponentTransformers.Add(GetIdentifier(), FromString);
        GISLol.Instance.ClassToIdentifier.Add(typeof(T).Name, GetIdentifier());
    }
    /// <summary>
    /// Determines if this component is equal to another component.
    /// </summary>
    public override bool Compare2(GISItemComponentBase data)
    {
        return EqualsSpecific((T)data);
    }
    /// <summary>
    /// Determines if this component is specifically equal to another component of the same type.
    /// </summary>
    public abstract bool EqualsSpecific(T data);
}
