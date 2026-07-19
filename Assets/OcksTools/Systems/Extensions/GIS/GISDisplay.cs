using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        item.CompareFunc = (x, y) => x.Amount == y.Amount && !y.Amount.HasChanged() && x.CompareDirect(y);
    }
    //GISItem olditem = null;
    public void UpdateDisplay(bool force = false)
    {
        if (item == null) item = new(new GISItem());


        if (!hasfoundcomponents) GetStuff();
        byte a = item.GetValue().AnimOverride;
        if (a != 2 || force)
        {
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
        if (force)
        {
            StopAnimation();
        }
        else
        {
            PlayAnimation(a);
        }
    }
    private Coroutine anim;

    public void StopAnimation()
    {
        if (anim != null)
        {
            StopCoroutine(anim);
            foreach (var a in displays.ToList())
            {
                a.transform.localScale = Vector3.one;
            }
        }
    }

    public void PlayAnimation(byte a)
    {
        if (a == 1 || a == 3)
        {
            item.GetValue().AnimOverride = (byte)(a == 1 ? 0 : 1);
            return;
        }
        StopAnimation();
        switch (a)
        {
            case 0:
                anim = StartCoroutine(GISInteract(displays.ToList()));
                break;
            case 2:
                anim = StartCoroutine(GISInteractToEmpty(displays.ToList(), this));
                break;
            case 1:
                return;
        }
        item.GetValue().AnimOverride = 0;
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
    public static IEnumerator GISInteract(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float overshoot = RandomFunctions.EaseOscillate(x, 3, 2f).Remap(0, 1, 0.7f, 1);
            foreach (var a in cum)
            {
                a.transform.localScale = Vector3.one * overshoot;
            }
        }, 0.35f);
    }
    public static IEnumerator GISInteractToEmpty(List<GameObject> cum, GISDisplay a)
    {
        yield return OXLerp.Linear((x) =>
        {
            x = 1 - RandomFunctions.EaseOut(x);
            foreach (var a in cum)
            {
                a.transform.localScale = Vector3.one * x;
            }
        }, 0.1f);
        a.UpdateDisplay(true);
    }
}
