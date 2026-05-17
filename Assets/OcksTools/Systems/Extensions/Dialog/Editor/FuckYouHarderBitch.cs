using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FuckYouHarderBitch
{
    static FuckYouHarderBitch()
    {
        var extensions =
            EditorSettings.projectGenerationUserExtensions;

        if (!extensions.Contains("dlg"))
        {
            EditorSettings.projectGenerationUserExtensions =
                extensions.Append("dlg").ToArray();

            Debug.Log("Added .dlg to VS project generation.");
        }
    }
}