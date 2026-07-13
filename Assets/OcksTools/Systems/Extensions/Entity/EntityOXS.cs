using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityOXS
{
    public EntityType Type = EntityType.Enemy;
    public double Health = 100;
    public double Max_Health = 100;
    public double Shield = 0;
    public double Max_Shield = 0;
    public bool CanOverhealIntoShield = false;
    public OXEventLayered<EntityOXS, DamageProfile> OnHitEvent = new();
    public OXEventLayered<EntityOXS, DamageProfile> OnHealEvent = new();
    public OXEventLayered<EntityOXS, EntityObject> OnKillEvent = new();
    public OXEventLayered<EntityOXS, EffectProfile> OnEffectGain = new();
    [HideInInspector]
    public bool IsDead = false;
    private EntityObject KillerObject = null;
    public EntityObject Self;
    public bool IsEnemy => Type == EntityType.Enemy;
    public bool IsPlayer => Type == EntityType.Player;

    public bool HasLifeRemaining => (Health > 0 || Shield > 0) && !IsDead;


    public void Hit(DamageProfile hit)
    {
        if (hit == null) throw new System.Exception($"Tried to Hit {Type} but 'null' Damage Profile was provided.");
        OnHitEvent.Append(500, "c", (x, y) => hit.CalcAmount());
        OnHitEvent.Invoke(this, hit);
        hit.OnProcHit.Invoke(this, hit);
        var dmg = hit.StoredValue;
        KillerObject = hit.SourceObject;

        Shield -= dmg;
        if (Shield <= 0)
        {
            Health += Shield;
            if (!HasLifeRemaining)
            {
                hit.OnProcKill.Invoke(this, hit);
                Kill();
            }
        }
        ClampHealth();
    }

    public void Heal(DamageProfile hit)
    {
        if (hit == null) throw new System.Exception($"Tried to Heal {Type} but 'null' Damage Profile was provided.");
        var oldh = Health;

        OnHealEvent.Append(500, "c", (x, y) => hit.CalcAmount());
        OnHealEvent.Invoke(this, hit);
        hit.OnProcHit.Invoke(this, hit);
        var heal = hit.StoredValue;

        Health = System.Math.Clamp(Health + heal, 0, Max_Health);
        if (CanOverhealIntoShield)
        {
            var leftovers = heal - (Health - oldh);
            var olds = Shield;
            Shield = System.Math.Clamp(Shield + leftovers, 0, Max_Shield);
            var leftovers_unable = leftovers - (Shield - olds); // extra health that there wasn't enough space for
        }


        ClampHealth(); // this shouldn't do anything, but still
    }

    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;
        OnKillEvent.Invoke(this, KillerObject);
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
    public void SetHealths(double x)
    {
        Max_Health = x;
        Health = x;
    }
    public void SetShields(double x)
    {
        Max_Shield = x;
        Shield = x;
    }
}

[System.Serializable]
public class DamageProfile
{
    public double Value;
    public EntityObject SourceObject = null;
    public DamageType HowItWasDealt = DamageType.Unknown;
    public DamageType WhatItWas = DamageType.Unknown;
    public HashSet<string> Procs = new HashSet<string>();
    public OXEventLayered<DamageProfile> CalcEvent = new();
    public OXEventLayered<EntityOXS, DamageProfile> OnProcHit = new();
    public OXEventLayered<EntityOXS, DamageProfile> OnProcKill = new();
    public Vector3? SourceLocation = null;
    public CriticalChance Crit = new(0);
    public DamageProfile(double TheValue, DamageType How, DamageType What, EntityObject src_orbject)
    {
        HowItWasDealt = How;
        WhatItWas = What;
        Value = TheValue;
        SetSource(src_orbject);
    }
    public DamageProfile(double TheValue, DamageType How, DamageType What)
    {
        HowItWasDealt = How;
        WhatItWas = What;
        Value = TheValue;
    }
    public DamageProfile(double TheValue)
    {
        Value = TheValue;
    }
    public DamageProfile(DamageProfile pp)
    {
        SourceObject = pp.SourceObject;
        HowItWasDealt = pp.HowItWasDealt;
        WhatItWas = pp.WhatItWas;
        CalcEvent = pp.CalcEvent;
        OnProcKill = pp.OnProcKill;
        OnProcHit = pp.OnProcHit;
        Value = pp.Value;
        Procs = new HashSet<string>(pp.Procs);
        SourceLocation = pp.SourceLocation;
        Crit = new(pp.Crit);
    }
    public double StoredValue = -1;
    public double CalcAmount()
    {
        var x = Value;
        CalcEvent.Invoke(this);
        StoredValue = Value;
        Value = x;

        StoredValue *= Crit.GetDegree() + 1;


        return StoredValue;
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
    public DamageProfile SetSource(EntityObject src_orbject)
    {
        SourceObject = src_orbject;
        if (src_orbject != null) SourceLocation = src_orbject.transform.position;
        return this;
    }
    public DamageProfile SetPosition(Vector3? position)
    {
        SourceLocation = position;
        return this;
    }
}
