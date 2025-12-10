using UnityEngine;

public class ExampleComponents : MonoBehaviour
{
    private void Start()
    {
        new GISExampleComponent().Init();
        new GISExampleComponentAlt().Init();
    }
}

public class GISExampleComponent : GISItemComponent<GISExampleComponent>
{
    public int examplevalue;
    public override string GetIdentifier()
    {
        return "Example";
    }
    public override GISItemComponentBase FromString(string data)
    {
        var a = new GISExampleComponent();
        a.examplevalue = int.Parse(data);
        return a;
    }

    public override string GetString()
    {
        return examplevalue.ToString();
    }
    public override string ToString()
    {
        return $"Example Value: [{examplevalue}]";
    }

    public override bool EqualsSpecific(GISExampleComponent data)
    {
        return examplevalue == data.examplevalue;
    }
}

public class GISExampleComponentAlt : GISItemComponent<GISExampleComponentAlt>
{
    public string examplevalue;
    public override string GetIdentifier()
    {
        return "ExampleAlt";
    }
    public override GISItemComponentBase FromString(string data)
    {
        var a = new GISExampleComponentAlt();
        a.examplevalue = data;
        return a;
    }

    public override string GetString()
    {
        return examplevalue;
    }
    public override string ToString()
    {
        return $"Example Alt Value: [{examplevalue}]";
    }

    public override bool EqualsSpecific(GISExampleComponentAlt data)
    {
        return examplevalue == data.examplevalue;
    }
}