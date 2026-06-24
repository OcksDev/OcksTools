using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WebSocketSharp;
using static OXFileData;

/*
 * What is this?
 * 
 * This is an experimental file format/parser I have been working on.
 * Currently this is not the same thing as the FileSystem, as that is meant for more general file interaction.
 * This specifically is only designed to read/write/parse any .ox file.
 * 
 */




public class OXFile
{
    //touch away
    public const short ParserVersion = 1;
    public static bool DoObsure = false;
    //no touchy
    public int FileVersion = 1;
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
        var ccd = WankFuckYou(cd, index, cd.Length - index);
        SmallObscure(ccd, 6969420);
        Data.DataRaw = ccd;
        Data.DataOXFiles = Data.Get_OXFileData(GetFileData());

        return true;
    }
    public void WriteFile(string FileName, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);

        if (CanOverride || !e)
        {
            var wank = Data.ByteSizeOfData(GetFileData(), 0);
            SmallObscure(wank, 6969420);
            List<byte> bytes = new List<byte>();
            SetVersionIntoFlag();
            var ver = BitConverter.GetBytes(Flags);
            foreach (byte b in ver) { bytes.Add(b); }

            wank = bytes.CombineLists(wank);
            File.WriteAllBytes(FileName, wank.ToArray());
        }

    }
    public void SmallObscure(List<byte> thing, int seed)
    {
        if (!DoObsure) return;
        var rand = new System.Random(seed + thing.Count);
        for (int i = 0; i < thing.Count; i++)
        {
            var w = ((byte)rand.Next(0, 256));
            w ^= 85;
            thing[i] ^= w;
        }
    }
    public void SmallObscure(byte[] thing, int seed)
    {
        if (!DoObsure) return;
        var rand = new System.Random(seed + thing.Length);
        for (int i = 0; i < thing.Length; i++)
        {
            var w = ((byte)rand.Next(0, 256));
            w ^= 85;
            thing[i] ^= w;
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
    /*
     * Flags:
     * 0 -> Linker
     */
    public static readonly Dictionary<OXFileType, byte> DefinedLengths = new()
    {
        { OXFileType.Bool, 1 },
        { OXFileType.Int, 4 },
        { OXFileType.Long, 8 },
        { OXFileType.Float, 4 },
        { OXFileType.Double, 8 },
    };
}

public class OXFileData
{
    public string Name = "";
    public OXFileType Type;
    public string DataString;
    public List<string> DataListString;
    public Dictionary<string, string> DataDictStringString;
    public float DataFloat;
    public double DataDouble;
    public long DataLong;
    public byte DataByte;
    public int DataInt;
    public bool DataBool;
    public _IOXFile DataCustom;
    public Dictionary<string, OXFileData> DataOXFiles = new Dictionary<string, OXFileData>();
    public List<OXFileData> DataListOXFiles = new List<OXFileData>();
    public byte[] DataRaw;
    //public string Data = "";
    public int LengthOffset;
    public double pVersion = 0;
    public OXFileData() { }
    public OXFileData(OXFileType tp)
    {
        Type = tp;
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
        List<byte> ret = new List<byte>();
        var w = Encoding.UTF8.GetBytes(Name);
        var w2 = DataRaw;
        Func<byte[], int> AppendAll = (x) =>
        {
            foreach (var a in x)
            {
                ret.Add(a);
            }
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
            case OXFileType.OXFileData:
                DataOXFiles = Get_OXFileData(fd);
                break;
            case OXFileType.ListString:
                DataListString = Get_ListString();
                break;
            case OXFileType.DictStringString:
                DataDictStringString = Get_DictStringString();
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
        dat.Type = OXFileData.OXFileType.OXFileData;
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
                DataListOXFiles.Add(dat);
                break;
            default:
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
    public List<byte> ByteSizeOfData(FileData fd, int current_step)
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
                        a.Value.DataRaw = a.Value.ByteSizeOfData(fd, fd.CurrentStep).ToArray();
                        bytes = a.Value.ToByte(fd);
                    }
                    foreach (var b in bytes)
                    {
                        ret.Add(b);
                    }
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
                        a.DataRaw = a.ByteSizeOfData(fd, fd.CurrentStep).ToArray();
                        bytes = a.ToByte(fd);
                    }
                    foreach (var b in bytes)
                    {
                        ret.Add(b);
                    }
                }
                break;
            case OXFileType.String:
                bytez = Encoding.UTF8.GetBytes(DataString);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Int:
                bytez = BitConverter.GetBytes(DataInt);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Long:
                bytez = BitConverter.GetBytes(DataLong);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Float:
                bytez = BitConverter.GetBytes(DataFloat);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Double:
                bytez = BitConverter.GetBytes(DataDouble);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Custom:
                var bytez2 = DataCustom.GetBytes();
                foreach (var b in bytez2)
                {
                    ret.Add(b);
                }
                break;
            case OXFileType.Raw: //I dont think this will ever be called
                return DataRaw.ToList();
            case OXFileType.ListString:
                foreach (var li in DataListString)
                {
                    var ccc = Encoding.UTF8.GetBytes(li);
                    bytez = BitConverter.GetBytes(ccc.Length);
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
                    bytez = ccc;
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
                }
                break;
            case OXFileType.DictStringString:
                foreach (var li in DataDictStringString)
                {
                    var ccc = Encoding.UTF8.GetBytes(li.Key);
                    var ccc2 = Encoding.UTF8.GetBytes(li.Value);
                    bytez = BitConverter.GetBytes(ccc.Length);
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
                    bytez = BitConverter.GetBytes(ccc2.Length);
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
                    bytez = ccc;
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
                    bytez = ccc2;
                    foreach (var b in bytez)
                    {
                        ret.Add(b);
                    }
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
        r3, r4, r5, r6, r7, r8, r9, r10
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