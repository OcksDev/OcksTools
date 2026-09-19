public class DefSet_ResX : SettingModifierSO<int>
{
    public override int GetDefault(int v)
    {
        var x = Render.GetWindowSize().x.ToString();
        int i = (Setting as SettingSwitcherSO).Items.IndexOf(x);
        if (i > -1) return i;
        return base.GetDefault(v);
    }

    public override int ModifySet(int v)
    {
        int sz = int.Parse((Setting as SettingSwitcherSO).Items[v]);
        var rsz = Render.GetWindowSize();
        rsz.x = sz;
        Render.SetWindowSize(rsz);
        return base.ModifySet(v);
    }
}
