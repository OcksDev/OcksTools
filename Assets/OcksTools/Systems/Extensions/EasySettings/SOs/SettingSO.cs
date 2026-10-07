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
        if (Modifier != null) v = Modifier.ModifySet(this, v);
        Data.Value = v;
    }
    public virtual T GetValue()
    {
        if (Modifier != null) return Modifier.ModifyGet(this, Data.Value);
        return Data.Value;
    }
    public override Q GetValue<Q>()
    {
        T value = GetValue();
        if (value is Q q) return q;
        throw new System.InvalidCastException(
            $"Setting '{Name}' stores {typeof(T).Name}, but {typeof(Q).Name} was requested.");
    }
    public override void SetValue<Q>(Q v)
    {
        if (v is T t) SetValue(t);
        else throw new System.InvalidCastException(
            $"Setting '{Name}' stores {typeof(T).Name}, but a {typeof(Q).Name} was provided.");
    }
    public override void DupeData()
    {
        Data = new()
        {
            Value = InspectorData.Value,
            DefaultValue = InspectorData.DefaultValue
        };
        if (Modifier != null)
        {
            T d = Modifier.GetDefault(this, Data.Value);
            Data.Value = d;
            Data.DefaultValue = d;
        }
    }
    public override string GetDisplayMod() => Modifier != null ? Modifier.ModifyDisplay(this, GetValue()) : null;
    public override bool GetShouldSkip() => Modifier != null ? Modifier.DisableSaving : false;
    public override void LateDataFind()
    {
        if (HasLateDefault)
        {
            T d = Modifier.GetDefaultLate(this, Data.Value);
            Data.Value = d;
            Data.DefaultValue = d;
        }
    }
    public override bool HasLateDefault => Modifier != null ? Modifier.HasLateDefault : false;
    public override void ApplyModiferValue()
    {
        if (Modifier == null) return;
        Modifier.ApplyValue(this, Data.Value);
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
    /// <summary>Reads this setting from the profile using the typed getter. Keeps the current value if the key is missing.</summary>
    public abstract void LoadFromProfile(SaveProfile dict, string key);
    /// <summary>Writes this setting to the profile using the typed setter.</summary>
    public abstract void SaveToProfile(SaveProfile dict, string key);
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
    public virtual T GetDefault(SettingSO<T> setting, T v) => v;
    public virtual T GetDefaultLate(SettingSO<T> setting, T v) => v;
    public virtual T ModifyGet(SettingSO<T> setting, T v) => v;
    public virtual T ModifySet(SettingSO<T> setting, T v)
    {
        ApplyValue(setting, v);
        return v;
    }
    public virtual void ApplyValue(SettingSO<T> setting, T v) { }
    public virtual string ModifyDisplay(SettingSO<T> setting, T v) => null;
    public virtual bool DisableSaving => false;
    public virtual bool HasLateDefault => false;
}
