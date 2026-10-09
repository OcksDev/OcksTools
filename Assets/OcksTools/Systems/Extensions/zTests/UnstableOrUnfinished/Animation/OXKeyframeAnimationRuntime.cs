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

    // Events from the asset, snapshotted and sorted by time when this runtime was created.
    private readonly List<OXEventKeyframe> eventList = new List<OXEventKeyframe>();
    private int nextEvent;

    /// <summary>
    /// Callbacks by event name. An event keyframe in the asset with a matching Name invokes the
    /// callback(s) registered here when playback reaches its time.
    /// </summary>
    public Dictionary<string, OXEvent> Events = new();

    /// <summary>Registers an OXEvent under this name (replaces any OXEvent already registered under it).</summary>
    public void Append(string a, OXEvent b) => Events[a] = b;

    /// <summary>Registers a callback under this name. Calling it again with the same name adds another callback.</summary>
    public void Append(string a, System.Action b)
    {
        if (!Events.TryGetValue(a, out var o) || o == null)
        {
            o = new OXEvent();
            Events[a] = o;
        }
        o.Append(b);
    }

    public void Invoke(string a)
    {
        if (Events.TryGetValue(a, out var e) && e != null) e.Invoke();
    }

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
    /// Also snapshots the event keyframes (they extend the duration, so late events still fire).
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
                    var pos = d.Transform.Position;
                    if (kf.Position.RelativeToSelf && t.PosKeys.Count > 0)
                        pos = t.PosKeys[t.PosKeys.Count - 1].Value + pos;
                    t.PosKeys.Add(new Key<Vector3> { Time = kf.Time, Value = pos, Channel = kf.Position });
                    used = true;
                }
                if (kf.Rotation != null && kf.Rotation.Enabled)
                {
                    var rot = IsZeroQuat(d.Transform.Rotation) ? Quaternion.identity : d.Transform.Rotation;
                    if (kf.Rotation.RelativeToSelf && t.RotKeys.Count > 0)
                        rot = t.RotKeys[t.RotKeys.Count - 1].Value * rot;
                    t.RotKeys.Add(new Key<Quaternion> { Time = kf.Time, Value = rot, Channel = kf.Rotation });
                    used = true;
                }
                if (kf.Scale != null && kf.Scale.Enabled)
                {
                    var scl = d.Transform.Scale;
                    if (kf.Scale.RelativeToSelf && t.ScaleKeys.Count > 0)
                        scl = Vector3.Scale(t.ScaleKeys[t.ScaleKeys.Count - 1].Value, scl);
                    t.ScaleKeys.Add(new Key<Vector3> { Time = kf.Time, Value = scl, Channel = kf.Scale });
                    used = true;
                }

                if (used)
                {
                    hasAnyTrack = true;
                    if (kf.Time > totalDuration) totalDuration = kf.Time;
                }
            }
        }

        eventList.Clear();
        foreach (var ev in Asset.GetSortedEventKeyframes()) // already sorted by time
        {
            if (ev == null) continue;
            eventList.Add(ev);
            if (ev.Time > totalDuration) totalDuration = ev.Time;
        }
        nextEvent = 0;
    }

    /// <summary>Time of the last key or event that does anything (0 if the animation is empty).</summary>
    public float Duration { get { return totalDuration; } }

    /// <summary>
    /// Poses the objects as they would be at animation time t, without playing.
    /// Used for scrubbing and the editor preview; works without an animator. Never fires events.
    /// </summary>
    public void Sample(float t)
    {
        if (!hasAnyTrack) return;
        Apply(t);
    }

    public void Play()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        nextEvent = 0;
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

    /// <summary>
    /// Fires (in time order) every event whose time has been reached and hasn't fired yet.
    /// A throwing callback is logged and does not stop the animation or the remaining events.
    /// </summary>
    private void FireEventsUpTo(float t)
    {
        while (nextEvent < eventList.Count && eventList[nextEvent].Time <= t)
        {
            var ev = eventList[nextEvent];
            nextEvent++; // advance first so a callback that re-enters can't fire it twice
            if (string.IsNullOrEmpty(ev.Name)) continue;
            try { Invoke(ev.Name); }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }
    }

    private IEnumerator Animation()
    {
        bool hasEvents = eventList.Count > 0;

        // Nothing opted into any channel and no events, so there is nothing to play.
        if (!hasAnyTrack && !hasEvents)
        {
            Stop();
            yield break;
        }

        // Callbacks are registered after Play() returns, but StartCoroutine runs this method up to its
        // first yield immediately. If an event sits at the very start, wait a frame so it can be heard.
        if (hasEvents && eventList[0].Time <= 0.0001f) yield return null;

        if (totalDuration > 0f)
        {
            // One continuous pass over the whole timeline; every channel samples its own track.
            float duration = totalDuration;
            yield return OXLerp.Frame.Linear((float x) =>
            {
                float t = x * duration;
                if (hasAnyTrack) Apply(t);
                FireEventsUpTo(t);
            }, duration);
        }

        // Make sure we land exactly on the final values, and nothing is skipped by a long frame.
        if (hasAnyTrack) Apply(totalDuration);
        FireEventsUpTo(float.PositiveInfinity);

        // Finished naturally.
        routine = null;
        IsPlaying = false;
        if (Asset.ResetAfterFinish) Reset();
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
