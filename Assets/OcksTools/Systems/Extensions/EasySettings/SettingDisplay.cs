using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class SettingDisplay : MonoBehaviour
{
    [BoxGroup("One of these two must be filled")]
    public SettingData sd;
    [BoxGroup("One of these two must be filled")]
    public BaseBaseSettingLol sd2;

    public string Prepend;
    public string Postpend;

    private TextMeshProUGUI t;
    private void Start()
    {
        t = GetComponent<TextMeshProUGUI>();
        if (sd2 != null) sd = sd2.GetData();
    }
    private void FixedUpdate()
    {
        string s = sd.GetDisplayMod();
        if (s == null) s = GetText();
        t.text = Prepend + s + Postpend;
    }
    public string GetText()
    {
        switch (sd.Type)
        {
            case SettingData.SType.Toggle: return sd.GetValue<bool>() ? "Yes" : "No";
            case SettingData.SType.Slider: return Mathf.RoundToInt(sd.GetValue<float>()).ToString();
            case SettingData.SType.Switcher: return (sd as SettingSwitcherSO).Items[sd.GetValue<int>()];
            case SettingData.SType.Keybind:
                var q = sd as SettingKeybindSO;
                if (q.CurrentlySelecting) return "...";
                return InputManager.keynames[q.GetValue<KeyCode>()];
            case SettingData.SType.Text: return sd.GetValue<string>();
            default: return "";
        }
    }
}
