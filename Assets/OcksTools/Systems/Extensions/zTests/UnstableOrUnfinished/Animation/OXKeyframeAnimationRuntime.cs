using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimationRuntime
{
    private class Target
    {
        public GameObject Go;
        public OXTransformWithScale Start = new OXTransformWithScale();
        public OXTransformWithScale Current = new OXTransformWithScale();
    }

    public readonly OXKeyframeAnimation Asset;
    public bool IsPlaying { get; private set; }

    private readonly OXKeyframeAnimator animator;
    private readonly List<Target> targets = new List<Target>();
    private Coroutine routine;

    public OXKeyframeAnimationRuntime(OXKeyframeAnimation asset, OXKeyframeAnimator animator, IList<GameObject> objects)
    {
        Asset = asset;
        this.animator = animator;
        foreach (var go in objects)
            targets.Add(new Target { Go = go });
    }

    public void Play()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        routine = animator.StartCoroutine(Animation());
    }

    public void Stop()
    {
        if (routine != null) animator.StopCoroutine(routine);
        routine = null;
        IsPlaying = false;
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }

    private static void ResetToIdentity(OXTransformWithScale t)
    {
        t.Position = Vector3.zero;
        t.Rotation = Quaternion.identity;
        t.Scale = Vector3.one;
    }

    private static void CopyState(OXTransformWithScale from, OXTransformWithScale to)
    {
        to.Position = from.Position;
        // A default-constructed Quaternion is (0,0,0,0), which is invalid. Treat it as identity.
        to.Rotation = IsZeroQuat(from.Rotation) ? Quaternion.identity : from.Rotation;
        to.Scale = from.Scale;
    }

    private static bool IsZeroQuat(Quaternion q)
    {
        return q.x == 0f && q.y == 0f && q.z == 0f && q.w == 0f;
    }

    private bool TryGetTarget(OXKeyframeObjectState d, out Target t)
    {
        t = null;
        if (d == null || d.ObjectIndex < 0 || d.ObjectIndex >= targets.Count) return false;
        t = targets[d.ObjectIndex];
        return t.Go != null;
    }

    private IEnumerator Animation()
    {
        var keyframes = Asset.GetSortedKeyframes();
        if (keyframes.Count <= 1)
        {
            Stop();
            yield break;
        }

        // Snapshot each object's starting transform; keyframes are applied relative to this.
        foreach (var t in targets)
        {
            ResetToIdentity(t.Current);
            if (t.Go == null) continue;
            t.Start.Position = t.Go.transform.localPosition;
            t.Start.Rotation = t.Go.transform.localRotation;
            t.Start.Scale = t.Go.transform.localScale;
        }

        // Optional delay before the first keyframe, after which the first keyframe's state is the baseline.
        if (keyframes[0].Time > 0)
        {
            yield return new WaitForSeconds(keyframes[0].Time);
        }
        foreach (var d in keyframes[0].Data)
        {
            if (TryGetTarget(d, out var t)) CopyState(d.Transform, t.Current);
        }

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            OXKeyframe next = keyframes[i + 1];
            float duration = Mathf.Max(next.Time - keyframes[i].Time, 0.0001f);

            yield return OXLerp.Frame.Linear((float x) =>
            {
                switch (next.InterpMode)
                {
                    case OXKeyframeInterpolationMode.Elastic: x = Ease.Elastic(x); break;
                    case OXKeyframeInterpolationMode.In: x = Ease.In(x); break;
                    case OXKeyframeInterpolationMode.Out: x = Ease.Out(x); break;
                    case OXKeyframeInterpolationMode.InAndOut: x = Ease.InAndOut(x); break;
                    case OXKeyframeInterpolationMode.Sin: x = Ease.Sin(x); break;
                    case OXKeyframeInterpolationMode.Cos: x = Ease.Cos(x); break;
                    case OXKeyframeInterpolationMode.SinInAndOut: x = Ease.SinInAndOut(x); break;
                    case OXKeyframeInterpolationMode.Overshoot: x = Ease.Overshoot(x); break;
                    case OXKeyframeInterpolationMode.Bounce: x = Ease.Bounce(x); break;
                    case OXKeyframeInterpolationMode.CircIn: x = Ease.CircIn(x); break;
                    case OXKeyframeInterpolationMode.CircOut: x = Ease.CircOut(x); break;
                }

                foreach (var d in next.Data)
                {
                    if (!TryGetTarget(d, out var t)) continue;

                    var goal = d.Transform;
                    var goalRot = IsZeroQuat(goal.Rotation) ? Quaternion.identity : goal.Rotation;

                    t.Go.transform.localPosition = t.Start.Position + t.Current.Position.LerpU(goal.Position, x);
                    t.Go.transform.localRotation = t.Start.Rotation * t.Current.Rotation.SlerpU(goalRot, x);

                    var s = t.Current.Scale.LerpU(goal.Scale, x);
                    s.x *= t.Start.Scale.x;
                    s.y *= t.Start.Scale.y;
                    s.z *= t.Start.Scale.z;
                    t.Go.transform.localScale = s;
                }
            }, duration);

            // Lock in this keyframe as the new baseline for the next segment.
            foreach (var d in next.Data)
            {
                if (TryGetTarget(d, out var t)) CopyState(d.Transform, t.Current);
            }
        }

        // Finished naturally.
        routine = null;
        IsPlaying = false;
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
