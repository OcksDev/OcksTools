using UnityEngine;

public class _VolumeSetting : SettingModifierSO<float>
{
    public override float GetDefaultLate(SettingSO<float> setting, float v)
    {
        return SoundSystem.Instance.GetChannelVolume(setting.Name);
    }

    public override float ModifySet(SettingSO<float> setting, float v)
    {
        SoundSystem.Instance.SetChannelVolume(setting.Name, v);
        return base.ModifySet(setting, v);
    }
    public override string ModifyDisplay(SettingSO<float> setting, float v)
    {
        return Mathf.RoundToInt(v * 100).ToString();
    }
    public override bool DisableSaving => true;
    public override bool HasLateDefault => true;
}
