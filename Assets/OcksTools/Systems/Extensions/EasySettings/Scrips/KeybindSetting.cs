using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindSetting : BaseSettingInput<SettingKeybindSO>
{
    public void ChangeKeybind()
    {
        if (Setting.CurrentlySelecting) return;
        StartCoroutine(WaitForKeybind());
    }
    public override void Init()
    {
        Setting.CurrentlySelecting = false;
    }
    public override void UpdateDisplay() { }

    public IEnumerator WaitForKeybind()
    {
        if (Setting.CurrentlySelecting) yield break;
        Setting.CurrentlySelecting = true;
        List<KeyCode> keys = new();
        yield return new WaitUntil(() => { keys = InputManager.GetAllCurrentlyPressedKeys(); return keys.Count > 0; });
        if (keys[0] == KeyCode.Escape)
        {
            Setting.CurrentlySelecting = false;
            yield break;
        }
        Setting.SetValue(keys[0]);
        yield return new WaitUntil(() => { return !Input.GetKey(KeyCode.Mouse0); });
        Setting.CurrentlySelecting = false;
    }
}
