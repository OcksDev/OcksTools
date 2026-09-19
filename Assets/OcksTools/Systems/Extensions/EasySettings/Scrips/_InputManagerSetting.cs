using UnityEngine;

public class _InputManagerSetting : SettingModifierSO<KeyCode>
{
    public override KeyCode GetDefaultLate(KeyCode v)
    {
        Setting.Data.DefaultValue = InputManager.defaultgamekeys[Setting.Name][0];
        return InputManager.gamekeys[Setting.Name][0];
    }
    public override KeyCode ModifySet(KeyCode v)
    {
        InputManager.gamekeys[Setting.Name][0] = v;
        return v;
    }
    public override bool DisableSaving => true;
    public override bool HasLateDefault => true;
}
