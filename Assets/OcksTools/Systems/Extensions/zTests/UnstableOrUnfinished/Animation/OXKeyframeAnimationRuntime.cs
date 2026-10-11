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

        /// <summary>The object's pose when this animation began. A fade blends away from the outgoing animation's pose, which starts here.</summary>
        public readonly OXTransformWithScale FadeFrom = new OXTransformWithScale();

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

    // ---- Fading (crossfade from the animation that was playing on the same animator) ----
    // The outgoing animation keeps running (time, loops, events) but no longer writes to the objects: every frame this
    // animation poses it, then blends its own pose over the top with a weight that rises 0 -> 1 over fadeDuration.
    private OXKeyframeAnimationRuntime outgoing;
    private float fadeStartTime;
    private float fadeDuration;
    private bool isFadingOut;      // a newer animation is blending over this one and does the posing
    private float curTime;         // last animation time this runtime was ticked at (an outgoing animation is posed from it)
    private bool curWrapped;

    /// <summary>True while this animation is still blending in over the one it replaced.</summary>
    public bool IsFading { get { return outgoing != null; } }

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
        : this(asset, animator, objects, null, 0f) { }

    /// <param name="outgoing">The animation currently playing that this one fades in over (null = no fade).</param>
    /// <param name="fadeTime">Seconds the fade takes. Ignored when 0 or when there is no outgoing animation.</param>
    public OXKeyframeAnimationRuntime(OXKeyframeAnimation asset, OXKeyframeAnimator animator, IList<GameObject> objects,
        OXKeyframeAnimationRuntime outgoing, float fadeTime)
    {
        Asset = asset;
        this.animator = animator;
        if (outgoing != null && fadeTime > 0.0001f)
        {
            this.outgoing = outgoing;
            outgoing.isFadingOut = true;
            fadeDuration = fadeTime;
            fadeStartTime = Time.time;
        }
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

            CopyState(t.Original, t.FadeFrom);
            // While fading, the object is mid-way through the outgoing animation, so "current" is not its rest pose.
            // Take the rest pose from the outgoing animation for every channel/axis it controls.
            if (outgoing != null) TakeRestFromOutgoing(t);

            if (Asset.OverrideData) ResetToIdentity(t.Start);
            else CopyState(t.Original, t.Start);
        }
        return t;
    }

    private void TakeRestFromOutgoing(Target t)
    {
        var p = t.Original.Position;
        var s = t.Original.Scale;
        for (int a = 0; a < 3; a++)
        {
            if (outgoing.TryGetRestPosition(t.Go, a, out float pv)) p[a] = pv;
            if (outgoing.TryGetRestScale(t.Go, a, out float sv)) s[a] = sv;
        }
        t.Original.Position = p;
        t.Original.Scale = s;
        if (outgoing.TryGetRestRotation(t.Go, out Quaternion q)) t.Original.Rotation = q;
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
        float lastUsedTime = -1f;  // time of the last real keyframe: one with at least one channel (and axis) enabled for a valid object. Keys with nothing enabled are empty and don't count

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
                        track.Add(new Key<float> { Time = kf.Time, Value = v, Channel = kf.Scale });
                        used = true;
                    }
                }

                if (used)
                {
                    hasAnyTrack = true;
                    if (kf.Time > totalDuration) totalDuration = kf.Time;
                    // Any enabled channel makes this a real key, even if it only holds the previous value (a deliberate hold).
                    // Only keys with no channel enabled are empty spacers (see the loop seam below).
                    if (kf.Time > lastUsedTime) lastUsedTime = kf.Time;
                }
            }
        }

        // Looping animation that ends on empty keyframe(s) (no channels enabled):
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
        Apply(t, wrapped, 1f);
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
        // Stopping mid-fade also stops the animation being faded out; the objects stay at the pose they are in.
        if (outgoing != null)
        {
            var o = outgoing;
            outgoing = null;
            o.Stop();
        }
    }

    /// <summary>
    /// Puts every animated channel back to the value it had when this animation was created.
    /// Channels this animation never touched are left alone.
    /// Does not stop playback; use StopAndReset() if the animation may still be running.
    /// </summary>
    public void Reset()
    {
        if (outgoing != null) outgoing.Reset();
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
        // Reset the whole fade chain, oldest first, so every channel ends up at its rest pose.
        var chain = new List<OXKeyframeAnimationRuntime>();
        for (var r = this; r != null; r = r.outgoing) chain.Add(r);
        Stop();
        for (int i = chain.Count - 1; i >= 0; i--) chain[i].Reset();
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
    /// weight &lt; 1 blends the result over whatever the objects currently show (used while fading in).
    /// </summary>
    private void Apply(float t, bool wrapped, float weight)
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
                        p[a] = BlendFloat(p[a], tar.Start.Position[a] + EvalFloat(tar.PosKeys[a], t, 0f, wrapped), weight);
                tr.localPosition = p;
            }

            if (tar.RotKeys.Count > 0)
                tr.localRotation = BlendRotation(tr.localRotation, tar.Start.Rotation * EvalRotation(tar.RotKeys, t, wrapped), weight);

            if (tar.ScaleKeys[0].Count > 0 || tar.ScaleKeys[1].Count > 0 || tar.ScaleKeys[2].Count > 0)
            {
                var s = tr.localScale;
                for (int a = 0; a < 3; a++)
                    if (tar.ScaleKeys[a].Count > 0)
                        s[a] = BlendFloat(s[a], EvalFloat(tar.ScaleKeys[a], t, 1f, wrapped) * tar.Start.Scale[a], weight);
                tr.localScale = s;
            }
        }
    }

    private static float BlendFloat(float from, float to, float weight)
    {
        return weight >= 0.9999f ? to : Mathf.LerpUnclamped(from, to, weight);
    }

    private static Quaternion BlendRotation(Quaternion from, Quaternion to, float weight)
    {
        return weight >= 0.9999f ? to : Quaternion.Slerp(from, to, weight);
    }

    /// <summary>Called every frame instead of Apply. An outgoing animation only records its time: the animation fading in poses it.</summary>
    private void Tick(float t, bool wrapped)
    {
        curTime = t;
        curWrapped = wrapped;
        if (isFadingOut) return;
        if (hasAnyTrack || outgoing != null) ApplyBlended(t, wrapped);
    }

    /// <summary>
    /// Poses the objects for this animation, blending over the outgoing animation while a fade is running.
    /// Order each frame: put the channels this animation controls back to where they were when it began, let the
    /// outgoing animation pose them (it may itself still be fading), ease the channels only the outgoing animation
    /// controls back to their rest pose, then blend this animation's pose over the top.
    /// </summary>
    private void ApplyBlended(float t, bool wrapped)
    {
        float w = 1f;
        if (outgoing != null)
        {
            w = FadeWeight();
            if (w >= 1f)
            {
                ReturnUntouchedToRest(1f); // the outgoing animation is about to stop: leave its exclusive channels at rest
                EndFade();
            }
            else
            {
                RestoreFadeFrom();
                outgoing.ApplyBlended(outgoing.curTime, outgoing.curWrapped);
                w = w * w * (3f - 2f * w); // smoothstep, so the blend eases in and out
                ReturnUntouchedToRest(w);
            }
        }
        Apply(t, wrapped, w);
    }

    private float FadeWeight()
    {
        if (outgoing == null || fadeDuration <= 0.0001f) return 1f;
        return Mathf.Clamp01((Time.time - fadeStartTime) / fadeDuration);
    }

    /// <summary>The fade is over: the outgoing animation is stopped (not reset) and this one owns the objects.</summary>
    private void EndFade()
    {
        var o = outgoing;
        outgoing = null;
        if (o != null) o.Stop();
    }

    private static readonly HashSet<GameObject> fadeScratch = new HashSet<GameObject>();

    /// <summary>
    /// Channels/axes (and whole objects) that the outgoing animation controls but this one has no keys for: this
    /// animation does not drive them, so they ease back to their rest pose over the fade, exactly where a plain
    /// Play() (stop and reset the old animation) would have put them. Called after the outgoing animation has posed
    /// the objects, so "current" is its pose; at weight 1 they land on rest.
    /// </summary>
    private void ReturnUntouchedToRest(float weight)
    {
        if (outgoing == null) return;
        fadeScratch.Clear();
        for (var r = outgoing; r != null; r = r.outgoing)
            foreach (var ot in r.targets)
                if (ot.Go != null) fadeScratch.Add(ot.Go);

        foreach (var go in fadeScratch)
        {
            var mine = FindTarget(go); // null when this animation doesn't use the object at all
            var tr = go.transform;

            var p = tr.localPosition;
            var s = tr.localScale;
            bool pChanged = false, sChanged = false;
            for (int a = 0; a < 3; a++)
            {
                if ((mine == null || mine.PosKeys[a].Count == 0) && outgoing.TryGetRestPosition(go, a, out float pv))
                {
                    p[a] = BlendFloat(p[a], pv, weight);
                    pChanged = true;
                }
                if ((mine == null || mine.ScaleKeys[a].Count == 0) && outgoing.TryGetRestScale(go, a, out float sv))
                {
                    s[a] = BlendFloat(s[a], sv, weight);
                    sChanged = true;
                }
            }
            if (pChanged) tr.localPosition = p;
            if (sChanged) tr.localScale = s;
            if ((mine == null || mine.RotKeys.Count == 0) && outgoing.TryGetRestRotation(go, out Quaternion q))
                tr.localRotation = BlendRotation(tr.localRotation, q, weight);
        }
    }

    /// <summary>Puts the channels this animation drives back to their pose at the start of the fade, so the outgoing animation poses from a clean slate.</summary>
    private void RestoreFadeFrom()
    {
        foreach (var tar in targets)
        {
            if (tar.Go == null) continue;
            var tr = tar.Go.transform;
            if (tar.PosKeys[0].Count > 0 || tar.PosKeys[1].Count > 0 || tar.PosKeys[2].Count > 0)
            {
                var p = tr.localPosition;
                for (int a = 0; a < 3; a++)
                    if (tar.PosKeys[a].Count > 0) p[a] = tar.FadeFrom.Position[a];
                tr.localPosition = p;
            }
            if (tar.RotKeys.Count > 0) tr.localRotation = tar.FadeFrom.Rotation;
            if (tar.ScaleKeys[0].Count > 0 || tar.ScaleKeys[1].Count > 0 || tar.ScaleKeys[2].Count > 0)
            {
                var s = tr.localScale;
                for (int a = 0; a < 3; a++)
                    if (tar.ScaleKeys[a].Count > 0) s[a] = tar.FadeFrom.Scale[a];
                tr.localScale = s;
            }
        }
    }

    private Target FindTarget(GameObject go)
    {
        foreach (var t in targets) if (t.Go == go) return t;
        return null;
    }

    // Rest pose of a channel/axis as this animation (or, failing that, the animation it replaced) knows it.
    private bool TryGetRestPosition(GameObject go, int axis, out float v)
    {
        var t = FindTarget(go);
        if (t != null && t.PosKeys[axis].Count > 0) { v = t.Original.Position[axis]; return true; }
        if (outgoing != null) return outgoing.TryGetRestPosition(go, axis, out v);
        v = 0f; return false;
    }

    private bool TryGetRestScale(GameObject go, int axis, out float v)
    {
        var t = FindTarget(go);
        if (t != null && t.ScaleKeys[axis].Count > 0) { v = t.Original.Scale[axis]; return true; }
        if (outgoing != null) return outgoing.TryGetRestScale(go, axis, out v);
        v = 0f; return false;
    }

    private bool TryGetRestRotation(GameObject go, out Quaternion q)
    {
        var t = FindTarget(go);
        if (t != null && t.RotKeys.Count > 0) { q = t.Original.Rotation; return true; }
        if (outgoing != null) return outgoing.TryGetRestRotation(go, out q);
        q = Quaternion.identity; return false;
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
            // Nothing to fade in: behave like a plain Play() and put whatever was playing back at rest.
            if (outgoing != null)
            {
                var o = outgoing;
                outgoing = null;
                o.StopAndReset();
            }
            Stop();
            yield break;
        }

        // Callbacks are registered after Play() returns, but StartCoroutine runs this method up to its
        // first yield immediately. If an event sits at the very start, wait a frame so it can be heard.
        if (hasEvents && eventList[0].Time <= 0.0001f) yield return null;

        // A zero-length animation can't loop (it would spin without ever advancing time).
        bool loop = Asset.Loop && totalDuration > 0.0001f;

        int pass = 0;
        bool wrapped = false;
        do
        {
            nextEvent = 0; // every pass re-fires the events
            // Start Last: even the first pass begins part-way along the return from the last real key.
            wrapped = pass > 0 || Asset.StartLast;

            if (totalDuration > 0f)
            {
                // One continuous pass over the whole timeline; every channel samples its own track.
                float duration = totalDuration;
                yield return OXLerp.Frame.Linear((float x) =>
                {
                    float t = x * duration;
                    Tick(t, wrapped);
                    FireEventsUpTo(t);
                }, duration);
            }

            // Make sure we land exactly on the final values, and nothing is skipped by a long frame.
            Tick(totalDuration, wrapped);
            FireEventsUpTo(float.PositiveInfinity);

            // Looping only ends through Stop(); always give a frame back before starting the next pass.
            if (loop) yield return null;
            pass++;
        } while (loop);

        // Keep blending until the fade is over, even when this animation is shorter than the fade.
        while (outgoing != null)
        {
            yield return null;
            Tick(totalDuration, wrapped);
        }

        // Finished naturally.
        routine = null;
        IsPlaying = false;
        // An animation that was faded out leaves the objects to the one that replaced it.
        if (Asset.ResetAfterFinish && !isFadingOut) Reset();
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
