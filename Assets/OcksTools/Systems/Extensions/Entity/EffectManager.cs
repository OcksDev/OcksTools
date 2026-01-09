using System.Collections.Generic;
using UnityEngine;
using static EntityOXS;

public class EffectManager : SingleInstance<EffectManager>
{
    private void Update()
    {
        foreach (var entity in ExtensionForEntityOXSForEffects.EffectsTicking)
        {
            entity.Value.UpdateEntityEffects(Time.deltaTime);
        }
    }
}

public static class ExtensionForEntityOXSForEffects
{
    public static Dictionary<EntityOXS, EntityEffectMiddleMan> EffectsTicking = new Dictionary<EntityOXS, EntityEffectMiddleMan>();
    public static Dictionary<EntityOXS, EntityEffectMiddleMan> EffectsGlobal = new Dictionary<EntityOXS, EntityEffectMiddleMan>();
    public static EntityEffectMiddleMan Effect(this EntityOXS nerd)
    {
        if (EffectsGlobal.ContainsKey(nerd))
        {
            return EffectsGlobal[nerd];
        }
        else
        {
            var a = new EntityEffectMiddleMan(nerd);
            EffectsGlobal.Add(nerd, a);
            nerd.OnKillEvent.Append(99999, "CleanUpEffects", CleanUpEffects);
            return a;
        }
    }
    public static EntityEffectMiddleManReadOnly EffectReadOnly(this EntityOXS nerd)
    {
        if (EffectsGlobal.ContainsKey(nerd))
        {
            return EffectsGlobal[nerd];
        }
        else
        {
            var a = new EntityEffectMiddleMan(nerd);
            EffectsGlobal.Add(nerd, a);
            nerd.OnKillEvent.Append("CleanUpEffects", CleanUpEffects);
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

public class EntityEffectMiddleMan : EntityEffectMiddleManReadOnly
{
    public EntityEffectMiddleMan(EntityOXS e) : base(e)
    {
        entity = e;
        Effects = new List<EffectProfile>();
    }
    public void UpdateEntityEffects(float time)
    {
        for (int i = Effects.Count - 1; i >= 0; i--)
        {
            var ef = Effects[i];
            ef.TimeRemaining -= time;
            if (ef.TimeRemaining <= 0)
            {
                Effects.RemoveAt(i);
            }
            else
            {
                Effects[i] = ef;
            }
        }
        CheckExistence();
    }

    public void Add(EffectProfile eff)
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
                    Effects.Remove(s);
                    Effects.Add(eff);
                    break;
                case EffectProfile.CombineMethods.AddNew:
                    //apply effect as new
                    Effects.Add(eff);
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
            Effects.Add(eff);
        }


    }

    public void Remove(string name)
    {
        for (int i = Effects.Count - 1; i >= 0; i--)
        {
            if (Effects[i].Name == name)
            {
                Effects.RemoveAt(i);
                break;
            }
        }
        CheckExistence();
    }

    public void RemoveAll(string name)
    {
        for (int i = Effects.Count - 1; i >= 0; i--)
        {
            if (Effects[i].Name == name)
            {
                Effects.RemoveAt(i);
            }
        }
        CheckExistence();
    }

    public void Remove(EffectProfile name)
    {
        Remove(name.Name);
    }

    public void RemoveAll(EffectProfile name)
    {
        RemoveAll(name.Name);
    }
    public void Clear()
    {
        Effects.Clear();
        CheckExistence();
    }

    public void CheckExistence()
    {
        if (Effects.Count == 0)
        {
            ExtensionForEntityOXSForEffects.EffectsTicking.Remove(entity);
        }
    }
}
public class EntityEffectMiddleManReadOnly
{
    public EntityOXS entity;
    public List<EffectProfile> Effects;
    public EntityEffectMiddleManReadOnly(EntityOXS e)
    {
        entity = e;
        Effects = new List<EffectProfile>();
    }
    public EffectProfile Get(string name)
    {
        foreach (var ef in Effects)
        {
            if (name == ef.Name)
            {
                return ef;
            }
        }
        return null;
    }
    public EffectProfile Get(EffectProfile eff)
    {
        return Get(eff.Name);
    }

}