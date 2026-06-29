using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GISDisplay : MonoBehaviour
{
    public Reactable<GISItem> item;
    public GameObject[] displays;
    public TextMeshProUGUI amnt;
    public bool IsPhysical = false;
    private bool hasfoundcomponents = false;
    private SpriteRenderer[] spriteRenderer;
    private Image[] imageRenderer;
    private void Awake()
    {
        GetStuff();
        if (item == null) item = new(new GISItem());
        item.OnValueChanged.Append(() => { UpdateDisplay(false); });
        item.CompareFunc = (x, y) => x.Amount == y.Amount && !y.Amount.HasChanged() && x.Compare(y, true);
    }
    //GISItem olditem = null;
    public void UpdateDisplay(bool force = false)
    {
        if (item == null) item = new(new GISItem());


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
}
