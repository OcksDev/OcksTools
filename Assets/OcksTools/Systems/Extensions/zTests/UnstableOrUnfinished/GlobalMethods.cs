using System;
using System.Collections.Generic;


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
public class GlobalMethod<T, T2>
{
    private static Dictionary<string, System.Action<T, T2>> Nerds = new Dictionary<string, System.Action<T, T2>>();
    public static void Set(string even, Action<T, T2> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a, T2 b)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a, b);
    }
}
public class GlobalMethod<T, T2, T3>
{
    private static Dictionary<string, System.Action<T, T2, T3>> Nerds = new Dictionary<string, System.Action<T, T2, T3>>();
    public static void Set(string even, Action<T, T2, T3> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a, T2 b, T3 c)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a, b, c);
    }
}
public class GlobalMethod<T, T2, T3, T4>
{
    private static Dictionary<string, System.Action<T, T2, T3, T4>> Nerds = new Dictionary<string, System.Action<T, T2, T3, T4>>();
    public static void Set(string even, Action<T, T2, T3, T4> method)
    {
        Nerds.Add(even, method);
    }
    public static void Run(string even, T a, T2 b, T3 c, T4 d)
    {
        if (Nerds.ContainsKey(even)) Nerds[even](a, b, c, d);
    }
}

