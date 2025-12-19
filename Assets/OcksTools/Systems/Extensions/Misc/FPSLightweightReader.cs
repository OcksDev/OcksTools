using NaughtyAttributes;
using UnityEngine;

public class FPSLightweightReader : SingleInstance<FPSLightweightReader>
{
    public float ReadInterval = 0.025f; //how often to poll fps
    public float OutReadInterval = 0.1f; // how share the fps (helps prevent fast changing numbers)
    private float tm = 0f;
    private float otm = 0f;
    private int frameCount = 0;
    public void Update()
    {
        frameCount++;
        if ((tm -= Time.unscaledDeltaTime) <= 0 && Time.time > 0.2)
        {
            tm += ReadInterval;
            LastMeasuredFPS = Mathf.FloorToInt(frameCount / ReadInterval) + LastMeasuredFPS;
            LastMeasuredFPS /= 2; // simple moving average to slightly smooth out fps
            frameCount = 0;
        }
        if ((otm -= Time.unscaledDeltaTime) <= 0 && Time.time > 0.2)
        {
            otm += OutReadInterval;
            LastOutgoingFPS = LastMeasuredFPS;
        }
    }
    [HideInInspector]
    public int LastMeasuredFPS = 0;
    [ReadOnly]
    public int LastOutgoingFPS = 0; // read this one
}
