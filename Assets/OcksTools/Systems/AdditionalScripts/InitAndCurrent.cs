public class ResettableValue<T>
{
    public T Initial;
    private T Current;

    public void Set(T a) { Current = a; }
    public T Get() { return Current; }
    public void SetToInitial() { Current = Initial; }
    public static implicit operator T(ResettableValue<T> a) { return a.Get(); }
}
