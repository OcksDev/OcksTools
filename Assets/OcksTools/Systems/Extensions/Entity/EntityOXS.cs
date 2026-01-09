using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityOXS : MonoBehaviour
{
    public EntityType Type = EntityType.Enemy;
    public double Health = 100;
    public double Shield = 100;
    public double Max_Health = 0;
    public double Max_Shield = 0;
    public OXEvent<EntityOXS, DamageProfile> OnHitEvent = new OXEvent<EntityOXS, DamageProfile>();
    public OXEvent<EntityOXS, DamageProfile> OnHealEvent = new OXEvent<EntityOXS, DamageProfile>();
    public OXEvent<EntityOXS> OnKillEvent = new OXEvent<EntityOXS>();
    public OXEvent<EntityOXS, EffectProfile> OnEffectGain = new OXEvent<EntityOXS, EffectProfile>();
    public OXEvent<EntityOXS> OnKillEventFinal = new OXEvent<EntityOXS>();
    public bool IsDead = false;
    public void Hit(DamageProfile hit)
    {
        OnHitEvent.Invoke(this, hit);
        var dmg = hit.CalcDamage();
        Shield -= dmg;
        if (Shield < 0)
        {
            Health += Shield;
            if (Health <= 0)
            {
                Kill();
            }
        }
        Shield = System.Math.Clamp(Shield, 0, Max_Shield);
    }

    public void Heal(DamageProfile amount)
    {
        var oldh = Health;
        var heal = amount.CalcDamage();
        Health = System.Math.Clamp(Health + heal, 0, Max_Health);
        var change = heal - (Health - oldh);
        var olds = Shield;
        Shield = System.Math.Clamp(Shield + change, 0, Max_Shield);
        var change2 = change - (Shield - olds);

        // Amount Healed: amount - change2

        if (Health != oldh || Shield != olds)
        {
            //runs if heal was successful
            OnHealEvent.Invoke(this, amount);
        }
    }

    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;
        OnKillEvent.Invoke(this);
        OnKillEventFinal.Invoke(this);
    }

    public enum EntityType
    {
        Enemy,
        Player,
        NPC,
        World,
    }

}


public class DamageProfile
{
    public object SourceObject;
    public DamageType HowItWasDealt = DamageType.Unknown;
    public DamageType WhatItWas = DamageType.Unknown;
    public double Damage;
    public HashSet<string> Procs = new HashSet<string>();
    public OXEvent<DamageProfile> DamageCalcEvent = new OXEvent<DamageProfile>();
    public DamageProfile(object OB, DamageType How, DamageType What, double damage)
    {
        SourceObject = OB;
        HowItWasDealt = How;
        WhatItWas = What;
        Damage = damage;
    }
    public DamageProfile(DamageProfile pp)
    {
        SourceObject = pp.SourceObject;
        HowItWasDealt = pp.HowItWasDealt;
        Damage = pp.Damage;
        Procs = new HashSet<string>(pp.Procs);
    }
    public double CalcDamage()
    {
        var x = Damage;
        DamageCalcEvent.Invoke(this);
        var damage = Damage;
        Damage = x;
        //do some other damage calculaations
        return damage;
    }
    public enum DamageType // add more as needed
    {
        Unknown = 0,
        Magic = 1,
        Melee = 2,
        Ranged = 3,
        Trap = 4,
        Fall = 5,
        World = 6,
        Fire = 7,
        Ice = 8,
        Water = 9,
        Earth = 10,
        Air = 11,
        Dark = 12,
        Light = 13,
        Healing = 14,
    }

}

public class EffectProfile
{
    //data you pass in
    public string Name;
    public float Duration;
    public CombineMethods CombineMethod = CombineMethods.Replace;
    //other data
    public int Stack = 1;
    public float TimeRemaining;
    //non-transferable data
    public int MaxStack;
    public EffectProfile(string type, float time, CombineMethods add_method = CombineMethods.Replace, int stacks = 1)
    {
        Name = type;
        Duration = time;
        CombineMethod = add_method;
        Stack = stacks;
        SetData();
    }
    public EffectProfile()
    {
        SetData();
    }

    public void SetData()
    {
        MaxStack = 0;
        switch (Name)
        {
            //some example effects
            case "Boner Juice":
                MaxStack = 6;
                break;
        }
    }
    public EffectProfile(EffectProfile pp)
    {
        Name = pp.Name;
        Duration = pp.Duration;
        CombineMethod = pp.CombineMethod;
        Stack = pp.Stack;
        TimeRemaining = pp.TimeRemaining;
        SetData();
    }
    public enum CombineMethods
    {
        Replace,
        AddNew,
        CombineStack,
        CombineStackSetTime,
        CombineStackCombineTime,
        CombineTime,
    }
}

public class ret_cum_shenan
{
    public bool hasthing;
    public EffectProfile susser;
}