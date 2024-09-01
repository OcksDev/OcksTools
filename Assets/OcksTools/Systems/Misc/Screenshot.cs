using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public static Screenshot Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void TakeScreenshot(ScreenshotData Sdata)
    {
        Sdata.CamerasToUpdate.Add(Sdata.Camera);
        if (Sdata.UseCameraSizeForImage)
        {
            Sdata.Width_PX = Sdata.Camera.pixelWidth;
            Sdata.Height_PX = Sdata.Camera.pixelHeight;
            Console.Log($"size: {Sdata.Width_PX}, {Sdata.Height_PX}");
        }
        Peenshot(Sdata);
    }


    public void Peenshot(ScreenshotData Sdata)
    {
        var rw = Sdata.Width_PX;
        var rh = Sdata.Height_PX;
        var camera = Sdata.Camera;
        //scary code
        RenderTexture rt = new RenderTexture(rw, rh, 24);
        var e = camera.targetTexture;
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(rw, rh, TextureFormat.RGBA32, false);
        foreach (var cam in Sdata.CamerasToUpdate)
        {
            cam.Render();
        }
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rw, rh), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();

        //output location
        var ww = FileSystem.Instance.GameDirectory + $"/{Sdata.Name}.png";
        Sdata.OutputLocation = ww;

        System.IO.File.WriteAllBytes(ww, bytes);
        if (e != null) camera.targetTexture = e;
        //camera.Render();
    }
}
[System.Serializable]
public class ScreenshotData
{
    public string Name = "";
    public int Width_PX = 69;
    public int Height_PX = 69;
    public Camera Camera;
    public bool UseCameraSizeForImage = false;
    [HideInInspector]
    public string OutputLocation;
    [HideInInspector]
    public List<Camera> CamerasToUpdate = new List<Camera>();
    public ScreenshotData()
    {

    }
    public ScreenshotData(string name, int width, int height, Camera cam, bool usecamsize = false)
    {
        Name = name;
        Width_PX = width;
        Height_PX = height;
        Camera = cam;
        UseCameraSizeForImage = usecamsize;    
    }
}
