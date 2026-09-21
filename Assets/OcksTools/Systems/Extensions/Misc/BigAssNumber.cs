using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
[System.Serializable]
public class BigAssNumberInspectorMaker
{
    public double Mantissa;
    public Magnitudes Magnitude = Magnitudes.None;
    public long Exponent = -1;
    public BigAssNumber ToBigAssNumber()
    {
        if (Magnitude == Magnitudes.None)
        {
            return new BigAssNumber(Mantissa, Exponent, true);
        }
        else
        {
            return new BigAssNumber(Mantissa, (long)Magnitude, true);
        }
    }


}


// max value is 9.999*10^(2^63)
//
// SPECIAL VALUES: 
//     0e1  -> +Infinity
//     0e0  -> 0
//     0e-1 -> -Infinity
//     0e-2 -> NaN
[System.Serializable]
public struct BigAssNumber : IComparable<BigAssNumber>
{
    public double Mantissa;
    public long Exponent;

    public static readonly BigAssNumber Zero = default;
    public static readonly BigAssNumber One = new BigAssNumber(1, 0);
    public static readonly BigAssNumber Two = new BigAssNumber(2, 0);
    public static readonly BigAssNumber Ten = new BigAssNumber(1, 1);
    public static readonly BigAssNumber PositiveInfinity = Special(PosInfExp);
    public static readonly BigAssNumber NegativeInfinity = Special(NegInfExp);
    public static readonly BigAssNumber NaN = Special(NaNExp);
    public static readonly BigAssNumber Max = new BigAssNumber(9.99999999999999999999, long.MaxValue);
    public static readonly BigAssNumber Min = new BigAssNumber(1, long.MinValue);

    public bool IsZero => Mantissa == 0 && Exponent == ZeroExp;
    public bool IsNaN => Mantissa == 0 && Exponent == NaNExp;
    public bool IsPositiveInfinity => Mantissa == 0 && Exponent == PosInfExp;
    public bool IsNegativeInfinity => Mantissa == 0 && Exponent == NegInfExp;
    public bool IsInfinity => Mantissa == 0 && (Exponent == PosInfExp || Exponent == NegInfExp);
    /// <summary>True for any number that is not NaN or +-Infinity (zero counts as finite, same as double).</summary>
    public bool IsFinite => Mantissa != 0 || Exponent == ZeroExp;
    /// <summary>True for 0, +-Infinity and NaN, i.e. anything stored with Mantissa == 0.</summary>
    public bool IsSpecial => Mantissa == 0;
    public BigAssNumber(double value)
    {
        Mantissa = 0;
        Exponent = 0;
        FromDouble(value);
    }
    /// <summary>Trusted fast constructor: assumes a finite mantissa (no NaN/Inf) already in [1,10) or 0. Use the 3-arg version to normalize.</summary>
    public BigAssNumber(double man, long exp)
    {
        Mantissa = man;
        Exponent = exp;
        if (man == 0) Exponent = SanitizeZeroExp(exp);
    }
    public BigAssNumber(double man, long exp, bool checkedexp = false)
    {
        Mantissa = man;
        Exponent = exp;
        FullResolveExpChange();
    }
    public BigAssNumber(BigAssNumber d)
    {
        Mantissa = d.Mantissa;
        Exponent = d.Exponent;
    }
    public double ToDouble()
    {
        if (Mantissa == 0) return SpecialDoubles[(int)(Exponent + 2)];
        return Mantissa * Pow10(Exponent);
    }

    public BigAssNumber FromDouble(double num)
    {
        if (num == 0)
        {
            Mantissa = 0;
            Exponent = ZeroExp;
            return this;
        }
        if (double.IsNaN(num) || double.IsInfinity(num))
        {
            Mantissa = 0;
            Exponent = num != num ? NaNExp : (num > 0 ? PosInfExp : NegInfExp);
            return this;
        }
        Exponent = (long)Math.Floor(Math.Log10(Math.Abs(num)));
        Mantissa = num / Pow10(Exponent);
        QuickResolveNonZero();
        return this;
    }

    public override string ToString()
    {
        return $"{Mantissa}E{Exponent}";
    }

    public static BigAssNumber FromString(string a)
    {
        var split = a.Split('E');
        var d = new BigAssNumber(double.Parse(split[0]), long.Parse(split[1]));
        return d;
    }

    public static BigAssNumber FromDoubleAsString(string a)
    {
        var d = new BigAssNumber(1, a.Length - 1);
        d.Mantissa = double.Parse(a.Substring(0, 1) + "." + a.Substring(1));
        return d;
    }


    public static BigAssNumber operator -(BigAssNumber a)
    {
        if (a.Mantissa == 0)
        {
            // only +Inf (1) and -Inf (-1) flip; both have the low bit set. 0 and NaN (0, -2) do not.
            if ((a.Exponent & 1) != 0) a.Exponent = -a.Exponent;
        }
        else
        {
            a.Mantissa = -a.Mantissa;
        }
        return a;
    }

    public static BigAssNumber operator *(BigAssNumber left, BigAssNumber right)
    {
        // finite mantissas are always |m| >= 1, so the product is 0 only if one side is a special. No extra check needed.
        double m = left.Mantissa * right.Mantissa;
        if (m == 0) return FromSpecialDouble(left.ClassValue * right.ClassValue);
        long l = left.Exponent, r = right.Exponent;
        long e = unchecked(l + r);
        // signed overflow: both operands have the same sign and the sum has the other one
        if (((l ^ e) & (r ^ e)) < 0) return FromOverflow(r > 0, m < 0);
        left.Mantissa = m;
        left.Exponent = e;
        left.QuickResolveNonZero(); // |m| is in [1,100) so one pass is enough
        return left;
    }
    public static BigAssNumber operator /(BigAssNumber left, BigAssNumber right)
    {
        if (left.Mantissa == 0 || right.Mantissa == 0) return FromSpecialDouble(left.ClassValue / right.ClassValue);
        double m = left.Mantissa / right.Mantissa;
        long l = left.Exponent, r = right.Exponent;
        long e = unchecked(l - r);
        // signed overflow: operands have different signs and the difference has the sign of the right one
        if (((l ^ r) & (l ^ e)) < 0) return FromOverflow(r < 0, m < 0);
        left.Mantissa = m;
        left.Exponent = e;
        left.QuickResolveNonZero(); // |m| is in (0.1,10) so one pass is enough
        return left;
    }
    public static BigAssNumber operator +(BigAssNumber a, BigAssNumber b)
    {
        if (a.Mantissa == 0 || b.Mantissa == 0) return AddSpecial(a, b);

        if (a.Exponent == b.Exponent)
        {
            a.Mantissa += b.Mantissa;
            a.ResolveAfterAddSub();
            return a;
        }

        if (a.Exponent < b.Exponent)
        {
            var t = a; a = b; b = t;
        }

        // a.Exponent > b.Exponent here, so the exact difference always fits in a ulong (a long subtraction could wrap)
        ulong diff = unchecked((ulong)a.Exponent - (ulong)b.Exponent);

        if (diff > 20UL) return a;

        b.Mantissa /= Pow10((long)diff);
        a.Mantissa += b.Mantissa;

        a.ResolveAfterAddSub();
        return a;
    }
    public static BigAssNumber operator -(BigAssNumber a, BigAssNumber b)
    {
        if (a.Mantissa == 0 || b.Mantissa == 0) return AddSpecial(a, -b);

        if (a.Exponent == b.Exponent)
        {
            a.Mantissa -= b.Mantissa;
            a.ResolveAfterAddSub();
            return a;
        }

        if (a.Exponent < b.Exponent)
        {
            var t = a; a = b; b = t;
        }

        // a.Exponent > b.Exponent here, so the exact difference always fits in a ulong (a long subtraction could wrap)
        ulong diff = unchecked((ulong)a.Exponent - (ulong)b.Exponent);

        if (diff > 20UL) return a;

        b.Mantissa /= Pow10((long)diff);
        a.Mantissa -= b.Mantissa;

        a.ResolveAfterAddSub();
        return a;
    }

    /// <summary>Slow path for a + b when at least one operand has Mantissa == 0.</summary>
    private static BigAssNumber AddSpecial(BigAssNumber a, BigAssNumber b)
    {
        if (a.Mantissa != 0) return b.Exponent == ZeroExp ? a : b; // finite + 0 = finite, finite + Inf/NaN = Inf/NaN
        if (b.Mantissa != 0) return a.Exponent == ZeroExp ? b : a;
        // both special: let IEEE rules decide (Inf + -Inf = NaN, NaN + x = NaN, 0 + 0 = 0, ...)
        return FromSpecialDouble(SpecialDoubles[(int)(a.Exponent + 2)] + SpecialDoubles[(int)(b.Exponent + 2)]);
    }

    /// <summary>After add/sub the mantissa may have cancelled to exactly 0, which must be plain 0 (0e0), not a leftover exponent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResolveAfterAddSub()
    {
        if (Mantissa == 0) Exponent = ZeroExp;
        else QuickResolveNonZero();
    }

    /// <summary>Public normalizer. Does nothing on 0 / Infinity / NaN, so it can never destroy a special value.</summary>
    public bool QuickResolveExpChange()
    {
        if (Mantissa == 0) return false;
        return QuickResolveNonZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool QuickResolveNonZero()
    {
        var d = Math.Abs(Mantissa);
        if (d >= 10)
        {
            if (Exponent == long.MaxValue) { this = FromOverflow(true, Mantissa < 0); return true; }
            Mantissa /= 10;
            Exponent += 1;
            return true;
        }
        else if (d < 1)
        {
            if (Exponent == long.MinValue) { this = FromOverflow(false, false); return true; }
            Mantissa *= 10;
            Exponent -= 1;
            return true;
        }
        return false;
    }


    public void FullResolveExpChange()
    {
        double abs = Math.Abs(Mantissa);

        // Already normalized (hot path, NaN fails both comparisons and falls through)
        if (abs >= 1 && abs < 10)
            return;

        if (abs == 0)
        {
            Exponent = SanitizeZeroExp(Exponent);
            return;
        }
        if (double.IsNaN(abs))
        {
            Mantissa = 0;
            Exponent = NaNExp;
            return;
        }
        if (double.IsInfinity(abs))
        {
            Exponent = Mantissa > 0 ? PosInfExp : NegInfExp;
            Mantissa = 0;
            return;
        }

        int shift = (int)Math.Floor(Math.Log10(abs));

        // Exponent + shift would leave the long range
        if (shift > 0 ? Exponent > long.MaxValue - shift : Exponent < long.MinValue - shift)
        {
            this = FromOverflow(shift > 0, Mantissa < 0);
            return;
        }

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
        if (exp < 0 && -exp < Pow10Cache.Length)
            return 1.0 / Pow10Cache[-exp];
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
    public static bool operator >(BigAssNumber a, BigAssNumber b) => a.CompareTo(b) > 0;
    public static bool operator <(BigAssNumber a, BigAssNumber b) => a.CompareTo(b) < 0;
    public static bool operator >=(BigAssNumber a, BigAssNumber b) => a.CompareTo(b) >= 0;
    public static bool operator <=(BigAssNumber a, BigAssNumber b) => a.CompareTo(b) <= 0;

    /// <summary>
    /// Total order, same as double.CompareTo: NaN &lt; -Inf &lt; negatives &lt; 0 &lt; positives &lt; +Inf, and NaN equals NaN.
    /// </summary>
    public int CompareTo(BigAssNumber other)
    {
        // slow path: at least one side is 0 / Inf / NaN. Compare by tier; two specials in the same tier are equal.
        if (Mantissa == 0 || other.Mantissa == 0)
        {
            int ta = Mantissa == 0 ? SpecialTier[(int)(Exponent + 2)] : (Mantissa < 0 ? 2 : 4);
            int tb = other.Mantissa == 0 ? SpecialTier[(int)(other.Exponent + 2)] : (other.Mantissa < 0 ? 2 : 4);
            return ta - tb;
        }

        bool aNegative = Mantissa < 0;
        bool bNegative = other.Mantissa < 0;

        if (aNegative != bNegative)
        {
            return aNegative ? -1 : 1;
        }

        int exponentComparison = Exponent.CompareTo(other.Exponent);
        if (exponentComparison != 0)
        {
            return aNegative ? -exponentComparison : exponentComparison;
        }

        return Mantissa.CompareTo(other.Mantissa);
    }

    // NOTE: like double.Equals, NaN == NaN is true here (representation equality), unlike the IEEE == operator on double.
    public static bool operator ==(BigAssNumber a, BigAssNumber b)
    {
        return a.Exponent == b.Exponent && a.Mantissa == b.Mantissa;
    }
    public static bool operator !=(BigAssNumber a, BigAssNumber b)
    {
        return a.Exponent != b.Exponent || a.Mantissa != b.Mantissa;
    }



    // ---- special value tags (stored in Exponent when Mantissa == 0) ----
    private const long ZeroExp = 0;
    private const long PosInfExp = 1;
    private const long NegInfExp = -1;
    private const long NaNExp = -2;

    // Indexed by (Exponent + 2). Valid for any Mantissa == 0 number.
    private static readonly double[] SpecialDoubles = { double.NaN, double.NegativeInfinity, 0.0, double.PositiveInfinity };

    // Ordering tiers, indexed by (Exponent + 2): NaN < -Inf < (negatives) < 0 < (positives) < +Inf
    // tiers: NaN=0, -Inf=1, negative=2, zero=3, positive=4, +Inf=5
    private static readonly int[] SpecialTier = { 0, 1, 3, 5 };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BigAssNumber Special(long tag)
    {
        var r = default(BigAssNumber);
        r.Exponent = tag;
        return r;
    }

    /// <summary>
    /// Result of an exponent that left the long range. Upward overflow becomes +-Infinity (sign of the mantissa), downward overflow (underflow) becomes 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static BigAssNumber FromOverflow(bool up, bool negative)
    {
        return up ? Special(negative ? NegInfExp : PosInfExp) : default;
    }

    /// <summary>Maps a double that is 0, +-Inf or NaN (or any double, by sign) to the special representation.</summary>
    private static BigAssNumber FromSpecialDouble(double d)
    {
        if (d != d) return Special(NaNExp);
        if (d == 0) return default;
        return Special(d > 0 ? PosInfExp : NegInfExp);
    }

    /// <summary>Value used to resolve special-case arithmetic through plain double rules: the special's own double, or +-1 for a finite number.</summary>
    private double ClassValue => Mantissa == 0 ? SpecialDoubles[(int)(Exponent + 2)] : (Mantissa < 0 ? -1.0 : 1.0);

    /// <summary>Zero mantissa may only carry exponents -2..1. Anything else collapses to plain 0.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long SanitizeZeroExp(long exp)
    {
        return unchecked((ulong)(exp + 2)) > 3UL ? ZeroExp : exp;
    }



    public static implicit operator double(BigAssNumber a)
    {
        return a.ToDouble();
    }

}
public static class BigAssNumberStuff
{

    public static double Logarithm(this BigAssNumber d, double logbase)
    {
        // 0 -> -Inf, +Inf -> +Inf, -Inf / NaN -> NaN, all straight from Math.Log
        if (d.Mantissa == 0) return Math.Log(d.ToDouble(), logbase);
        return Math.Log(d.Mantissa, logbase) + (d.Exponent * Math.Log(10, logbase));
    }

    private const double TwoPow63 = 9.2233720368547758E18; // first double outside the long range

    /// <summary>True if a * b does not fit in a long. Exact, no exceptions, no division by zero.</summary>
    private static bool MulOverflows(long a, int b)
    {
        if (a == 0 || b == 0) return false;
        ulong ua = a < 0 ? (ulong)(-(a + 1)) + 1UL : (ulong)a; // |a|, safe for long.MinValue
        ulong ub = b < 0 ? (ulong)(-(long)b) : (ulong)b;
        // a negative product may reach -2^63, a positive one only 2^63-1
        ulong limit = ((a < 0) != (b < 0)) ? 0x8000000000000000UL : (ulong)long.MaxValue;
        return ua > limit / ub;
    }
    public static BigAssNumber Pow(this BigAssNumber d, int amnt)
    {
        // specials follow IEEE pow rules: 0^0 = 1, 0^-n = +Inf, Inf^-n = 0, (-Inf)^odd = -Inf, ...
        if (d.Mantissa == 0) return new BigAssNumber(Math.Pow(d.ToDouble(), amnt));

        bool neg = d.Mantissa < 0 && (amnt & 1) != 0;
        long ex = d.Exponent;
        long e = unchecked(ex * amnt);
        // |amnt| always fits in 32 bits, so the exact (slower) overflow test is only needed for exponents that do not
        if (unchecked((ulong)(ex + 0x80000000L)) > 0xFFFFFFFFUL && MulOverflows(ex, amnt))
            return BigAssNumber.FromOverflow((ex < 0) == (amnt < 0), neg);

        double pm = Math.Pow(d.Mantissa, amnt);
        if (pm == 0 || double.IsInfinity(pm))
        {
            // mantissa^amnt is outside the double range, so move the excess into the exponent via log10
            double l = Math.Log10(Math.Abs(d.Mantissa)) * amnt;
            double fl = Math.Floor(l);
            pm = Math.Pow(10, l - fl);
            if (neg) pm = -pm;
            long s = (long)fl;
            long e2 = unchecked(e + s);
            if (((e ^ e2) & (s ^ e2)) < 0) return BigAssNumber.FromOverflow(s > 0, neg);
            e = e2;
        }

        d.Mantissa = pm;
        d.Exponent = e;
        d.FullResolveExpChange();
        return d;
    }

    public static BigAssNumber Pow(this BigAssNumber d, double amnt)
    {
        if (d.Mantissa == 0) return new BigAssNumber(Math.Pow(d.ToDouble(), amnt));

        double ex = d.Exponent * amnt;
        // one test catches: exponent overflow, amnt = +-Infinity and amnt = NaN (NaN fails both comparisons)
        if (!(ex > -TwoPow63 && ex < TwoPow63)) return PowOutOfRange(d, amnt, ex);

        double pm = Math.Pow(d.Mantissa, amnt);
        if (pm == 0 || double.IsInfinity(pm))
        {
            // mantissa^amnt is outside the double range, so move the excess into the exponent via log10
            bool neg = pm < 0 || (1.0 / pm < 0); // sign of +-0 / +-Inf
            double l = Math.Log10(Math.Abs(d.Mantissa)) * amnt;
            double fl = Math.Floor(l);
            pm = Math.Pow(10, l - fl);
            if (neg) pm = -pm;
            ex += fl;
            if (!(ex > -TwoPow63 && ex < TwoPow63)) return BigAssNumber.FromOverflow(ex > 0, neg);
        }

        d.Mantissa = pm;
        d.Exponent = (long)ex;
        d.Mantissa *= Math.Pow(10, ex % 1);
        d.FullResolveExpChange();
        return d;
    }

    /// <summary>Slow path of Pow(double): the new exponent is too big for a long, or amnt is NaN / +-Infinity.</summary>
    private static BigAssNumber PowOutOfRange(BigAssNumber d, double amnt, double ex)
    {
        double m = Math.Pow(d.Mantissa, amnt); // gives the sign / NaN, and is the whole answer when Exponent == 0 and amnt is infinite
        if (m != m) return BigAssNumber.NaN;
        bool neg = m < 0 || (m == 0 && 1.0 / m < 0);
        if (ex >= TwoPow63) return BigAssNumber.FromOverflow(true, neg);
        if (ex <= -TwoPow63) return BigAssNumber.FromOverflow(false, neg);
        return new BigAssNumber(m); // ex is NaN: 0 * Infinity, so the exponent is 0 and only the mantissa power remains
    }
    public static string NumToRead(this BigAssNumber d, int decimals = 0, bool shifttofitsmaller = true)
    {
        if (d.Mantissa == 0)
        {
            switch (d.Exponent)
            {
                case 1: return "Infinity";
                case -1: return "-Infinity";
                case -2: return "NaN";
                default: return 0.0.ToString($"F{decimals}");
            }
        }
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
            return $"{d.Mantissa.ToString("F2")}E{d.Exponent.ToString().NumToRead()}";
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



public class _ConsoleForBigAssNumber
{
    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Append("test", new OXCommand("bigass").Action(ConsoleScreenShot));
        });
    }

    public static void ConsoleScreenShot()
    {
        var d = new BigAssNumber(100);
        var e = new BigAssNumber(1000);
        var a1 = new BigAssNumber(9500);
        var a2 = new BigAssNumber(800);
        $"d-e: {d - e}".Log();
        $"{new BigAssNumber(6.5, (long)Magnitudes.Qn).NumToRead()}".Log();
        $"{new BigAssNumber(6.5, (long)Magnitudes.Qn).ToDouble()}".Log();
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
        $"big thing: {new BigAssNumber(10000000).NumToRead()}".Log();
        $"big thing 2: {new BigAssNumber(100, (long)Magnitudes.DDe, true).NumToRead()}".Log();
        $"bigger thing: {new BigAssNumber(double.MaxValue).NumToRead()}".Log();
        $"super big thing: {new BigAssNumber(double.MaxValue).Pow(1000).NumToRead()}".Log();
        $"max-est big thing: {BigAssNumber.Max.NumToRead()}".Log();
        "---- specials ----".Log();
        var inf = BigAssNumber.PositiveInfinity;
        var ninf = BigAssNumber.NegativeInfinity;
        var nan = BigAssNumber.NaN;
        var zero = BigAssNumber.Zero;
        $"inf: {inf.NumToRead()}  -inf: {ninf.NumToRead()}  nan: {nan.NumToRead()}  zero: {zero.NumToRead()}".Log();
        $"inf+inf={(inf + inf).NumToRead()} (Infinity)".Log();
        $"inf-inf={(inf - inf).NumToRead()} (NaN)".Log();
        $"inf*0={(inf * zero).NumToRead()} (NaN)".Log();
        $"1/0={(new BigAssNumber(1) / zero).NumToRead()} (Infinity)".Log();
        $"-1/0={(new BigAssNumber(-1) / zero).NumToRead()} (-Infinity)".Log();
        $"0/0={(zero / zero).NumToRead()} (NaN)".Log();
        $"5+0={(new BigAssNumber(5) + zero).NumToRead()} (5)".Log();
        $"5-inf={(new BigAssNumber(5) - inf).NumToRead()} (-Infinity)".Log();
        $"0-5={(zero - new BigAssNumber(5)).NumToRead()} (-5)".Log();
        $"5-5={(new BigAssNumber(5) - new BigAssNumber(5))} (0E0)".Log();
        $"-inf*-5={(ninf * new BigAssNumber(-5)).NumToRead()} (Infinity)".Log();
        $"5/inf={(new BigAssNumber(5) / inf).NumToRead()} (0)".Log();
        $"-(inf)={(-inf).NumToRead()} (-Infinity)  -(nan)={(-nan).NumToRead()} (NaN)".Log();
        $"nan<1: {nan < new BigAssNumber(1)}  -inf<-1e9: {ninf < new BigAssNumber(-1e9)}  1e9<inf: {new BigAssNumber(1e9) < inf}  0<1: {zero < new BigAssNumber(1)}".Log();
        $"0^0={zero.Pow(0).NumToRead()} (1)  0^-1={zero.Pow(-1).NumToRead()} (Infinity)".Log();
        "---- overflow ----".Log();
        var huge = new BigAssNumber(5, long.MaxValue - 1);
        $"huge*huge={(huge * huge).NumToRead()} (Infinity)".Log();
        $"-huge*huge={((-huge) * huge).NumToRead()} (-Infinity)".Log();
        var tiny = new BigAssNumber(5, long.MinValue + 1);
        $"tiny*tiny={(tiny * tiny).NumToRead()} (0)".Log();
        $"huge/tiny={(huge / tiny).NumToRead()} (Infinity)".Log();
        $"tiny/huge={(tiny / huge).NumToRead()} (0)".Log();
        $"Max+Max={(BigAssNumber.Max + BigAssNumber.Max).NumToRead()} (Infinity)".Log();
        $"Max+Min={(BigAssNumber.Max + BigAssNumber.Min).NumToRead()} (Max, no garbage)".Log();
        $"huge^2={huge.Pow(2).NumToRead()} (Infinity)  huge^-2={huge.Pow(-2).NumToRead()} (0)  huge^2.5={huge.Pow(2.5).NumToRead()} (Infinity)".Log();
        $"9.9e0^1000={new BigAssNumber(9.9).Pow(1000)} (~E995, not Infinity)".Log();
        $"2^inf={new BigAssNumber(2).Pow(double.PositiveInfinity).NumToRead()} (Infinity)  2^-inf={new BigAssNumber(2).Pow(double.NegativeInfinity).NumToRead()} (0)  2^nan={new BigAssNumber(2).Pow(double.NaN).NumToRead()} (NaN)".Log();
        $"double->big: {new BigAssNumber(double.PositiveInfinity).NumToRead()} {new BigAssNumber(double.NaN).NumToRead()} {new BigAssNumber(0.0).NumToRead()} {new BigAssNumber(0.5)}".Log();

    }
}
