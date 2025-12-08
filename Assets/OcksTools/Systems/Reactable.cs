using UnityEngine;

public class Reactable<T>
{
    private T data;
    bool diff_marked = false;
    public OXEvent OnValueChanged = new OXEvent();
    public Reactable() { }
    public Reactable(T d) { data = d; }
    public bool HasChanged() 
    {
        bool changed = diff_marked;
        diff_marked = false;
        return changed; 
    }
    public T GetValue() { return data; }
    public void SetValue(T d)
    {
        if (!d.Equals(data))
        {
            diff_marked = true;
            data = d;
            OnValueChanged.Invoke();
        }
    }
}
