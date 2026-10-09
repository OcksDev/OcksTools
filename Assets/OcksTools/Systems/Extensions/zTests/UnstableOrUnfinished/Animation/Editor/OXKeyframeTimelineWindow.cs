// PUT THIS FILE IN A FOLDER NAMED "Editor" (e.g. Assets/OcksTools/Editor/OXKeyframeTimelineWindow.cs)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Timeline editor for OXKeyframeAnimation assets.
///
/// Controls:
///   Click key ............ select (Ctrl/Shift = add/remove from selection)
///   Drag key ............. move selected keys in time (snaps if Snap is on)
///   Stacked keys ......... shown as layered diamonds with a count badge; click an already-selected key
///                          again to cycle through the stack; right-click for Select All / Merge
///   Drag empty space ..... box select
///   Double-click a lane .. add keyframe (Keyframes lane = all channels, other lanes = just that channel)
///   Right-click .......... context menu (add / delete / duplicate / toggle channels)
///   Click empty timeline . move the playhead (click/drag the ruler works too); used by "+ Key" and paste
///   Mouse wheel .......... zoom, Shift+wheel = pan, Middle-drag or Alt+drag = pan
///   Ctrl+C / Ctrl+X / Ctrl+V  copy / cut / paste keys (paste lands at the playhead, spacing is kept)
///   Ctrl+D ............... duplicate selected keys
///   Delete / Backspace ... delete selected keys, F = frame all
/// </summary>
public class OXKeyframeTimelineWindow : EditorWindow
{
    private const float LabelW = 92f;
    private const float RulerH = 22f;
    private const float MinLaneH = 20f;
    private const float MaxLaneH = 80f;
    private float LaneH { get { return laneH; } }
    private const float MarkerR = 7f;
    private const int LaneCount = 4; // 0 = all keyframes, 1 = position, 2 = rotation, 3 = scale

    private static readonly string[] LaneNames = { "Keyframes", "Position", "Rotation", "Scale" };
    private static readonly Color[] LaneColors =
    {
        new Color(0.85f, 0.85f, 0.92f),
        new Color(0.35f, 0.70f, 1.00f),
        new Color(0.45f, 0.90f, 0.50f),
        new Color(1.00f, 0.72f, 0.30f),
    };

    [SerializeField] private OXKeyframeAnimation asset;
    [SerializeField] private bool locked;
    [SerializeField] private bool snap = true;
    [SerializeField] private float snapStep = 0.05f;
    [SerializeField] private float pxPerSec = 160f;
    [SerializeField] private float viewStart = -0.1f;
    [SerializeField] private float playhead;
    [SerializeField] private float laneH = 26f;      // height of one lane; dragging the timeline/details divider changes it
    [SerializeField] private float splitFrac = 0.5f; // fraction of the width given to the left (channels) half of the details

    private readonly List<int> selected = new List<int>();
    private int primary = -1;
    private Vector2 inspectorScroll;

    private enum DragMode { None, Keys, Playhead, Pan, Box }
    private DragMode drag = DragMode.None;
    private float dragStartX;
    private float dragStartView;
    private bool dragUndoRecorded;
    private readonly Dictionary<int, float> dragOrigin = new Dictionary<int, float>();
    private Vector2 boxStart, boxEnd;
    private List<int> boxBase = new List<int>();

    private float timelineX;

    // Stacked-key handling: click an already-selected key to cycle through the keys under the cursor.
    private List<int> cycleStack;
    private bool cycleArmed;
    private int preCyclePrimary = -1;
    private double cycledAt = -10;

    // Keyframe clipboard (static, so you can copy in one animation and paste into another).
    private static readonly List<string> clipJson = new List<string>();
    private static float clipBaseTime;
    private static GUIStyle laneLabel;
    private static GUIStyle badgeLabel;

    // ------------------------------------------------------------------ window plumbing

    [MenuItem("OcksTools/Keyframe Timeline")]
    public static void Open()
    {
        var w = GetWindow<OXKeyframeTimelineWindow>("Keyframe Timeline");
        w.ApplyTitle();
        w.minSize = new Vector2(560f, 380f);
        w.Show();
    }

    /// <summary>Sets the tab title and Unity's built-in monochrome (theme-aware) keyframe diamond icon.</summary>
    private void ApplyTitle()
    {
        var icon = EditorGUIUtility.IconContent("AnimationKeyframe").image;
        if (icon == null) icon = EditorGUIUtility.IconContent("Animation.AddKeyframe").image;
        titleContent = new GUIContent("Keyframe Timeline", icon);
    }

    /// <summary>Double-clicking an OXKeyframeAnimation asset (or "Open" in its context menu) opens this window on it.</summary>
    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var a = EditorUtility.InstanceIDToObject(instanceID) as OXKeyframeAnimation;
        if (a == null) return false; // not ours, let Unity handle it

        var w = GetWindow<OXKeyframeTimelineWindow>("Keyframe Timeline");
        w.ApplyTitle();
        w.minSize = new Vector2(560f, 380f);
        w.Show();
        w.Focus();
        w.SetAsset(a); // explicit open overrides the Lock toggle
        w.Repaint();
        return true;
    }

    private void OnEnable()
    {
        ApplyTitle();
        Undo.undoRedoPerformed += Repaint;
        if (asset == null) PickFromSelection();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
    }

    private void OnSelectionChange()
    {
        if (!locked) PickFromSelection();
    }

    private void PickFromSelection()
    {
        if (Selection.activeObject is OXKeyframeAnimation a && a != asset)
        {
            SetAsset(a);
            Repaint();
        }
    }

    private void SetAsset(OXKeyframeAnimation a)
    {
        asset = a;
        selected.Clear();
        primary = -1;
        drag = DragMode.None;
        if (asset != null) FrameAll();
    }

    private void FrameAll()
    {
        float maxT = 0f;
        if (asset != null)
            foreach (var k in asset.Keyframes)
                if (k != null && k.Time > maxT) maxT = k.Time;
        maxT = Mathf.Max(maxT, 0.1f);
        float w = Mathf.Max(100f, position.width - LabelW - 60f);
        pxPerSec = Mathf.Clamp(w / maxT, 10f, 3000f);
        viewStart = -20f / pxPerSec;
    }

    // ------------------------------------------------------------------ helpers

    private float TimeToX(float t) { return timelineX + (t - viewStart) * pxPerSec; }
    private float XToTime(float x) { return (x - timelineX) / pxPerSec + viewStart; }

    private float Snap(float t)
    {
        if (snap && snapStep > 0.0001f) t = Mathf.Round(t / snapStep) * snapStep;
        return Mathf.Max(0f, t);
    }

    private void ClampView() { viewStart = Mathf.Max(viewStart, -30f / pxPerSec); }

    /// <summary>Structural change in the middle of a GUI event: abort this pass so layout stays consistent.</summary>
    private void Mutated()
    {
        Repaint();
        GUIUtility.ExitGUI();
    }

    private void Edit(string undoName, Action apply)
    {
        Undo.RecordObject(asset, undoName);
        apply();
        EditorUtility.SetDirty(asset);
        Repaint();
    }

    private static OXKeyframeChannel GetChannel(OXKeyframe kf, int lane)
    {
        switch (lane)
        {
            case 1: return kf.Position;
            case 2: return kf.Rotation;
            case 3: return kf.Scale;
            default: return null;
        }
    }

    private static bool LaneHas(OXKeyframe kf, int lane)
    {
        if (kf == null) return false;
        if (lane == 0) return true;
        var c = GetChannel(kf, lane);
        return c != null && c.Enabled;
    }

    private static bool AnyEnabled(OXKeyframe kf) { return LaneHas(kf, 1) || LaneHas(kf, 2) || LaneHas(kf, 3); }

    private static bool IsZeroQuat(Quaternion q) { return q.x == 0f && q.y == 0f && q.z == 0f && q.w == 0f; }

    private void ValidateSelection()
    {
        int n = asset.Keyframes.Count;
        selected.RemoveAll(i => i < 0 || i >= n || asset.Keyframes[i] == null);
        if (primary < 0 || primary >= n || asset.Keyframes[primary] == null)
            primary = selected.Count > 0 ? selected[selected.Count - 1] : -1;
    }

    // ------------------------------------------------------------------ data operations

    private OXKeyframeObjectState MakeState(int objectIndex, float time, OXKeyframe exclude)
    {
        // Start from the nearest earlier keyframe's pose for this object, so a new key "holds" the previous value.
        OXKeyframe best = null;
        OXKeyframeObjectState bestState = null;
        foreach (var k in asset.Keyframes)
        {
            if (k == null || k == exclude || k.Data == null || k.Time > time) continue;
            if (best != null && k.Time < best.Time) continue;
            var s = k.Data.FirstOrDefault(x => x != null && x.ObjectIndex == objectIndex && x.Transform != null);
            if (s == null) continue;
            best = k;
            bestState = s;
        }

        var state = new OXKeyframeObjectState { ObjectIndex = objectIndex };
        if (bestState != null)
        {
            state.Transform = new OXTransformWithScale
            {
                Position = bestState.Transform.Position,
                Rotation = IsZeroQuat(bestState.Transform.Rotation) ? Quaternion.identity : bestState.Transform.Rotation,
                Scale = bestState.Transform.Scale
            };
        }
        return state;
    }

    private int AddKeyframe(float time, bool pos, bool rot, bool scl)
    {
        Undo.RecordObject(asset, "Add Keyframe");
        var kf = new OXKeyframe
        {
            Time = Mathf.Max(0f, time),
            Position = new OXKeyframeChannel(pos),
            Rotation = new OXKeyframeChannel(rot),
            Scale = new OXKeyframeChannel(scl),
        };

        var indices = new SortedSet<int>();
        foreach (var k in asset.Keyframes)
            if (k != null && k.Data != null)
                foreach (var d in k.Data)
                    if (d != null) indices.Add(d.ObjectIndex);
        if (indices.Count == 0) indices.Add(0);
        foreach (var idx in indices) kf.Data.Add(MakeState(idx, kf.Time, null));

        asset.Keyframes.Add(kf);
        EditorUtility.SetDirty(asset);

        int newIndex = asset.Keyframes.Count - 1;
        selected.Clear();
        selected.Add(newIndex);
        primary = newIndex;
        return newIndex;
    }

    private void DeleteSelected()
    {
        if (selected.Count == 0) return;
        Undo.RecordObject(asset, "Delete Keyframes");
        foreach (var i in selected.Distinct().OrderByDescending(i => i))
            if (i >= 0 && i < asset.Keyframes.Count) asset.Keyframes.RemoveAt(i);
        EditorUtility.SetDirty(asset);
        selected.Clear();
        primary = -1;
    }

    private void DuplicateSelected()
    {
        if (selected.Count == 0) return;
        Undo.RecordObject(asset, "Duplicate Keyframes");
        var sources = selected.Distinct().OrderBy(i => i).ToArray();
        selected.Clear();
        foreach (var i in sources)
        {
            var src = asset.Keyframes[i];
            if (src == null) continue;
            var copy = JsonUtility.FromJson<OXKeyframe>(JsonUtility.ToJson(src));
            copy.Time = src.Time + (snap && snapStep > 0.0001f ? snapStep : 0.1f);
            asset.Keyframes.Add(copy);
            selected.Add(asset.Keyframes.Count - 1);
        }
        primary = selected.Count > 0 ? selected[selected.Count - 1] : -1;
        EditorUtility.SetDirty(asset);
    }

    private void CopySelected()
    {
        var valid = selected.Distinct()
            .Where(i => i >= 0 && i < asset.Keyframes.Count && asset.Keyframes[i] != null)
            .OrderBy(i => asset.Keyframes[i].Time)
            .ToList();
        if (valid.Count == 0) return;

        clipJson.Clear();
        clipBaseTime = asset.Keyframes[valid[0]].Time;
        foreach (var i in valid) clipJson.Add(JsonUtility.ToJson(asset.Keyframes[i]));
    }

    /// <summary>Pastes the clipboard so the earliest copied key lands on 'time'; relative spacing is preserved.</summary>
    private void PasteAt(float time)
    {
        if (clipJson.Count == 0) return;
        Undo.RecordObject(asset, "Paste Keyframes");
        selected.Clear();
        foreach (var json in clipJson)
        {
            var kf = JsonUtility.FromJson<OXKeyframe>(json);
            kf.Time = Mathf.Max(0f, time + (kf.Time - clipBaseTime));
            asset.Keyframes.Add(kf);
            selected.Add(asset.Keyframes.Count - 1);
        }
        primary = selected[selected.Count - 1];
        EditorUtility.SetDirty(asset);
    }

    private const float SameTimeEpsilon = 0.0005f;

    private void SelectAtTime(float time)
    {
        selected.Clear();
        for (int i = 0; i < asset.Keyframes.Count; i++)
            if (asset.Keyframes[i] != null && Mathf.Abs(asset.Keyframes[i].Time - time) < SameTimeEpsilon)
                selected.Add(i);
        primary = selected.Count > 0 ? selected[selected.Count - 1] : -1;
    }

    /// <summary>
    /// Folds several keyframes into the lowest-numbered one: every channel a later key enables
    /// (settings + the values for each object) is copied over, then the later keys are removed.
    /// If two keys enable the same channel, the later key wins.
    /// </summary>
    private void MergeKeys(List<int> indices)
    {
        var list = indices.Distinct()
            .Where(i => i >= 0 && i < asset.Keyframes.Count && asset.Keyframes[i] != null)
            .OrderBy(i => i).ToList();
        if (list.Count < 2) return;

        Undo.RecordObject(asset, "Merge Keyframes");
        var target = asset.Keyframes[list[0]];
        if (target.Data == null) target.Data = new List<OXKeyframeObjectState>();

        for (int n = 1; n < list.Count; n++)
        {
            var src = asset.Keyframes[list[n]];

            for (int lane = 1; lane <= 3; lane++)
            {
                if (!LaneHas(src, lane)) continue;
                var copy = JsonUtility.FromJson<OXKeyframeChannel>(JsonUtility.ToJson(GetChannel(src, lane)));
                if (lane == 1) target.Position = copy;
                else if (lane == 2) target.Rotation = copy;
                else target.Scale = copy;
            }

            if (src.Data == null) continue;
            foreach (var sd in src.Data)
            {
                if (sd == null || sd.Transform == null) continue;
                var td = target.Data.FirstOrDefault(x => x != null && x.ObjectIndex == sd.ObjectIndex);
                if (td == null)
                {
                    td = MakeState(sd.ObjectIndex, target.Time, target);
                    target.Data.Add(td);
                }
                if (td.Transform == null) td.Transform = new OXTransformWithScale { Rotation = Quaternion.identity, Scale = Vector3.one };
                if (LaneHas(src, 1)) td.Transform.Position = sd.Transform.Position;
                if (LaneHas(src, 2)) td.Transform.Rotation = sd.Transform.Rotation;
                if (LaneHas(src, 3)) td.Transform.Scale = sd.Transform.Scale;
            }
        }

        for (int n = list.Count - 1; n >= 1; n--) asset.Keyframes.RemoveAt(list[n]);
        EditorUtility.SetDirty(asset);
        selected.Clear();
        selected.Add(list[0]);
        primary = list[0];
    }

    private static void SetChannel(OXKeyframe kf, int lane, bool value)
    {
        var c = GetChannel(kf, lane);
        if (c == null)
        {
            c = new OXKeyframeChannel(value);
            if (lane == 1) kf.Position = c;
            else if (lane == 2) kf.Rotation = c;
            else kf.Scale = c;
        }
        c.Enabled = value;
    }

    private void ToggleChannelOnSelected(int lane, bool value)
    {
        Edit("Toggle Channel", () =>
        {
            foreach (var i in selected)
            {
                var kf = asset.Keyframes[i];
                if (kf != null) SetChannel(kf, lane, value);
            }
        });
    }

    /// <summary>Index of the keyframe sitting at this x position (any lane), or -1.</summary>
    private int FindKeyAtX(float x)
    {
        if (primary >= 0 && primary < asset.Keyframes.Count && asset.Keyframes[primary] != null &&
            Mathf.Abs(x - TimeToX(asset.Keyframes[primary].Time)) <= MarkerR + 1f)
            return primary;

        for (int i = asset.Keyframes.Count - 1; i >= 0; i--)
        {
            var kf = asset.Keyframes[i];
            if (kf != null && Mathf.Abs(x - TimeToX(kf.Time)) <= MarkerR + 1f) return i;
        }
        return -1;
    }

    // ------------------------------------------------------------------ OnGUI

    private void OnGUI()
    {
        DrawToolbar();

        if (asset == null)
        {
            EditorGUILayout.HelpBox(
                "Select an OXKeyframeAnimation asset in the Project window, or drop one into the field above.",
                MessageType.Info);
            return;
        }

        ValidateSelection();
        HandleKeyboard();
        DrawTimeline();
        DrawTimelineSplitter();
        DrawInspector();
    }

    // ------------------------------------------------------------------ splitters

    private float splitStartMouse;
    private float splitStartValue;

    /// <summary>Draggable bar under the timeline. Dragging it makes the timeline lanes taller or shorter.</summary>
    private void DrawTimelineSplitter()
    {
        Rect r = GUILayoutUtility.GetRect(10f, 6f, GUILayout.ExpandWidth(true), GUILayout.Height(6f));
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);

        switch (e.GetTypeForControl(id))
        {
            case EventType.Repaint:
                EditorGUI.DrawRect(new Rect(r.x, r.center.y - 0.5f, r.width, 1f), new Color(0f, 0f, 0f, 0.45f));
                break;
            case EventType.MouseDown:
                if (e.button == 0 && r.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = id;
                    splitStartMouse = e.mousePosition.y;
                    splitStartValue = laneH;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    laneH = Mathf.Clamp(splitStartValue + (e.mousePosition.y - splitStartMouse) / LaneCount, MinLaneH, MaxLaneH);
                    e.Use();
                    Repaint();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
        }
    }

    /// <summary>Draggable divider between the channel settings (left) and the object states (right).</summary>
    private void DrawDetailsSplitter()
    {
        Rect r = GUILayoutUtility.GetRect(6f, 6f, GUILayout.Width(6f), GUILayout.ExpandHeight(true));
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeHorizontal);

        switch (e.GetTypeForControl(id))
        {
            case EventType.Repaint:
                EditorGUI.DrawRect(new Rect(r.center.x - 0.5f, r.y, 1f, r.height), new Color(0f, 0f, 0f, 0.45f));
                break;
            case EventType.MouseDown:
                if (e.button == 0 && r.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = id;
                    splitStartMouse = e.mousePosition.x;
                    splitStartValue = splitFrac;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    splitFrac = Mathf.Clamp(splitStartValue + (e.mousePosition.x - splitStartMouse) / Mathf.Max(1f, position.width), 0.2f, 0.8f);
                    e.Use();
                    Repaint();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
        }
    }

    private void HandleKeyboard()
    {
        var e = Event.current;
        if (EditorGUIUtility.editingTextField) return; // let text fields use Backspace normally

        // Editor "Delete" / "SoftDelete" commands (this is how Backspace arrives on some platforms).
        if (e.type == EventType.ValidateCommand || e.type == EventType.ExecuteCommand)
        {
            bool execute = e.type == EventType.ExecuteCommand;
            switch (e.commandName)
            {
                case "Delete":
                case "SoftDelete":
                    if (selected.Count == 0) return;
                    e.Use();
                    if (execute) { DeleteSelected(); Mutated(); }
                    return;

                case "Copy":
                    if (selected.Count == 0) return;
                    e.Use();
                    if (execute) CopySelected();
                    return;

                case "Cut":
                    if (selected.Count == 0) return;
                    e.Use();
                    if (execute) { CopySelected(); DeleteSelected(); Mutated(); }
                    return;

                case "Paste":
                    if (clipJson.Count == 0) return;
                    e.Use();
                    if (execute) { PasteAt(playhead); Mutated(); }
                    return;

                case "Duplicate":
                    if (selected.Count == 0) return;
                    e.Use();
                    if (execute) { DuplicateSelected(); Mutated(); }
                    return;
            }
            return;
        }

        if (e.type != EventType.KeyDown) return;

        if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
        {
            if (selected.Count == 0) return;
            DeleteSelected();
            e.Use();
            Mutated();
        }
        else if (e.keyCode == KeyCode.F && GUIUtility.keyboardControl == 0)
        {
            FrameAll();
            e.Use();
            Repaint();
        }
    }

    // ------------------------------------------------------------------ toolbar

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.FlexibleSpace(); // pushes the main buttons toward the center

        EditorGUI.BeginChangeCheck();
        var picked = (OXKeyframeAnimation)EditorGUILayout.ObjectField(asset, typeof(OXKeyframeAnimation), false, GUILayout.Width(220));
        if (EditorGUI.EndChangeCheck())
        {
            SetAsset(picked);
            GUILayout.EndHorizontal();
            Mutated();
        }

        locked = GUILayout.Toggle(locked,
            new GUIContent("Lock", "When on, the window keeps showing this animation even if you click a different asset in the Project window."),
            EditorStyles.toolbarButton, GUILayout.Width(40));

        if (asset != null)
        {
            // Save: only enabled when the asset has unsaved changes.
            bool dirty = EditorUtility.IsDirty(asset);
            EditorGUI.BeginDisabledGroup(!dirty);
            if (GUILayout.Button(new GUIContent(dirty ? "Save*" : "Save", "Write this animation asset to disk"),
                    EditorStyles.toolbarButton, GUILayout.Width(44)))
            {
                AssetDatabase.SaveAssetIfDirty(asset);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(6);
            if (GUILayout.Button("+ Key", EditorStyles.toolbarButton, GUILayout.Width(46)))
            {
                AddKeyframe(playhead, true, true, true);
                GUILayout.EndHorizontal();
                Mutated();
            }

            EditorGUI.BeginDisabledGroup(selected.Count == 0);
            if (GUILayout.Button("Duplicate", EditorStyles.toolbarButton, GUILayout.Width(62)))
            {
                DuplicateSelected();
                GUILayout.EndHorizontal();
                Mutated();
            }
            if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(48)))
            {
                DeleteSelected();
                GUILayout.EndHorizontal();
                Mutated();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(6);
            snap = GUILayout.Toggle(snap, "Snap", EditorStyles.toolbarButton, GUILayout.Width(42));
            EditorGUI.BeginDisabledGroup(!snap);
            snapStep = Mathf.Max(0.001f, EditorGUILayout.FloatField(snapStep, EditorStyles.toolbarTextField, GUILayout.Width(44)));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Frame All", EditorStyles.toolbarButton, GUILayout.Width(62))) FrameAll();

            GUILayout.Label("Playhead", EditorStyles.miniLabel, GUILayout.Width(50));
            playhead = Mathf.Max(0f, EditorGUILayout.FloatField(playhead, EditorStyles.toolbarTextField, GUILayout.Width(50)));

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            bool reset = GUILayout.Toggle(asset.ResetAfterFinish, "Reset After Finish", EditorStyles.toolbarButton);
            bool over = GUILayout.Toggle(asset.OverrideData, "Override Data", EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
                Edit("Change Animation Settings", () => { asset.ResetAfterFinish = reset; asset.OverrideData = over; });
        }
        else
        {
            GUILayout.FlexibleSpace();
        }

        GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ timeline

    private static void EnsureStyles()
    {
        if (laneLabel == null)
            laneLabel = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
        if (badgeLabel == null)
        {
            badgeLabel = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 9 };
            badgeLabel.normal.textColor = Color.white;
        }
    }

    private void DrawTimeline()
    {
        float totalH = RulerH + LaneCount * LaneH;
        Rect area = GUILayoutUtility.GetRect(10f, totalH, GUILayout.ExpandWidth(true), GUILayout.Height(totalH));
        timelineX = area.x + LabelW;
        Rect content = new Rect(timelineX, area.y, area.width - LabelW, totalH);
        Rect ruler = new Rect(timelineX, area.y, content.width, RulerH);
        Rect lanes = new Rect(timelineX, area.y + RulerH, content.width, LaneCount * LaneH);

        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        EventType type = e.GetTypeForControl(id);
        EnsureStyles();

        if (type == EventType.Repaint) DrawTimelineVisuals(area, content, ruler, lanes);
        HandleTimelineInput(type, id, e, area, content, ruler, lanes);
    }

    private float ChooseStep()
    {
        float[] candidates = { 0.01f, 0.02f, 0.05f, 0.1f, 0.2f, 0.25f, 0.5f, 1f, 2f, 5f, 10f, 30f, 60f };
        foreach (var s in candidates)
            if (s * pxPerSec >= 70f) return s;
        return 60f;
    }

    private static string FormatTime(float t) { return t.ToString("0.###") + "s"; }

    private void DrawDiamond(Vector2 c, float r, Color fill, Color outline, float lineWidth)
    {
        var pts = new[]
        {
            new Vector3(c.x, c.y - r), new Vector3(c.x + r, c.y),
            new Vector3(c.x, c.y + r), new Vector3(c.x - r, c.y),
        };
        Handles.color = fill;
        Handles.DrawAAConvexPolygon(pts);
        Handles.color = outline;
        Handles.DrawAAPolyLine(lineWidth, pts[0], pts[1], pts[2], pts[3], pts[0]);
        Handles.color = Color.white;
    }

    private void DrawTimelineVisuals(Rect area, Rect content, Rect ruler, Rect lanes)
    {
        bool pro = EditorGUIUtility.isProSkin;
        Color bg = pro ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.76f, 0.76f, 0.76f);
        Color laneA = pro ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.80f, 0.80f, 0.80f);
        Color laneB = pro ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.77f, 0.77f, 0.77f);
        Color grid = pro ? new Color(1f, 1f, 1f, 0.07f) : new Color(0f, 0f, 0f, 0.10f);
        Color tick = pro ? new Color(1f, 1f, 1f, 0.45f) : new Color(0f, 0f, 0f, 0.5f);

        EditorGUI.DrawRect(area, bg);

        // lane backgrounds + labels
        for (int l = 0; l < LaneCount; l++)
        {
            float y = lanes.y + l * LaneH;
            EditorGUI.DrawRect(new Rect(content.x, y, content.width, LaneH), l % 2 == 0 ? laneA : laneB);
            EditorGUI.DrawRect(new Rect(area.x + 4f, y + LaneH * 0.5f - 4f, 3f, 8f), LaneColors[l]);
            GUI.Label(new Rect(area.x + 12f, y, LabelW - 14f, LaneH), LaneNames[l], laneLabel);
        }

        // region before t = 0
        float zeroX = TimeToX(0f);
        if (zeroX > content.x)
            EditorGUI.DrawRect(new Rect(content.x, area.y, Mathf.Min(zeroX, content.xMax) - content.x, area.height), new Color(0f, 0f, 0f, 0.25f));

        // ruler ticks + grid
        float step = ChooseStep();
        float sub = step / 5f;
        int first = Mathf.FloorToInt(XToTime(content.x) / step);
        int last = Mathf.CeilToInt(XToTime(content.xMax) / step);
        for (int i = first; i <= last; i++)
        {
            float t = i * step;
            float x = TimeToX(t);
            if (x >= content.x && x <= content.xMax)
            {
                EditorGUI.DrawRect(new Rect(x, ruler.yMax - 9f, 1f, 9f), tick);
                EditorGUI.DrawRect(new Rect(x, lanes.y, 1f, lanes.height), grid);
                if (t >= -0.0001f) GUI.Label(new Rect(x + 3f, ruler.y + 1f, 60f, 14f), FormatTime(t), EditorStyles.miniLabel);
            }
            if (sub * pxPerSec >= 7f)
            {
                for (int k = 1; k < 5; k++)
                {
                    float sx = TimeToX(t + k * sub);
                    if (sx >= content.x && sx <= content.xMax)
                        EditorGUI.DrawRect(new Rect(sx, ruler.yMax - 4f, 1f, 4f), tick);
                }
            }
        }
        EditorGUI.DrawRect(new Rect(content.x, ruler.yMax - 1f, content.width, 1f), tick);

        // keys (keys that sit on top of each other in a lane are drawn as a stack with a count badge)
        bool anyKey = asset.Keyframes.Any(k => k != null);
        List<int> preview = drag == DragMode.Box ? BoxHits(lanes, MakeBoxRect()) : null;
        Func<int, bool> isSel = i => selected.Contains(i) || (preview != null && preview.Contains(i));

        for (int l = 0; l < LaneCount; l++)
        {
            var inLane = new List<int>();
            for (int i = 0; i < asset.Keyframes.Count; i++)
            {
                var kf = asset.Keyframes[i];
                if (!LaneHas(kf, l)) continue;
                float x = TimeToX(kf.Time);
                if (x < content.x || x > content.xMax) continue;
                inLane.Add(i);
            }
            inLane.Sort((p1, p2) => asset.Keyframes[p1].Time.CompareTo(asset.Keyframes[p2].Time));

            int start = 0;
            while (start < inLane.Count)
            {
                int end = start + 1;
                while (end < inLane.Count &&
                       TimeToX(asset.Keyframes[inLane[end]].Time) - TimeToX(asset.Keyframes[inLane[end - 1]].Time) <= 4f)
                    end++;

                // selected keys are drawn last so they sit on top of the stack
                var cluster = inLane.GetRange(start, end - start)
                    .OrderBy(i => isSel(i) ? 1 : 0).ThenBy(i => i).ToList();
                int n = cluster.Count;
                float laneCenter = lanes.y + l * LaneH + LaneH * 0.5f;

                for (int j = 0; j < n; j++)
                {
                    int i = cluster[j];
                    var kf = asset.Keyframes[i];
                    bool sel = isSel(i);
                    int layer = Mathf.Min(n - 1 - j, 2); // 0 = top of the stack
                    Color fill = LaneColors[l];
                    if (l == 0 && !AnyEnabled(kf)) fill = new Color(0.45f, 0.45f, 0.45f);
                    if (layer > 0) fill = Color.Lerp(fill, Color.black, 0.3f * layer);
                    Color outline = sel ? new Color(1f, 0.95f, 0.3f) : new Color(0f, 0f, 0f, 0.8f);
                    var c = new Vector2(TimeToX(kf.Time), laneCenter - layer * 3f);
                    DrawDiamond(c, sel ? MarkerR + 1f : MarkerR, fill, outline, sel ? 2.5f : 1.5f);
                }

                if (n > 1)
                {
                    float topX = TimeToX(asset.Keyframes[cluster[n - 1]].Time);
                    var badge = new Rect(topX + 6f, lanes.y + l * LaneH + 1f, n >= 10 ? 18f : 13f, 11f);
                    EditorGUI.DrawRect(badge, new Color(0.05f, 0.05f, 0.05f, 0.9f));
                    GUI.Label(badge, n.ToString(), badgeLabel);
                }

                start = end;
            }
        }

        if (!anyKey)
        {
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            GUI.Label(lanes, "Double-click a lane to add a keyframe", style);
        }

        // box select
        if (drag == DragMode.Box)
        {
            var r = MakeBoxRect();
            EditorGUI.DrawRect(r, new Color(0.3f, 0.6f, 1f, 0.15f));
            Handles.color = new Color(0.4f, 0.7f, 1f, 0.9f);
            Handles.DrawAAPolyLine(1.5f,
                new Vector3(r.xMin, r.yMin), new Vector3(r.xMax, r.yMin), new Vector3(r.xMax, r.yMax),
                new Vector3(r.xMin, r.yMax), new Vector3(r.xMin, r.yMin));
            Handles.color = Color.white;
        }

        // playhead
        float px = TimeToX(playhead);
        if (px >= content.x && px <= content.xMax)
        {
            var red = new Color(1f, 0.25f, 0.25f);
            EditorGUI.DrawRect(new Rect(px - 0.5f, area.y, 2f, area.height), red);
        }
    }

    private Rect MakeBoxRect()
    {
        return Rect.MinMaxRect(
            Mathf.Min(boxStart.x, boxEnd.x), Mathf.Min(boxStart.y, boxEnd.y),
            Mathf.Max(boxStart.x, boxEnd.x), Mathf.Max(boxStart.y, boxEnd.y));
    }

    private List<int> BoxHits(Rect lanes, Rect box)
    {
        var hits = new List<int>();
        for (int i = 0; i < asset.Keyframes.Count; i++)
        {
            var kf = asset.Keyframes[i];
            if (kf == null) continue;
            float x = TimeToX(kf.Time);
            for (int l = 0; l < LaneCount; l++)
            {
                if (!LaneHas(kf, l)) continue;
                if (box.Contains(new Vector2(x, lanes.y + l * LaneH + LaneH * 0.5f)))
                {
                    hits.Add(i);
                    break;
                }
            }
        }
        return hits;
    }

    /// <summary>Every keyframe whose marker is under the cursor, topmost (highest index) first.</summary>
    private List<int> GetHits(Vector2 m, Rect lanes)
    {
        var hits = new List<int>();
        for (int i = asset.Keyframes.Count - 1; i >= 0; i--)
        {
            var kf = asset.Keyframes[i];
            if (kf == null) continue;
            if (Mathf.Abs(m.x - TimeToX(kf.Time)) > MarkerR + 1f) continue;
            for (int l = 0; l < LaneCount; l++)
            {
                if (!LaneHas(kf, l)) continue;
                float cy = lanes.y + l * LaneH + LaneH * 0.5f;
                if (Mathf.Abs(m.y - cy) <= MarkerR + 1f)
                {
                    hits.Add(i);
                    break;
                }
            }
        }
        return hits;
    }

    /// <summary>Of the stacked keys under the cursor, prefer one that is already selected (so dragging a selection works).</summary>
    private int PickFromStack(List<int> hits)
    {
        if (primary >= 0 && selected.Contains(primary) && hits.Contains(primary)) return primary;
        foreach (var h in hits)
            if (selected.Contains(h)) return h;
        return hits[0];
    }

    private void HandleTimelineInput(EventType type, int id, Event e, Rect area, Rect content, Rect ruler, Rect lanes)
    {
        Vector2 m = e.mousePosition;

        switch (type)
        {
            case EventType.ScrollWheel:
                if (!content.Contains(m)) break;
                if (e.shift)
                {
                    viewStart += e.delta.y * 20f / pxPerSec;
                }
                else
                {
                    float t0 = XToTime(m.x);
                    pxPerSec = Mathf.Clamp(pxPerSec * Mathf.Exp(-e.delta.y * 0.05f), 10f, 3000f);
                    viewStart = t0 - (m.x - timelineX) / pxPerSec;
                }
                ClampView();
                e.Use();
                Repaint();
                break;

            case EventType.MouseDown:
                if (!content.Contains(m)) break;

                // Drop focus from any inspector field so Backspace/Delete reach the timeline.
                GUI.FocusControl(null);

                // pan
                if (e.button == 2 || (e.button == 0 && e.alt))
                {
                    drag = DragMode.Pan;
                    dragStartX = m.x;
                    dragStartView = viewStart;
                    GUIUtility.hotControl = id;
                    e.Use();
                    break;
                }

                // playhead
                if (e.button == 0 && ruler.Contains(m))
                {
                    drag = DragMode.Playhead;
                    playhead = Snap(XToTime(m.x));
                    GUIUtility.hotControl = id;
                    e.Use();
                    Repaint();
                    break;
                }

                if (!lanes.Contains(m)) break;

                if (e.button == 0)
                {
                    List<int> stackHits = null;
                    if (e.clickCount == 2)
                    {
                        int lane = Mathf.Clamp(Mathf.FloorToInt((m.y - lanes.y) / LaneH), 0, LaneCount - 1);

                        // If the first click of this double-click cycled the stack, go back to the key that was selected.
                        if (EditorApplication.timeSinceStartup - cycledAt < 0.6 &&
                            preCyclePrimary >= 0 && preCyclePrimary < asset.Keyframes.Count)
                        {
                            selected.Clear();
                            selected.Add(preCyclePrimary);
                            primary = preCyclePrimary;
                        }
                        cycledAt = -10;

                        int existing = FindKeyAtX(m.x);
                        if (existing >= 0)
                        {
                            // A keyframe already lives here: toggle that lane's channel on/off.
                            var ekf = asset.Keyframes[existing];
                            selected.Clear();
                            selected.Add(existing);
                            primary = existing;
                            Edit("Toggle Channel", () =>
                            {
                                if (lane == 0)
                                {
                                    // Keyframes lane: switch every channel off, or all on if none are enabled.
                                    bool on = !AnyEnabled(ekf);
                                    for (int l = 1; l <= 3; l++) SetChannel(ekf, l, on);
                                }
                                else
                                {
                                    SetChannel(ekf, lane, !LaneHas(ekf, lane));
                                }
                            });
                        }
                        else
                        {
                            AddKeyframe(Snap(XToTime(m.x)), lane == 0 || lane == 1, lane == 0 || lane == 2, lane == 0 || lane == 3);
                        }
                        e.Use();
                        Mutated();
                    }
                    else if ((stackHits = GetHits(m, lanes)).Count > 0)
                    {
                        int hit = PickFromStack(stackHits);
                        bool wasSelected = selected.Contains(hit);
                        bool mod = e.control || e.command || e.shift;
                        if (mod)
                        {
                            if (selected.Contains(hit))
                            {
                                selected.Remove(hit);
                                primary = selected.Count > 0 ? selected[selected.Count - 1] : -1;
                            }
                            else
                            {
                                selected.Add(hit);
                                primary = hit;
                            }
                        }
                        else
                        {
                            if (!wasSelected)
                            {
                                selected.Clear();
                                selected.Add(hit);
                            }
                            primary = hit;
                        }

                        // Clicking an already-selected key in a stack (without dragging) cycles to the next key in it.
                        cycleStack = stackHits;
                        cycleArmed = !mod && wasSelected && stackHits.Count > 1 && selected.Count == 1;

                        if (selected.Contains(hit))
                        {
                            drag = DragMode.Keys;
                            dragStartX = m.x;
                            dragUndoRecorded = false;
                            dragOrigin.Clear();
                            foreach (var i in selected) dragOrigin[i] = asset.Keyframes[i].Time;
                            GUIUtility.hotControl = id;
                        }
                        e.Use();
                        Mutated();
                    }
                    else
                    {
                        boxBase = e.shift ? new List<int>(selected) : new List<int>();
                        if (!e.shift)
                        {
                            selected.Clear();
                            primary = -1;
                        }
                        drag = DragMode.Box;
                        boxStart = boxEnd = m;
                        GUIUtility.hotControl = id;
                        e.Use();
                        Mutated();
                    }
                }
                else if (e.button == 1)
                {
                    var rightHits = GetHits(m, lanes);
                    if (rightHits.Count > 0)
                    {
                        int rhit = PickFromStack(rightHits);
                        if (!selected.Contains(rhit))
                        {
                            selected.Clear();
                            selected.Add(rhit);
                        }
                        primary = rhit;
                        ShowKeyMenu();
                    }
                    else
                    {
                        ShowAddMenu(Snap(XToTime(m.x)));
                    }
                    e.Use();
                    Mutated();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl != id) break;
                switch (drag)
                {
                    case DragMode.Pan:
                        viewStart = dragStartView - (m.x - dragStartX) / pxPerSec;
                        ClampView();
                        break;

                    case DragMode.Playhead:
                        playhead = Snap(XToTime(m.x));
                        break;

                    case DragMode.Keys:
                        if (!dragUndoRecorded)
                        {
                            if (Mathf.Abs(m.x - dragStartX) < 2f) break;
                            Undo.RecordObject(asset, "Move Keyframes");
                            dragUndoRecorded = true;
                        }
                        {
                            float delta = (m.x - dragStartX) / pxPerSec;
                            int lead = dragOrigin.ContainsKey(primary) ? primary : dragOrigin.Keys.First();
                            float newLead = Snap(dragOrigin[lead] + delta);
                            delta = newLead - dragOrigin[lead];
                            float minOrig = dragOrigin.Values.Min();
                            delta = Mathf.Max(delta, -minOrig);
                            foreach (var kv in dragOrigin)
                                if (kv.Key < asset.Keyframes.Count && asset.Keyframes[kv.Key] != null)
                                    asset.Keyframes[kv.Key].Time = kv.Value + delta;
                            EditorUtility.SetDirty(asset);
                        }
                        break;

                    case DragMode.Box:
                        boxEnd = m;
                        break;
                }
                e.Use();
                Repaint();
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl != id) break;
                GUIUtility.hotControl = 0;
                bool wasBox = drag == DragMode.Box;
                if (wasBox)
                {
                    // A plain click (no real drag) on empty timeline space moves the playhead there.
                    if ((boxEnd - boxStart).sqrMagnitude < 9f) playhead = Snap(XToTime(boxStart.x));

                    var hits = BoxHits(lanes, MakeBoxRect());
                    selected.Clear();
                    selected.AddRange(boxBase);
                    foreach (var h in hits)
                        if (!selected.Contains(h)) selected.Add(h);
                    primary = selected.Count > 0 ? selected[selected.Count - 1] : -1;
                }

                bool cycled = false;
                if (drag == DragMode.Keys && cycleArmed && !dragUndoRecorded && cycleStack != null && cycleStack.Count > 1)
                {
                    int at = cycleStack.IndexOf(primary);
                    int next = cycleStack[(at + 1) % cycleStack.Count];
                    preCyclePrimary = primary;
                    cycledAt = EditorApplication.timeSinceStartup;
                    selected.Clear();
                    selected.Add(next);
                    primary = next;
                    cycled = true;
                }
                cycleArmed = false;

                drag = DragMode.None;
                e.Use();
                if (wasBox || cycled) Mutated();
                else Repaint();
                break;
        }
    }

    private void ShowKeyMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete Keyframe(s)"), false, () => { DeleteSelected(); Repaint(); });
        menu.AddItem(new GUIContent("Duplicate Keyframe(s)"), false, () => { DuplicateSelected(); Repaint(); });
        menu.AddItem(new GUIContent("Copy"), false, CopySelected);
        menu.AddSeparator("");

        var pkf = primary >= 0 && primary < asset.Keyframes.Count ? asset.Keyframes[primary] : null;
        float atTime = pkf != null ? pkf.Time : 0f;
        menu.AddItem(new GUIContent("Select All Keys At This Time"), false, () => { SelectAtTime(atTime); Repaint(); });
        if (selected.Count > 1)
            menu.AddItem(new GUIContent("Merge Selected Keyframes"), false, () => { MergeKeys(new List<int>(selected)); Repaint(); });
        else
            menu.AddDisabledItem(new GUIContent("Merge Selected Keyframes"));
        menu.AddSeparator("");

        var pk = pkf;
        for (int lane = 1; lane <= 3; lane++)
        {
            int l = lane;
            bool on = LaneHas(pk, l);
            menu.AddItem(new GUIContent(LaneNames[l] + " Enabled"), on, () => ToggleChannelOnSelected(l, !on));
        }
        menu.ShowAsContext();
    }

    private void ShowAddMenu(float time)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Keyframe Here (All Channels)"), false, () => { AddKeyframe(time, true, true, true); Repaint(); });
        if (clipJson.Count > 0)
            menu.AddItem(new GUIContent("Paste Keyframes Here"), false, () => { PasteAt(time); Repaint(); });
        else
            menu.AddDisabledItem(new GUIContent("Paste Keyframes Here"));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Add Position Keyframe"), false, () => { AddKeyframe(time, true, false, false); Repaint(); });
        menu.AddItem(new GUIContent("Add Rotation Keyframe"), false, () => { AddKeyframe(time, false, true, false); Repaint(); });
        menu.AddItem(new GUIContent("Add Scale Keyframe"), false, () => { AddKeyframe(time, false, false, true); Repaint(); });
        menu.ShowAsContext();
    }

    // ------------------------------------------------------------------ inspector

    private Vector2 leftScroll;
    private Vector2 rightScroll;

    private void DrawInspector()
    {
        OXKeyframe kf = (primary >= 0 && primary < asset.Keyframes.Count) ? asset.Keyframes[primary] : null;

        if (kf == null)
        {
            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
            EditorGUILayout.HelpBox(
                "No keyframe selected. Click a keyframe in the timeline to edit it.\n" +
                "Double-click a lane to add one (or toggle that channel on an existing key), right-click for more options.",
                MessageType.None);
            EditorGUILayout.EndScrollView();
            return;
        }

        if (kf.Data == null) kf.Data = new List<OXKeyframeObjectState>();
        if (kf.Position == null) kf.Position = new OXKeyframeChannel();
        if (kf.Rotation == null) kf.Rotation = new OXKeyframeChannel();
        if (kf.Scale == null) kf.Scale = new OXKeyframeChannel();

        int pendingRemove = -1;
        int pendingDuplicate = -1;
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 95f;
        float leftW = Mathf.Max(120f, Mathf.Floor(position.width * splitFrac) - 6f);
        float rightW = Mathf.Max(120f, position.width - 16f - leftW);

        EditorGUILayout.BeginHorizontal();

        // ---------------- LEFT HALF: time + enabled channels and their interpolation ----------------
        EditorGUILayout.BeginVertical(GUILayout.Width(leftW));
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        string header = "Keyframe #" + primary;
        if (selected.Count > 1) header += "   (" + selected.Count + " selected, editing the last clicked)";
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        float nt = EditorGUILayout.FloatField("Time", kf.Time);
        if (EditorGUI.EndChangeCheck()) Edit("Change Keyframe Time", () => kf.Time = Mathf.Max(0f, nt));

        DrawStackStrip(kf);

        EditorGUILayout.Space(2);
        DrawChannel("Position", kf.Position, LaneColors[1]);
        DrawChannel("Rotation", kf.Rotation, LaneColors[2]);
        DrawChannel("Scale", kf.Scale, LaneColors[3]);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // draggable divider
        DrawDetailsSplitter();

        // ---------------- RIGHT HALF: object states ----------------
        EditorGUILayout.BeginVertical(GUILayout.Width(rightW));
        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

        EditorGUILayout.LabelField("Object States", EditorStyles.boldLabel);
        if (!asset.OverrideData)
            EditorGUILayout.LabelField("Relative to each object's starting pose (position offset, scale multiplier).", EditorStyles.miniLabel);

        for (int i = 0; i < kf.Data.Count; i++)
        {
            var d = kf.Data[i];
            if (d == null)
            {
                d = new OXKeyframeObjectState();
                kf.Data[i] = d;
            }
            if (d.Transform == null) d.Transform = new OXTransformWithScale { Rotation = Quaternion.identity, Scale = Vector3.one };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // left column: which object this state belongs to (+ remove)
            EditorGUILayout.BeginVertical(GUILayout.Width(110f));

            // label and number side by side
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object", GUILayout.Width(44f));
            EditorGUI.BeginChangeCheck();
            int oi = EditorGUILayout.IntField(d.ObjectIndex);
            if (EditorGUI.EndChangeCheck()) Edit("Change Object Index", () => d.ObjectIndex = Mathf.Max(0, oi));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Duplicate", "Copy this object state onto the next free object index")))
                pendingDuplicate = i;
            if (GUILayout.Button(new GUIContent("Delete", "Remove this object state")))
                pendingRemove = i;
            EditorGUILayout.EndVertical();

            // right column: the enabled channel values for that object
            EditorGUILayout.BeginVertical();
            var tr = d.Transform;

            // Only channels enabled on this keyframe are shown; disabled ones can't be edited.
            if (kf.Position.Enabled)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 p = Vector3Row("Position", tr.Position, LaneColors[1]);
                if (EditorGUI.EndChangeCheck()) Edit("Change Position", () => tr.Position = p);
            }

            if (kf.Rotation.Enabled)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion q = IsZeroQuat(tr.Rotation) ? Quaternion.identity : tr.Rotation;
                Vector3 eul = Vector3Row("Rotation", q.eulerAngles, LaneColors[2]);
                if (EditorGUI.EndChangeCheck()) Edit("Change Rotation", () => tr.Rotation = Quaternion.Euler(eul));
            }

            if (kf.Scale.Enabled)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 s = Vector3Row("Scale", tr.Scale, LaneColors[3]);
                if (EditorGUI.EndChangeCheck()) Edit("Change Scale", () => tr.Scale = s);
            }

            if (!kf.Position.Enabled && !kf.Rotation.Enabled && !kf.Scale.Enabled)
                EditorGUILayout.LabelField("No channels enabled on this keyframe.", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical(); // channel rows
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical(); // help box
        }

        if (GUILayout.Button("+ Add Object State"))
        {
            Edit("Add Object State", () =>
            {
                int next = 0;
                foreach (var d in kf.Data)
                    if (d != null && d.ObjectIndex >= next) next = d.ObjectIndex + 1;
                kf.Data.Add(MakeState(next, kf.Time, kf));
            });
            Mutated();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUIUtility.labelWidth = oldLabelWidth;

        if (pendingDuplicate >= 0 && pendingDuplicate < kf.Data.Count)
        {
            int src = pendingDuplicate;
            Edit("Duplicate Object State", () =>
            {
                var from = kf.Data[src];
                int next = 0;
                foreach (var d in kf.Data)
                    if (d != null && d.ObjectIndex >= next) next = d.ObjectIndex + 1;

                var copy = new OXKeyframeObjectState { ObjectIndex = next };
                copy.Transform = new OXTransformWithScale
                {
                    Position = from.Transform.Position,
                    Rotation = IsZeroQuat(from.Transform.Rotation) ? Quaternion.identity : from.Transform.Rotation,
                    Scale = from.Transform.Scale,
                };
                kf.Data.Insert(src + 1, copy);
            });
            Mutated();
        }

        if (pendingRemove >= 0)
        {
            int r = pendingRemove;
            Edit("Remove Object State", () => kf.Data.RemoveAt(r));
            Mutated();
        }
    }

    /// <summary>When several keyframes share the selected key's time, lets you hop between them or merge them.</summary>
    private void DrawStackStrip(OXKeyframe kf)
    {
        var same = new List<int>();
        for (int i = 0; i < asset.Keyframes.Count; i++)
            if (asset.Keyframes[i] != null && Mathf.Abs(asset.Keyframes[i].Time - kf.Time) < SameTimeEpsilon)
                same.Add(i);
        if (same.Count < 2) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(same.Count + " keyframes share this time. Click to switch:", EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();
        foreach (var i in same.Take(8))
        {
            var k = asset.Keyframes[i];
            string text = "#" + i + " " + (LaneHas(k, 1) ? "P" : "") + (LaneHas(k, 2) ? "R" : "") + (LaneHas(k, 3) ? "S" : "");
            bool on = GUILayout.Toggle(i == primary, text, EditorStyles.miniButton);
            if (on && i != primary)
            {
                selected.Clear();
                selected.Add(i);
                primary = i;
                Mutated();
            }
        }
        if (same.Count > 8) GUILayout.Label("+" + (same.Count - 8), EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(new GUIContent("Merge These " + same.Count + " Into One",
                "Folds every channel and value into the lowest-numbered key. Later keys win if two enable the same channel.")))
        {
            MergeKeys(same);
            Mutated();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawChannel(string label, OXKeyframeChannel ch, Color color)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        var bar = GUILayoutUtility.GetRect(4f, 18f, GUILayout.Width(4f));
        EditorGUI.DrawRect(bar, color);
        EditorGUI.BeginChangeCheck();
        bool en = EditorGUILayout.ToggleLeft(label, ch.Enabled, EditorStyles.boldLabel);
        bool toggled = EditorGUI.EndChangeCheck();
        if (toggled) Edit("Toggle " + label, () => ch.Enabled = en);
        EditorGUILayout.EndHorizontal();
        if (toggled)
        {
            EditorGUILayout.EndVertical();
            Mutated(); // the Object States panel now shows/hides this channel, so restart the layout pass
        }

        // Disabled channel: nothing but the checkbox (no interpolation settings, no graph).
        if (!ch.Enabled)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        EditorGUI.BeginChangeCheck();
        var mode = (OXKeyframeInterpolationMode)EditorGUILayout.EnumPopup("Interpolation", ch.InterpMode);
        if (EditorGUI.EndChangeCheck())
        {
            Edit("Change Interpolation", () => ch.InterpMode = mode);
            EditorGUILayout.EndVertical();
            Mutated(); // the set of parameter fields below depends on the mode
        }

        switch (ch.InterpMode)
        {
            case OXKeyframeInterpolationMode.In:
            case OXKeyframeInterpolationMode.Out:
            case OXKeyframeInterpolationMode.InAndOut:
                FloatProp("Power", ch.Power, v => ch.Power = v);
                break;
            case OXKeyframeInterpolationMode.CircIn:
            case OXKeyframeInterpolationMode.CircOut:
                FloatProp("Circ Power", ch.CircPower, v => ch.CircPower = v);
                break;
            case OXKeyframeInterpolationMode.Bounce:
                IntProp("Bounces", ch.Bounces, v => ch.Bounces = Mathf.Max(1, v));
                FloatProp("Bounce Power", ch.BouncePower, v => ch.BouncePower = v);
                break;
            case OXKeyframeInterpolationMode.Elastic:
                FloatProp("Oscillations", ch.Oscillations, v => ch.Oscillations = v);
                break;
            case OXKeyframeInterpolationMode.Overshoot:
                FloatProp("Magnification", ch.Magnification, v => ch.Magnification = v);
                FloatProp("Overshoot Power", ch.OvershootPower, v => ch.OvershootPower = v);
                break;
        }

        EditorGUILayout.EndVertical();
        DrawEasePreview(ch, color);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    /// <summary>A label with its X/Y/Z fields on the same row, plus the channel's color as a side bar and a faint tint.</summary>
    private static Vector3 Vector3Row(string label, Vector3 value, Color color)
    {
        Rect row = EditorGUILayout.BeginHorizontal();
        if (Event.current.type == EventType.Repaint)
            EditorGUI.DrawRect(row, new Color(color.r, color.g, color.b, 0.10f));

        var bar = GUILayoutUtility.GetRect(4f, 18f, GUILayout.Width(4f), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(bar, color);

        GUILayout.Label(label, GUILayout.Width(56f));

        // Three separate fields so the layout never wraps the values onto a second line under the label.
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 12f;
        value.x = EditorGUILayout.FloatField("X", value.x);
        value.y = EditorGUILayout.FloatField("Y", value.y);
        value.z = EditorGUILayout.FloatField("Z", value.z);
        EditorGUIUtility.labelWidth = oldLabelWidth;

        EditorGUILayout.EndHorizontal();
        return value;
    }

    private void FloatProp(string label, float value, Action<float> set)
    {
        EditorGUI.BeginChangeCheck();
        float v = EditorGUILayout.FloatField(label, value);
        if (EditorGUI.EndChangeCheck()) Edit("Change " + label, () => set(v));
    }

    private void IntProp(string label, int value, Action<int> set)
    {
        EditorGUI.BeginChangeCheck();
        int v = EditorGUILayout.IntField(label, value);
        if (EditorGUI.EndChangeCheck()) Edit("Change " + label, () => set(v));
    }

    private void DrawEasePreview(OXKeyframeChannel ch, Color color)
    {
        Rect r = GUILayoutUtility.GetRect(84f, 64f, GUILayout.Width(84f), GUILayout.Height(64f));
        if (Event.current.type != EventType.Repaint) return;

        EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.7f, 0.7f, 0.7f));

        const int N = 48;
        var vals = new float[N + 1];
        float lo = 0f, hi = 1f;
        for (int i = 0; i <= N; i++)
        {
            float v = ch.Evaluate(i / (float)N);
            if (float.IsNaN(v) || float.IsInfinity(v)) v = 0f;
            vals[i] = v;
            if (v < lo) lo = v;
            if (v > hi) hi = v;
        }
        if (hi - lo < 1e-4f) hi = lo + 1f;

        var pts = new Vector3[N + 1];
        for (int i = 0; i <= N; i++)
        {
            float x = r.x + 4f + (r.width - 8f) * i / N;
            float y = r.yMax - 4f - (r.height - 8f) * (vals[i] - lo) / (hi - lo);
            pts[i] = new Vector3(x, y);
        }
        Handles.color = color;
        Handles.DrawAAPolyLine(2f, pts);
        Handles.color = Color.white;
    }
}
