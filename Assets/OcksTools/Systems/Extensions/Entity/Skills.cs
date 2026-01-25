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
    public OXEvent<EntityOXS, Skill> OnSkillActivation = new();
}

[System.Serializable]
public class Skill
{
    public string Name = "Missing?";
    public int Stacks = 0;
    public float Duration = 0;
    public float Cooldown = 0;
    public float InterUseCooldown = 0;
    [HideInInspector]
    public SkillData data = null;

    public Skill(string name)
    {
        Name = name;
    }

    public void SetDataRef(SkillData data) { this.data = data; }
    public void SetDataRefFromManager() { this.data = SkillManager.Instance.AllSkills.Dict[Name]; }
    public void Update(float x)
    {
        InterUseCooldown = Mathf.Max(InterUseCooldown - x, 0);
        Duration = Mathf.Max(Duration - x, 0);
        Cooldown -= x;
        if (Cooldown <= 0)
        {
            if (Stacks < data.MaxStacks || data.MaxStacks < 1)
            {
                Cooldown += data.Cooldown;
                GrantStacks(1);
                Debug.Log("Granmted actrivated");
            }
            else
            {
                Cooldown = 0;
            }
        }
    }
    public void GrantStacks(int amnt)
    {
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
        Debug.Log("actitgi actrivated");
        TakeStacks(data.StacksPerUse);
        return data.OnSkillActivation.InvokeWithHitCheck(ox, this);
    }
}
