using System.Collections.Generic;

public class RangeMap<TValue>
{
    private readonly List<double> _starts;
    private readonly List<(double, TValue)> _values;
    public RangeMap()
    {
        _starts = new();
        _values = new();
    }
    public RangeMap(IEnumerable<(double Start, double End, TValue Value)> ranges)
    {
        var sorted = new List<(double Start, double End, TValue Value)>(ranges);
        sorted.Sort((a, b) => a.Start.CompareTo(b.Start));

        _starts = new List<double>(sorted.Count);
        _values = new List<(double, TValue)>(sorted.Count);

        for (int i = 0; i < sorted.Count; i++)
        {
            _starts.Add(sorted[i].Start);
            _values.Add((sorted[i].End, sorted[i].Value));
        }
    }

    public void Add(double Start, double End, TValue Value)
    {
        int idx = _starts.BinarySearch(Start);
        idx = ~idx - 1;
        _starts.Insert(idx, Start);
        _values.Insert(idx, (End, Value));
    }

    public void Add(double Start, TValue Value)
    {
        Add(Start, Start, Value);
    }

    public bool Remove(double Start)
    {
        int idx = _starts.BinarySearch(Start);
        if (idx < 0) idx = ~idx - 1;

        if (idx >= 0 && idx < _starts.Count && Start >= _starts[idx] && Start <= _values[idx].Item1)
        {
            _starts.RemoveAt(idx);
            _values.RemoveAt(idx);
            return true;
        }
        return false;
    }
    public bool TryGetValue(double x, out TValue value)
    {
        int idx = _starts.BinarySearch(x);
        if (idx < 0) idx = ~idx - 1;

        if (idx >= 0 && idx < _starts.Count && x >= _starts[idx] && x <= _values[idx].Item1)
        {
            value = _values[idx].Item2;
            return true;
        }

        value = default!;
        return false;
    }
}