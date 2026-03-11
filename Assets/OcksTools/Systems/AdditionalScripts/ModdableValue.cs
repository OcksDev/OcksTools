public class ModdableValue<T>
{
    public T BaseValue;
    public T ProcessedValue;
    public T Value;
    public OXEventLayered<ModdableValue<T>> ActionsForCompile = new();
    public OXEventLayered<ModdableValue<T>> ActionsForDynamic = new();

    public void SetBaseValue(T b)
    {
        BaseValue = b;
        Compile();
    }

    public void Compile()
    {
        ProcessedValue = BaseValue;
        ActionsForCompile.Invoke(this);
    }
    public T GetValue()
    {
        Value = ProcessedValue;
        ActionsForDynamic.Invoke(this);
        return Value;
    }
    public static implicit operator T(ModdableValue<T> r) => r.GetValue();
}
