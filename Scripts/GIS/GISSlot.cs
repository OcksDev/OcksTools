using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GISSlot : MonoBehaviour
{
    public GISItem Held_Item;
    public GISDisplay Displayer;
    public GISContainer Conte;
    private float DoubleClickTimer = -69f;

    private void OnMouseOver()
    {
        bool shift = InputManager.IsKey(InputManager.gamekeys["item_alt"]);
        bool left = InputManager.IsKeyDown(InputManager.gamekeys["item_select"]);
        bool right = InputManager.IsKeyDown(InputManager.gamekeys["item_half"]);
        if (shift)
        {
            if (left||right)
            {
                Held_Item.AddConnection(Conte);
                SaveItemContainerData();
                var a = new GISItem(Held_Item);
                Held_Item = new GISItem();
                int i = left?0:Conte.slots.Count - 1;
                bool found = false;
                foreach (var item in Conte.slots)
                {
                    var x = Conte.slots[i].Held_Item;
                    if (x.Compare(a))
                    {
                        int max = GISLol.Instance.ItemMaxAmount[a.ItemIndex];
                        int t = x.Amount + a.Amount;
                        if(max <= 0)
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
                            if(a.Amount == 0)
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
                    i += left?1:-1;
                }
                if (!found)
                {
                    Conte.slots[i].Held_Item = a;
                }
                SaveItemContainerData();
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
                    var a = GISLol.Instance.Mouse_Held_Item;

                    if (Held_Item.Compare(a))
                    {
                        int d = Held_Item.ItemIndex;
                        int b = a.Amount;
                        int c = Held_Item.Amount + b;
                        Held_Item.Amount = c;
                        int K = GISLol.Instance.ItemMaxAmount[d];
                        if (K != 0)
                        {
                            if (c > K)
                            {
                                Held_Item.Amount = K;
                                GISLol.Instance.Mouse_Held_Item.Amount = c - K;
                            }
                            else
                            {
                                GISLol.Instance.Mouse_Held_Item.Amount = 0;
                            }
                        }
                        else
                        {
                            GISLol.Instance.Mouse_Held_Item.Amount = 0;
                        }
                        Held_Item.AddConnection(GISLol.Instance.Mouse_Held_Item.Container);


                        if (GISLol.Instance.Mouse_Held_Item.Amount <= 0)
                        {
                            GISLol.Instance.Mouse_Held_Item = new GISItem();
                        }
                    }
                    else
                    {
                        GISLol.Instance.Mouse_Held_Item = Held_Item;
                        GISLol.Instance.Mouse_Held_Item.AddConnection(Conte);
                        if (GISLol.Instance.Mouse_Held_Item.ItemIndex == 0)
                        {
                            GISLol.Instance.Mouse_Held_Item.SetContainer(null);
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
                        if (slot != this && slot.Held_Item.Compare(GISLol.Instance.Mouse_Held_Item))
                        {
                            int x = GISLol.Instance.ItemMaxAmount[GISLol.Instance.Mouse_Held_Item.ItemIndex];
                            if (x != 0 && GISLol.Instance.Mouse_Held_Item.Amount + slot.Held_Item.Amount > x)
                            {
                                slot.Held_Item.Amount = slot.Held_Item.Amount - (x - GISLol.Instance.Mouse_Held_Item.Amount);
                                GISLol.Instance.Mouse_Held_Item.Amount = x;
                            }
                            else
                            {
                                GISLol.Instance.Mouse_Held_Item.Amount += slot.Held_Item.Amount;
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
                    GISLol.Instance.Mouse_Held_Item.AddConnection(Conte);

                }
                SaveItemContainerData();

            }
            if (right)
            {
                Held_Item.AddConnection(Conte);
                SaveItemContainerData();
                var a = GISLol.Instance.Mouse_Held_Item;

                if (Held_Item.ItemIndex == 0)
                {
                    if (a.Amount > 0)
                    {
                        Held_Item = new GISItem(a);
                        Held_Item.Amount = 1;
                        Held_Item.SetContainer(Conte);

                        GISLol.Instance.Mouse_Held_Item.Amount--;
                        GISLol.Instance.Mouse_Held_Item.AddConnection(Conte);
                        if (GISLol.Instance.Mouse_Held_Item.Amount <= 0)
                        {
                            GISLol.Instance.Mouse_Held_Item = new GISItem();
                        }
                    }
                }
                else
                {
                    if (a.ItemIndex == 0)
                    {
                        float b = (float)Held_Item.Amount / 2;
                        GISLol.Instance.Mouse_Held_Item = new GISItem(Held_Item);
                        GISLol.Instance.Mouse_Held_Item.Amount = Mathf.CeilToInt(b);
                        Held_Item.Amount = Mathf.FloorToInt(b);
                        GISLol.Instance.Mouse_Held_Item.AddConnection(Conte);
                        if (Held_Item.Amount <= 0)
                        {
                            Held_Item = new GISItem();
                        }
                    }
                    else if (Held_Item.Compare(a))
                    {
                        int max = GISLol.Instance.ItemMaxAmount[Held_Item.ItemIndex];
                        if(max == 0 || Held_Item.Amount < max)
                        {
                            Held_Item.Amount++;
                            Held_Item.AddConnection(GISLol.Instance.Mouse_Held_Item.Container);
                            GISLol.Instance.Mouse_Held_Item.AddConnection(Held_Item.Container);
                            GISLol.Instance.Mouse_Held_Item.Amount--;
                            if (GISLol.Instance.Mouse_Held_Item.Amount <= 0)
                            {
                                GISLol.Instance.Mouse_Held_Item = new GISItem();
                            }
                        }
                    }
                }

                SaveItemContainerData();



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
        if (Conte.CanRetainItems)
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
        /*
        foreach(var item in placed_items)
        {
            item.Container.SaveTempContents();
        }
        */
    }
}
