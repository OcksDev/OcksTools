using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public class GlobalAttributeCollector
{
    [RuntimeInitializeOnLoadMethod]
    public static void GatherMethods()
    {
        Assembly[] assemblies = new Assembly[1];

        assemblies[0] = Assembly.GetExecutingAssembly();

        foreach (var ass in assemblies)
        {
            if (ass == null) continue;
            var methods = ass
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .ToList();

            // ---- Add new attributes here. ----

            Register<ConversionMethod>(methods, (method, attribute, instance) =>
            {
                Converter.ConversionMethods.Add(
                    method.ReflectedType.Name,
                    (x) => { return method.Invoke(instance, new object[] { x }); });
            });

            Register<AddToEvent>(methods, (method, attribute, instance) =>
            {
                GlobalEvent.Append(
                    attribute.dingle,
                    () => { method.Invoke(instance, new object[] { }); });
            });
        }
    }









    public delegate A Convertion<A>(string message);

    // One shared instance per declaring type, so we don't create a new one per method.
    private static readonly Dictionary<Type, object> instanceCache = new Dictionary<Type, object>();

    /// <summary>
    /// Gets an instance to invoke the method on, without running any constructors
    /// for Unity types (so no "new keyword" warning and no Awake/OnEnable).
    /// </summary>
    private static object GetInstance(MethodInfo method)
    {
        if (method.IsStatic) return null;

        Type type = method.ReflectedType;
        if (instanceCache.TryGetValue(type, out object cached)) return cached;

        object instance;
        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            // Skips the constructor entirely: no warning, no Awake, no GameObject.
            instance = FormatterServices.GetUninitializedObject(type);
        }
        else
        {
            instance = Activator.CreateInstance(type);
        }

        instanceCache[type] = instance;
        return instance;
    }

    /// <summary>
    /// Finds every method marked with attribute T and hands it to <paramref name="register"/>.
    /// Instance creation and try/catch are handled here, so callers only write the interesting part.
    /// </summary>
    /// <param name="methods">All candidate methods to scan.</param>
    /// <param name="register">Called as (method, attribute, instance). Instance is null for static methods.</param>
    private static void Register<T>(IEnumerable<MethodInfo> methods, Action<MethodInfo, T, object> register)
        where T : Attribute
    {
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<T>();
            if (attribute == null) continue;

            try
            {
                register(method, attribute, GetInstance(method));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }

}

public static class Freakybob
{
    public static List<MRef<string, MethodInfo>> ToMRef(this IEnumerable<MethodInfo> banana, Func<MethodInfo, string> nutt)
    {
        var dd = new List<MRef<string, MethodInfo>>();
        foreach (var z in banana)
        {
            dd.Add(new MRef<string, MethodInfo>(z.ReflectedType.Name, z));
        }
        return dd;
    }
}
