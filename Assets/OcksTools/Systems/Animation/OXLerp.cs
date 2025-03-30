using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXLerp : MonoBehaviour
{
    public static IEnumerator Linear(Action<float> method, float time = 1f)
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
    //infinitely progresses from 0-1, when it reaches 1 it jumps back to 0
    public static IEnumerator LinearInfniteLooped(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            x = (x+Time.deltaTime*f)%1;
            method(x);
            yield return null;
        }
    }

    //infinitely progresses from 0-infinity, never stops increasing
    public static IEnumerator LinearInfniteUncapped(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            x = x+Time.deltaTime*f;
            method(x);
            yield return null;
        }
    }

    //bounces back and forth linearly between 0-1
    public static IEnumerator Bounce(Action<float> method, int bounces, float time = 1f)
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
    public static IEnumerator BounceInfinite(Action<float> method, float time = 1f)
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
