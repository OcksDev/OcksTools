#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

/// Fixes the glyph metrics of the controller icon sprite asset.
/// The defaults Unity generates for a center-pivot sprite (BearingX = -width/2, BearingY = height/2)
/// shift icons left and center them on the text baseline, so they look small, low and off-center.
/// Run via Tools > Fix Controller Icon Metrics. Safe to re-run after changing the constants.
public static class ControllerIconMetricsFixer
{
    private const string AtlasPath = "Assets/OcksTools/Systems/Internal/ControllerIcons/ControllerIcons_Atlas.asset";

    // Size multiplier applied to every icon. Raise to make icons bigger.
    private const float IconScale = 1.6f;

    // Fraction of the icon's height that sits ABOVE the baseline.
    // 0.5 = icon centered on the baseline (the broken default), 1.0 = icon sits fully on the baseline.
    // Raise to move icons up, lower to move them down.
    private const float BearingYRatio = 0.75f;

    // Extra horizontal space around each icon, as a fraction of its width (split left/right).
    private const float AdvancePadding = 0.1f;

    [MenuItem("OcksTools/Testing/Fix Controller Icon Metrics")]
    private static void Fix()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AtlasPath);
        if (asset == null)
        {
            Debug.LogError("[ControllerIconMetricsFixer] No sprite asset found at " + AtlasPath);
            return;
        }

        Undo.RecordObject(asset, "Fix controller icon metrics");

        // Uses only width/height, so re-running never compounds earlier changes.
        foreach (var glyph in asset.spriteGlyphTable)
        {
            var m = glyph.metrics;
            glyph.metrics = new GlyphMetrics(
                m.width,
                m.height,
                m.width * AdvancePadding * 0.5f,       // bearing X: centers the icon inside its advance
                m.height * BearingYRatio,              // bearing Y: how far the icon's top is above the baseline
                m.width * (1f + AdvancePadding));      // advance
        }

        foreach (var character in asset.spriteCharacterTable)
            character.scale = IconScale;

        asset.UpdateLookupTables();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ControllerIconMetricsFixer] Updated {asset.spriteGlyphTable.Count} icons (scale {IconScale}, bearingY {BearingYRatio}).");
    }
}
#endif
