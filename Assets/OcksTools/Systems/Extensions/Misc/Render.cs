using UnityEngine;

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
        return QualitySettings.vSyncCount == 1;
    }
    public static bool GetAnisotropicFiltering()
    {
        return QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable;
    }
    public static int GetAntiAliasing()
    {
        return QualitySettings.antiAliasing;
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
    public static void SetTargetFramerate(int amount)
    {
        Application.targetFrameRate = amount;
    }
    public static void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled?1:0;
    }
    public static void SetAnisotropicFiltering(bool enabled)
    {
        QualitySettings.anisotropicFiltering = enabled? AnisotropicFiltering.Enable : AnisotropicFiltering.Enable;
    }
    public static void SetAntiAliasing(int amount)
    {
        QualitySettings.antiAliasing = amount;
    }
}
