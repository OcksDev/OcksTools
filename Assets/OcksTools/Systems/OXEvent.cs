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
    public static bool Contains(string name)
    {
        return Nerds.ContainsKey(name);
    }
}

public abstract class _OXEventBase<T, T2>
{
    public Dictionary<string, T2> StoredMethods = new Dictionary<string, T2>();
    protected List<string> killme = new List<string>();
    public void Append(string name, T2 method) => StoredMethods.AddOrUpdate(name, method);
    public void Append(T2 method)
    {
        var dd = Tags.GenerateID();
        StoredMethods.AddOrUpdate(dd, method);
    }
    public void Append(string name, T method) => Append(name, Convert(method));
    public void Append(T method) => Append(Convert(method));
    public void Remove(string name) => StoredMethods.Remove(name);

    public bool Contains(string name) => StoredMethods.ContainsKey(name);
    public void Clear() => StoredMethods.Clear();
    public abstract T2 Convert(T input);
}

public class OXEvent : _OXEventBase<Action, Func<bool>>
{
    public void Invoke()
    {
        killme.Clear();
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
    public override Func<bool> Convert(Action input) => () => { input(); return false; };
    public bool InvokeWithHitCheck(bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value();
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
    }
}
public class OXEvent<T> : _OXEventBase<Action<T>, Func<T, bool>>
{
    public static bool SuccessfulHit = false;
    public void Invoke(T t)
    {
        killme.Clear();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(t);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public override Func<T, bool> Convert(Action<T> input) => (x) => { input(x); return false; };
    public bool InvokeWithHitCheck(T t, bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value(t);
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
    }
}
public class OXEvent<T, T2> : _OXEventBase<Action<T, T2>, Func<T, T2, bool>>
{
    public static bool SuccessfulHit = false;
    public void Invoke(T t, T2 t2)
    {
        killme.Clear();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(t, t2);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public override Func<T, T2, bool> Convert(Action<T, T2> input) => (x, y) => { input(x, y); return false; };
    public bool InvokeWithHitCheck(T t, T2 t2, bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value(t, t2);
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
    }
}
public class OXEvent<T, T2, T3> : _OXEventBase<Action<T, T2, T3>, Func<T, T2, T3, bool>>
{
    public static bool SuccessfulHit = false;
    public void Invoke(T t, T2 t2, T3 t3)
    {
        killme.Clear();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(t, t2, t3);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public override Func<T, T2, T3, bool> Convert(Action<T, T2, T3> input) => (x, y, z) => { input(x, y, z); return false; };
    public bool InvokeWithHitCheck(T t, T2 t2, T3 t3, bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value(t, t2, t3);
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
    }
}
public class OXEvent<T, T2, T3, T4> : _OXEventBase<Action<T, T2, T3, T4>, Func<T, T2, T3, T4, bool>>
{
    public static bool SuccessfulHit = false;
    public void Invoke(T t, T2 t2, T3 t3, T4 t4)
    {
        killme.Clear();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(t, t2, t3, t4);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public override Func<T, T2, T3, T4, bool> Convert(Action<T, T2, T3, T4> input) => (x, y, z, w) => { input(x, y, z, w); return false; };

    public bool InvokeWithHitCheck(T t, T2 t2, T3 t3, T4 t4, bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value(t, t2, t3, t4);
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
    }
}
public class OXEvent<T, T2, T3, T4, T5> : _OXEventBase<Action<T, T2, T3, T4, T5>, Func<T, T2, T3, T4, T5, bool>>
{
    public static bool SuccessfulHit = false;
    public void Invoke(T t, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        killme.Clear();
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) w.Value(t, t2, t3, t4, t5);
            else killme.Add(w.Key);
        }
        foreach (var kill in killme)
        {
            StoredMethods.Remove(kill);
        }
    }
    public override Func<T, T2, T3, T4, T5, bool> Convert(Action<T, T2, T3, T4, T5> input) => (x, y, z, w, v) => { input(x, y, z, w, v); return true; };
    public bool InvokeWithHitCheck(T t, T2 t2, T3 t3, T4 t4, T5 t5, bool CanEarlyExit = false)
    {
        killme.Clear();
        bool hit = false;
        foreach (var w in StoredMethods)
        {
            if (w.Value != null) hit |= w.Value(t, t2, t3, t4, t5);
            else killme.Add(w.Key);
            if (CanEarlyExit && hit) return true;
        }
        foreach (var kill in killme)
            StoredMethods.Remove(kill);
        return hit;
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

public class _OXEventLayeredBase<T, T2, T3> where T : _OXEventBase<T2, T3>, new()
{
    public List<MultiRef<int, T>> StoredEvents = new List<MultiRef<int, T>>();

    // Finds the event bucket for a given layer, creating and inserting it
    // in sorted order if it doesn't exist yet.
    private T GetOrCreateLayer(int layer)
    {
        for (int i = 0; i < StoredEvents.Count; i++)
        {
            var w = StoredEvents[i];
            if (w.a == layer) return w.b;
            if (w.a > layer)
            {
                var x = new T();
                StoredEvents.Insert(i, new MultiRef<int, T>(layer, x));
                return x;
            }
        }
        var x2 = new T();
        StoredEvents.Add(new MultiRef<int, T>(layer, x2));
        return x2;
    }

    // --- Raw type (T3) overloads ---
    public void Append(int layer, string name, T3 method) => GetOrCreateLayer(layer).Append(name, method);
    public void Append(int layer, T3 method) => Append(layer, Tags.GenerateID(), method);
    public void Append(string name, T3 method) => Append(0, name, method);
    public void Append(T3 method) => Append(0, method);

    // --- Friendly type (T2) overloads, converted via the event's Convert() ---
    public void Append(int layer, string name, T2 method)
    {
        var ev = GetOrCreateLayer(layer);
        ev.Append(name, ev.Convert(method));
    }
    public void Append(int layer, T2 method) => Append(layer, Tags.GenerateID(), method);
    public void Append(string name, T2 method) => Append(0, name, method);
    public void Append(T2 method) => Append(0, method);

    public void Clear() => StoredEvents.Clear();
}

public class OXEventLayered : _OXEventLayeredBase<OXEvent, Action, Func<bool>>
{
    public void Invoke()
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke();
        }
    }
}

public class OXEventLayered<T> : _OXEventLayeredBase<OXEvent<T>, Action<T>, Func<T, bool>>
{
    public void Invoke(T t)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(t);
        }
    }
}

public class OXEventLayered<T, T2> : _OXEventLayeredBase<OXEvent<T, T2>, Action<T, T2>, Func<T, T2, bool>>
{
    public void Invoke(T t, T2 t2)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(t, t2);
        }
    }
}

public class OXEventLayered<T, T2, T3> : _OXEventLayeredBase<OXEvent<T, T2, T3>, Action<T, T2, T3>, Func<T, T2, T3, bool>>
{
    public void Invoke(T t, T2 t2, T3 t3)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(t, t2, t3);
        }
    }
}

public class OXEventLayered<T, T2, T3, T4> : _OXEventLayeredBase<OXEvent<T, T2, T3, T4>, Action<T, T2, T3, T4>, Func<T, T2, T3, T4, bool>>
{
    public void Invoke(T t, T2 t2, T3 t3, T4 t4)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(t, t2, t3, t4);
        }
    }
}

public class OXEventLayered<T, T2, T3, T4, T5> : _OXEventLayeredBase<OXEvent<T, T2, T3, T4, T5>, Action<T, T2, T3, T4, T5>, Func<T, T2, T3, T4, T5, bool>>
{
    public void Invoke(T t, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        foreach (var aa in StoredEvents)
        {
            aa.b.Invoke(t, t2, t3, t4, t5);
        }
    }
}
