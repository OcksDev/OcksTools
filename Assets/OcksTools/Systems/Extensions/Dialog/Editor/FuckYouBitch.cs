using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "dlg")]
public class FuckYouBitch : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text = File.ReadAllText(ctx.assetPath);

        TextAsset asset = new TextAsset(text);

        ctx.AddObjectToAsset("main obj", asset);
        ctx.SetMainObject(asset);
    }
}