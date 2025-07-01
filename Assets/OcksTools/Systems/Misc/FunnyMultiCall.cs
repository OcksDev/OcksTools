using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCallerHandler
{
    /*
     * The idea for this is to store the outputs of deterministic functions so they dont need to be recalculated many times, idk how useful this will be lol
     */


    public static Dictionary<IMultiCaller, bool> all = new Dictionary<IMultiCaller, bool>();
    public static void ClearAll()
    {
        foreach (var caller in all)
        {
            caller.Key.ClearDict();
        }
        all.Clear();
    }
}
public class Caller<T, T_Output> : IMultiCaller where T : class
{
    public Dictionary<string, Dictionary<T, T_Output>> dict = new Dictionary<string, Dictionary<T, T_Output>>();
    public void ClearDict()
    {
        dict.Clear();
    }
    public T_Output Multicall(Func<T, T_Output> aa, string index, T in1)
    {
        if (!GlobalCallerHandler.all.ContainsKey(this)) GlobalCallerHandler.all.Add(this, false);
        bool needsfull = false;
        if (dict.ContainsKey(index))
        {
            if (dict[index].ContainsKey(in1))
            {
                return dict[index][in1];
            }
        }
        else
        {
            needsfull = true;
        }
        var x = aa(in1);
        if (needsfull)
        {
            dict.Add(index, new Dictionary<T, T_Output>() { { in1, x } });
        }
        else
        {
            dict[index].Add(in1, x);
        }
        return x;
    }
}
public class Caller<T_Output> : IMultiCaller
{
    public Dictionary<string, T_Output> dict = new Dictionary<string, T_Output>();
    public void ClearDict()
    {
        dict.Clear();
    }
    public T_Output Multicall(Func<T_Output> aa, string index)
    {
        if (!GlobalCallerHandler.all.ContainsKey(this)) GlobalCallerHandler.all.Add(this, false);
        bool needsfull = false;
        if (dict.ContainsKey(index))
        {
            return dict[index];
        }
        else
        {
            needsfull = true;
        }
        var x = aa();
        dict.Add(index, x);
        return x;
    }
}
public interface IMultiCaller
{
    public void ClearDict();
}