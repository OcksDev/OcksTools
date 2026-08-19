
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
#endif
public class ColorSystem : SingleInstance<ColorSystem>
{
    public CompileableDictionary<HDRColor> HDRColors;
    public CompileableDictionary<FakeHDRColor> FakeHDRColors;
    public CompileableDictionary<Color> Colors;
    public CompileableDictionary<Material> Materials;
    public override void Awake2()
    {
        HDRColors.Compile();
        FakeHDRColors.Compile();
        Colors.Compile();
        Materials.Compile();
    }
    public Color GetColor(string name)
    {
        if (Colors.TryGetValue(name, out var c)) return c;
        if (HDRColors.TryGetValue(name, out var hc)) return hc.color;
        if (FakeHDRColors.TryGetValue(name, out var hc2)) return hc2.color.SetIntensity(hc2.intensity);
        return Color.pink;
    }
    public Material GetMaterial(string name)
    {
        if (Materials.TryGetValue(name, out var c)) return c;
        return null;
    }
}

[System.Serializable]
public struct HDRColor
{
    [ColorUsage(true, true)]
    public Color color;
}

[System.Serializable]
public struct FakeHDRColor
{
    public Color color;
    public float intensity;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HDRColor))]
public class FuckassDrawer : AutoCompressedSideBySideInspector
{
}
[CustomPropertyDrawer(typeof(FakeHDRColor))]
public class Fuckass2Drawer : AutoCompressedSideBySideInspector
{
}
#endif

public static class ColorUtils
{
    public static Color SetIntensity(this Color a, float intensity)
    {
        intensity -= GetIntensity(a);
        float factor = Mathf.Pow(2, intensity);
        return new Color(a.r * factor, a.g * factor, a.b * factor);
    }

    public static float GetIntensity(this Color a)
    {
        var maxColorComponent = a.maxColorComponent;
        if (maxColorComponent <= 0f) return 0f;
        var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
        return Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
    }
    private const byte k_MaxByteForOverexposedColor = 191; //internal Unity const
}