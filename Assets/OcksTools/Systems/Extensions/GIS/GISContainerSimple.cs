
using System.Collections.Generic;

public class GISContainerSimple : GISContainer
{
    public override bool IsAbstract => true;
    public List<GISItem> items = new List<GISItem>();
    public OXEvent OnContentsChanged = new();

    public override void StartCode() { }
    public override void AbstractAdd(GISItem item, bool ignore_anim = false)
    {
        var remained = Add(item);
        if (remained != null)
        {
            var pp = remained;
            pp.AnimOverride = (byte)(ignore_anim ? 1 : 0);
            items.Add(pp);
        }
        OnContentsChanged.Invoke();
    }
    public void AbstractCollapseEmpty()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null || items[i].IsEmpty())
            {
                items.RemoveAt(i);
                i--;
            }
        }
        OnContentsChanged.Invoke();
    }

    public override int AmountOf(GISItem item, bool usebase = false)
    {
        int amnt = 0;

        foreach (var st in items)
        {
            if (st.Compare(item, usebase))
            {
                amnt += st.Amount;
            }
        }
        return amnt;
    }

    public override int AmountOf(string name)
    {
        int amnt = 0;

        foreach (var st in items)
        {
            if (st.Name == name)
            {
                amnt += st.Amount;
            }
        }

        return amnt;
    }

    public override int TotalAmountOfItems()
    {
        int total = 0;
        foreach (var t in items)
        {
            if (t != null && t.Name != "Empty")
            {
                total += t.Amount;
            }
        }
        return total;
    }

    public override GISItem Add(GISItem item)
    {
        foreach (var st in items)
        {
            if (st.Compare(item))
            {
                int max = GISLol.Instance.ItemDict[item.Name].MaxAmount;
                if (max > 0)
                {
                    int ideal = st.Amount + item.Amount;
                    if (ideal > max)
                    {
                        int overflow = ideal - max;
                        st.Amount.SetValue(max);
                        item.Amount.SetValue(overflow);
                        continue;
                    }
                    else
                    {
                        st.Amount.SetValue(ideal);
                        return null;
                    }
                }
                else
                {
                    st.Amount.SetValue(st.Amount + item.Amount);
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
        items.Clear();
        OnContentsChanged.Invoke();
    }

    public override void Clear(GISItem diedie, bool usebase = true)
    {
        foreach (var ns in items)
        {
            if (ns.Compare(diedie, usebase))
            {
                ns.SetEmpty();
            }
        }
        AbstractCollapseEmpty();
    }

    public override void Clear(string name)
    {
        foreach (var ns in items)
        {
            if (ns.Name == name)
            {
                ns.SetEmpty();
            }
        }
        AbstractCollapseEmpty();
    }

    public override void LoadTempContents()
    {
        items.Clear();
        foreach (var h in saved_items)
        {
            var pp = new GISItem(h);
            pp.AnimOverride = 1;
            items.Add(pp);
        }
        if (GISLol.Instance.Mouse_Held_Item.Container == this) GISLol.Instance.Mouse_Held_Item = new GISItem();
        OnContentsChanged.Invoke();
    }

    public override void Remove(GISItem diedie, int amount, bool usebase = true)
    {
        int im = amount;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it.IsEmpty()) continue;
            if (!it.Compare(diedie, usebase)) continue;
            if (it.Amount > amount)
            {
                it.Amount.SetValue(it.Amount - amount);
                return;
            }
            else
            {
                amount -= it.Amount;
                it.SetEmpty();
            }
            if (amount <= 0) return;
        }
        if (amount > 0)
        {
            $"Attemped removing {im}, had {amount} remaining".DLogError();
        }
        AbstractCollapseEmpty();
    }

    public override void Remove(string name, int amount)
    {
        int im = amount;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it.IsEmpty()) continue;
            if (it.Name != name) continue;
            if (it.Amount > amount)
            {
                it.Amount.SetValue(it.Amount - amount);
                return;
            }
            else
            {
                amount -= it.Amount;
                it.SetEmpty();
            }
            if (amount <= 0) return;
        }
        if (amount > 0)
        {
            $"Attemped removing {im}, had {amount} remaining".DLogError();
        }
        AbstractCollapseEmpty();
    }

    public override void SaveContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            GISLol.Instance.LoadTempForAll();
            dict.SetList(GetName(), items);
        }
    }

    public override void LoadContents(SaveProfile dict)
    {
        if (SaveLoadData)
        {
            items = dict.GetList(GetName(), new List<GISItem>());
            foreach (GISItem it in items)
            {
                it.Container = this;
                it.SetContainer(this);
            }
            SaveTempContents();
            OnContentsChanged.Invoke();
        }
    }

    public override void Sort(SortingMethod prefered_sort, bool reversed)
    {
        _m = prefered_sort;
        items.Sort(CompareGISItems);
        if (reversed) items.Reverse();
    }

    public override int IndexOf(GISItem item, bool truecompare = false)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (truecompare ? item.Compare(items[i]) : items[i] == item)
            {
                return i;
            }
        }
        return -1;
    }

    public override int IndexOf(string name)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Name == name)
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
            foreach (var h in items)
            {
                saved_items.Add(new GISItem(h));
            }
            return true;
        }
        return false;
    }
    public override void UpdateCode() { }
}
