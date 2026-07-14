using System;
using System.Collections.Generic;


public class MeetUp<TKey, TVal1, TVal2>
{
    public Dictionary<TKey, TVal1> Val1s = new();
    public Dictionary<TKey, TVal2> Val2s = new();
    public Action<TVal1, TVal2> Combiner;
    public virtual int Count => Val1s.Count + Val2s.Count;
    public MeetUp(Action<TVal1, TVal2> c)
    {
        Combiner = c;
    }
    public virtual void AddT1(TKey key, TVal1 v)
    {
        if (Val2s.TryGetValue(key, out TVal2 a2))
        {
            Val2s.Remove(key);
            Combiner(v, a2);
            return;
        }
        Val1s.Add(key, v);
    }
    public virtual void AddT2(TKey key, TVal2 v)
    {
        if (Val1s.TryGetValue(key, out TVal1 a2))
        {
            Val1s.Remove(key);
            Combiner(a2, v);
            return;
        }
        Val2s.Add(key, v);
    }
    public virtual void Remove(TKey key)
    {
        Val1s.Remove(key);
        Val2s.Remove(key);
    }
    public virtual bool Contains(TKey key)
    {
        return Val1s.ContainsKey(key) || Val2s.ContainsKey(key);
    }
    public virtual void Clear()
    {
        Val1s.Clear();
        Val2s.Clear();
    }
}

public class MeetUpWithDecay<TKey, TVal1, TVal2> : MeetUp<TKey, TVal1, TVal2>
{
    public MeetUpWithDecay(Action<TVal1, TVal2> c) : base(c) { }
    public DecayingDictionary<TKey, TVal1> Val1sDecay = new();
    public DecayingDictionary<TKey, TVal2> Val2sDecay = new();
    public void Tick(float dt)
    {
        Val1sDecay.Tick(dt);
        Val1sDecay.Tick(dt);
    }
    public override void Remove(TKey key)
    {
        Val1sDecay.Remove(key);
        Val2sDecay.Remove(key);
        base.Remove(key);
    }
    public override bool Contains(TKey key)
    {
        return Val1sDecay.Contains(key) || Val2sDecay.Contains(key) || base.Contains(key);
    }
    public override void Clear()
    {
        Val1sDecay.Clear();
        Val2sDecay.Clear();
        base.Clear();
    }
    public override int Count => Val1sDecay.Count + Val2sDecay.Count + base.Count;
    public override void AddT1(TKey key, TVal1 v) => AddT1(key, v, -1);
    public override void AddT2(TKey key, TVal2 v) => AddT2(key, v, -1);
    public void AddT1(TKey key, TVal1 v, float t)
    {
        if (Val2sDecay.TryGetValue(key, out TVal2 a))
        {
            Val2sDecay.Remove(key);
            Combiner(v, a);
            return;
        }
        else if (Val2s.TryGetValue(key, out TVal2 a2))
        {
            Val2s.Remove(key);
            Combiner(v, a2);
            return;
        }
        if (t > 0)
        {
            Val1sDecay.Add(key, v, t);
        }
        else
        {
            Val1s.Add(key, v);
        }
    }
    public void AddT2(TKey key, TVal2 v, float t)
    {
        if (Val1sDecay.TryGetValue(key, out TVal1 a))
        {
            Val1sDecay.Remove(key);
            Combiner(a, v);
            return;
        }
        else if (Val1s.TryGetValue(key, out TVal1 a2))
        {
            Val1s.Remove(key);
            Combiner(a2, v);
            return;
        }
        if (t > 0)
        {
            Val2sDecay.Add(key, v, t);
        }
        else
        {
            Val2s.Add(key, v);
        }
    }
}
public class MeetUp<TVal1, TVal2> : MeetUp<string, TVal1, TVal2>
{
    public MeetUp(Action<TVal1, TVal2> c) : base(c) { }
}

public class MeetUpWithDecay<TVal1, TVal2> : MeetUpWithDecay<string, TVal1, TVal2>
{
    public MeetUpWithDecay(Action<TVal1, TVal2> c) : base(c) { }
}