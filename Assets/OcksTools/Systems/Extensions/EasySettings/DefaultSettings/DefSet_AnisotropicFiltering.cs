public class DefSet_AnisotropicFiltering : SettingModifierSO<bool>
{
    public override bool GetDefault(SettingSO<bool> setting, bool v)
    {
        return Render.GetAnisotropicFiltering();
    }
    public override void ApplyValue(SettingSO<bool> setting, bool v)
    {
        Render.SetAnisotropicFiltering(v);
    }
}
