
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentLoader
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitComps()
    {
        var g = RandomFunctions.GetListOfInheritors<OXComponentBase>();
        foreach (var comp in g)
        {
            comp.Init();
        }
    }
}

public static class ComponentData
{
    public static Dictionary<string, Func<string, OXComponentBase>> ComponentTransformers = new Dictionary<string, Func<string, OXComponentBase>>();
    public static Dictionary<string, string> ClassToIdentifier = new Dictionary<string, string>();
}


/// <summary>
/// Components have to be initialized in order to register their FromString functions.
/// </summary>
public abstract class OXComponentBase
{
    /// <summary>
    /// Unique identifier for this component type. No two components should share the same identifier.
    /// </summary>
    public abstract string GetUniqueIdentifier();
    /// <summary>
    /// Converts the current component instance into its string representation for serialization.
    /// </summary>
    public abstract string GetString();

    /// <summary>
    /// Creates a new instance by parsing the specified string representation.
    /// IT DOES NOT set any values on the current instance!
    /// </summary>
    public abstract OXComponentBase FromString(string data);

    /// <summary>
    /// Determines if this component is equal to another component.
    /// </summary>
    public bool Compare(OXComponentBase data)
    {
        if (GetUniqueIdentifier() != data.GetUniqueIdentifier()) return false;
        return Compare2(data);
    }
    public abstract bool Compare2(OXComponentBase data);
    public abstract void Init();
}
/// <summary>
/// Components have to be initialized in order to register their FromString functions.
/// </summary>
public abstract class OXComponent<T> : OXComponentBase where T : OXComponent<T>
{
    public override void Init()
    {
        ComponentData.ComponentTransformers.Add(GetUniqueIdentifier(), FromString);
        ComponentData.ClassToIdentifier.Add(typeof(T).Name, GetUniqueIdentifier());
    }
    /// <summary>
    /// Determines if this component is equal to another component.
    /// </summary>
    public override bool Compare2(OXComponentBase data)
    {
        return EqualsSpecific((T)data);
    }
    /// <summary>
    /// Determines if this component is specifically equal to another component of the same type.
    /// </summary>
    public abstract bool EqualsSpecific(T data);
}
