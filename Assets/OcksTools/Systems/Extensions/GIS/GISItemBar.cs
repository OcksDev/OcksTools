using System.Collections.Generic;
using UnityEngine;

public class GISItemBar : MonoBehaviour
{
    public GISContainer Container;
    public GameObject DisplayPrefab;
    public CheckingMethod UpdateMethod = CheckingMethod.Event;
    private List<GISDisplay> shites = new List<GISDisplay>();
    // Start is called before the first frame update
    private void Start()
    {
        //updates the display on start
        UpdateDisplay();
        Container.OnContentsChanged.Append(() => { if (UpdateMethod == CheckingMethod.Event) UpdateDisplay(); });
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //updates the display every fixed update
        if (UpdateMethod == CheckingMethod.FixedUpdate) UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        var e = shites.Count;
        var s = Container.slots.Count;
        var i = e - s;
        if (i > 0)
        {
            for (int z = 0; z < i; z++)
            {
                Destroy(shites[0].gameObject);
                shites.RemoveAt(0);
            }
        }
        else if (i < 0)
        {
            i *= -1;
            for (int z = 0; z < i; z++)
            {
                var es = Instantiate(DisplayPrefab, transform.position, transform.rotation, transform);
                var eff = es.GetComponent<GISDisplay>();
                shites.Add(eff);
            }
        }

        for (int iz = 0; iz < shites.Count; iz++)
        {
            shites[iz].item.SetValue(Container.slots[iz].Held_Item);
        }
    }

    public enum CheckingMethod
    {
        FixedUpdate,
        Event,
    }
}
