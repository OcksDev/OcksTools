using UnityEngine;

public class Toggler : SettingInput
{
    public Sprite[] images = null;
    public Color32[] color32s = null;
    public override void UpdateValue()
    {
        switch (Type)
        {
            case "TestToggle":
                img.color = color32s[fard ? 0 : 1];
                img.sprite = images[fard ? 0 : 1];
                break;
        }
    }
}
