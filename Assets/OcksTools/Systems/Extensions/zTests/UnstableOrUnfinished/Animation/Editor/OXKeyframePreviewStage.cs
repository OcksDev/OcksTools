// PUT THIS FILE IN A FOLDER NAMED "Editor" (e.g. Assets/OcksTools/Editor/OXKeyframePreviewStage.cs)
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// An editor-only "stage" (like Prefab Mode) that swaps the Scene view over to a preview scene while an
/// OXKeyframeAnimation asset is being edited. The scene itself is never saved and never part of a build, but
/// its contents ARE saved: every root object in it is mirrored into asset.PreviewObjects (kind / prefab /
/// object index / rest transform) and respawned the next time the stage opens.
///
/// What goes in the scene:
///   - Use the timeline window's "+ Add" menu / prefab field, or drag prefabs into the Scene view.
///     Prefab instances, built-in primitives and bare empty GameObjects are picked up automatically.
///   - Root objects only (children of those are part of the prefab, not tracked individually).
///
/// Object index:
///   - Each tracked object has an ObjectIndex = the "Object" number in the keyframes' Object States.
///   - -1 means scenery (shown, never animated).
///
/// Posing:
///   - Pose(t) resets every object to its REST pose and then samples the animation at time t using the
///     normal runtime, so what you scrub is exactly what Play() would do.
///   - While posed, scene-view moves are not remembered; turn Pose off in the window to move objects.
/// </summary>
public class OXKeyframePreviewStage : PreviewSceneStage
{
    private const float Eps = 0.0001f;
    private const int MaxPreviewIndex = 256;

    [SerializeField] private OXKeyframeAnimation asset;

    // Parallel to asset.PreviewObjects: live[i] is the scene object for PreviewObjects[i].
    private readonly List<GameObject> live = new List<GameObject>();
    private readonly HashSet<int> warned = new HashSet<int>();

    /// <summary>The animation asset this stage was opened for.</summary>
    public OXKeyframeAnimation Asset { get { return asset; } }

    /// <summary>True while the objects are showing an animated pose instead of their rest pose.</summary>
    public bool Posed { get; private set; }

    public int Count { get { return asset != null ? Entries.Count : 0; } }

    public GameObject GetLive(int i) { return i >= 0 && i < live.Count ? live[i] : null; }

    private List<OXPreviewObject> Entries
    {
        get
        {
            if (asset.PreviewObjects == null) asset.PreviewObjects = new List<OXPreviewObject>();
            return asset.PreviewObjects;
        }
    }

    // ------------------------------------------------------------------ opening / closing

    /// <summary>Switches the Scene view to the preview scene for this asset (no-op if it is already showing it).</summary>
    public static void Open(OXKeyframeAnimation a)
    {
        if (a == null) return;
        if (Current(a) != null) return; // already open (a stale stage left over from a script reload doesn't count)

        var stage = CreateInstance<OXKeyframePreviewStage>();
        stage.asset = a;
        // true = this becomes the first stage after the main scene, replacing any other preview we had open.
        StageUtility.GoToStage(stage, true);
    }

    // Stages that are open right now. Looked up by asset instead of via StageUtility.GetCurrentStage(), which
    // depends on which Scene view is focused. Cleared by a script reload, which also kills the preview scenes.
    private static readonly List<OXKeyframePreviewStage> openStages = new List<OXKeyframePreviewStage>();

    /// <summary>Bumped whenever the live objects were rebuilt or the saved list changed, so the window knows to re-pose.</summary>
    public int Version { get; private set; }

    /// <summary>The open stage for this asset, or null if none is open (or it is left over from before a script reload).</summary>
    public static OXKeyframePreviewStage Current(OXKeyframeAnimation a)
    {
        if (a == null) return null;
        for (int i = openStages.Count - 1; i >= 0; i--)
        {
            var s = openStages[i];
            if (s == null) { openStages.RemoveAt(i); continue; }
            if (s.asset == a && s.scene.IsValid()) return s;
        }
        return null;
    }

    protected override bool OnOpenStage()
    {
        base.OnOpenStage();
        scene = EditorSceneManager.NewPreviewScene();
        openStages.Add(this);
        Rebuild();
        return true;
    }

    protected override void OnCloseStage()
    {
        openStages.Remove(this);
        if (scene.IsValid())
        {
            if (!Posed) CaptureRest(); // keep any last move that the sync tick hasn't seen yet
            EditorSceneManager.ClosePreviewScene(scene);
        }
        live.Clear();
        base.OnCloseStage();
    }

    protected override void OnFirstTimeOpenStageInSceneView(SceneView sceneView)
    {
        sceneView.Frame(new Bounds(Vector3.zero, Vector3.one * 6f), true);
    }

    protected override GUIContent CreateHeaderContent()
    {
        string title = asset != null ? asset.name : "Keyframe Animation";
        Texture icon = asset != null ? EditorGUIUtility.ObjectContent(asset, typeof(OXKeyframeAnimation)).image : null;
        return new GUIContent(title, icon);
    }

    // ------------------------------------------------------------------ spawning

    /// <summary>Throws away every live object and respawns them all from the asset's saved list.</summary>
    public void Rebuild()
    {
        foreach (var go in live)
            if (go != null) DestroyImmediate(go);
        live.Clear();
        Posed = false;
        Version++;
        if (asset == null || !scene.IsValid()) return;
        foreach (var p in Entries) live.Add(Spawn(p));
    }

    /// <summary>After an undo/redo: rebuild if the live objects no longer match the saved list, otherwise just reset to rest.</summary>
    public void Reconcile()
    {
        if (asset == null || !scene.IsValid()) return;
        if (live.Count != Entries.Count || live.Any(g => g == null)) Rebuild();
        else { RestoreRest(); Posed = false; Version++; }
    }

    private GameObject Spawn(OXPreviewObject p)
    {
        GameObject go = null;
        switch (p.Kind)
        {
            case OXPreviewObjectKind.Prefab:
                if (p.Prefab != null) go = PrefabUtility.InstantiatePrefab(p.Prefab, scene) as GameObject;
                break;
            case OXPreviewObjectKind.Empty:
                break;
            default:
                go = GameObject.CreatePrimitive(ToPrimitive(p.Kind));
                break;
        }
        if (go == null) go = new GameObject(); // Empty, or a prefab that has since been deleted
        if (go.scene != scene) SceneManager.MoveGameObjectToScene(go, scene);

        go.name = string.IsNullOrEmpty(p.Name) ? "Object" : p.Name;
        var t = go.transform;
        t.localPosition = p.Position;
        t.localRotation = IsZeroQuat(p.Rotation) ? Quaternion.identity : p.Rotation;
        t.localScale = p.Scale;
        return go;
    }

    private static PrimitiveType ToPrimitive(OXPreviewObjectKind k)
    {
        switch (k)
        {
            case OXPreviewObjectKind.Sphere: return PrimitiveType.Sphere;
            case OXPreviewObjectKind.Capsule: return PrimitiveType.Capsule;
            case OXPreviewObjectKind.Cylinder: return PrimitiveType.Cylinder;
            case OXPreviewObjectKind.Plane: return PrimitiveType.Plane;
            case OXPreviewObjectKind.Quad: return PrimitiveType.Quad;
            default: return PrimitiveType.Cube;
        }
    }

    private static bool IsZeroQuat(Quaternion q) { return q.x == 0f && q.y == 0f && q.z == 0f && q.w == 0f; }

    // ------------------------------------------------------------------ editing the list (from the window)

    /// <summary>Adds a primitive / empty / prefab to the scene and the saved list. Gets the next free object index.</summary>
    public int Add(OXPreviewObjectKind kind, GameObject prefab = null)
    {
        if (asset == null || !scene.IsValid()) return -1;
        if (kind == OXPreviewObjectKind.Prefab && prefab == null) return -1;

        Undo.RecordObject(asset, "Add Preview Object");
        var p = new OXPreviewObject
        {
            Kind = kind,
            Prefab = kind == OXPreviewObjectKind.Prefab ? prefab : null,
            Name = kind == OXPreviewObjectKind.Prefab ? prefab.name : kind.ToString(),
            ObjectIndex = NextFreeIndex(),
        };
        Entries.Add(p);
        var go = Spawn(p);
        live.Add(go);
        EditorUtility.SetDirty(asset);
        Selection.activeGameObject = go;
        return Entries.Count - 1;
    }

    public void RemoveAt(int i)
    {
        if (asset == null || i < 0 || i >= Entries.Count) return;
        Undo.RecordObject(asset, "Remove Preview Object");
        if (i < live.Count)
        {
            if (live[i] != null) DestroyImmediate(live[i]);
            live.RemoveAt(i);
        }
        Entries.RemoveAt(i);
        EditorUtility.SetDirty(asset);
    }

    /// <summary>Replaces the live object for entry i with a fresh spawn (used after its prefab/kind changed).</summary>
    public void Respawn(int i)
    {
        if (asset == null || i < 0 || i >= Entries.Count || i >= live.Count) return;
        if (live[i] != null) DestroyImmediate(live[i]);
        live[i] = Spawn(Entries[i]);
        Version++;
    }

    public void SetName(int i, string newName)
    {
        if (asset == null || i < 0 || i >= Entries.Count) return;
        Undo.RecordObject(asset, "Rename Preview Object");
        Entries[i].Name = newName;
        if (i < live.Count && live[i] != null) live[i].name = newName;
        EditorUtility.SetDirty(asset);
    }

    public void SetIndex(int i, int index)
    {
        if (asset == null || i < 0 || i >= Entries.Count) return;
        Undo.RecordObject(asset, "Change Preview Object Index");
        Entries[i].ObjectIndex = Mathf.Max(-1, index);
        EditorUtility.SetDirty(asset);
    }

    /// <summary>
    /// Hooks a scene object up to the next free object index (adopting it into the saved list first if it
    /// wasn't tracked yet). Returns the index, or -1 if the object can't be saved (see log).
    /// </summary>
    public int Hook(GameObject go)
    {
        if (asset == null || go == null || go.scene != scene) return -1;
        go = go.transform.root.gameObject;

        int i = live.IndexOf(go);
        if (i < 0)
        {
            if (!Describe(go, out var p))
            {
                Debug.LogWarning("[Keyframe Preview] '" + go.name + "' can't be saved with the animation. " +
                                 "Use a prefab instance, a built-in primitive or an empty GameObject.", go);
                return -1;
            }
            Undo.RecordObject(asset, "Hook Preview Object");
            Entries.Add(p);
            live.Add(go);
            i = Entries.Count - 1;
        }
        else Undo.RecordObject(asset, "Hook Preview Object");

        Entries[i].ObjectIndex = NextFreeIndex();
        EditorUtility.SetDirty(asset);
        Version++;
        return Entries[i].ObjectIndex;
    }

    /// <summary>
    /// The index a new object should get: the lowest index that keyframes already use but no object is hooked to,
    /// otherwise the lowest unused index.
    /// </summary>
    public int NextFreeIndex()
    {
        int unhooked = NextUnhookedKeyIndex();
        if (unhooked >= 0) return unhooked;

        var taken = new HashSet<int>(Entries.Where(e => e != null && e.ObjectIndex >= 0).Select(e => e.ObjectIndex));
        int n = 0;
        while (taken.Contains(n)) n++;
        return n;
    }

    /// <summary>The lowest object index that keyframes animate but no preview object is hooked to, or -1 if there is none.</summary>
    public int NextUnhookedKeyIndex()
    {
        var taken = new HashSet<int>(Entries.Where(e => e != null && e.ObjectIndex >= 0).Select(e => e.ObjectIndex));
        var used = new SortedSet<int>();
        foreach (var k in asset.Keyframes)
            if (k != null && k.Data != null)
                foreach (var d in k.Data)
                    if (d != null && d.ObjectIndex >= 0) used.Add(d.ObjectIndex);
        foreach (var u in used)
            if (!taken.Contains(u)) return u;
        return -1;
    }

    // ------------------------------------------------------------------ keeping the saved list in sync with the scene

    /// <summary>
    /// Called regularly by the window. Drops entries whose object was deleted, picks up new root objects the
    /// user dragged in, and (when not posed) saves any moves/renames. Returns true if the asset changed.
    /// </summary>
    public bool Sync()
    {
        if (asset == null || !scene.IsValid()) return false;
        var entries = Entries;

        if (live.Count != entries.Count)
        {
            Rebuild(); // bumps Version
            return true;
        }

        bool changed = false;

        for (int i = live.Count - 1; i >= 0; i--)
        {
            if (live[i] != null) continue;
            entries.RemoveAt(i);
            live.RemoveAt(i);
            changed = true;
        }

        var tracked = new HashSet<GameObject>(live);
        foreach (var root in scene.GetRootGameObjects())
        {
            if (tracked.Contains(root)) continue;
            if (!Describe(root, out var p))
            {
                if (warned.Add(root.GetInstanceID()))
                    Debug.LogWarning("[Keyframe Preview] '" + root.name + "' won't be saved with the animation " +
                                     "(only prefab instances, built-in primitives and empty GameObjects are).", root);
                continue;
            }
            // Objects dragged in are hooked to an index the keyframes use but nothing owns yet; otherwise they are scenery.
            p.ObjectIndex = NextUnhookedKeyIndex();
            entries.Add(p);
            live.Add(root);
            changed = true;
        }

        if (!Posed) changed |= CaptureRest();
        if (changed)
        {
            EditorUtility.SetDirty(asset);
            Version++;
        }
        return changed;
    }

    /// <summary>Works out how to respawn an arbitrary scene object. Fails for objects we can't recreate.</summary>
    private bool Describe(GameObject go, out OXPreviewObject p)
    {
        p = new OXPreviewObject { Name = go.name, ObjectIndex = -1 };

        if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
        {
            var src = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (src != null)
            {
                p.Kind = OXPreviewObjectKind.Prefab;
                p.Prefab = src;
                ReadTransform(go, p);
                return true;
            }
        }

        var mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
            bool builtin = string.IsNullOrEmpty(path) || path.Contains("unity default resources");
            if (builtin)
            {
                switch (mf.sharedMesh.name)
                {
                    case "Cube": p.Kind = OXPreviewObjectKind.Cube; break;
                    case "Sphere": p.Kind = OXPreviewObjectKind.Sphere; break;
                    case "Capsule": p.Kind = OXPreviewObjectKind.Capsule; break;
                    case "Cylinder": p.Kind = OXPreviewObjectKind.Cylinder; break;
                    case "Plane": p.Kind = OXPreviewObjectKind.Plane; break;
                    case "Quad": p.Kind = OXPreviewObjectKind.Quad; break;
                    default: return false;
                }
                ReadTransform(go, p);
                return true;
            }
        }

        if (go.GetComponents<Component>().Length == 1) // just a Transform
        {
            p.Kind = OXPreviewObjectKind.Empty;
            ReadTransform(go, p);
            return true;
        }
        return false;
    }

    private static void ReadTransform(GameObject go, OXPreviewObject p)
    {
        var t = go.transform;
        p.Position = t.localPosition;
        p.Rotation = t.localRotation;
        p.Scale = t.localScale;
    }

    // ------------------------------------------------------------------ rest pose / posing

    /// <summary>Copies the live transforms (and names) into the saved list. Does nothing while posed.</summary>
    public bool CaptureRest()
    {
        if (Posed || asset == null) return false;
        var entries = Entries;
        bool changed = false;
        int n = Mathf.Min(entries.Count, live.Count);
        for (int i = 0; i < n; i++)
        {
            var go = live[i];
            if (go == null) continue;
            var p = entries[i];
            var t = go.transform;
            bool diff =
                (t.localPosition - p.Position).sqrMagnitude > Eps * Eps ||
                (t.localScale - p.Scale).sqrMagnitude > Eps * Eps ||
                Quaternion.Angle(t.localRotation, IsZeroQuat(p.Rotation) ? Quaternion.identity : p.Rotation) > 0.01f ||
                p.Name != go.name;
            if (!diff) continue;
            ReadTransform(go, p);
            p.Name = go.name;
            changed = true;
        }
        if (changed) EditorUtility.SetDirty(asset);
        return changed;
    }

    /// <summary>Puts every live object back to its saved rest pose.</summary>
    public void RestoreRest()
    {
        var entries = Entries;
        int n = Mathf.Min(entries.Count, live.Count);
        for (int i = 0; i < n; i++)
        {
            var go = live[i];
            if (go == null) continue;
            var p = entries[i];
            var t = go.transform;
            t.localPosition = p.Position;
            t.localRotation = IsZeroQuat(p.Rotation) ? Quaternion.identity : p.Rotation;
            t.localScale = p.Scale;
        }
    }

    /// <summary>Returns to the rest pose so objects can be moved around.</summary>
    public void Unpose()
    {
        if (asset == null || !scene.IsValid()) return;
        RestoreRest();
        Posed = false;
    }

    /// <summary>
    /// Shows the animation at time t: rest pose first, then the runtime's own Sample, with each object
    /// handed over at its object index. Objects with index -1 (scenery) or a duplicate index are left at rest.
    /// </summary>
    public void Pose(float t)
    {
        if (asset == null || !scene.IsValid()) return;
        if (!Posed) CaptureRest(); // don't lose a move made just before posing
        RestoreRest();

        var entries = Entries;
        var list = new List<GameObject>();
        var used = new HashSet<int>();
        for (int i = 0; i < entries.Count && i < live.Count; i++)
        {
            int idx = entries[i].ObjectIndex;
            if (idx < 0 || idx > MaxPreviewIndex || live[i] == null || !used.Add(idx)) continue;
            while (list.Count <= idx) list.Add(null);
            list[idx] = live[i];
        }

        if (list.Count > 0)
            new OXKeyframeAnimationRuntime(asset, null, list).Sample(t); // snapshots rest, then poses
        Posed = true;
    }
}
