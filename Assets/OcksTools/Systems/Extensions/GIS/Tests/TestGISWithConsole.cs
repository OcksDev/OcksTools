using System;
using UnityEngine;

public class TestGISWithConsole : MonoBehaviour
{
    private void Start()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("gistest").Action(ItemConversion));
        });
    }


    public void ItemConversion()
    {
        try
        {
            var item = new GISItem("Tester");
            var ding = new GISExampleComponent().FromString("69");
            Console.Log(ding.ToString());
            Console.Log("----");
            item.Components.AddComponent(ding);
            string stored = item.ToString();
            Console.Log(stored);
            Console.Log("----");
            var g = new GISItem().StringToItem(stored);
            var pp = g.Components.GetComponent<GISExampleComponent>();
            Console.Log(pp.ToString());
            Console.Log("----");
            var a = new GISExampleComponent().FromString("69");
            var b = new GISExampleComponent().FromString("420");
            var c = new GISExampleComponentAlt().FromString("Hello");
            var d = new GISExampleComponentAlt().FromString("Hello");
            Console.Log($"a: {a.ToString()}");
            Console.Log($"b: {b.ToString()}");
            Console.Log($"c: {c.ToString()}");
            Console.Log($"d: {d.ToString()}");
            Console.Log($"a equals a: {a.Compare(a)}");
            Console.Log($"a equals b: {a.Compare(b)}");
            Console.Log($"a equals c: {a.Compare(c)}");
            Console.Log($"c equals d: {c.Compare(d)}");

        }
        catch (Exception e)
        {
            Console.LogError(e);
        }
    }
}
