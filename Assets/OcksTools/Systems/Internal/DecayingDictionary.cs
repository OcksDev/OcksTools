using System.Collections.Generic;
using System.Linq;

public class DecayingDictionary<T, T2>
{
    public Dictionary<T, MultiRefClass<T2, float>> Items = new Dictionary<T, MultiRefClass<T2, float>>();

    public int Count => Items.Count;

    public void Add(T key, T2 item, float stayDuration) =>
        Items.AddOrUpdate(key, new MultiRefClass<T2, float>(item, stayDuration));

    public bool Contains(T key) => Items.ContainsKey(key);

    public bool TryGetValue(T key, out T2 value)
    {
        if (Items.TryGetValue(key, out var entry))
        {
            value = entry.a;
            return true;
        }
        value = default;
        return false;
    }

    public bool Remove(T key) => Items.Remove(key);

    public void Clear() => Items.Clear();

    public void Clean()
    {
        var keysToRemove = Items.Where(kvp => kvp.Value.a == null || kvp.Value.b <= 0)
                                 .Select(kvp => kvp.Key)
                                 .ToList();

        foreach (var key in keysToRemove)
            Items.Remove(key);
    }

    private List<T> keysToRemove = new();
    public void Tick(float dt)
    {
        keysToRemove.Clear();

        foreach (var kvp in Items)
        {
            kvp.Value.b -= dt;
            if (kvp.Value.b <= 0f)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
            Items.Remove(key);
    }
}
