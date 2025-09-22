using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ConversionAgrigator : MonoBehaviour
{
    // Start is called before the first frame update
    [ConversionMethod]
    [RuntimeInitializeOnLoadMethod]
    public static void GatherMethods()
    {
        Assembly[] assemblies = new Assembly[1];

        assemblies[0] = Assembly.GetExecutingAssembly();


        foreach (var ass in assemblies)
        {
            // object instance = Activator.CreateInstance(t);
            var methods = ass
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(y => y.GetCustomAttributes().OfType<ConversionMethod>().Any())
                .ToDictionary(z => z.ReflectedType.Name);
            if (methods.Count > 0)
            {
                Console.Log(methods.ABDictionaryToStringDictionary().DictionaryToRead());
                foreach (var a in methods)
                {
                    ConversionMethods.Add(a.Key, (x) =>
                    {
                        var dd = Activator.CreateInstance(a.Value.ReflectedType);
                        return methods[a.Key].Invoke(dd, new object[] { x });
                    });
                }
            }
        }
        


    }

    public static Dictionary<string, Func<string, object>> ConversionMethods = new Dictionary<string, Func<string, object>>();

    public static object ConvertString<A>(string data)
    {
        string typename = typeof(A).Name;
        if (!ConversionMethods.ContainsKey(typename))
        {
            return null;
        }
        return ConversionMethods[typename](data);
    }

}
