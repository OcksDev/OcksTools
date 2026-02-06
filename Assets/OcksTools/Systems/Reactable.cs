using System;
using UnityEngine;

[System.Serializable]
public class Reactable<T>
{
    [SerializeField]
    private T data;
    private bool diff_marked = false;
    public OXEvent OnValueChanged = new OXEvent();
    public Func<T, T, bool> CompareFunc;
    public Reactable()
    {
        CompareFunc = (x, y) => x.Equals(y);
    }
    public Reactable(T d)
    {
        CompareFunc = (x, y) => x.Equals(y);
        data = d;
    }
    public bool HasChanged()
    {
        bool changed = diff_marked;
        diff_marked = false;
        return changed;
    }
    public T GetValue() { return data; }
    public void SetValue(T d)
    {
        if (!CompareFunc(d, data))
        {
            diff_marked = true;
            data = d;
            OnValueChanged.Invoke();
        }
    }
    public override string ToString()
    {
        return data.ToString();
    }
    public static implicit operator T(Reactable<T> r) => r.GetValue();
}
