using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackyWidthFixLol : MonoBehaviour
{
    public RectTransform Parente;
    public RectTransform Myself;
    // Update is called once per frame
    void OnEnable()
    {
        var m = Myself.sizeDelta;
        m.x = (Parente.rect.max - Parente.rect.min).x;
        Myself.sizeDelta = m;
    }
}
