using System.Collections.Generic;
using UnityEngine;

public class Screenshot : SingleInstance<Screenshot>
{

    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        GlobalEvent.Append("Console", BuildScreenshotCums);
    }
    public static void BuildScreenshotCums()
    {
        ConsoleLol.Instance.Add(new OXCommand("screenshot", "Console", "Message_HelpScreenshot").Action(ConsoleScreenShot));
    }

    public static void ConsoleScreenShot()
    {
        ConsoleLol.Instance.CloseConsole();
        Instance.TakeScreenshot(new ScreenshotData("Example", -1, -1, Camera.main, true));
    }


    public void TakeScreenshot(ScreenshotData Sdata)
    {
        Sdata._CamerasToUpdate.Add(Sdata._Camera);
        if (Sdata._UseCameraSizeForImage)
        {
            Sdata._Width_PX = Sdata._Camera.pixelWidth;
            Sdata._Height_PX = Sdata._Camera.pixelHeight;
            Console.Log($"size: {Sdata._Width_PX}, {Sdata._Height_PX}");
        }
        Peenshot(Sdata);
    }


    public void Peenshot(ScreenshotData Sdata)
    {
        var rw = Sdata._Width_PX;
        var rh = Sdata._Height_PX;
        var camera = Sdata._Camera;
        //scary code
        RenderTexture rt = new RenderTexture(rw, rh, 24);
        var e = camera.targetTexture;
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
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();

        //output location
        var ww = FileSystem.Instance.GameDirectory + $"/{Sdata._Name}.png";
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
    public ScreenshotData UseCameraSizeForImage(bool a) { _UseCameraSizeForImage = a; return this; }
    public ScreenshotData CamerasToUpdate(List<Camera> a) { _CamerasToUpdate = a; return this; }
    public ScreenshotData PreRenderCameras(bool a) { _PreRenderCameras = a; return this; }
    public ScreenshotData PostRenderCameras(bool a) { _PostRenderCameras = a; return this; }

    public ScreenshotData(string name, int width, int height, Camera cam, bool usecamsize = false)
    {
        _Name = name;
        _Width_PX = width;
        _Height_PX = height;
        _Camera = cam;
        _UseCameraSizeForImage = usecamsize;
    }
}
