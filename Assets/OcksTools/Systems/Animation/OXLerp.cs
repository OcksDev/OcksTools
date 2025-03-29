using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXLerp : MonoBehaviour
{
    public static IEnumerator Anim(Action<float> method)
    {
        float x = 0f;
        while (x < 1)
        {
            x = Mathf.Clamp01(x+Time.deltaTime);
            method(x);
            yield return null;
        }
    }
    public static IEnumerator Anim(float time, Action<float> method)
    {
        float x = 0f;
        float f = 1 / time;
        while (x < 1)
        {
            x = Mathf.Clamp01(x+Time.deltaTime*f);
            method(x);
            yield return null;
        }
    }
}
