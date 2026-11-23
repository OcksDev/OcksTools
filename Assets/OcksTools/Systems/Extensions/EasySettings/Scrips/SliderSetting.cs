using UnityEngine.UI;

public class SliderSetting : BaseSettingInput<SettingSliderSO>
{
    public Slider slider;
    public override void UpdateDisplay()
    {
        slider.value = Setting.GetHeldValue();
    }

    public void SetValue()
    {
        Setting.SetValue(slider.value);
        UpdateDisplay();
    }
}
