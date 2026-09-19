public class DefSet_FPS : SettingModifierSO<float>
{
    public override float GetDefault(float v)
    {
        return (float)(Render.GetMonitorRefreshRate() / 4);
    }
    public override float ModifySet(float v)
    {
        int f = ((int)v) * 4;
        if (v >= 61) f = -1;
        Render.SetTargetFramerate(f);
        return base.ModifySet(v);
    }
    public override string ModifyDisplay(float v)
    {
        if (v >= 61) return "Unlimited";
        return (((int)v) * 4).ToString();
    }
}
