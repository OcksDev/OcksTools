using UnityEngine;

public class DefSet_FullScreen : SettingModifierSO<int>
{
    public override int GetDefault(int v)
    {
        var a = Render.GetFullscreen();
        switch (a)
        {
            case FullScreenMode.Windowed: return 0;
            case FullScreenMode.MaximizedWindow: return 1;
            case FullScreenMode.FullScreenWindow: return 2;
            case FullScreenMode.ExclusiveFullScreen: return 3;
        }
        return base.GetDefault(v);
    }

    public override int ModifySet(int v)
    {
        switch (v)
        {
            case 0: Render.SetFullscreen(FullScreenMode.Windowed); break;
            case 1: Render.SetFullscreen(FullScreenMode.MaximizedWindow); break;
            case 2: Render.SetFullscreen(FullScreenMode.FullScreenWindow); break;
            case 3: Render.SetFullscreen(FullScreenMode.ExclusiveFullScreen); break;
        }
        return base.ModifySet(v);
    }
}
