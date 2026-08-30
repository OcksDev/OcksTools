using System.Collections.Generic;

public class WeightedAverageHandler
{
    public static T DrawFromWeights<T>(WeightedAverage nerds, int? seed = null)
    {
        float get = 0;
        if (seed != null)
        {
            get = (float)(new OXRandom(seed.Value).NextDouble());
        }
        else
        {
            get = UnityEngine.Random.Range(0f, 1f);
        }
        var deez = nerds.lists;

        float sum = 0;
        float tot = 0;
        foreach (var dd in deez) sum += dd.a;
        for (int i = 0; i < deez.Count; i++)
        {
            var x = deez[i].a / sum;
            tot += x;
            if (tot >= get)
            {
                return (T)deez[i].b;
            }
        }
        return (T)deez[deez.Count - 1].b;
    }
}
public class WeightedAverage
{
    public List<MRef<float, object>> lists = new();
    public WeightedAverage Add(float weight, object n)
    {
        lists.Add(new MRef<float, object>(weight, n));
        return this;
    }
    public WeightedAverage() { }
    public WeightedAverage(List<MRef<float, object>> l) { lists = l; }
    public static implicit operator WeightedAverage(List<MRef<float, object>> l) => new WeightedAverage(l);
}