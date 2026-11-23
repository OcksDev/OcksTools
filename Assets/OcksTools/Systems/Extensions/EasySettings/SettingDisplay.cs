using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class SettingDisplay : MonoBehaviour
{
    [BoxGroup("One of these two must be filled")]
    public SettingData sd;
    [BoxGroup("One of these two must be filled")]
    public BaseBaseSettingLol sd2;


    public bool AutoApply => (sd == null || sd.AutoApply) && (sd2 == null || sd2.GetData().AutoApply);
    public string Prepend;
    public string Postpend;
#if UNITY_EDITOR
    [HideIf("AutoApply")]
#endif
    public bool ReadTrueState = false;
    private TextMeshProUGUI t;
    private void Start()
    {
        t = GetComponent<TextMeshProUGUI>();
        if (sd2 != null) sd = sd2.GetData();
    }
    private void FixedUpdate()
    {
        string s = null;
        if (ReadTrueState) s = sd.GetDisplayMod();
        else s = sd.GetDisplayHeldMod();
        if (s == null) s = GetText();
        t.text = Prepend + s + Postpend;
    }
    public string GetText()
    {
        if (ReadTrueState)
        {
            switch (sd.Type)
            {
                case SettingData.SType.Toggle: return sd.GetValue<bool>() ? "Yes" : "No";
                case SettingData.SType.Slider: return Mathf.RoundToInt(sd.GetValue<float>()).ToString();
                case SettingData.SType.Switcher: return (sd as SettingSwitcherSO).Items[sd.GetValue<int>()];
                case SettingData.SType.Keybind:
                    var q = sd as SettingKeybindSO;
                    if (q.CurrentlySelecting) return "...";
                    return InputManager.GetKeyRichText(q.GetValue<KeyCode>());
                case SettingData.SType.Text: return sd.GetValue<string>();
                default: return "";
            }
        }
        else
        {
            switch (sd.Type)
            {
                case SettingData.SType.Toggle: return sd.GetHeldValue<bool>() ? "Yes" : "No";
                case SettingData.SType.Slider:
                    Mathf.RoundToInt(sd.GetHeldValue<float>()).ToString().DLog();
                    return Mathf.RoundToInt(sd.GetHeldValue<float>()).ToString();
                case SettingData.SType.Switcher: return (sd as SettingSwitcherSO).Items[sd.GetHeldValue<int>()];
                case SettingData.SType.Keybind:
                    var q = sd as SettingKeybindSO;
                    if (q.CurrentlySelecting) return "...";
                    return InputManager.GetKeyRichText(q.GetHeldValue<KeyCode>());
                case SettingData.SType.Text: return sd.GetHeldValue<string>();
                default: return "";
            }
        }
    }
}
