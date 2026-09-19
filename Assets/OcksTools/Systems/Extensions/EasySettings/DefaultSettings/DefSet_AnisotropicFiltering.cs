public class DefSet_AnisotropicFiltering : SettingModifierSO<bool>
{
    public override bool GetDefault(bool v)
    {
        return Render.GetAnisotropicFiltering();
    }
    public override bool ModifySet(bool v)
    {
        Render.SetAnisotropicFiltering(v);
        return base.ModifySet(v);
    }
}
