using System.Collections;
using UnityEngine;

public class OXDefaultAnimations
{
    public static IEnumerator WobbleInEven(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float overshoot = RandomFunctions.EaseOscillate(x, 4, 2f);
            foreach (var a in c)
            {
                a.transform.localScale = Vector3.one * overshoot;
            }
        }, 0.5f);
    }
    public static IEnumerator WobbleInVH(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseOscillate(Mathf.Clamp01(y), 4, 2f);
            float overshoot2 = RandomFunctions.EaseOscillate(Mathf.Clamp01(y - off), 4, 2f);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 * overshoot2, 1 * overshoot1, 1);
            }
        }, 0.5f);
    }
    public static IEnumerator WobbleInHV(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseOscillate(Mathf.Clamp01(y), 4, 2f);
            float overshoot2 = RandomFunctions.EaseOscillate(Mathf.Clamp01(y - off), 4, 2f);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot2, 1);
            }
        }, 0.5f);
    }

    public static IEnumerator SpinInLeft(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseIn(Mathf.Clamp01(y));
            float overshoot2 = RandomFunctions.EaseIn(Mathf.Clamp01(y - off));
            float amount_1 = -270f;
            float amount_2 = 45f;
            foreach (var a in c)
            {
                float onedir = (amount_1 * (1f - overshoot1));
                float twodir = (amount_2 * (1f - overshoot2));
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir + twodir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinOutLeft(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            var y = x;
            float overshoot1 = RandomFunctions.EaseOut(Mathf.Clamp01(y));
            float amount_1 = -270f;
            foreach (var a in c)
            {
                float onedir = (amount_1 * (overshoot1));
                a.transform.localScale = new Vector3(1 - overshoot1, 1 - overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinInRight(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseIn(Mathf.Clamp01(y));
            float overshoot2 = RandomFunctions.EaseIn(Mathf.Clamp01(y - off));
            float amount_1 = 270f;
            float amount_2 = -45f;
            foreach (var a in c)
            {
                float onedir = (amount_1 * (1f - overshoot1));
                float twodir = (amount_2 * (1f - overshoot2));
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir + twodir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinOutRight(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            var y = x;
            float overshoot1 = RandomFunctions.EaseOut(Mathf.Clamp01(y));
            float amount_1 = 270f;
            foreach (var a in c)
            {
                float onedir = (amount_1 * (overshoot1));
                a.transform.localScale = new Vector3(1 - overshoot1, 1 - overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir);
            }
        }, 0.5f);
    }
    public static IEnumerator TVOut(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float overshootx = RandomFunctions.EaseIn(x, 3);
            float overshooty = RandomFunctions.EaseIn(x, 9);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 + overshootx, 1 - overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator TVOutAlt(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 9);
            overshooty *= 0.975f;
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 - overshootx, 1 - overshooty, 1);
            }
        }, 0.5f);
    }
    public static IEnumerator EaseInEven(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.0f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(overshootx, overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutEven(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.0f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 - overshooty, 1 - overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseInVH(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(overshootx, overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutVH(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 - overshooty, 1 - overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseInHV(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(overshooty, overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutHV(BetterList<GameObject> cum)
    {
        var c = cum.ToList();
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in c)
            {
                a.transform.localScale = new Vector3(1 - overshootx, 1 - overshooty, 1);
            }
        }, 0.35f);
    }
}



