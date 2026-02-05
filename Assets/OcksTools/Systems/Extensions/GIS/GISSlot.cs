using System.Collections.Generic;
using UnityEngine;

public class GISSlot : MonoBehaviour
{
    public string Name = "";
    public GISItem Held_Item;
    public GISDisplay Displayer;
    public GISContainer Conte;
    public string InteractFilter = "";
    private float DoubleClickTimer = -69f;
    private RectTransform erect;
    public OXEvent<GISSlot> OnInteractEvent = new OXEvent<GISSlot>();
    public OXEvent<GISSlot> OnFilterCheck = new OXEvent<GISSlot>();
    [HideInInspector]
    public int StoredAmount;
    public void Awake()
    {
        if (Held_Item == null)
        {
            Held_Item = new GISItem();
        }
    }
    private void Start()
    {
        erect = GetComponent<RectTransform>();
    }
    public bool FailToClick()
    {
        var pp = GISLol.Instance.Mouse_Held_Item;
        switch (InteractFilter)
        {
            case "TakeOnly":
                if (!pp.IsEmpty()) return true;
                break;
            case "PlaceOnly":
                if (!Held_Item.IsEmpty()) return true;
                break;
            case " ":
            case "":
                break;
            default:
                return OnFilterCheck.InvokeWithHitCheck(this);
        }
        return false;
    }

    public void OnInteract()
    {
        switch (Name)
        {
            case "AbstractAdd":
                var mitem = Held_Item;
                Conte.AbstractAdd(mitem);
                Held_Item = new GISItem();
                break;
        }
        OnInteractEvent.Invoke(this);
    }

    public void UpdateCall()
    {
        var g = GISLol.Instance;
        if (FailToClick()) return;
        if (!IsHovering()) return;
        bool left = GISLol.Instance.MouseLeftClickingDown && GISLol.Instance.DragLock != 2;
        bool right = GISLol.Instance.MouseRightClickingDown && GISLol.Instance.DragLock != 1;

        if (Conte.CanDragDistributeItems)
        {
            if (GISLol.Instance.MouseLeftClicking && !left && GISLol.Instance.DragSlotsLeft.Count > 0 && !GISLol.Instance.DragSlotsLeft.Contains(this))
            {
                DragLeft();
            }
            else if (GISLol.Instance.MouseRightClicking && !right && GISLol.Instance.DragSlotsRight.Count > 0 && !GISLol.Instance.DragSlotsRight.Contains(this))
            {
                DragRight();
            }
        }

        if (!(left || right)) return;
        bool shift = InputManager.IsKey("item_alt");
        bool ctrl = InputManager.IsKey("item_mod");
        bool alt = InputManager.IsKey("item_change");
        switch (Name)
        {
            default:
                if (Conte.CanShiftClickItems && shift)
                {
                    if (left || right)
                    {
                        if (Conte.CanMassShiftClickItems && ctrl)
                        {
                            MassShiftClick(left, false);
                        }
                        else if (Conte.CanMassShiftClickItems && alt)
                        {
                            MassShiftClick(left, true);
                        }
                        else ShiftClick(left);
                    }
                }
                else if (Conte.CanCtrlClickItems && ctrl)
                {
                    if (left || right)
                    {
                        CtrlClick(left);
                    }
                }
                else
                {
                    if (left)
                    {
                        LeftClick();
                    }
                    if (right)
                    {
                        RightClick();
                    }
                }
                break;
        }

    }
    public void DragLeft()
    {
        var g = GISLol.Instance;
        if (g.SameFixedStop > 0) return;
        if (g.SameFrameStop > 0) return;
        if (!Held_Item.IsEmpty() && !Held_Item.Compare(g.DragItemLeft)) return;
        if (Held_Item.IsEmpty())
        {
            StoredAmount = 0;
        }
        else
        {
            StoredAmount = Held_Item.Amount;
        }
        g.DragSlotsLeft.Add(this);
        if (g.DragItemLeft.Compare(g.Mouse_Held_Item) && g.DragItemLeft.Amount > 0) GISLol.Instance.Mouse_Held_Item = new GISItem();
        int rem = g.DragItemLeft.Amount;
        int max = GISLol.Instance.ItemDict[g.DragItemLeft.Name].MaxAmount;
        List<GISSlot> nerds = new List<GISSlot>(g.DragSlotsLeft);
        foreach (GISSlot slot in nerds)
        {
            slot.Held_Item = new GISItem(g.DragItemLeft);
            slot.Held_Item.Amount = 0;
        }
        while (rem > 0 && nerds.Count > 0)
        {
            rem = _DistributeToNerds(nerds, g.DragItemLeft, rem, max);
            if (rem > 0 && max > 0)
            {
                for (int i = 0; i < nerds.Count; i++)
                {
                    int j = nerds.Count - 1 - i;
                    if (nerds[j].Held_Item.Amount >= max) nerds.RemoveAt(j);
                }
            }
        }

        foreach (GISSlot slot in g.DragSlotsLeft)
        {
            if (slot.Held_Item.Amount == 0)
            {
                if (slot.StoredAmount == 0) slot.Held_Item = new GISItem();
                else slot.Held_Item.Amount = slot.StoredAmount;
            }
        }

        if (nerds.Count == 0 && rem > 0)
        {
            Debug.LogError("???"); // this means that its trying to distribute more items, but all the slots are somehow filled?
        }
        int t = 0;
        foreach (var a in g.DragSlotsLeft)
        {
            t += a.Held_Item.Amount;
        }
        if (t < g.DragItemLeft.Amount)
        {
            Debug.LogError("Items:" + (t - g.DragItemLeft.Amount));
            Debug.LogError($"{t}, {g.DragItemLeft.Amount}");
        }
    }
    public void DragRight()
    {
        var g = GISLol.Instance;
        if (g.Mouse_Held_Item.IsEmpty())
        {
            g.DragSlotsRight.Add(this);
            return;
        }
        if (g.SameFixedStop > 0) return;
        if (g.SameFrameStop > 0) return;
        if (!Held_Item.IsEmpty() && !Held_Item.Compare(g.Mouse_Held_Item)) return;
        g.DragSlotsRight.Add(this);
        int max = GISLol.Instance.ItemDict[g.Mouse_Held_Item.Name].MaxAmount;
        if (max > 0 && Held_Item.Amount >= max) return;
        if (Held_Item.IsEmpty())
        {
            Held_Item = new GISItem(g.Mouse_Held_Item);
            Held_Item.Amount = 1;
        }
        else
        {
            Held_Item.Amount++;
        }
        g.Mouse_Held_Item.Amount--;
        if (g.Mouse_Held_Item.Amount <= 0)
        {
            g.Mouse_Held_Item = new GISItem();
        }

    }

    public int _DistributeToNerds(List<GISSlot> nerds, GISItem item, int amnt, int max)
    {
        float x = amnt;
        x /= nerds.Count;
        int rem = amnt;
        if (x < 1.01f) x = 1.01f;
        foreach (var a in nerds)
        {
            if (rem <= 0) break;
            int orig = a.Held_Item.Amount > 0 ? a.Held_Item.Amount : a.StoredAmount;
            int prop = ((int)x) + orig;
            if (max > 0 && prop > max)
            {
                prop = max;
            }
            rem -= prop - orig;
            a.Held_Item.Amount = prop;
        }

        return rem;
    }


    public void CtrlClick(bool left)
    {
        var g = GISLol.Instance;
        int cc = int.MinValue;
        GISContainer nerd = null;
        if (left)
        {
            foreach (var a in g.All_Containers)
            {
                if (a.Value != Conte && a.Value.CtrlClickPriority > cc)
                {
                    if (a.Value.FindEmptySlot() >= 0)
                    {
                        cc = a.Value.CtrlClickPriority;
                        nerd = a.Value;
                    }
                }
            }
        }
        else
        {
            cc = int.MaxValue;
            foreach (var a in g.All_Containers)
            {
                if (a.Value != Conte && a.Value.CtrlClickPriority < cc)
                {
                    if (a.Value.FindEmptySlot() >= 0)
                    {
                        cc = a.Value.CtrlClickPriority;
                        nerd = a.Value;
                    }
                }
            }
        }
        if (nerd != null)
        {
            int x = nerd.FindEmptySlot();
            nerd.slots[x].Held_Item = Held_Item;
            Held_Item = new GISItem();
            nerd.SaveTempContents();
            SaveItemContainerData();
            OnInteract();
        }
    }


    public void ShiftClick(bool left)
    {
        var g = GISLol.Instance;
        Held_Item.AddConnection(Conte);
        SaveItemContainerData();
        var a = new GISItem(Held_Item);
        Held_Item = new GISItem();
        int i = left ? 0 : Conte.slots.Count - 1;
        bool found = false;
        foreach (var item in Conte.slots)
        {
            var x = Conte.slots[i].Held_Item;
            if (x.Compare(a))
            {
                int max = g.ItemDict[a.Name].MaxAmount;
                int t = x.Amount + a.Amount;
                if (max <= 0)
                {
                    x.Amount = t;
                    found = true;
                    break;
                }
                else
                {
                    int z = Mathf.Clamp(t, 0, max);
                    x.Amount = z;
                    a.Amount = t - z;
                    if (a.Amount == 0)
                    {
                        found = true;
                        break;
                    }
                }
            }
            else if (x.IsEmpty())
            {
                break;
            }
            i += left ? 1 : -1;
        }
        if (!found)
        {
            Conte.slots[i].Held_Item = a;
        }
        SaveItemContainerData();
        OnInteract();
    }

    public void MassShiftClick(bool wasleft, bool requiresame)
    {
        for (int i = 0; i < Conte.slots.Count; i++)
        {
            int j = i;
            if (!wasleft) j = (Conte.slots.Count - 1) - i;
            if (Conte.slots[j].Held_Item.IsEmpty()) continue;
            if (requiresame && !Held_Item.Compare(Conte.slots[j].Held_Item)) continue;
            Conte.slots[j].ShiftClick(wasleft);
        }
    }


    public void LeftClick()
    {

        var g = GISLol.Instance;
        Held_Item.AddConnection(Conte);
        SaveItemContainerData();

        int xx = Held_Item.Amount;
        var it = new GISItem(Held_Item);

        if (DoubleClickTimer >= 0)
        {
            var d = g.Mouse_Held_Item.Name;
            var K = g.ItemDict[d].MaxAmount;
            if (g.Mouse_Held_Item.IsEmpty())
            {
                DoubleClickTimer = -69f;
            }
            else if (K != 0 && g.Mouse_Held_Item.Amount >= K)
            {
                DoubleClickTimer = -69f;
            }
            else if (Conte.AmountOfItem(g.Mouse_Held_Item) == 0)
            {
                DoubleClickTimer = -69f;
            }
        }

        if (DoubleClickTimer < 0)
        {
            if (Conte.CanDoubleClickItems) DoubleClickTimer = 0.2f;
            var a = g.Mouse_Held_Item;

            if (Held_Item.Compare(a))
            {
                var d = Held_Item.Name;
                int b = a.Amount;
                int c = Held_Item.Amount + b;
                Held_Item.Amount = c;
                int K = g.ItemDict[d].MaxAmount;
                if (K != 0)
                {
                    if (c > K)
                    {
                        Held_Item.Amount = K;
                        g.Mouse_Held_Item.Amount = c - K;
                    }
                    else
                    {
                        g.Mouse_Held_Item.Amount = 0;
                    }
                }
                else
                {
                    g.Mouse_Held_Item.Amount = 0;
                }
                Held_Item.AddConnection(g.Mouse_Held_Item.Container);


                if (g.Mouse_Held_Item.Amount <= 0)
                {
                    g.Mouse_Held_Item = new GISItem();
                }
            }
            else
            {
                g.Mouse_Held_Item = Held_Item;
                g.Mouse_Held_Item.AddConnection(Conte);
                if (g.Mouse_Held_Item.IsEmpty())
                {
                    g.Mouse_Held_Item.SetContainer(null);
                }
                Held_Item = a;
                if (Held_Item.Container != Conte)
                {
                    Held_Item.SetContainer(Conte);
                }
            }
        }
        else
        {
            //double click code
            DoubleClickTimer = -69f;

            int x = g.ItemDict[g.Mouse_Held_Item.Name].MaxAmount;
            foreach (var slot in Conte.slots)
            {
                if (slot != this && slot.Held_Item.Compare(g.Mouse_Held_Item))
                {
                    if (x != 0 && g.Mouse_Held_Item.Amount + slot.Held_Item.Amount > x)
                    {
                        slot.Held_Item.Amount = slot.Held_Item.Amount - (x - g.Mouse_Held_Item.Amount);
                        g.Mouse_Held_Item.Amount = x;
                    }
                    else
                    {
                        g.Mouse_Held_Item.Amount += slot.Held_Item.Amount;
                        slot.Held_Item.Amount = 0;
                    }
                }
            }
            for (int i = 0; i < Conte.slots.Count; i++)
            {
                if (Conte.slots[i].Held_Item.Amount <= 0)
                {
                    Conte.slots[i].Held_Item = new GISItem();
                }
            }
            g.Mouse_Held_Item.AddConnection(Conte);

        }


        if (Conte.CanDragDistributeItems && !Held_Item.IsEmpty())
        {
            g.DragSlotsLeft.Clear();
            g.DragItemLeft = new GISItem(Held_Item);
            if (Held_Item.Compare(it))
            {
                if (xx > 0) g.DragItemLeft.Amount -= xx;
                StoredAmount = xx;
            }
            else StoredAmount = 0;
            g.SameFixedStop = 1;
            g.SameFrameStop = 3;
            g.DragSlotsLeft.Add(this);
        }


        SaveItemContainerData();
        OnInteract();
    }

    public void RightClick()
    {
        var g = GISLol.Instance;
        Held_Item.AddConnection(Conte);
        SaveItemContainerData();
        var a = g.Mouse_Held_Item;

        int xx = Held_Item.Amount;
        var it = new GISItem(Held_Item);

        if (Held_Item.IsEmpty())
        {
            if (a.Amount > 0)
            {
                Held_Item = new GISItem(a);
                Held_Item.Amount = 1;
                Held_Item.SetContainer(Conte);

                g.Mouse_Held_Item.Amount--;
                g.Mouse_Held_Item.AddConnection(Conte);
                if (g.Mouse_Held_Item.Amount <= 0)
                {
                    g.Mouse_Held_Item = new GISItem();
                }
            }
        }
        else
        {
            if (a.IsEmpty())
            {
                float b = (float)Held_Item.Amount / 2;
                g.Mouse_Held_Item = new GISItem(Held_Item);
                g.Mouse_Held_Item.Amount = Mathf.CeilToInt(b);
                Held_Item.Amount = Mathf.FloorToInt(b);
                g.Mouse_Held_Item.AddConnection(Conte);
                if (Held_Item.Amount <= 0)
                {
                    Held_Item = new GISItem();
                }
            }
            else if (Held_Item.Compare(a))
            {
                int max = g.ItemDict[Held_Item.Name].MaxAmount;
                if (max <= 0 || Held_Item.Amount < max)
                {
                    Held_Item.Amount++;
                    Held_Item.AddConnection(g.Mouse_Held_Item.Container);
                    g.Mouse_Held_Item.AddConnection(Held_Item.Container);
                    g.Mouse_Held_Item.Amount--;
                    if (g.Mouse_Held_Item.Amount <= 0)
                    {
                        g.Mouse_Held_Item = new GISItem();
                    }
                }
            }
        }

        if (Conte.CanDragDistributeItems && !Held_Item.IsEmpty())
        {
            g.DragSlotsRight.Clear();
            GISLol.Instance.DragSlotsRight.Add(this);
        }

        SaveItemContainerData();
        OnInteract();



    }


    public bool IsHoveringBaldVersion()
    {
        //deprecated mouse hovering code, only keeping it incase anyone wants to use it because im like 95% certain its faster than the new method, it just works less well
        var size = 1f;

        var m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var pos = erect.position;
        if (pos.x - size < m.x && pos.x + size > m.x && pos.y - size < m.y && pos.y + size > m.y)
        {
            return true;
        }
        return false;
    }
    public bool IsHovering()
    {
        return Hover.IsHovering(gameObject);
    }

    public GISSlot HoverCheckerData()
    {
        if (!gameObject.activeInHierarchy) return null;
        if (IsHovering())
        {
            return this;
        }
        return null;
    }
    private void FixedUpdate()
    {
        DoubleClickTimer -= Time.deltaTime;
        Displayer.item = Held_Item;
    }

    private void SaveItemContainerData()
    {
        bool sexg = Conte.SaveTempContents();
        if (sexg)
        {
            //Held_Item.AddConnection(Held_Item.Container);
            foreach (var c in Held_Item.Interacted_Containers)
            {
                if (c != null) c.SaveTempContents();
            }
            Held_Item.Interacted_Containers.Clear();
        }
    }
}
