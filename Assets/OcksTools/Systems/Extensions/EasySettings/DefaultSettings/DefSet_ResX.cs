public class DefSet_ResX : SettingModifierSO<int>
{
    public override int GetDefault(SettingSO<int> setting, int v)
    {
        var x = Render.GetWindowSize().x.ToString();
        int i = (setting as SettingSwitcherSO).Items.IndexOf(x);
        if (i > -1) return i;
        return base.GetDefault(setting, v);
    }

    public override void ApplyValue(SettingSO<int> setting, int v)
    {
        int sz = int.Parse((setting as SettingSwitcherSO).Items[v]);
        var rsz = Render.GetWindowSize();
        rsz.x = sz;
        Render.SetWindowSize(rsz);
    }
}
