using UnityEngine;

public class _InputManagerSetting : SettingModifierSO<KeyCode>
{
    public override KeyCode GetDefaultLate(SettingSO<KeyCode> setting, KeyCode v)
    {
        setting.Data.DefaultValue = InputManager.defaultgamekeys[setting.Name][0];
        return InputManager.gamekeys[setting.Name][0];
    }
    public override KeyCode ModifySet(SettingSO<KeyCode> setting, KeyCode v)
    {
        InputManager.gamekeys[setting.Name][0] = v;
        return v;
    }
    public override bool DisableSaving => true;
    public override bool HasLateDefault => true;
}
