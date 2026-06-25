using System.Collections.Generic;
using UnityEngine;
using static EntityOXS;

[System.Serializable]
public class EntityOXS
{
    public EntityObject Self;
    public EntityType Type = EntityType.Enemy;
    public double Health = 100;
    public double Max_Health = 100;
    public double Shield = 0;
    public double Max_Shield = 0;
    public OXEventLayered<EntityOXS, DamageProfile> OnHitEvent = new OXEventLayered<EntityOXS, DamageProfile>();
    public OXEventLayered<EntityOXS, DamageProfile> OnHealEvent = new OXEventLayered<EntityOXS, DamageProfile>();
    public OXEventLayered<EntityOXS, DamageProfile> OnHealEventPostCalc = new OXEventLayered<EntityOXS, DamageProfile>();
    public OXEventLayered<EntityOXS, MultiRef<EntityObject, EntityType>> OnKillEvent = new OXEventLayered<EntityOXS, MultiRef<EntityObject, EntityType>>();
    public OXEventLayered<EntityOXS, EffectProfile> OnEffectGain = new OXEventLayered<EntityOXS, EffectProfile>();
    public bool IsDead = false;
    private EntityObject KillerObject = null;
    private EntityType KillerType = EntityType.World;
    public void Hit(DamageProfile hit)
    {
        OnHitEvent.Append(500, "c", (x, y) => hit.CalcAmount());
        OnHitEvent.Invoke(this, hit);
        var dmg = hit.StoredDamage;
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
        ClampHealth();
    }

    public void Heal(DamageProfile amount)
    {
        var oldh = Health;
        var heal = amount.CalcAmount();
        Health = System.Math.Clamp(Health + heal, 0, Max_Health);
        var leftovers = heal - (Health - oldh);
        var olds = Shield;
        Shield = System.Math.Clamp(Shield + leftovers, 0, Max_Shield);
        var leftovers_unable = leftovers - (Shield - olds);

        // Amount Healed: heal - leftovers_unable

        if (Health != oldh || Shield != olds)
        {
            //runs if heal was successful
            OnHealEvent.Invoke(this, amount);
        }
        ClampHealth(); // this shouldn't do anything, but still
    }

    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;
        var mf = new MultiRef<EntityObject, EntityType>(KillerObject, KillerType);
        OnKillEvent.Invoke(this, mf);
    }

    public enum EntityType
    {
        Enemy = 0,
        Player = 1,
        NPC = 2,
        World = 3,
    }
    public EntityOXS SetSelf(EntityObject self)
    {
        Self = self;
        return this;
    }

    public void ClampHealth()
    {
        Health = System.Math.Clamp(Health, 0, Max_Health);
        Shield = System.Math.Clamp(Shield, 0, Max_Shield);
    }
}

[System.Serializable]
public class DamageProfile
{
    public double Value;
    public EntityObject SourceObject = null;
    public EntityType SourceType = EntityType.World;
    public DamageType HowItWasDealt = DamageType.Unknown;
    public DamageType WhatItWas = DamageType.Unknown;
    public HashSet<string> Procs = new HashSet<string>();
    public OXEventLayered<DamageProfile> CalcEvent = new OXEventLayered<DamageProfile>();
    public Vector3? SourceLocation = null;
    public CriticalChance Crit = new(0);
    public DamageProfile(EntityObject src_orbject, EntityType src_type, DamageType How, DamageType What, double TheValue)
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
        SourceType = pp.SourceType;
        HowItWasDealt = pp.HowItWasDealt;
        WhatItWas = pp.WhatItWas;
        CalcEvent = pp.CalcEvent;
        Value = pp.Value;
        Procs = new HashSet<string>(pp.Procs);
        SourceLocation = pp.SourceLocation;
        Crit = new(pp.Crit);
    }
    public double StoredDamage = -1;
    public double CalcAmount()
    {
        var x = Value;
        CalcEvent.Invoke(this);
        StoredDamage = Value;
        Value = x;

        StoredDamage *= Crit.GetDegree() + 1;


        return StoredDamage;
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
