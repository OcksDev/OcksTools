using NaughtyAttributes;
using UnityEngine;

public class FPSLightweightReader : SingleInstance<FPSLightweightReader>
{
    public float ReadInterval = 0.1f; // how share the fps (helps prevent fast changing numbers)
    private float otm = 0f;
    public void Update()
    {
        LastMeasuredFPS = (int)Mathf.Lerp(LastMeasuredFPS, 1 / Time.smoothDeltaTime, 0.8f.TimeStableLerp());
        if ((otm -= Time.unscaledDeltaTime) <= 0 && Time.time > 0.2)
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
