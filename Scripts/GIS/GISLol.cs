using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GISLol : MonoBehaviour
{
    public bool UseLanguageFile = true;
    public GISItem Mouse_Held_Item;
    public GISDisplay Mouse_Displayer;
    public GameObject MouseFollower;
    public static GISLol instance;



    public List<GISContainer> All_Containers = new List<GISContainer>();


    [Header("Item Data")]
    public Sprite[] ItemImages;
    public string[] ItemNameTechnical;
    public string[] ItemName;
    public string[] ItemDescription;
    public int[] ItemMaxAmount;

    public static GISLol Instance
    {
        get { return instance; }
    }

    public void LoadTempForAll()
    {
        foreach(var con in All_Containers)
        {
            if (con != null) con.LoadTempContents();
        }
    }

    private void Awake()
    {
        if (Instance == null) instance = this;
        Mouse_Held_Item = new GISItem();
    }

    private void Start()
    {
        MouseFollower.SetActive(true);

        if (UseLanguageFile)
        {
            var l = LanguageFileSystem.Instance;
            l.UpdateGameFromFile();
            bool changed = false;
            for (int i = 0; i < ItemNameTechnical.Length; i++)
            {
                var s = "Item_" + ItemNameTechnical[i];
                var s2 = "ItemDesc_" + ItemNameTechnical[i];
                if (l.IndexValuePairs.ContainsKey(s))
                {
                    ItemName[i] = l.IndexValuePairs[s];
                }
                else
                {
                    changed = true;
                    l.IndexValuePairs.Add(s, ItemName[i]);
                }
                if (l.IndexValuePairs.ContainsKey(s2))
                {
                    ItemDescription[i] = l.IndexValuePairs[s2];
                }
                else
                {
                    changed = true;
                    l.IndexValuePairs.Add(s2, ItemDescription[i]);
                }
            }
            if (changed) l.UpdateTextFile();
        }
    }

    private void Update()
    {
        Mouse_Displayer.item = Mouse_Held_Item;
        var za = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        za.z = 0;
        MouseFollower.transform.position = za;

        if (InputManager.IsKeyDown(InputManager.gamekeys["reload"]))
        {
            LoadTempForAll();
        }
        if (InputManager.IsKeyDown(KeyCode.X))
        {
            Mouse_Held_Item = new GISItem();
        }
    }
}



public class GISItem
{
    /*
     * When adding new attributes for items, make sure to update the below functions:
     * GISItem.GISItem()
     * GISItem.GISItem(GISItem)
     * GISItem.Compare(GISItem)
     * GISContainer.LoadContents()
     */

    public int ItemIndex;
    public int Amount;
    public GISContainer Container;
    public List<GISContainer> Interacted_Containers = new List<GISContainer>();

    public GISItem()
    {
        Amount = 0;
        ItemIndex = 0;
        Container = null;
    }
    public GISItem(GISItem sexnut)
    {
        Amount = sexnut.Amount;
        ItemIndex = sexnut.ItemIndex;
        Container = sexnut.Container;
    }
    public void Solidify()
    {
        AddConnection(Container);
        foreach (var c in Interacted_Containers)
        {
            if (c != null) c.SaveTempContents();
        }
    }

    public bool Compare(GISItem sexnut)
    {
        /* returns:
         * false - not the same
         * true - are the same
         */
        bool comp = false;

        if (ItemIndex == sexnut.ItemIndex) comp = true;

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
        if (gis == null || gis.CanRetainItems)
        {
            Container = gis;
        }
    }
}