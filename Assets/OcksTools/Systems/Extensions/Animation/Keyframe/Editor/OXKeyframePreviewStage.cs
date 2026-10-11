// PUT THIS FILE IN A FOLDER NAMED "Editor" (e.g. Assets/OcksTools/Editor/OXKeyframePreviewStage.cs)
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only stage (like Prefab Mode) that swaps the Scene view to a preview scene while an OXKeyframeAnimation is edited.
/// The scene is never saved or built, but every root object in it is mirrored into asset.PreviewObjects and respawned
/// on the next open. Prefab instances, built-in primitives, sprites and empty GameObjects are picked up automatically
/// (root objects only). Each tracked object has an ObjectIndex matching the keyframes' Object States (-1 = scenery).
/// Pose(t) resets objects to rest and samples the normal runtime, so scrubbing matches Play().
/// </summary>
public class OXKeyframePreviewStage : PreviewSceneStage
{
    private const float Eps = 0.0001f;
    private const int MaxPreviewIndex = 256;

    [SerializeField] private OXKeyframeAnimation asset;

    // live[i] is the scene object for asset.PreviewObjects[i].
    private readonly List<GameObject> live = new List<GameObject>();
    private readonly HashSet<int> warned = new HashSet<int>();

    // Looked up by asset rather than StageUtility.GetCurrentStage(), which depends on the focused Scene view.
    private static readonly List<OXKeyframePreviewStage> openStages = new List<OXKeyframePreviewStage>();

    public OXKeyframeAnimation Asset => asset;

    /// <summary>True while objects show an animated pose instead of their rest pose.</summary>
    public bool Posed { get; private set; }

    /// <summary>Bumped when live objects are rebuilt or the saved list changes, so the window knows to re-pose.</summary>
    public int Version { get; private set; }

    public int Count => asset != null ? Entries.Count : 0;

    public GameObject GetLive(int i) => i >= 0 && i < live.Count ? live[i] : null;

    private List<OXPreviewObject> Entries
    {
        get
        {
            if (asset.PreviewObjects == null) asset.PreviewObjects = new List<OXPreviewObject>();
            return asset.PreviewObjects;
        }
    }

    private bool Valid => asset != null && scene.IsValid();

    private bool InRange(int i) => asset != null && i >= 0 && i < Entries.Count;

    // ------------------------------------------------------------------ opening / closing

    /// <summary>Switches the Scene view to this asset's preview scene (no-op if already showing it).</summary>
    public static void Open(OXKeyframeAnimation a)
    {
        if (a == null || Current(a) != null) return;
        var stage = CreateInstance<OXKeyframePreviewStage>();
        stage.asset = a;
        StageUtility.GoToStage(stage, true);
    }

    /// <summary>The open stage for this asset, or null (stale stages from before a script reload don't count).</summary>
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
            if (!Posed) CaptureRest();
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

    /// <summary>Destroys every live object and respawns them from the saved list.</summary>
    public void Rebuild()
    {
        foreach (var go in live)
            if (go != null) DestroyImmediate(go);
        live.Clear();
        Posed = false;
        Version++;
        if (!Valid) return;
        foreach (var p in Entries) live.Add(Spawn(p));
    }

    /// <summary>After undo/redo: rebuild if live objects no longer match the saved list, otherwise reset to rest.</summary>
    public void Reconcile()
    {
        if (!Valid) return;
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
            case OXPreviewObjectKind.Sprite:
            case OXPreviewObjectKind.Square2D:
            case OXPreviewObjectKind.Circle2D:
            case OXPreviewObjectKind.Triangle2D:
            case OXPreviewObjectKind.Capsule2D:
            case OXPreviewObjectKind.Diamond2D:
            case OXPreviewObjectKind.Hexagon2D:
                go = new GameObject();
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = p.Kind == OXPreviewObjectKind.Sprite ? p.Sprite : OXPreviewSprites.Get(p.Kind);
                sr.color = p.Color == default(Color) ? Color.white : p.Color;
                break;
            default:
                go = GameObject.CreatePrimitive(ToPrimitive(p.Kind));
                break;
        }
        if (go == null) go = new GameObject(); // Empty, or a prefab that has since been deleted
        if (go.scene != scene) SceneManager.MoveGameObjectToScene(go, scene);

        go.name = string.IsNullOrEmpty(p.Name) ? "Object" : p.Name;
        ApplyRest(go.transform, p);
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

    private static bool IsZeroQuat(Quaternion q) => q.x == 0f && q.y == 0f && q.z == 0f && q.w == 0f;

    private static Quaternion RestRotation(OXPreviewObject p) => IsZeroQuat(p.Rotation) ? Quaternion.identity : p.Rotation;

    private static void ApplyRest(Transform t, OXPreviewObject p)
    {
        t.localPosition = p.Position;
        t.localRotation = RestRotation(p);
        t.localScale = p.Scale;
    }

    // ------------------------------------------------------------------ editing the list (from the window)

    /// <summary>Adds a primitive / empty / prefab / sprite to the scene and saved list with the next free object index.</summary>
    public int Add(OXPreviewObjectKind kind, GameObject prefab = null, Sprite sprite = null)
    {
        if (!Valid) return -1;
        if (kind == OXPreviewObjectKind.Prefab && prefab == null) return -1;
        if (kind == OXPreviewObjectKind.Sprite && sprite == null) return -1;

        Undo.RecordObject(asset, "Add Preview Object");
        var p = new OXPreviewObject
        {
            Kind = kind,
            Prefab = kind == OXPreviewObjectKind.Prefab ? prefab : null,
            Sprite = kind == OXPreviewObjectKind.Sprite ? sprite : null,
            Name = kind == OXPreviewObjectKind.Prefab ? prefab.name
                 : kind == OXPreviewObjectKind.Sprite ? sprite.name
                 : kind.ToString().Replace("2D", " (2D)"),
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
        if (!InRange(i)) return;
        Undo.RecordObject(asset, "Remove Preview Object");
        if (i < live.Count)
        {
            if (live[i] != null) DestroyImmediate(live[i]);
            live.RemoveAt(i);
        }
        Entries.RemoveAt(i);
        EditorUtility.SetDirty(asset);
    }

    /// <summary>Replaces entry i's live object with a fresh spawn (after its prefab/sprite changed).</summary>
    public void Respawn(int i)
    {
        if (!InRange(i) || i >= live.Count) return;
        if (live[i] != null) DestroyImmediate(live[i]);
        live[i] = Spawn(Entries[i]);
        Version++;
    }

    public void SetName(int i, string newName)
    {
        if (!InRange(i)) return;
        Undo.RecordObject(asset, "Rename Preview Object");
        Entries[i].Name = newName;
        if (i < live.Count && live[i] != null) live[i].name = newName;
        EditorUtility.SetDirty(asset);
    }

    public void SetIndex(int i, int index)
    {
        if (!InRange(i)) return;
        Undo.RecordObject(asset, "Change Preview Object Index");
        Entries[i].ObjectIndex = Mathf.Max(-1, index);
        EditorUtility.SetDirty(asset);
    }

    /// <summary>
    /// Hooks a scene object to the next free object index, adopting it into the saved list if untracked.
    /// Returns the index, or -1 if the object can't be saved.
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
                                 "Use a prefab instance, a built-in primitive, a sprite or an empty GameObject.", go);
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

    private HashSet<int> TakenIndices()
    {
        return new HashSet<int>(Entries.Where(e => e != null && e.ObjectIndex >= 0).Select(e => e.ObjectIndex));
    }

    /// <summary>The lowest unused object index (0, 1, 2, ...).</summary>
    public int NextFreeIndex()
    {
        var taken = TakenIndices();
        int n = 0;
        while (taken.Contains(n)) n++;
        return n;
    }

    /// <summary>The lowest object index keyframes animate that no preview object is hooked to, or -1.</summary>
    public int NextUnhookedKeyIndex()
    {
        var taken = TakenIndices();
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
    /// Called regularly by the window: drops entries whose object was deleted, adopts new root objects, and (when not posed)
    /// saves moves/renames. Returns true if the asset changed.
    /// </summary>
    public bool Sync()
    {
        if (!Valid) return false;
        var entries = Entries;

        if (live.Count != entries.Count)
        {
            Rebuild();
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
                                     "(only prefab instances, built-in primitives, sprites and empty GameObjects are).", root);
                continue;
            }
            p.ObjectIndex = NextFreeIndex();
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

    /// <summary>Works out how to respawn a scene object. Fails for objects that can't be recreated.</summary>
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

        // Anything with a SpriteRenderer; other components (colliders, rigidbodies) are ignored.
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (OXPreviewSprites.TryGetKind(sr.sprite, out var shape)) p.Kind = shape;
            else { p.Kind = OXPreviewObjectKind.Sprite; p.Sprite = sr.sprite; }
            p.Color = sr.color;
            ReadTransform(go, p);
            return true;
        }

        var mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (string.IsNullOrEmpty(path) || path.Contains("unity default resources"))
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

    /// <summary>Copies live transforms, names and sprite tints into the saved list. Does nothing while posed.</summary>
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

            SpriteRenderer sr = p.Kind >= OXPreviewObjectKind.Sprite ? go.GetComponent<SpriteRenderer>() : null;
            bool colorDiff = sr != null && sr.color != p.Color;
            bool diff = colorDiff ||
                (t.localPosition - p.Position).sqrMagnitude > Eps * Eps ||
                (t.localScale - p.Scale).sqrMagnitude > Eps * Eps ||
                Quaternion.Angle(t.localRotation, RestRotation(p)) > 0.01f ||
                p.Name != go.name;
            if (!diff) continue;

            ReadTransform(go, p);
            p.Name = go.name;
            if (colorDiff) p.Color = sr.color;
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
            if (live[i] != null) ApplyRest(live[i].transform, entries[i]);
    }

    /// <summary>Returns to the rest pose so objects can be moved around.</summary>
    public void Unpose()
    {
        if (!Valid) return;
        RestoreRest();
        Posed = false;
    }

    /// <summary>
    /// Shows the animation at time t: rest pose first, then the runtime's Sample with each object at its object index.
    /// Scenery (-1) and duplicate indices stay at rest.
    /// </summary>
    public void Pose(float t, bool wrapped = false)
    {
        if (!Valid) return;
        if (!Posed) CaptureRest();
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
            new OXKeyframeAnimationRuntime(asset, null, list).Sample(t, wrapped);
        Posed = true;
    }
}

/// <summary>Generates the built-in 2D shape sprites (white, 1 unit across, so the tint shows) on demand.</summary>
public static class OXPreviewSprites
{
    private const int Size = 64;
    private const int SuperSample = 4;
    private const string Prefix = "OX2D_";
    private static readonly Dictionary<OXPreviewObjectKind, Sprite> cache = new Dictionary<OXPreviewObjectKind, Sprite>();

    public static bool IsShape(OXPreviewObjectKind k)
    {
        return k >= OXPreviewObjectKind.Square2D && k <= OXPreviewObjectKind.Hexagon2D;
    }

    /// <summary>Maps a generated sprite back to its kind, so duplicated or moved shapes are still recognised.</summary>
    public static bool TryGetKind(Sprite s, out OXPreviewObjectKind kind)
    {
        kind = OXPreviewObjectKind.Empty;
        if (s == null || !s.name.StartsWith(Prefix)) return false;
        try { kind = (OXPreviewObjectKind)System.Enum.Parse(typeof(OXPreviewObjectKind), s.name.Substring(Prefix.Length)); }
        catch { return false; }
        return IsShape(kind);
    }

    public static Sprite Get(OXPreviewObjectKind k)
    {
        if (!IsShape(k)) return null;
        if (cache.TryGetValue(k, out var sprite) && sprite != null) return sprite;

        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            name = Prefix + k,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };

        const int total = SuperSample * SuperSample;
        var px = new Color32[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SuperSample; sy++)
                    for (int sx = 0; sx < SuperSample; sx++)
                    {
                        float u = (x + (sx + 0.5f) / SuperSample) / Size * 2f - 1f;
                        float v = (y + (sy + 0.5f) / SuperSample) / Size * 2f - 1f;
                        if (Inside(k, u, v)) hit++;
                    }
                px[y * Size + x] = new Color32(255, 255, 255, (byte)(255 * hit / total));
            }
        }
        tex.SetPixels32(px);
        tex.Apply(false, false);

        sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        sprite.name = Prefix + k;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        cache[k] = sprite;
        return sprite;
    }

    // u, v in -1..1 across the texture.
    private static bool Inside(OXPreviewObjectKind k, float u, float v)
    {
        float au = Mathf.Abs(u), av = Mathf.Abs(v);
        switch (k)
        {
            case OXPreviewObjectKind.Circle2D: return u * u + v * v <= 1f;
            case OXPreviewObjectKind.Triangle2D: return v >= -1f && au <= (1f - v) * 0.5f;
            case OXPreviewObjectKind.Diamond2D: return au + av <= 1f;
            case OXPreviewObjectKind.Hexagon2D: return au <= 0.8660254f && av + au / 1.7320508f <= 1f;
            case OXPreviewObjectKind.Capsule2D:
                if (au > 0.5f) return false;
                if (av <= 0.5f) return true;
                return au * au + (av - 0.5f) * (av - 0.5f) <= 0.25f;
            default: return true; // Square2D
        }
    }
}
