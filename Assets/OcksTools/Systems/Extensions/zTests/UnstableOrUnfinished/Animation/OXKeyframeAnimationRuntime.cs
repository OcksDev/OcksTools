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
        /// <summary>
        /// True for the synthetic key added at the end of a looping animation that finishes on an empty keyframe.
        /// It carries the first key's value, so the last real pose eases back into the first pose during the tail.
        /// </summary>
        public bool Closing;
        /// <summary>
        /// Closing key of a non-looping animation: it only describes how the start eases in from the last real key
        /// (Start Last). Normal evaluation ignores it so the animation still ends on the last real keyframe.
        /// </summary>
        public bool Virtual;
    }

    private class Target
    {
        public GameObject Go;
        /// <summary>The object's real transform before the animation touched it. Reset() restores this.</summary>
        public OXTransformWithScale Original = new OXTransformWithScale();
        /// <summary>The base the channel values are applied relative to (identity when OverrideData is on).</summary>
        public OXTransformWithScale Start = new OXTransformWithScale();

        // One independent timeline per channel. Empty list = this animation never touches that channel.
        // Position and scale are split further into one timeline per axis (0 = X, 1 = Y, 2 = Z), so a key
        // that disables an axis simply doesn't appear on that axis' timeline and the axis is left alone.
        public readonly List<Key<float>>[] PosKeys = NewAxisTracks();
        public readonly List<Key<Quaternion>> RotKeys = new List<Key<Quaternion>>();
        public readonly List<Key<float>>[] ScaleKeys = NewAxisTracks();

        private static List<Key<float>>[] NewAxisTracks()
        {
            return new[] { new List<Key<float>>(), new List<Key<float>>(), new List<Key<float>>() };
        }
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
        float lastKeyTime = 0f;    // time of the last keyframe of any kind (empty ones included)
        float lastUsedTime = -1f;  // time of the last real keyframe: one that changes a track's value (spacer/empty keys don't count)

        foreach (var kf in keyframes)
        {
            if (kf == null) continue;
            if (kf.Time > lastKeyTime) lastKeyTime = kf.Time;
            // Every keyframe counts toward the duration, even one with no channels enabled or no object data.
            // That lets an empty key act as "keep playing until here" after the last real motion.
            if (kf.Time > totalDuration) totalDuration = kf.Time;
            if (kf.Data == null) continue;
            foreach (var d in kf.Data)
            {
                if (!TryGetTarget(d, out var t)) continue;
                if (d.Transform == null) continue;

                bool used = false;
                bool changed = false; // did this keyframe change any track's value (or start a track)?
                if (kf.Position != null && kf.Position.Enabled)
                {
                    var pos = d.Transform.Position;
                    for (int a = 0; a < 3; a++)
                    {
                        if (!kf.Position.AxisEnabled(a)) continue;
                        var track = t.PosKeys[a];
                        float v = pos[a];
                        if (kf.Position.RelativeToSelf && track.Count > 0)
                            v = track[track.Count - 1].Value + v;
                        if (track.Count == 0 || Mathf.Abs(v - track[track.Count - 1].Value) > 1e-5f) changed = true;
                        track.Add(new Key<float> { Time = kf.Time, Value = v, Channel = kf.Position });
                        used = true;
                    }
                }
                if (kf.Rotation != null && kf.Rotation.Enabled && kf.Rotation.AnyAxisEnabled)
                {
                    var rot = IsZeroQuat(d.Transform.Rotation) ? Quaternion.identity : d.Transform.Rotation;
                    if (!kf.Rotation.AllAxesEnabled)
                    {
                        // Disabled Euler axes contribute no rotation, so that axis stays at the starting pose.
                        var e = rot.eulerAngles;
                        if (!kf.Rotation.AxisEnabled(0)) e.x = 0f;
                        if (!kf.Rotation.AxisEnabled(1)) e.y = 0f;
                        if (!kf.Rotation.AxisEnabled(2)) e.z = 0f;
                        rot = Quaternion.Euler(e);
                    }
                    if (kf.Rotation.RelativeToSelf && t.RotKeys.Count > 0)
                        rot = t.RotKeys[t.RotKeys.Count - 1].Value * rot;
                    if (t.RotKeys.Count == 0 || Quaternion.Angle(rot, t.RotKeys[t.RotKeys.Count - 1].Value) > 0.001f) changed = true;
                    t.RotKeys.Add(new Key<Quaternion> { Time = kf.Time, Value = rot, Channel = kf.Rotation });
                    used = true;
                }
                if (kf.Scale != null && kf.Scale.Enabled)
                {
                    var scl = d.Transform.Scale;
                    for (int a = 0; a < 3; a++)
                    {
                        if (!kf.Scale.AxisEnabled(a)) continue;
                        var track = t.ScaleKeys[a];
                        float v = scl[a];
                        if (kf.Scale.RelativeToSelf && track.Count > 0)
                            v *= track[track.Count - 1].Value;
                        if (track.Count == 0 || Mathf.Abs(v - track[track.Count - 1].Value) > 1e-5f) changed = true;
                        track.Add(new Key<float> { Time = kf.Time, Value = v, Channel = kf.Scale });
                        used = true;
                    }
                }

                if (used)
                {
                    hasAnyTrack = true;
                    if (kf.Time > totalDuration) totalDuration = kf.Time;
                    // A key that only repeats the previous value is a spacer, not a "real" key (see the loop seam below).
                    if (changed && kf.Time > lastUsedTime) lastUsedTime = kf.Time;
                }
            }
        }

        // Looping animation that ends on empty keyframe(s) (no channels, or keys that only repeat the previous value):
        // the last keyframe is where the loop closes. Empty keys are ignored, and the pose eases from the last REAL
        // key to the first real key, arriving exactly at the closing time, so there is no pause and no snap.
        // Start Last needs the same structure (even with no empty tail, or with Loop off) so the first pass can begin
        // part-way along the return from the last real key.
        bool hasTail = lastKeyTime > lastUsedTime + 0.0001f;
        if (hasAnyTrack && lastUsedTime >= 0f && ((Asset.Loop && hasTail) || Asset.StartLast))
        {
            bool isVirtual = !Asset.Loop;
            foreach (var tar in targets)
            {
                SealLoop(tar.RotKeys, lastUsedTime, lastKeyTime, isVirtual);
                for (int a = 0; a < 3; a++)
                {
                    SealLoop(tar.PosKeys[a], lastUsedTime, lastKeyTime, isVirtual);
                    SealLoop(tar.ScaleKeys[a], lastUsedTime, lastKeyTime, isVirtual);
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

    /// <summary>
    /// Makes one track loop back on itself: drops trailing spacer keys after the last real key, makes sure the track
    /// holds its last value until the last real key's time (so every track starts returning at the same moment),
    /// then adds a closing key holding the first key's value. The loop is cyclic: the next pass reaches the first key
    /// at (closeTime + the first key's time), so that is where the closing key sits. At closeTime (the end of the
    /// pass) the track is therefore only part-way back, and the next pass carries on from exactly that pose
    /// (see WrapX). The return is eased with the first key's own interpolation settings.
    /// </summary>
    private static void SealLoop<T>(List<Key<T>> keys, float lastRealTime, float closeTime, bool isVirtual)
    {
        if (keys.Count == 0) return;
        while (keys.Count > 1 && keys[keys.Count - 1].Time > lastRealTime + 0.0001f) keys.RemoveAt(keys.Count - 1);
        var last = keys[keys.Count - 1];
        if (last.Time < lastRealTime - 0.0001f)
            keys.Add(new Key<T> { Time = lastRealTime, Value = last.Value, Channel = last.Channel });
        var first = keys[0];
        keys.Add(new Key<T> { Time = closeTime + first.Time, Value = first.Value, Channel = first.Channel, Closing = true, Virtual = isVirtual });
    }

    /// <summary>Time of the last keyframe (empty ones included) or event (0 if the animation has none).</summary>
    public float Duration { get { return totalDuration; } }

    /// <summary>
    /// Poses the objects as they would be at animation time t, without playing.
    /// Used for scrubbing and the editor preview; works without an animator. Never fires events.
    /// Pass wrapped = true to show a later pass of a looping animation (see Apply).
    /// </summary>
    public void Sample(float t, bool wrapped = false)
    {
        if (!hasAnyTrack) return;
        Apply(t, wrapped);
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
            // Only axes this animation actually animated are restored; masked-off axes were never touched.
            var p = t.Go.transform.localPosition;
            var s = t.Go.transform.localScale;
            for (int a = 0; a < 3; a++)
            {
                if (t.PosKeys[a].Count > 0) p[a] = t.Original.Position[a];
                if (t.ScaleKeys[a].Count > 0) s[a] = t.Original.Scale[a];
            }
            t.Go.transform.localPosition = p;
            t.Go.transform.localScale = s;
            if (t.RotKeys.Count > 0) t.Go.transform.localRotation = t.Original.Rotation;
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
    private static int NextKeyIndex<T>(List<Key<T>> keys, float t, int count)
    {
        for (int i = 0; i < count; i++)
            if (keys[i].Time > t) return i;
        return count;
    }

    /// <summary>Number of keys normal evaluation uses: a virtual closing key (Start Last without Loop) is left out.</summary>
    private static int UsedCount<T>(List<Key<T>> keys)
    {
        int n = keys.Count;
        return n > 0 && keys[n - 1].Closing && keys[n - 1].Virtual ? n - 1 : n;
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

    /// <summary>
    /// Eased progress of the return segment while a later pass of a seamless loop approaches its first key.
    /// The segment runs from the last real key (one loop length earlier) to the first key, the same segment the
    /// previous pass was in the middle of when it ended, so the pose carries on without a jump or a pause.
    /// </summary>
    private static float WrapX<T>(List<Key<T>> keys, float t)
    {
        float loopLength = keys[keys.Count - 1].Time - keys[0].Time;
        float fromTime = keys[keys.Count - 2].Time - loopLength;
        float dt = keys[0].Time - fromTime;
        float x = dt <= 0.0001f ? 1f : Mathf.Clamp01((t - fromTime) / dt);
        return keys[0].Channel.Evaluate(x);
    }

    private static float EvalFloat(List<Key<float>> keys, float t, float identity, bool wrapped)
    {
        int n = UsedCount(keys);
        int i = NextKeyIndex(keys, t, n);
        if (i >= n) return keys[n - 1].Value; // hold last value
        // On a later pass of a seamless loop the first key is approached from where the last pass ended, not from the start pose.
        if (i == 0 && wrapped && keys[keys.Count - 1].Closing)
            return Mathf.LerpUnclamped(keys[keys.Count - 2].Value, keys[0].Value, WrapX(keys, t));
        float prev = i == 0 ? identity : keys[i - 1].Value;
        return Mathf.LerpUnclamped(prev, keys[i].Value, SegmentX(keys, i, t));
    }

    private static Quaternion EvalRotation(List<Key<Quaternion>> keys, float t, bool wrapped)
    {
        int n = UsedCount(keys);
        int i = NextKeyIndex(keys, t, n);
        if (i >= n) return keys[n - 1].Value;
        if (i == 0 && wrapped && keys[keys.Count - 1].Closing)
            return keys[keys.Count - 2].Value.SlerpU(keys[0].Value, WrapX(keys, t));
        Quaternion prev = i == 0 ? Quaternion.identity : keys[i - 1].Value;
        return prev.SlerpU(keys[i].Value, SegmentX(keys, i, t));
    }

    /// <summary>
    /// Evaluates every channel track at animation time t and writes it to the objects.
    /// wrapped = this is a repeat pass of a loop, or the first pass with Start Last. It only matters for tracks with a
    /// closing key, which start the pass part-way along the return from the last real key so there is no snap.
    /// </summary>
    private void Apply(float t, bool wrapped)
    {
        foreach (var tar in targets)
        {
            if (tar.Go == null) continue;
            var tr = tar.Go.transform;

            // Position / scale: write only the axes that have a timeline, leave the rest as they are.
            if (tar.PosKeys[0].Count > 0 || tar.PosKeys[1].Count > 0 || tar.PosKeys[2].Count > 0)
            {
                var p = tr.localPosition;
                for (int a = 0; a < 3; a++)
                    if (tar.PosKeys[a].Count > 0)
                        p[a] = tar.Start.Position[a] + EvalFloat(tar.PosKeys[a], t, 0f, wrapped);
                tr.localPosition = p;
            }

            if (tar.RotKeys.Count > 0)
                tr.localRotation = tar.Start.Rotation * EvalRotation(tar.RotKeys, t, wrapped);

            if (tar.ScaleKeys[0].Count > 0 || tar.ScaleKeys[1].Count > 0 || tar.ScaleKeys[2].Count > 0)
            {
                var s = tr.localScale;
                for (int a = 0; a < 3; a++)
                    if (tar.ScaleKeys[a].Count > 0)
                        s[a] = EvalFloat(tar.ScaleKeys[a], t, 1f, wrapped) * tar.Start.Scale[a];
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
        if (!hasAnyTrack && !hasEvents && totalDuration <= 0f)
        {
            Stop();
            yield break;
        }

        // Callbacks are registered after Play() returns, but StartCoroutine runs this method up to its
        // first yield immediately. If an event sits at the very start, wait a frame so it can be heard.
        if (hasEvents && eventList[0].Time <= 0.0001f) yield return null;

        // A zero-length animation can't loop (it would spin without ever advancing time).
        bool loop = Asset.Loop && totalDuration > 0.0001f;

        int pass = 0;
        do
        {
            nextEvent = 0; // every pass re-fires the events
            // Start Last: even the first pass begins part-way along the return from the last real key.
            bool wrapped = pass > 0 || Asset.StartLast;

            if (totalDuration > 0f)
            {
                // One continuous pass over the whole timeline; every channel samples its own track.
                float duration = totalDuration;
                yield return OXLerp.Frame.Linear((float x) =>
                {
                    float t = x * duration;
                    if (hasAnyTrack) Apply(t, wrapped);
                    FireEventsUpTo(t);
                }, duration);
            }

            // Make sure we land exactly on the final values, and nothing is skipped by a long frame.
            if (hasAnyTrack) Apply(totalDuration, wrapped);
            FireEventsUpTo(float.PositiveInfinity);

            // Looping only ends through Stop(); always give a frame back before starting the next pass.
            if (loop) yield return null;
            pass++;
        } while (loop);

        // Finished naturally.
        routine = null;
        IsPlaying = false;
        if (Asset.ResetAfterFinish) Reset();
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
