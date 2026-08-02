using System.Collections.Generic;
using UnityEngine;

public class GISContainerComplex : GISContainer
{
    public int CtrlClickPriority = 0;
    public bool CanDragDistributeItems = true;
    public bool CanShiftClickItems = true;
    public bool CanMassShiftClickItems = true;
    public bool CanCtrlClickItems = true;
    public bool CanDoubleClickItems = true;
    public bool CanSortItems = true;
    public bool CanTypeForStackSize = true;
    public bool CanTypeForMove = true;
    public bool AutomaticallyAddChildren = true;
    public bool GenerateRandomItems = false;
    public bool GenerateSlotObjects = true;
    public int GenerateXSlots = 20;
    public GameObject SlotPrefab;
    public List<GISSlot> slots = new List<GISSlot>();

    public override void StartCode()
    {
        GISLol.Instance.All_ComplexContainers.Add(Name, this);
        var myass = GetComponentsInChildren<GISSlot>();
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
    public override bool CanRandomGen => true;
    public override void RunRandomGen()
    {
        if (!GenerateRandomItems) return;
        //this is some debug shit for creating a bunch of randomly generated new containers.
        foreach (var s in slots)
        {
            s.Held_Item = new GISItem(GISLol.Instance.ItemDefs.Items.RandomElement().Name);
            s.Held_Item.Amount = new(69);
            s.Held_Item.Container = this;
            s.Held_Item.AnimOverride = 1;
            if (s.Held_Item.Name == "Empty")
            {
                s.Held_Item.Amount = new(0);
            }
        }
        SaveTempContents();
    }

    public void GenerateSlots(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var h = Instantiate(SlotPrefab, transform.position, transform.rotation, transform);
            var h2 = h.GetComponent<GISSlot>();
            h2._SetConte(this);
            h2.Held_Item = new GISItem();
            h2.Held_Item.AnimOverride = 1;
            slots.Add(h2);
        }
    }

    public override int AmountOf(GISItem item, bool usebase = false)
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

    public override int AmountOf(string name)
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

    public override int TotalAmountOfItems()
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

    public override GISItem Add(GISItem item)
    {
        foreach (var st in slots)
        {
            var i = st.Held_Item;
            if (i.IsEmpty())
            {
                st.Held_Item = item;
                return null;
            }
            if (i.Compare(item))
            {
                int max = GISLol.Instance.ItemDict[item.Name].MaxAmount;
                if (max > 0)
                {
                    int ideal = i.Amount + item.Amount;
                    if (ideal > max)
                    {
                        int overflow = ideal - max;
                        i.Amount.SetValue(max);
                        item.Amount.SetValue(overflow);
                        continue;
                    }
                    else
                    {
                        i.Amount.SetValue(ideal);
                        return null;
                    }
                }
                else
                {
                    i.Amount.SetValue(i.Amount + item.Amount);
                    return null;
                }
            }
        }
        if (item.Amount > 0)
        {
            return item;
        }

        return null;
    }

    public override void Clear()
    {
        foreach (var ns in slots)
        {
            ns.Held_Item = new GISItem();
            ns.OnInteract();
        }
    }

    public override void Clear(GISItem diedie, bool usebase = true)
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

    public override void Clear(string name)
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

    public override void LoadTempContents()
    {
        int i = 0;
        foreach (var h in saved_items)
        {
            slots[i].Held_Item = new GISItem(h);
            slots[i].Held_Item.AnimOverride = 1;
            slots[i].ResetTypeStack();
            i++;
        }
        if (GISLol.Instance.Mouse_Held_Item.Container == this) GISLol.Instance.Mouse_Held_Item = new GISItem();
    }

    public override void Remove(GISItem diedie, int amount, bool usebase = true)
    {
        int im = amount;
        for (int i = 0; i < slots.Count; i++)
        {
            var it = slots[i].Held_Item;
            if (it.IsEmpty()) continue;
            if (!it.Compare(diedie, usebase)) continue;
            if (it.Amount > amount)
            {
                it.Amount.SetValue(it.Amount - amount);
                slots[i].OnInteract();
                return;
            }
            else
            {
                amount -= it.Amount;
                slots[i].Held_Item = new GISItem();
                slots[i].OnInteract();
            }
            if (amount <= 0) return;
        }
        if (amount > 0)
        {
            $"Attemped removing {im}, had {amount} remaining".DLogError();
        }
    }

    public override void Remove(string name, int amount)
    {
        int im = amount;
        for (int i = 0; i < slots.Count; i++)
        {
            var it = slots[i].Held_Item;
            if (it.IsEmpty()) continue;
            if (it.Name != name) continue;
            if (it.Amount > amount)
            {
                it.Amount.SetValue(it.Amount - amount);
                slots[i].OnInteract();
                return;
            }
            else
            {
                amount -= it.Amount;
                slots[i].Held_Item = new GISItem();
                slots[i].OnInteract();
            }
            if (amount <= 0) return;
        }
        if (amount > 0)
        {
            $"Attemped removing {im}, had {amount} remaining".DLogError();
        }
    }

    public override void SaveContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            GISLol.Instance.LoadTempForAll();
            dict.SetList(GetName(), slots.AToB((x) => x.Held_Item));
        }
    }

    public override void LoadContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            var gg = dict.GetList(GetName(), new List<GISItem>());
            if (gg.Count > 0)
            {
                int i = 0;
                foreach (var ghj in gg)
                {
                    ghj.Container = this;
                    ghj.AnimOverride = 1;
                    slots[i].Held_Item = ghj;
                    i++;
                    if (i >= slots.Count) break;
                }

            }
            SaveTempContents();
        }
    }

    public override void Sort(SortingMethod prefered_sort, bool reversed)
    {
        List<GISSlot> interacts = new();
        List<GISItem> items = new();
        foreach (GISSlot slot in slots)
        {
            if (!slot.Held_Item.IsEmpty())
                interacts.Add(slot);
            items.Add(slot.Held_Item);
            slot.Held_Item = new GISItem();
        }

        _m = prefered_sort;
        items.Sort(CompareGISItems);

        for (int i = 0; i < slots.Count; i++)
        {
            int j = i;
            if (!reversed) j = (slots.Count - 1) - i;
            slots[j].Held_Item = items[i];
            interacts.AddIfUnique(slots[j]);
        }
        foreach (GISSlot slot in interacts)
        {
            slot.OnInteract();
        }
    }

    public override int IndexOf(GISItem item, bool truecompare = false)
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

    public override int IndexOf(string name)
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

    public override bool SaveTempContents()
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

    public override void UpdateCode()
    {
        foreach (var s in slots)
        {
            s.UpdateCall();
        }
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
    public void ClearSlotObjects()
    {
        foreach (var ns in slots)
        {
            if (ns != null && ns.gameObject != null)
                Destroy(ns.gameObject);
        }
        slots.Clear();
    }
}
