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
public class OXSpeed
{
    public float Value;
    public OXSpeed(float value = 1f) { Value = value; }
    public void Reverse() => Value = -Value;
    public static implicit operator OXSpeed(float value) => new OXSpeed(value);
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
    public override WaitForSeconds Get() => new(t);
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

    public IEnumerator Linear(Action<float> method, float time = 1f, OXSpeed speed = null)
        => Linear(Wrap(method), time, speed);
    public IEnumerator Linear(Func<float, OXYielder> method, float time = 1f, OXSpeed speed = null)
    {
        float f = 1 / time;
        float x = Spd(speed) < 0f ? 1f : 0f;
        method(x);
        while (!LinearDone(x, Spd(speed)))
        {
            yield return Get();
            float d = Delta;
            float s = Spd(speed);
            if (s == 0f) continue;
            x = Mathf.Clamp01(x + (d * f * s));
            var q = method(x);
            if (q.setperc.HasValue)
            {
                x = q.setperc.Value;
                method(x);
            }
            if (q.yielder != null) yield return q.yielder;
        }
    }
    public IEnumerator LinearInfniteLooped(Action<float> method, float time = 1f, OXSpeed speed = null)
        => LinearInfniteLooped(Wrap(method), time, speed);
    public IEnumerator LinearInfniteLooped(Func<float, OXYielder> method, float time = 1f, OXSpeed speed = null)
    {
        float f = 1 / time;
        float x = Spd(speed) < 0f ? 1f : 0f;
        method(x);
        while (true)
        {
            yield return Get();
            float d = Delta;
            float s = Spd(speed);
            if (s == 0f) continue;
            x = (x + d * f * s) % 1;
            if (x < 0f) x += 1f;
            var q = method(x);
            if (q.setperc.HasValue)
            {
                x = q.setperc.Value;
                method(x);
            }
            if (q.yielder != null) yield return q.yielder;
        }
    }
    public IEnumerator LinearInfniteUncapped(Action<float> method, float time = 1f, OXSpeed speed = null)
        => LinearInfniteUncapped(Wrap(method), time, speed);
    public IEnumerator LinearInfniteUncapped(Func<float, OXYielder> method, float time = 1f, OXSpeed speed = null)
    {
        float f = 1 / time;
        float x = Spd(speed) < 0f ? 1f : 0f;
        method(x);
        while (true)
        {
            yield return Get();
            float d = Delta;
            float s = Spd(speed);
            if (s == 0f) continue;
            x = x + d * f * s;
            var q = method(x);
            if (q.setperc.HasValue)
            {
                x = q.setperc.Value;
                method(x);
            }
            if (q.yielder != null) yield return q.yielder;
        }
    }
    public IEnumerator Bounce(Action<float> method, int bounces, float time = 1f, OXSpeed speed = null)
        => Bounce(Wrap(method), bounces, time, speed);

    public IEnumerator Bounce(Func<float, OXYielder> method, int bounces, float time = 1f, OXSpeed speed = null)
    {
        float f = 1 / time;
        double end = bounces > 0 ? bounces : 0;
        double p = Spd(speed) < 0f ? end : 0.0;
        method(BounceX(p));
        while (true)
        {
            float s = Spd(speed);
            if ((s > 0f && p >= end) || (s < 0f && p <= 0.0)) yield break;
            yield return Get();
            float d = Delta;
            s = Spd(speed);
            if (s == 0f) continue;
            p = BounceStep(p, (double)(d * f * s));
            if (p < 0.0) p = 0.0;
            if (p > end) p = end;
            var q = method(BounceX(p));
            if (q.setperc.HasValue)
            {
                p = BounceSet(p, q.setperc.Value, s);
                if (p < 0.0) p = 0.0;
                if (p > end) p = end;
                method(q.setperc.Value);
            }
            if (q.yielder != null) yield return q.yielder;
        }
    }

    public IEnumerator BounceInfinite(Action<float> method, float time = 1f, OXSpeed speed = null)
        => BounceInfinite(Wrap(method), time, speed);
    public IEnumerator BounceInfinite(Func<float, OXYielder> method, float time = 1f, OXSpeed speed = null)
    {
        float f = 1 / time;
        double p = Spd(speed) < 0f ? 1.0 : 0.0; //p lives in [0,2): 0->1 rising, 1->2 falling
        method(BounceX(p));
        while (true)
        {
            yield return Get();
            float d = Delta;
            float s = Spd(speed);
            if (s == 0f) continue;
            p = Wrap2(BounceStep(p, (double)(d * f * s)));
            var q = method(BounceX(p));
            if (q.setperc.HasValue)
            {
                p = Wrap2(BounceSet(p, q.setperc.Value, s));
                method(q.setperc.Value);
            }
            if (q.yielder != null) yield return q.yielder;
        }
    }

    //helpers and shit
    private static float Spd(OXSpeed speed) => speed == null ? 1f : speed.Value;

    private static bool LinearDone(float x, float s) => (s > 0f && x >= 1f) || (s < 0f && x <= 0f);

    private static float BounceX(double p)
    {
        double leg = Math.Floor(p);
        double frac = p - leg;
        bool descending = (((long)leg) & 1L) == 1L;
        return (float)(descending ? 1.0 - frac : frac);
    }
    private static double BounceStep(double p, double d)
    {
        double np = p + d;
        if (d > 0.0)
        {
            double b = Math.Floor(p) + 1.0;
            if (np >= b) np = b;
        }
        else if (d < 0.0)
        {
            double b = Math.Ceiling(p) - 1.0;
            if (np <= b) np = b;
        }
        return np;
    }
    private static double BounceSet(double p, float x, float s)
    {
        double xc = x < 0f ? 0.0 : (x > 1f ? 1.0 : x);
        long leg = s >= 0f ? (long)Math.Ceiling(p) - 1L : (long)Math.Floor(p);
        bool descending = (leg & 1L) == 1L;
        return leg + (descending ? 1.0 - xc : xc);
    }
    private static double Wrap2(double p)
    {
        p %= 2.0;
        if (p < 0.0) p += 2.0;
        return p;
    }
    private static Func<float, OXYielder> Wrap(Action<float> method)
        => x => { method(x); return default(OXYielder); };

}


public struct OXYielder
{
    public YieldInstruction yielder;
    public float? setperc;
    public static implicit operator OXYielder(YieldInstruction yielder)
    => new OXYielder { yielder = yielder, setperc = null };
    public static implicit operator OXYielder(float x)
    => new OXYielder { yielder = null, setperc = x };
    public OXYielder(YieldInstruction a, float? b = null)
    {
        yielder = a;
        setperc = b;
    }
}
