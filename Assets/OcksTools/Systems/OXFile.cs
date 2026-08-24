using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using WebSocketSharp;
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
    public int FileVersion = -1;
    public OXFileData Data = new OXFileData(OXFileData.OXFileType.OXFileData);
    public Dictionary<string, byte> NameLinker = new();
    public Dictionary<byte, string> IndexLinker = new();

    public OXFile LinkOptimizer(Dictionary<string, byte> n)
    {
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
        var cd = File.ReadAllBytes(str);
        if (cd.Length < 4) return false;
        int index = 0;
        Flags = BitConverter.ToInt32(cd, index);
        SetVersionFromFlag();
        index += 4;
        Data = new OXFileData(OXFileData.OXFileType.OXFileData);
        Data.pVersion = FileVersion;
        Data.DataRaw = WankFuckYou(cd, index, cd.Length - index);
        if (!GetFlag(2)) DeObfuscate(Data.DataRaw);
        if (GetFlag(1)) Data.DataRaw = Decompress(Data.DataRaw);
        Data.DataOXFiles = Data.Get_OXFileData(GetFileData());

        return true;
    }
    public void WriteFile(string FileName, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);
        if (CanOverride || !e)
        {
            int oldflags = Flags;
            byte[] wank = Data.BytesOfData(GetFileData(), 0).ToArray();
            if (wank.Length >= 300 && !GetFlag(1))
            {
                SetFlag(1);
                wank = Compress(wank);
            }
            else
            {
                SetFlag(1, false);
            }
            if (!GetFlag(2))
            {
                wank = Obfuscate(wank);
            }
            SetVersionIntoFlag();
            var ver = BitConverter.GetBytes(Flags);
            byte[] final = new byte[ver.Length + wank.Length];
            Buffer.BlockCopy(ver, 0, final, 0, ver.Length);
            Buffer.BlockCopy(wank, 0, final, ver.Length, wank.Length);
            File.WriteAllBytes(FileName, final);
            Flags = oldflags;
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
        FileVersion = Flags & p;
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
        { OXFileType.Long, 8 },
        { OXFileType.Float, 4 },
        { OXFileType.Double, 8 },
        { OXFileType.Vector2, 8 },
        { OXFileType.Vector3, 12 },
        { OXFileType.Quaternion, 16 },
        { OXFileType.Color, 16 },
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
        Vector3,
        Quaternion,
        Color,
    }

    public int LengthOffset;
    public int pVersion = 0;
    // Only meaningful for OXFileType.Sound: true = lossless (delta+gzip), false = lossy (ADPCM). Defaults lossy for max space savings.
    public bool SoundLossless = false;
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

        byte[] data_size = new byte[1];
        byte l = (byte)w.Length;
        if (fd.File.GetFlag(0)) l = fd.File.NameLinker[Name];
        l &= 127;
        if (OXFile.DefinedLengths.ContainsKey(Type))
        {
            data_size = new byte[1] { OXFile.DefinedLengths[Type] };
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
        if (!ExcludeCuzRepeated) AppendAll(new byte[1] { (byte)Type });
        if (!OXFile.DefinedLengths.ContainsKey(Type)) AppendAll(data_size);
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
            Name = Encoding.UTF8.GetString(WankFuckYou(dat, index, length));
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
                DataTexture = Get_Texture();
                break;
            case OXFileType.Sound:
                DataSound = Get_Sound();
                break;
            case OXFileType.Mesh:
                DataMesh = Get_Mesh();
                break;
            case OXFileType.Custom:
                DataCustom = Get_Custom();
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
    public void Add(string Name, Vector2 DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Vector2;
        dat.DataVector2 = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Vector3 DataIn)
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
    public void Add(string Name, Texture2D DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataTexture = DataIn;
        Add(Name, dat);
    }
    public void Add(string Name, Sprite DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.Texture;
        dat.DataSprite = DataIn;
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
    public void Add(string Name, List<OXFileData> DataIn)
    {
        var dat = new OXFileData();
        dat.Type = OXFileData.OXFileType.ListOXFileData;
        dat.DataListOXFiles = DataIn;
        Add(Name, dat);
    }

    public void Add(string Name, List<string> DataIn)
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
        switch (Type)
        {
            case OXFileType.OXFileData:
                var p = DataOXFiles.ToList();
                p.Sort((a, b) => a.Value.Type.CompareTo(b.Value.Type));
                OXFileType c = p[0].Value.Type;
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
                    if (a.Value.Type == c && same <= 253 + repeatmax)
                    {
                        same++;
                    }
                    else
                    {
                        fard();
                        c = a.Value.Type;
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
            case OXFileType.Texture:
                bytez = DataTexture.EncodeToPNG();
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

    public static Dictionary<string, Func<byte[], _IOXFile>> CustomFormats = new();

    private _IOXFile Get_Custom()
    {
        byte length = DataRaw[0];
        byte[] selection = DataRaw.SubArray(1, length);
        string id = Encoding.UTF8.GetString(selection);
        return CustomFormats[id](DataRaw.SubArray(length + 1, DataRaw.Length - length - 1));
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
                stored = cd.Type;
            }
            else if (cd.RepeatRun < 0)
            {
                reps = -cd.RepeatRun;
                reps--;
                stored = cd.Type;
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

    public static byte[] Decompress(byte[] compressedData)
    {
        try
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
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

    private static byte[] XorObfuscate(byte[] data)
    {
        int bLen = SuperSecretKeyBytes.Length;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= (byte)((i + 100) ^ SuperSecretKeyBytes[i % bLen]);
        }
        return data;
    }
    public static byte[] Obfuscate(byte[] data) => XorObfuscate(data);
    public static byte[] DeObfuscate(byte[] data) => XorObfuscate(data);

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
    public static byte[] MeshToBytes(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;
        Color[] colors = mesh.colors;
        Vector4[] tangents = mesh.tangents;

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

            writer.Write(mesh.subMeshCount);
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                int[] indices = mesh.GetTriangles(s);
                writer.Write((byte)mesh.GetTopology(s));
                writer.Write(indices.Length);
                foreach (var idx in indices) writer.Write(idx);
            }
            return stream.ToArray();
        }
    }
    public static Mesh BytesToMesh(byte[] raw, string meshName = "loadedMesh")
    {

        using (MemoryStream stream = new MemoryStream(raw))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            int vertexCount = reader.ReadInt32();
            Vector3[] vertices = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            MeshDataFlags flags = (MeshDataFlags)reader.ReadByte();

            Vector3[] normals = null;
            if ((flags & MeshDataFlags.Normals) != 0)
            {
                normals = new Vector3[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            Vector2[] uv = null;
            if ((flags & MeshDataFlags.UV) != 0)
            {
                uv = new Vector2[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    uv[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }

            Color[] colors = null;
            if ((flags & MeshDataFlags.Colors) != 0)
            {
                colors = new Color[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    colors[i] = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            Vector4[] tangents = null;
            if ((flags & MeshDataFlags.Tangents) != 0)
            {
                tangents = new Vector4[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                    tangents[i] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            int subMeshCount = reader.ReadInt32();

            Mesh mesh = new Mesh();
            mesh.name = meshName;
            // Large meshes (>65535 verts) need a 32-bit index format nya
            mesh.indexFormat = vertexCount > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.vertices = vertices;
            if (normals != null) mesh.normals = normals;
            if (uv != null) mesh.uv = uv;
            if (colors != null) mesh.colors = colors;
            if (tangents != null) mesh.tangents = tangents;

            mesh.subMeshCount = subMeshCount;
            for (int s = 0; s < subMeshCount; s++)
            {
                MeshTopology topology = (MeshTopology)reader.ReadByte();
                int indexCount = reader.ReadInt32();
                int[] indices = new int[indexCount];
                for (int i = 0; i < indexCount; i++) indices[i] = reader.ReadInt32();
                mesh.SetIndices(indices, topology, s);
            }

            if (normals == null) mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }

    public static Texture2D BytesToTexture(byte[] bytes)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        bool success = texture.LoadImage(bytes);

        if (!success)
        {
            Debug.LogError("BytesToTexture: failed to load image data!");
            return null;
        }

        return texture;
    }


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
        // Grab raw float samples from the clip
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        int channels = Mathf.Max(1, clip.channels);

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
            BitConverter.GetBytes(clip.frequency).CopyTo(lossyResult, 5);
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
        byte[] wav = WriteWavHeader(deltaBytes, channels, clip.frequency);
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
    public static AudioClip BytesToAudioClip(byte[] wavBytes, string clipName = "loadedClip")
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

            AudioClip lossyClip = AudioClip.Create(clipName, pcm.Length / channels, channels, frequency, false);
            lossyClip.SetData(lossySamples, 0);
            return lossyClip;
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

        AudioClip clip = AudioClip.Create(clipName, sampleCount / wavChannels, wavChannels, wavFrequency, false);
        clip.SetData(samples, 0);

        return clip;
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
            case OXFileType.Custom: return DataCustom.ToString();
            default: return "Error";
        }
    }
}

public class FileData
{
    public OXFile File;
    public int CurrentStep = 0;
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