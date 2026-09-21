public class DefSet_ResY : SettingModifierSO<int>
{
    public override int GetDefault(int v)
    {
        var x = Render.GetWindowSize().y.ToString();
        int i = (Setting as SettingSwitcherSO).Items.IndexOf(x);
        if (i > -1) return i;
        return base.GetDefault(v);
    }
    public override void ApplyValue(int v)
    {
        int sz = int.Parse((Setting as SettingSwitcherSO).Items[v]);
        var rsz = Render.GetWindowSize();
        rsz.y = sz;
        Render.SetWindowSize(rsz);
    }
}
