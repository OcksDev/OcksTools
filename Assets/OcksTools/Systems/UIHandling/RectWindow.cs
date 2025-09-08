#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UI;

public class RectWindow: EditorWindow
{
    GameObject s;
    float border;
    [MenuItem("OcksTools/Rect Utils %&r", false, 1)]
    public static void ShowWindow()
    {
        var f = GetWindow<RectWindow>("Rect Utils");
    }

    private void OnGUI()
    {
        s = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Canvas", "OH NO NOT THE Canvas!!!!!"), s, typeof(GameObject), true);
        if (GUILayout.Button(new GUIContent("Auto Canvas Scaling", "(requires canvas to be set) Applies and sets the parameters needed for good canvas scaling")))
        {
            var gpp = s.GetComponent<CanvasScaler>();
            CanvasScaler g = null;
            if (gpp == null)
            {
                g = s.AddComponent<CanvasScaler>();
            }
            else
            {
                g = gpp;
            }
            g.referenceResolution = new Vector2(1066.666f, 800);
            g.matchWidthOrHeight = 1;
            g.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }
        GUILayout.Space(15);
        if (GUILayout.Button(new GUIContent("Rect Auto Size", "Automatically fits the rect anchors of all selected Unity UI elements")))
        {
            fard();
        }
        GUILayout.Space(10);
        GUILayout.Label(new GUIContent("Grid Layout Child Size Fitter", "Automatically fits the size for children in the Grid Layout Group of all selected Unity UI elements"));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("X", "Fits the X size for children in the Grid Layout Group of all selected Unity UI elements")))
        {
            fardgrid(0);
        }
        if (GUILayout.Button(new GUIContent("Y", "Fits the Y size for children in the Grid Layout Group of all selected Unity UI elements")))
        {
            fardgrid(1);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Parent Border Fitter", "Automatically fits the size of the selected UI elements to the parents size, with an inputted border"));
        border = EditorGUILayout.FloatField(new GUIContent("Border:", "Border size used"), border);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("X", "Fits the X Axis")))
        {
            fardborder(0);
        }
        if (GUILayout.Button(new GUIContent("Y", "Fits the Y Axis")))
        {
            fardborder(1);
        }
        if (GUILayout.Button(new GUIContent("All", "Fits every Axis")))
        {
            fardborder(2);
        }
        GUILayout.EndHorizontal();

    }


    void fard()
    {

        foreach (var f in Selection.gameObjects)
        {
            var g = f.GetComponent<RectTransform>();
            if (g != null)
            {
                var r = g;
                Undo.RecordObject(r, "Set anchors around object");
                var p = f.transform.parent.GetComponent<RectTransform>();

                var offsetMin = r.offsetMin;
                var offsetMax = r.offsetMax;
                var _anchorMin = r.anchorMin;
                var _anchorMax = r.anchorMax;

                var parent_width = p.rect.width;
                var parent_height = p.rect.height;

                var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                                            _anchorMin.y + (offsetMin.y / parent_height));
                var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                                            _anchorMax.y + (offsetMax.y / parent_height));

                r.anchorMin = anchorMin;
                r.anchorMax = anchorMax;

                r.offsetMin = new Vector2(0, 0);
                r.offsetMax = new Vector2(0, 0);
                r.pivot = new Vector2(0.5f, 0.5f);

            }
        }
    }

    void fardgrid(int t)
    {
        bool e = t == 0;
        foreach (var f in Selection.gameObjects)
        {
            var g = f.GetComponent<GridLayoutGroup>();
            if (g != null)
            {
                Undo.RecordObject(g, "Grid shit lol");
                var p = f.GetComponent<RectTransform>();
                float to = e ? p.rect.width : p.rect.height;
                float ignore = g.spacing.x;
                ignore *= g.constraintCount - 1;
                ignore += e ? g.padding.left + g.padding.right : g.padding.top + g.padding.bottom;
                var x = to - ignore;
                x /= g.constraintCount;
                g.cellSize = e ? new Vector2(x, g.cellSize.y) : new Vector2(g.cellSize.x, x);
            }
        }
    }
    void fardborder(int t)
    {
        bool e = t == 0;
        foreach (var f in Selection.gameObjects)
        {
            var g = f.GetComponent<RectTransform>();
            var gp = f.transform.parent.GetComponent<RectTransform>();
            if (g != null)
            {
                Undo.RecordObject(g, "Fitted rect transform to parent");
                if(t != 0)g.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gp.rect.size.y - (border * 2));
                if (t != 1) g.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gp.rect.size.x - (border * 2));
            }
        }
    }
}
#endif
