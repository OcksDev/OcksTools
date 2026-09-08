
using System.Collections.Generic;
using UnityEngine;

public readonly struct BetterList<T>
{
    private readonly List<T> _values;

    public BetterList(T value)
    {
        _values = new List<T> { value };
    }

    public BetterList(List<T> values)
    {
        _values = values;
    }

    public BetterList(T[] values)
    {
        _values = new List<T>(values);
    }

    public static implicit operator BetterList<T>(T value)
        => new BetterList<T>(value);

    public static implicit operator BetterList<T>(List<T> values)
        => new BetterList<T>(values);

    public static implicit operator BetterList<T>(T[] values)
        => new BetterList<T>(values);

    public static implicit operator List<T>(BetterList<T> values)
        => values.ToList();
    public static implicit operator T[](BetterList<T> values)
        => values._values.ToArray();

    public List<T> ToList() => _values;
}
public readonly struct BetterVector2
{
    private readonly Vector2 _values;

    public BetterVector2(Vector2 value)
    {
        _values = value;
    }

    public BetterVector2((float x, float y) v)
    {
        _values = new(v.x, v.y);
    }

    public static implicit operator BetterVector2(Vector2 value)
        => new BetterVector2(value);

    public static implicit operator BetterVector2((float x, float y) values)
        => new BetterVector2(values);

    public static implicit operator Vector2(BetterVector2 values)
        => values.Value();

    public static implicit operator (float x, float y)(BetterVector2 values)
        => (values._values.x, values._values.y);

    public Vector2 Value() => _values;
}
public readonly struct BetterVector3
{
    private readonly Vector3 _values;

    public BetterVector3(Vector3 value)
    {
        _values = value;
    }

    public BetterVector3((float x, float y, float z) v)
    {
        _values = new(v.x, v.y, v.z);
    }

    public static implicit operator BetterVector3(Vector3 value)
        => new BetterVector3(value);

    public static implicit operator BetterVector3((float x, float y, float z) values)
        => new BetterVector3(values);

    public static implicit operator Vector3(BetterVector3 values)
        => values.Value();

    public static implicit operator (float x, float y, float z)(BetterVector3 values)
        => (values._values.x, values._values.y, values._values.z);

    public Vector3 Value() => _values;
}
public readonly struct BetterVector2Int
{
    private readonly Vector2Int _values;

    public BetterVector2Int(Vector2Int value)
    {
        _values = value;
    }

    public BetterVector2Int((int x, int y) v)
    {
        _values = new(v.x, v.y);
    }

    public static implicit operator BetterVector2Int(Vector2Int value)
        => new BetterVector2Int(value);

    public static implicit operator BetterVector2Int((int x, int y) values)
        => new BetterVector2Int(values);

    public static implicit operator Vector2Int(BetterVector2Int values)
        => values.Value();

    public static implicit operator (int x, int y)(BetterVector2Int values)
        => (values._values.x, values._values.y);

    public Vector2Int Value() => _values;
}
public readonly struct BetterVector3Int
{
    private readonly Vector3Int _values;

    public BetterVector3Int(Vector3Int value)
    {
        _values = value;
    }

    public BetterVector3Int((int x, int y, int z) v)
    {
        _values = new(v.x, v.y, v.z);
    }

    public static implicit operator BetterVector3Int(Vector3Int value)
        => new BetterVector3Int(value);

    public static implicit operator BetterVector3Int((int x, int y, int z) values)
        => new BetterVector3Int(values);

    public static implicit operator Vector3Int(BetterVector3Int values)
        => values.Value();

    public static implicit operator (int x, int y, int z)(BetterVector3Int values)
        => (values._values.x, values._values.y, values._values.z);

    public Vector3Int Value() => _values;
}