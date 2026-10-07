public class DefSet_ResY : SettingModifierSO<int>
{
    public override int GetDefault(SettingSO<int> setting, int v)
    {
        var x = Render.GetWindowSize().y.ToString();
        int i = (setting as SettingSwitcherSO).Items.IndexOf(x);
        if (i > -1) return i;
        return base.GetDefault(setting, v);
    }
    public override void ApplyValue(SettingSO<int> setting, int v)
    {
        int sz = int.Parse((setting as SettingSwitcherSO).Items[v]);
        var rsz = Render.GetWindowSize();
        rsz.y = sz;
        Render.SetWindowSize(rsz);
    }
}
