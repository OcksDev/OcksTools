//nowrap OXTLS_MULTIPLAYER
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

[InitializeOnLoad]
public static class MultiplayerDefineToggle
{

    static MultiplayerDefineToggle()
    {
        var q = AppDomain.CurrentDomain.GetAssemblies();
        bool hasNetcode = q.Any(a => a.GetName().Name.StartsWith("Unity.Netcode.Runtime");
        bool hasServices = q.Any(a => a.GetName().Name.StartsWith("Unity.Services.Multiplayer");
        SetDefine(hasNetcode && hasServices);
    }
    private static void SetDefine(bool enabled)
    {
        foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (group == BuildTargetGroup.Unknown) continue;
            try
            {
                var target = NamedBuildTarget.FromBuildTargetGroup(group);
                var defines = PlayerSettings.GetScriptingDefineSymbols(target)
                    .Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

                bool has = defines.Contains(WrapMultiplayerFiles.Define);
                if (enabled == has) continue;

                if (enabled) defines.Add(WrapMultiplayerFiles.Define); else defines.Remove(WrapMultiplayerFiles.Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            }
            catch { /* some groups are obsolete or unsupported, skip them */ }
        }
    }
}