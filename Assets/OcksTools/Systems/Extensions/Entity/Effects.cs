using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class EffectProfile : Containable
{
    //data you pass in
    public float Duration;
    public CombineMethods CombineMethod = CombineMethods.Replace;
    //other data
    [ReadOnly]
    public int Stack = 1;
    [ReadOnly]
    public float TimeRemaining;
    //non-transferable data
    [HideInInspector]
    public int MaxStack;
    public EffectProfile(string type, float time, CombineMethods add_method = CombineMethods.Replace, int stacks = 1)
    {
        Name = type;
        Duration = time;
        CombineMethod = add_method;
        Stack = stacks;
        SetData();
    }
    public EffectProfile()
    {
        SetData();
    }

    public void SetData()
    {
        MaxStack = 0;
        switch (Name)
        {
            //some example effects
            case "Boner Juice":
                MaxStack = 6;
                break;
        }
    }
    public EffectProfile(EffectProfile pp)
    {
        Name = pp.Name;
        Duration = pp.Duration;
        CombineMethod = pp.CombineMethod;
        Stack = pp.Stack;
        TimeRemaining = pp.TimeRemaining;
        SetData();
    }
    public enum CombineMethods
    {
        Replace,
        AddNew,
        CombineStack,
        CombineStackSetTime,
        CombineStackCombineTime,
        CombineTime,
    }
}
