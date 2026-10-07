

using UnityEngine;

public class DefSet_FPS : SettingModifierSO<float>
{
    public override float GetDefault(SettingSO<float> setting, float v)
    {
        return (float)(Render.GetMonitorRefreshRate() / 4);
    }
    public override void ApplyValue(SettingSO<float> setting, float v)
    {
        int f = ((int)v) * 4;
        if (v >= 61) f = -1;
        Render.SetTargetFramerate(f);
        Debug.Log(f);
    }
    public override string ModifyDisplay(SettingSO<float> setting, float v)
    {
        if (v >= 61) return "Unlimited";
        return (((int)v) * 4).ToString();
    }
}
