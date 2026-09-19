using System.Collections;
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
        yield return new WaitUntil(() => { return InputManager.GetAllCurrentlyPressedKeys().Count > 0; });
        var aa = InputManager.GetAllCurrentlyPressedKeys();
        if (aa[0] == KeyCode.Escape)
        {
            Setting.CurrentlySelecting = false;
            yield break;
        }
        Setting.SetValue(aa[0]);
        yield return new WaitUntil(() => { return !Input.GetKey(KeyCode.Mouse0); });
        Setting.CurrentlySelecting = false;
    }
}
