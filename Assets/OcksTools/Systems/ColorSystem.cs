
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ColorSystem : SingleInstance<ColorSystem>
{
    public CompileableDictionary<HDRColor> HDRColors;
    public CompileableDictionary<Color> Colors;
    public CompileableDictionary<Material> Materials;

    public override void Awake2()
    {
        HDRColors.Compile();
        Colors.Compile();
        Materials.Compile();
    }
}

[System.Serializable]
public struct HDRColor
{
    [ColorUsage(true, true)]
    public Color color;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HDRColor))]
public class FuckassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Find the color field inside the struct
        var colorProp = property.FindPropertyRelative("color");

        // Draw it directly using the parent label
        EditorGUI.PropertyField(position, colorProp, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var colorProp = property.FindPropertyRelative("color");
        return EditorGUI.GetPropertyHeight(colorProp, label);
    }
}
#endif