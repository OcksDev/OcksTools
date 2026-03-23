public class CriticalChance
{
    public int Degree = -1;
    public float Chance;
    protected int seed = 0;
    public CriticalChance(float chance)
    {
        this.Chance = chance;
        seed = UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
    }
    public CriticalChance(CriticalChance crit)
    {
        this.Chance = crit.Chance;
        this.Degree = crit.Degree;
        this.seed = crit.seed;
    }

    public int GetDegree()
    {
        RollDegree();
        return this.Degree;
    }

    public void RollDegree()
    {
        if (Degree > -1) return;
        if (Chance <= 0)
        {
            Degree = 0;
            return;
        }
        Degree = (int)Chance;
        if (new System.Random(seed).NextDouble() < (Chance % 1))
        {
            Degree++;
        }
    }
    public void FromString(string str)
    {
        var e = str.Split(":");
        Degree = int.Parse(e[0]);
        Chance = float.Parse(e[1]);
        seed = int.Parse(e[2]);
    }
    public override string ToString()
    {
        return $"{Degree}:{Chance}:{seed}";
    }
}
