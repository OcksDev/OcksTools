public class DefSet_VSync : SettingModifierSO<bool>
{
    public override bool GetDefault(bool v)
    {
        return Render.GetVSync();
    }
    public override bool ModifySet(bool v)
    {
        Render.SetVSync(v);
        return base.ModifySet(v);
    }
}
