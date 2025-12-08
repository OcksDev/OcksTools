using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GlobalAttributeCollector : MonoBehaviour
{
    // Start is called before the first frame update

    public delegate A Convertion<A>(string message);

    [ConversionMethod]
    [RuntimeInitializeOnLoadMethod]
    public static void GatherMethods()
    {
        Assembly[] assemblies = new Assembly[1];

        assemblies[0] = Assembly.GetExecutingAssembly();


        foreach (var ass in assemblies)
        {
            if(ass == null) continue;
            // object instance = Activator.CreateInstance(t);
            var methods = ass
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(y => y.GetCustomAttributes().OfType<ConversionMethod>().Any())
                .ToMultiref(z => z.ReflectedType.Name);
            if (methods.Count > 0)
            {
                foreach (var a in methods)
                {
                    try
                    {

                        var dd = Activator.CreateInstance(a.b.ReflectedType);
                        Converter.ConversionMethods.Add(a.a, (x) => { return a.b.Invoke(dd, new object[] { x }); });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }

                }
            }

            methods = ass
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(y => y.GetCustomAttributes().OfType<AddToEvent>().Any())
                .ToMultiref(z => z.ReflectedType.Name);
            if (methods.Count > 0)
            {
                foreach (var a in methods)
                {
                    var dd = Activator.CreateInstance(a.b.ReflectedType);
                    var sex = a.b.GetCustomAttribute<AddToEvent>();
                    GlobalEvent.Append(sex.dingle, () => { a.b.Invoke(dd, new object[] { }); });

                }
            }
        }
        


    }

}
public static class Freakybob
{
    public static List<MultiRef<string,MethodInfo>> ToMultiref(this IEnumerable<MethodInfo> banana, Func<MethodInfo,string> nutt)
    {
        var dd = new List<MultiRef<string,MethodInfo>>();
        foreach(var z in banana)
        {
            dd.Add(new MultiRef<string, MethodInfo>(z.ReflectedType.Name, z));
        }
        return dd;
    }
}