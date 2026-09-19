public class SwitcherSetting : BaseSettingInput<SettingSwitcherSO>
{
    public override void UpdateDisplay() { }

    public void ValueUp()
    {
        int v = Setting.GetValue();
        v = (v + 1).Mod(Setting.Items.Count);
        Setting.SetValue(v);
    }

    public void ValueDown()
    {
        int v = Setting.GetValue();
        v = (v - 1).Mod(Setting.Items.Count);
        Setting.SetValue(v);
    }
}
