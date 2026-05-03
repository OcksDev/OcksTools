using System;
using System.Collections.Generic;
using WebSocketSharp;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class CompileableDictionary<A, B> : Dictionary<A, B>
{
    public List<MultiRef<A, B>> List = new List<MultiRef<A, B>>();
    public void Compile()
    {
        Clear();
        foreach (var a in List)
        {
            Add(a.a, a.b);
        }
    }
}

[System.Serializable]
public class CompileableDictionaryAlt<A, B> : Dictionary<A, B>
{
    public List<B> List = new List<B>();
    public void Compile(Func<B, A> funky)
    {
        Clear();
        foreach (var a in List)
        {
            Add(funky(a), a);
        }
    }
    public Func<B, string> Auto = null;
}

[System.Serializable]
public class CompileableDictionary<B> : Dictionary<string, B>
{
    public List<MultiRef<string, B>> List = new List<MultiRef<string, B>>();
    public void Compile()
    {
        Clear();
        foreach (var a in List)
        {
            string s = a.a;
            if (Auto != null && s.IsNullOrEmpty()) s = Auto(a.b);
            Add(s, a.b);
        }
    }
    public Func<B, string> Auto = null;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CompileableDictionary<,>), true)]
public class CompileableDictionaryDrawer : AutoCompressedInspector
{
}

[CustomPropertyDrawer(typeof(CompileableDictionaryAlt<,>), true)]
public class CompileableDictionaryDrawer2 : AutoCompressedInspector
{
}

[CustomPropertyDrawer(typeof(CompileableDictionary<>), true)]
public class CompileableDictionaryDrawer3 : AutoCompressedInspector
{
}

#endif