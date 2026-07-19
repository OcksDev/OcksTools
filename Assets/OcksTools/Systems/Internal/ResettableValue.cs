[System.Serializable]
public class ResettableValue<T>
{
    public T Initial;
    public T Current;
    public ResettableValue(T a)
    {
        Initial = a;
    }
    public void OverrideInitial(T a) { Initial = a; }
    public void Set(T a) { Current = a; }
    public T Get() { return Current; }
    public void SetToInitial() { Current = Initial; }
    public void Reset() => SetToInitial();
    public static implicit operator T(ResettableValue<T> a) => a.Get();

    public void FromString(string a)
    {
        var p = a.Split(":");
        Initial = p[0].StringToObject<T>();
        Current = p[1].StringToObject<T>();
    }

    public override string ToString()
    {
        return Initial.ToString() + ":" + Current.ToString();
    }
}

public static class _ResetValExtensions
{
    public static float Percent(this ResettableValue<float> nerd)
    {
        return nerd.Current / nerd.Initial;
    }

    public static double Percent(this ResettableValue<double> nerd)
    {
        return nerd.Current / nerd.Initial;
    }

    public static float Percent(this ResettableValue<int> nerd)
    {
        return ((float)nerd.Current) / nerd.Initial;
    }

    public static double Percent(this ResettableValue<long> nerd)
    {
        return ((double)nerd.Current) / nerd.Initial;
    }
}