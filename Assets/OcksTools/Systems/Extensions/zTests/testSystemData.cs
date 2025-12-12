using UnityEngine;

public class testSystemData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Debug.Log(Render.GetMonitorRefreshRate());
        Debug.Log(Render.GetTargetFramerate());
        Debug.Log(Render.GetWindowSize());
        Debug.Log(Render.GetMonitorSize());
    }

}
