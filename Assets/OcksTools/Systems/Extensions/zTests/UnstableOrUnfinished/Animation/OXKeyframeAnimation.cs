using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure data asset. Holds NO runtime state, so it is safe to reference from any number of objects.
/// Each Play() call creates its own OXKeyframeAnimationRuntime that owns the per-use state.
/// </summary>
[CreateAssetMenu(fileName = "NewKeyframeAnimation", menuName = "OcksTools/Animation/Keyframe Animation")]
public class OXKeyframeAnimation : ScriptableObject
{
    public bool ResetAfterFinish = false;
    public bool OverrideData = false;
    public List<OXKeyframe> Keyframes = new List<OXKeyframe>();
    /// <summary>
    /// Named events fired during playback when their time is reached. Hook them up from code with
    /// runtime.Append("Name", callback) on the runtime returned by Play().
    /// </summary>
    public List<OXEventKeyframe> Events = new List<OXEventKeyframe>();

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: the objects that make up this animation's preview scene. Saved with the asset so the
    /// scene comes back the same every time. Compiled out of builds, so prefabs referenced here are never pulled in.
    /// </summary>
    public List<OXPreviewObject> PreviewObjects = new List<OXPreviewObject>();
#endif

    public List<OXKeyframe> GetSortedKeyframes()
    {
        return Keyframes.OrderBy(k => k.Time).ToList();
    }
    public List<OXEventKeyframe> GetSortedEventKeyframes()
    {
        return Events.OrderBy(k => k.Time).ToList();
    }
    public OXKeyframeAnimationRuntime Play(BetterList<GameObject> objects)
    {
        var pp = objects.ToList();
        if (pp == null || pp.Count == 0) return null;
        var animator = OXKeyframeAnimator.GetOrAdd(pp[0]);
        return animator.Play(this, pp);
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
    /// <summary>Opt-in. When false, this keyframe contributes nothing to the channel's timeline.</summary>
    public bool Enabled = false;

    /// <summary>How the channel eases from its previous key into this one.</summary>
    public OXKeyframeInterpolationMode InterpMode = OXKeyframeInterpolationMode.Linear;

    /// <summary>
    /// When true, this key's value is applied on top of the previous key's resulting value on this channel
    /// (position adds, scale multiplies, rotation composes) instead of on top of the starting pose.
    /// Repeated keys with Y = 5 therefore keep climbing. The first key of a channel has no previous key,
    /// so it behaves the same either way.
    /// </summary>
    public bool RelativeToSelf = false;

    // Easing parameters. Each group is only read by the modes listed, and the defaults match
    // the defaults in Ease, so leaving them alone gives the same result as before.
    // They are kept separate (rather than one shared "power") so switching modes never loses a value.

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

    // Per-axis opt-out. Stored inverted (Disable*, default false) so every existing asset and clipboard entry
    // keeps animating all three axes with no migration. A disabled axis is left completely untouched by
    // this channel: it contributes no keys to that axis and the object keeps whatever value that axis has.
    // Position/Scale: X, Y, Z. Rotation: the Euler X, Y, Z components of the key's rotation.
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

    /// <summary>True when at least one axis is still animated (Enabled is checked separately).</summary>
    public bool AnyAxisEnabled { get { return !DisableX || !DisableY || !DisableZ; } }

    /// <summary>True when every axis is animated, i.e. no masking is in effect.</summary>
    public bool AllAxesEnabled { get { return !DisableX && !DisableY && !DisableZ; } }

    public OXKeyframeChannel() { }

    public OXKeyframeChannel(bool enabled, OXKeyframeInterpolationMode mode = OXKeyframeInterpolationMode.Linear)
    {
        Enabled = enabled;
        InterpMode = mode;
    }

    /// <summary>Applies this channel's easing (mode + its parameters) to a 0..1 progress value.</summary>
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
            default: return x; // Linear
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

    public static implicit operator OXKeyframeObjectState((int, OXTransformWithScale) tuple)
    {
        return new OXKeyframeObjectState
        {
            ObjectIndex = tuple.Item1,
            Transform = tuple.Item2
        };
    }
}

[Serializable]
public class OXKeyframe
{
    public float Time;

    /// <summary>
    /// One settings object per channel, all opted out by default.
    /// Each channel runs on its own timeline made of just the keyframes that enabled it.
    /// </summary>
    public OXKeyframeChannel Position = new OXKeyframeChannel();
    public OXKeyframeChannel Rotation = new OXKeyframeChannel();
    public OXKeyframeChannel Scale = new OXKeyframeChannel();

    public List<OXKeyframeObjectState> Data = new List<OXKeyframeObjectState>();

    /// <summary>Code convenience: enables every channel with the same interpolation mode. Returns this for chaining.</summary>
    public OXKeyframe EnableAll(OXKeyframeInterpolationMode mode = OXKeyframeInterpolationMode.Linear)
    {
        Position = new OXKeyframeChannel(true, mode);
        Rotation = new OXKeyframeChannel(true, mode);
        Scale = new OXKeyframeChannel(true, mode);
        return this;
    }
}

#if UNITY_EDITOR
public enum OXPreviewObjectKind { Empty, Cube, Sphere, Capsule, Cylinder, Plane, Quad, Prefab }

/// <summary>
/// One object in the editor preview scene. ObjectIndex is the index keyframes use in their Object States
/// (-1 = scenery that is shown but never animated). Transform is the REST pose the animation is applied on top of.
/// </summary>
[Serializable]
public class OXPreviewObject
{
    public int ObjectIndex = -1;
    public string Name = "Object";
    public OXPreviewObjectKind Kind = OXPreviewObjectKind.Empty;
    public GameObject Prefab;
    public Vector3 Position = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
}
#endif

/// <summary>A named point in time. When playback reaches Time, the callback(s) registered under Name run.</summary>
[Serializable]
public class OXEventKeyframe
{
    public float Time;
    public string Name = "Event";
}
