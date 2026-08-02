using System;
using System.Collections.Generic;

public class OXFactory
{
    private static Dictionary<Type, _OXFactory> AllFactories = new();
    public static T Get<T>(string name) where T : class
    {
        return (T)AllFactories[typeof(T)].Create(name);
    }
    public static void Append<T, T2>(string name) where T : class where T2 : T, new()
    {
        if (!AllFactories.TryGetValue(typeof(T), out var factory))
        {
            factory = new _CoolOXFactory<T>();
            AllFactories.Add(typeof(T), factory);
        }
        ((_CoolOXFactory<T>)factory).Append<T2>(name);
    }
}
public abstract class _OXFactory
{
    public abstract object Create(string name);
}

public class _CoolOXFactory<T> : _OXFactory where T : class
{
    public Dictionary<string, _Maker<T>> Makers = new();
    public override object Create(string name) => Makers[name].Create();
    public void Append<T2>(string name) where T2 : T, new()
    {
        Makers.AddOrUpdate(name, new _Maker<T, T2>());
    }
}
public interface _Maker<T>
{
    T Create();
}
public struct _Maker<T, T2> : _Maker<T>
    where T : class
    where T2 : T, new()
{
    public T Create() => new T2();
}