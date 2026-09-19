#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click generator: OcksTools > EasySettings > Generate Missing Assets
///
/// Finds every concrete class inheriting from one of the RootTypes below and creates
/// a ScriptableObject asset for it in OutputFolder. A type that already has an asset
/// ANYWHERE in the project is skipped, so existing asset references are never touched.
///
/// Put this file in a folder named "Editor".
/// </summary>
public static class EasySettingsAssetGenerator
{
    const string OutputFolder = "Assets/OcksTools/Systems/Extensions/EasySettings/Generated";

    // Add more roots here later (open generics are fine) and nothing else needs to change.
    static readonly Type[] RootTypes =
    {
        typeof(SettingModifierSO<>),
    };

    [MenuItem("OcksTools/EasySettings/Generate Missing Assets")]
    public static void GenerateMissing()
    {
        EnsureFolder(OutputFolder);

        var created = new List<string>();
        var skippedNoScript = new List<string>();
        int alreadyExisted = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (Type type in FindConcreteInheritors())
            {
                if (AssetExists(type))
                {
                    alreadyExisted++;
                    continue;
                }

                var so = ScriptableObject.CreateInstance(type);

                // If the class isn't in a file with the same name, Unity can't link the asset to its script.
                if (MonoScript.FromScriptableObject(so) == null)
                {
                    UnityEngine.Object.DestroyImmediate(so);
                    skippedNoScript.Add(type.Name);
                    continue;
                }

                so.name = type.Name;
                string path = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{type.Name}.asset");
                AssetDatabase.CreateAsset(so, path);
                created.Add(type.Name);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EasySettings] Created {created.Count} new asset(s), {alreadyExisted} already existed."
                  + (created.Count > 0 ? $"\nNew: {string.Join(", ", created)}" : ""));

        if (skippedNoScript.Count > 0)
        {
            Debug.LogWarning("[EasySettings] Skipped (class must live in a .cs file with the same name as the class): "
                             + string.Join(", ", skippedNoScript));
        }
    }

    // Same filter as RandomFunctions.GetListOfInheritors (class, not abstract, assignable),
    // but returns Types instead of Activator instances (ScriptableObjects can't be made with new)
    // and understands open generic roots like SettingModifierSO<>.
    static IEnumerable<Type> FindConcreteInheritors()
    {
        return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters)
            .Where(t => RootTypes.Any(root => InheritsFrom(t, root)))
            .OrderBy(t => t.Name);
    }

    static bool InheritsFrom(Type type, Type root)
    {
        for (Type t = type.BaseType; t != null; t = t.BaseType)
        {
            Type candidate = (t.IsGenericType && root.IsGenericTypeDefinition) ? t.GetGenericTypeDefinition() : t;
            if (candidate == root) return true;
        }
        return false;
    }

    // Checks the whole project (not just the Generated folder) so moved assets aren't duplicated.
    static bool AssetExists(Type type)
    {
        foreach (string guid in AssetDatabase.FindAssets($"t:{type.Name}"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == type) return true;
        }
        return false;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
