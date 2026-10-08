using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure data asset. Holds NO runtime state, so it is safe to reference from any number of objects.
/// Each Play() call creates its own OXKeyframeAnimationPlayer that owns the per-use state.
/// </summary>
[CreateAssetMenu(fileName = "NewKeyframeAnimation", menuName = "OcksTools/Animation/Keyframe Animation")]
public class OXKeyframeAnimation : ScriptableObject
{
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
public class OXKeyframeObjectState
{
    /// <summary>Index into the object list passed to Play().</summary>
    public int ObjectIndex;
    public OXTransformWithScale Transform = new OXTransformWithScale { Rotation = Quaternion.identity };

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
    public OXKeyframeInterpolationMode InterpMode;
    public List<OXKeyframeObjectState> Data = new List<OXKeyframeObjectState>();
}
