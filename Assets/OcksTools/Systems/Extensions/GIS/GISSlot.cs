using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (pp.Name == "Empty") return false;
        switch (InteractFilter)
        {
            case "TakeOnly":
                if (pp.Name != "Empty") return true;
                break;
            case "PlaceOnly":
                if (Held_Item.Name != "Empty") return true;
                break;
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

    private void Update()
    {
        var g = GISLol.Instance;
        bool shift = InputManager.IsKey("item_alt");
        bool ctrl = InputManager.IsKey("item_mod");
        bool left = InputManager.IsKeyDown("item_select");
        bool right = InputManager.IsKeyDown("item_half");
        if (!(left || right)) return;
        if (FailToClick()) return;
        if (!IsHovering()) return;
        switch (Name)
        {
            default:
                if (Conte.CanShiftClickItems && shift)
                {
                    if (left || right)
                    {
                        ShiftClick(left);
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
        if(nerd != null)
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
            else if (x.Name == "Empty")
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

    public void LeftClick()
    {

        var g = GISLol.Instance;
        Held_Item.AddConnection(Conte);
        SaveItemContainerData();
        if (DoubleClickTimer < 0)
        {
            DoubleClickTimer = 0.2f;
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
                if (g.Mouse_Held_Item.Name == "Empty")
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

            foreach (var slot in Conte.slots)
            {
                if (slot != this && slot.Held_Item.Compare(g.Mouse_Held_Item))
                {
                    int x = g.ItemDict[g.Mouse_Held_Item.Name].MaxAmount;
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
        SaveItemContainerData();
        OnInteract();
    }

    public void RightClick()
    {
        var g = GISLol.Instance;
        Held_Item.AddConnection(Conte);
        SaveItemContainerData();
        var a = g.Mouse_Held_Item;

        if (Held_Item.Name == "Empty")
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
            if (a.Name == "Empty")
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
                if (max == 0 || Held_Item.Amount < max)
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
