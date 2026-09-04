using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class OXFactory
{
    private static Dictionary<Type, _OXFactory> AllFactories = new();

    public static T Create<T>(string name) where T : class => (T)_GetFactory<T>().Create(name);
    public static _CoolOXFactory<T> GetFactory<T>() where T : class => (_CoolOXFactory<T>)_GetFactory<T>();

    public static void Define<T, T2>(string name) where T : class where T2 : T, new()
    {
        _GetOrCreateFactory<T>().Define<T2>(name);
    }
    public static void Define<T, T2>(params string[] names) where T : class where T2 : T, new()
    {
        _GetOrCreateFactory<T>().Define<T2>(names);
    }
    public static void DefineForInheritorsOf<T>(Func<T, string> getName) where T : class
    {
        var factory = _GetOrCreateFactory<T>();

        // Cache the single-string overload's MethodInfo once (not per-type).
        var defineMethod = typeof(_CoolOXFactory<T>)
            .GetMethod(nameof(_CoolOXFactory<T>.Define), new[] { typeof(string) });

        var candidateTypes = Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                        && typeof(T).IsAssignableFrom(t)
                        && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var type in candidateTypes)
        {
            T instance = (T)Activator.CreateInstance(type);
            string name = getName(instance);

            defineMethod.MakeGenericMethod(type).Invoke(factory, new object[] { name });
        }
    }

    private static _CoolOXFactory<T> _GetOrCreateFactory<T>() where T : class
    {
        if (!AllFactories.TryGetValue(typeof(T), out var factory))
        {
            factory = new _CoolOXFactory<T>();
            AllFactories.Add(typeof(T), factory);
        }
        return (_CoolOXFactory<T>)factory;
    }

    private static _OXFactory _GetFactory<T>() where T : class
    {
        if (!AllFactories.TryGetValue(typeof(T), out var factory))
            throw new KeyNotFoundException($"No factory defined for '{typeof(T)}'");
        return factory;
    }
}
public abstract class _OXFactory
{
    public abstract object Create(string name);
}
public class _CoolOXFactory<T> : _OXFactory where T : class
{
    public Dictionary<string, _Maker<T>> Makers = new();
    public override object Create(string name)
    {
        if (!Makers.TryGetValue(name, out var maker))
            throw new KeyNotFoundException($"Nothing defined for \"{name}\" on factory '{typeof(T)}'");

        return maker.Create();
    }
    public void Define<T2>(string name) where T2 : T, new()
    {
        Makers[name] = _CoolFuckinSingleMakerPerTypeShit<T2>.Instance;
    }
    public void Define<T2>(params string[] names) where T2 : T, new()
    {
        var maker = _CoolFuckinSingleMakerPerTypeShit<T2>.Instance;
        foreach (var n in names)
            Makers[n] = maker;
    }

    // one boxed _Maker<T,T2> per T2
    private static class _CoolFuckinSingleMakerPerTypeShit<T2> where T2 : T, new()
    {
        public static readonly _Maker<T> Instance = new _Maker<T, T2>();
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