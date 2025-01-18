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
    public List<TextMeshProUGUI> CalcSizeOfTexts = new List<TextMeshProUGUI>();
    public List<RectTransform> CalcSizeOf = new List<RectTransform>();
    private void Start()
    {
        Debug.Log(CalcSizeDelta());
    }
    public void SetTitle(string st)
    {
        CalcSizeOfTexts[0].text = st;
    }
    public void SetDesc(string st)
    {
        CalcSizeOfTexts[1].text = st;
    }
    public Vector2 CalcSizeDelta()
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
        initpos = CalcSizeOf[0].anchoredPosition;
        size = CalcSizeOf[0].sizeDelta;
        for(int i = 1; i < CalcSizeOf.Count; i++)
        {
            var relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            Debug.Log("pos" + relativepos);
            var nerdsize = CalcSizeOf[i].sizeDelta;
            Debug.Log(nerdsize.x);
            var dd = relativepos.x + (nerdsize.x / 2);
            Debug.Log(dd);
            if (dd > (size.x/2))
            {
                var ff = (dd - (size.x / 2))/2;
                Debug.Log(ff);
                size.x += ff;
                initpos.x += ff;
            }
            relativepos = CalcSizeOf[i].anchoredPosition - initpos;
            Debug.Log("pos" + relativepos);

           /* dd = relativepos.x - (nerdsize.x / 2);
            if (-dd > (size.x/2))
            {
                var ff = (dd + (size.x / 2))/2;
                size.x -= ff;
                initpos.x += ff;
            }*/
        }

        //size.x += bordersize*2;
        //size.y += bordersize*2;
        return size;
    }
}
