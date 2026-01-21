public class Pointer<T>
{
    public T Value;
    public Pointer(T value)
    {
        Value = value;
    }
    public T Get()
    {
        return Value;
    }
    public void Set(T t)
    {
        Value = t;
    }
    public override string ToString()
    {
        return Value.ToString();
    }
    public override bool Equals(object obj)
    {
        return Value.Equals(obj);
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator T(Pointer<T> r) => r.Value;

}
