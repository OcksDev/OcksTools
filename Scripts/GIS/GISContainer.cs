using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GISContainer : MonoBehaviour
{
    public string Name = "RandomThingIDK";
    public bool SaveLoadData = true;
    [Tooltip("Can cause item destruction when trying to return items to a container with no open slots")]
    public bool CanRetainItems = true;
    public bool GenerateRandomItems = false;
    public bool GenerateSlotObjects = true;
    public int GenerateXSlots = 20;
    public GameObject SlotPrefab;
    public List<GISSlot> slots= new List<GISSlot>();

    public List<GISItem> saved_items = new List<GISItem>();
    // Start is called before the first frame update
    void Start()
    {
        var myass = GetComponentsInChildren<GISSlot>();
        GISLol.Instance.All_Containers.Add(this);
        if (GenerateSlotObjects)
        {
            foreach (var pp in myass)
            {
                Destroy(pp.gameObject);
            }

            for(int i = 0; i < GenerateXSlots; i++)
            {
                var h = Instantiate(SlotPrefab, transform.position, transform.rotation, transform);
                var h2 = h.GetComponent<GISSlot>();
                h2.Conte = this;
                h2.Held_Item = new GISItem();
                slots.Add(h2);
            }
        }
        else
        {
            foreach (var pp in myass)
            {
                pp.Conte= this;
                slots.Add(pp);
            }
        }



        if (SaveLoadData)
        {
            LoadContents();
        }
        else if(GenerateRandomItems)
        {
            //this is some debug shit for creating a bunch of randomly generated new containers.
            foreach(var s in slots)
            {
                s.Held_Item = new GISItem();
                s.Held_Item.Amount = 69;
                s.Held_Item.ItemIndex = UnityEngine.Random.Range(0, 5);
                s.Held_Item.Container = this;
                if (s.Held_Item.ItemIndex == 0)
                {
                    s.Held_Item.Amount = 0;
                }
            }
        }
    }

    public void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var x = slots[FindEmptySlot()];
            x.Held_Item = new GISItem();
            x.Held_Item.Amount = 50;
            x.Held_Item.ItemIndex = UnityEngine.Random.Range(1, 5);
            x.Held_Item.Container = this;
            x.Held_Item.Solidify();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SaveContents();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            LoadContents();
        }*/
    }

    private void OnApplicationQuit()
    {
        SaveContents();
    }

    public bool SaveTempContents()
    {
        if(CanRetainItems && GISLol.Instance.Mouse_Held_Item.ItemIndex == 0)
        {
            saved_items.Clear();
            foreach(var h in slots)
            {
                saved_items.Add(new GISItem(h.Held_Item));
            }
            return true;
        }
        return false;
    }
    public void LoadTempContents()
    {
        if (CanRetainItems)
        {
            int i = 0;
            foreach (var h in saved_items)
            {
                slots[i].Held_Item = new GISItem(h);
                i++;
            }
        }
        else
        {
            foreach (var h in slots)
            {
                if(h.Held_Item.ItemIndex != 0)h.Held_Item.Container.ReturnItem(h.Held_Item);
            }
            foreach (var h in slots)
            {
                h.Held_Item = new GISItem();
            }
        }
        if (GISLol.Instance.Mouse_Held_Item.Container == this) GISLol.Instance.Mouse_Held_Item = new GISItem();
    }

    public void SaveContents()
    {
        GISLol.Instance.LoadTempForAll();
        List<string> a = new List<string>();
        List<string> b = new List<string>();

        foreach (var p in slots)
        {
            b.Clear();
            var c = p.Held_Item;
            b.Add(c.ItemIndex.ToString());
            b.Add(c.Amount.ToString());

            a.Add(ListToString(b, "~|~"));
        }




        PlayerPrefs.SetString(SaveSystem.Instance.Prefix(-1) + "cnt_" + Name, ListToString(a, "+=+"));
    }

    public int FindEmptySlot()
    {
        int i = -1;
        int k = 0;
        foreach(var j in slots)
        {
            if(j.Held_Item.ItemIndex == 0)
            {
                i = k;
                break;
            }
            k++;
        }

        return i;
    }



    public void LoadContents()
    {
        List<string> a = new List<string>();
        List<string> b = new List<string>();
        var gg = PlayerPrefs.GetString(SaveSystem.Instance.Prefix(-1) + "cnt_" + Name, "fuck");
        if (gg != "fuck")
        {
            a = StringToList(gg, "+=+");
            int i = 0;
            GISItem ghj = null;
            foreach(var st in a)
            {
                if (st != "")
                {
                    
                    b = StringToList(st, "~|~");
                    //Debug.Log(b);
                    ghj = new GISItem();

                    ghj.ItemIndex = int.Parse(b[0]);
                    ghj.Amount = int.Parse(b[1]);
                    ghj.Container = this;
                    
                    slots[i].Held_Item = ghj;
                    i++;
                    if (i >= slots.Count) break;
                }
            }

        }
        SaveTempContents();
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
                int max = GISLol.Instance.ItemMaxAmount[a.ItemIndex];
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
                found = true;
                slots[i].Held_Item = a;
                break;
            }
            if(!found)
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

    public int BoolToInt(bool a)
    {
        return a ? 1 : 0;
    }
    public bool IntToBool(int a)
    {
        return a == 1;
    }

    public string ListToString(List<string> eee, string split = ", ")
    {
        return String.Join(split, eee);
    }

    public List<string> StringToList(string eee, string split = ", ")
    {
        return eee.Split(split).ToList();
    }
}
