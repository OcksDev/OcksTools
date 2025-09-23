using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ConversionAgrigator;

public class ConversionAgrigator : MonoBehaviour
{
    // Start is called before the first frame update

    public delegate A Convertion<A>(string message);

    [ConversionMethod]
    [RuntimeInitializeOnLoadMethod]
    public static void GatherMethods()
    {
        Assembly[] assemblies = new Assembly[4];

        assemblies[0] = Assembly.GetExecutingAssembly();
        assemblies[1] = Assembly.Load("OcksTools");
        assemblies[2] = Assembly.Load("OcksTools.Multiplayer");
        assemblies[3] = Assembly.Load("OcksTools.NavMesh");


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
                    var dd = Activator.CreateInstance(a.Value.ReflectedType);
                    Converter.ConversionMethods.Add(a.Key, (x) => { return a.Value.Invoke(dd, new object[] { x }); });

                }
            }
        }
        


    }

}
