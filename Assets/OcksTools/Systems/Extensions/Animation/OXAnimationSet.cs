using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OXAnimationSet;

public class OXAnimationSet
{
    public List<OXAnimationPart> parts = new();
    public Reactable<bool> HasCompleted = new(false);

    public static OXAnimationSet FromBasicWithHandoff(System.Func<List<GameObject>, IEnumerator> s)
    {
        return new OXAnimationPart(s);
    }

    public static OXAnimationSet FromBasic(System.Func<List<GameObject>, IEnumerator> s, BetterList<GameObject> nerds)
    {
        return new OXAnimationPart(s, nerds);
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

    public void HandleHandoffs(BetterList<GameObject> nerds)
    {
        foreach (var part in parts)
        {
            if (part.OutsideTargeting) part.gameObjects = nerds;
        }
    }


    public IEnumerator PlayAnimation(MonoBehaviour source)
    {
        foreach (var a in parts)
        {
            a.OnPartStart.Invoke();
            switch (a._Playtype)
            {
                case OXAnimationPlayType.WaitUntilComplete:
                    yield return source.StartCoroutine(a._Func.Invoke(a.gameObjects));
                    break;
                case OXAnimationPlayType.DontWaitUntilComplete:
                    source.StartCoroutine(a._Func.Invoke(a.gameObjects));
                    break;
            }
            a.OnPartEnd.Invoke();
            if (a._Delay > 0) yield return new WaitForSeconds(a._Delay);
        }
        HasCompleted.SetValue(true);
    }

    //this converter is too powerful and c# is limiting my power
    //
    //public static implicit operator OXAnimationSet(System.Func<List<GameObject>, IEnumerator> r) => new OXAnimationSet(new OXAnimationPart(r));

    public static implicit operator OXAnimationSet(OXAnimationPart r) => new OXAnimationSet(r);

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
    public OXEvent OnPartStart = new();
    public OXEvent OnPartEnd = new();
    public OXAnimationPart(System.Func<List<GameObject>, IEnumerator> f)
    {
        _Func = f;
        OutsideTargeting = true;
    }
    public OXAnimationPart(System.Func<List<GameObject>, IEnumerator> f, BetterList<GameObject> gameObjects)
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
