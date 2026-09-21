public class DefSet_VSync : SettingModifierSO<bool>
{
    public override bool GetDefault(bool v)
    {
        return Render.GetVSync();
    }
    public override void ApplyValue(bool v)
    {
        Render.SetVSync(v);
    }
}
