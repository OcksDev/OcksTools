using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Screenshot : SingleInstance<Screenshot>
{

    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("screenshot", "Console", "Message_HelpScreenshot").Action(ConsoleScreenShot));
        });
    }
    public static void ConsoleScreenShot()
    {
        ConsoleLol.Instance.CloseConsole();
        Instance.TakeScreenshot(new ScreenshotData("Example", -1, -1, Camera.main, true));
    }


    public void TakeScreenshot(ScreenshotData Sdata)
    {
        /*
         * Transparent background screenshots need post processing disabled on the cameras
         */

        Sdata._CamerasToUpdate.Add(Sdata._Camera);
        if (Sdata._UseCameraSizeForImage)
        {
            Sdata._Width_PX = Sdata._Camera.pixelWidth;
            Sdata._Height_PX = Sdata._Camera.pixelHeight;
            Console.Log($"size: {Sdata._Width_PX}, {Sdata._Height_PX}");
        }
        Peenshot(Sdata);
    }
    /// <summary>
    /// Heavily experimental transparent screenshot method lol
    /// </summary>

    public void Peenshot(ScreenshotData Sdata)
    {
        var rw = Sdata._Width_PX;
        var rh = Sdata._Height_PX;
        var camera = Sdata._Camera;
        //scary code
        RenderTexture rt = new RenderTexture(rw, rh, 24, RenderTextureFormat.ARGB32);
        var e = camera.targetTexture;
        var oldFlags = camera.clearFlags;
        var oldBgColor = camera.backgroundColor;
        bool oldPosty = false;
        if (Sdata._ForceTransparentSettings)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            var a = camera.GetUniversalAdditionalCameraData();
            if (a != null)
            {
                oldPosty = a.renderPostProcessing;
                a.renderPostProcessing = false;
            }
        }
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(rw, rh, TextureFormat.RGBA32, false);
        if (Sdata._PreRenderCameras)
        {
            foreach (var cam in Sdata._CamerasToUpdate)
            {
                cam.Render();
            }
        }
        Sdata.PreRenderEvent.Invoke();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rw, rh), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        rt.Release();
        byte[] bytes = screenShot.EncodeToPNG();

        //output location
        var ww = Sdata._OverridePath != "" ? Sdata._OverridePath : FileSystem.Instance.GameDirectory + $"/{Sdata._Name}.png";
        Sdata.OutputLocation = ww;

        System.IO.File.WriteAllBytes(ww, bytes);
        if (e != null) camera.targetTexture = e;
        if (Sdata._PostRenderCameras)
        {
            foreach (var cam in Sdata._CamerasToUpdate)
            {
                cam.Render();
            }
        }
        Sdata.PostRenderEvent.Invoke();
        if (Sdata._ForceTransparentSettings)
        {
            camera.clearFlags = oldFlags;
            camera.backgroundColor = oldBgColor;
            var a = camera.GetUniversalAdditionalCameraData();
            if (a != null)
            {
                a.renderPostProcessing = oldPosty;
            }
        }
    }
}
[System.Serializable]
public class ScreenshotData
{
    public string _Name = "";
    public int _Width_PX = 69;
    public int _Height_PX = 69;
    public Camera _Camera;
    public bool _UseCameraSizeForImage = false;
    public bool _ForceTransparentSettings = false;
    public string _OverridePath = "";

    [HideInInspector]
    public string OutputLocation;
    [HideInInspector]
    public List<Camera> _CamerasToUpdate = new List<Camera>();
    [HideInInspector]
    public bool _PreRenderCameras = true;
    [HideInInspector]
    public bool _PostRenderCameras = false;
    public OXEvent PreRenderEvent = new OXEvent();
    public OXEvent PostRenderEvent = new OXEvent();
    public ScreenshotData(string name)
    {
        _Name = name;
    }

    public ScreenshotData WidthPX(int amnt) { _Width_PX = amnt; return this; }
    public ScreenshotData HeightPX(int amnt) { _Height_PX = amnt; return this; }
    public ScreenshotData Camera(Camera a) { _Camera = a; return this; }
    public ScreenshotData OverridePath(string a) { _OverridePath = a; return this; }
    public ScreenshotData UseCameraSizeForImage(bool a = true) { _UseCameraSizeForImage = a; return this; }
    public ScreenshotData CamerasToUpdate(BetterList<Camera> a) { _CamerasToUpdate = a; return this; }
    public ScreenshotData PreRenderCameras(bool a = true) { _PreRenderCameras = a; return this; }
    public ScreenshotData PostRenderCameras(bool a = true) { _PostRenderCameras = a; return this; }
    public ScreenshotData ForceTransparentSettings(bool a = true) { _ForceTransparentSettings = true; return this; }

    public ScreenshotData(string name, int width, int height, Camera cam, bool usecamsize = false)
    {
        _Name = name;
        _Width_PX = width;
        _Height_PX = height;
        _Camera = cam;
        _UseCameraSizeForImage = usecamsize;
    }
#if UNITY_EDITOR
    public Sprite SetupSpriteImport() => SetupSpriteImport(OutputLocation);
    public Sprite SetupSpriteImport(string assetPath)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Could not get TextureImporter for {assetPath}");
            return null;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;

        importer.SaveAndReimport();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        return sprite;
    }
#endif
}
