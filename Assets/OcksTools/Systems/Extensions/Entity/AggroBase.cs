using System.Collections.Generic;
using System.Linq;

public abstract class AggroBase
{
    protected EntityObject CurrentTarget = null;
    protected EntityObject Self = null;
    public List<EntityObject> FallbackTargets = new List<EntityObject>();
    public virtual void Initialize(EntityObject s) => Self = s;
    public abstract List<EntityObject> GetAvailableTargets();

    public virtual void SetCurrentTarget(EntityObject a)
    {
        if (FallbackTargets.Contains(a)) FallbackTargets.Remove(a);
        if (CheckTarget(CurrentTarget)) FallbackTargets.Add(CurrentTarget);
        CurrentTarget = a;
    }
    public virtual EntityObject GetCurrentTarget()
    {
        if (CheckTarget(CurrentTarget)) return CurrentTarget;
        if (FallbackCondition())
        {
            Fallback();
        }
        else
        {
            CurrentTarget = null;
        }
        return CurrentTarget;
    }
    public virtual void Tick(float dt) { }
    public virtual void Fallback() => SetCurrentTarget(FallbackTargets.Last());
    public virtual bool FallbackCondition() => FallbackTargets.Count > 0;
    protected bool CheckTarget(EntityObject a) => a != null && !a.Entity.IsDead;
    public virtual bool CheckValidTargetCondition(DamageProfile y) => y.SourceObject != null && y.SourceType != Self.Entity.Type;
}

public abstract class MostDamageAggro : AggroBase
{
    protected Dictionary<EntityObject, double> agros = new Dictionary<EntityObject, double>();
    public override void Initialize(EntityObject s)
    {
        base.Initialize(s);
        Self.Entity.OnHitEvent.Append(999, "agro", (x, y) =>
        {
            if (CheckValidTargetCondition(y)) { return; }
            var pcheck = y.SourceObject;
            if (CurrentTarget == pcheck) return;
            else if (!CheckTarget(CurrentTarget))
            {
                SetCurrentTarget(pcheck);
                return;
            }
            try
            {
                if (!agros.ContainsKey(pcheck)) agros.Add(pcheck, 0);
                agros[pcheck] += y.StoredValue;
                if (agros[CurrentTarget] < agros[pcheck])
                {
                    SetCurrentTarget(pcheck);
                }
            }
            catch
            {
                SetCurrentTarget(pcheck);
            }
        });
    }
    public override void Tick(float dt)
    {
        var p = agros.ToList();
        float x = 0.99f.TimeStablePow(dt);
        foreach (var kv in p)
        {
            if (kv.Key == null || kv.Key.Entity.IsDead) { agros.Remove(kv.Key); continue; }
            agros[kv.Key] *= x;
        }
    }

    public override bool FallbackCondition() => agros.Count > 0;

    public override void Fallback()
    {
        EntityObject e = null;
        double m = double.MinValue;
        foreach (var kv in agros)
        {
            if (CheckTarget(kv.Key) && m < kv.Value)
            {
                e = kv.Key;
                m = kv.Value;
            }
        }
        if (e != null)
        {
            SetCurrentTarget(e);
            return;
        }
    }
    public override void SetCurrentTarget(EntityObject a)
    {
        CurrentTarget = a;
    }
}
