using UnityEngine;

public abstract class SettingSO<T> : SettingData
{
    [AutoCompressField]
    public CoolSettingData<T> InspectorData;
    protected CoolSettingData<T> Data;
    public override void ResetToDefault() => Data.Value = Data.DefaultValue;
    public override void SaveCurrentToDefault() => Data.DefaultValue = Data.Value;
    public virtual void SetValue(T v) => Data.Value = v;
    public virtual T GetValue() => Data.Value;
    public override Q GetValue<Q>() => Data.Value is Q q ? q : throw new System.Exception("wrong type bro");
    public override void SetValue<Q>(Q v) => SetValue(v);
    public override void DupeData()
    {
        Data = new()
        {
            Value = InspectorData.Value,
            DefaultValue = InspectorData.DefaultValue
        };
    }
}


public abstract class SettingData : ScriptableObject
{
    public string Name;
    public virtual SType Type => SType.Toggle;
    public abstract T GetValue<T>();
    public abstract void SetValue<T>(T v);
    public abstract void ResetToDefault();
    public abstract void SaveCurrentToDefault();
    public abstract void LoadFromString(string s);
    public abstract string SaveToString();
    public abstract void DupeData();
    public enum SType
    {
        Toggle,
        Keybind,
        Slider,
        Switcher,
        Text,
    }
}

[System.Serializable]
public class CoolSettingData<T>
{
    public T Value;
    [HideInInspector]
    public T DefaultValue;
}