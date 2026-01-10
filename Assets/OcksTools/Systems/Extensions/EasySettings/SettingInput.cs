using UnityEngine;
using UnityEngine.UI;

public class SettingInput : MonoBehaviour
{
    public string Type = "";
    public Color32[] color32s = null;
    private Slider slider;
    private Image img;
    private Switcher switcher;
    private KeybindInput keybinder;
    [HideInInspector]
    public bool fard;
    private bool hasattached = false;
    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        img = GetComponent<Image>();
        switcher = GetComponent<Switcher>();
        keybinder = GetComponent<KeybindInput>();
        if (!hasattached)
        {
            hasattached = true;
            SaveSystem.LoadAllData.Append(Tags.GenerateID(), ReadValue);
        }
        else
        {
            ReadValue(SaveSystem.Profile(SaveSystem.ActiveDir));
        }
    }
    public void ToggleVal()
    {
        fard = !fard;
        WriteValue();
        UpdateValue();
    }

    public void WriteValue()
    {
        switch (Type)
        {
            case "MasterVolume":
                SoundSystem.Instance.MasterVolume = slider.value;
                break;
            case "SFXVolume":
                SoundSystem.Instance.SFXVolume = slider.value;
                break;
            case "MusicVolume":
                SoundSystem.Instance.MusicVolume = slider.value;
                break;
            case "TestToggle":
                SaveSystem.Instance.TestBool = fard;
                break;
            case "TestSwitcher":
                SaveSystem.Instance.test = switcher.index;
                break;
            case "TestKeybind":
                SaveSystem.Instance.testkeybind = keybinder.keyCode;
                break;
        }
    }

    public void ReadValue(SaveProfile dict)
    {
        switch (Type)
        {
            case "MasterVolume":
                slider.value = SoundSystem.Instance.MasterVolume;
                UpdateValue();
                break;
            case "SFXVolume":
                slider.value = SoundSystem.Instance.SFXVolume;
                UpdateValue();
                break;
            case "MusicVolume":
                slider.value = SoundSystem.Instance.MusicVolume;
                UpdateValue();
                break;
            case "TestToggle":
                fard = SaveSystem.Instance.TestBool;
                UpdateValue();
                break;
            case "TestSwitcher":
                switcher.index = SaveSystem.Instance.test;
                UpdateValue();
                break;
            case "TestKeybind":
                keybinder.keyCode = SaveSystem.Instance.testkeybind;
                UpdateValue();
                break;
        }
    }

    public void UpdateValue()
    {
        switch (Type)
        {
            case "TestToggle":
                img.color = color32s[fard ? 0 : 1];
                break;
        }
    }
}
