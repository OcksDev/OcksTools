//nowrap OXTLS_MULTIPLAYER
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WrapMultiplayerFiles
{
    public const string Define = "OXTLS_MULTIPLAYER";

    [MenuItem("OcksTools/Multiplayer/Wrap Selected Folder In Define")]
    private static void Wrap()
    {
        string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!AssetDatabase.IsValidFolder(folder)) { Debug.LogError("Select a folder first, nya"); return; }

        int count = 0;
        foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            if (text.Contains("#if " + Define)) continue; // already wrapped
            if (text.Contains("//nowrap " + Define)) continue;
            File.WriteAllText(file, "#if " + Define + "\n" + text.TrimEnd() + "\n#endif\n");
            count++;
        }
        AssetDatabase.Refresh();
        Debug.Log($"Wrapped {count} files, uwu");
    }
}