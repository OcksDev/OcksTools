using System.Collections.Generic;

[System.Serializable]
public class PoolSelector<T>
{
    public SelectionTypeFromPool SelectionType = SelectionTypeFromPool.Weighted;
    private List<PoolNode<T>> Pool = new();
    public enum SelectionTypeFromPool
    {
        Weighted = 0,
        ChanceFallThroughTopFirst = 1,
        ChanceFallThroughBottomFirst = 2,
    }
    public PoolNode<T> Pull()
    {
        switch (SelectionType)
        {
            case SelectionTypeFromPool.Weighted:
                var d = new WeightedAverage();
                foreach (PoolNode<T> node in Pool) d.Add(node.ChanceOrWeight, node);
                return WeightedAverageHandler.DrawFromWeights<PoolNode<T>>(d);
            case SelectionTypeFromPool.ChanceFallThroughTopFirst:
                for (int i = 0; i < Pool.Count - 1; i++)
                {
                    if (UnityEngine.Random.Range(0f, 1f) <= Pool[i].ChanceOrWeight) return Pool[i];
                }
                return Pool[Pool.Count - 1];
            case SelectionTypeFromPool.ChanceFallThroughBottomFirst:
                for (int i = 0; i < Pool.Count - 1; i++)
                {
                    int j = (Pool.Count - 1) - i;
                    if (UnityEngine.Random.Range(0f, 1f) <= Pool[j].ChanceOrWeight) return Pool[j];
                }
                return Pool[0];
        }
        return null;

    }
}
[System.Serializable]
public class PoolNode<T>
{
    public string Name;
    public T Object;
    public float ChanceOrWeight = 0.5f;
    public bool Enabled = true;
}