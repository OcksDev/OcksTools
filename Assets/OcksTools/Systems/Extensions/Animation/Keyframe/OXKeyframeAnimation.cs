using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure data asset with no runtime state. Each Play() creates its own OXKeyframeAnimationRuntime.
/// </summary>
[CreateAssetMenu(fileName = "NewKeyframeAnimation", menuName = "OcksTools/Animation/Keyframe Animation")]
public class OXKeyframeAnimation : ScriptableObject
{
    public bool ResetAfterFinish = false;
    public bool OverrideData = false;

    /// <summary>
    /// Repeat until Stop(). Each pass re-fires events. If the animation ends on empty keyframe(s), the last one
    /// marks the loop length: objects ease from the last real key to the first real key of the next pass
    /// (using the first key's interpolation), and the next pass continues from that exact pose. Ignored for zero duration.
    /// </summary>
    public bool Loop = false;

    /// <summary>
    /// Start as if already transitioning from the last real keyframe toward the first, instead of easing out of
    /// the current pose. With Loop on, this is exactly the pose every later pass starts from.
    /// </summary>
    public bool StartLast = false;

    /// <summary>Seconds to crossfade from the animation already playing on the same animator. 0 = cut straight over. Play() can override it.</summary>
    public float FadeTime = 0f;

    public List<OXKeyframe> Keyframes = new List<OXKeyframe>();

    /// <summary>Named events fired during playback. Hook up with runtime.Append("Name", callback).</summary>
    public List<OXEventKeyframe> Events = new List<OXEventKeyframe>();

#if UNITY_EDITOR
    /// <summary>Editor-only preview scene contents, saved with the asset and compiled out of builds.</summary>
    public List<OXPreviewObject> PreviewObjects = new List<OXPreviewObject>();
#endif

    private static readonly Func<OXKeyframe, float> KeyframeTime = k => k != null ? k.Time : float.NegativeInfinity;
    private static readonly Func<OXEventKeyframe, float> EventTime = e => e != null ? e.Time : float.NegativeInfinity;

    /// <summary>Returns a new list sorted by time (stable, like OrderBy, but without LINQ allocations).</summary>
    public List<OXKeyframe> GetSortedKeyframes() => StableSortedCopy(Keyframes, KeyframeTime);
    public List<OXEventKeyframe> GetSortedEventKeyframes() => StableSortedCopy(Events, EventTime);

    // Copy + fast path for already-sorted input (the common case); otherwise stable binary insertion.
    private static List<T> StableSortedCopy<T>(List<T> source, Func<T, float> time) where T : class
    {
        int n = source.Count;
        var result = new List<T>(n);
        float prev = float.NegativeInfinity;
        int i = 0;
        for (; i < n; i++)
        {
            float t = time(source[i]);
            if (t < prev) break;
            prev = t;
            result.Add(source[i]);
        }
        for (; i < n; i++)
        {
            var item = source[i];
            float t = time(item);
            int lo = 0, hi = result.Count; // upper bound keeps equal times in original order
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (time(result[mid]) <= t) lo = mid + 1; else hi = mid;
            }
            result.Insert(lo, item);
        }
        return result;
    }

    /// <summary>Plays this animation. fadeTime &lt; 0 uses the asset's FadeTime; 0 cuts straight over.</summary>
    public OXKeyframeAnimationRuntime Play(BetterList<GameObject> objects, float fadeTime = -1f)
    {
        var list = objects.ToList();
        if (list == null || list.Count == 0) return null;
        return OXKeyframeAnimator.GetOrAdd(list[0]).Play(this, list, fadeTime);
    }
}

public enum OXKeyframeInterpolationMode
{
    Linear,
    Elastic,
    In,
    Out,
    InAndOut,
    Sin,
    Cos,
    SinInAndOut,
    CircIn,
    CircOut,
    Bounce,
    Overshoot,
}

[Serializable]
public class OXKeyframeChannel
{
    /// <summary>Opt-in. When false, the keyframe contributes nothing to this channel's timeline.</summary>
    public bool Enabled = false;

    /// <summary>How the channel eases from its previous key into this one.</summary>
    public OXKeyframeInterpolationMode InterpMode = OXKeyframeInterpolationMode.Linear;

    /// <summary>
    /// Apply this key's value on top of the previous key's result (position adds, scale multiplies, rotation composes)
    /// instead of on top of the starting pose. The first key of a channel behaves the same either way.
    /// </summary>
    public bool RelativeToSelf = false;

    // Separate parameter groups so switching modes never loses a value.
    [Tooltip("In, Out, InAndOut")]
    public float Power = 3f;

    [Tooltip("CircIn, CircOut")]
    public float CircPower = 2f;

    [Tooltip("Bounce")]
    public int Bounces = 4;
    [Tooltip("Bounce")]
    public float BouncePower = 5f;

    [Tooltip("Elastic")]
    public float Oscillations = 3f;

    [Tooltip("Overshoot")]
    public float Magnification = 2f;
    [Tooltip("Overshoot (shouldn't be less than 2)")]
    public float OvershootPower = 2f;

    // Stored inverted so existing assets keep animating all axes. A disabled axis is left untouched.
    [Tooltip("Stop this channel from animating the X axis")]
    public bool DisableX = false;
    [Tooltip("Stop this channel from animating the Y axis")]
    public bool DisableY = false;
    [Tooltip("Stop this channel from animating the Z axis")]
    public bool DisableZ = false;

    /// <summary>True if this channel animates the given axis (0 = X, 1 = Y, 2 = Z).</summary>
    public bool AxisEnabled(int axis)
    {
        switch (axis)
        {
            case 0: return !DisableX;
            case 1: return !DisableY;
            default: return !DisableZ;
        }
    }

    public void SetAxisEnabled(int axis, bool enabled)
    {
        switch (axis)
        {
            case 0: DisableX = !enabled; break;
            case 1: DisableY = !enabled; break;
            default: DisableZ = !enabled; break;
        }
    }

    public bool AnyAxisEnabled => !DisableX || !DisableY || !DisableZ;
    public bool AllAxesEnabled => !DisableX && !DisableY && !DisableZ;

    public OXKeyframeChannel() { }

    public OXKeyframeChannel(bool enabled, OXKeyframeInterpolationMode mode = OXKeyframeInterpolationMode.Linear)
    {
        Enabled = enabled;
        InterpMode = mode;
    }

    /// <summary>Applies this channel's easing to a 0..1 progress value.</summary>
    public float Evaluate(float x)
    {
        switch (InterpMode)
        {
            case OXKeyframeInterpolationMode.Elastic: return Ease.Elastic(x, Oscillations);
            case OXKeyframeInterpolationMode.In: return Ease.In(x, Power);
            case OXKeyframeInterpolationMode.Out: return Ease.Out(x, Power);
            case OXKeyframeInterpolationMode.InAndOut: return Ease.InAndOut(x, Power);
            case OXKeyframeInterpolationMode.Sin: return Ease.Sin(x);
            case OXKeyframeInterpolationMode.Cos: return Ease.Cos(x);
            case OXKeyframeInterpolationMode.SinInAndOut: return Ease.SinInAndOut(x);
            case OXKeyframeInterpolationMode.Overshoot: return Ease.Overshoot(x, Magnification, OvershootPower);
            case OXKeyframeInterpolationMode.Bounce: return Ease.Bounce(x, Bounces, BouncePower);
            case OXKeyframeInterpolationMode.CircIn: return Ease.CircIn(x, CircPower);
            case OXKeyframeInterpolationMode.CircOut: return Ease.CircOut(x, CircPower);
            default: return x;
        }
    }
}

[Serializable]
public class OXKeyframeObjectState
{
    /// <summary>Index into the object list passed to Play().</summary>
    public int ObjectIndex;

    public OXTransformWithScale Transform = new OXTransformWithScale
    {
        Rotation = Quaternion.identity,
        Scale = Vector3.one
    };

    /// <summary>
    /// Unwrapped rotation in Euler degrees (e.g. 720 = two full spins). A quaternion can't hold more than one turn, so the
    /// rotation channel reads this instead when HasRotationEuler is set. Transform.Rotation is kept in sync as Quaternion.Euler(RotationEuler).
    /// Old assets leave this off and keep the shortest-path quaternion behavior.
    /// </summary>
    public Vector3 RotationEuler;
    public bool HasRotationEuler;

    public static implicit operator OXKeyframeObjectState((int, OXTransformWithScale) tuple)
    {
        return new OXKeyframeObjectState { ObjectIndex = tuple.Item1, Transform = tuple.Item2 };
    }
}

[Serializable]
public class OXKeyframe
{
    public float Time;

    /// <summary>Per-channel settings, all opted out by default. Each channel runs on its own timeline of the keys that enabled it.</summary>
    public OXKeyframeChannel Position = new OXKeyframeChannel();
    public OXKeyframeChannel Rotation = new OXKeyframeChannel();
    public OXKeyframeChannel Scale = new OXKeyframeChannel();

    public List<OXKeyframeObjectState> Data = new List<OXKeyframeObjectState>();

    /// <summary>Enables every channel with the same interpolation mode. Returns this for chaining.</summary>
    public OXKeyframe EnableAll(OXKeyframeInterpolationMode mode = OXKeyframeInterpolationMode.Linear)
    {
        Position = new OXKeyframeChannel(true, mode);
        Rotation = new OXKeyframeChannel(true, mode);
        Scale = new OXKeyframeChannel(true, mode);
        return this;
    }
}

#if UNITY_EDITOR
// Append only: serialized as ints.
public enum OXPreviewObjectKind
{
    Empty, Cube, Sphere, Capsule, Cylinder, Plane, Quad, Prefab,
    Sprite,
    // Generated 2D shapes. Keep Square2D first and Hexagon2D last.
    Square2D, Circle2D, Triangle2D, Capsule2D, Diamond2D, Hexagon2D,
}

/// <summary>
/// One object in the editor preview scene. ObjectIndex matches the keyframes' Object States (-1 = scenery, never animated).
/// Transform is the rest pose the animation is applied on top of.
/// </summary>
[Serializable]
public class OXPreviewObject
{
    public int ObjectIndex = -1;
    public string Name = "Object";
    public OXPreviewObjectKind Kind = OXPreviewObjectKind.Empty;
    public GameObject Prefab;
    public Sprite Sprite;
    /// <summary>Tint for sprite kinds.</summary>
    public Color Color = Color.white;
    public Vector3 Position = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
}
#endif

/// <summary>A named point in time; callbacks registered under Name run when playback reaches Time.</summary>
[Serializable]
public class OXEventKeyframe
{
    public float Time;
    public string Name = "Event";
}
