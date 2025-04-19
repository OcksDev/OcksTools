using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayValue : MonoBehaviour
{
    public TextMeshProUGUI myself;
    public SettingInput reference;
    public DisplayType displayType;
    public DisplayDataType displayDataType;
    private Slider slider;
    private Switcher switcher;
    private KeybindInput keybinder;
    public string pre_text = "";
    public string post_text = "";
    // Start is called before the first frame update
    void Awake()
    {
        if (displayType == DisplayType.Slider) slider = reference.GetComponent<Slider>();
        if (displayType == DisplayType.Switcher) switcher = reference.GetComponent<Switcher>();
        if (displayType == DisplayType.Keybind) keybinder = reference.GetComponent<KeybindInput>();
        if (myself == null) myself = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Func<string, string> textme = (x) =>
        {
            return pre_text + x + post_text;
        };



        switch (displayType)
        {
            case DisplayType.Slider:
                switch (displayDataType)
                {
                    case DisplayDataType.Text: myself.text = "Unsupported"; break;
                    case DisplayDataType.Text_Full: myself.text = "Unsupported"; break;
                    case DisplayDataType.Percent: myself.text = textme(((int)(slider.normalizedValue * 100)).ToString()); break;
                    case DisplayDataType.Value: myself.text = textme(slider.value.ToString()); break;
                }
                break;
            case DisplayType.Toggle:
                switch (displayDataType)
                {
                    case DisplayDataType.Text: myself.text = textme(reference.fard ? "On" : "Off"); break;
                    case DisplayDataType.Text_Full: myself.text = "Unsupported"; break;
                    case DisplayDataType.Percent: myself.text = textme(reference.fard ? "100" : "0"); break;
                    case DisplayDataType.Value: myself.text = textme(reference.fard ? "1" : "0"); break;
                }
                break;
            case DisplayType.Switcher:
                switch (displayDataType)
                {
                    case DisplayDataType.Text: myself.text = textme(switcher.Items[switcher.index]); break;
                    case DisplayDataType.Text_Full: myself.text = "Unsupported"; break;
                    case DisplayDataType.Percent: myself.text = textme(((int)((((float)switcher.index)/(switcher.Items.Count-1))*100)).ToString()); break;
                    case DisplayDataType.Value: myself.text = textme(switcher.index.ToString()); break;
                }
                break;
            case DisplayType.Keybind:
                if (keybinder.CurrentlySelecting)
                {
                    myself.text = "Press a key..."; break;
                }
                switch (displayDataType)
                {
                    case DisplayDataType.Text: myself.text = textme(InputManager.keynames[keybinder.keyCode]); break;
                    case DisplayDataType.Text_Full: myself.text = textme(keybinder.keyCode.ToString()); break;
                    case DisplayDataType.Percent: myself.text = "Unsupported"; break;
                    case DisplayDataType.Value: myself.text = "Unsupported"; break;
                }
                break;
        }
    }



    public enum DisplayType
    {
        Slider,
        Toggle,
        Switcher,
        Keybind,
    }
    public enum DisplayDataType
    {
        Value,
        Percent,
        Text,
        Text_Full,
    }
}
