using UnityEngine;

public abstract class SingleInstance<T> : MonoBehaviour where T : SingleInstance<T>
{
    public static T Instance 
    {
        get;
        private set;
    }
    private void SetInstance(T t) {  Instance = t; }
    public void Awake()
    {
        SetInstance((T)this);
        Awake2();
    }
    public virtual void Awake2() { }
}
