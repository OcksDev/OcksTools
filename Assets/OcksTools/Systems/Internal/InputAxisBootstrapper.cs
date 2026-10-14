#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity's legacy Input.GetAxis("joystick 1 axis 2") only works if an axis with that exact
/// name exists in Project Settings > Input Manager. This script adds them for you
/// automatically whenever scripts reload, so nothing has to be set up by hand.
///
/// Names:  "joystick {n} axis {a}"   n = 0 (any controller) or 1-4, a = 1-10 (1-based)
/// Put this file anywhere in Assets (an Editor folder is the convention).
/// The axes are saved into ProjectSettings/InputManager.asset, so builds include them.
/// </summary>
[InitializeOnLoad]
public static class InputAxisBootstrapper
{
    private const int MaxJoysticks = 4; // joystick 0 = "any", plus 1..MaxJoysticks
    private const int MaxAxes = 10;

    static InputAxisBootstrapper()
    {
        // delayCall avoids touching project settings in the middle of a domain reload
        EditorApplication.delayCall += EnsureJoystickAxes;
    }

    private static void EnsureJoystickAxes()
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
        if (asset == null || asset.Length == 0) return;

        var manager = new SerializedObject(asset[0]);
        var axes = manager.FindProperty("m_Axes");
        if (axes == null) return;

        bool changed = false;
        for (int joy = 0; joy <= MaxJoysticks; joy++)
        {
            for (int axisNumber = 1; axisNumber <= MaxAxes; axisNumber++)
            {
                string axisName = $"joystick {joy} axis {axisNumber}";
                if (AxisExists(axes, axisName)) continue;
                AddAxis(axes, axisName, joy, axisNumber - 1);
                changed = true;
            }
        }

        if (changed)
        {
            manager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[InputAxisBootstrapper] Registered controller axes in the Input Manager.");
        }
    }

    private static bool AxisExists(SerializedProperty axes, string axisName)
    {
        for (int i = 0; i < axes.arraySize; i++)
        {
            if (axes.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == axisName)
                return true;
        }
        return false;
    }

    private static void AddAxis(SerializedProperty axes, string axisName, int joyNum, int axisIndex)
    {
        axes.arraySize++;
        var p = axes.GetArrayElementAtIndex(axes.arraySize - 1);

        p.FindPropertyRelative("m_Name").stringValue = axisName;
        p.FindPropertyRelative("descriptiveName").stringValue = "";
        p.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
        p.FindPropertyRelative("negativeButton").stringValue = "";
        p.FindPropertyRelative("positiveButton").stringValue = "";
        p.FindPropertyRelative("altNegativeButton").stringValue = "";
        p.FindPropertyRelative("altPositiveButton").stringValue = "";
        p.FindPropertyRelative("gravity").floatValue = 0f;
        p.FindPropertyRelative("dead").floatValue = 0f;       // deadzone is handled in code
        p.FindPropertyRelative("sensitivity").floatValue = 1f;
        p.FindPropertyRelative("snap").boolValue = false;
        p.FindPropertyRelative("invert").boolValue = false;
        p.FindPropertyRelative("type").intValue = 2;          // 2 = Joystick Axis
        p.FindPropertyRelative("axis").intValue = axisIndex;  // 0-based (0 = X axis, 1 = Y axis, 2 = 3rd axis...)
        p.FindPropertyRelative("joyNum").intValue = joyNum;   // 0 = all joysticks
    }
}
#endif
