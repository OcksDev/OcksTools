using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveData : MonoBehaviour
{
    public OXData<string> wank;
    // Start is called before the first frame update
    void Start()
    {
        OXData<string> dat = new OXData<string>();
        dat.Value = "Test Banana";
        Debug.Log(dat.Value);
        dat.OnValueChanged_GetValue += Test;
        dat.Value = "Yummy Banana";
        Debug.Log(dat.Value);
        var weenis = dat;
        weenis.Value = "Cum";
    }
    public void Test(string weenor)
    {
        Debug.Log(weenor + " Wanker");
    }
}

[Serializable]
public class OXData<T>
{
    [SerializeField]
    private T hidden_value;
    public T Value   // property
    {
        get { return hidden_value; }   // get method
        set { if (!value.Equals(Value)) {
                hidden_value = value;
                OnValueChanged?.Invoke();
                OnValueChanged_GetValue?.Invoke(value);
            } }  // set method
    }

    public delegate void die1();
    public delegate void die2<T2>(T2 val);
    public event die1 OnValueChanged;
    public event die2<T> OnValueChanged_GetValue;

}
