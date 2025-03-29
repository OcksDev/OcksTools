using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXLerp : MonoBehaviour
{
    public static IEnumerator Linear(Action<float> method)
    {
        float x = 0f;
        while (x < 1)
        {
            x = Mathf.Clamp01(x+Time.deltaTime);
            method(x);
            yield return null;
        }
    }
    public static IEnumerator Linear(float time, Action<float> method)
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

    //bounces back and forth linearly between 0-1
    public static IEnumerator Bounce(int bounces, Action<float> method)
    {
        float x = 0f;
        int i = 0;
        while(i < bounces)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Time.deltaTime);
                method(x);
                yield return null;
            }
            i++;
            if(i >= bounces) yield break;
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Time.deltaTime);
                method(x);
                yield return null;
            }
            i++;
            if (i >= bounces) yield break;
        }
    }
    public static IEnumerator Bounce(int bounces, float time, Action<float> method)
    {
        float x = 0f;
        float f = 1 / time;
        int i = 0;
        while(i < bounces)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Time.deltaTime*f);
                method(x);
                yield return null;
            }
            i++;
            if(i >= bounces) yield break;
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Time.deltaTime*f);
                method(x);
                yield return null;
            }
            i++;
            if (i >= bounces) yield break;
        }
    }
    public static IEnumerator BounceInfinite(Action<float> method)
    {
        float x = 0f;
        while (true)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Time.deltaTime);
                method(x);
                yield return null;
            }
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Time.deltaTime);
                method(x);
                yield return null;
            }
        }
    }
    public static IEnumerator BounceInfinite(float time, Action<float> method)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Time.deltaTime*f);
                method(x);
                yield return null;
            }
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Time.deltaTime*f);
                method(x);
                yield return null;
            }
        }
    }
}
