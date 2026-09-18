using UnityEngine;

public abstract class SettingSO<T> : SettingData
{
    public T Value;
    private T DefaultValue;
    public override void ResetToDefault() => Value = DefaultValue;
    public override void SaveCurrentToDefault() => DefaultValue = Value;
    public virtual void SetValue(T v) => Value = v;
    public virtual T GetValue() => Value;
    public abstract void LoadFromString(string s);
    public abstract string SaveToString();
    public override Q GetValue<Q>() => Value is Q q ? q : throw new System.Exception("wrong type bro");
}


public abstract class SettingData : ScriptableObject
{
    public string Name;
    public abstract T GetValue<T>();
    public abstract void ResetToDefault();
    public abstract void SaveCurrentToDefault();
}