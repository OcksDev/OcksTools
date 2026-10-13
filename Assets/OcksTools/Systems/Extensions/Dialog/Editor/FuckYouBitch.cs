using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(2, "dlg")] // version bumped from 1 so existing .dlg files reimport and pick up the icon
public class FuckYouBitch : ScriptedImporter
{
    private const string IconPath = "Assets/OcksTools/Systems/Extensions/Dialog/Editor/DialogIcon.png";

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text = File.ReadAllText(ctx.assetPath);

        TextAsset asset = new TextAsset(text);

        // Reimport all .dlg files whenever the icon texture changes (or appears for the first time).
        ctx.DependsOnSourceAsset(IconPath);
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);

        ctx.AddObjectToAsset("main obj", asset, icon); // icon may be null, which just means the default icon
        ctx.SetMainObject(asset);
    }
}
