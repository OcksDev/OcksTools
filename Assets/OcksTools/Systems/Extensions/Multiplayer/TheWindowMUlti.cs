#if (UNITY_EDITOR)

using UnityEditor;
using UnityEngine;

public class MultiplayerWindow : EditorWindow
{
    private string s2 = "";
    private bool seeking = false;
    [MenuItem("OcksTools/Multiplayer/Utils")]
    public static void ShowWindow()
    {
        var f = GetWindow<MultiplayerWindow>("Console Utils");
    }

    private void OnGUI()
    {
        GUILayout.Space(15);
        /*
        var f22 = new string[3] { "Setup", "Dialog", "Chat" };
        c = GUILayout.Toolbar(c, f22);
        */
        GUILayout.Label("Works only during run-time");
        GUILayout.Space(5);
        s2 = EditorGUILayout.TextField(new GUIContent("Code", "The join code for multiplayer connections"), s2);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Host", "Hosts a match, and generates a new code")))
        {
            seeking = true;
            _ = RelayServerManager.Instance.CreateGame();
        }
        if (GUILayout.Button(new GUIContent("Join", "Enter a code and join a hosted match")))
        {
            RelayServerManager.Instance.JoinGame(s2);
        }
        GUILayout.EndHorizontal();
        if (seeking)
        {
            var g2 = RelayServerManager.Instance.Join_Code;
            if (g2 != "" && g2 != s2)
            {
                s2 = g2;
                seeking = false;
                Debug.LogWarning(g2);
            }
        }
    }
}
#endif
