using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXEvent
{
    public Dictionary<string, Action> StoredMethods = new Dictionary<string, Action>();
    public void Append(string name, Action method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke()
    {
        List<string> killme = new List<string>();
        foreach(var w in StoredMethods)
        {
            if(w.Value != null) w.Value();
            else killme.Add(w.Key);
        }
        foreach(var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}

public class OXEvent<T>
{
    public Dictionary<string, Action<T>> StoredMethods = new Dictionary<string, Action<T>>();
    public void Append(string name, Action<T> method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke(T a)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}
public class OXEvent<T, T2>
{
    public Dictionary<string, Action<T, T2>> StoredMethods = new Dictionary<string, Action<T, T2>>();
    public void Append(string name, Action<T, T2> method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke(T a, T2 b)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}

public class OXEvent<T, T2, T3>
{
    public Dictionary<string, Action<T, T2, T3>> StoredMethods = new Dictionary<string, Action<T, T2, T3>>();
    public void Append(string name, Action<T, T2, T3> method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke(T a, T2 b, T3 c)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}

public class OXEvent<T, T2, T3, T4>
{
    public Dictionary<string, Action<T, T2, T3, T4>> StoredMethods = new Dictionary<string, Action<T, T2, T3, T4>>();
    public void Append(string name, Action<T, T2, T3, T4> method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke(T a, T2 b, T3 c, T4 d)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c,d);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}

public class OXEvent<T, T2, T3, T4, T5>
{
    public Dictionary<string, Action<T, T2, T3, T4, T5>> StoredMethods = new Dictionary<string, Action<T, T2, T3, T4, T5>>();
    public void Append(string name, Action<T, T2, T3, T4, T5> method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public void Invoke(T a, T2 b, T3 c, T4 d, T5 e)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c,d,e);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

}

