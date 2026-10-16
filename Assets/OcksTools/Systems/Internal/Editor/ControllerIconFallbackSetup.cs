#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// Makes sure the controller icon atlas is in the fallback list of the project's
/// Default Sprite Asset, so <sprite name="cA"> works in every TMP text. Runs on every
/// editor load / recompile and only touches anything when the atlas is missing.
[InitializeOnLoad]
public static class ControllerIconFallbackSetup
{
    private const string AtlasPath = "Assets/OcksTools/Systems/Internal/ControllerIcons/ControllerIcons_Atlas.asset";

    // Also add the atlas to TMP Settings > Fallback Emoji Text Assets.
    // Off by default: that list is for Unicode emoji, not <sprite> tags.

    static ControllerIconFallbackSetup()
    {
        // delayCall: TMP settings and assets aren't safe to touch during static init.
        EditorApplication.delayCall += Ensure;
    }

    private static void Ensure()
    {
        var atlas = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AtlasPath);
        if (atlas == null || TMP_Settings.instance == null) return; // atlas not created yet / TMP not imported

        bool changed = false;

        // Default sprite asset's fallback list
        var def = TMP_Settings.defaultSpriteAsset;
        if (def != null && def != atlas)
        {
            if (def.fallbackSpriteAssets == null)
                def.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

            if (!def.fallbackSpriteAssets.Contains(atlas))
            {
                def.fallbackSpriteAssets.Add(atlas);
                EditorUtility.SetDirty(def);
                changed = true;
            }
        }


        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[ControllerIconFallbackSetup] Added controller icon atlas to the TMP fallback list.");
        }
    }
}
#endif
