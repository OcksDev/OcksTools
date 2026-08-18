using System;
using System.Collections;
using UnityEngine;

public class OXLerp
{
    public static _OXL_Fixed Fixed = new _OXL_Fixed();
    public static _OXL_FrameSTart Frame = new _OXL_FrameSTart();
    public static _OXL_FrameEnd FrameEnd = new _OXL_FrameEnd();
    public static _OXL_Custom Custom(float t) => new _OXL_Custom(t, Time.time);
}

public class _OXL_Fixed : _OXLerpType<WaitForFixedUpdate>
{
    public override WaitForFixedUpdate Get() => new();
}
public class _OXL_FrameEnd : _OXLerpType<WaitForEndOfFrame>
{
    public override WaitForEndOfFrame Get() => new();
}
public class _OXL_Custom : _OXLerpType<WaitForSeconds>
{
    private float t;
    private float ct;
    public _OXL_Custom(float tt, float ctt) { t = tt; ct = ctt; }
    public override WaitForSeconds Get() => new WaitForSeconds(t);
    public float GetDelta()
    {
        var dif = Time.time - ct;
        ct = Time.time;
        return dif;
    }
    public override float Delta => GetDelta();
}
public class _OXL_FrameSTart : _OXLerpType<WaitForEndOfFrame>
{
    public override WaitForEndOfFrame Get() => null;
}


public abstract class _OXLerpType<T> where T : YieldInstruction
{
    public abstract T Get();
    public virtual float Delta => Time.deltaTime;
    public IEnumerator Linear(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (x < 1)
        {
            x = Mathf.Clamp01(x + (Delta * f));
            method(x);
            yield return Get();
        }
    }
    //infinitely progresses from 0-1, when it reaches 1 it jumps back to 0
    public IEnumerator LinearInfniteLooped(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            x = (x + Delta * f) % 1;
            method(x);
            yield return null;
        }
    }

    //infinitely progresses from 0-infinity, never stops increasing
    public IEnumerator LinearInfniteUncapped(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            x = x + Delta * f;
            method(x);
            yield return null;
        }
    }

    //bounces back and forth linearly between 0-1
    public IEnumerator Bounce(Action<float> method, int bounces, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        int i = 0;
        while (i < bounces)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Delta * f);
                method(x);
                yield return Get();
            }
            i++;
            if (i >= bounces) yield break;
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Delta * f);
                method(x);
                yield return Get();
            }
            i++;
            if (i >= bounces) yield break;
        }
    }
    public IEnumerator BounceInfinite(Action<float> method, float time = 1f)
    {
        float x = 0f;
        float f = 1 / time;
        while (true)
        {
            while (x < 1)
            {
                x = Mathf.Clamp01(x + Delta * f);
                method(x);
                yield return Get();
            }
            while (x > 0)
            {
                x = Mathf.Clamp01(x - Delta * f);
                method(x);
                yield return Get();
            }
        }
    }
}