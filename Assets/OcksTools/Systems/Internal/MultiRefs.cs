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