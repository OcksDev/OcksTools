using System;
using UnityEngine;

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

    public static BigAssNumber operator +(BigAssNumber left, BigAssNumber right)
    {
        if (left.Exponent > right.Exponent)
        {
            right.Mantissa /= Math.Pow(10, left.Exponent - right.Exponent);
            left.Mantissa += right.Mantissa;
            left.QuickResolveExpChange();
            return left;
        }
        else if (left.Exponent < right.Exponent)
        {
            left.Mantissa /= Math.Pow(10, right.Exponent - left.Exponent);
            right.Mantissa += left.Mantissa;
            right.QuickResolveExpChange();
            return right;
        }
        else
        {
            right.Mantissa += left.Mantissa;
            left.QuickResolveExpChange();
            return right;
        }
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
    public static BigAssNumber operator -(BigAssNumber left, BigAssNumber right)
    {
        var dif = left.Exponent - right.Exponent;

        right.Mantissa /= Math.Pow(10, dif);
        left.Mantissa -= right.Mantissa;
        left.QuickResolveExpChange();
        return left;
    }
    public bool QuickResolveExpChange()
    {
        if (Mantissa > 0)
        {
            if (Mantissa > 10)
            {
                Mantissa /= 10;
                Exponent += 1;
                return true;
            }
            else if (Mantissa < 0.1)
            {
                Mantissa *= 10;
                Exponent -= 1;
                return true;
            }
        }
        else
        {
            if (Mantissa < -10)
            {
                Mantissa /= 10;
                Exponent += 1;
                return true;
            }
            else if (Mantissa > -0.1)
            {
                Mantissa *= 10;
                Exponent -= 1;
                return true;
            }
        }
        return false;
    }

    public void RepeatedResolveExpChange()
    {
        bool s = true;
        while (s)
        {
            s = QuickResolveExpChange();
        }
    }

    public double ToDouble()
    {
        return Mantissa * Math.Pow(10, Exponent);
    }
    public void FromDouble(double num)
    {
        Exponent = (long)Math.Floor(Math.Log10(num));
        Mantissa = num / Math.Pow(10, Exponent);
    }


    public override string ToString()
    {
        return $"{Mantissa}e{Exponent}";
    }
}
public static class BigAssNumberStuff
{

    public static double Logarithm(this BigAssNumber d, double logbase)
    {
        return Math.Log(d.Mantissa,logbase) + (d.Exponent * Math.Log(10,logbase));
    }

    public static BigAssNumber Pow(this BigAssNumber d, int amnt)
    {
        d.Exponent *= amnt;
        d.Mantissa = Math.Pow(d.Mantissa, amnt);
        d.QuickResolveExpChange();
        d.QuickResolveExpChange();
        return d;
    }
}