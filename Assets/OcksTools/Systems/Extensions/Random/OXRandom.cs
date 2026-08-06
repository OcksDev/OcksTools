using System;
using System.Linq;
using System.Runtime.InteropServices;

public class OXRandom : IOXFile_SaveLoadable<OXRandom>
{
    protected ulong[] _state;
    public OXRandom()
    {
        var q = new byte[8];
        new System.Random().NextBytes(q);
        SetState((ulong)BitConverter.ToInt64(q, 0));
    }
    public OXRandom(long seed) => SetState((ulong)seed);
    public OXRandom(ulong seed) => SetState(seed);
    public OXRandom(string load) => FromString(load);
    public OXRandom(ulong[] state) => _state = state;
    public OXRandom(OXRandom other) => _state = (ulong[])other._state.Clone();
    public void SetState(ulong seed)
    {
        _state = xorshift256_init(seed);
    }
    /// <returns>[0, ulong.MaxValue]</returns>
    public ulong Next() => xoshiro256p(_state);

    /// <returns>[0, max)</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ulong Next(ulong max)
    {
        if (max <= 0)
        {
            if (max == 0) return 0;
            throw new ArgumentOutOfRangeException(nameof(max), "max must 0 or greater");
        }

        // Rejection sampling to avoid modulo bias.
        ulong limit = ulong.MaxValue - (ulong.MaxValue % max);

        ulong r;
        do
        {
            r = xoshiro256p(_state);
        } while (r >= limit);

        return r % max;
    }
    public void Mutate(ulong x)
    {
        _state[0] ^= x;
        _state[1] ^= splitmix64(_state[0]);
        _state[2] ^= splitmix64(_state[1]);
        _state[3] ^= splitmix64(_state[2]);
        xoshiro256p(_state);
    }

    /// <returns>[min, max)</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ulong Next(ulong min, ulong max)
    {
        if (min >= max)
        {
            if (min == max) return min;
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        }

        return min + Next(max - min);
    }
    /// <returns>[0, max)</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Next(int max)
    {
        if (max <= 0)
        {
            if (max == 0) return 0;
            throw new ArgumentOutOfRangeException(nameof(max), "max must 0 or greater");
        }


        return (int)Next((ulong)max);
    }

    /// <returns>[min, max)</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Next(int min, int max)
    {
        if (min >= max)
        {
            if (min == max) return min;
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        }


        return min + Next(max - min);
    }
    /// <returns>[0.0, 1.0)</returns>
    public double NextDouble()
    {
        return (xoshiro256p(_state) >> 11) * (1.0 / (1UL << 53));
    }
    /// <returns>[0.0, 1.0)</returns>
    public float NextFloat()
    {
        return (float)NextDouble();
    }

    /// <returns>(x,y) where axis range from -1 to 1</returns>
    public (double x, double y) NextInSquare()
    {
        return (NextDouble() * 2 - 1, NextDouble() * 2 - 1);
    }

    /// <returns>(x,y,z) where axis range from -1 to 1</returns>
    public (double x, double y, double z) NextInCube()
    {
        return (NextDouble() * 2 - 1, NextDouble() * 2 - 1, NextDouble() * 2 - 1);
    }

    /// <returns>(x,y) where axis range from -1 to 1</returns>
    public (double x, double y) NextInCircle()
    {
        double angle = NextDouble() * (Math.PI * 2.0);
        double radius = Math.Sqrt(NextDouble());

        return (
            Math.Cos(angle) * radius,
            Math.Sin(angle) * radius
        );
    }
    /// <returns>(x,y,z) where axis range from -1 to 1</returns>
    public (double x, double y, double z) NextInSphere()
    {
        double z = NextDouble() * 2.0 - 1.0;
        double theta = NextDouble() * (Math.PI * 2.0);
        double radius = Math.Cbrt(NextDouble());

        double xy = Math.Sqrt(1.0 - z * z);

        return (
            radius * xy * Math.Cos(theta),
            radius * xy * Math.Sin(theta),
            radius * z
        );
    }
    /// <summary>
    /// Fills given byte[] with random data
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public void NextBytes(byte[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        int i = 0;

        while (i + 8 <= buffer.Length)
        {
            ulong value = xoshiro256p(_state);

            buffer[i++] = (byte)value;
            buffer[i++] = (byte)(value >> 8);
            buffer[i++] = (byte)(value >> 16);
            buffer[i++] = (byte)(value >> 24);
            buffer[i++] = (byte)(value >> 32);
            buffer[i++] = (byte)(value >> 40);
            buffer[i++] = (byte)(value >> 48);
            buffer[i++] = (byte)(value >> 56);
        }

        if (i < buffer.Length)
        {
            ulong value = xoshiro256p(_state);

            while (i < buffer.Length)
            {
                buffer[i++] = (byte)value;
                value >>= 8;
            }
        }
    }



    //implementation of xoshiro256** algorithm
    private static ulong splitmix64(ulong state)
    {
        state = state + 0x9E3779B97f4A7C15;
        state = (state ^ (state >> 30)) * 0xBF58476D1CE4E5B9;
        state = (state ^ (state >> 27)) * 0x94D049BB133111EB;
        return state ^ (state >> 31);
    }

    private static ulong[] xorshift256_init(ulong seed)
    {
        var result = new ulong[4];
        result[0] = splitmix64(seed);
        result[1] = splitmix64(result[0]);
        result[2] = splitmix64(result[1]);
        result[3] = splitmix64(result[2]);
        return result;
    }

    private static ulong rol64(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    private static ulong xoshiro256p(ulong[] state)
    {
        ulong result = rol64(state[1] * 5, 7) * 9;
        ulong t = state[1] << 17;

        state[2] ^= state[0];
        state[3] ^= state[1];
        state[1] ^= state[2];
        state[0] ^= state[3];

        state[2] ^= t;
        state[3] = rol64(state[3], 45);

        return result;
    }


    // save/loading + oxfile integreation

    public override string ToString()
    {
        return $"({_state[0]},{_state[1]},{_state[2]},{_state[3]})";
    }

    public void FromString(string s)
    {
        if (s.Length <= 2) return;
        s = s.Substring(1, s.Length - 2);
        _state = s.StringToArray(",").Select(x => ulong.Parse(x)).ToArray();
    }
    public string OXF_GetIdentifier() => "RND";
    public OXRandom OXF_CreateInstanceFromBytes(byte[] data) => new OXRandom(MemoryMarshal.Cast<byte, ulong>(data).ToArray());
    public byte[] OXF_GetBytes() => MemoryMarshal.AsBytes(_state.AsSpan()).ToArray();
}
