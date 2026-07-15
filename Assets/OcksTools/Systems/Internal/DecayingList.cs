using System.Collections.Generic;
public class DecayingList<T>
{
    public List<MultiRefClass<T, float>> Items = new List<MultiRefClass<T, float>>();
    public OXEvent<T> OnRemove = new();
    public int Count => Items.Count;
    public void Add(T item, float stayDuration) =>
        Items.Add(new MultiRefClass<T, float>(item, stayDuration));

    public bool Contains(T item)
    {
        foreach (var thing in Items)
        {
            if (thing.a == null) continue;
            if (thing.a.Equals(item)) return true;
        }
        return false;
    }

    public bool Remove(T item)
    {
        int index = Items.FindIndex(e => EqualityComparer<T>.Default.Equals(e.a, item));
        if (index < 0)
            return false;

        OnRemove.Invoke(Items[index].a);
        Items.RemoveAt(index);
        return true;
    }

    public void Clear() => Items.Clear();
    public void Clean() => Items.RemoveAll((x) => x.a == null || x.b <= 0);
    public void Tick(float dt)
    {
        for (int i = Items.Count - 1; i >= 0; i--)
        {
            Items[i].b -= dt;
            if (Items[i].b <= 0f)
            {
                OnRemove.Invoke(Items[i].a);
                Items.RemoveAt(i);
            }
        }
    }

}
