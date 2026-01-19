

using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DynamicString
{
    private string Value;
    private string CompiledValue;
    public List<object> References = new List<object>();
    public DynamicString(string initialValue)
    {
        SetBase(initialValue);
        CompiledValue = "(Uncompiled)";
    }
    public void SetBase(string newValue)
    {
        Value = newValue;
    }
    public DynamicString Compile()
    {
        CompiledValue = Value;
        int x = 0;
        foreach (var a in References)
        {
            CompiledValue = Regex.Replace(CompiledValue, $"<{x}>", a.ToString());
            x++;
        }
        return this;
    }
    public string Get()
    {
        return CompiledValue;
    }
    public DynamicString SetAll(List<object> b)
    {
        References = b;
        return this;
    }
    public DynamicString SetSpecificRef(int x, object y)
    {
        References.SetMinCount(x + 1);
        References[x] = y;
        return this;
    }
    public DynamicString AddRef(object y)
    {
        References.Add(y);
        return this;
    }
    public DynamicString AddRef(object y, object y2)
    {
        References.Add(y);
        References.Add(y2);
        return this;
    }
    public DynamicString AddRef(object y, object y2, object y3)
    {
        References.Add(y);
        References.Add(y2);
        References.Add(y3);
        return this;
    }
    public DynamicString AddRef(object y, object y2, object y3, object y4)
    {
        References.Add(y);
        References.Add(y2);
        References.Add(y3);
        References.Add(y4);
        return this;
    }
    public override string ToString()
    {
        return CompiledValue;
    }
    public static implicit operator string(DynamicString r) => r.Get();
}
