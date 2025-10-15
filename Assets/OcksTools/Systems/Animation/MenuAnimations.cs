using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class OXDefaultAnimations
{
    public static IEnumerator WobbleInEven(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float overshoot = RandomFunctions.EaseOvershoot(x, 4, 2f);
            foreach (var a in cum)
            {
                a.transform.localScale = Vector3.one * overshoot;
            }
        }, 0.5f);
    }
    public static IEnumerator WobbleInVH(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseOvershoot(Mathf.Clamp01(y), 4, 2f);
            float overshoot2 = RandomFunctions.EaseOvershoot(Mathf.Clamp01(y - off), 4, 2f);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 * overshoot2, 1 * overshoot1, 1);
            }
        }, 0.5f);
    }
    public static IEnumerator WobbleInHV(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseOvershoot(Mathf.Clamp01(y), 4, 2f);
            float overshoot2 = RandomFunctions.EaseOvershoot(Mathf.Clamp01(y - off), 4, 2f);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot2, 1);
            }
        }, 0.5f);
    }

    public static IEnumerator SpinInLeft(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseIn(Mathf.Clamp01(y));
            float overshoot2 = RandomFunctions.EaseIn(Mathf.Clamp01(y - off));
            float amount_1 = -270f;
            float amount_2 = 45f;
            foreach (var a in cum)
            {
                float onedir = (amount_1 * (1f - overshoot1));
                float twodir = (amount_2 * (1f - overshoot2));
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir + twodir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinOutLeft(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            var y = x;
            float overshoot1 = RandomFunctions.EaseOut(Mathf.Clamp01(y));
            float amount_1 = -270f;
            foreach (var a in cum)
            {
                float onedir = (amount_1 * (overshoot1));
                a.transform.localScale = new Vector3(1 - overshoot1, 1 - overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinInRight(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshoot1 = RandomFunctions.EaseIn(Mathf.Clamp01(y));
            float overshoot2 = RandomFunctions.EaseIn(Mathf.Clamp01(y - off));
            float amount_1 = 270f;
            float amount_2 = -45f;
            foreach (var a in cum)
            {
                float onedir = (amount_1 * (1f - overshoot1));
                float twodir = (amount_2 * (1f - overshoot2));
                a.transform.localScale = new Vector3(1 * overshoot1, 1 * overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir + twodir);
            }
        }, 0.5f);
    }
    public static IEnumerator SpinOutRight(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            var y = x;
            float overshoot1 = RandomFunctions.EaseOut(Mathf.Clamp01(y));
            float amount_1 = 270f;
            foreach (var a in cum)
            {
                float onedir = (amount_1 * (overshoot1));
                a.transform.localScale = new Vector3(1 - overshoot1, 1 - overshoot1, 1);
                a.transform.localRotation = Quaternion.Euler(0, 0, onedir);
            }
        }, 0.5f);
    }
    public static IEnumerator TVOut(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float overshootx = RandomFunctions.EaseIn(x, 3);
            float overshooty = RandomFunctions.EaseIn(x, 9);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 + overshootx, 1 - overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator TVOutAlt(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.35f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 9);
            overshooty *= 0.975f;
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 - overshootx, 1 - overshooty, 1);
            }
        }, 0.5f);
    }
    public static IEnumerator EaseInEven(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.0f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(overshootx, overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutEven(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.0f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 - overshooty, 1 - overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseInVH(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(overshootx, overshooty, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutVH(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 - overshooty, 1 - overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseInHV(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseIn(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseIn(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(overshooty, overshootx, 1);
            }
        }, 0.35f);
    }
    public static IEnumerator EaseOutHV(List<GameObject> cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float off = 0.15f;
            var y = x * (1 + off);
            float overshootx = RandomFunctions.EaseOut(Mathf.Clamp01(y - off), 3);
            float overshooty = RandomFunctions.EaseOut(Mathf.Clamp01(y), 3);
            foreach (var a in cum)
            {
                a.transform.localScale = new Vector3(1 - overshootx, 1 - overshooty, 1);
            }
        }, 0.35f);
    }
}



