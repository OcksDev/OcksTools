using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using static OXFileData;

/*
 * What is this?
 * 
 * This is a file format/parser I have been working on, to read/write/parse any .ox file.
 * 
 */




public class OXFile
{
    //touch away
    public const short ParserVersion = 2;
    //no touchy
    public int ObservedFileVersion = -1;
    public OXFileData Data = new OXFileData(OXFileData.OXFileType.OXFileData);
    public Dictionary<string, byte> NameLinker = new();
    public Dictionary<byte, string> IndexLinker = new();

    // Profiler markers, so you can see where read/write time goes
    static readonly ProfilerMarker pmWriteBuild = new ProfilerMarker("OX.Write.BuildBytes");
    static readonly ProfilerMarker pmWriteCompress = new ProfilerMarker("OX.Write.Compress");
    static readonly ProfilerMarker pmWriteObfuscate = new ProfilerMarker("OX.Write.Obfuscate");
    static readonly ProfilerMarker pmWriteDisk = new ProfilerMarker("OX.Write.Disk");
    static readonly ProfilerMarker pmReadDisk = new ProfilerMarker("OX.Read.Disk");
    static readonly ProfilerMarker pmReadDeobfuscate = new ProfilerMarker("OX.Read.Deobfuscate");
    static readonly ProfilerMarker pmReadDecompress = new ProfilerMarker("OX.Read.Decompress");
    static readonly ProfilerMarker pmReadParse = new ProfilerMarker("OX.Read.Parse");

    public OXFile LinkOptimizer(Dictionary<string, byte> n)
    {
        if (n.Count > 128)
        {
            throw new Exception("Linker only supports up to 128 unique entry names");
        }
        var p = new Dictionary<byte, string>();
        NameLinker = n;
        foreach (var k in NameLinker)
        {
            p.Add(k.Value, k.Key);
        }
        IndexLinker = p;
        SetFlag(0);
        return this;
    }

    public OXFile DisableObfuscation()
    {
        SetFlag(2);
        return this;
    }

    public OXFile DisableCompression()
    {
        SetFlag(1);
        return this;
    }

    public bool ReadFile(string str)
    {
        byte[] cd;
        using (pmReadDisk.Auto()) cd = File.ReadAllBytes(str);
        if (cd.Length < 4) return false;
        const int headerSize = 4;
        int bodyLen = cd.Length - headerSize;
        Flags = BitConverter.ToInt32(cd, 0);
        SetVersionFromFlag();
        Data = new OXFileData(OXFileData.OXFileType.OXFileData);
        Data.pVersion = ObservedFileVersion;

        // We own the freshly read buffer, so de-obfuscate it in place (no slice copy first)
        if (!GetFlag(2))
        {
            using (pmReadDeobfuscate.Auto()) DeObfuscate(cd, headerSize, bodyLen);
        }
        using (pmReadDecompress.Auto())
        {
            // Decompress straight out of the file buffer; only copy when there's nothing to decompress
            if (GetFlag(1)) Data.DataRaw = Decompress(cd, headerSize, bodyLen);
            else Data.DataRaw = WankFuckYou(cd, headerSize, bodyLen);
        }
        using (pmReadParse.Auto())
        {
            Data.DataOXFiles = Data.Get_OXFileData(GetFileData());
        }

        return true;
    }
    public bool WriteFile(string FileName, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        if (CanOverride || !File.Exists(FileName))
        {
            int oldflags = Flags;
            byte[] wank;
            using (pmWriteBuild.Auto()) wank = Data.BytesOfData(GetFileData(), 0).ToArray();

            // Only flag the file as compressed if we actually compressed it AND it got smaller.
            // Already-compressed payloads (PNG/JPEG) are detected and skipped, gzip on those just burns time.
            bool compressed = false;
            if (wank.Length >= 300 && !GetFlag(1))
            {
                using (pmWriteCompress.Auto())
                {
                    byte[] packed = CompressIfWorthIt(wank);
                    if (packed != null)
                    {
                        wank = packed;
                        compressed = true;
                    }
                }
            }
            SetFlag(1, compressed);

            if (!GetFlag(2))
            {
                using (pmWriteObfuscate.Auto()) wank = Obfuscate(wank);
            }
            SetVersionIntoFlag();
            var ver = BitConverter.GetBytes(Flags);
            using (pmWriteDisk.Auto())
            {
                // Header and body written separately: no extra "final" buffer copy of the whole file
                using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16))
                {
                    fs.Write(ver, 0, ver.Length);
                    fs.Write(wank, 0, wank.Length);
                }
            }
            Flags = oldflags;
            return true;
        }
        return false;
    }

    // ---------------------------------------------------------------------------------------------
    // Async versions of ReadFile / WriteFile.
    //
    // Await these from the Unity main thread. What runs where:
    //   worker threads : disk I/O, gzip, XOR, parsing / serializing the tree, ADPCM + delta coding,
    //                    mesh array parsing, PNG/JPEG encoding of textures (EncodeArrayToPNG is thread safe)
    //   main thread    : only the calls Unity forces onto it - copying data out of / into Texture2D,
    //                    AudioClip and Mesh objects, LoadImage (PNG/JPEG decode), and custom-format code.
    //                    These are spread over frames: after frameBudgetMs of work the method yields
    //                    and carries on next frame. One single huge asset can still take longer than the
    //                    budget, since a single Unity call can't be interrupted.
    //
    // Don't touch Data / Flags or start another read/write on this OXFile while one is running.
    // ---------------------------------------------------------------------------------------------

    public async Task<bool> ReadFileAsync(string str, CancellationToken ct = default, float frameBudgetMs = 4f)
    {
        byte[] cd = await ReadAllBytesAsync(str, ct);
        if (cd.Length < 4) return false;
        const int headerSize = 4;
        int bodyLen = cd.Length - headerSize;

        int newFlags = BitConverter.ToInt32(cd, 0);
        bool compressed = (newFlags & (1 << (1 + 16))) != 0;
        bool plain = (newFlags & (1 << (2 + 16))) != 0; // flag 2 set = NOT obfuscated

        // Parsing needs the new flags (linker flag etc.), so set them now and roll back if we fail / get cancelled
        int prevFlags = Flags, prevVersion = ObservedFileVersion;
        Flags = newFlags;
        SetVersionFromFlag();
        try
        {
            // Everything that doesn't need a Unity API, on a worker thread
            var parsed = await Task.Run(() =>
            {
                if (!plain)
                {
                    using (pmReadDeobfuscate.Auto()) DeObfuscate(cd, headerSize, bodyLen);
                }
                byte[] raw;
                using (pmReadDecompress.Auto())
                {
                    raw = compressed ? Decompress(cd, headerSize, bodyLen) : WankFuckYou(cd, headerSize, bodyLen);
                }

                var data = new OXFileData(OXFileData.OXFileType.OXFileData);
                data.pVersion = ObservedFileVersion;
                data.DataRaw = raw;

                var fd = GetFileData();
                fd.Deferred = true; // textures / sounds / meshes / custom formats are finished on the main thread below
                using (pmReadParse.Auto()) data.DataOXFiles = data.Get_OXFileData(fd);
                return (data: data, pending: fd.Pending);
            }, ct);

            // Main thread, one asset at a time, yielding to the frame loop when over budget
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (var finish in parsed.pending)
            {
                ct.ThrowIfCancellationRequested();
                finish();
                if (sw.Elapsed.TotalMilliseconds >= frameBudgetMs)
                {
                    await Task.Yield();
                    sw.Restart();
                }
            }

            Data = parsed.data; // only swap in the new tree once it's complete
            return true;
        }
        catch
        {
            Flags = prevFlags;
            ObservedFileVersion = prevVersion;
            throw;
        }
    }

    public async Task<bool> WriteFileAsync(string FileName, bool CanOverride, CancellationToken ct = default, float frameBudgetMs = 4f)
    {
        if (!CanOverride && File.Exists(FileName)) return false;

        // Step 1 (main thread, spread over frames): for every texture / sound / mesh / custom node, copy what's
        // needed out of the Unity object, then immediately hand the heavy encoding to a worker thread.
        var targets = new List<OXFileData>();
        OXFileData.CollectEncodeTargets(Data, targets, true);

        var jobs = new List<Task<byte[]>>(targets.Count);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        foreach (var node in targets)
        {
            ct.ThrowIfCancellationRequested();
            Func<byte[]> work = node.BeginEncode();
            jobs.Add(work == null ? Task.FromResult<byte[]>(null) : Task.Run(work, ct));
            if (sw.Elapsed.TotalMilliseconds >= frameBudgetMs)
            {
                await Task.Yield();
                sw.Restart();
            }
        }
        byte[][] encoded = await Task.WhenAll(jobs);
        for (int i = 0; i < targets.Count; i++)
        {
            // BytesOfData treats a node with DataRaw already set as "already encoded" and just wraps it
            if (encoded[i] != null) targets[i].DataRaw = encoded[i];
        }

        // Step 2 (worker thread): serialize the tree, compress, obfuscate
        bool noCompress = GetFlag(1);
        bool noObfuscate = GetFlag(2);
        var result = await Task.Run(() =>
        {
            byte[] body;
            using (pmWriteBuild.Auto()) body = Data.BytesOfData(GetFileData(), 0).ToArray();

            bool didCompress = false;
            if (body.Length >= 300 && !noCompress)
            {
                using (pmWriteCompress.Auto())
                {
                    byte[] packed = CompressIfWorthIt(body);
                    if (packed != null)
                    {
                        body = packed;
                        didCompress = true;
                    }
                }
            }
            if (!noObfuscate)
            {
                using (pmWriteObfuscate.Auto()) body = Obfuscate(body);
            }
            return (body: body, compressed: didCompress);
        }, ct);

        // Flags are only touched here, with no await in between, so they're always restored
        int oldflags = Flags;
        byte[] ver;
        try
        {
            SetFlag(1, result.compressed);
            SetVersionIntoFlag();
            ver = BitConverter.GetBytes(Flags);
        }
        finally
        {
            Flags = oldflags;
        }

        // Step 3: async disk write. Deliberately not cancellable, a cancel mid-write would leave a truncated file
        using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, FileOptions.Asynchronous))
        {
            await fs.WriteAsync(ver, 0, ver.Length);
            await fs.WriteAsync(result.body, 0, result.body.Length);
            await fs.FlushAsync();
        }
        return true;
    }

    private static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken ct)
    {
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16,
                   FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            int len = checked((int)fs.Length);
            byte[] buf = new byte[len];
            int read = 0;
            while (read < len)
            {
                int n = await fs.ReadAsync(buf, read, len - read, ct).ConfigureAwait(false);
                if (n == 0) break; // file shrank underneath us
                read += n;
            }
            if (read != len) Array.Resize(ref buf, read);
            return buf;
        }
    }

    private byte[] WankFuckYou(byte[] array, int offset, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }

    public FileData GetFileData()
    {
        var x = new FileData();
        x.File = this;
        return x;
    }
    public int Flags;
    public void SetVersionIntoFlag()
    {
        int p = 255;
        p |= p << 8;
        Flags &= ~p;
        Flags |= ParserVersion;
    }
    public void SetVersionFromFlag()
    {
        int p = 255;
        p |= p << 8;
        ObservedFileVersion = Flags & p;
    }
    public void SetFlag(int i, bool enabled = true)
    {
        int mask = (1 << (i + 16));
        Flags &= ~mask;
        if (enabled) Flags |= mask;
    }
    public bool GetFlag(int i)
    {
        int mask = (1 << (i + 16));
        return (Flags & mask) != 0;
    }
    public void ResetAllFlags()
    {
        int mask = (255 << (16));
        mask |= (255 << (16 + 8));
        Flags &= ~mask;
    }
    /*
     * Flags:
     * 0 -> Linker
     * 1 -> Compressed with GZIP
     * 2 -> Simple Anti-Skim Obfuscation
     */
    public static readonly Dictionary<OXFileType, byte> DefinedLengths = new()
    {
        { OXFileType.Bool, 1 },
        { OXFileType.Int, 4 },
        { OXFileType.Int1, 1 },
        { OXFileType.Long1, 1 },
        { OXFileType.Long, 8 },
        { OXFileType.Float, 4 },
        { OXFileType.Double, 8 },
        { OXFileType.Vector2, 8 },
        { OXFileType.Vector3, 12 },
        { OXFileType.Quaternion, 16 },
        { OXFileType.Color, 16 },
        { OXFileType.Vector2Int, 8 },
        { OXFileType.Vector3Int, 12 },
        { OXFileType.Color32, 4 },
        // Disk-only compact variants (see OXFileData.EffectiveType)
        { OXFileType.Float1, 1 },
        { OXFileType.Double1, 1 },
        { OXFileType.Vector2Int1, 2 },
        { OXFileType.Vector3Int1, 3 },
        { OXFileType.Vector2Whole1, 2 },
        { OXFileType.Vector3Whole1, 3 },
        { OXFileType.QuaternionWhole1, 4 },
        { OXFileType.Color1, 4 },
        { OXFileType.Color32Gray1, 1 },
    };
}

public class OXFileData
{
    public string Name = "";
    public OXFileType Type;

    private object _value;

    public string DataString
    {
        get => (string)_value;
        set => _value = value;
    }
    public List<string> DataListString
    {
        get => (List<string>)_value;
        set => _value = value;
    }
    public Dictionary<string, string> DataDictStringString
    {
        get => (Dictionary<string, string>)_value;
        set => _value = value;
    }
    public Dictionary<string, int> DataDictStringInt
    {
        get => (Dictionary<string, int>)_value;
        set => _value = value;
    }
    public Dictionary<string, long> DataDictStringLong
    {
        get => (Dictionary<string, long>)_value;
        set => _value = value;
    }
    public Dictionary<string, double> DataDictStringDouble
    {
        get => (Dictionary<string, double>)_value;
        set => _value = value;
    }
    public Dictionary<string, float> DataDictStringFloat
    {
        get => (Dictionary<string, float>)_value;
        set => _value = value;
    }
    public float DataFloat
    {
        get => _value is float f ? f : 0f;
        set => _value = value;
    }
    public double DataDouble
    {
        get => _value is double d ? d : 0.0;
        set => _value = value;
    }
    public long DataLong
    {
        get => _value is long l ? l : 0L;
        set => _value = value;
    }
    public byte DataByte
    {
        get => _value is byte b ? b : (byte)0;
        set => _value = value;
    }
    public int DataInt
    {
        get => _value is int i ? i : 0;
        set => _value = value;
    }
    public bool DataBool
    {
        get => _value is bool b && b;
        set => _value = value;
    }
    public Vector2 DataVector2
    {
        get => _value is Vector2 v ? v : Vector2.zero;
        set => _value = value;
    }
    public Vector3 DataVector3
    {
        get => _value is Vector3 v ? v : Vector3.zero;
        set => _value = value;
    }
    public Quaternion DataQuaternion
    {
        get => _value is Quaternion q ? q : Quaternion.identity;
        set => _value = value;
    }
    public Color DataColor
    {
        get => _value is Color c ? c : Color.clear;
        set => _value = value;
    }
    public Vector2Int DataVector2Int
    {
        get => _value is Vector2Int v ? v : Vector2Int.zero;
        set => _value = value;
    }
    public Vector3Int DataVector3Int
    {
        get => _value is Vector3Int v ? v : Vector3Int.zero;
        set => _value = value;
    }
    public Color32 DataColor32
    {
        get => _value is Color32 c ? c : new Color32(0, 0, 0, 0);
        set => _value = value;
    }
    public Texture2D DataTexture
    {
        get => _value as Texture2D;
        set => _value = value;
    }
    public Sprite DataSprite
    {
        get => DataTexture.Texture2DToSprite();
        set => DataTexture = value.texture;
    }
    public AudioClip DataSound
    {
        get => _value as AudioClip;
        set => _value = value;
    }
    public Mesh DataMesh
    {
        get => _value as Mesh;
        set => _value = value;
    }
    public _IOXFile DataCustom
    {
        get => (_IOXFile)_value;
        set => _value = value;
    }
    public Dictionary<string, OXFileData> DataOXFiles
    {
        get => _value as Dictionary<string, OXFileData>;
        set => _value = value;
    }
    public List<OXFileData> DataListOXFiles
    {
        get => _value as List<OXFileData>;
        set => _value = value;
    }

    public byte[] DataRaw;


    public enum OXFileType
    {
        String,
        OXFileData,
        ListOXFileData,
        DictStringString,
        ListString,
        Int,
        Float,
        Long,
        Double,
        Bool,
        Raw,
        Custom,
        Repeat,
        r3, r4, r5, r6, r7, r8, r9, r10,
        Texture,
        Sound,
        Mesh,
        Vector2,
        Vector2Int,
        Vector3,
        Vector3Int,
        Quaternion,
        Color,
        Color32,
        Int1,
        Long1,
        ListString1,
        Float1,
        Double1,
        Vector2Int1,
        Vector3Int1,
        Vector2Whole1,
        Vector3Whole1,
        QuaternionWhole1,
        // Color where every channel is exactly n/255 -> 1 byte per channel (4 bytes instead of 16), lossless
        Color1,
        // Color32 that is opaque gray (r == g == b, a == 255) -> 1 byte
        Color32Gray1,
        DictStringString1,
        DictStringInt,
        DictStringLong,
        DictStringDouble,
        DictStringFloat,
        DictStringInt1,
        DictStringLong1,
        DictStringDouble1,
        DictStringFloat1,
        // Disk-only "11" versions: 1-byte key lengths AND 1-byte values.
        DictStringInt11,
        DictStringLong11,
        DictStringDouble11,
        DictStringFloat11,
    }

    // The type that actually gets written to disk. Int/Long shrink to Int1/Long1 when the value fits
    // in a signed byte, and ListString shrinks to ListString1 when every element is <= 255 bytes long.
    public OXFileType EffectiveType
    {
        get
        {
            switch (Type)
            {
                case OXFileType.Int:
                    {
                        int v = DataInt;
                        if (v >= sbyte.MinValue && v <= sbyte.MaxValue) return OXFileType.Int1;
                        break;
                    }
                case OXFileType.Long:
                    {
                        long v = DataLong;
                        if (v >= sbyte.MinValue && v <= sbyte.MaxValue) return OXFileType.Long1;
                        break;
                    }
                case OXFileType.ListString:
                    if (ListStringFitsOneByte(DataListString)) return OXFileType.ListString1;
                    break;
                case OXFileType.DictStringString:
                    if (DictFitsOneByte(DataDictStringString)) return OXFileType.DictStringString1;
                    break;
                case OXFileType.DictStringInt:
                    if (KeysFitOneByte(DataDictStringInt))
                    {
                        if (ValuesAll(DataDictStringInt, FitsSByte)) return OXFileType.DictStringInt11;
                        return OXFileType.DictStringInt1;
                    }
                    break;
                case OXFileType.DictStringLong:
                    if (KeysFitOneByte(DataDictStringLong))
                    {
                        if (ValuesAll(DataDictStringLong, v => v >= sbyte.MinValue && v <= sbyte.MaxValue)) return OXFileType.DictStringLong11;
                        return OXFileType.DictStringLong1;
                    }
                    break;
                case OXFileType.DictStringDouble:
                    if (KeysFitOneByte(DataDictStringDouble))
                    {
                        if (ValuesAll(DataDictStringDouble, FitsWholeSByte)) return OXFileType.DictStringDouble11;
                        return OXFileType.DictStringDouble1;
                    }
                    break;
                case OXFileType.DictStringFloat:
                    if (KeysFitOneByte(DataDictStringFloat))
                    {
                        if (ValuesAll(DataDictStringFloat, FitsWholeSByte)) return OXFileType.DictStringFloat11;
                        return OXFileType.DictStringFloat1;
                    }
                    break;
                case OXFileType.Float:
                    if (FitsWholeSByte(DataFloat)) return OXFileType.Float1;
                    break;
                case OXFileType.Double:
                    if (FitsWholeSByte(DataDouble)) return OXFileType.Double1;
                    break;
                case OXFileType.Vector2Int:
                    {
                        var v = DataVector2Int;
                        if (FitsSByte(v.x) && FitsSByte(v.y)) return OXFileType.Vector2Int1;
                        break;
                    }
                case OXFileType.Vector3Int:
                    {
                        var v = DataVector3Int;
                        if (FitsSByte(v.x) && FitsSByte(v.y) && FitsSByte(v.z)) return OXFileType.Vector3Int1;
                        break;
                    }
                case OXFileType.Vector2:
                    {
                        var v = DataVector2;
                        if (FitsWholeSByte(v.x) && FitsWholeSByte(v.y)) return OXFileType.Vector2Whole1;
                        break;
                    }
                case OXFileType.Vector3:
                    {
                        var v = DataVector3;
                        if (FitsWholeSByte(v.x) && FitsWholeSByte(v.y) && FitsWholeSByte(v.z)) return OXFileType.Vector3Whole1;
                        break;
                    }
                case OXFileType.Quaternion:
                    {
                        var q = DataQuaternion;
                        if (FitsWholeSByte(q.x) && FitsWholeSByte(q.y) && FitsWholeSByte(q.z) && FitsWholeSByte(q.w)) return OXFileType.QuaternionWhole1;
                        break;
                    }
                case OXFileType.Color:
                    {
                        var c = DataColor;
                        if (FloatIsByteFraction(c.r) && FloatIsByteFraction(c.g) && FloatIsByteFraction(c.b) && FloatIsByteFraction(c.a)) return OXFileType.Color1;
                        break;
                    }
                case OXFileType.Color32:
                    {
                        var c = DataColor32;
                        if (c.r == c.g && c.g == c.b && c.a == 255) return OXFileType.Color32Gray1;
                        break;
                    }
            }
            return Type;
        }
    }

    private static bool FitsSByte(int v) => v >= sbyte.MinValue && v <= sbyte.MaxValue;

    // True only if the value is a whole number in -128..127 AND survives the round trip exactly
    // (so -0, NaN, 0.5, 300 and friends are all rejected and keep their full-size encoding).
    private static bool FitsWholeSByte(float v)
    {
        if (!(v >= -128f && v <= 127f)) return false;
        sbyte s = (sbyte)v;
        if ((float)s != v) return false;
        if (s == 0 && 1f / v < 0f) return false; // negative zero
        return true;
    }
    private static bool FitsWholeSByte(double v)
    {
        if (!(v >= -128.0 && v <= 127.0)) return false;
        sbyte s = (sbyte)v;
        if ((double)s != v) return false;
        if (s == 0 && 1.0 / v < 0.0) return false; // negative zero
        return true;
    }

    // True if the float is exactly n/255 for some byte n, so it can be stored as that byte losslessly.
    private static bool FloatIsByteFraction(float c)
    {
        if (!(c >= 0f && c <= 1f)) return false;
        int r = (int)Math.Round(c * 255f);
        if (r < 0 || r > 255) return false;
        return (r / 255f) == c;
    }
    private static byte FloatToByteFraction(float c) => (byte)(int)Math.Round(c * 255f);

    private static bool DictFitsOneByte(Dictionary<string, string> dict)
    {
        if (dict == null) return false;
        foreach (var kv in dict)
        {
            if (kv.Key.Length * 3 > 255 && Encoding.UTF8.GetByteCount(kv.Key) > 255) return false;
            if (kv.Value.Length * 3 > 255 && Encoding.UTF8.GetByteCount(kv.Value) > 255) return false;
        }
        return true;
    }

    // True if every key of a string-keyed dictionary is <= 255 UTF8 bytes (so a 1-byte length is enough)
    private static bool KeysFitOneByte<T>(Dictionary<string, T> dict)
    {
        if (dict == null) return false;
        foreach (var kv in dict)
        {
            if (kv.Key.Length * 3 > 255 && Encoding.UTF8.GetByteCount(kv.Key) > 255) return false;
        }
        return true;
    }

    // True if every value of the dictionary satisfies the predicate (used to decide if "11" encoding is possible)
    private static bool ValuesAll<T>(Dictionary<string, T> dict, Func<T, bool> fits)
    {
        foreach (var kv in dict)
        {
            if (!fits(kv.Value)) return false;
        }
        return true;
    }

    // Writes [key length][key UTF8][value] for every entry. oneByteLen picks 1-byte vs 4-byte key lengths.
    private static void WriteDictStringFixed<T>(List<byte> ret, Dictionary<string, T> dict, Func<T, byte[]> valueToBytes, bool oneByteLen)
    {
        foreach (var kv in dict)
        {
            var kb = Encoding.UTF8.GetBytes(kv.Key);
            if (oneByteLen) ret.Add((byte)kb.Length);
            else ret.AddRange(BitConverter.GetBytes(kb.Length));
            ret.AddRange(kb);
            ret.AddRange(valueToBytes(kv.Value));
        }
    }

    // Reads what WriteDictStringFixed wrote.
    private static Dictionary<string, T> ReadDictStringFixed<T>(byte[] raw, int valueSize, Func<byte[], int, T> readValue, bool oneByteLen)
    {
        var ret = new Dictionary<string, T>();
        int index = 0;
        while (oneByteLen ? index < raw.Length : index + 3 < raw.Length)
        {
            int keyLength;
            if (oneByteLen)
            {
                keyLength = raw[index];
                index += 1;
            }
            else
            {
                keyLength = BitConverter.ToInt32(raw, index);
                index += 4;
            }
            string key = Encoding.UTF8.GetString(raw, index, keyLength);
            index += keyLength;
            ret.Add(key, readValue(raw, index));
            index += valueSize;
        }
        return ret;
    }

    private static bool ListStringFitsOneByte(List<string> list)
    {
        if (list == null) return false;
        foreach (var s in list)
        {
            // Fast path: a char is at most 3 UTF8 bytes, so short strings can skip the exact count
            if (s.Length * 3 <= 255) continue;
            if (Encoding.UTF8.GetByteCount(s) > 255) return false;
        }
        return true;
    }

    // The type exactly as it was read from the file (before Int1 turns back into Int). Used for repeat runs.
    public OXFileType WireType;

    public int LengthOffset;
    public int pVersion = 0;
    // Only meaningful for OXFileType.Sound: true = lossless (delta+gzip), false = lossy (ADPCM). Defaults lossy for max space savings.
    public bool SoundLossless = false;
    // Only meaningful for OXFileType.Texture: true = lossy (JPEG), false = lossless PNG (default, same as before).
    public bool TextureLossy = false;
    // JPEG quality 1..100 used when TextureLossy is true.
    public int TextureQuality = 75;
    public OXFileData() { }
    public OXFileData(OXFileType tp)
    {
        Type = tp;
        if (tp == OXFileType.OXFileData)
            _value = new Dictionary<string, OXFileData>();
        else if (tp == OXFileType.ListOXFileData)
            _value = new List<OXFileData>();
    }

    public OXFileData this[int index]
    {
        get => DataListOXFiles[index];
        set => DataListOXFiles[index] = value;
    }
    public OXFileData this[string index]
    {
        get => DataOXFiles[index];
        set => DataOXFiles[index] = value;
    }

    public bool TryGetValue(string name, out OXFileData dd) => DataOXFiles.TryGetValue(name, out dd);
    public bool Contains(string name) => DataOXFiles.ContainsKey(name);

    public List<byte> ToByte(FileData fd)
    {
        var w = Encoding.UTF8.GetBytes(Name);
        var w2 = DataRaw;
        List<byte> ret = new List<byte>(w2.Length + w.Length + 8);
        Func<byte[], int> AppendAll = (x) =>
        {
            ret.AddRange(x);
            return 1;
        };

        OXFileType wt = EffectiveType;
        byte[] data_size = new byte[1];
        byte l = (byte)w.Length;
        if (fd.File.GetFlag(0)) l = fd.File.NameLinker[Name];
        l &= 127;
        if (OXFile.DefinedLengths.ContainsKey(wt))
        {
            data_size = new byte[1] { OXFile.DefinedLengths[wt] };
        }
        else if (w2.Length < 256)
        {
            data_size = new byte[1] { (byte)w2.Length };
        }
        else
        {
            data_size = BitConverter.GetBytes(w2.Length);
            l |= 128;
        }
        AppendAll(new byte[1] { l });
        if (RepeatRun < 0)
        {
            switch (RepeatRun)
            {
                case -3: AppendAll(new byte[1] { (byte)OXFileType.r3 }); break;
                case -4: AppendAll(new byte[1] { (byte)OXFileType.r4 }); break;
                case -5: AppendAll(new byte[1] { (byte)OXFileType.r5 }); break;
                case -6: AppendAll(new byte[1] { (byte)OXFileType.r6 }); break;
                case -7: AppendAll(new byte[1] { (byte)OXFileType.r7 }); break;
                case -8: AppendAll(new byte[1] { (byte)OXFileType.r8 }); break;
                case -9: AppendAll(new byte[1] { (byte)OXFileType.r9 }); break;
                case -10: AppendAll(new byte[1] { (byte)OXFileType.r10 }); break;
            }
        }
        else if (RepeatRun > 0)
        {
            AppendAll(new byte[2] { (byte)OXFileType.Repeat, (byte)RepeatRun });
        }
        if (!ExcludeCuzRepeated) AppendAll(new byte[1] { (byte)wt });
        if (!OXFile.DefinedLengths.ContainsKey(wt)) AppendAll(data_size);
        if (!fd.File.GetFlag(0)) AppendAll(w);
        AppendAll(w2);
        return ret;
    }
    public OXFileData Parse(byte[] dat, int index, FileData fd)
    {
        int initiniex = index;
        byte length = dat[index];
        bool longermode = false;
        if (length > 127)
        {
            longermode = true;
        }
        length &= 127;
        int bodylength = 0;
        int incindex = 1;
        if (!ExcludeCuzRepeated)
        {
            index++;
            Type = (OXFileType)dat[index];
        }
        switch (Type)
        {
            case OXFileType.r3: RepeatRun = -3; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r4: RepeatRun = -4; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r5: RepeatRun = -5; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r6: RepeatRun = -6; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r7: RepeatRun = -7; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r8: RepeatRun = -8; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r9: RepeatRun = -9; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.r10: RepeatRun = -10; index++; Type = (OXFileType)dat[index]; break;
            case OXFileType.Repeat:
                index++;
                RepeatRun = dat[index];
                index++;
                Type = (OXFileType)dat[index];
                break;
        }
        WireType = Type;
        if (OXFile.DefinedLengths.ContainsKey(Type))
        {
            bodylength = OXFile.DefinedLengths[Type];
        }
        else if (longermode)
        {
            bodylength = BitConverter.ToInt32(dat, index + 1);
            incindex += 4;
        }
        else
        {
            bodylength = dat[index + 1];
            incindex += 1;
        }
        if (length == 0 && !fd.File.GetFlag(0)) goto end;
        index += incindex;
        if (fd.File.GetFlag(0))
        {
            Name = fd.File.IndexLinker[length];
        }
        else
        {
            Name = Encoding.UTF8.GetString(dat, index, length);
            index += length;
        }
        DataRaw = WankFuckYou(dat, index, bodylength);
        index += bodylength;
        switch (Type)
        {
            case OXFileType.String:
                DataString = Get_String();
                break;
            case OXFileType.Int:
                DataInt = Get_Int();
                break;
            case OXFileType.Int1:
                // Stored as 1 signed byte, becomes a normal Int again in memory
                DataInt = (sbyte)DataRaw[0];
                Type = OXFileType.Int;
                break;
            case OXFileType.Long1:
                // Stored as 1 signed byte, becomes a normal Long again in memory
                DataLong = (sbyte)DataRaw[0];
                Type = OXFileType.Long;
                break;
            case OXFileType.ListString1:
                // 1-byte element lengths on disk, becomes a normal ListString in memory
                DataListString = Get_ListString1();
                Type = OXFileType.ListString;
                break;
            case OXFileType.DictStringString1:
                DataDictStringString = Get_DictStringString1();
                Type = OXFileType.DictStringString;
                break;
            case OXFileType.DictStringInt1:
                DataDictStringInt = ReadDictStringFixed(DataRaw, 4, BitConverter.ToInt32, true);
                Type = OXFileType.DictStringInt;
                break;
            case OXFileType.DictStringLong1:
                DataDictStringLong = ReadDictStringFixed(DataRaw, 8, BitConverter.ToInt64, true);
                Type = OXFileType.DictStringLong;
                break;
            case OXFileType.DictStringDouble1:
                DataDictStringDouble = ReadDictStringFixed(DataRaw, 8, BitConverter.ToDouble, true);
                Type = OXFileType.DictStringDouble;
                break;
            case OXFileType.DictStringFloat1:
                DataDictStringFloat = ReadDictStringFixed(DataRaw, 4, BitConverter.ToSingle, true);
                Type = OXFileType.DictStringFloat;
                break;
            case OXFileType.DictStringInt11:
                DataDictStringInt = ReadDictStringFixed(DataRaw, 1, (raw, i) => (int)(sbyte)raw[i], true);
                Type = OXFileType.DictStringInt;
                break;
            case OXFileType.DictStringLong11:
                DataDictStringLong = ReadDictStringFixed(DataRaw, 1, (raw, i) => (long)(sbyte)raw[i], true);
                Type = OXFileType.DictStringLong;
                break;
            case OXFileType.DictStringDouble11:
                DataDictStringDouble = ReadDictStringFixed(DataRaw, 1, (raw, i) => (double)(sbyte)raw[i], true);
                Type = OXFileType.DictStringDouble;
                break;
            case OXFileType.DictStringFloat11:
                DataDictStringFloat = ReadDictStringFixed(DataRaw, 1, (raw, i) => (float)(sbyte)raw[i], true);
                Type = OXFileType.DictStringFloat;
                break;
            case OXFileType.DictStringInt:
                DataDictStringInt = ReadDictStringFixed(DataRaw, 4, BitConverter.ToInt32, false);
                break;
            case OXFileType.DictStringLong:
                DataDictStringLong = ReadDictStringFixed(DataRaw, 8, BitConverter.ToInt64, false);
                break;
            case OXFileType.DictStringDouble:
                DataDictStringDouble = ReadDictStringFixed(DataRaw, 8, BitConverter.ToDouble, false);
                break;
            case OXFileType.DictStringFloat:
                DataDictStringFloat = ReadDictStringFixed(DataRaw, 4, BitConverter.ToSingle, false);
                break;
            case OXFileType.Float1:
                DataFloat = (sbyte)DataRaw[0];
                Type = OXFileType.Float;
                break;
            case OXFileType.Double1:
                DataDouble = (sbyte)DataRaw[0];
                Type = OXFileType.Double;
                break;
            case OXFileType.Vector2Int1:
                DataVector2Int = new Vector2Int((sbyte)DataRaw[0], (sbyte)DataRaw[1]);
                Type = OXFileType.Vector2Int;
                break;
            case OXFileType.Vector3Int1:
                DataVector3Int = new Vector3Int((sbyte)DataRaw[0], (sbyte)DataRaw[1], (sbyte)DataRaw[2]);
                Type = OXFileType.Vector3Int;
                break;
            case OXFileType.Vector2Whole1:
                DataVector2 = new Vector2((sbyte)DataRaw[0], (sbyte)DataRaw[1]);
                Type = OXFileType.Vector2;
                break;
            case OXFileType.Vector3Whole1:
                DataVector3 = new Vector3((sbyte)DataRaw[0], (sbyte)DataRaw[1], (sbyte)DataRaw[2]);
                Type = OXFileType.Vector3;
                break;
            case OXFileType.QuaternionWhole1:
                DataQuaternion = new Quaternion((sbyte)DataRaw[0], (sbyte)DataRaw[1], (sbyte)DataRaw[2], (sbyte)DataRaw[3]);
                Type = OXFileType.Quaternion;
                break;
            case OXFileType.Color1:
                DataColor = new Color(DataRaw[0] / 255f, DataRaw[1] / 255f, DataRaw[2] / 255f, DataRaw[3] / 255f);
                Type = OXFileType.Color;
                break;
            case OXFileType.Color32Gray1:
                DataColor32 = new Color32(DataRaw[0], DataRaw[0], DataRaw[0], 255);
                Type = OXFileType.Color32;
                break;
            case OXFileType.Long:
                DataLong = Get_Long();
                break;
            case OXFileType.Float:
                DataFloat = Get_Float();
                break;
            case OXFileType.Double:
                DataDouble = Get_Double();
                break;
            case OXFileType.Vector2:
                DataVector2 = Get_Vector2();
                break;
            case OXFileType.Vector3:
                DataVector3 = Get_Vector3();
                break;
            case OXFileType.Quaternion:
                DataQuaternion = Get_Quaternion();
                break;
            case OXFileType.Color:
                DataColor = Get_Color();
                break;
            case OXFileType.Vector2Int:
                DataVector2Int = Get_Vector2Int();
                break;
            case OXFileType.Vector3Int:
                DataVector3Int = Get_Vector3Int();
                break;
            case OXFileType.Color32:
                DataColor32 = Get_Color32();
                break;
            case OXFileType.OXFileData:
                DataOXFiles = Get_OXFileData(fd);
                break;
            case OXFileType.ListString:
                DataListString = Get_ListString();
                break;
            case OXFileType.DictStringString:
                DataDictStringString = Get_DictStringString();
                break;
            case OXFileType.Texture:
                if (fd.Deferred)
                {
                    var texPayload = PrepareTexturePayload(DataRaw); // pure managed (alpha plane gunzip)
                    fd.Pending.Add(() => { DataTexture = BuildTexture(texPayload); });
                }
                else DataTexture = Get_Texture();
                break;
            case OXFileType.Sound:
                if (fd.Deferred)
                {
                    var audio = DecodeAudioBytes(DataRaw); // pure managed (gunzip + ADPCM/delta decode)
                    fd.Pending.Add(() => { DataSound = DecodedToAudioClip(audio); });
                }
                else DataSound = Get_Sound();
                break;
            case OXFileType.Mesh:
                if (fd.Deferred)
                {
                    var parsedMesh = ParseMeshBytes(DataRaw); // pure managed
                    fd.Pending.Add(() => { DataMesh = BuildMesh(parsedMesh); });
                }
                else DataMesh = Get_Mesh();
                break;
            case OXFileType.Custom:
                if (fd.Deferred)
                {
                    // Custom formats are user code that may touch Unity APIs, so link them on the main thread
                    var customRaw = DataRaw;
                    fd.Pending.Add(() => { DataCustom = LinkCustomBytes(customRaw); });
                }
                else DataCustom = Get_Custom();
                break;
            case OXFileType.ListOXFileData:
                DataListOXFiles = Get_ListOXFileData(fd);
                break;
            default: break;
        }
        switch (this.Type)
        {
            case OXFileType.Raw: break;
            default: DataRaw = null; break;
        }

    end:
        LengthOffset = index - initiniex;
        return this;
    }

    public void Add(string Name, string DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.String;
        dat.DataString = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, int DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Int;
        dat.DataInt = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, bool DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Bool;
        dat.DataBool = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, BetterVector2 DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Vector2;
        dat.DataVector2 = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, BetterVector3 DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Vector3;
        dat.DataVector3 = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Quaternion DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Quaternion;
        dat.DataQuaternion = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Color DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Color;
        dat.DataColor = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, BetterVector2Int DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Vector2Int;
        dat.DataVector2Int = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, BetterVector3Int DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Vector3Int;
        dat.DataVector3Int = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Color32 DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Color32;
        dat.DataColor32 = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Texture2D DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataTexture = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Texture2D DataIn, bool lossy, int quality = 75)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataTexture = DataIn;
        dat.TextureLossy = lossy;
        dat.TextureQuality = quality;
        Add(Name, dat);
    }
    // Adds an image that is ALREADY encoded (PNG/JPEG file bytes, e.g. straight from disk).
    // Skips the decode + re-encode entirely, which is by far the fastest way to put an image in a file.
    public void AddEncodedTexture(string Name, byte[] encodedImage)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataRaw = encodedImage;
        Add(Name, dat);
    }
    public void Add(string Name, Sprite DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataSprite = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Sprite DataIn, bool lossy, int quality = 75)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataSprite = DataIn;
        dat.TextureLossy = lossy;
        dat.TextureQuality = quality;
        Add(Name, dat);
    }
    public void Add(string Name, AudioClip DataIn, bool lossless = false)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Sound;
        dat.DataSound = DataIn;
        dat.SoundLossless = lossless;
        Add(Name, dat);
    }
    public void Add(string Name, Mesh DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Mesh;
        dat.DataMesh = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, float DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Float;
        dat.DataFloat = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, byte[] DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Raw;
        dat.DataRaw = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, double DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Double;
        dat.DataDouble = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, long DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Long;
        dat.DataLong = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, OXFileData> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.OXFileData;
        dat.DataOXFiles = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, BetterList<OXFileData> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.ListOXFileData;
        dat.DataListOXFiles = DataIn;
        Add(Name, dat);
    }

    public void Add(string Name, BetterList<string> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.ListString;
        dat.DataListString = DataIn;
        Add(Name, dat);
    }

    public void Add(string Name, _IOXFile DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Custom;
        dat.DataCustom = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, string> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.DictStringString;
        dat.DataDictStringString = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, int> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.DictStringInt;
        dat.DataDictStringInt = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, long> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.DictStringLong;
        dat.DataDictStringLong = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, double> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.DictStringDouble;
        dat.DataDictStringDouble = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Dictionary<string, float> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.DictStringFloat;
        dat.DataDictStringFloat = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, OXFileData dat)
    {
        dat.Name = Name;
        dat.pVersion = pVersion;
        switch (this.Type)
        {
            case OXFileType.ListOXFileData:
                if (_value == null) _value = new List<OXFileData>();
                DataListOXFiles.Add(dat);
                break;
            default:
                if (_value == null) _value = new Dictionary<string, OXFileData>();
                if (ContainsKey(Name))
                {
                    DataOXFiles[Name] = dat;
                }
                else
                {
                    DataOXFiles.Add(Name, dat);
                }
                break;
        }
    }
    public int RepeatRun = 0;
    public bool ExcludeCuzRepeated = false;
    private const int repeatmax = 11;
    public List<byte> BytesOfData(FileData fd, int current_step)
    {
        List<byte> ret = new List<byte>();
        List<byte> bytes = new List<byte>();
        byte[] bytez;
        switch (EffectiveType)
        {
            case OXFileType.Int1:
                ret.Add((byte)(sbyte)DataInt);
                break;
            case OXFileType.Long1:
                ret.Add((byte)(sbyte)DataLong);
                break;
            case OXFileType.ListString1:
                foreach (var li in DataListString)
                {
                    var ccc = Encoding.UTF8.GetBytes(li);
                    ret.Add((byte)ccc.Length);
                    ret.AddRange(ccc);
                }
                break;
            case OXFileType.DictStringString1:
                foreach (var li in DataDictStringString)
                {
                    var kb = Encoding.UTF8.GetBytes(li.Key);
                    var vb = Encoding.UTF8.GetBytes(li.Value);
                    ret.Add((byte)kb.Length);
                    ret.Add((byte)vb.Length);
                    ret.AddRange(kb);
                    ret.AddRange(vb);
                }
                break;
            case OXFileType.DictStringInt1:
                WriteDictStringFixed(ret, DataDictStringInt, BitConverter.GetBytes, true);
                break;
            case OXFileType.DictStringLong1:
                WriteDictStringFixed(ret, DataDictStringLong, BitConverter.GetBytes, true);
                break;
            case OXFileType.DictStringDouble1:
                WriteDictStringFixed(ret, DataDictStringDouble, BitConverter.GetBytes, true);
                break;
            case OXFileType.DictStringFloat1:
                WriteDictStringFixed(ret, DataDictStringFloat, BitConverter.GetBytes, true);
                break;
            case OXFileType.DictStringInt11:
                WriteDictStringFixed(ret, DataDictStringInt, v => new byte[] { (byte)(sbyte)v }, true);
                break;
            case OXFileType.DictStringLong11:
                WriteDictStringFixed(ret, DataDictStringLong, v => new byte[] { (byte)(sbyte)v }, true);
                break;
            case OXFileType.DictStringDouble11:
                WriteDictStringFixed(ret, DataDictStringDouble, v => new byte[] { (byte)(sbyte)v }, true);
                break;
            case OXFileType.DictStringFloat11:
                WriteDictStringFixed(ret, DataDictStringFloat, v => new byte[] { (byte)(sbyte)v }, true);
                break;
            case OXFileType.DictStringInt:
                WriteDictStringFixed(ret, DataDictStringInt, BitConverter.GetBytes, false);
                break;
            case OXFileType.DictStringLong:
                WriteDictStringFixed(ret, DataDictStringLong, BitConverter.GetBytes, false);
                break;
            case OXFileType.DictStringDouble:
                WriteDictStringFixed(ret, DataDictStringDouble, BitConverter.GetBytes, false);
                break;
            case OXFileType.DictStringFloat:
                WriteDictStringFixed(ret, DataDictStringFloat, BitConverter.GetBytes, false);
                break;
            case OXFileType.Float1:
                ret.Add((byte)(sbyte)DataFloat);
                break;
            case OXFileType.Double1:
                ret.Add((byte)(sbyte)DataDouble);
                break;
            case OXFileType.Vector2Int1:
                ret.Add((byte)(sbyte)DataVector2Int.x);
                ret.Add((byte)(sbyte)DataVector2Int.y);
                break;
            case OXFileType.Vector3Int1:
                ret.Add((byte)(sbyte)DataVector3Int.x);
                ret.Add((byte)(sbyte)DataVector3Int.y);
                ret.Add((byte)(sbyte)DataVector3Int.z);
                break;
            case OXFileType.Vector2Whole1:
                ret.Add((byte)(sbyte)DataVector2.x);
                ret.Add((byte)(sbyte)DataVector2.y);
                break;
            case OXFileType.Vector3Whole1:
                ret.Add((byte)(sbyte)DataVector3.x);
                ret.Add((byte)(sbyte)DataVector3.y);
                ret.Add((byte)(sbyte)DataVector3.z);
                break;
            case OXFileType.QuaternionWhole1:
                ret.Add((byte)(sbyte)DataQuaternion.x);
                ret.Add((byte)(sbyte)DataQuaternion.y);
                ret.Add((byte)(sbyte)DataQuaternion.z);
                ret.Add((byte)(sbyte)DataQuaternion.w);
                break;
            case OXFileType.Color1:
                ret.Add(FloatToByteFraction(DataColor.r));
                ret.Add(FloatToByteFraction(DataColor.g));
                ret.Add(FloatToByteFraction(DataColor.b));
                ret.Add(FloatToByteFraction(DataColor.a));
                break;
            case OXFileType.Color32Gray1:
                ret.Add(DataColor32.r);
                break;
            case OXFileType.OXFileData:
                var p = DataOXFiles.ToList();
                if (p.Count == 0) break;
                p.Sort((a, b) => a.Value.EffectiveType.CompareTo(b.Value.EffectiveType));
                OXFileType c = p[0].Value.EffectiveType;
                int same = 0;
                int index = 0;
                Action forwardupdate = () =>
                {
                    for (int i = 1; i < same; i++)
                    {
                        p[(index - same) + i].Value.ExcludeCuzRepeated = true;
                    }
                };
                Action fard = () =>
                {
                    if (same >= repeatmax)
                    {
                        p[index - same].Value.RepeatRun = (byte)(same - (repeatmax - 1));
                        forwardupdate();
                    }
                    else if (same >= 3)
                    {
                        p[index - same].Value.RepeatRun = -same;
                        forwardupdate();
                    }
                };
                foreach (var a in p)
                {
                    a.Value.RepeatRun = 0;
                    a.Value.ExcludeCuzRepeated = false;
                    if (a.Value.EffectiveType == c && same <= 253 + repeatmax)
                    {
                        same++;
                    }
                    else
                    {
                        fard();
                        c = a.Value.EffectiveType;
                        same = 1;
                    }
                    index++;
                }
                fard();
                foreach (var a in p)
                {
                    if (a.Value.DataRaw != null && a.Value.DataRaw.Length > 0)
                    {
                        bytes = a.Value.ToByte(fd);
                    }
                    else
                    {
                        fd.CurrentStep++;
                        a.Value.DataRaw = a.Value.BytesOfData(fd, fd.CurrentStep).ToArray();
                        bytes = a.Value.ToByte(fd);
                    }
                    ret.AddRange(bytes);
                }
                break;
            case OXFileType.ListOXFileData:
                foreach (var a in DataListOXFiles)
                {
                    if (a.DataRaw != null && a.DataRaw.Length > 0)
                    {
                        bytes = a.ToByte(fd);
                    }
                    else
                    {
                        fd.CurrentStep++;
                        a.DataRaw = a.BytesOfData(fd, fd.CurrentStep).ToArray();
                        bytes = a.ToByte(fd);
                    }
                    ret.AddRange(bytes);
                }
                break;
            case OXFileType.String:
                bytez = Encoding.UTF8.GetBytes(DataString);
                ret.AddRange(bytez);
                break;
            case OXFileType.Int:
                bytez = BitConverter.GetBytes(DataInt);
                ret.AddRange(bytez);
                break;
            case OXFileType.Long:
                bytez = BitConverter.GetBytes(DataLong);
                ret.AddRange(bytez);
                break;
            case OXFileType.Float:
                bytez = BitConverter.GetBytes(DataFloat);
                ret.AddRange(bytez);
                break;
            case OXFileType.Double:
                bytez = BitConverter.GetBytes(DataDouble);
                ret.AddRange(bytez);
                break;
            case OXFileType.Vector2:
                ret.AddRange(BitConverter.GetBytes(DataVector2.x));
                ret.AddRange(BitConverter.GetBytes(DataVector2.y));
                break;
            case OXFileType.Vector3:
                ret.AddRange(BitConverter.GetBytes(DataVector3.x));
                ret.AddRange(BitConverter.GetBytes(DataVector3.y));
                ret.AddRange(BitConverter.GetBytes(DataVector3.z));
                break;
            case OXFileType.Quaternion:
                ret.AddRange(BitConverter.GetBytes(DataQuaternion.x));
                ret.AddRange(BitConverter.GetBytes(DataQuaternion.y));
                ret.AddRange(BitConverter.GetBytes(DataQuaternion.z));
                ret.AddRange(BitConverter.GetBytes(DataQuaternion.w));
                break;
            case OXFileType.Color:
                ret.AddRange(BitConverter.GetBytes(DataColor.r));
                ret.AddRange(BitConverter.GetBytes(DataColor.g));
                ret.AddRange(BitConverter.GetBytes(DataColor.b));
                ret.AddRange(BitConverter.GetBytes(DataColor.a));
                break;
            case OXFileType.Vector2Int:
                ret.AddRange(BitConverter.GetBytes(DataVector2Int.x));
                ret.AddRange(BitConverter.GetBytes(DataVector2Int.y));
                break;
            case OXFileType.Vector3Int:
                ret.AddRange(BitConverter.GetBytes(DataVector3Int.x));
                ret.AddRange(BitConverter.GetBytes(DataVector3Int.y));
                ret.AddRange(BitConverter.GetBytes(DataVector3Int.z));
                break;
            case OXFileType.Color32:
                ret.Add(DataColor32.r);
                ret.Add(DataColor32.g);
                ret.Add(DataColor32.b);
                ret.Add(DataColor32.a);
                break;
            case OXFileType.Texture:
                bytez = TextureToBytes(DataTexture, TextureLossy, TextureQuality);
                ret.AddRange(bytez);
                break;
            case OXFileType.Sound:
                bytez = AudioClipToBytes(DataSound, SoundLossless);
                ret.AddRange(bytez);
                break;
            case OXFileType.Mesh:
                bytez = MeshToBytes(DataMesh);
                ret.AddRange(bytez);
                break;
            case OXFileType.Custom:
                var bytez2 = DataCustom.GetBytes();
                ret.AddRange(bytez2);
                break;
            case OXFileType.Raw: //I dont think this will ever be called
                return DataRaw.ToList();
            case OXFileType.ListString:
                foreach (var li in DataListString)
                {
                    var ccc = Encoding.UTF8.GetBytes(li);
                    ret.AddRange(BitConverter.GetBytes(ccc.Length));
                    ret.AddRange(ccc);
                }
                break;
            case OXFileType.DictStringString:
                foreach (var li in DataDictStringString)
                {
                    var ccc = Encoding.UTF8.GetBytes(li.Key);
                    var ccc2 = Encoding.UTF8.GetBytes(li.Value);
                    ret.AddRange(BitConverter.GetBytes(ccc.Length));
                    ret.AddRange(BitConverter.GetBytes(ccc2.Length));
                    ret.AddRange(ccc);
                    ret.AddRange(ccc2);
                }
                break;
            case OXFileType.Bool:
                ret.Add((byte)(DataBool ? 69 : 0));
                break;
        }
        return ret;
    }

    private string Get_String()
    {
        return Encoding.UTF8.GetString(DataRaw);
    }
    private Texture2D Get_Texture()
    {
        return BytesToTexture(DataRaw);
    }
    private AudioClip Get_Sound()
    {
        return BytesToAudioClip(DataRaw);
    }
    private Mesh Get_Mesh()
    {
        return BytesToMesh(DataRaw);
    }
    private int Get_Int()
    {
        return BitConverter.ToInt32(DataRaw, 0);
    }
    private long Get_Long()
    {
        return BitConverter.ToInt64(DataRaw, 0);
    }
    private float Get_Float()
    {
        return BitConverter.ToSingle(DataRaw, 0);
    }
    private double Get_Double()
    {
        return BitConverter.ToDouble(DataRaw, 0);
    }
    private Vector2 Get_Vector2()
    {
        return new Vector2(
            BitConverter.ToSingle(DataRaw, 0),
            BitConverter.ToSingle(DataRaw, 4));
    }
    private Vector3 Get_Vector3()
    {
        return new Vector3(
            BitConverter.ToSingle(DataRaw, 0),
            BitConverter.ToSingle(DataRaw, 4),
            BitConverter.ToSingle(DataRaw, 8));
    }
    private Quaternion Get_Quaternion()
    {
        return new Quaternion(
            BitConverter.ToSingle(DataRaw, 0),
            BitConverter.ToSingle(DataRaw, 4),
            BitConverter.ToSingle(DataRaw, 8),
            BitConverter.ToSingle(DataRaw, 12));
    }
    private Color Get_Color()
    {
        return new Color(
            BitConverter.ToSingle(DataRaw, 0),
            BitConverter.ToSingle(DataRaw, 4),
            BitConverter.ToSingle(DataRaw, 8),
            BitConverter.ToSingle(DataRaw, 12));
    }
    private Vector2Int Get_Vector2Int()
    {
        return new Vector2Int(
            BitConverter.ToInt32(DataRaw, 0),
            BitConverter.ToInt32(DataRaw, 4));
    }
    private Vector3Int Get_Vector3Int()
    {
        return new Vector3Int(
            BitConverter.ToInt32(DataRaw, 0),
            BitConverter.ToInt32(DataRaw, 4),
            BitConverter.ToInt32(DataRaw, 8));
    }
    private Color32 Get_Color32()
    {
        return new Color32(DataRaw[0], DataRaw[1], DataRaw[2], DataRaw[3]);
    }

    public static Dictionary<string, Func<byte[], _IOXFile>> CustomFormats = new();

    private _IOXFile Get_Custom() => LinkCustomBytes(DataRaw);
    private static _IOXFile LinkCustomBytes(byte[] raw)
    {
        byte length = raw[0];
        byte[] selection = raw.SubArray(1, length);
        string id = Encoding.UTF8.GetString(selection);
        return CustomFormats[id](raw.SubArray(length + 1, raw.Length - length - 1));
    }
    private bool Get_Bool()
    {
        return DataRaw[0] == (byte)69;
    }
    public Dictionary<string, OXFileData> Get_OXFileData(FileData fd)
    {
        var ret = new Dictionary<string, OXFileData>();

        int index = 0;
        OXFileType stored = OXFileType.Repeat;
        int reps = 0;
        while (index + 1 < DataRaw.Length)
        {
            var cd = new OXFileData();
            if (reps > 0)
            {
                reps--;
                cd.Type = stored;
                cd.ExcludeCuzRepeated = true;
            }
            cd.Parse(DataRaw, index, fd);
            cd.pVersion = pVersion;
            ret.Add(cd.Name, cd);
            index += cd.LengthOffset;
            if (cd.RepeatRun > 0)
            {
                reps = cd.RepeatRun;
                reps += (repeatmax - 2);
                stored = cd.WireType; // wire type, since Int1 turns into Int after parsing
            }
            else if (cd.RepeatRun < 0)
            {
                reps = -cd.RepeatRun;
                reps--;
                stored = cd.WireType;
            }
        }

        return ret;
    }
    public List<OXFileData> Get_ListOXFileData(FileData fd)
    {
        var ret = new List<OXFileData>();
        int index = 0;

        while (index + 1 < DataRaw.Length)
        {
            var cd = new OXFileData().Parse(DataRaw, index, fd);
            cd.pVersion = pVersion;
            ret.Add(cd);
            index += cd.LengthOffset;
        }

        return ret;
    }

    private List<string> Get_ListString()
    {
        var ret = new List<string>();

        int index = 0;
        while (index + 3 < DataRaw.Length)
        {
            var length = BitConverter.ToInt32(DataRaw, index);
            index += 4;
            ret.Add(Encoding.UTF8.GetString(WankFuckYou(DataRaw, index, length)));
            index += length;
        }

        return ret;
    }

    private List<string> Get_ListString1()
    {
        var ret = new List<string>();

        int index = 0;
        while (index < DataRaw.Length)
        {
            int length = DataRaw[index];
            index++;
            ret.Add(Encoding.UTF8.GetString(WankFuckYou(DataRaw, index, length)));
            index += length;
        }

        return ret;
    }

    private Dictionary<string, string> Get_DictStringString1()
    {
        var ret = new Dictionary<string, string>();

        int index = 0;
        while (index + 1 < DataRaw.Length)
        {
            int length = DataRaw[index];
            int length2 = DataRaw[index + 1];
            index += 2;
            ret.Add(Encoding.UTF8.GetString(WankFuckYou(DataRaw, index, length)), Encoding.UTF8.GetString(WankFuckYou(DataRaw, index + length, length2)));
            index += length + length2;
        }

        return ret;
    }

    private Dictionary<string, string> Get_DictStringString()
    {
        var ret = new Dictionary<string, string>();

        int index = 0;
        while (index + 3 < DataRaw.Length)
        {
            var length = BitConverter.ToInt32(DataRaw, index);
            index += 4;
            var length2 = BitConverter.ToInt32(DataRaw, index);
            index += 4;
            ret.Add(Encoding.UTF8.GetString(WankFuckYou(DataRaw, index, length)), Encoding.UTF8.GetString(WankFuckYou(DataRaw, index + length, length2)));
            index += length + length2;
        }

        return ret;
    }

    public static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    // Payloads bigger than this use the fast gzip level (much quicker, slightly bigger output).
    // Smaller ones keep the Optimal level. Touch away.
    public static int CompressOptimalMaxBytes = 256 * 1024;

    public static byte[] Compress(byte[] data, System.IO.Compression.CompressionLevel level)
    {
        using var output = new MemoryStream(Math.Max(256, data.Length / 2));
        using (var gzip = new GZipStream(output, level, leaveOpen: true))
        {
            gzip.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    // Quick compressibility probe: gzip a few small samples spread across a big payload.
    // PNG/JPEG/ADPCM data barely shrinks, so compressing it again is wasted time.
    public static bool LooksIncompressible(byte[] data)
    {
        const int chunk = 4096;
        const int samples = 4;
        if (data.Length < 64 * 1024) return false;

        long inTotal = 0, outTotal = 0;
        for (int s = 0; s < samples; s++)
        {
            int start = (int)((long)(data.Length - chunk) * s / (samples - 1));
            using var ms = new MemoryStream(chunk + 64);
            using (var gz = new GZipStream(ms, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
            {
                gz.Write(data, start, chunk);
            }
            inTotal += chunk;
            outTotal += ms.Length;
        }
        return outTotal > inTotal * 95 / 100;
    }

    // Returns the gzip'd data, or null if it isn't worth it (looks incompressible, or didn't come out smaller).
    public static byte[] CompressIfWorthIt(byte[] data)
    {
        if (LooksIncompressible(data)) return null;
        var level = data.Length > CompressOptimalMaxBytes
            ? System.IO.Compression.CompressionLevel.Fastest
            : System.IO.Compression.CompressionLevel.Optimal;
        byte[] packed = Compress(data, level);
        return packed.Length < data.Length ? packed : null;
    }

    public static byte[] Decompress(byte[] compressedData) => Decompress(compressedData, 0, compressedData.Length);

    // Decompresses a slice of an array without copying the slice out first. The gzip footer stores the
    // original size, so the output buffer is allocated once at the right size (no growth copies, and no
    // final ToArray copy when it matches).
    public static byte[] Decompress(byte[] data, int offset, int count)
    {
        try
        {
            int expected = 0;
            if (count >= 18)
            {
                expected = BitConverter.ToInt32(data, offset + count - 4);
                if (expected < 0 || expected > (1 << 30)) expected = 0;
            }
            using var input = new MemoryStream(data, offset, count, false);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream(expected > 0 ? expected : Math.Max(256, count * 3));
            gzip.CopyTo(output, 1 << 16);
            if (output.Length == output.Capacity) return output.GetBuffer();
            return output.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
    private const string SuperSecretKey = "B!d&llp)897633G%^&*g576iyu";
    private static readonly byte[] SuperSecretKeyBytes = Encoding.UTF8.GetBytes(SuperSecretKey);

    // The per-byte key is (byte)((i + 100) ^ key[i % keyLen]), which repeats every lcm(256, keyLen) bytes
    // (3328 for the current key). We build that repeating stream once and XOR 8 bytes at a time with it,
    // instead of doing a modulo + xor per byte. Output is byte-for-byte identical to the old version.
    private static readonly byte[] XorKeyStream = BuildXorKeyStream();

    private static byte[] BuildXorKeyStream()
    {
        int bLen = SuperSecretKeyBytes.Length;
        int a = 256, b = bLen;
        while (b != 0) { int t = a % b; a = b; b = t; }
        int period = 256 / a * bLen; // lcm(256, bLen), always a multiple of 256 (so of 8)
        var ks = new byte[period];
        for (int i = 0; i < period; i++)
        {
            ks[i] = (byte)((i + 100) ^ SuperSecretKeyBytes[i % bLen]);
        }
        return ks;
    }

    // XORs data[offset .. offset+length) in place, with the key stream starting at position 0 of that range
    private static void XorRange(byte[] data, int offset, int length)
    {
        byte[] ks = XorKeyStream;
        int period = ks.Length;
        Span<ulong> ks64 = MemoryMarshal.Cast<byte, ulong>(ks.AsSpan());
        int words = ks64.Length;

        Span<byte> span = data.AsSpan(offset, length);
        int whole = length & ~7;
        Span<ulong> d64 = MemoryMarshal.Cast<byte, ulong>(span.Slice(0, whole));
        int k = 0;
        for (int i = 0; i < d64.Length; i++)
        {
            d64[i] ^= ks64[k];
            if (++k == words) k = 0;
        }
        for (int i = whole; i < length; i++)
        {
            span[i] ^= ks[i % period];
        }
    }

    private static byte[] XorObfuscate(byte[] data)
    {
        XorRange(data, 0, data.Length);
        return data;
    }
    public static byte[] Obfuscate(byte[] data) => XorObfuscate(data);
    public static byte[] DeObfuscate(byte[] data) => XorObfuscate(data);
    // In-place on part of a bigger buffer (used by ReadFile to skip the slice copy)
    public static void DeObfuscate(byte[] data, int offset, int length) => XorRange(data, offset, length);

    [Flags]
    private enum MeshDataFlags : byte
    {
        None = 0,
        Normals = 1,
        UV = 2,
        Colors = 4,
        Tangents = 8,
    }

    // Packs a Unity Mesh (positions, optional normals/uv/vertex colors/tangents, and all submeshes with their topology)
    // Everything MeshToBytes needs, copied out of the Unity Mesh. Capturing it is main-thread only,
    // turning it into bytes (MeshSnapshotToBytes) is pure managed code and can run on any thread.
    public sealed class MeshSnapshot
    {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector2[] UV;
        public Color[] Colors;
        public Vector4[] Tangents;
        public int[][] Indices;
        public MeshTopology[] Topology;
    }

    public static MeshSnapshot CaptureMesh(Mesh mesh)
    {
        var snap = new MeshSnapshot
        {
            Vertices = mesh.vertices,
            Normals = mesh.normals,
            UV = mesh.uv,
            Colors = mesh.colors,
            Tangents = mesh.tangents,
        };
        int subMeshCount = mesh.subMeshCount;
        snap.Indices = new int[subMeshCount][];
        snap.Topology = new MeshTopology[subMeshCount];
        for (int s = 0; s < subMeshCount; s++)
        {
            snap.Indices[s] = mesh.GetTriangles(s);
            snap.Topology[s] = mesh.GetTopology(s);
        }
        return snap;
    }

    public static byte[] MeshToBytes(Mesh mesh) => MeshSnapshotToBytes(CaptureMesh(mesh));

    public static byte[] MeshSnapshotToBytes(MeshSnapshot m)
    {
        Vector3[] vertices = m.Vertices;
        Vector3[] normals = m.Normals;
        Vector2[] uv = m.UV;
        Color[] colors = m.Colors;
        Vector4[] tangents = m.Tangents;

        MeshDataFlags flags = MeshDataFlags.None;
        if (normals != null && normals.Length == vertices.Length) flags |= MeshDataFlags.Normals;
        if (uv != null && uv.Length == vertices.Length) flags |= MeshDataFlags.UV;
        if (colors != null && colors.Length == vertices.Length) flags |= MeshDataFlags.Colors;
        if (tangents != null && tangents.Length == vertices.Length) flags |= MeshDataFlags.Tangents;

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(vertices.Length);
            foreach (var v in vertices)
            {
                writer.Write(v.x); writer.Write(v.y); writer.Write(v.z);
            }

            writer.Write((byte)flags);

            if ((flags & MeshDataFlags.Normals) != 0)
            {
                foreach (var n in normals) { writer.Write(n.x); writer.Write(n.y); writer.Write(n.z); }
            }
            if ((flags & MeshDataFlags.UV) != 0)
            {
                foreach (var u in uv) { writer.Write(u.x); writer.Write(u.y); }
            }
            if ((flags & MeshDataFlags.Colors) != 0)
            {
                foreach (var c in colors) { writer.Write(c.r); writer.Write(c.g); writer.Write(c.b); writer.Write(c.a); }
            }
            if ((flags & MeshDataFlags.Tangents) != 0)
            {
                foreach (var t in tangents) { writer.Write(t.x); writer.Write(t.y); writer.Write(t.z); writer.Write(t.w); }
            }

            writer.Write(m.Indices.Length);
            for (int s = 0; s < m.Indices.Length; s++)
            {
                int[] indices = m.Indices[s];
                writer.Write((byte)m.Topology[s]);
                writer.Write(indices.Length);
                foreach (var idx in indices) writer.Write(idx);
            }
            return stream.ToArray();
        }
    }

    // Mesh bytes parsed into plain arrays (pure managed, any thread). BuildMesh turns it into a Mesh (main thread).
    public sealed class ParsedMesh
    {
        public int VertexCount;
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector2[] UV;
        public Color[] Colors;
        public Vector4[] Tangents;
        public int[][] Indices;
        public MeshTopology[] Topology;
    }

    public static ParsedMesh ParseMeshBytes(byte[] raw)
    {
        using (MemoryStream stream = new MemoryStream(raw))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            var pm = new ParsedMesh();
            int vertexCount = reader.ReadInt32();
            pm.VertexCount = vertexCount;
            pm.Vertices = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                pm.Vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            MeshDataFlags flags = (MeshDataFlags)reader.ReadByte();

            if ((flags & MeshDataFlags.Normals) != 0)
            {
                pm.Normals = new Vector3[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    pm.Normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
            if ((flags & MeshDataFlags.UV) != 0)
            {
                pm.UV = new Vector2[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    pm.UV[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            if ((flags & MeshDataFlags.Colors) != 0)
            {
                pm.Colors = new Color[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    pm.Colors[i] = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
            if ((flags & MeshDataFlags.Tangents) != 0)
            {
                pm.Tangents = new Vector4[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    pm.Tangents[i] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            int subMeshCount = reader.ReadInt32();
            pm.Indices = new int[subMeshCount][];
            pm.Topology = new MeshTopology[subMeshCount];
            for (int s = 0; s < subMeshCount; s++)
            {
                pm.Topology[s] = (MeshTopology)reader.ReadByte();
                int indexCount = reader.ReadInt32();
                int[] indices = new int[indexCount];
                for (int i = 0; i < indexCount; i++) indices[i] = reader.ReadInt32();
                pm.Indices[s] = indices;
            }
            return pm;
        }
    }

    public static Mesh BuildMesh(ParsedMesh pm, string meshName = "loadedMesh")
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName;
        // Large meshes (>65535 verts) need a 32-bit index format nya
        mesh.indexFormat = pm.VertexCount > 65535
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.vertices = pm.Vertices;
        if (pm.Normals != null) mesh.normals = pm.Normals;
        if (pm.UV != null) mesh.uv = pm.UV;
        if (pm.Colors != null) mesh.colors = pm.Colors;
        if (pm.Tangents != null) mesh.tangents = pm.Tangents;

        mesh.subMeshCount = pm.Indices.Length;
        for (int s = 0; s < pm.Indices.Length; s++)
        {
            mesh.SetIndices(pm.Indices[s], pm.Topology[s], s);
        }

        if (pm.Normals == null) mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh BytesToMesh(byte[] raw, string meshName = "loadedMesh") => BuildMesh(ParseMeshBytes(raw), meshName);

    // Texture container formats, told apart by the first byte so old files keep loading:
    //   0x89 ... = PNG (lossless, what OX always wrote before)
    //   0xFF 0xD8 = plain JPEG (lossy, no alpha)
    //   0xAE = JPEG color + gzip'd lossless 8-bit alpha plane (lossy color, exact alpha)
    private const byte TextureMagicJpegAlpha = 0xAE;

    static readonly ProfilerMarker pmPngEncode = new ProfilerMarker("OX.Texture.EncodePNG");
    static readonly ProfilerMarker pmJpgEncode = new ProfilerMarker("OX.Texture.EncodeJPG");
    static readonly ProfilerMarker pmPixelScan = new ProfilerMarker("OX.Texture.PixelScan");
    static readonly ProfilerMarker pmTexDecode = new ProfilerMarker("OX.Texture.Decode");

    // One pass over the pixels: does anything have transparency, and are there only a few distinct colors
    // (flat art / pixel art, where PNG can beat JPEG)? Stops early once both questions are answered.
    private static void ScanPixels(Color32[] px, out bool hasAlpha, out bool fewColors)
    {
        hasAlpha = false;
        var seen = new HashSet<uint>();
        bool counting = true;
        uint last = 0;
        bool haveLast = false;
        for (int i = 0; i < px.Length; i++)
        {
            Color32 c = px[i];
            if (c.a != 255) hasAlpha = true;
            if (counting)
            {
                uint key = (uint)c.r | ((uint)c.g << 8) | ((uint)c.b << 16) | ((uint)c.a << 24);
                if (!haveLast || key != last) // runs of the same color are free
                {
                    seen.Add(key);
                    last = key;
                    haveLast = true;
                    if (seen.Count > 256) counting = false;
                }
            }
            if (hasAlpha && !counting) break;
        }
        fewColors = counting;
    }

    private static void DestroyTemp(UnityEngine.Object o)
    {
        if (o == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(o);
        else UnityEngine.Object.DestroyImmediate(o);
    }

    // Encodes a texture. lossy = false -> PNG exactly like before.
    // lossy = true -> JPEG at the given quality (1..100). If the texture has any transparency,
    // the alpha is kept losslessly in a side plane. If PNG would somehow be smaller than the
    // lossy result (flat colors / pixel art), PNG is used instead since it's smaller AND exact nya.
    public static byte[] TextureToBytes(Texture2D tex, bool lossy = false, int quality = 75)
    {
        // PNG encoding is by far the slowest step here, so lossy mode only runs it when PNG can plausibly win
        // (few distinct colors, or the JPEG came out big, which means sharp edges that JPEG handles badly).
        if (!lossy)
        {
            using (pmPngEncode.Auto()) return tex.EncodeToPNG();
        }

        quality = Mathf.Clamp(quality, 1, 100);

        Color32[] px;
        bool hasAlpha, fewColors;
        using (pmPixelScan.Auto())
        {
            px = tex.GetPixels32();
            ScanPixels(px, out hasAlpha, out fewColors);
        }

        byte[] jpg;
        using (pmJpgEncode.Auto()) jpg = tex.EncodeToJPG(quality);

        byte[] result = jpg;
        if (hasAlpha)
        {
            byte[] alpha = new byte[px.Length];
            for (int i = 0; i < px.Length; i++) alpha[i] = px[i].a;
            byte[] alphaCompressed = Compress(alpha, System.IO.Compression.CompressionLevel.Fastest);

            result = new byte[1 + 4 + jpg.Length + alphaCompressed.Length];
            result[0] = TextureMagicJpegAlpha;
            BitConverter.GetBytes(jpg.Length).CopyTo(result, 1);
            jpg.CopyTo(result, 5);
            alphaCompressed.CopyTo(result, 5 + jpg.Length);
        }

        bool bigForJpeg = (long)result.Length * 8 > (long)px.Length * 5 / 2; // more than ~2.5 bits per pixel
        if (fewColors || bigForJpeg)
        {
            byte[] png;
            using (pmPngEncode.Auto()) png = tex.EncodeToPNG();
            if (png.Length <= result.Length) return png;
        }
        return result;
    }

    // Same result as the lossy half of TextureToBytes, but starting from pixels that were already copied
    // out of the texture, so it only uses thread-safe APIs and can run on a worker thread.
    // (ImageConversion.EncodeArrayToPNG is documented as thread safe by Unity.)
    private static byte[] EncodeLossyFromPixels(Color32[] px, int w, int h, int quality)
    {
        const UnityEngine.Experimental.Rendering.GraphicsFormat rgba = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        quality = Math.Clamp(quality, 1, 100);

        bool hasAlpha, fewColors;
        using (pmPixelScan.Auto()) ScanPixels(px, out hasAlpha, out fewColors);

        byte[] rgbaBytes = MemoryMarshal.Cast<Color32, byte>(px.AsSpan()).ToArray();

        byte[] jpg;
        using (pmJpgEncode.Auto()) jpg = ImageConversion.EncodeArrayToJPG(rgbaBytes, rgba, (uint)w, (uint)h, 0u, quality);

        byte[] result = jpg;
        if (hasAlpha)
        {
            byte[] alpha = new byte[px.Length];
            for (int i = 0; i < px.Length; i++) alpha[i] = px[i].a;
            byte[] alphaCompressed = Compress(alpha, System.IO.Compression.CompressionLevel.Fastest);

            result = new byte[1 + 4 + jpg.Length + alphaCompressed.Length];
            result[0] = TextureMagicJpegAlpha;
            BitConverter.GetBytes(jpg.Length).CopyTo(result, 1);
            jpg.CopyTo(result, 5);
            alphaCompressed.CopyTo(result, 5 + jpg.Length);
        }

        bool bigForJpeg = (long)result.Length * 8 > (long)px.Length * 5 / 2; // more than ~2.5 bits per pixel
        if (fewColors || bigForJpeg)
        {
            byte[] png;
            using (pmPngEncode.Auto()) png = ImageConversion.EncodeArrayToPNG(rgbaBytes, rgba, (uint)w, (uint)h, 0u);
            if (png.Length <= result.Length) return png;
        }
        return result;
    }

    // Texture bytes split into the part that needs no Unity API (PrepareTexturePayload: slicing the JPEG out and
    // gunzipping the alpha plane, any thread) and the part that does (BuildTexture, main thread only).
    public sealed class TexturePayload
    {
        public byte[] Jpg;   // set for the JPEG + alpha plane container
        public byte[] Alpha;
        public byte[] Plain; // PNG or plain JPEG, LoadImage sniffs it itself
    }

    public static TexturePayload PrepareTexturePayload(byte[] bytes)
    {
        if (bytes.Length > 5 && bytes[0] == TextureMagicJpegAlpha)
        {
            int jpgLength = BitConverter.ToInt32(bytes, 1);
            byte[] jpg = new byte[jpgLength];
            Buffer.BlockCopy(bytes, 5, jpg, 0, jpgLength);
            // decompress the alpha plane straight out of the buffer, no slice copy
            byte[] alpha = Decompress(bytes, 5 + jpgLength, bytes.Length - 5 - jpgLength);
            return new TexturePayload { Jpg = jpg, Alpha = alpha };
        }
        return new TexturePayload { Plain = bytes };
    }

    public static Texture2D BuildTexture(TexturePayload payload)
    {
        // JPEG + separate alpha plane container
        if (payload.Jpg != null)
        {
            using (pmTexDecode.Auto())
            {
                // LoadImage turns a JPEG texture into RGB24 (no alpha channel), so decode into a scratch
                // texture and build the final RGBA32 one from it with the alpha plane filled back in.
                var rgb = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!rgb.LoadImage(payload.Jpg))
                {
                    DestroyTemp(rgb);
                    Debug.LogError("BytesToTexture: failed to load lossy image data!");
                    return null;
                }

                Color32[] px = rgb.GetPixels32();
                int w = rgb.width, h = rgb.height;
                DestroyTemp(rgb);

                byte[] alpha = payload.Alpha;
                int n = Math.Min(px.Length, alpha.Length);
                for (int i = 0; i < n; i++) px[i].a = alpha[i];

                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply();
                return tex;
            }
        }

        // PNG or plain JPEG, LoadImage sniffs it itself
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        bool success;
        using (pmTexDecode.Auto()) success = texture.LoadImage(payload.Plain);

        if (!success)
        {
            Debug.LogError("BytesToTexture: failed to load image data!");
            return null;
        }

        return texture;
    }

    public static Texture2D BytesToTexture(byte[] bytes) => BuildTexture(PrepareTexturePayload(bytes));


    // Marker bytes identifying OX's own sound containers. Both are distinct from any
    // legal WAV first byte ('R' = 0x52 for "RIFF"), so old .ox files saved before any
    // of this existed (raw uncompressed WAV) still decode correctly nya.
    private const byte SoundMagicLossless = 0xAC; // delta + gzip, bit-exact
    private const byte SoundMagicLossy = 0xAD;    // IMA ADPCM + gzip, ~4x smaller, tiny quality loss

    // Standard IMA ADPCM tables (public-domain algorithm, our own implementation) uwu
    private static readonly int[] ImaIndexTable = { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8 };
    private static readonly int[] ImaStepTable =
    {
        7,8,9,10,11,12,13,14,16,17,19,21,23,25,28,31,34,37,41,45,50,55,60,66,73,80,88,97,107,118,130,143,157,173,190,209,230,253,279,307,337,371,408,449,494,544,598,658,724,796,876,963,1060,1166,1282,1411,1552,1707,1878,2066,2272,2499,2749,3024,3327,3660,4026,4428,4871,5358,5894,6484,7132,7845,8630,9493,10442,11487,12635,13899,15289,16818,18500,20350,22385,24623,27086,29794,32767
    };

    // Convert an AudioClip into a compressed byte array uwu. lossless = true keeps every
    // sample bit-exact (delta+gzip); lossless = false (default) uses IMA ADPCM, which
    // packs each 16-bit sample into 4 bits (~4x smaller) for maximum space saving, with
    // a small, usually inaudible amount of quality loss.
    public static byte[] AudioClipToBytes(AudioClip clip, bool lossless = false)
    {
        // Grab raw float samples from the clip (main thread only)
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        int channels = Mathf.Max(1, clip.channels);
        return AudioSamplesToBytes(samples, channels, clip.frequency, lossless);
    }

    // The pure managed half of AudioClipToBytes (PCM conversion, ADPCM / delta encode, gzip). Any thread.
    public static byte[] AudioSamplesToBytes(float[] samples, int channels, int frequency, bool lossless)
    {

        // Convert float samples (-1f to 1f) into 16-bit PCM shorts
        const float rescaleFactor = 32767f; // to convert float to Int16
        short[] pcm = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            pcm[i] = (short)(samples[i] * rescaleFactor);
        }

        if (!lossless)
        {
            byte[] adpcm = EncodeImaAdpcm(pcm, channels);
            byte[] adpcmCompressed = Compress(adpcm);

            byte[] lossyResult = new byte[17 + adpcmCompressed.Length];
            lossyResult[0] = SoundMagicLossy;
            BitConverter.GetBytes(channels).CopyTo(lossyResult, 1);
            BitConverter.GetBytes(frequency).CopyTo(lossyResult, 5);
            BitConverter.GetBytes(pcm.Length).CopyTo(lossyResult, 9);
            BitConverter.GetBytes(adpcm.Length).CopyTo(lossyResult, 13);
            adpcmCompressed.CopyTo(lossyResult, 17);
            return lossyResult;
        }

        // Order-1 delta encode per channel (lossless, wraps safely and un-wraps exactly)
        short[] delta = new short[pcm.Length];
        for (int ch = 0; ch < channels; ch++)
        {
            short prev = 0;
            for (int i = ch; i < pcm.Length; i += channels)
            {
                short cur = pcm[i];
                delta[i] = (short)(cur - prev);
                prev = cur;
            }
        }

        byte[] deltaBytes = new byte[delta.Length * 2];
        for (int i = 0; i < delta.Length; i++)
        {
            BitConverter.GetBytes(delta[i]).CopyTo(deltaBytes, i * 2);
        }

        // Build the WAV file header + delta data, then gzip the whole thing
        byte[] wav = WriteWavHeader(deltaBytes, channels, frequency);
        byte[] compressed = Compress(wav);

        byte[] result = new byte[5 + compressed.Length];
        result[0] = SoundMagicLossless;
        BitConverter.GetBytes(wav.Length).CopyTo(result, 1);
        compressed.CopyTo(result, 5);
        return result;
    }

    // Convert a byte array back into an AudioClip nya~
    // Handles the lossless delta+gzip container, the lossy ADPCM container, and legacy
    // raw WAV bytes (anything saved before compression existed), so old .ox files keep working.
    public static DecodedAudio DecodeAudioBytes(byte[] wavBytes)
    {
        if (wavBytes.Length > 17 && wavBytes[0] == SoundMagicLossy)
        {
            int channels = BitConverter.ToInt32(wavBytes, 1);
            int frequency = BitConverter.ToInt32(wavBytes, 5);
            int totalSamples = BitConverter.ToInt32(wavBytes, 9);
            int adpcmLength = BitConverter.ToInt32(wavBytes, 13);

            byte[] compressed = new byte[wavBytes.Length - 17];
            Array.Copy(wavBytes, 17, compressed, 0, compressed.Length);
            byte[] adpcm = Decompress(compressed);
            if (adpcm.Length != adpcmLength)
            {
                Debug.LogWarning("BytesToAudioClip: decompressed ADPCM size mismatch owo, data might be corrupt!");
            }

            short[] pcm = DecodeImaAdpcm(adpcm, channels, totalSamples);
            float[] lossySamples = new float[pcm.Length];
            for (int i = 0; i < pcm.Length; i++)
            {
                lossySamples[i] = pcm[i] / 32767f;
            }

            return new DecodedAudio { Samples = lossySamples, Channels = channels, Frequency = frequency };
        }

        bool deltaEncoded = wavBytes.Length > 5 && wavBytes[0] == SoundMagicLossless;
        byte[] wav;

        if (deltaEncoded)
        {
            int originalLength = BitConverter.ToInt32(wavBytes, 1);
            byte[] compressed = new byte[wavBytes.Length - 5];
            Array.Copy(wavBytes, 5, compressed, 0, compressed.Length);
            wav = Decompress(compressed);
            if (wav.Length != originalLength)
            {
                Debug.LogWarning("BytesToAudioClip: decompressed sound size mismatch owo, data might be corrupt!");
            }
        }
        else
        {
            wav = wavBytes; // legacy uncompressed WAV, read as-is
        }

        // Parse WAV header
        int wavChannels = BitConverter.ToInt16(wav, 22);
        int wavFrequency = BitConverter.ToInt32(wav, 24);

        // Find "data" chunk (skips over any extra chunks safely)
        int dataChunkPos = FindDataChunk(wav);
        if (dataChunkPos < 0)
        {
            Debug.LogError("BytesToAudioClip: could not find data chunk owo!");
            return null;
        }

        int dataSize = BitConverter.ToInt32(wav, dataChunkPos + 4);
        int sampleStart = dataChunkPos + 8;

        int sampleCount = dataSize / 2; // 16-bit = 2 bytes per sample
        float[] samples = new float[sampleCount];

        if (deltaEncoded)
        {
            // Undo the per-channel delta (cumulative sum) before normalizing to float
            short[] prev = new short[wavChannels];
            for (int i = 0; i < sampleCount; i++)
            {
                int ch = i % wavChannels;
                short d = BitConverter.ToInt16(wav, sampleStart + i * 2);
                short cur = (short)(prev[ch] + d);
                prev[ch] = cur;
                samples[i] = cur / 32767f;
            }
        }
        else
        {
            for (int i = 0; i < sampleCount; i++)
            {
                short sampleShort = BitConverter.ToInt16(wav, sampleStart + i * 2);
                samples[i] = sampleShort / 32767f;
            }
        }

        return new DecodedAudio { Samples = samples, Channels = wavChannels, Frequency = wavFrequency };
    }

    // Decoded float samples, ready to hand to AudioClip.Create (main thread only)
    public static AudioClip DecodedToAudioClip(DecodedAudio audio, string clipName = "loadedClip")
    {
        if (audio == null) return null;
        AudioClip clip = AudioClip.Create(clipName, audio.Samples.Length / audio.Channels, audio.Channels, audio.Frequency, false);
        clip.SetData(audio.Samples, 0);
        return clip;
    }

    public static AudioClip BytesToAudioClip(byte[] wavBytes, string clipName = "loadedClip") => DecodedToAudioClip(DecodeAudioBytes(wavBytes), clipName);

    public sealed class DecodedAudio
    {
        public float[] Samples;
        public int Channels;
        public int Frequency;
    }


    // Encodes interleaved 16-bit PCM into 4-bit-per-sample IMA ADPCM, one predictor/step
    // pair per channel so multi-channel audio doesn't bleed state between channels.
    private static byte[] EncodeImaAdpcm(short[] pcm, int channels)
    {
        int[] predictor = new int[channels];
        int[] stepIndex = new int[channels];
        byte[] output = new byte[(pcm.Length + 1) / 2];
        bool highNibble = false;
        int outPos = 0;
        byte pending = 0;

        for (int i = 0; i < pcm.Length; i++)
        {
            int ch = i % channels;
            byte nibble = EncodeImaSample(pcm[i], ref predictor[ch], ref stepIndex[ch]);

            if (!highNibble)
            {
                pending = nibble;
                highNibble = true;
            }
            else
            {
                output[outPos++] = (byte)(pending | (nibble << 4));
                highNibble = false;
            }
        }
        if (highNibble)
        {
            output[outPos] = pending; // trailing lone nibble, high bits stay 0
        }
        return output;
    }

    private static short[] DecodeImaAdpcm(byte[] adpcm, int channels, int totalSamples)
    {
        int[] predictor = new int[channels];
        int[] stepIndex = new int[channels];
        short[] pcm = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            int ch = i % channels;
            byte packed = adpcm[i / 2];
            byte nibble = (i % 2 == 0) ? (byte)(packed & 0x0F) : (byte)((packed >> 4) & 0x0F);
            pcm[i] = DecodeImaSample(nibble, ref predictor[ch], ref stepIndex[ch]);
        }
        return pcm;
    }

    private static byte EncodeImaSample(short sample, ref int predictor, ref int stepIndex)
    {
        int step = ImaStepTable[stepIndex];
        int diff = sample - predictor;
        int sign = 0;
        if (diff < 0) { sign = 8; diff = -diff; }

        int delta = 0;
        int vpdiff = step >> 3;
        if (diff >= step) { delta = 4; diff -= step; vpdiff += step; }
        step >>= 1;
        if (diff >= step) { delta |= 2; diff -= step; vpdiff += step; }
        step >>= 1;
        if (diff >= step) { delta |= 1; vpdiff += step; }

        predictor = sign != 0 ? predictor - vpdiff : predictor + vpdiff;
        predictor = Math.Clamp(predictor, -32768, 32767);

        stepIndex = Math.Clamp(stepIndex + ImaIndexTable[delta | sign], 0, ImaStepTable.Length - 1);

        return (byte)(delta | sign);
    }

    private static short DecodeImaSample(byte nibble, ref int predictor, ref int stepIndex)
    {
        int step = ImaStepTable[stepIndex];
        int diff = step >> 3;
        if ((nibble & 4) != 0) diff += step;
        if ((nibble & 2) != 0) diff += step >> 1;
        if ((nibble & 1) != 0) diff += step >> 2;
        if ((nibble & 8) != 0) diff = -diff;

        predictor = Math.Clamp(predictor + diff, -32768, 32767);
        stepIndex = Math.Clamp(stepIndex + ImaIndexTable[nibble], 0, ImaStepTable.Length - 1);

        return (short)predictor;
    }

    private static byte[] WriteWavHeader(byte[] pcmData, int channels, int frequency)
    {
        int byteRate = frequency * channels * 2; // 16-bit
        int blockAlign = channels * 2;
        int dataSize = pcmData.Length;
        int fileSize = 36 + dataSize;

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size for PCM
            writer.Write((short)1); // AudioFormat = 1 (PCM)
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)16); // bits per sample

            // data chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
            writer.Write(pcmData);

            return stream.ToArray();
        }
    }

    private static int FindDataChunk(byte[] wavBytes)
    {
        // Search for "data" tag starting after the standard 12-byte RIFF/WAVE header
        for (int i = 12; i < wavBytes.Length - 4; i++)
        {
            if (wavBytes[i] == 'd' && wavBytes[i + 1] == 'a' && wavBytes[i + 2] == 't' && wavBytes[i + 3] == 'a')
            {
                return i;
            }
        }
        return -1;
    }
    private byte[] WankFuckYou(byte[] array, int offset, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }
    // ---- async write support -------------------------------------------------------------------
    // Finds the nodes whose bytes are expensive or touch Unity objects and have nothing cached yet.
    internal static void CollectEncodeTargets(OXFileData node, List<OXFileData> into, bool isRoot)
    {
        if (node == null) return;
        bool cached = node.DataRaw != null && node.DataRaw.Length > 0;
        switch (node.Type)
        {
            case OXFileType.OXFileData:
                if (cached && !isRoot) return;
                if (node.DataOXFiles != null)
                    foreach (var kv in node.DataOXFiles) CollectEncodeTargets(kv.Value, into, false);
                break;
            case OXFileType.ListOXFileData:
                if (cached) return;
                if (node.DataListOXFiles != null)
                    foreach (var c in node.DataListOXFiles) CollectEncodeTargets(c, into, false);
                break;
            case OXFileType.Texture:
            case OXFileType.Sound:
            case OXFileType.Mesh:
            case OXFileType.Custom:
                if (!cached) into.Add(node);
                break;
        }
    }

    // MAIN THREAD part of encoding this node: copies what's needed out of the Unity object (or runs the
    // user's custom GetBytes). Returns the remaining work as a function that only uses thread-safe code,
    // so the caller can run it on a worker. Returns null when there's nothing to pre-encode.
    internal Func<byte[]> BeginEncode()
    {
        switch (Type)
        {
            case OXFileType.Texture:
                {
                    Texture2D tex = DataTexture;
                    if (tex == null) return null;
                    int w = tex.width, h = tex.height;
                    if (!TextureLossy)
                    {
                        var fmt = tex.graphicsFormat;
                        byte[] rawPixels = tex.GetRawTextureData();
                        return () => ImageConversion.EncodeArrayToPNG(rawPixels, fmt, (uint)w, (uint)h, 0u);
                    }
                    Color32[] px = tex.GetPixels32();
                    int quality = TextureQuality;
                    return () => EncodeLossyFromPixels(px, w, h, quality);
                }
            case OXFileType.Sound:
                {
                    AudioClip clip = DataSound;
                    if (clip == null) return null;
                    float[] samples = new float[clip.samples * clip.channels];
                    clip.GetData(samples, 0);
                    int channels = Mathf.Max(1, clip.channels);
                    int frequency = clip.frequency;
                    bool lossless = SoundLossless;
                    return () => AudioSamplesToBytes(samples, channels, frequency, lossless);
                }
            case OXFileType.Mesh:
                {
                    Mesh mesh = DataMesh;
                    if (mesh == null) return null;
                    MeshSnapshot snap = CaptureMesh(mesh);
                    return () => MeshSnapshotToBytes(snap);
                }
            case OXFileType.Custom:
                {
                    if (DataCustom == null) return null;
                    byte[] bytes = DataCustom.GetBytes().ToArray(); // user code, keep it on the main thread
                    return () => bytes;
                }
        }
        return null;
    }

    public bool ContainsKey(string name)
    {
        if (Type != OXFileType.OXFileData) return false;
        return DataOXFiles.ContainsKey(name);
    }
    public int Count()
    {
        switch (this.Type)
        {
            case OXFileType.ListOXFileData: return DataListOXFiles.Count;
            default: return DataOXFiles.Count;
        }
    }
    private static string DictToDisplayString<T>(Dictionary<string, T> dict)
    {
        if (dict == null) return "";
        var sb = new StringBuilder();
        foreach (var kv in dict)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(kv.Key).Append(": ").Append(kv.Value);
        }
        return sb.ToString();
    }
    public override string ToString()
    {
        switch (Type)
        {
            case OXFileType.String: return DataString;
            case OXFileType.Int: return DataInt.ToString();
            case OXFileType.Float: return DataFloat.ToString();
            case OXFileType.Long: return DataLong.ToString();
            case OXFileType.Double: return DataDouble.ToString();
            case OXFileType.Bool: return DataBool.ToString();
            case OXFileType.ListString: return Converter.ListToString(DataListString);
            case OXFileType.DictStringString: return Converter.DictionaryToString(DataDictStringString);
            case OXFileType.DictStringInt: return DictToDisplayString(DataDictStringInt);
            case OXFileType.DictStringLong: return DictToDisplayString(DataDictStringLong);
            case OXFileType.DictStringDouble: return DictToDisplayString(DataDictStringDouble);
            case OXFileType.DictStringFloat: return DictToDisplayString(DataDictStringFloat);
            case OXFileType.Custom: return DataCustom.ToString();
            default: return "Error";
        }
    }
}

public class FileData
{
    public OXFile File;
    public int CurrentStep = 0;

    // Async read support: when Deferred is true, Parse() does all the pure-managed work right away
    // but pushes anything that needs a Unity main-thread API (Texture2D / AudioClip / Mesh creation,
    // custom formats) into Pending as a closure, to be run later on the main thread.
    internal bool Deferred = false;
    internal List<Action> Pending = new List<Action>();
}

public static class OXFileLoader
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitFiles()
    {
        OXFileData.CustomFormats.Clear();
        var g = RandomFunctions.GetListOfInheritors<_IOXFile>();
        foreach (var f in g)
        {
            OXFileData.CustomFormats.Add(f.OXF_GetIdentifier(), f.Link);
        }
    }
}

public interface _IOXFile
{
    string OXF_GetIdentifier();
    byte[] OXF_GetBytes();
    _IOXFile Link(byte[] data);

    virtual List<byte> GetBytes()
    {
        var oxconfirm = Encoding.UTF8.GetBytes(OXF_GetIdentifier());
        byte length = (byte)oxconfirm.Length;
        var li = oxconfirm.ToList();
        li.Insert(0, length);
        var d = OXF_GetBytes();
        foreach (byte b in d)
        {
            li.Add(b);
        }
        return li;
    }
}
public interface IOXFile_SaveLoadable<T> : _IOXFile where T : IOXFile_SaveLoadable<T>
{
    T OXF_CreateInstanceFromBytes(byte[] data);
    _IOXFile _IOXFile.Link(byte[] data)
    {
        return OXF_CreateInstanceFromBytes(data);
    }
}