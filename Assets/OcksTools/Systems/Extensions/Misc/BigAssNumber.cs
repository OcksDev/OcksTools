using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
[System.Serializable]
public class BigAssNumberInspectorMaker
{
    public double Mantissa;
    public Number.Magnitudes Magnitude = Number.Magnitudes.None;
    public long Exponent = -1;
    public BigAssNumber ToBigAssNumber()
    {
        if (Magnitude == Number.Magnitudes.None)
        {
            return new BigAssNumber(Mantissa, Exponent);
        }
        else
        {
            return new BigAssNumber(Mantissa, (long)Magnitude);
        }
    }


}


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
        return Mantissa * Pow10(Exponent);
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
    public static double Pow10(long exp)
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
    public static string NumToRead(this BigAssNumber d, int decimals = 0, bool shifttofitsmaller = true)
    {
        List<string> bingle = new List<string>() { "", "K", "M", "B", "T", "Qa", "Qn", "Sx", "Sp", "Oc", "No", };
        List<string> bingle2 = new List<string>() { "", "De", "Vt", "Tg", "Qt", "Qg", "St", "Sg", "Og", "Nt", };
        List<string> bingle3 = new List<string>() { "", "Ce" };
        if (d.Exponent == 0) return d.Mantissa.ToString($"F{decimals}");
        if (d.Exponent == 1) return (d.Mantissa * 10).ToString($"F{decimals}");
        if (d.Exponent == 2) return (d.Mantissa * 100).ToString($"F{decimals}");
        int off = 0;
        if (d.Exponent >= 33)
        {
            bingle[1] = "U";
            bingle[2] = "D";
            bingle.RemoveAt(3);
            off = 3;
        }
        var x = (Math.Max(d.Exponent - 3, 0) / 300);
        if (x >= 2)
        {
            return $"{d.Mantissa.ToString("F2")}e+{d.Exponent}";
        }
        var f = "F2";
        var shi = d.Exponent % 3;
        if (shifttofitsmaller) f = $"F{2 - shi}";
        string initalpart = (d.Mantissa * BigAssNumber.Pow10(shi)).ToString(f);
        string endpart = bingle[(int)((d.Exponent - off) / 3).Mod(bingle.Count)];
        endpart += bingle2[(int)(Math.Max(d.Exponent - 3, 0) / 30).Mod(bingle2.Count)];
        endpart += bingle3[(int)x.Mod(bingle3.Count)];
        return initalpart + endpart;
    }

}
public static class Number
{
    //the suffixes for large numbers
    public enum Magnitudes
    {
        None = 0,
        K = 3,
        M = 6,
        B = 9,
        T = 12,
        Qa = 15,
        Qn = 18,
        Sx = 21,
        Sp = 24,
        Oc = 27,
        No = 30,

        De = 33,
        UDe = 36,
        DDe = 39,
        TDe = 42,
        QaDe = 45,
        QnDe = 48,
        SxDe = 51,
        SpDe = 54,
        OcDe = 57,
        NoDe = 60,

        Vt = 63,
        UVt = 66,
        DVt = 69,
        TVt = 72,
        QaVt = 75,
        QnVt = 78,
        SxVt = 81,
        SpVt = 84,
        OcVt = 87,
        NoVt = 90,

        Tg = 93,
        UTg = 96,
        DTg = 99,
        TTg = 102,
        QaTg = 105,
        QnTg = 108,
        SxTg = 111,
        SpTg = 114,
        OcTg = 117,
        NoTg = 120,

        Qt = 123,
        UQt = 126,
        DQt = 129,
        TQt = 132,
        QaQt = 135,
        QnQt = 138,
        SxQt = 141,
        SpQt = 144,
        OcQt = 147,
        NoQt = 150,

        Qg = 153,
        UQg = 156,
        DQg = 159,
        TQg = 162,
        QaQg = 165,
        QnQg = 168,
        SxQg = 171,
        SpQg = 174,
        OcQg = 177,
        NoQg = 180,

        St = 183,
        USt = 186,
        DSt = 189,
        TSt = 192,
        QaSt = 195,
        QnSt = 198,
        SxSt = 201,
        SpSt = 204,
        OcSt = 207,
        NoSt = 210,

        Sg = 213,
        USg = 216,
        DSg = 219,
        TSg = 222,
        QaSg = 225,
        QnSg = 228,
        SxSg = 231,
        SpSg = 234,
        OcSg = 237,
        NoSg = 240,

        Og = 243,
        UOg = 246,
        DOg = 249,
        TOg = 252,
        QaOg = 255,
        QnOg = 258,
        SxOg = 261,
        SpOg = 264,
        OcOg = 267,
        NoOg = 270,

        Nt = 273,
        UNt = 276,
        DNt = 279,
        TNt = 282,
        QaNt = 285,
        QnNt = 288,
        SxNt = 291,
        SpNt = 294,
        OcNt = 297,
        NoNt = 300,

        Ce = 303
    }
}



public class _ConsoleForBigAssNumber
{
    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        GlobalEvent.Append("Console", BuildBigAssCums);
    }
    public static void BuildBigAssCums()
    {
        ConsoleLol.Instance.Append("test", new OXCommand("bigass").Action(ConsoleScreenShot));
    }

    public static void ConsoleScreenShot()
    {
        var d = new BigAssNumber(100);
        var e = new BigAssNumber(1000);
        var a1 = new BigAssNumber(9500);
        var a2 = new BigAssNumber(800);
        $"d-e: {d - e}".Log();
        $"{new BigAssNumber(6.5, (long)Number.Magnitudes.Qn).NumToRead()}".Log();
        $"{new BigAssNumber(6.5, (long)Number.Magnitudes.Qn).ToDouble()}".Log();
        (d + e).ToString().Log();
        (e + d).ToString().Log();
        e -= d;
        (e).ToString().Log();
        $"a1: {a1}".Log();
        $"a2: {a2}".Log();
        $"a1+a2: {a1 + a2}".Log();
        $"a1-a2: {a1 - a2}".Log();
        $"{new BigAssNumber(100).Logarithm(10)}".Log();
        $"{new BigAssNumber(100) * new BigAssNumber(100)}".Log();
        $"{new BigAssNumber(1000) / new BigAssNumber(100)}".Log();
        "----".Log();
        $"{new BigAssNumber(100000).ToDouble()}".Log();
        $"{new BigAssNumber(200000).Pow(1)}".Log();
        $"{new BigAssNumber(200000) - new BigAssNumber(200000)} = 0".Log();
        $"{new BigAssNumber(200000).Pow(2)} = {new BigAssNumber(200000).Pow(2.0)}".Log();
        $"{new BigAssNumber(200000).Pow(2.5)}".Log();
        $"{new BigAssNumber(2000).Pow(2.5).ToDouble()} = {Math.Pow(2000, 2.5)}".Log();
        $"super big thing: {new BigAssNumber(10000000).NumToRead()}".Log();
        $"super big thing: {new BigAssNumber(double.MaxValue).NumToRead()}".Log();
        $"super big thing: {new BigAssNumber(double.MaxValue).Pow(1000).NumToRead()}".Log();
    }
}