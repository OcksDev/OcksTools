public class ModdableValue<T>
{
    public T BaseValue;
    public T ProcessedValue;
    public T Value;
    public OXEventLayered<ModdableValue<T>> ActionsCompile = new();
    public OXEventLayered<ModdableValue<T>> ActionsDynamic = new();

    public ModdableValue(T initial)
    {
        SetBaseValue(initial);
        ProcessedValue = BaseValue;
    }
    public ModdableValue<T> SetBaseValue(T b)
    {
        BaseValue = b;
        Compile();
        return this;
    }

    public ModdableValue<T> Compile()
    {
        ProcessedValue = BaseValue;
        ActionsCompile.Invoke(this);
        return this;
    }
    public T GetValue()
    {
        Value = ProcessedValue;
        ActionsDynamic.Invoke(this);
        return Value;
    }
    public static implicit operator T(ModdableValue<T> r) => r.GetValue();
}
