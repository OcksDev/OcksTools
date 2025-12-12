using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GISDisplay : MonoBehaviour
{
    public GISItem item;
    public GameObject[] displays;
    public TextMeshProUGUI amnt;
    public bool IsPhysical = false;
    public bool UpdateOnFixedUpdate = true;
    private bool hasfoundcomponents = false;
    private SpriteRenderer[] spriteRenderer;
    private Image[] imageRenderer;
    private void Awake()
    {
        GetStuff();
    }
    //GISItem olditem = null;
    public void UpdateDisplay(bool force = false)
    {
        if (item == null) item = new GISItem();

        /*if (StoreOldAndCompare && !force)
        {
            if (olditem == item) return;
            olditem = item;
        }*/



        if (!hasfoundcomponents) GetStuff();
        var data = new GISDisplayData(item);
        amnt.text = data.Count;
        if (IsPhysical)
        {
            for (int i = 0; i < data.Images.Length; i++)
            {
                spriteRenderer[i].sprite = data.Images[i];
            }
        }
        else
        {
            for (int i = 0; i < data.Images.Length; i++)
            {
                imageRenderer[i].sprite = data.Images[i];
            }
        }
    }

    public void GetStuff()
    {
        if (hasfoundcomponents) return;
        if (IsPhysical)
        {
            spriteRenderer = new SpriteRenderer[displays.Length];
        }
        else
        {
            imageRenderer = new Image[displays.Length];
        }
        for (int i = 0; i < displays.Length; i++)
        {
            if (IsPhysical)
            {
                spriteRenderer[i] = displays[i].GetComponent<SpriteRenderer>();
            }
            else
            {
                imageRenderer[i] = displays[i].GetComponent<Image>();
            }
        }
        hasfoundcomponents = true;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (UpdateOnFixedUpdate) UpdateDisplay();
    }
}
