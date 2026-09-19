using UnityEngine;

public class _VolumeSetting : SettingModifierSO<float>
{
    public override float GetDefaultLate(float v)
    {
        return SoundSystem.Instance.GetChannelVolume(Setting.Name);
    }

    public override float ModifySet(float v)
    {
        SoundSystem.Instance.SetChannelVolume(Setting.Name, v);
        return base.ModifySet(v);
    }
    public override string ModifyDisplay(float v)
    {
        return Mathf.RoundToInt(v * 100).ToString();
    }
    public override bool DisableSaving => true;
    public override bool HasLateDefault => true;
}
