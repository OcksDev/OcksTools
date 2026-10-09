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
    public List<OXKeyframe> GetSortedKeyframes()
    {
        return Keyframes.OrderBy(k => k.Time).ToList();
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
