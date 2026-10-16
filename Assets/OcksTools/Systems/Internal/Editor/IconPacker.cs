#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ControllerIconPacker
{
    [MenuItem("Assets/OcksTools/Pack Images To Atlas")]
    private static void Pack()
    {
        string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError("Select a folder containing the icon PNGs first.");
            return;
        }

        string outPath = $"{folder}/{Path.GetFileName(folder)}_Atlas.png";
        var names = new List<string>();
        var textures = new List<Texture2D>();

        foreach (string file in Directory.GetFiles(folder, "*.png"))
        {
            string assetPath = file.Replace('\\', '/');
            if (assetPath == outPath) continue; // skip a previous atlas

            // LoadImage on a fresh texture, so the source imports don't need Read/Write enabled.
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(file));
            names.Add(Path.GetFileNameWithoutExtension(file));
            textures.Add(tex);
        }

        if (textures.Count == 0)
        {
            Debug.LogError("No PNGs found in " + folder);
            return;
        }

        var atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Rect[] uvs = atlas.PackTextures(textures.ToArray(), 4, 4096, false);
        File.WriteAllBytes(outPath, atlas.EncodeToPNG());
        foreach (var t in textures) Object.DestroyImmediate(t);

        AssetDatabase.ImportAsset(outPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(outPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var sheet = new SpriteMetaData[names.Count];
        for (int i = 0; i < names.Count; i++)
        {
            sheet[i] = new SpriteMetaData
            {
                name = names[i],
                rect = new Rect(uvs[i].x * atlas.width, uvs[i].y * atlas.height,
                                uvs[i].width * atlas.width, uvs[i].height * atlas.height),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
            };
        }
#pragma warning disable CS0618 // spritesheet is marked obsolete but works without extra packages
        importer.spritesheet = sheet;
#pragma warning restore CS0618
        importer.SaveAndReimport();

        Object.DestroyImmediate(atlas);
        Debug.Log($"Packed {names.Count} icons into {outPath}.");
    }
}
#endif