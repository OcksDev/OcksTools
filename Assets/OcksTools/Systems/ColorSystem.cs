
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
    public Color GetColor(string name)
    {
        if (Colors.TryGetValue(name, out var c)) return c;
        if (HDRColors.TryGetValue(name, out var hc)) return hc.color;
        return Color.pink;
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
public class FuckassDrawer : AutoCompressedInspector
{
}
#endif