using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalEvent
{
    private static Dictionary<string, OXEvent> Nerds = new Dictionary<string, OXEvent>();
    public static void Append(string even, string name, Action method)
    {
        if (!Nerds.ContainsKey(even)) Nerds.Add(even, new OXEvent());
        Nerds[even].Append(name, method);
    }
    public static void Append(string even, Action method)
    {
        if (!Nerds.ContainsKey(even)) Nerds.Add(even, new OXEvent());
        Nerds[even].Append(method);
    }

    public static void Remove(string even, string name)
    {
        if (!Nerds.ContainsKey(even)) return;
        Nerds[even].Remove(name);
    }
    public static void Invoke(string even)
    {
        if(Nerds.ContainsKey(even)) Nerds[even].Invoke();
    }

    public static A Set<A>(out A banana, A newval, string eventt)
    {
        banana = newval;
        Invoke(eventt);
        return banana;
    }

}

public class GlobalMethod
{
    private static Dictionary<string, System.Action> Nerds = new Dictionary<string, System.Action>();
    public static void Set(string even, Action method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even)
    {
        if (Nerds.ContainsKey(even)) Nerds[even]();
    }
}
public class GlobalMethod<T>
{
    private static Dictionary<string, System.Action<T>> Nerds = new Dictionary<string, System.Action<T>>();
    public static void Set(string even, Action<T> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a);
    }
}
public class GlobalMethod<T,T2>
{
    private static Dictionary<string, System.Action<T, T2>> Nerds = new Dictionary<string, System.Action<T, T2>>();
    public static void Set(string even, Action<T, T2> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a, T2 b)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a,b);
    }
}
public class GlobalMethod<T,T2,T3>
{
    private static Dictionary<string, System.Action<T, T2, T3>> Nerds = new Dictionary<string, System.Action<T, T2, T3>>();
    public static void Set(string even, Action<T, T2, T3> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a,T2 b, T3 c)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a,b,c);
    }
}
public class GlobalMethod<T,T2, T3, T4>
{
    private static Dictionary<string, System.Action<T, T2, T3, T4>> Nerds = new Dictionary<string, System.Action<T, T2, T3, T4>>();
    public static void Set(string even, Action<T, T2, T3, T4> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a, T2 b, T3 c, T4 d)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a,b,c,d);
    }
}




public class OXEvent
{
    public Dictionary<string, Action> StoredMethods = new Dictionary<string, Action>();
    public static bool SuccessfulHit = false;
    public void Append(string name, Action method)
    {
        if (!StoredMethods.ContainsKey(name))
        {
            StoredMethods.Add(name, method);
        }
    }
    public void Append(Action method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke()
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if(w.Value != null) w.Value();
            else killme.Add(w.Key);
        }
        foreach(var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
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
    public void Append(Action<T> method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke(T a)
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
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
    public void Append(Action<T, T2> method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke(T a, T2 b)
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
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
    public void Append(Action<T, T2, T3> method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke(T a, T2 b, T3 c)
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
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
    public void Append(Action<T, T2, T3, T4> method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke(T a, T2 b, T3 c, T4 d)
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c,d);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
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
    public void Append(Action<T, T2, T3, T4, T5> method)
    {
        var dd = Tags.GenerateID();
        if (!StoredMethods.ContainsKey(dd))
        {
            StoredMethods.Add(dd, method);
        }
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool Invoke(T a, T2 b, T3 c, T4 d, T5 e)
    {
        List<string> killme = new List<string>();
        OXEvent.SuccessfulHit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a,b,c,d,e);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
        return OXEvent.SuccessfulHit;
    }

}


public class AddToEvent : Attribute
{
    public string dingle;
    public AddToEvent(string a)
    {
        dingle = a;
    }
}