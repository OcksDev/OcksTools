using System.Collections.Generic;
using UnityEngine;

public class ShakeHolder
{
    public List<Shake> shakes = new();
    public void Add(Shake s)
    {
        shakes.Add(s);
    }

    public Vector3 GetPos(float dt)
    {
        Vector3 total = Vector3.zero;

        for (int i = 0; i < shakes.Count; i++)
        {
            total += shakes[i].GetShake(dt);
            if (shakes[i]._Strength <= 0.0001)
            {
                shakes.RemoveAt(i);
                i--;
            }
        }
        return total;
    }

}

public class Shake
{
    public float _Strength;
    public float _Speed;
    public float _Decay;
    public RandomType _RT;
    private Vector3 p1;
    private Vector3 p2;
    private float x;
    public enum RandomType
    {
        Circle,
        Square,
        Sphere,
        Box,
    }
    public Shake(float strength, float speed, float decay, RandomType rt = RandomType.Circle)
    {
        _Strength = strength;
        _Speed = speed;
        _RT = rt;
        _Decay = decay;
        Init();
    }
    public Shake()
    {
        _Strength = 1;
        _Speed = 1;
        _RT = RandomType.Circle;
        _Decay = 0.95f;
        Init();
    }
    public Shake Strength(float t)
    {
        _Strength = t;
        return this;
    }
    public Shake Speed(float t)
    {
        _Speed = t;
        return this;
    }
    public Shake Decay(float t)
    {
        _Decay = t;
        return this;
    }
    public Shake RandomMethod(RandomType t)
    {
        _RT = t;
        return this;
    }
    private void Init()
    {
        p1 = Vector3.zero;
        p2 = GetRandomPosByRT();
        x = 0;
    }
    public Vector3 GetRandomPosByRT()
    {
        switch (_RT)
        {
            case RandomType.Square: return new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            case RandomType.Box: return new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
            case RandomType.Circle: return Random.insideUnitCircle;
            case RandomType.Sphere: return Random.insideUnitSphere;
        }
        return Vector3.zero;
    }

    public Vector3 GetShake(float dt)
    {
        x = x + (dt * _Speed * 50);
        while (x > 1)
        {
            p1 = p2;
            p2 = GetRandomPosByRT();
            x -= 1;
        }
        var s = Vector3.Lerp(p1, p2, RandomFunctions.EaseSinInAndOut(x)) * _Strength;
        _Strength *= _Decay.TimeStablePow(dt);
        return s;

    }

}
