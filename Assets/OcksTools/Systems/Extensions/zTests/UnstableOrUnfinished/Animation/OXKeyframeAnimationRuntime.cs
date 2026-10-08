using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimationRuntime
{
    private struct Key<T>
    {
        public float Time;
        public T Value;
        /// <summary>The keyframe's channel settings for this key; owns the easing mode and its parameters.</summary>
        public OXKeyframeChannel Channel;
    }

    private class Target
    {
        public GameObject Go;
        /// <summary>The object's real transform before the animation touched it. Reset() restores this.</summary>
        public OXTransformWithScale Original = new OXTransformWithScale();
        /// <summary>The base the channel values are applied relative to (identity when OverrideData is on).</summary>
        public OXTransformWithScale Start = new OXTransformWithScale();

        // One independent timeline per channel. Empty list = this animation never touches that channel.
        public readonly List<Key<Vector3>> PosKeys = new List<Key<Vector3>>();
        public readonly List<Key<Quaternion>> RotKeys = new List<Key<Quaternion>>();
        public readonly List<Key<Vector3>> ScaleKeys = new List<Key<Vector3>>();
    }

    public readonly OXKeyframeAnimation Asset;
    public bool IsPlaying { get; private set; }

    private readonly OXKeyframeAnimator animator;
    private readonly List<Target> targets = new List<Target>();
    private Coroutine routine;
    private float totalDuration;
    private bool hasAnyTrack;

    public OXKeyframeAnimationRuntime(OXKeyframeAnimation asset, OXKeyframeAnimator animator, IList<GameObject> objects)
    {
        Asset = asset;
        this.animator = animator;
        foreach (var go in objects)
            targets.Add(CreateTarget(go));
        BuildTracks();
    }

    // Snapshot the starting transform up front, so Reset() works even before/after playback.
    private Target CreateTarget(GameObject go)
    {
        var t = new Target { Go = go };
        if (go != null)
        {
            t.Original.Position = go.transform.localPosition;
            t.Original.Rotation = go.transform.localRotation;
            t.Original.Scale = go.transform.localScale;

            if (Asset.OverrideData) ResetToIdentity(t.Start);
            else CopyState(t.Original, t.Start);
        }
        return t;
    }

    /// <summary>
    /// Splits the shared keyframe timeline into one track per object per channel.
    /// A keyframe only contributes a key to a channel if that object's state opted into it.
    /// </summary>
    private void BuildTracks()
    {
        var keyframes = Asset.GetSortedKeyframes(); // already sorted by time
        totalDuration = 0f;
        hasAnyTrack = false;

        foreach (var kf in keyframes)
        {
            if (kf == null || kf.Data == null) continue;
            foreach (var d in kf.Data)
            {
                if (!TryGetTarget(d, out var t)) continue;
                if (d.Transform == null) continue;

                bool used = false;
                if (kf.Position != null && kf.Position.Enabled)
                {
                    t.PosKeys.Add(new Key<Vector3> { Time = kf.Time, Value = d.Transform.Position, Channel = kf.Position });
                    used = true;
                }
                if (kf.Rotation != null && kf.Rotation.Enabled)
                {
                    var rot = IsZeroQuat(d.Transform.Rotation) ? Quaternion.identity : d.Transform.Rotation;
                    t.RotKeys.Add(new Key<Quaternion> { Time = kf.Time, Value = rot, Channel = kf.Rotation });
                    used = true;
                }
                if (kf.Scale != null && kf.Scale.Enabled)
                {
                    t.ScaleKeys.Add(new Key<Vector3> { Time = kf.Time, Value = d.Transform.Scale, Channel = kf.Scale });
                    used = true;
                }

                if (used)
                {
                    hasAnyTrack = true;
                    if (kf.Time > totalDuration) totalDuration = kf.Time;
                }
            }
        }
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

    /// <summary>
    /// Puts every animated channel back to the value it had when this animation was created.
    /// Channels this animation never touched are left alone.
    /// Does not stop playback; use StopAndReset() if the animation may still be running.
    /// </summary>
    public void Reset()
    {
        foreach (var t in targets)
        {
            if (t.Go == null) continue;
            if (t.PosKeys.Count > 0) t.Go.transform.localPosition = t.Original.Position;
            if (t.RotKeys.Count > 0) t.Go.transform.localRotation = t.Original.Rotation;
            if (t.ScaleKeys.Count > 0) t.Go.transform.localScale = t.Original.Scale;
        }
    }

    /// <summary>Stops playback, then restores the objects to their starting state.</summary>
    public void StopAndReset()
    {
        Stop();
        Reset();
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

    /// <summary>
    /// Index of the first key strictly after time t, or keys.Count if t is at/after the last key.
    /// </summary>
    private static int NextKeyIndex<T>(List<Key<T>> keys, float t)
    {
        for (int i = 0; i < keys.Count; i++)
            if (keys[i].Time > t) return i;
        return keys.Count;
    }

    /// <summary>
    /// Eased 0..1 progress through the segment ending at keys[i].
    /// The segment starts at the previous key, or at time 0 (the object's start pose) for the first key,
    /// so a channel's first key is eased into rather than snapped to.
    /// </summary>
    private static float SegmentX<T>(List<Key<T>> keys, int i, float t)
    {
        float prevTime = i == 0 ? 0f : keys[i - 1].Time;
        float dt = keys[i].Time - prevTime;
        float x = dt <= 0.0001f ? 1f : Mathf.Clamp01((t - prevTime) / dt);
        return keys[i].Channel.Evaluate(x);
    }

    private static Vector3 EvalVector(List<Key<Vector3>> keys, float t, Vector3 identity)
    {
        int i = NextKeyIndex(keys, t);
        if (i >= keys.Count) return keys[keys.Count - 1].Value; // hold last value
        Vector3 prev = i == 0 ? identity : keys[i - 1].Value;
        return prev.LerpU(keys[i].Value, SegmentX(keys, i, t));
    }

    private static Quaternion EvalRotation(List<Key<Quaternion>> keys, float t)
    {
        int i = NextKeyIndex(keys, t);
        if (i >= keys.Count) return keys[keys.Count - 1].Value;
        Quaternion prev = i == 0 ? Quaternion.identity : keys[i - 1].Value;
        return prev.SlerpU(keys[i].Value, SegmentX(keys, i, t));
    }

    /// <summary>Evaluates every channel track at animation time t and writes it to the objects.</summary>
    private void Apply(float t)
    {
        foreach (var tar in targets)
        {
            if (tar.Go == null) continue;
            var tr = tar.Go.transform;

            if (tar.PosKeys.Count > 0)
                tr.localPosition = tar.Start.Position + EvalVector(tar.PosKeys, t, Vector3.zero);

            if (tar.RotKeys.Count > 0)
                tr.localRotation = tar.Start.Rotation * EvalRotation(tar.RotKeys, t);

            if (tar.ScaleKeys.Count > 0)
            {
                var s = EvalVector(tar.ScaleKeys, t, Vector3.one);
                s.x *= tar.Start.Scale.x;
                s.y *= tar.Start.Scale.y;
                s.z *= tar.Start.Scale.z;
                tr.localScale = s;
            }
        }
    }

    private IEnumerator Animation()
    {
        // Nothing opted into any channel, so there is nothing to play.
        if (!hasAnyTrack)
        {
            Stop();
            yield break;
        }

        if (totalDuration > 0f)
        {
            // One continuous pass over the whole timeline; every channel samples its own track.
            float duration = totalDuration;
            yield return OXLerp.Frame.Linear((float x) => Apply(x * duration), duration);
        }

        // Make sure we land exactly on the final values.
        Apply(totalDuration);

        // Finished naturally.
        routine = null;
        IsPlaying = false;
        if (Asset.ResetAfterFinish) Reset();
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
