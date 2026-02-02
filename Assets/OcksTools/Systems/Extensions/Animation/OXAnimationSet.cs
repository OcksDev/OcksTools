using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OXAnimationSet;

public class OXAnimationSet
{
    public List<OXAnimationPart> parts = new();
    public Reactable<bool> HasCompleted = new(false);

    public static OXAnimationSet FromBasic(System.Func<List<GameObject>, IEnumerator> s)
    {
        return new OXAnimationSet(new OXAnimationPart(s));
    }

    public OXAnimationSet()
    {
    }
    public OXAnimationSet(OXAnimationPart part)
    {
        parts.Add(part);
    }
    public OXAnimationSet(List<OXAnimationPart> parts)
    {
        this.parts = parts;
    }
    public OXAnimationSet AddPart(OXAnimationPart part)
    {
        parts.Add(part);
        return this;
    }

    public IEnumerator PlayAnimation(MonoBehaviour source)
    {
        foreach (var a in parts)
        {
            switch (a._Playtype)
            {
                case OXAnimationPlayType.WaitUntilComplete:
                    yield return source.StartCoroutine(a._Func.Invoke(a.gameObjects));
                    break;
                case OXAnimationPlayType.DontWaitUntilComplete:
                    source.StartCoroutine(a._Func.Invoke(a.gameObjects));
                    break;
            }
            if (a._Delay > 0) yield return new WaitForSeconds(a._Delay);
        }
        HasCompleted.SetValue(true);
    }

    //this converter is too powerful and c# is limiting my power
    //
    //public static implicit operator OXAnimationSet(System.Func<List<GameObject>, IEnumerator> r) => new OXAnimationSet(new OXAnimationPart(r));


    public enum OXAnimationPlayType
    {
        WaitUntilComplete,
        DontWaitUntilComplete,
    }
}

public class OXAnimationPart
{
    public System.Func<List<GameObject>, IEnumerator> _Func;
    public List<GameObject> gameObjects = new();
    public OXAnimationPlayType _Playtype = OXAnimationPlayType.WaitUntilComplete;
    public float _Delay = 0f;
    public bool OutsideTargeting = false;
    public OXAnimationPart(System.Func<List<GameObject>, IEnumerator> f)
    {
        _Func = f;
        OutsideTargeting = true;
    }
    public OXAnimationPart(System.Func<List<GameObject>, IEnumerator> f, List<GameObject> gameObjects)
    {
        _Func = f;
        this.gameObjects = gameObjects;
    }
    public OXAnimationPart Playtype(OXAnimationPlayType playtype)
    {
        _Playtype = playtype;
        return this;
    }
    public OXAnimationPart Delay(float delay)
    {
        _Delay = delay;
        return this;
    }
    public OXAnimationPart GameObjects(List<GameObject> gameObjects)
    {
        this.gameObjects = gameObjects;
        return this;
    }
}
public static class AnimationExtensions
{
    public static OXAnimationSet AsAnimation(
        this Func<List<GameObject>, IEnumerator> f
    ) => new(new OXAnimationPart(f));
}

public readonly struct AnimationFunc
{
    public readonly Func<List<GameObject>, IEnumerator> _func;

    public AnimationFunc(Func<List<GameObject>, IEnumerator> func)
    {
        _func = func;
    }

    public static implicit operator AnimationFunc(
        Func<List<GameObject>, IEnumerator> func
    ) => new(func);

    public static implicit operator OXAnimationSet(AnimationFunc f)
        => new(new OXAnimationPart(f._func));
}