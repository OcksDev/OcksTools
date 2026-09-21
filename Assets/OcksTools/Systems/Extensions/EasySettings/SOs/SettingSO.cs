using NaughtyAttributes;
using UnityEngine;

public abstract class SettingSO<T> : SettingData
{
    [AutoCompressField]
    public CoolSettingData<T> InspectorData;
    [NaughtyAttributes.ReadOnly]
    [Label("Runtime Data (view only)")]
    public CoolSettingData<T> Data;
    public SettingModifierSO<T> Modifier;
    public override void ResetToDefault() => Data.Value = Data.DefaultValue;
    public override void SaveCurrentToDefault() => Data.DefaultValue = Data.Value;
    public virtual void SetValue(T v)
    {
        if (Modifier != null) v = Modifier.ModifySet(v);
        Data.Value = v;
    }
    public virtual T GetValue()
    {
        if (Modifier != null) return Modifier.ModifyGet(Data.Value);
        return Data.Value;
    }
    public override Q GetValue<Q>() => GetValue() is Q q ? q : throw new System.Exception("wrong type bro");
    public override void SetValue<Q>(Q v) => SetValue(v);
    public override void DupeData()
    {
        Data = new()
        {
            Value = InspectorData.Value,
            DefaultValue = InspectorData.DefaultValue
        };
        if (Modifier != null)
        {
            Modifier.Setting = this;
            T d = Modifier.GetDefault(Data.Value);
            Data.Value = d;
            Data.DefaultValue = d;
        }
    }
    public override string GetDisplayMod() => Modifier != null ? Modifier.ModifyDisplay(GetValue()) : null;
    public override bool GetShouldSkip() => Modifier != null ? Modifier.DisableSaving : false;
    public override void LateDataFind()
    {
        if (HasLateDefault)
        {
            Modifier.Setting = this;
            T d = Modifier.GetDefaultLate(Data.Value);
            Data.Value = d;
            Data.DefaultValue = d;
        }
    }
    public override bool HasLateDefault => Modifier != null ? Modifier.HasLateDefault : false;
    public override void ApplyModiferValue()
    {
        if (Modifier == null) return;
        Modifier.Setting = this;
        Modifier.ApplyValue(Data.Value);
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
    public abstract string GetDisplayMod();
    public abstract bool GetShouldSkip();
    public abstract void DupeData();
    public abstract void LateDataFind();
    public abstract void ApplyModiferValue();
    public virtual bool HasLateDefault => false;
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

public abstract class SettingModifierSO<T> : ScriptableObject
{
    public SettingSO<T> Setting;
    public virtual T GetDefault(T v) => v;
    public virtual T GetDefaultLate(T v) => v;
    public virtual T ModifyGet(T v) => v;
    public virtual T ModifySet(T v)
    {
        ApplyValue(v);
        return v;
    }
    public virtual void ApplyValue(T v) { }
    public virtual string ModifyDisplay(T v) => null;
    public virtual bool DisableSaving => false;
    public virtual bool HasLateDefault => false;
}