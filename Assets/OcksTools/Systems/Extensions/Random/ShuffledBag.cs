using System.Collections.Generic;

public class ShuffledBag<T>
{
    public List<T> BaseItems = new List<T>();
    public List<List<T>> Layers = new List<List<T>>();
    OXRandom Random;

    public ShuffledBag(OXRandom r, List<T> items)
    {
        BaseItems = new List<T>(items);
        Random = r;
    }

    public ShuffledBag(List<T> items)
    {
        BaseItems = new List<T>(items);
        Random = new();
    }

    public void Collapse()
    {
        if (Layers.Count == 0) return;
        if (Layers[0].Count == 0)
        {
            Layers.RemoveAt(0);
        }
    }

    public void Fill(int layer)
    {
        int curl = Layers.Count - 1;
        if (curl >= layer) return;
        int diff = layer - curl;
        for (int i = 0; i < diff; i++)
        {
            var it = new List<T>(BaseItems);
            it = it.ShuffleList(Random);
            Layers.Add(it);
        }
    }

    public T Draw()
    {
        if (Layers.Count == 0 || Layers[0].Count == 0)
        {
            Collapse();
            Fill(0);
        }
        T item = Layers[0][0];
        Layers[0].RemoveAt(0);
        return item;
    }

    public T Peak(int depth = 0)
    {
        if (Layers.Count == 0 || Layers[0].Count == 0)
        {
            Collapse();
            Fill(0);
        }
        int layer = 0;
        int index = depth;
        for (int i = 0; index >= Layers[layer].Count; i++)
        {
            index -= Layers[layer].Count;
            layer++;
            Fill(layer);
        }
        return Layers[layer][index];
    }
}
