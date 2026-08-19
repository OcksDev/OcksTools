

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
    public DynamicString AddRef(params object[] y)
    {
        foreach (var a in y)
        {
            References.Add(a);
        }
        return this;
    }
    public override string ToString()
    {
        return CompiledValue;
    }
    public static implicit operator string(DynamicString r) => r.Get();
}
