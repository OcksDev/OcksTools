using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ColliderGizmoOverride : MonoBehaviour
{
    [SerializeField] private bool onlyWhenSelected = true;
    [SerializeField] private bool drawFill = false;
    [SerializeField] private bool drawWire = true;
    private bool hideBuiltInGizmo = true;

    [Header("Colors")]
    [SerializeField] private Color fillColor = new Color(0.4f, 0.95124f, 1, 0.25f);
    [SerializeField] private Color wireColor = new Color(0.4f, 0.95124f, 1, 1f);
    [SerializeField] private bool useSeparateTriggerColors = false;
    [NaughtyAttributes.ShowIf("useSeparateTriggerColors")]
    [SerializeField] private Color triggerFillColor = new Color(1, 0.6883331f, 0.2980392f, 0.25f);
    [NaughtyAttributes.ShowIf("useSeparateTriggerColors")]
    [SerializeField] private Color triggerWireColor = new Color(1, 0.6883331f, 0.2980392f, 1f);

    private int circleSegments = 55;

    // Scratch data reused every draw to limit allocations
    private readonly List<Collider> colliders3D = new List<Collider>();
    private readonly List<Collider2D> colliders2D = new List<Collider2D>();
    private readonly List<Vector3> pointBuffer = new List<Vector3>();
    private readonly List<Vector3> meshVerts = new List<Vector3>();
    private readonly List<Vector3> meshNormals = new List<Vector3>();
    private readonly List<int> meshTris = new List<int>();

    private Mesh capsuleMesh;  // generated, used by 3D capsule fill (single mesh = no overlapping alpha)
    private float capsuleMeshRadius = -1f;
    private float capsuleMeshHalf = -1f;
    private int capsuleMeshSegments = -1;
    private Mesh fillMesh;     // generated, used by 2D fills

    public bool HideBuiltInGizmo => hideBuiltInGizmo;

    private void OnDrawGizmos()
    {
        if (!onlyWhenSelected) DrawAll();
    }

    private void OnDrawGizmosSelected()
    {
        if (onlyWhenSelected) DrawAll();
    }

    private void OnDisable() { CleanupMesh(); }
    private void OnDestroy() { CleanupMesh(); }

    private void CleanupMesh()
    {
        DestroyMesh(ref fillMesh);
        DestroyMesh(ref capsuleMesh);
        capsuleMeshRadius = capsuleMeshHalf = -1f;
        capsuleMeshSegments = -1;
    }

    private static void DestroyMesh(ref Mesh mesh)
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
        mesh = null;
    }

    private void DrawAll()
    {
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Color prevColor = Gizmos.color;

        GetComponents(colliders3D);
        foreach (Collider col in colliders3D)
        {
            if (col == null || !col.enabled) continue;
            GetColors(col.isTrigger, out Color fill, out Color wire);

            switch (col)
            {
                case BoxCollider box: DrawBox(box, fill, wire); break;
                case SphereCollider sphere: DrawSphere(sphere, fill, wire); break;
                case CapsuleCollider capsule: DrawCapsule(capsule, fill, wire); break;
                case MeshCollider meshCol: DrawMeshCollider(meshCol, fill, wire); break;
            }
        }

        GetComponents(colliders2D);
        foreach (Collider2D col in colliders2D)
        {
            if (col == null || !col.enabled) continue;
            GetColors(col.isTrigger, out Color fill, out Color wire);

            switch (col)
            {
                case BoxCollider2D box2D: DrawBox2D(box2D, fill, wire); break;
                case CircleCollider2D circle2D: DrawCircle2D(circle2D, fill, wire); break;
                case CapsuleCollider2D capsule2D: DrawCapsule2D(capsule2D, fill, wire); break;
                case PolygonCollider2D poly2D: DrawPolygon2D(poly2D, fill, wire); break;
                case EdgeCollider2D edge2D: DrawEdge2D(edge2D, wire); break;
                case CompositeCollider2D composite2D: DrawComposite2D(composite2D, fill, wire); break;
            }
        }

        Gizmos.matrix = prevMatrix;
        Gizmos.color = prevColor;
    }

    private void GetColors(bool isTrigger, out Color fill, out Color wire)
    {
        if (useSeparateTriggerColors && isTrigger)
        {
            fill = triggerFillColor;
            wire = triggerWireColor;
        }
        else
        {
            fill = fillColor;
            wire = wireColor;
        }
    }

    // =====================================================================
    //  3D
    // =====================================================================

    private void DrawBox(BoxCollider box, Color fill, Color wire)
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        if (drawFill)
        {
            Gizmos.color = fill;
            Gizmos.DrawCube(box.center, box.size);
        }
        if (drawWire)
        {
            Gizmos.color = wire;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }

    private void DrawSphere(SphereCollider sphere, Color fill, Color wire)
    {
        Vector3 s = transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        float radius = sphere.radius * maxScale;

        Vector3 worldCenter = transform.TransformPoint(sphere.center);
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, Vector3.one);

        if (drawFill)
        {
            Gizmos.color = fill;
            Gizmos.DrawSphere(Vector3.zero, radius);
        }
        if (drawWire)
        {
            Gizmos.color = wire;
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
    }

    private void DrawCapsule(CapsuleCollider capsule, Color fill, Color wire)
    {
        Vector3 s = transform.lossyScale;
        float ax = Mathf.Abs(s.x), ay = Mathf.Abs(s.y), az = Mathf.Abs(s.z);

        int dir = capsule.direction; // 0 = X, 1 = Y, 2 = Z
        float axisScale, radialScale;
        Quaternion axisRot;
        switch (dir)
        {
            case 0:
                axisScale = ax; radialScale = Mathf.Max(ay, az);
                axisRot = Quaternion.Euler(0f, 0f, 90f);
                break;
            case 2:
                axisScale = az; radialScale = Mathf.Max(ax, ay);
                axisRot = Quaternion.Euler(90f, 0f, 0f);
                break;
            default:
                axisScale = ay; radialScale = Mathf.Max(ax, az);
                axisRot = Quaternion.identity;
                break;
        }

        float radius = capsule.radius * radialScale;
        float height = Mathf.Max(capsule.height * axisScale, radius * 2f);
        float cylinderHalf = Mathf.Max(0f, height * 0.5f - radius);

        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation * axisRot, Vector3.one);

        Vector3 top = Vector3.up * cylinderHalf;
        Vector3 bottom = Vector3.down * cylinderHalf;

        if (drawFill)
        {
            // One single mesh for the whole capsule, so nothing overlaps and
            // the translucent fill stays an even density.
            EnsureCapsuleMesh(radius, cylinderHalf);
            Gizmos.color = fill;
            Gizmos.DrawMesh(capsuleMesh);
        }

        if (drawWire)
        {
            Gizmos.color = wire;

            // Only the OUTER half of each end sphere (plus its equator ring) is drawn,
            // so no hemispheres show up inside the capsule body.
            DrawWireCap(cylinderHalf, 1f, radius);
            DrawWireCap(-cylinderHalf, -1f, radius);

            Vector3[] offsets =
            {
                Vector3.right * radius, Vector3.left * radius,
                Vector3.forward * radius, Vector3.back * radius
            };
            foreach (Vector3 o in offsets)
                Gizmos.DrawLine(top + o, bottom + o);
        }
    }

    // Draws one hemisphere's wireframe (equator ring + two outward arcs) in the
    // current Gizmos.matrix. centerY is the hemisphere's center height,
    // direction is +1 for the top cap and -1 for the bottom cap.
    private void DrawWireCap(float centerY, float direction, float radius)
    {
        int segments = Mathf.Max(8, circleSegments);
        int arcSteps = Mathf.Max(4, segments / 2);

        // Equator ring (horizontal circle)
        Vector3 prev = new Vector3(radius, centerY, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = new Vector3(Mathf.Cos(a) * radius, centerY, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        // Outward arc in the XY plane, then in the ZY plane (half circles only)
        Vector3 prevX = new Vector3(radius, centerY, 0f);
        Vector3 prevZ = new Vector3(0f, centerY, radius);
        for (int i = 1; i <= arcSteps; i++)
        {
            float a = (i / (float)arcSteps) * Mathf.PI; // 0..PI sweeps over the pole
            float c = Mathf.Cos(a) * radius;
            float h = Mathf.Sin(a) * radius * direction;

            Vector3 nextX = new Vector3(c, centerY + h, 0f);
            Vector3 nextZ = new Vector3(0f, centerY + h, c);
            Gizmos.DrawLine(prevX, nextX);
            Gizmos.DrawLine(prevZ, nextZ);
            prevX = nextX;
            prevZ = nextZ;
        }
    }

    private void EnsureCapsuleMesh(float radius, float half)
    {
        int segments = Mathf.Max(8, circleSegments);

        if (capsuleMesh != null &&
            Mathf.Approximately(capsuleMeshRadius, radius) &&
            Mathf.Approximately(capsuleMeshHalf, half) &&
            capsuleMeshSegments == segments)
        {
            return;
        }

        if (capsuleMesh == null)
        {
            capsuleMesh = new Mesh { name = "ColliderGizmoCapsule", hideFlags = HideFlags.HideAndDontSave };
            capsuleMesh.MarkDynamic();
        }

        int capRings = Mathf.Max(3, segments / 4);

        // Ring profile from the top pole down to the bottom pole.
        // The step between the two equators is the straight cylinder section.
        List<Vector2> profile = new List<Vector2>(); // x = ring radius, y = height
        List<float> capCenters = new List<float>();   // y of the hemisphere centre (for normals)
        for (int i = 0; i <= capRings; i++)
        {
            float theta = (Mathf.PI * 0.5f) * (i / (float)capRings); // 0 = top pole, PI/2 = equator
            profile.Add(new Vector2(Mathf.Sin(theta) * radius, half + Mathf.Cos(theta) * radius));
            capCenters.Add(half);
        }
        for (int i = 0; i <= capRings; i++)
        {
            float theta = Mathf.PI * 0.5f + (Mathf.PI * 0.5f) * (i / (float)capRings); // equator -> bottom pole
            profile.Add(new Vector2(Mathf.Sin(theta) * radius, -half + Mathf.Cos(theta) * radius));
            capCenters.Add(-half);
        }

        List<Vector3> verts = new List<Vector3>(profile.Count * segments);
        List<Vector3> normals = new List<Vector3>(profile.Count * segments);
        for (int k = 0; k < profile.Count; k++)
        {
            for (int j = 0; j < segments; j++)
            {
                float phi = (j / (float)segments) * Mathf.PI * 2f;
                float cx = Mathf.Cos(phi);
                float cz = Mathf.Sin(phi);
                Vector3 v = new Vector3(profile[k].x * cx, profile[k].y, profile[k].x * cz);
                verts.Add(v);

                Vector3 n = new Vector3(v.x, v.y - capCenters[k], v.z);
                normals.Add(n.sqrMagnitude > 1e-12f ? n.normalized : new Vector3(0f, Mathf.Sign(v.y), 0f));
            }
        }

        // Outward-facing triangles only (single surface, no double layering)
        List<int> tris = new List<int>((profile.Count - 1) * segments * 6);
        for (int k = 0; k < profile.Count - 1; k++)
        {
            for (int j = 0; j < segments; j++)
            {
                int j2 = (j + 1) % segments;
                int a = k * segments + j;
                int b = k * segments + j2;
                int c = (k + 1) * segments + j;
                int d = (k + 1) * segments + j2;

                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(b); tris.Add(d); tris.Add(c);
            }
        }

        capsuleMesh.Clear();
        capsuleMesh.SetVertices(verts);
        capsuleMesh.SetNormals(normals);
        capsuleMesh.SetTriangles(tris, 0);

        capsuleMeshRadius = radius;
        capsuleMeshHalf = half;
        capsuleMeshSegments = segments;
    }

    private void DrawMeshCollider(MeshCollider meshCol, Color fill, Color wire)
    {
        Mesh mesh = meshCol.sharedMesh;
        if (mesh == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        if (drawFill)
        {
            Gizmos.color = fill;
            Gizmos.DrawMesh(mesh);
        }
        if (drawWire)
        {
            Gizmos.color = wire;
            Gizmos.DrawWireMesh(mesh);
        }
    }

    // =====================================================================
    //  2D
    // =====================================================================

    private void DrawBox2D(BoxCollider2D box, Color fill, Color wire)
    {
        Vector2 c = box.offset;
        Vector2 h = box.size * 0.5f;

        pointBuffer.Clear();
        pointBuffer.Add(new Vector3(c.x - h.x, c.y - h.y, 0f));
        pointBuffer.Add(new Vector3(c.x + h.x, c.y - h.y, 0f));
        pointBuffer.Add(new Vector3(c.x + h.x, c.y + h.y, 0f));
        pointBuffer.Add(new Vector3(c.x - h.x, c.y + h.y, 0f));

        Gizmos.matrix = transform.localToWorldMatrix;
        DrawPolygon(pointBuffer, fill, wire, true, true, true);
    }

    private void DrawCircle2D(CircleCollider2D circle, Color fill, Color wire)
    {
        Vector3 s = transform.lossyScale;
        float radius = circle.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
        Vector3 worldCenter = transform.TransformPoint(circle.offset);

        pointBuffer.Clear();
        for (int i = 0; i < circleSegments; i++)
        {
            float a = (i / (float)circleSegments) * Mathf.PI * 2f;
            pointBuffer.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }

        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, Vector3.one);
        DrawPolygon(pointBuffer, fill, wire, true, true, true);
    }

    private void DrawCapsule2D(CapsuleCollider2D capsule, Color fill, Color wire)
    {
        Vector3 s = transform.lossyScale;
        float w = capsule.size.x * Mathf.Abs(s.x);
        float h = capsule.size.y * Mathf.Abs(s.y);
        Vector3 worldCenter = transform.TransformPoint(capsule.offset);

        bool vertical = capsule.direction == CapsuleDirection2D.Vertical;
        float radius = vertical ? w * 0.5f : h * 0.5f;
        float half = vertical
            ? Mathf.Max(0f, h * 0.5f - radius)
            : Mathf.Max(0f, w * 0.5f - radius);

        int steps = Mathf.Max(4, circleSegments / 2);

        pointBuffer.Clear();
        if (vertical)
        {
            AddArc(pointBuffer, new Vector2(0f, half), radius, 0f, steps);
            AddArc(pointBuffer, new Vector2(0f, -half), radius, Mathf.PI, steps);
        }
        else
        {
            AddArc(pointBuffer, new Vector2(half, 0f), radius, -Mathf.PI * 0.5f, steps);
            AddArc(pointBuffer, new Vector2(-half, 0f), radius, Mathf.PI * 0.5f, steps);
        }

        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, Vector3.one);
        DrawPolygon(pointBuffer, fill, wire, true, true, true);
    }

    private static void AddArc(List<Vector3> pts, Vector2 center, float radius, float startAngle, int steps)
    {
        for (int i = 0; i <= steps; i++)
        {
            float a = startAngle + Mathf.PI * (i / (float)steps);
            pts.Add(new Vector3(center.x + Mathf.Cos(a) * radius,
                                center.y + Mathf.Sin(a) * radius, 0f));
        }
    }

    private void DrawPolygon2D(PolygonCollider2D poly, Color fill, Color wire)
    {
        List<Vector2[]> paths = new List<Vector2[]>(poly.pathCount);
        for (int i = 0; i < poly.pathCount; i++)
            paths.Add(poly.GetPath(i));

        DrawPaths(paths, poly.offset, fill, wire);
    }

    private void DrawComposite2D(CompositeCollider2D composite, Color fill, Color wire)
    {
        List<Vector2[]> paths = new List<Vector2[]>(composite.pathCount);
        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] arr = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, arr);
            paths.Add(arr);
        }

        DrawPaths(paths, composite.offset, fill, wire);
    }

    private void DrawEdge2D(EdgeCollider2D edge, Color wire)
    {
        if (!drawWire) return;

        Vector2[] pts = edge.points;
        Vector2 off = edge.offset;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = wire;
        for (int i = 0; i < pts.Length - 1; i++)
        {
            Gizmos.DrawLine((Vector3)(pts[i] + off), (Vector3)(pts[i + 1] + off));
        }
    }

    private void DrawPaths(List<Vector2[]> paths, Vector2 offset, Color fill, Color wire)
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int i = 0; i < paths.Count; i++)
        {
            Vector2[] path = paths[i];
            if (path == null || path.Length < 2) continue;

            pointBuffer.Clear();
            for (int p = 0; p < path.Length; p++)
                pointBuffer.Add(new Vector3(path[p].x + offset.x, path[p].y + offset.y, 0f));

            // A path nested inside an odd number of other paths is a hole
            bool isHole = IsHole(paths, i);
            DrawPolygon(pointBuffer, fill, wire, true, false, !isHole);
        }
    }

    private static bool IsHole(List<Vector2[]> paths, int index)
    {
        if (paths[index] == null || paths[index].Length == 0) return false;
        Vector2 probe = paths[index][0];

        int depth = 0;
        for (int j = 0; j < paths.Count; j++)
        {
            if (j == index || paths[j] == null || paths[j].Length < 3) continue;
            if (PointInPolygon(probe, paths[j])) depth++;
        }
        return depth % 2 == 1;
    }

    private static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    // =====================================================================
    //  Shared 2D polygon drawing (uses the current Gizmos.matrix)
    // =====================================================================

    private void DrawPolygon(List<Vector3> pts, Color fill, Color wire, bool closed, bool convex, bool allowFill)
    {
        if (pts.Count < 2) return;

        if (drawFill && allowFill && closed && pts.Count >= 3)
        {
            BuildFillMesh(pts, convex);
            Gizmos.color = fill;
            Gizmos.DrawMesh(fillMesh);
        }

        if (drawWire)
        {
            Gizmos.color = wire;
            int count = closed ? pts.Count : pts.Count - 1;
            for (int i = 0; i < count; i++)
                Gizmos.DrawLine(pts[i], pts[(i + 1) % pts.Count]);
        }
    }

    private void BuildFillMesh(List<Vector3> pts, bool convex)
    {
        if (fillMesh == null)
        {
            fillMesh = new Mesh { name = "ColliderGizmoFill", hideFlags = HideFlags.HideAndDontSave };
            fillMesh.MarkDynamic();
        }

        meshTris.Clear();
        if (convex) FanTriangulate(pts.Count, meshTris);
        else EarClip(pts, meshTris);

        // Duplicate triangles with reversed winding so the fill is visible from both sides
        int triCount = meshTris.Count;
        for (int i = 0; i < triCount; i += 3)
        {
            meshTris.Add(meshTris[i]);
            meshTris.Add(meshTris[i + 2]);
            meshTris.Add(meshTris[i + 1]);
        }

        meshVerts.Clear();
        meshNormals.Clear();
        meshVerts.AddRange(pts);
        for (int i = 0; i < pts.Count; i++) meshNormals.Add(Vector3.back);

        fillMesh.Clear();
        fillMesh.SetVertices(meshVerts);
        fillMesh.SetNormals(meshNormals);
        fillMesh.SetTriangles(meshTris, 0);
    }

    private static void FanTriangulate(int count, List<int> tris)
    {
        for (int i = 1; i < count - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }
    }

    // Simple ear clipping for (possibly concave) polygons in the XY plane
    private static void EarClip(List<Vector3> pts, List<int> tris)
    {
        int n = pts.Count;
        List<int> idx = new List<int>(n);

        float area = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector3 a = pts[i], b = pts[(i + 1) % n];
            area += a.x * b.y - b.x * a.y;
        }

        // Make sure we work with counter-clockwise ordering
        if (area >= 0f) for (int i = 0; i < n; i++) idx.Add(i);
        else for (int i = n - 1; i >= 0; i--) idx.Add(i);

        int guard = 0;
        while (idx.Count > 3 && guard++ < 10000)
        {
            bool clipped = false;

            for (int i = 0; i < idx.Count; i++)
            {
                int i0 = idx[(i + idx.Count - 1) % idx.Count];
                int i1 = idx[i];
                int i2 = idx[(i + 1) % idx.Count];

                Vector2 a = pts[i0], b = pts[i1], c = pts[i2];

                // Skip reflex / degenerate corners
                if (Cross(b - a, c - b) <= 0f) continue;

                bool blocked = false;
                for (int j = 0; j < idx.Count; j++)
                {
                    int k = idx[j];
                    if (k == i0 || k == i1 || k == i2) continue;
                    if (PointInTriangle(pts[k], a, b, c)) { blocked = true; break; }
                }
                if (blocked) continue;

                tris.Add(i0);
                tris.Add(i1);
                tris.Add(i2);
                idx.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped) break; // degenerate polygon, bail out
        }

        if (idx.Count == 3)
        {
            tris.Add(idx[0]);
            tris.Add(idx[1]);
            tris.Add(idx[2]);
        }
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(b - a, p - a) > 0f &&
               Cross(c - b, p - b) > 0f &&
               Cross(a - c, p - c) > 0f;
    }
}

#if UNITY_EDITOR
// ---------------------------------------------------------------------------
//  Editor-only: hides the built-in collider gizmos while an object using
//  ColliderGizmoOverride is selected, and restores them afterwards.
//
//  It tries Unity's public GizmoUtility first (newer Unity versions), then the
//  internal AnnotationUtility (older versions) - both are what the Scene view
//  "Gizmos" dropdown uses - located through reflection so this compiles on any
//  Unity version.
// ---------------------------------------------------------------------------
namespace ColliderGizmoOverrideEditor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    internal static class BuiltInColliderGizmoSuppressor
    {
        private const string PrefsKey = "ColliderGizmoOverride.SuppressedGizmoIds";
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Parallel arrays: collider type <-> Unity native class ID
        private static readonly Type[] ColliderTypes =
        {
            typeof(MeshCollider), typeof(BoxCollider), typeof(SphereCollider), typeof(CapsuleCollider),
            typeof(CircleCollider2D), typeof(PolygonCollider2D), typeof(BoxCollider2D),
            typeof(CompositeCollider2D), typeof(EdgeCollider2D), typeof(CapsuleCollider2D)
        };
        private static readonly int[] ColliderClassIds = { 64, 65, 135, 136, 58, 60, 61, 66, 68, 70 };

        // Script class names (only needed by AnnotationUtility), filled from its annotation list
        private static readonly string[] scriptClasses = new string[ColliderTypes.Length];

        // Backend 1: public GizmoUtility
        private static MethodInfo gizmoUtilGetInfo;
        private static MethodInfo gizmoUtilSet;

        // Backend 2: internal AnnotationUtility
        private static MethodInfo annotGet;
        private static MethodInfo annotSet;
        private static MethodInfo annotList;

        private static bool backendResolved;
        private static bool backendAvailable;
        private static bool useGizmoUtility;
        private static bool warned;

        private static readonly List<int> disabledByUs = new List<int>(); // indices into ColliderTypes
        private static bool suppressed;

        static BuiltInColliderGizmoSuppressor()
        {
            // If a previous session ended (crash etc.) while gizmos were hidden, put them back
            RestoreFromPrefs();

            Selection.selectionChanged += Refresh;
            EditorApplication.hierarchyChanged += Refresh;
            Undo.postprocessModifications += OnUndoModify;
            AssemblyReloadEvents.beforeAssemblyReload += Restore;
            EditorApplication.quitting += Restore;
            EditorApplication.delayCall += Refresh;
        }

        private static UndoPropertyModification[] OnUndoModify(UndoPropertyModification[] mods)
        {
            // Covers toggling the component's enabled checkbox or "Hide Built In Gizmo"
            EditorApplication.delayCall += Refresh;
            return mods;
        }

        // ------------------------------------------------------------------
        //  Selection logic
        // ------------------------------------------------------------------

        private static void Refresh()
        {
            bool shouldSuppress = SelectionUsesOverride();
            if (shouldSuppress == suppressed) return;

            if (shouldSuppress) Suppress();
            else Restore();
        }

        private static bool SelectionUsesOverride()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null) continue;
                ColliderGizmoOverride o = go.GetComponent<ColliderGizmoOverride>();
                if (o != null && o.enabled && o.HideBuiltInGizmo) return true;
            }
            return false;
        }

        private static void Suppress()
        {
            if (!ResolveBackend()) { WarnOnce(); return; }

            bool[] known, enabled;
            Snapshot(out known, out enabled);

            StringBuilder saved = new StringBuilder();
            for (int i = 0; i < ColliderTypes.Length; i++)
            {
                if (known[i] && !enabled[i]) continue; // user already had it off, leave it alone

                if (SetEnabled(i, false))
                {
                    disabledByUs.Add(i);
                    saved.Append(ColliderClassIds[i]).Append(';');
                }
            }

            suppressed = true;
            EditorPrefs.SetString(PrefsKey, saved.ToString());
            SceneView.RepaintAll();
        }

        private static void Restore()
        {
            if (backendAvailable)
            {
                foreach (int i in disabledByUs) SetEnabled(i, true);
            }

            disabledByUs.Clear();
            suppressed = false;
            EditorPrefs.DeleteKey(PrefsKey);
            SceneView.RepaintAll();
        }

        private static void RestoreFromPrefs()
        {
            string data = EditorPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(data)) return;

            if (ResolveBackend())
            {
                bool[] known, enabled;
                Snapshot(out known, out enabled); // also refreshes script class names

                foreach (string entry in data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int id;
                    if (!int.TryParse(entry, out id)) continue;
                    int index = Array.IndexOf(ColliderClassIds, id);
                    if (index >= 0) SetEnabled(index, true);
                }
            }

            EditorPrefs.DeleteKey(PrefsKey);
        }

        // ------------------------------------------------------------------
        //  Backend resolution (reflection)
        // ------------------------------------------------------------------

        private static bool ResolveBackend()
        {
            if (backendResolved) return backendAvailable;
            backendResolved = true;

            // Backend 1: UnityEditor.GizmoUtility
            Type gu = FindType("UnityEditor.GizmoUtility");
            if (gu != null)
            {
                foreach (MethodInfo m in gu.GetMethods(AnyStatic))
                {
                    ParameterInfo[] ps = m.GetParameters();
                    if (m.Name == "GetGizmoInfo" && ps.Length == 0) gizmoUtilGetInfo = m;
                    else if (m.Name == "SetGizmoEnabled" && ps.Length >= 2 &&
                             ps[0].ParameterType == typeof(Type) && ps[1].ParameterType == typeof(bool))
                        gizmoUtilSet = m;
                }

                if (gizmoUtilGetInfo != null && gizmoUtilSet != null)
                {
                    useGizmoUtility = true;
                    backendAvailable = true;
                    return true;
                }
            }

            // Backend 2: UnityEditor.AnnotationUtility
            Type au = FindType("UnityEditor.AnnotationUtility");
            if (au != null)
            {
                foreach (MethodInfo m in au.GetMethods(AnyStatic))
                {
                    ParameterInfo[] ps = m.GetParameters();
                    if (m.Name == "GetGizmoEnabled" && ps.Length == 2 &&
                        ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(string))
                        annotGet = m;
                    else if (m.Name == "SetGizmoEnabled" && ps.Length >= 3 &&
                             ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(string) &&
                             ps[2].ParameterType == typeof(int))
                        annotSet = m;
                    else if (m.Name == "GetAnnotations" && ps.Length == 0)
                        annotList = m;
                }

                if (annotSet != null && (annotGet != null || annotList != null))
                {
                    useGizmoUtility = false;
                    backendAvailable = true;
                    return true;
                }
            }

            backendAvailable = false;
            return false;
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetType(fullName); } catch { /* ignore broken assemblies */ }
                if (t != null) return t;
            }
            return null;
        }

        private static void WarnOnce()
        {
            if (warned) return;
            warned = true;

            Debug.LogWarning(
                "ColliderGizmoOverride: couldn't find a usable Unity gizmo API, so the built-in collider " +
                "gizmo can't be hidden automatically (untick the collider type in the Scene view Gizmos " +
                "dropdown instead). Diagnostic info: " + DescribeApis());
        }

        private static string DescribeApis()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Unity ").Append(Application.unityVersion).Append(". ");

            foreach (string typeName in new[] { "UnityEditor.GizmoUtility", "UnityEditor.AnnotationUtility" })
            {
                Type t = FindType(typeName);
                if (t == null) { sb.Append(typeName).Append(" not found. "); continue; }

                sb.Append(typeName).Append(" found: ");
                foreach (MethodInfo m in t.GetMethods(AnyStatic))
                {
                    if (m.Name.IndexOf("Gizmo", StringComparison.OrdinalIgnoreCase) < 0 &&
                        m.Name.IndexOf("Annotation", StringComparison.OrdinalIgnoreCase) < 0) continue;

                    sb.Append(m.ReturnType.Name).Append(' ').Append(m.Name).Append('(');
                    ParameterInfo[] ps = m.GetParameters();
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(ps[i].ParameterType.Name);
                    }
                    sb.Append("); ");
                }
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        //  Reading / writing gizmo state
        // ------------------------------------------------------------------

        private static void Snapshot(out bool[] known, out bool[] enabled)
        {
            int n = ColliderTypes.Length;
            known = new bool[n];
            enabled = new bool[n];
            for (int i = 0; i < n; i++) { enabled[i] = true; if (scriptClasses[i] == null) scriptClasses[i] = ""; }

            try
            {
                if (useGizmoUtility)
                {
                    Array infos = gizmoUtilGetInfo.Invoke(null, null) as Array;
                    if (infos == null) return;

                    foreach (object info in infos)
                    {
                        Type t = GetMember(info, "type") as Type;
                        object flag = GetMember(info, "gizmoEnabled");
                        if (t == null || flag == null) continue;

                        int index = Array.IndexOf(ColliderTypes, t);
                        if (index < 0) continue;

                        known[index] = true;
                        enabled[index] = ToBool(flag);
                    }
                }
                else
                {
                    if (annotList != null)
                    {
                        Array annotations = annotList.Invoke(null, null) as Array;
                        if (annotations != null)
                        {
                            foreach (object a in annotations)
                            {
                                object idObj = GetMember(a, "classID");
                                if (idObj == null) continue;

                                int index = Array.IndexOf(ColliderClassIds, Convert.ToInt32(idObj));
                                if (index < 0) continue;

                                scriptClasses[index] = GetMember(a, "scriptClass") as string ?? "";
                                object flag = GetMember(a, "gizmoEnabled");
                                if (flag != null)
                                {
                                    known[index] = true;
                                    enabled[index] = ToBool(flag);
                                }
                            }
                        }
                    }

                    if (annotGet != null)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            try
                            {
                                enabled[i] = (bool)annotGet.Invoke(null, new object[] { ColliderClassIds[i], scriptClasses[i] });
                                known[i] = true;
                            }
                            catch { /* leave as unknown */ }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("ColliderGizmoOverride: couldn't read gizmo state: " + e.Message);
            }
        }

        private static bool SetEnabled(int index, bool value)
        {
            try
            {
                if (useGizmoUtility)
                {
                    ParameterInfo[] ps = gizmoUtilSet.GetParameters();
                    object[] args = new object[ps.Length];
                    args[0] = ColliderTypes[index];
                    args[1] = value;
                    for (int i = 2; i < ps.Length; i++) args[i] = DefaultFor(ps[i].ParameterType);
                    gizmoUtilSet.Invoke(null, args);
                }
                else
                {
                    ParameterInfo[] ps = annotSet.GetParameters();
                    object[] args = new object[ps.Length];
                    args[0] = ColliderClassIds[index];
                    args[1] = scriptClasses[index] ?? "";
                    args[2] = value ? 1 : 0;
                    for (int i = 3; i < ps.Length; i++) args[i] = DefaultFor(ps[i].ParameterType);
                    annotSet.Invoke(null, args);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("ColliderGizmoOverride: failed to change gizmo state: " + e.Message);
                return false;
            }
        }

        private static object DefaultFor(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool ToBool(object o)
        {
            if (o is bool) return (bool)o;
            try { return Convert.ToInt32(o) != 0; }
            catch { return true; }
        }

        private static object GetMember(object o, string name)
        {
            if (o == null) return null;
            Type t = o.GetType();

            PropertyInfo prop = t.GetProperty(name, AnyInstance);
            if (prop != null) return prop.GetValue(o, null);

            FieldInfo field = t.GetField(name, AnyInstance);
            return field != null ? field.GetValue(o) : null;
        }
    }
}
#endif
