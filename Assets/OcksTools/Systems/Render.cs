using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Render
{
    /*
     * Lets you get/set various screen rendering related datas such as refreshrate, render size, anti-aliasing, etc
     */
    public static double GetTargetFramerate()
    {
        return Application.targetFrameRate;
    }
    public static double GetMonitorRefreshRate()
    {
        return Screen.currentResolution.refreshRateRatio.value;
    }
    /// <summary>
    /// in pixels
    /// </summary>
    public static Vector2Int GetMonitorSize()
    {
        return new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
    }
    /// <summary>
    /// in pixels
    /// </summary>
    public static Vector2Int GetWindowSize()
    {
        return new Vector2Int(Screen.width, Screen.height);
    }
    public static FullScreenMode GetFullscreen()
    {
        return Screen.fullScreenMode;
    }
    public static bool GetVSync()
    {
        if (QualitySettings.vSyncCount > 1) throw new System.Exception("Fuck your dumb ass higher vsyncs");
        return QualitySettings.vSyncCount == 1;
    }
    public static string GetLanguage()
    {
        //should be forced to "en", based on some code in Converter.cs, feel free to remove the forced language culture when you want to deal with localization fr.
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    }
    public static bool GetAnisotropicFiltering()
    {
        return QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable || QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable;
    }
    public static void SetFullscreen(FullScreenMode mode)
    {
        Screen.fullScreenMode = mode;
    }
    /// <summary>
    /// in pixels
    /// </summary>
    public static void SetWindowSize(int width, int height)
    {
        Screen.SetResolution(width, height, GetFullscreen());
    }
    public static void SetWindowSize(Vector2Int size)
    {
        Screen.SetResolution(size.x, size.y, GetFullscreen());
    }
    public static void SetTargetFramerate(int amount)
    {
        Application.targetFrameRate = amount;
    }
    public static void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
    public static void SetAnisotropicFiltering(bool enabled)
    {
        QualitySettings.anisotropicFiltering = enabled ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
    }
    private static UniversalRenderPipelineAsset GetURPAsset()
    {
        // null if the project is using the built-in pipeline
        return GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
    }

    /// <summary>
    /// 0 = off, 1 = FXAA, 2+ = MSAA (2, 4, 8)
    /// </summary>
    public static int GetAntiAliasing()
    {
        // MSAA first, since it's the "higher" setting
        int msaa = GetMSAA();
        if (msaa > 1) return msaa;

        if (GetAntiAliasingMode() == AntialiasingMode.FastApproximateAntialiasing) return 1;

        return 0;
    }

    public static void SetAntiAliasing(int amount)
    {
        if (amount <= 0)
        {
            // everything off
            SetAntiAliasingMode(AntialiasingMode.None);
            SetMSAA(1);
        }
        else if (amount == 1)
        {
            // FXAA only
            SetAntiAliasingMode(AntialiasingMode.FastApproximateAntialiasing);
            SetMSAA(1);
        }
        else
        {
            // MSAA only
            SetAntiAliasingMode(AntialiasingMode.None);
            SetMSAA(amount);
        }
    }

    private static int GetMSAA()
    {
        var urp = GetURPAsset();
        if (urp != null) return urp.msaaSampleCount;

        // legacy returns 0 for off, normalize to 1
        return Mathf.Max(1, QualitySettings.antiAliasing);
    }

    private static void SetMSAA(int amount)
    {
        // URP only accepts 1, 2, 4 or 8
        int samples = amount <= 1 ? 1 : Mathf.Clamp(Mathf.ClosestPowerOfTwo(amount), 2, 8);

        var urp = GetURPAsset();
        if (urp != null)
            urp.msaaSampleCount = samples;
        else
            QualitySettings.antiAliasing = samples == 1 ? 0 : samples; // legacy fallback
    }
    public static void SetAntiAliasingMode(AntialiasingMode mode)
    {
        foreach (var cam in Camera.allCameras)
        {
            cam.GetUniversalAdditionalCameraData().antialiasing = mode;
        }
    }

    public static AntialiasingMode GetAntiAliasingMode()
    {
        var cam = Camera.main;
        return cam != null ? cam.GetUniversalAdditionalCameraData().antialiasing : AntialiasingMode.None;
    }

    public static float GetRenderScale()
    {
        var urp = GetURPAsset();
        return urp != null ? urp.renderScale : 1f;
    }

    public static void SetRenderScale(float scale)
    {
        var urp = GetURPAsset();
        if (urp != null) urp.renderScale = Mathf.Clamp(scale, 0.1f, 2f);
    }

}

public class _ConsoleRenderQueryererr
{
    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("getrender")
                .Append(new OXCommand("fps").Action(() => Console.Log(Render.GetTargetFramerate())))
                .Append(new OXCommand("vsync").Action(() => Console.Log(Render.GetVSync())))
                .Append(new OXCommand("fullscreen").Action(() => Console.Log(Render.GetFullscreen())))
                .Append(new OXCommand("anistropic").Action(() => Console.Log(Render.GetAnisotropicFiltering())))
                .Append(new OXCommand("antialiasing").Action(() => Console.Log(Render.GetAntiAliasing())))
                .Append(new OXCommand("hz").Action(() => Console.Log(Render.GetMonitorRefreshRate())))
                .Append(new OXCommand("renderscale").Action(() => Console.Log(Render.GetRenderScale())))
                .Append(new OXCommand("monitor_size").Action(() => Console.Log(Render.GetMonitorSize())))
                .Append(new OXCommand("window_size").Action(() => Console.Log(Render.GetWindowSize())))
                .Append(new OXCommand("language").Action(() => Console.Log(Render.GetLanguage())))
                );
            ConsoleLol.Instance.Add(new OXCommand("setrender")
                .Append(new OXCommand("fps").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetTargetFramerate(int.Parse(r.com[2])))))
                .Append(new OXCommand("vsync").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetVSync(int.Parse(r.com[2]).IntToBool()))))
                .Append(new OXCommand("fullscreen").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetFullscreen(int.Parse(r.com[2]).IntToBool() ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed))))
                .Append(new OXCommand("renderscale").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetRenderScale(int.Parse(r.com[2]) / 100f))))
                .Append(new OXCommand("anistropic").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetAnisotropicFiltering(int.Parse(r.com[2]).IntToBool()))))
                .Append(new OXCommand("antialiasing").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) => Render.SetAntiAliasing(int.Parse(r.com[2])))))
                .Append(new OXCommand("window_size").Append(new OXCommand(OXCommand.ExpectedInputType.Long).Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action((r) =>
                {
                    var x = int.Parse(r.com_caps[2]);
                    var y = int.Parse(r.com_caps[3]);
                    if (x <= 250 || y <= 250)
                    {
                        Console.LogError("Trying to set the window too small, stopping this for your own good. (it's hard to undo if you get stuck in a small window)");
                        return;
                    }
                    Render.SetWindowSize(x, y);
                }))).Action((r) => Render.SetWindowSize(Render.GetMonitorSize())))
                );
        });
    }
}