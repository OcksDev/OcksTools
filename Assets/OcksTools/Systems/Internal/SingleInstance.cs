using UnityEngine;

public abstract class SingleInstance<T> : MonoBehaviour where T : SingleInstance<T>
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
    public void Awake()
    {
        SetInstance((T)this);
        DestroyOnLoad();
        Awake2();
    }
    public virtual void Awake2() { }
}
