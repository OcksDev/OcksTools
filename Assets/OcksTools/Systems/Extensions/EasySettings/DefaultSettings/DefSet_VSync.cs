public class DefSet_VSync : SettingModifierSO<bool>
{
    public override bool GetDefault(SettingSO<bool> setting, bool v)
    {
        return Render.GetVSync();
    }
    public override void ApplyValue(SettingSO<bool> setting, bool v)
    {
        Render.SetVSync(v);
    }
}
