using System;
using System.Collections.Generic;

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
        if (Nerds.ContainsKey(even)) Nerds[even].Invoke();
    }
}

/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */

public class OXEvent
{
    public Dictionary<string, Action> StoredMethods = new Dictionary<string, Action>();
    public static bool SuccessfulHit = false;
    public void Append(string name, Action method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool InvokeWithHitCheck()
    {
        OXEvent.SuccessfulHit = false;
        Invoke();
        return OXEvent.SuccessfulHit;
    }
    public void Invoke()
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value();
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
    }
}

/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */
public class OXEvent<T>
{
    public Dictionary<string, Action<T>> StoredMethods = new Dictionary<string, Action<T>>();
    public void Append(string name, Action<T> method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action<T> method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool InvokeWithHitCheck(T a)
    {
        OXEvent.SuccessfulHit = false;
        Invoke(a);
        return OXEvent.SuccessfulHit;
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

    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
    }
}
/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */
public class OXEvent<T, T2>
{
    public Dictionary<string, Action<T, T2>> StoredMethods = new Dictionary<string, Action<T, T2>>();
    public void Append(string name, Action<T, T2> method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action<T, T2> method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool InvokeWithHitCheck(T a, T2 b)
    {
        OXEvent.SuccessfulHit = false;
        Invoke(a, b);
        return OXEvent.SuccessfulHit;
    }
    public void Invoke(T a, T2 b)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a, b);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
    }

}

/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */
public class OXEvent<T, T2, T3>
{
    public Dictionary<string, Action<T, T2, T3>> StoredMethods = new Dictionary<string, Action<T, T2, T3>>();
    public void Append(string name, Action<T, T2, T3> method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action<T, T2, T3> method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool InvokeWithHitCheck(T a, T2 b, T3 c)
    {
        OXEvent.SuccessfulHit = false;
        Invoke(a, b, c);
        return OXEvent.SuccessfulHit;
    }
    public void Invoke(T a, T2 b, T3 c)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a, b, c);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
    }
}

/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */
public class OXEvent<T, T2, T3, T4>
{
    public Dictionary<string, Action<T, T2, T3, T4>> StoredMethods = new Dictionary<string, Action<T, T2, T3, T4>>();
    public void Append(string name, Action<T, T2, T3, T4> method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action<T, T2, T3, T4> method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }
    public bool InvokeWithHitCheck(T a, T2 b, T3 c, T4 d)
    {
        OXEvent.SuccessfulHit = false;
        Invoke(a, b, c, d);
        return OXEvent.SuccessfulHit;
    }
    public void Invoke(T a, T2 b, T3 c, T4 d)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a, b, c, d);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
    }
}

/*
 * WARNING: InvokeWithHitCheck() is NOT thread safe, if using oxevent on other threads: DO NOT USE THIS
 */
public class OXEvent<T, T2, T3, T4, T5>
{
    public Dictionary<string, Action<T, T2, T3, T4, T5>> StoredMethods = new Dictionary<string, Action<T, T2, T3, T4, T5>>();
    public void Append(string name, Action<T, T2, T3, T4, T5> method)
    {
        StoredMethods.AddOrUpdate(name, method);
    }
    public void Append(Action<T, T2, T3, T4, T5> method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Remove(string name)
    {
        if (StoredMethods.ContainsKey(name))
        {
            StoredMethods.Remove(name);
        }
    }

    public bool InvokeWithHitCheck(T a, T2 b, T3 c, T4 d, T5 e)
    {
        OXEvent.SuccessfulHit = false;
        Invoke(a, b, c, d, e);
        return OXEvent.SuccessfulHit;
    }
    public void Invoke(T a, T2 b, T3 c, T4 d, T5 e)
    {
        List<string> killme = new List<string>();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(a, b, c, d, e);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }

    public bool Contains(string name)
    {
        return StoredMethods.ContainsKey(name);
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


public class OXEventLayered
{
    public List<MultiRef<int, OXEvent>> StoredEvents = new List<MultiRef<int, OXEvent>>();
    public void Append(int layer, string name, Action method)
    {
        MultiRef<int, OXEvent> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent>(layer, x2));
    }
    public void Append(int layer, Action method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action method)
    {
        Append(0, name, method);
    }
    public void Append(Action method)
    {
        Append(0, method);
    }
    public void Invoke()
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke();
        }
    }
}
public class OXEventLayered<T>
{
    public List<MultiRef<int, OXEvent<T>>> StoredEvents = new List<MultiRef<int, OXEvent<T>>>();
    public void Append(int layer, string name, Action<T> method)
    {
        MultiRef<int, OXEvent<T>> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent<T>();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent<T>>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent<T>();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent<T>>(layer, x2));
    }
    public void Append(int layer, Action<T> method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action<T> method)
    {
        Append(0, name, method);
    }
    public void Append(Action<T> method)
    {
        Append(0, method);
    }
    public void Invoke(T a)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(a);
        }
    }
}
public class OXEventLayered<T, T2>
{
    public List<MultiRef<int, OXEvent<T, T2>>> StoredEvents = new List<MultiRef<int, OXEvent<T, T2>>>();
    public void Append(int layer, string name, Action<T, T2> method)
    {
        MultiRef<int, OXEvent<T, T2>> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent<T, T2>();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent<T, T2>>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent<T, T2>();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent<T, T2>>(layer, x2));
    }
    public void Append(int layer, Action<T, T2> method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action<T, T2> method)
    {
        Append(0, name, method);
    }
    public void Append(Action<T, T2> method)
    {
        Append(0, method);
    }
    public void Invoke(T a, T2 b)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(a, b);
        }
    }
}

public class OXEventLayered<T, T2, T3>
{
    public List<MultiRef<int, OXEvent<T, T2, T3>>> StoredEvents = new List<MultiRef<int, OXEvent<T, T2, T3>>>();
    public void Append(int layer, string name, Action<T, T2, T3> method)
    {
        MultiRef<int, OXEvent<T, T2, T3>> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent<T, T2, T3>();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent<T, T2, T3>>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent<T, T2, T3>();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent<T, T2, T3>>(layer, x2));
    }
    public void Append(int layer, Action<T, T2, T3> method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action<T, T2, T3> method)
    {
        Append(0, name, method);
    }
    public void Append(Action<T, T2, T3> method)
    {
        Append(0, method);
    }
    public void Invoke(T a, T2 b, T3 c)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(a, b, c);
        }
    }
}

public class OXEventLayered<T, T2, T3, T4>
{
    public List<MultiRef<int, OXEvent<T, T2, T3, T4>>> StoredEvents = new List<MultiRef<int, OXEvent<T, T2, T3, T4>>>();
    public void Append(int layer, string name, Action<T, T2, T3, T4> method)
    {
        MultiRef<int, OXEvent<T, T2, T3, T4>> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent<T, T2, T3, T4>();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent<T, T2, T3, T4>>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent<T, T2, T3, T4>();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent<T, T2, T3, T4>>(layer, x2));
    }
    public void Append(int layer, Action<T, T2, T3, T4> method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action<T, T2, T3, T4> method)
    {
        Append(0, name, method);
    }
    public void Append(Action<T, T2, T3, T4> method)
    {
        Append(0, method);
    }
    public void Invoke(T a, T2 b, T3 c, T4 d)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(a, b, c, d);
        }
    }
}


public class OXEventLayered<T, T2, T3, T4, T5>
{
    public List<MultiRef<int, OXEvent<T, T2, T3, T4, T5>>> StoredEvents = new List<MultiRef<int, OXEvent<T, T2, T3, T4, T5>>>();
    public void Append(int layer, string name, Action<T, T2, T3, T4, T5> method)
    {
        MultiRef<int, OXEvent<T, T2, T3, T4, T5>> w;
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            w = StoredEvents[i];
            if (w.a == layer)
            {
                w.b.Append(method);
                return;
            }
            else if (w.a > layer)
            {
                var x = new OXEvent<T, T2, T3, T4, T5>();
                x.Append(method);
                StoredEvents.Insert(i, new MultiRef<int, OXEvent<T, T2, T3, T4, T5>>(layer, x));
                return;
            }
        }
        var x2 = new OXEvent<T, T2, T3, T4, T5>();
        x2.Append(method);
        StoredEvents.Add(new MultiRef<int, OXEvent<T, T2, T3, T4, T5>>(layer, x2));
    }
    public void Append(int layer, Action<T, T2, T3, T4, T5> method)
    {
        var dd = Tags.GenerateID();
        Append(layer, dd, method);
    }
    public void Append(string name, Action<T, T2, T3, T4, T5> method)
    {
        Append(0, name, method);
    }
    public void Append(Action<T, T2, T3, T4, T5> method)
    {
        Append(0, method);
    }
    public void Invoke(T a, T2 b, T3 c, T4 d, T5 e)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(a, b, c, d, e);
        }
    }
}

