using UnityEngine;

public static class Ease
{
    public static float In(float perc, float pow = 3)
    {
        return 1 - Mathf.Pow(1 - perc, pow);
    }
    public static float Out(float perc, float pow = 3)
    {
        return Mathf.Pow(perc, pow);
    }
    public static float InAndOut(float perc, float pow = 3)
    {
        //using values like 0.4 make it go fast at the start, slow down in the middle, then speed up again at the end
        if (perc <= 0.5f)
        {
            return Mathf.Pow(2 * perc, pow) / 2;
        }
        else
        {
            return (2 - Mathf.Pow(2 * (1 - perc), pow)) / 2;
        }
    }

    public static float CircIn(float perc)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(perc, 2));
    }

    public static float CircOut(float perc)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(perc - 1, 2));
    }


    public static float Bounce(float perc, int bounces = 4, float pow = 5)
    {
        var a = perc * (bounces + 0.5f);
        var x = Mathf.Abs(Mathf.Cos(Mathf.PI * a));
        x /= Mathf.Pow(pow + 1, Mathf.Floor(a + 0.5f));
        return 1 - x;
    }
    public static float Oscillate(float perc, float quantity = 4, float pow = 1)
    {
        pow *= 5;
        var x = Mathf.Cos(Mathf.PI * perc * quantity);
        x *= 1 - perc;
        x /= (pow * perc) + 1;
        return 1 - x;
    }

    public static float Elastic(float perc, float oscillations = 3)
    {
        if (perc <= 0f || perc >= 1f) return perc;
        var x = Mathf.Pow(2, -10 * perc) * Mathf.Sin((perc * oscillations - 0.5f) * Mathf.PI) + 1;
        return x;
    }

    //pow shouldn't be less than 2
    public static float Overshoot(float perc, float magnification = 2, float pow = 2)
    {
        var x = (1 - perc);
        x = Mathf.Pow(x, pow);
        var a = Mathf.Pow(magnification, pow);
        return x * a * perc * perc + (1 - x);

    }
    public static float Sin(float perc)
    {
        return Mathf.Sin(perc * Mathf.PI / 2);
    }
    public static float Cos(float perc)
    {
        return 1 - Mathf.Cos(perc * Mathf.PI / 2);
    }
    public static float SinInAndOut(float perc)
    {
        perc -= 0.5f;
        return 0.5f + (0.5f * Mathf.Sin(perc * Mathf.PI));
    }
}
