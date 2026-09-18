using UnityEngine;
using UnityEngine.UI;

public class ToggleSetting : BaseSettingInput<SettingToggleSO>
{
    public Image Renderer;
    public Sprite[] images = null;
    public Color32[] color32s = null;
    public override void UpdateDisplay()
    {
        Renderer.color = color32s[Setting.GetValue() ? 0 : 1];
        Renderer.sprite = images[Setting.GetValue() ? 0 : 1];
    }

    public void Toggle()
    {
        Setting.SetValue(!Setting.GetValue());
        UpdateDisplay();
    }
}
