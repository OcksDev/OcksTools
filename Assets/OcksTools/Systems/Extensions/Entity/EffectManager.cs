using System.Collections.Generic;
using UnityEngine;
using static EntityOXS;

public class EffectManager : SingleInstance<EffectManager>
{
    private void Update()
    {
        foreach (var entity in ExtensionForEntityOXSForEffects.EffectsTicking)
        {
            entity.Value.UpdateContainer(Time.deltaTime);
        }
    }
}

public static class ExtensionForEntityOXSForEffects
{
    public static Dictionary<EntityOXS, EffectContainer> EffectsTicking = new Dictionary<EntityOXS, EffectContainer>();
    public static Dictionary<EntityOXS, EffectContainer> EffectsGlobal = new Dictionary<EntityOXS, EffectContainer>();
    public static EffectContainer Effect(this EntityOXS nerd)
    {
        if (EffectsGlobal.ContainsKey(nerd))
        {
            return EffectsGlobal[nerd];
        }
        else
        {
            var a = new EffectContainer(nerd);
            EffectsGlobal.Add(nerd, a);
            nerd.OnKillEvent.Append(99999, "CleanUpEffects", CleanUpEffects);
            return a;
        }
    }

    public static void CleanUpEffects(EntityOXS nerd, MultiRef<object, EntityType> b)
    {
        if (EffectsTicking.ContainsKey(nerd))
        {
            EffectsTicking.Remove(nerd);
        }
        EffectsGlobal.Remove(nerd);
    }
}

public class EffectContainer : ContainerListStyle<EffectProfile>
{
    public EffectContainer(EntityOXS e)
    {
        entity = e;
        List = new List<EffectProfile>();
    }
    public override void UpdateContainer(float time)
    {
        for (int i = List.Count - 1; i >= 0; i--)
        {
            var ef = List[i];
            ef.TimeRemaining -= time;
            if (ef.TimeRemaining <= 0)
            {
                List.RemoveAt(i);
            }
            else
            {
                List[i] = ef;
            }
        }
        CheckExistence();
    }

    public override void Add(EffectProfile eff)
    {
        entity.OnEffectGain.Invoke(entity, eff);
        if (!ExtensionForEntityOXSForEffects.EffectsTicking.ContainsKey(entity))
        {
            ExtensionForEntityOXSForEffects.EffectsTicking.Add(entity, this);
        }
        eff.TimeRemaining = eff.Duration;
        EffectProfile s = Get(eff.Name);
        if (s != null)
        {
            switch (eff.CombineMethod)
            {
                case EffectProfile.CombineMethods.Replace:
                    //replace existing effect with new
                    List.Remove(s);
                    List.Add(eff);
                    break;
                case EffectProfile.CombineMethods.AddNew:
                    //apply effect as new
                    List.Add(eff);
                    break;
                case EffectProfile.CombineMethods.CombineStack:
                    //increase stack count
                    s.Stack += eff.Stack;
                    if (s.MaxStack > 0 && s.Stack > s.MaxStack) s.Stack = s.MaxStack;
                    break;
                case EffectProfile.CombineMethods.CombineStackSetTime:
                    //increase stack count refresh duration
                    s.Stack += eff.Stack;
                    s.TimeRemaining = eff.Duration;
                    if (s.MaxStack > 0 && s.Stack > s.MaxStack) s.Stack = s.MaxStack;
                    break;
                case EffectProfile.CombineMethods.CombineStackCombineTime:
                    //add old time remaining with new time (2s + 5s = 7s), also increase stack count
                    s.TimeRemaining += eff.Duration;
                    s.Stack += eff.Stack;
                    if (s.MaxStack > 0 && s.Stack > s.MaxStack) s.Stack = s.MaxStack;
                    break;
                case EffectProfile.CombineMethods.CombineTime:
                    //add old time remaining with new time (2s + 5s = 7s)
                    s.TimeRemaining += eff.Duration;
                    break;
            }
        }
        else
        {
            List.Add(eff);
        }


    }
    public override void CheckExistence()
    {
        if (List.Count == 0)
        {
            ExtensionForEntityOXSForEffects.EffectsTicking.Remove(entity);
        }
    }
}