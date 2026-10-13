#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Automatically assigns a custom icon to the OXKeyframeAnimation script (and therefore all assets of that type).
/// Runs once per editor session. Place this file inside any folder named "Editor".
/// </summary>
public static class OXKeyframeAnimationIconApplier
{
    // Change this to wherever your icon texture lives.
    private const string IconPath = "Assets/OcksTools/Systems/Extensions/Animation/Keyframe/Editor/OcksAnimation.png";

    // SessionState survives domain reloads but resets when the editor restarts.
    private const string SessionKey = "OXKeyframeAnimationIconApplier3.Checked";

    [InitializeOnLoadMethod]
    private static void Init()
    {
        if (SessionState.GetBool(SessionKey, false)) return;

        // delayCall waits until the editor has finished importing, so the icon texture is ready.
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon == null) return; // Icon not found yet. Not marked as checked, so it retries after the next reload.

        // Icon exists, so this session's check is done regardless of the outcome below.
        SessionState.SetBool(SessionKey, true);

        foreach (var guid in AssetDatabase.FindAssets("OXKeyframeAnimation t:MonoScript"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null || script.GetClass() != typeof(OXKeyframeAnimation)) continue;

            var importer = AssetImporter.GetAtPath(path) as MonoImporter;
            if (importer == null || importer.GetIcon() == icon) return; // Already correct.

            importer.SetIcon(icon);
            importer.SaveAndReimport();
            return;
        }
    }
}
#endif
