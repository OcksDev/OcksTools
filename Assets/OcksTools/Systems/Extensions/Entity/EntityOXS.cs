using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using static EntityOXS;

[System.Serializable]
public class EntityOXS : MonoBehaviour
{
    public EntityType Type = EntityType.Enemy;
    public double Health = 100;
    public double Shield = 100;
    public double Max_Health = 0;
    public double Max_Shield = 0;
    public OXEventLayered<EntityOXS, DamageProfile> OnHitEvent = new OXEventLayered<EntityOXS, DamageProfile>();
    public OXEventLayered<EntityOXS, DamageProfile> OnHealEvent = new OXEventLayered<EntityOXS, DamageProfile>();
    public OXEventLayered<EntityOXS, MultiRef<object, EntityType>> OnKillEvent = new OXEventLayered<EntityOXS, MultiRef<object, EntityType>>();
    public OXEventLayered<EntityOXS, EffectProfile> OnEffectGain = new OXEventLayered<EntityOXS, EffectProfile>();
    public bool IsDead = false;
    private object KillerObject = null;
    private EntityType KillerType = EntityType.World;
    public void Hit(DamageProfile hit)
    {
        OnHitEvent.Invoke(this, hit);
        var dmg = hit.CalcAmount();
        KillerObject = hit.SourceObject;
        KillerType = hit.SourceType;

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
        var heal = amount.CalcAmount();
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
        var mf = new MultiRef<object, EntityType>(KillerObject, KillerType);
        OnKillEvent.Invoke(this, mf);
    }

    public enum EntityType
    {
        Enemy = 0,
        Player = 1,
        NPC = 2,
        World = 3,
    }

}

[System.Serializable]
public class DamageProfile
{
    public double Value;
    public object SourceObject = null;
    public EntityType SourceType = EntityType.World;
    public DamageType HowItWasDealt = DamageType.Unknown;
    public DamageType WhatItWas = DamageType.Unknown;
    public HashSet<string> Procs = new HashSet<string>();
    public OXEventLayered<DamageProfile> CalcEvent = new OXEventLayered<DamageProfile>();
    public DamageProfile(object src_orbject, EntityType src_type, DamageType How, DamageType What, double TheValue)
    {
        SourceObject = src_orbject;
        SourceType = src_type;
        HowItWasDealt = How;
        WhatItWas = What;
        Value = TheValue;
    }
    public DamageProfile(DamageType How, DamageType What, double TheValue)
    {
        HowItWasDealt = How;
        WhatItWas = What;
        Value = TheValue;
    }
    public DamageProfile(DamageProfile pp)
    {
        SourceObject = pp.SourceObject;
        HowItWasDealt = pp.HowItWasDealt;
        Value = pp.Value;
        Procs = new HashSet<string>(pp.Procs);
    }
    public double CalcAmount()
    {
        var x = Value;
        CalcEvent.Invoke(this);
        var output_value = Value;
        Value = x;
        //do some other calculaations
        return output_value;
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

[System.Serializable]
public class EffectProfile
{
    //data you pass in
    public string Name;
    public float Duration;
    public CombineMethods CombineMethod = CombineMethods.Replace;
    //other data
    [ReadOnly]
    public int Stack = 1;
    [ReadOnly]
    public float TimeRemaining;
    //non-transferable data
    [HideInInspector]
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