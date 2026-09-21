public class DefSet_AnisotropicFiltering : SettingModifierSO<bool>
{
    public override bool GetDefault(bool v)
    {
        return Render.GetAnisotropicFiltering();
    }
    public override void ApplyValue(bool v)
    {
        Render.SetAnisotropicFiltering(v);
    }
}
