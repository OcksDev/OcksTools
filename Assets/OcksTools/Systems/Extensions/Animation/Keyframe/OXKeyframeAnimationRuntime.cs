using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimationRuntime
{
    /// <summary>
    /// Multiplies the playback rate (2 = twice as fast, 0.5 = half speed, 0 = paused). Read every frame, so it can change
    /// mid-playback. A negative value plays backwards: if playback starts with a negative speed it begins at the end of
    /// the animation and runs toward the start (events fire in reverse order). Flipping the sign mid-playback reverses
    /// direction from the current position. Fade time is always real seconds and is not affected.
    /// </summary>
    public float PlaybackSpeed = 1;
    private const float Eps = 0.0001f;

    /// <summary>
    /// A rotation key value. Euler keys (IsEuler) carry unwrapped degrees, so 0 -> 360 spins a full turn and 0 -> -720 spins
    /// twice the other way. Legacy quaternion keys keep slerping the short way. Q is always valid (Euler keys store Quaternion.Euler(E)).
    /// </summary>
    private struct RotValue
    {
        public Quaternion Q;
        public Vector3 E;
        public bool IsEuler;

        public static RotValue FromEuler(Vector3 e) => new RotValue { Q = Quaternion.Euler(e), E = e, IsEuler = true };
        public static RotValue FromQuat(Quaternion q) => new RotValue { Q = q, E = Vector3.zero, IsEuler = false };
        /// <summary>The pose before the first key. Counts as Euler zero so the first key can spin from rest.</summary>
        public static RotValue Identity => FromEuler(Vector3.zero);

        /// <summary>Euler to Euler goes straight through the unwrapped angles; anything else slerps the short way.</summary>
        public static Quaternion Lerp(RotValue a, RotValue b, float x)
        {
            if (a.IsEuler && b.IsEuler) return Quaternion.Euler(Vector3.LerpUnclamped(a.E, b.E, x));
            return a.Q.SlerpU(b.Q, x);
        }
    }

    private struct Key<T>
    {
        public float Time;
        public T Value;
        public OXKeyframeChannel Channel;
        /// <summary>Synthetic key closing a loop; carries the first key's value so the last pose eases back into it.</summary>
        public bool Closing;
        /// <summary>Closing key of a non-looping Start Last animation: only shapes the start, ignored by normal evaluation.</summary>
        public bool Virtual;
    }

    /// <summary>
    /// Keys of one channel/axis. Evaluation walks a cursor from where the last lookup ended, so steady playback
    /// is O(1) per frame instead of a scan from the first key.
    /// </summary>
    private sealed class Track<T>
    {
        public Key<T>[] Items = new Key<T>[4];
        public int Count;
        /// <summary>Keys normal evaluation uses: a virtual closing key is left out. Valid after Finish().</summary>
        public int Used;
        private int cursor;

        public void Add(Key<T> k)
        {
            if (Count == Items.Length) System.Array.Resize(ref Items, Count * 2);
            Items[Count++] = k;
        }

        public void Finish()
        {
            Used = Count > 0 && Items[Count - 1].Closing && Items[Count - 1].Virtual ? Count - 1 : Count;
            cursor = 0;
        }

        /// <summary>Index of the first used key strictly after t, or Used if t is at/after the last.</summary>
        public int Seek(float t)
        {
            var items = Items;
            int n = Used;
            int i = cursor > n ? n : cursor;
            while (i > 0 && items[i - 1].Time > t) i--;   // seeking backwards (loop restart, Sample)
            while (i < n && items[i].Time <= t) i++;      // normal forward playback
            cursor = i;
            return i;
        }
    }

    private sealed class Target
    {
        public GameObject Go;
        public Transform Tr;

        /// <summary>The object's transform before the animation touched it; Reset() restores this.</summary>
        public Vector3 OrigPos, OrigScale;
        public Quaternion OrigRot;
        /// <summary>Base the channel values apply relative to (identity when OverrideData is on).</summary>
        public Vector3 StartPos, StartScale;
        public Quaternion StartRot;
        /// <summary>Pose when this animation began; a fade re-poses the outgoing animation from here.</summary>
        public Vector3 FadeFromPos, FadeFromScale;
        public Quaternion FadeFromRot;

        // One timeline per channel; position and scale are split per axis. Null/zero mask = channel/axis untouched.
        public Track<float>[] Pos;
        public Track<RotValue> Rot;
        public Track<float>[] Scale;

        /// <summary>Bit per axis (x=1, y=2, z=4) that has keys.</summary>
        public int PosMask, ScaleMask;
        public bool HasRot;
        /// <summary>True if this target has any track at all (and a valid transform).</summary>
        public bool Active;

        public void Finish()
        {
            PosMask = FinishAxes(Pos);
            ScaleMask = FinishAxes(Scale);
            HasRot = Rot != null && Rot.Count > 0;
            if (HasRot) Rot.Finish();
            Active = Tr != null && (PosMask != 0 || ScaleMask != 0 || HasRot);
        }

        private static int FinishAxes(Track<float>[] tracks)
        {
            if (tracks == null) return 0;
            int mask = 0;
            for (int a = 0; a < 3; a++)
            {
                var tr = tracks[a];
                if (tr == null || tr.Count == 0) continue;
                tr.Finish();
                mask |= 1 << a;
            }
            return mask;
        }
    }

    /// <summary>Resolved once per fade: a channel the outgoing animation drives that this one doesn't, and its rest value.</summary>
    private sealed class FadeEntry
    {
        public Transform Tr;
        public int PosMask, ScaleMask;
        public bool Rot;
        public Vector3 RestPos, RestScale;
        public Quaternion RestRot;
    }

    private static readonly List<OXEventKeyframe> NoEvents = new List<OXEventKeyframe>(0);
    private static readonly HashSet<GameObject> fadeScratch = new HashSet<GameObject>();
    private static readonly List<OXKeyframeAnimationRuntime> chainScratch = new List<OXKeyframeAnimationRuntime>();

    public readonly OXKeyframeAnimation Asset;
    public bool IsPlaying { get; private set; }

    private readonly OXKeyframeAnimator animator;
    private readonly Target[] targets;
    private Dictionary<GameObject, Target> targetLookup; // built lazily, only for large target sets
    private Coroutine routine;
    private float totalDuration;
    private bool hasAnyTrack;

    // Fading: the outgoing animation keeps running (time, loops, events) but this one poses the objects,
    // blending its pose over the outgoing one with a weight rising 0 -> 1 over fadeDuration.
    private OXKeyframeAnimationRuntime outgoing;
    private float fadeStartTime;
    private float fadeDuration;
    private bool isFadingOut;
    private float curTime;
    private bool curWrapped;
    private FadeEntry[] fadeEntries;

    public bool IsFading => outgoing != null;

    private List<OXEventKeyframe> eventList = NoEvents;
    /// <summary>Next event to fire: counts up while playing forward, down while playing backward.</summary>
    private int nextEvent;

    /// <summary>Callbacks by event name, invoked when an event keyframe with a matching Name is reached.</summary>
    public Dictionary<string, OXEvent> Events = new();

    /// <summary>Registers an OXEvent under this name, replacing any existing one.</summary>
    public void Append(string a, OXEvent b) => Events[a] = b;

    /// <summary>Registers a callback under this name; repeat calls add more callbacks.</summary>
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
        : this(asset, animator, objects, null, 0f, 1f) { }

    /// <param name="outgoing">Animation currently playing that this one fades in over (null = no fade).</param>
    /// <param name="fadeTime">Fade seconds; ignored when 0 or without an outgoing animation.</param>
    public OXKeyframeAnimationRuntime(OXKeyframeAnimation asset, OXKeyframeAnimator animator, IList<GameObject> objects,
        OXKeyframeAnimationRuntime outgoing, float fadeTime, float speed)
    {
        Asset = asset;
        this.animator = animator;
        if (outgoing != null && fadeTime > Eps)
        {
            this.outgoing = outgoing;
            outgoing.isFadingOut = true;
            fadeDuration = fadeTime;
            fadeStartTime = Time.time;
        }
        PlaybackSpeed = speed;
        int count = objects.Count;
        targets = new Target[count];
        for (int i = 0; i < count; i++) targets[i] = CreateTarget(objects[i]);
        BuildTracks();
    }

    private Target CreateTarget(GameObject go)
    {
        var t = new Target { Go = go };
        if (go == null) return t;

        var tr = go.transform;
        t.Tr = tr;
        t.OrigPos = tr.localPosition;
        t.OrigRot = tr.localRotation;
        t.OrigScale = tr.localScale;

        t.FadeFromPos = t.OrigPos;
        t.FadeFromRot = FixQuat(t.OrigRot);
        t.FadeFromScale = t.OrigScale;
        // Mid-fade the object isn't at rest, so take rest from the outgoing animation for the channels it controls.
        if (outgoing != null) TakeRestFromOutgoing(t);

        if (Asset.OverrideData)
        {
            t.StartPos = Vector3.zero;
            t.StartRot = Quaternion.identity;
            t.StartScale = Vector3.one;
        }
        else
        {
            t.StartPos = t.OrigPos;
            t.StartRot = FixQuat(t.OrigRot);
            t.StartScale = t.OrigScale;
        }
        return t;
    }

    private void TakeRestFromOutgoing(Target t)
    {
        var p = t.OrigPos;
        var s = t.OrigScale;
        for (int a = 0; a < 3; a++)
        {
            if (outgoing.TryGetRestPosition(t.Go, a, out float pv)) p[a] = pv;
            if (outgoing.TryGetRestScale(t.Go, a, out float sv)) s[a] = sv;
        }
        t.OrigPos = p;
        t.OrigScale = s;
        if (outgoing.TryGetRestRotation(t.Go, out Quaternion q)) t.OrigRot = q;
    }

    /// <summary>
    /// Splits the keyframe timeline into one track per object per channel (per axis for position/scale) and
    /// snapshots the events. Every keyframe, even an empty one, extends the duration.
    /// </summary>
    private void BuildTracks()
    {
        totalDuration = 0f;
        hasAnyTrack = false;
        float lastKeyTime = 0f;   // last keyframe of any kind
        float lastUsedTime = -1f; // last keyframe with at least one enabled channel/axis on a valid object

        if (Asset.Keyframes.Count > 0)
        {
            var sorted = Asset.GetSortedKeyframes();
            for (int k = 0; k < sorted.Count; k++)
            {
                var kf = sorted[k];
                if (kf == null) continue;
                if (kf.Time > lastKeyTime) lastKeyTime = kf.Time;
                if (kf.Time > totalDuration) totalDuration = kf.Time;
                if (kf.Data == null) continue;

                var data = kf.Data;
                for (int di = 0; di < data.Count; di++)
                {
                    var d = data[di];
                    if (!TryGetTarget(d, out var t) || d.Transform == null) continue;

                    bool used = false;
                    if (kf.Position != null && kf.Position.Enabled)
                        used |= AddAxisKeys(ref t.Pos, kf, kf.Position, d.Transform.Position, false);
                    if (kf.Rotation != null && kf.Rotation.Enabled && kf.Rotation.AnyAxisEnabled)
                    {
                        AddRotationKey(t, kf, d);
                        used = true;
                    }
                    if (kf.Scale != null && kf.Scale.Enabled)
                        used |= AddAxisKeys(ref t.Scale, kf, kf.Scale, d.Transform.Scale, true);

                    if (used)
                    {
                        hasAnyTrack = true;
                        if (kf.Time > lastUsedTime) lastUsedTime = kf.Time;
                    }
                }
            }
        }

        // A loop ending on empty keyframe(s), or Start Last, needs a closing key so the pose eases from the last
        // real key to the first with no pause and no snap.
        bool hasTail = lastKeyTime > lastUsedTime + Eps;
        if (hasAnyTrack && lastUsedTime >= 0f && ((Asset.Loop && hasTail) || Asset.StartLast))
        {
            bool isVirtual = !Asset.Loop;
            for (int i = 0; i < targets.Length; i++)
            {
                var tar = targets[i];
                SealLoop(tar.Rot, lastUsedTime, lastKeyTime, isVirtual);
                for (int a = 0; a < 3; a++)
                {
                    if (tar.Pos != null) SealLoop(tar.Pos[a], lastUsedTime, lastKeyTime, isVirtual);
                    if (tar.Scale != null) SealLoop(tar.Scale[a], lastUsedTime, lastKeyTime, isVirtual);
                }
            }
        }

        // Tracks are final: compute masks, used counts and the active flag once.
        for (int i = 0; i < targets.Length; i++) targets[i].Finish();

        eventList = NoEvents;
        if (Asset.Events.Count > 0)
        {
            eventList = Asset.GetSortedEventKeyframes();
            for (int i = eventList.Count - 1; i >= 0; i--)
                if (eventList[i] == null) eventList.RemoveAt(i);
            if (eventList.Count > 0 && eventList[eventList.Count - 1].Time > totalDuration)
                totalDuration = eventList[eventList.Count - 1].Time;
        }
        nextEvent = 0;
    }

    private static bool AddAxisKeys(ref Track<float>[] tracks, OXKeyframe kf, OXKeyframeChannel ch, Vector3 value, bool multiply)
    {
        bool used = false;
        for (int a = 0; a < 3; a++)
        {
            if (!ch.AxisEnabled(a)) continue;
            tracks ??= new Track<float>[3];
            var track = tracks[a] ??= new Track<float>();
            float v = value[a];
            if (ch.RelativeToSelf && track.Count > 0)
            {
                float prev = track.Items[track.Count - 1].Value;
                v = multiply ? v * prev : v + prev;
            }
            track.Add(new Key<float> { Time = kf.Time, Value = v, Channel = ch });
            used = true;
        }
        return used;
    }

    private static void AddRotationKey(Target t, OXKeyframe kf, OXKeyframeObjectState d)
    {
        var ch = kf.Rotation;
        RotValue v;
        if (d.HasRotationEuler)
        {
            // Unwrapped degrees; disabled axes contribute no rotation.
            var e = d.RotationEuler;
            if (!ch.AxisEnabled(0)) e.x = 0f;
            if (!ch.AxisEnabled(1)) e.y = 0f;
            if (!ch.AxisEnabled(2)) e.z = 0f;
            v = RotValue.FromEuler(e);
        }
        else
        {
            var rot = IsZeroQuat(d.Transform.Rotation) ? Quaternion.identity : d.Transform.Rotation;
            if (!ch.AllAxesEnabled)
            {
                // Disabled Euler axes contribute no rotation.
                var e = rot.eulerAngles;
                if (!ch.AxisEnabled(0)) e.x = 0f;
                if (!ch.AxisEnabled(1)) e.y = 0f;
                if (!ch.AxisEnabled(2)) e.z = 0f;
                rot = Quaternion.Euler(e);
            }
            v = RotValue.FromQuat(rot);
        }

        var track = t.Rot ??= new Track<RotValue>();
        if (ch.RelativeToSelf && track.Count > 0)
        {
            var prev = track.Items[track.Count - 1].Value;
            // Euler on Euler accumulates the angles (so +360 keys keep spinning); otherwise compose as before.
            if (prev.IsEuler && v.IsEuler) v = RotValue.FromEuler(prev.E + v.E);
            else v = RotValue.FromQuat(prev.Q * v.Q);
        }
        track.Add(new Key<RotValue> { Time = kf.Time, Value = v, Channel = ch });
    }

    /// <summary>
    /// Makes a track loop: drops trailing spacer keys, holds the last value until the last real key's time, then adds a
    /// closing key holding the first key's value at closeTime + first.Time. At closeTime the track is only part-way back,
    /// and the next pass continues from exactly that pose (see WrapX).
    /// </summary>
    private static void SealLoop<T>(Track<T> keys, float lastRealTime, float closeTime, bool isVirtual)
    {
        if (keys == null || keys.Count == 0) return;
        while (keys.Count > 1 && keys.Items[keys.Count - 1].Time > lastRealTime + Eps) keys.Count--;
        var last = keys.Items[keys.Count - 1];
        if (last.Time < lastRealTime - Eps)
            keys.Add(new Key<T> { Time = lastRealTime, Value = last.Value, Channel = last.Channel });
        var first = keys.Items[0];
        keys.Add(new Key<T> { Time = closeTime + first.Time, Value = first.Value, Channel = first.Channel, Closing = true, Virtual = isVirtual });
    }

    /// <summary>Time of the last keyframe (empty ones included) or event.</summary>
    public float Duration => totalDuration;

    /// <summary>Poses the objects at animation time t without playing. Works without an animator and never fires events.</summary>
    public void Sample(float t, bool wrapped = false)
    {
        if (hasAnyTrack) Apply(t, wrapped, 1f);
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
        if (outgoing != null)
        {
            var o = outgoing;
            outgoing = null;
            fadeEntries = null;
            o.Stop();
        }
    }

    /// <summary>Restores every channel this animation animated to its original value. Does not stop playback.</summary>
    public void Reset()
    {
        outgoing?.Reset();
        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            if (!t.Active) continue;
            var tr = t.Tr;
            if (tr == null) continue;
            if (t.PosMask != 0) tr.localPosition = MergeMasked(tr.localPosition, t.OrigPos, t.PosMask);
            if (t.ScaleMask != 0) tr.localScale = MergeMasked(tr.localScale, t.OrigScale, t.ScaleMask);
            if (t.HasRot) tr.localRotation = t.OrigRot;
        }
    }

    /// <summary>Stops playback, then restores the objects (the whole fade chain, oldest first).</summary>
    public void StopAndReset()
    {
        chainScratch.Clear();
        for (var r = this; r != null; r = r.outgoing) chainScratch.Add(r);
        Stop();
        for (int i = chainScratch.Count - 1; i >= 0; i--) chainScratch[i].Reset();
        chainScratch.Clear();
    }

    private static Quaternion FixQuat(Quaternion q) => IsZeroQuat(q) ? Quaternion.identity : q;

    private static bool IsZeroQuat(Quaternion q) => q.x == 0f && q.y == 0f && q.z == 0f && q.w == 0f;

    private bool TryGetTarget(OXKeyframeObjectState d, out Target t)
    {
        t = null;
        if (d == null || d.ObjectIndex < 0 || d.ObjectIndex >= targets.Length) return false;
        t = targets[d.ObjectIndex];
        return t.Go != null;
    }

    /// <summary>Eased progress through the segment ending at keys[i]; the first key's segment starts at time 0.</summary>
    private static float SegmentX<T>(Track<T> keys, int i, float t)
    {
        var items = keys.Items;
        float prevTime = i == 0 ? 0f : items[i - 1].Time;
        float dt = items[i].Time - prevTime;
        float x = dt <= Eps ? 1f : Mathf.Clamp01((t - prevTime) / dt);
        return items[i].Channel.Evaluate(x);
    }

    /// <summary>Eased progress of the return segment on a later loop pass, continuing where the previous pass ended.</summary>
    private static float WrapX<T>(Track<T> keys, float t)
    {
        var items = keys.Items;
        float loopLength = items[keys.Count - 1].Time - items[0].Time;
        float fromTime = items[keys.Count - 2].Time - loopLength;
        float dt = items[0].Time - fromTime;
        float x = dt <= Eps ? 1f : Mathf.Clamp01((t - fromTime) / dt);
        return items[0].Channel.Evaluate(x);
    }

    private static float EvalFloat(Track<float> keys, float t, float identity, bool wrapped)
    {
        int n = keys.Used;
        int i = keys.Seek(t);
        var items = keys.Items;
        if (i >= n) return items[n - 1].Value;
        if (i == 0 && wrapped && items[keys.Count - 1].Closing)
            return Mathf.LerpUnclamped(items[keys.Count - 2].Value, items[0].Value, WrapX(keys, t));
        float prev = i == 0 ? identity : items[i - 1].Value;
        return Mathf.LerpUnclamped(prev, items[i].Value, SegmentX(keys, i, t));
    }

    private static Quaternion EvalRotation(Track<RotValue> keys, float t, bool wrapped)
    {
        int n = keys.Used;
        int i = keys.Seek(t);
        var items = keys.Items;
        if (i >= n) return items[n - 1].Value.Q;
        if (i == 0 && wrapped && items[keys.Count - 1].Closing)
            return RotValue.Lerp(items[keys.Count - 2].Value, items[0].Value, WrapX(keys, t));
        RotValue prev = i == 0 ? RotValue.Identity : items[i - 1].Value;
        return RotValue.Lerp(prev, items[i].Value, SegmentX(keys, i, t));
    }

    /// <summary>
    /// Evaluates every track at time t and writes it to the objects. wrapped = a repeat loop pass or the first Start Last
    /// pass. weight &lt; 1 blends over the current pose (used while fading in).
    /// </summary>
    private void Apply(float t, bool wrapped, float weight)
    {
        var arr = targets;
        for (int i = 0; i < arr.Length; i++)
        {
            var tar = arr[i];
            if (!tar.Active) continue;
            var tr = tar.Tr;
            if (tr == null) continue;

            if (tar.PosMask != 0) ApplyPosition(tar, tr, t, wrapped, weight);
            if (tar.HasRot) ApplyRotation(tar, tr, t, wrapped, weight);
            if (tar.ScaleMask != 0) ApplyScale(tar, tr, t, wrapped, weight);
        }
    }

    // Writes are skipped when nothing changed (held keys, finished animations): a Transform set is far costlier than a compare.
    private static void ApplyPosition(Target tar, Transform tr, float t, bool wrapped, float w)
    {
        var cur = tr.localPosition;
        var p = cur;
        var tracks = tar.Pos;
        int m = tar.PosMask;
        var s = tar.StartPos;
        if ((m & 1) != 0) p.x = BlendFloat(p.x, s.x + EvalFloat(tracks[0], t, 0f, wrapped), w);
        if ((m & 2) != 0) p.y = BlendFloat(p.y, s.y + EvalFloat(tracks[1], t, 0f, wrapped), w);
        if ((m & 4) != 0) p.z = BlendFloat(p.z, s.z + EvalFloat(tracks[2], t, 0f, wrapped), w);
        if (p.x != cur.x || p.y != cur.y || p.z != cur.z) tr.localPosition = p;
    }

    private static void ApplyScale(Target tar, Transform tr, float t, bool wrapped, float w)
    {
        var cur = tr.localScale;
        var sc = cur;
        var tracks = tar.Scale;
        int m = tar.ScaleMask;
        var s = tar.StartScale;
        if ((m & 1) != 0) sc.x = BlendFloat(sc.x, EvalFloat(tracks[0], t, 1f, wrapped) * s.x, w);
        if ((m & 2) != 0) sc.y = BlendFloat(sc.y, EvalFloat(tracks[1], t, 1f, wrapped) * s.y, w);
        if ((m & 4) != 0) sc.z = BlendFloat(sc.z, EvalFloat(tracks[2], t, 1f, wrapped) * s.z, w);
        if (sc.x != cur.x || sc.y != cur.y || sc.z != cur.z) tr.localScale = sc;
    }

    private static void ApplyRotation(Target tar, Transform tr, float t, bool wrapped, float w)
    {
        var cur = tr.localRotation;
        var q = BlendRotation(cur, tar.StartRot * EvalRotation(tar.Rot, t, wrapped), w);
        if (q.x != cur.x || q.y != cur.y || q.z != cur.z || q.w != cur.w) tr.localRotation = q;
    }

    private static float BlendFloat(float from, float to, float weight)
    {
        return weight >= 0.9999f ? to : Mathf.LerpUnclamped(from, to, weight);
    }

    private static Quaternion BlendRotation(Quaternion from, Quaternion to, float weight)
    {
        return weight >= 0.9999f ? to : Quaternion.Slerp(from, to, weight);
    }

    /// <summary>Overwrites the masked axes of cur with src.</summary>
    private static Vector3 MergeMasked(Vector3 cur, Vector3 src, int mask)
    {
        if ((mask & 1) != 0) cur.x = src.x;
        if ((mask & 2) != 0) cur.y = src.y;
        if ((mask & 4) != 0) cur.z = src.z;
        return cur;
    }

    /// <summary>Blends the masked axes of cur toward rest by weight.</summary>
    private static Vector3 BlendMasked(Vector3 cur, Vector3 rest, int mask, float weight)
    {
        if ((mask & 1) != 0) cur.x = BlendFloat(cur.x, rest.x, weight);
        if ((mask & 2) != 0) cur.y = BlendFloat(cur.y, rest.y, weight);
        if ((mask & 4) != 0) cur.z = BlendFloat(cur.z, rest.z, weight);
        return cur;
    }

    /// <summary>Per-frame update. An outgoing animation only records its time; the animation fading in poses it.</summary>
    private void Tick(float t, bool wrapped)
    {
        curTime = t;
        curWrapped = wrapped;
        if (isFadingOut) return;
        if (hasAnyTrack || outgoing != null) ApplyBlended(t, wrapped);
    }

    /// <summary>
    /// Poses the objects, blending over the outgoing animation during a fade: restore this animation's channels to
    /// their fade-start pose, let the outgoing animation pose, ease its exclusive channels toward rest, then blend ours on top.
    /// </summary>
    private void ApplyBlended(float t, bool wrapped)
    {
        float w = 1f;
        if (outgoing != null)
        {
            w = FadeWeight();
            if (w >= 1f)
            {
                ReturnUntouchedToRest(1f);
                EndFade();
            }
            else
            {
                RestoreFadeFrom();
                outgoing.ApplyBlended(outgoing.curTime, outgoing.curWrapped);
                w = w * w * (3f - 2f * w); // smoothstep
                ReturnUntouchedToRest(w);
            }
        }
        Apply(t, wrapped, w);
    }

    private float FadeWeight()
    {
        if (outgoing == null || fadeDuration <= Eps) return 1f;
        return Mathf.Clamp01((Time.time - fadeStartTime) / fadeDuration);
    }

    /// <summary>The outgoing animation is stopped (not reset) and this one owns the objects.</summary>
    private void EndFade()
    {
        var o = outgoing;
        outgoing = null;
        fadeEntries = null;
        o?.Stop();
    }

    /// <summary>
    /// Channels/axes/objects the outgoing animation controls but this one has no keys for ease back to rest over the
    /// fade, matching where a plain Play() (stop and reset) would put them.
    /// </summary>
    private void ReturnUntouchedToRest(float weight)
    {
        if (outgoing == null) return;
        var entries = fadeEntries ??= BuildFadeEntries();
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            var tr = e.Tr;
            if (tr == null) continue;
            if (e.PosMask != 0) tr.localPosition = BlendMasked(tr.localPosition, e.RestPos, e.PosMask, weight);
            if (e.ScaleMask != 0) tr.localScale = BlendMasked(tr.localScale, e.RestScale, e.ScaleMask, weight);
            if (e.Rot) tr.localRotation = BlendRotation(tr.localRotation, e.RestRot, weight);
        }
    }

    /// <summary>One-time resolve of what the fade has to return to rest, so the per-frame work is just the blend.</summary>
    private FadeEntry[] BuildFadeEntries()
    {
        fadeScratch.Clear();
        var result = new List<FadeEntry>();
        for (var r = outgoing; r != null; r = r.outgoing)
        {
            for (int i = 0; i < r.targets.Length; i++)
            {
                var ot = r.targets[i];
                if (ot.Go == null || !fadeScratch.Add(ot.Go)) continue;

                var go = ot.Go;
                var mine = FindTarget(go);
                var e = new FadeEntry { Tr = go.transform };
                for (int a = 0; a < 3; a++)
                {
                    if ((mine == null || (mine.PosMask & (1 << a)) == 0) && outgoing.TryGetRestPosition(go, a, out float pv))
                    {
                        e.PosMask |= 1 << a;
                        e.RestPos[a] = pv;
                    }
                    if ((mine == null || (mine.ScaleMask & (1 << a)) == 0) && outgoing.TryGetRestScale(go, a, out float sv))
                    {
                        e.ScaleMask |= 1 << a;
                        e.RestScale[a] = sv;
                    }
                }
                if ((mine == null || !mine.HasRot) && outgoing.TryGetRestRotation(go, out Quaternion q))
                {
                    e.Rot = true;
                    e.RestRot = q;
                }
                if (e.PosMask != 0 || e.ScaleMask != 0 || e.Rot) result.Add(e);
            }
        }
        fadeScratch.Clear();
        return result.ToArray();
    }

    /// <summary>Puts the channels this animation drives back to their fade-start pose so the outgoing animation poses from a clean slate.</summary>
    private void RestoreFadeFrom()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var tar = targets[i];
            if (!tar.Active) continue;
            var tr = tar.Tr;
            if (tr == null) continue;
            if (tar.PosMask != 0) tr.localPosition = MergeMasked(tr.localPosition, tar.FadeFromPos, tar.PosMask);
            if (tar.HasRot) tr.localRotation = tar.FadeFromRot;
            if (tar.ScaleMask != 0) tr.localScale = MergeMasked(tr.localScale, tar.FadeFromScale, tar.ScaleMask);
        }
    }

    private Target FindTarget(GameObject go)
    {
        if (targets.Length > 8)
        {
            if (targetLookup == null)
            {
                targetLookup = new Dictionary<GameObject, Target>(targets.Length);
                for (int i = 0; i < targets.Length; i++)
                {
                    var t = targets[i];
                    if (t.Go != null && !targetLookup.ContainsKey(t.Go)) targetLookup[t.Go] = t; // first wins, like a linear scan
                }
            }
            targetLookup.TryGetValue(go, out var found);
            return found;
        }
        for (int i = 0; i < targets.Length; i++)
            if (targets[i].Go == go) return targets[i];
        return null;
    }

    // Rest pose of a channel/axis as this animation (or, failing that, the one it replaced) knows it.
    private bool TryGetRestPosition(GameObject go, int axis, out float v)
    {
        var t = FindTarget(go);
        if (t != null && (t.PosMask & (1 << axis)) != 0) { v = t.OrigPos[axis]; return true; }
        if (outgoing != null) return outgoing.TryGetRestPosition(go, axis, out v);
        v = 0f; return false;
    }

    private bool TryGetRestScale(GameObject go, int axis, out float v)
    {
        var t = FindTarget(go);
        if (t != null && (t.ScaleMask & (1 << axis)) != 0) { v = t.OrigScale[axis]; return true; }
        if (outgoing != null) return outgoing.TryGetRestScale(go, axis, out v);
        v = 0f; return false;
    }

    private bool TryGetRestRotation(GameObject go, out Quaternion q)
    {
        var t = FindTarget(go);
        if (t != null && t.HasRot) { q = t.OrigRot; return true; }
        if (outgoing != null) return outgoing.TryGetRestRotation(go, out q);
        q = Quaternion.identity; return false;
    }

    /// <summary>Fires, in time order, every unfired event whose time has been reached. A throwing callback is logged and skipped.</summary>
    private void FireEventsUpTo(float t)
    {
        while (nextEvent < eventList.Count && eventList[nextEvent].Time <= t)
        {
            var ev = eventList[nextEvent++]; // advance first so a re-entrant callback can't fire it twice
            if (string.IsNullOrEmpty(ev.Name)) continue;
            try { Invoke(ev.Name); }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }
    }

    /// <summary>Backward counterpart of FireEventsUpTo: fires, latest first, every unfired event at or after t.</summary>
    private void FireEventsDownTo(float t)
    {
        while (nextEvent >= 0 && nextEvent < eventList.Count && eventList[nextEvent].Time >= t)
        {
            var ev = eventList[nextEvent--];
            if (string.IsNullOrEmpty(ev.Name)) continue;
            try { Invoke(ev.Name); }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }
    }

    private void FireEvents(float t, bool reverse)
    {
        if (reverse) FireEventsDownTo(t);
        else FireEventsUpTo(t);
    }

    /// <summary>Points the event cursor at the start of a pass: first event going forward, last event going backward.</summary>
    private void ResetEventCursor(bool reverse) => nextEvent = reverse ? eventList.Count - 1 : 0;

    /// <summary>
    /// Re-aims the event cursor after the direction flips mid-pass. Events at exactly t count as not yet fired in the new
    /// direction, so a flip re-fires them; events behind the new direction of travel fire again as they are crossed.
    /// </summary>
    private void SeekEventCursor(float t, bool reverse)
    {
        int n = eventList.Count;
        if (reverse)
        {
            int i = n - 1;
            while (i >= 0 && eventList[i].Time > t) i--;
            nextEvent = i;
        }
        else
        {
            int i = 0;
            while (i < n && eventList[i].Time < t) i++;
            nextEvent = i;
        }
    }

    private IEnumerator Animation()
    {
        bool hasEvents = eventList.Count > 0;

        if (!hasAnyTrack && !hasEvents && totalDuration <= 0f)
        {
            // Nothing to play or fade in: behave like a plain Play().
            if (outgoing != null)
            {
                var o = outgoing;
                outgoing = null;
                fadeEntries = null;
                o.StopAndReset();
            }
            Stop();
            yield break;
        }

        // Callbacks are registered after Play() returns, so give them a frame if an event sits at the very start
        // (the end of the animation when it starts out playing backwards).
        if (hasEvents)
        {
            bool startsReversed = PlaybackSpeed < 0f;
            bool eventAtStart = startsReversed
                ? eventList[eventList.Count - 1].Time >= totalDuration - Eps
                : eventList[0].Time <= Eps;
            if (eventAtStart) yield return null;
        }

        bool loop = Asset.Loop && totalDuration > Eps; // a zero-length loop would spin forever
        int pass = 0;
        bool wrapped = false;
        float endTime = 0f; // where the last pass finished, so a trailing fade keeps holding that pose
        bool reverse = false;
        do
        {
            // Direction is picked when a pass starts: a negative speed starts at the end and plays toward 0.
            reverse = PlaybackSpeed < 0f;
            wrapped = pass > 0 || Asset.StartLast;
            ResetEventCursor(reverse);

            float pos = reverse ? totalDuration : 0f;

            if (totalDuration > 0f)
            {
                bool first = true;
                while (true)
                {
                    float speed = PlaybackSpeed;
                    if (speed != 0f && (speed < 0f) != reverse)
                    {
                        reverse = speed < 0f;
                        SeekEventCursor(pos, reverse);
                    }

                    // The first iteration just poses the starting edge; later ones advance by the scaled frame time.
                    if (!first) pos = Mathf.Clamp(pos + Time.deltaTime * speed, 0f, totalDuration);
                    first = false;

                    Tick(pos, wrapped);
                    FireEvents(pos, reverse);

                    if (reverse ? pos <= 0f : pos >= totalDuration) break;
                    yield return null;
                }
            }

            // Land exactly on the final values, even after a long frame.
            endTime = reverse ? 0f : totalDuration;
            Tick(endTime, wrapped);
            FireEvents(reverse ? float.NegativeInfinity : float.PositiveInfinity, reverse);

            if (loop) yield return null;
            pass++;
        } while (loop);

        // Keep blending until the fade ends, even if this animation is shorter than the fade.
        while (outgoing != null)
        {
            yield return null;
            Tick(endTime, wrapped);
        }

        routine = null;
        IsPlaying = false;
        if (Asset.ResetAfterFinish && !isFadingOut) Reset();
        if (animator.CurrentAnim == this) animator.CurrentAnim = null;
    }
}
