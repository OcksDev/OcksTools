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
       // object instance = Activator.CreateInstance(t);
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(x => x.GetMethods())
            .Where(y => y.GetCustomAttributes().OfType<ConversionMethod>().Any())
            .ToDictionary(z => z.ReflectedType.Name);
        Console.Log(methods.ABDictionaryToStringDictionary().DictionaryToRead());
        foreach(var a in methods)
        {
            ConversionMethods.Add(a.Key, (x) => 
            {
                var dd = Activator.CreateInstance(a.Value.ReflectedType);
                return methods[a.Key].Invoke(dd, new object[] { x }); 
            });
        }
        


    }

    public static Dictionary<string, Func<string, object>> ConversionMethods = new Dictionary<string, Func<string, object>>();

    public static object ConvertStringUsingMethod(string data, string methodname)
    {
        if (!ConversionMethods.ContainsKey(methodname))
        {
            return null;
        }
        return ConversionMethods[methodname](data);
    }

}
