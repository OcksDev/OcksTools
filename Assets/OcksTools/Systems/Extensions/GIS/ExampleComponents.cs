public class GISExampleComponent : OXComponent<GISExampleComponent>
{
    public int examplevalue;
    public override string GetUniqueIdentifier()
    {
        return "Example";
    }
    public override OXComponentBase FromString(string data)
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

public class GISExampleComponentAlt : OXComponent<GISExampleComponentAlt>
{
    public string examplevalue;
    public override string GetUniqueIdentifier()
    {
        return "ExampleAlt";
    }
    public override OXComponentBase FromString(string data)
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