using UnityEngine;

[System.Serializable]
public class SkillData
{
    public string Name;
    public float Duration;
    public float Cooldown;
    public float InterUseCooldown;
    public int MaxStacks = 0;
    public int StacksPerCooldown = 1;
    public int StacksPerUse = 1;
    public bool DisableAutoCooldown;
    public bool AllowActivationWhileActive;
    public OXEvent<EntityOXS, Skill> OnSkillActivation = new();
}

[System.Serializable]
public class Skill : Containable
{
    public int Stacks = 0;
    public float Duration = 0;
    public float Cooldown = 0;
    public float InterUseCooldown = 0;
    public float CooldownMult = 1;
    public float DurationMult = 1;
    [HideInInspector]
    public SkillData data = null;

    public Skill(string name)
    {
        Name = name;
    }

    public void SetDataRef(SkillData data) { this.data = data; }
    public void SetDataRefFromManager() { this.data = SkillManager.Instance.AllSkills[Name]; }
    public void Update(float x)
    {
        Duration = Mathf.Max(Duration - x, 0);
        if (Duration == 0 || data.AllowActivationWhileActive) InterUseCooldown = Mathf.Max(InterUseCooldown - x, 0);
        if (data.MaxStacks >= 1 && Stacks >= data.MaxStacks) return;
        if (data.DisableAutoCooldown) return;
        Cooldown -= x;
        if (Cooldown <= 0)
        {
            Cooldown += data.Cooldown;
            GrantStacks();
            if (data.MaxStacks >= 1 && Stacks >= data.MaxStacks) Cooldown = 0;
        }
    }
    public void GrantStacks(int amnt = -1)
    {
        if (amnt < 1) amnt = data.StacksPerCooldown;
        int max = int.MaxValue;
        if (data.MaxStacks > 0) max = data.MaxStacks;
        Stacks = System.Math.Min(max, Stacks + amnt);
    }
    public void TakeStacks(int amnt)
    {
        Stacks = System.Math.Max(0, Stacks - amnt);
    }
    public bool Activate(EntityOXS ox)
    {
        if (Stacks < data.StacksPerUse) return false;
        if (Duration > 0 && !data.AllowActivationWhileActive) return false;
        if (InterUseCooldown > 0) return false;
        var b = data.OnSkillActivation.InvokeWithHitCheck(ox, this);
        if (b)
        {
            if (Stacks == data.MaxStacks)
            {
                Cooldown = data.Cooldown;
            }
            TakeStacks(data.StacksPerUse);
            InterUseCooldown = data.InterUseCooldown * CooldownMult;
            Duration = data.Duration * DurationMult;
        }
        return b;
    }
}
