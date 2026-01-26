
using System;
using System.Collections.Generic;
using UnityEngine;

public static class OXComponentLoader
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

public static class OXComponentData
{
    public static Dictionary<string, Func<string, OXComponentBase>> ComponentTransformers = new Dictionary<string, Func<string, OXComponentBase>>();
    public static Dictionary<string, string> ClassToIdentifier = new Dictionary<string, string>();

    public static OXComponentBase ComponentConvert(string wish, string data)
    {
        if (ComponentTransformers.ContainsKey(wish))
        {
            return ComponentTransformers[wish](data);
        }
        throw new Exception($"No item data conversion defined for {wish}");
    }
}

public class ComponentHolder
{
    public void CompFromString(string a)
    {
        Components = a.EscapedStringToDictionary().ABDictionaryToCDDictionary((x, y) => x, (x, y) => OXComponentData.ComponentConvert(x, y));
    }
    public string CompToString()
    {
        return Components.ABDictionaryToCDDictionary((x) => x, (x) => x.GetString()).EscapedDictionaryToString();
    }


    public Dictionary<string, OXComponentBase> Components = new Dictionary<string, OXComponentBase>();
    public void AddComponent(OXComponentBase cum)
    {
        Components.Add(cum.GetUniqueIdentifier(), cum);
    }
    public T GetComponent<T>() where T : OXComponentBase
    {
        var s = OXComponentData.ClassToIdentifier[typeof(T).Name];
        if (!Components.ContainsKey(s)) return null;
        return (T)Components[s];
    }
    public bool HasComponent<T>() where T : OXComponentBase
    {
        var s = OXComponentData.ClassToIdentifier[typeof(T).Name];
        return Components.ContainsKey(s);
    }
    public bool HasComponent(string s)
    {
        return Components.ContainsKey(s);
    }
    public bool Compare(ComponentHolder sexnut)
    {
        bool comp = true;
        if (sexnut.Components.Count != Components.Count) comp = false;
        else
        {
            foreach (var c in Components)
            {
                if (!sexnut.Components.ContainsKey(c.Key))
                {
                    comp = false;
                    break;
                }
                else
                {
                    if (!c.Value.Compare(sexnut.Components[c.Key]))
                    {
                        comp = false;
                        break;
                    }
                }
            }
        }
        return comp;
    }

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
        OXComponentData.ComponentTransformers.Add(GetUniqueIdentifier(), FromString);
        OXComponentData.ClassToIdentifier.Add(typeof(T).Name, GetUniqueIdentifier());
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
