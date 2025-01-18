using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotifOb : MonoBehaviour
{
    public Image Background1;
    public Image Background2;
    public Image Icon;
    public RectTransform SubObjectParent;
    public List<TextMeshProUGUI> CalcSizeOfTexts = new List<TextMeshProUGUI>();
    public List<RectTransform> CalcSizeOf = new List<RectTransform>();
    public RectTransform self;


    public void SetTitle(string st)
    {
        CalcSizeOfTexts[0].text = st;
    }
    public void SetDesc(string st)
    {
        CalcSizeOfTexts[1].text = st;
    }
    bool imageset = false;
    public void SetIMG(Sprite st)
    {
        if (st == null) return;
        imageset = true;
        Icon.sprite = st;
    }
    public BANNA CalcSizeDelta()
    {
        float bordersize = 10;


        var initpos = new Vector2(0, 0);
        var size = new Vector2(0, 0);

        foreach (var w in CalcSizeOfTexts)
        {
            var w2 = w.GetComponent<ContentSizeFitter>();
            w2.SetLayoutHorizontal();
            w2.SetLayoutVertical();
        }


        Icon.gameObject.SetActive(imageset);

        initpos = CalcSizeOf[0].anchoredPosition;
        size = CalcSizeOf[0].sizeDelta;
        for(int i = 1; i < CalcSizeOf.Count; i++)
        {
            if (CalcSizeOf[i] == null) continue;
            if (!CalcSizeOf[i].gameObject.activeSelf) continue;

            var relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            var nerdsize = CalcSizeOf[i].sizeDelta;

            var dd = relativepos.x + (nerdsize.x / 2);
            if (dd > (size.x/2))
            {
                var ff = (dd - (size.x / 2));
                size.x += ff;
                initpos.x += ff/2;
                relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            }
            dd = relativepos.x - (nerdsize.x / 2);
            if (-dd > (size.x/2))
            {
                var ff = (dd + (size.x / 2));
                size.x -= ff;
                initpos.x += ff/2;
                relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            }
            dd = relativepos.y + (nerdsize.y / 2);
            if (dd > (size.y/2))
            {
                var ff = (dd - (size.y / 2));
                size.y += ff;
                initpos.y += ff/2;
                relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            }
            dd = relativepos.y - (nerdsize.y / 2);
            if (-dd > (size.y /2))
            {
                var ff = (dd + (size.y / 2));
                size.y -= ff;
                initpos.y += ff/2;
                relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            }
        }
        bordersize += 5;
        size.x += bordersize*2;
        size.y += bordersize*2;
        var ba = new BANNA();
        ba.size = size;
        ba.pos = initpos;

        self.sizeDelta = ba.size;
        SubObjectParent.anchoredPosition = -ba.pos;

        return ba;
    }
}
public class BANNA
{
    public Vector2 size;
    public Vector2 pos;
}