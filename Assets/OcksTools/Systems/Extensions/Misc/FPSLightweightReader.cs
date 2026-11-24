using NaughtyAttributes;
using UnityEngine;

public class FPSLightweightReader : SingleInstance<FPSLightweightReader>
{
    public float ReadInterval = 0.07f; // how fast to update the fps (helps prevent fast changing numbers)
    private float otm = 0f;
    public void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt > 0f)
            LastMeasuredFPS = (int)Mathf.Lerp(LastMeasuredFPS, 1f / dt, 0.15f.TimeStableLerp());

        if ((otm -= dt) <= 0 && Time.unscaledTime > 0.2f)
        {
            otm += ReadInterval;
            LastOutgoingFPS = LastMeasuredFPS;
        }
    }
    [HideInInspector]
    public int LastMeasuredFPS = 0;
    [ReadOnly]
    public int LastOutgoingFPS = 0; // read this one
}
