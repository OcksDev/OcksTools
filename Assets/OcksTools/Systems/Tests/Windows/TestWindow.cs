#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

public class TestWindow : EditorWindow
{
    string test = "";
    string test2 = "";
    int sel = 0;
    [MenuItem("OcksTools/Testing/Sex %g")]
    public static void ShowWindow()
    {
        GetWindow<TestWindow>("Sex Lol");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sex Testing", EditorStyles.boldLabel);
        //EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        GUILayout.Space(10);
        sel = GUILayout.Toolbar(sel, new string[4] { "Sex", "Balls", "Fuck", "Shit" });

        switch (sel)
        {
            case 0:
                test = EditorGUILayout.TextField("Boner", test);
                test2 = EditorGUILayout.TextField("Boner2", test2);
                break;
        }

        if (GUILayout.Button("HOLY FFUCK NO WAY I LOVE SEX"))
        {
            foreach (var f in Selection.gameObjects)
            {
                f.name = test;
            }
        }
    }
}
#endif