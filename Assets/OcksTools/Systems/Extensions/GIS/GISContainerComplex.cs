using UnityEngine;

public class GISContainerComplex : GISContainer
{
    public bool AutomaticallyAddChildren = true;
    public bool GenerateRandomItems = false;
    public bool GenerateSlotObjects = true;
    public int GenerateXSlots = 20;
    public GameObject SlotPrefab;

    public override void StartCode()
    {
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
            s.Held_Item = new GISItem(GISLol.Instance.Items.RandomElement().Name);
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
}
