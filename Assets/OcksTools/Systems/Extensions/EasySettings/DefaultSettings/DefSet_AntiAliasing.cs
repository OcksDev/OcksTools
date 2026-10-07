public class DefSet_AntiAliasing : SettingModifierSO<int>
{
    public override int GetDefault(SettingSO<int> setting, int v)
    {
        int x = Render.GetAntiAliasing();
        if (x == 4) return 3;
        if (x == 8) return 4;
        return x;
    }
    public override void ApplyValue(SettingSO<int> setting, int v)
    {
        if (v == 4) Render.SetAntiAliasing(8);
        else if (v == 3) Render.SetAntiAliasing(4);
        else Render.SetAntiAliasing(v);
    }
}
