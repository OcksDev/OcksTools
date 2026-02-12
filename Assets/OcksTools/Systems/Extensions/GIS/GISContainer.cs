using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GISContainer : MonoBehaviour
{
    public string Name = "RandomThingIDK";
    public bool SaveLoadData = true;
    [Tooltip("Uses the GISItems however doesn't use slots")]
    public bool IsAbstract = false;
    [HideIf("IsAbstract")]
    public bool CanDragDistributeItems = true;
    [HideIf("IsAbstract")]
    public bool CanShiftClickItems = true;
    [HideIf("IsAbstract")]
    public bool CanMassShiftClickItems = true;
    [HideIf("IsAbstract")]
    public bool CanCtrlClickItems = true;
    [HideIf("IsAbstract")]
    public bool CanDoubleClickItems = true;
    public int CtrlClickPriority = 0;
    [HideIf("IsAbstract")]
    public bool AutomaticallyAddChildren = true;
    public bool GenerateRandomItems = false;
    [HideIf("IsAbstract")]
    public bool GenerateSlotObjects = true;
    [HideIf("IsAbstract")]
    [ShowIf("GenerateSlotObjects")]
    public int GenerateXSlots = 20;
    [HideIf("IsAbstract")]
    public GameObject SlotPrefab;
    public List<GISSlot> slots = new List<GISSlot>();
    [HideInInspector]
    public List<GISSlot> extraslots = new List<GISSlot>();
    [HideInInspector]
    public bool LoadedData = false;

    public List<GISItem> saved_items = new List<GISItem>();
    // Start is called before the first frame update
    private void Start()
    {
        var myass = GetComponentsInChildren<GISSlot>();
        GISLol.Instance.All_Containers.Add(Name, this);
        if (!IsAbstract)
        {
            if (GenerateSlotObjects)
            {
                if (AutomaticallyAddChildren)
                    foreach (var pp in myass)
                    {
                        Destroy(pp.gameObject);
                    }

                GenerateSlots(GenerateXSlots);
            }
            else if (AutomaticallyAddChildren)
            {
                foreach (var pp in myass)
                {
                    pp._SetConte(this);
                    if (!slots.Contains(pp)) slots.Add(pp);
                }
            }
        }
        else
        {
            slots.Clear();
        }



        if (SaveLoadData)
        {
            StartCoroutine(WaitForSaveSystem());
        }
        else if (GenerateRandomItems)
        {
            //this is some debug shit for creating a bunch of randomly generated new containers.
            foreach (var s in slots)
            {
                s.Held_Item = new GISItem(GISLol.Instance.Items[UnityEngine.Random.Range(0, GISLol.Instance.Items.Count)].Name);
                s.Held_Item.Amount = new(69);
                s.Held_Item.Container = this;
                if (s.Held_Item.Name == "Empty")
                {
                    s.Held_Item.Amount = new(0);
                }
            }
        }
    }
    public IEnumerator WaitForSaveSystem()
    {
        yield return new WaitUntil(() => { return SaveSystem.Instance.LoadedData; });
        LoadContents(SaveSystem.ActiveProf);
        LoadedData = true;
    }
    public void Update()
    {
        if (IsAbstract)
        {
            foreach (var s in extraslots)
            {
                s.UpdateCall();
            }
            return;
        }
        foreach (var s in slots)
        {
            s.UpdateCall();
        }
        foreach (var s in extraslots)
        {
            s.UpdateCall();
        }
    }


    public bool SaveTempContents()
    {
        if (GISLol.Instance.Mouse_Held_Item.Name == "Empty")
        {
            saved_items.Clear();
            foreach (var h in slots)
            {
                saved_items.Add(new GISItem(h.Held_Item));
            }
            return true;
        }
        return false;
    }
    public void LoadTempContents()
    {
        int i = 0;
        if (IsAbstract)
        {
            slots.Clear();
            foreach (var h in saved_items)
            {
                var pp = new GISSlot();
                pp._SetConte(this);
                pp.Held_Item = new GISItem(h);
                slots.Add(pp);
            }
        }
        else
        {
            foreach (var h in saved_items)
            {
                slots[i].Held_Item = new GISItem(h);
                i++;
            }
        }
        if (GISLol.Instance.Mouse_Held_Item.Container == this) GISLol.Instance.Mouse_Held_Item = new GISItem();
    }
    public int FindEmptySlot()
    {
        int i = -1;
        int k = 0;
        foreach (var j in slots)
        {
            if (j.Held_Item.Name == "Empty")
            {
                i = k;
                break;
            }
            k++;
        }

        return i;
    }

    public void SaveContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            GISLol.Instance.LoadTempForAll();
            dict.SetList(GetName(), slots.AListToBList((x) => x.Held_Item));
        }
    }

    public string GetName()
    {
        return "cnt_" + Name;
    }

    private void Awake()
    {
        if (SaveLoadData)
        {
            SaveSystem.SaveAllData.Append($"{GetName()}_save", SaveContents);
        }
    }
    public void LoadContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            if (IsAbstract)
            {
                slots.Clear();
            }
            List<string> a = new List<string>();
            List<string> b = new List<string>();
            var gg = dict.GetList(GetName(), new List<GISItem>());
            if (gg.Count > 0)
            {
                int i = 0;
                foreach (var ghj in gg)
                {
                    ghj.Container = this;

                    if (IsAbstract)
                    {
                        AbstractAdd(ghj);
                    }
                    else
                    {
                        slots[i].Held_Item = ghj;
                    }
                    i++;
                    if (!IsAbstract && i >= slots.Count) break;
                }

            }
            SaveTempContents();
        }
    }

    public int AmountOf(GISItem item, bool usebase = false)
    {
        int amnt = 0;

        foreach (var st in slots)
        {
            if (st.Held_Item.Compare(item, usebase))
            {
                amnt += st.Held_Item.Amount;
            }
        }

        return amnt;
    }

    public int AmountOf(string name)
    {
        int amnt = 0;

        foreach (var st in slots)
        {
            if (st.Held_Item.Name == name)
            {
                amnt += st.Held_Item.Amount;
            }
        }

        return amnt;
    }

    public int TotalAmountOfItems()
    {
        int total = 0;
        foreach (var t in slots)
        {
            if (t.Held_Item != null && t.Held_Item.Name != "Empty")
            {
                total += t.Held_Item.Amount;
            }
        }
        return total;
    }

    public bool ReturnItem(GISItem Held_Item)
    {
        bool left = true;
        var a = new GISItem(Held_Item);
        int i = left ? 0 : slots.Count - 1;
        bool found = false;
        foreach (var item in slots)
        {
            var x = slots[i].Held_Item;
            if (x.Compare(a))
            {
                int max = GISLol.Instance.ItemDict[a.Name].MaxAmount;
                int t = x.Amount + a.Amount;
                if (max <= 0)
                {
                    x.Amount.SetValue(t);
                    found = true;
                    break;
                }
                else
                {
                    int z = Mathf.Clamp(t, 0, max);
                    x.Amount.SetValue(z);
                    a.Amount.SetValue(t - z);
                    if (a.Amount == 0)
                    {
                        found = true;
                        break;
                    }
                }
            }
            else if (x.Name == "Empty")
            {
                found = true;
                slots[i].Held_Item = a;
                break;
            }
            if (!found)
            {
                i += left ? 1 : -1;
            }
        }
        if (found)
        {
            slots[i].Held_Item.AddConnection(this);
            slots[i].Held_Item.Solidify();
        }
        return found;
    }
    //any method prefixed with "Abstract" should only be used if the container is abstract.
    public void AbstractAdd(GISItem item)
    {
        var f = GISLol.Instance.ItemDict[item.Name].MaxAmount;
        foreach (var s in slots)
        {
            if (item.Compare(s.Held_Item))
            {
                int z = s.Held_Item.Amount;
                z += item.Amount;
                if (f > 0)
                {
                    s.Held_Item.Amount.SetValue(Math.Clamp(z, 0, f));
                    if (z > f)
                    {
                        z -= f;
                    }
                    else
                    {
                        z = 0;
                        item.Amount.SetValue(z);
                        break;
                    }
                    item.Amount.SetValue(z);
                }
                else
                {
                    s.Held_Item.Amount.SetValue(z);
                    z = 0;
                    item.Amount.SetValue(z);
                    break;
                }
            }
        }
        if (item.Amount > 0)
        {
            if (f > 0)
            {
                while (item.Amount > f)
                {
                    //Debug.Log("sex: " +item.Amount);
                    var ns = new GISSlot();
                    ns._SetConte(this);
                    var pp = new GISItem(item);
                    pp.Amount.SetValue(f);
                    ns.Held_Item = pp;
                    slots.Add(ns);
                    item.Amount.SetValue(item.Amount - f);
                }
                if (item.Amount > 0)
                {
                    var ns = new GISSlot();
                    ns._SetConte(this);
                    ns.Held_Item = item;
                    slots.Add(ns);
                }
            }
            else
            {
                var ns = new GISSlot();
                ns._SetConte(this);
                ns.Held_Item = item;
                slots.Add(ns);
            }
        }
    }
    public void ClearSlotObjects()
    {
        foreach (var ns in slots)
        {
            if (ns != null && ns.gameObject != null)
                Destroy(ns.gameObject);
        }
        slots.Clear();
    }
    public void Clear()
    {
        foreach (var ns in slots)
        {
            ns.Held_Item = new GISItem();
            ns.OnInteract();
        }
    }
    public void Clear(GISItem diedie, bool usebase = true)
    {
        foreach (var ns in slots)
        {
            if (ns.Held_Item.Compare(diedie, usebase))
            {
                ns.Held_Item = new GISItem();
                ns.OnInteract();
            }
        }
    }
    public void Clear(string name)
    {
        foreach (var ns in slots)
        {
            if (ns.Held_Item.Name == name)
            {
                ns.Held_Item = new GISItem();
                ns.OnInteract();
            }
        }
    }
    public void GenerateSlots(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var h = Instantiate(SlotPrefab, transform.position, transform.rotation, transform);
            var h2 = h.GetComponent<GISSlot>();
            h2._SetConte(this);
            h2.Held_Item = new GISItem();
            slots.Add(h2);
        }
    }
    public void RegenerateAndLoadSlots(int amount)
    {
        ClearSlotObjects();
        GenerateSlots(amount);
        LoadContents(SaveSystem.ActiveProf);
    }

    public int IndexOf(GISItem item, bool truecompare = false)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (truecompare ? item.Compare(slots[i].Held_Item) : slots[i].Held_Item == item)
            {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf(string name)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].Held_Item.Name == name)
            {
                return i;
            }
        }
        return -1;
    }

}
