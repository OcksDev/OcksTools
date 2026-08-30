using UnityEditor;


[System.Serializable]
public struct MRef<A, B>
{
    public A a;
    public B b;
    public MRef(A a, B b)
    {
        this.a = a;
        this.b = b;
    }
    public static implicit operator MRef<A, B>((A, B) l) => new MRef<A, B>(l.Item1, l.Item2);
    public static implicit operator (A, B)(MRef<A, B> l) => (l.a, l.b);
}

[System.Serializable]
public struct MRef<A, B, C>
{
    public A a;
    public B b;
    public C c;
    public MRef(A a, B b, C c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    public static implicit operator MRef<A, B, C>((A, B, C) l) => new MRef<A, B, C>(l.Item1, l.Item2, l.Item3);
    public static implicit operator (A, B, C)(MRef<A, B, C> l) => (l.a, l.b, l.c);
}

[System.Serializable]
public struct MRef<A, B, C, D>
{
    public A a;
    public B b;
    public C c;
    public D d;
    public MRef(A a, B b, C c, D d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
    public static implicit operator MRef<A, B, C, D>((A, B, C, D) l) => new MRef<A, B, C, D>(l.Item1, l.Item2, l.Item3, l.Item4);
    public static implicit operator (A, B, C, D)(MRef<A, B, C, D> l) => (l.a, l.b, l.c, l.d);
}


[System.Serializable]
public class MRefClass<A, B>
{
    public A a;
    public B b;
    public MRefClass(A a, B b)
    {
        this.a = a;
        this.b = b;
    }
    public static implicit operator MRefClass<A, B>((A, B) l) => new MRefClass<A, B>(l.Item1, l.Item2);
    public static implicit operator (A, B)(MRefClass<A, B> l) => (l.a, l.b);
}

[System.Serializable]
public class MRefClass<A, B, C>
{
    public A a;
    public B b;
    public C c;
    public MRefClass(A a, B b, C c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    public static implicit operator MRefClass<A, B, C>((A, B, C) l) => new MRefClass<A, B, C>(l.Item1, l.Item2, l.Item3);
    public static implicit operator (A, B, C)(MRefClass<A, B, C> l) => (l.a, l.b, l.c);
}

[System.Serializable]
public class MRefClass<A, B, C, D>
{
    public A a;
    public B b;
    public C c;
    public D d;
    public MRefClass(A a, B b, C c, D d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
    public static implicit operator MRefClass<A, B, C, D>((A, B, C, D) l) => new MRefClass<A, B, C, D>(l.Item1, l.Item2, l.Item3, l.Item4);
    public static implicit operator (A, B, C, D)(MRefClass<A, B, C, D> l) => (l.a, l.b, l.c, l.d);
}


[System.Serializable]
public struct MRefNoName<A, B>
{
    public A a;
    public B b;
    public MRefNoName(A a, B b)
    {
        this.a = a;
        this.b = b;
    }
    public static implicit operator MRefNoName<A, B>((A, B) l) => new MRefNoName<A, B>(l.Item1, l.Item2);
    public static implicit operator (A, B)(MRefNoName<A, B> l) => (l.a, l.b);
}

[System.Serializable]
public struct MRefNoName<A, B, C>
{
    public A a;
    public B b;
    public C c;
    public MRefNoName(A a, B b, C c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    public static implicit operator MRefNoName<A, B, C>((A, B, C) l) => new MRefNoName<A, B, C>(l.Item1, l.Item2, l.Item3);
    public static implicit operator (A, B, C)(MRefNoName<A, B, C> l) => (l.a, l.b, l.c);
}

[System.Serializable]
public struct MRefNoName<A, B, C, D>
{
    public A a;
    public B b;
    public C c;
    public D d;
    public MRefNoName(A a, B b, C c, D d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
    public static implicit operator MRefNoName<A, B, C, D>((A, B, C, D) l) => new MRefNoName<A, B, C, D>(l.Item1, l.Item2, l.Item3, l.Item4);
    public static implicit operator (A, B, C, D)(MRefNoName<A, B, C, D> l) => (l.a, l.b, l.c, l.d);
}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MRefNoName<,>))]
public class FuckassMultiRefComDrawer : AutoCompressedSideBySideInspector
{
}

[CustomPropertyDrawer(typeof(MRefNoName<,,>))]
public class FuckassMultiRef2ComDrawer : AutoCompressedSideBySideInspector
{
}

[CustomPropertyDrawer(typeof(MRefNoName<,,,>))]
public class FuckassMultiRef3ComDrawer : AutoCompressedSideBySideInspector
{
}
[CustomPropertyDrawer(typeof(MRef<,>))]
public class FuckassMultiRefAComDrawer : AutoCompressedSideBySideInspectorWithName
{
}

[CustomPropertyDrawer(typeof(MRef<,,>))]
public class FuckassMultiRefA2ComDrawer : AutoCompressedSideBySideInspectorWithName
{
}

[CustomPropertyDrawer(typeof(MRef<,,,>))]
public class FuckassMultiRefA3ComDrawer : AutoCompressedSideBySideInspectorWithName
{
}
[CustomPropertyDrawer(typeof(MRefClass<,>))]
public class FuckassMultiRefAAComDrawer : AutoCompressedSideBySideInspectorWithName
{
}

[CustomPropertyDrawer(typeof(MRefClass<,,>))]
public class FuckassMultiRefAA2ComDrawer : AutoCompressedSideBySideInspectorWithName
{
}

[CustomPropertyDrawer(typeof(MRefClass<,,,>))]
public class FuckassMultiRefAA3ComDrawer : AutoCompressedSideBySideInspectorWithName
{
}
#endif