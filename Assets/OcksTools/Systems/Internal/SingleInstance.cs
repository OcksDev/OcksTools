using UnityEngine;

public abstract class SingleInstance<T> : SingleInstanceGeneric<T> where T : SingleInstance<T>
{
    public override void SetBaseInstance() => SetInstance((T)this);
}

public abstract class SingleInstanceGeneric<T> : MonoBehaviour
{
    public static T Instance
    {
        get;
        private set;
    }
    public virtual void SetInstance(T t) { Instance = t; }
    /// <summary>
    /// When overridden, object will be opted out of "DontDestroyOnLoad"
    /// </summary>
    public virtual void DestroyOnLoad()
    {
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }
    public abstract void SetBaseInstance();
    public virtual void Awake()
    {
        SetBaseInstance();
        DestroyOnLoad();
        Awake2();
    }
    public virtual void Awake2() { }
}
