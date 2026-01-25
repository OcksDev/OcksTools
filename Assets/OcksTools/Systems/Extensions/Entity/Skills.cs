[System.Serializable]
public class SkillData
{
    public string Name;
    public float Duration;
    public float Cooldown;
    public float InterUseCooldown;
    public int MaxStacks;

}

[System.Serializable]
public class Skill
{
    public string Name;
    public int Stacks;
    public float Duration;
    public float Cooldown;
    public float InterUseCooldown;

    public void Update(float x)
    {

    }

}
