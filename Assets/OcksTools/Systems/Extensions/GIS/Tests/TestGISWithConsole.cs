using System;
using UnityEngine;

public class TestGISWithConsole : MonoBehaviour
{
    private void Start()
    {
        GlobalEvent.Append("Console", GISTestCommands);
        Debug.Log("Ranstart");
    }

    public void GISTestCommands()
    {
        ConsoleLol.Instance.Add(new OXCommand("gistest").Action(ItemConversion));
        Debug.Log("ADDED");
    }

    public void ItemConversion()
    {
        var item= new GISItem("Tester");
        item.AddComponent(new ItemExampleComponent().FromString("Hellonana"));
        string stored = item.ToString();
        Console.Log(stored);
        Console.Log("----");
        try
        {
            var g = new GISItem().StringToItem(stored);
            var pp = g.GetComponent<ItemExampleComponent>("Example");
            Console.Log(pp.StoredData);
        }
        catch(Exception e)
        {
            Console.LogError(e);
        }
    }
}
