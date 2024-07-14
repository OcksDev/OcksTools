using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityOXS : MonoBehaviour
{
    public double Health = 100;
    public double Shield = 100;
    public double Max_Health = 100;
    public double Max_Shield = 100;
    public List<EffectProfile> Effects = new List<EffectProfile>();
    public void Hit(DamageProfile hit)
    {
        foreach (var effect in hit.Effects)
        {
            AddEffect(effect);
        }
        Shield -= hit.Damage;
        if (Shield < 0)
        {
            Health += Shield;
        }
    }
    public void Kill()
    {
        Destroy(gameObject);
    }
    private void Update()
    {
        Health = System.Math.Clamp(Health, 0, Max_Health);
        Shield = System.Math.Clamp(Shield, 0, Max_Shield);
        for(int i = 0; i < Effects.Count; i++)
        {
            Effects[i].TimeRemaining -= Time.deltaTime;
            if (Effects[i].TimeRemaining <= 0)
            {
                Effects.RemoveAt(i);
                i--;
            }
        }
        if (Health <= 0)
        {
            Kill();
        }
    }

    public void AddEffect(EffectProfile eff)
    {
        eff.TimeRemaining = eff.Duration;
        bool alreadyhaseffect = false;
        EffectProfile s = null;
        foreach(var ef in Effects)
        {
            if (eff.Type == ef.Type)
            {
                s = ef;
                alreadyhaseffect = true;
                break;
            }
        }
        if (alreadyhaseffect)
        {
            switch (eff.CombineMethod)
            {
                default:
                    //replace existing effect with new
                    Effects.Remove(s);
                    Effects.Add(eff);
                    break;
                case 1:
                    //apply effect as new
                    Effects.Add(eff);
                    break;
                case 2:
                    //increase stack count
                    s.Stack++;
                    break;
                case 3:
                    //increase stack count, up to maximum value
                    s.Stack++;
                    if (s.Stack > s.MaxStack) s.Stack = s.MaxStack;
                    break;
                case 4:
                    //increase stack count, up to maximum value, refresh duration
                    s.Stack++;
                    s.TimeRemaining = eff.Duration;
                    if (s.Stack > s.MaxStack) s.Stack = s.MaxStack;
                    break;
                case 5:
                    //add old time remaining with new time (2s + 5s = 7s)
                    s.TimeRemaining += eff.Duration;
                    break;
                case 6:
                    //add old time remaining with new time (2s + 5s = 7s), also increase stack count
                    s.TimeRemaining += eff.Duration;
                    s.Stack++;
                    break;
                case 7:
                    //increase stack count, refresh time remaining
                    s.Stack++;
                    s.TimeRemaining = eff.Duration;
                    break;
            }
        }
        else
        {
            Effects.Add(eff);
        }


    }


}


public class DamageProfile
{
    public string Name = "";
    public double Damage;
    public List<EffectProfile> Effects = new List<EffectProfile>();
    public List<string> Procs = new List<string>();
    public DamageProfile(string name, float damage)
    {
        Damage = damage;
        Name = name;
    }
    public List<string> GiveProcs()
    {
        var e = new List<string>(Procs);
        e.Add(Name);
        return e;
    }
    public DamageProfile(DamageProfile pp)
    {
        Name = pp.Name;
        Damage = pp.Damage;
        Procs = new List<string>(pp.Procs);
        Effects = new List<EffectProfile>(pp.Effects);
    }

}

public class EffectProfile
{
    //data you pass in
    public int Type;
    public float Duration;
    public int CombineMethod;
    //other data
    public int Stack = 1;
    public float TimeRemaining;
    public int MaxStack;
    public string Name;
    public EffectProfile(int type, float time, int add_method, int stacks = 1)
    {
        SetData();
        Type = type;
        Duration = time;
        CombineMethod = add_method;
        Stack =stacks;
    }
    public EffectProfile()
    {
        SetData();
    }

    public void SetData()
    {
        MaxStack = 0;
        Name = "Error";
        switch (Type)
        {
            //some example effects
            case 0:
                Name = "Burning";
                break;
            case 1:
                Name = "Healing Energy";
                MaxStack = 6;
                break;
        }
    }
    public EffectProfile(EffectProfile pp)
    {
        Type = pp.Type;
        Duration = pp.Duration;
        CombineMethod = pp.CombineMethod;
        Stack = pp.Stack;
        TimeRemaining = pp.TimeRemaining;
        SetData();
    }

}
