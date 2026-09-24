//nowrap OXTLS_MULTIPLAYER
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WrapMultiplayerFiles
{
    public const string Define = "OXTLS_MULTIPLAYER";

    [MenuItem("OcksTools/Multiplayer/Wrap Files")]
    private static void Wrap()
    {
        const string folder = "Assets/OcksTools/Systems/Extensions/Multiplayer";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError($"Folder not found: {folder}, nya");
            return;
        }

        int count = 0;
        var pp = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
        foreach (string file in pp)
        {
            string text = File.ReadAllText(file);
            if (text.Contains("#if " + Define)) continue;       // already wrapped
            if (text.Contains("//nowrap " + Define)) continue;  // manually skipped

            File.WriteAllText(file, "#if " + Define + "\n" + text.TrimEnd() + "\n#endif\n");
            count++;
        }

        Debug.Log($"Wrapped {count} files, uwu");
        AssetDatabase.Refresh();
    }
}