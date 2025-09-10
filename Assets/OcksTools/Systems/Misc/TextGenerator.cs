using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OXGenerator
{
    public virtual string Pull(GenProfile pp)
    {
        return "";
    }
}

public class OXGenerator_Username : OXGenerator
{
    public override string Pull(GenProfile pp)
    {
        return "";
    }
}

public class GenProfile
{
    public int? Min;
    public int? Max;
    public GenProfile MinLength(int m)
    {
        Min = m; return this;
    }
    public GenProfile MaxLength(int m)
    {
        Max = m; return this;
    }
} 

