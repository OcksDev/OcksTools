using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GISSlot : MonoBehaviour
{
    public string Name = "";
    public GISItem Held_Item;
    public GISDisplay Displayer;
    public GISContainer Conte;
    public string InteractFilter = "";
    private float DoubleClickTimer = -69f;

    public GISSlot(GISContainer cum)
    {
        Conte = cum;
    }

    public void Awake()
    {
        if (Held_Item == null)
        {
            Held_Item = new GISItem();
        }
    }
    public bool FailToClick()
    {
        var pp = GISLol.Instance.Mouse_Held_Item;
        if (pp.ItemIndex == 0) return false;
        switch (InteractFilter)
        {
            case "TakeOnly":
                if (pp.ItemIndex != 0) return true;
                break;
            case "PlaceOnly":
                if (Held_Item.ItemIndex != 0) return true;
                break;
            case "AbstractInput":
                if (pp.ItemIndex != 0)
                {
                    Conte.AbstractAdd(pp);
                    return true;
                }
                break;
        }
        return false;
    }

    public void OnInteract()
    {
        //code called whenever a slot if interacted with
    }

    private void OnMouseOver()
    {
        if (FailToClick()) return;
        var g = GISLol.Instance;

        bool shift = InputManager.IsKey(InputManager.gamekeys["item_alt"]);
        bool left = InputManager.IsKeyDown(InputManager.gamekeys["item_select"]);
        bool right = InputManager.IsKeyDown(InputManager.gamekeys["item_half"]);
        if (Conte.CanShiftClickItems && shift)
        {
            if (left || right)
            {
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
                        int max = g.Items[a.ItemIndex].MaxAmount;
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
                    else if (x.ItemIndex == 0)
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
        }
        else
        {
            if (left)
            {
                Held_Item.AddConnection(Conte);
                SaveItemContainerData();
                if (DoubleClickTimer < 0)
                {
                    DoubleClickTimer = 0.2f;
                    var a = g.Mouse_Held_Item;

                    if (Held_Item.Compare(a))
                    {
                        int d = Held_Item.ItemIndex;
                        int b = a.Amount;
                        int c = Held_Item.Amount + b;
                        Held_Item.Amount = c;
                        int K = g.Items[d].MaxAmount;
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
                        if (g.Mouse_Held_Item.ItemIndex == 0)
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
                            int x = g.Items[g.Mouse_Held_Item.ItemIndex].MaxAmount;
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
            if (right)
            {
                Held_Item.AddConnection(Conte);
                SaveItemContainerData();
                var a = g.Mouse_Held_Item;

                if (Held_Item.ItemIndex == 0)
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
                    if (a.ItemIndex == 0)
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
                        int max = g.Items[Held_Item.ItemIndex].MaxAmount;
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
        }

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
