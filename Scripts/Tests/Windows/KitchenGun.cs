#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

public class KitchenGun: EditorWindow
{
    [MenuItem("OcksTools/Testing/Kitchen Gun")]
    public static void ShowWindow()
    {
        GetWindow<KitchenGun>("Kitchen Gun");
    }

    private void OnGUI()
    {

        GUILayout.Space(15);
        if (GUILayout.Button("Kitchens Your Gun"))
        {
            foreach (var f in Selection.gameObjects)
            {
                Undo.RecordObject(f, "Kitchened Thyn Gun");
                f.name = "Kitchen Gun";
            }
        }
    }
}
#endif
