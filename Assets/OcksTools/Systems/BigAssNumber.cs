using System;
using System.Runtime.CompilerServices;

// max value is 9.999*10^(2^63)
[System.Serializable]
public struct BigAssNumber
{
    public double Mantissa;
    public long Exponent;

    public BigAssNumber(double value)
    {
        Mantissa = 0;
        Exponent = 0;
        FromDouble(value);
    }
    public BigAssNumber(double man, long exp)
    {
        Mantissa = man;
        Exponent = exp;
    }
    public BigAssNumber(BigAssNumber d)
    {
        Mantissa = d.Mantissa;
        Exponent = d.Exponent;
    }

    public double ToDouble()
    {
        return Mantissa * Math.Pow(10, Exponent);
    }

    public void FromDouble(double num)
    {
        Exponent = (long)Math.Log10(num);
        Mantissa = num / Pow10(Exponent);
    }

    public override string ToString()
    {
        return $"{Mantissa}e{Exponent}";
    }

    public static BigAssNumber FromString(string a)
    {
        var split = a.Split('e');
        var d = new BigAssNumber(double.Parse(split[0]), long.Parse(split[1]));
        return d;
    }

    public static BigAssNumber FromDoubleAsString(string a)
    {
        var d = new BigAssNumber(1, a.Length - 1);
        d.Mantissa = double.Parse(a.Substring(0, 1) + "." + a.Substring(1));
        return d;
    }



    public static BigAssNumber operator +(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            a.Mantissa += b.Mantissa;
            a.QuickResolveExpChange();
            return a;
        }

        if (a.Exponent < b.Exponent)
        {
            var t = a; a = b; b = t;
        }

        long diff = a.Exponent - b.Exponent;

        b.Mantissa /= Pow10(diff);
        a.Mantissa += b.Mantissa;

        a.QuickResolveExpChange();
        return a;
    }

    public static BigAssNumber operator *(BigAssNumber left, BigAssNumber right)
    {
        left.Mantissa *= right.Mantissa;
        left.Exponent += right.Exponent;
        left.QuickResolveExpChange();
        left.QuickResolveExpChange();
        return left;
    }
    public static BigAssNumber operator /(BigAssNumber left, BigAssNumber right)
    {
        left.Mantissa /= right.Mantissa;
        left.Exponent -= right.Exponent;
        left.QuickResolveExpChange();
        left.QuickResolveExpChange();
        return left;
    }
    public static BigAssNumber operator -(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            a.Mantissa -= b.Mantissa;
            a.QuickResolveExpChange();
            return a;
        }

        if (a.Exponent < b.Exponent)
        {
            var t = a; a = b; b = t;
        }

        long diff = a.Exponent - b.Exponent;

        b.Mantissa /= Pow10(diff);
        a.Mantissa -= b.Mantissa;

        a.QuickResolveExpChange();
        return a;
    }


    public bool QuickResolveExpChange()
    {
        if (Mantissa == 0)
        {
            Exponent = 0;
            return false;
        }
        var d = Math.Abs(Mantissa);
        if (d >= 10)
        {
            Mantissa /= 10;
            Exponent += 1;
            return true;
        }
        else if (d < 1)
        {
            Mantissa *= 10;
            Exponent -= 1;
            return true;
        }
        return false;
    }


    public void FullResolveExpChange()
    {
        double abs = Math.Abs(Mantissa);

        // Already normalized
        if (abs >= 1 && abs < 10)
            return;

        int shift = (int)Math.Floor(Math.Log10(abs));

        Mantissa /= Pow10(shift);
        Exponent += shift;
    }


    private static readonly double[] Pow10Cache =
{
    1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9,
    1e10,1e11,1e12,1e13,1e14,1e15,1e16,1e17,1e18,1e19,
    1e20,1e21,1e22,1e23,1e24,1e25,1e26,1e27,1e28,1e29,
    1e30,1e31,1e32,1e33,1e34,1e35,1e36,1e37,1e38,1e39,
    1e40,1e41,1e42,1e43,1e44,1e45,1e46,1e47,1e48,1e49,
    1e50,1e51,1e52,1e53,1e54,1e55,1e56,1e57,1e58,1e59,
    1e60,1e61,1e62,1e63,1e64,1e65,1e66,1e67,1e68,1e69,
    1e70,1e71,1e72,1e73,1e74,1e75,1e76,1e77,1e78,1e79,
    1e80,1e81,1e82,1e83,1e84,1e85,1e86,1e87,1e88,1e89,
    1e90,1e91,1e92,1e93,1e94,1e95,1e96,1e97,1e98,1e99,
    1e100
};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Pow10(long exp)
    {
        if (exp >= 0 && exp < Pow10Cache.Length)
            return Pow10Cache[exp];
        return Math.Pow(10.0, exp);
    }
    public override bool Equals(object obj)
    {
        return obj is BigAssNumber number &&
               Mantissa == number.Mantissa &&
               Exponent == number.Exponent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }

    public static bool operator >(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            return a.Mantissa > b.Mantissa;
        }
        return a.Exponent > b.Exponent;
    }

    public static bool operator <(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            return a.Mantissa < b.Mantissa;
        }
        return a.Exponent < b.Exponent;
    }

    public static bool operator >=(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            return a.Mantissa >= b.Mantissa;
        }
        return a.Exponent >= b.Exponent;
    }

    public static bool operator <=(BigAssNumber a, BigAssNumber b)
    {
        if (a.Exponent == b.Exponent)
        {
            return a.Mantissa <= b.Mantissa;
        }
        return a.Exponent <= b.Exponent;
    }
    public static bool operator ==(BigAssNumber a, BigAssNumber b)
    {
        return a.Exponent == b.Exponent && a.Mantissa == b.Mantissa;
    }
    public static bool operator !=(BigAssNumber a, BigAssNumber b)
    {
        return a.Exponent != b.Exponent || a.Mantissa != b.Mantissa;
    }
}
public static class BigAssNumberStuff
{

    public static double Logarithm(this BigAssNumber d, double logbase)
    {
        return Math.Log(d.Mantissa, logbase) + (d.Exponent * Math.Log(10, logbase));
    }

    public static BigAssNumber Pow(this BigAssNumber d, int amnt)
    {
        d.Exponent *= amnt;
        d.Mantissa = Math.Pow(d.Mantissa, amnt);
        d.FullResolveExpChange();
        return d;
    }

    public static BigAssNumber Pow(this BigAssNumber d, double amnt)
    {
        d.Mantissa = Math.Pow(d.Mantissa, amnt);
        var dingle = d.Exponent * amnt;
        d.Exponent = (long)dingle;
        d.Mantissa *= Math.Pow(10, dingle % 1);
        d.FullResolveExpChange();
        return d;
    }
}