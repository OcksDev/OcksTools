using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
    public const double ParserVersion = 1;
    public static bool DoObsure = true;
    //no touchy
    public double FileVersion = 0;
    public OXFileData Data = new OXFileData(OXFileData.OXFileType.OXFileData);
    public bool ReadFile(string str)
    {
        var oxconfirm = Encoding.UTF8.GetBytes("OXFile");


        var cd = File.ReadAllBytes(str);
        if (cd.Length < oxconfirm.Length) return false;
        for (int i = 0; i < oxconfirm.Length; i++)
        {
            if (cd[i] != oxconfirm[i]) return false;
        }
        int index = oxconfirm.Length;
        FileVersion = BitConverter.ToDouble(cd, index);
        index += 8;
        Data = new OXFileData(OXFileData.OXFileType.OXFileData);
        Data.pVersion = FileVersion;
        var ccd = WankFuckYou(cd, index, cd.Length - index);
        SmallObscure(ccd, 6969420);
        Data.DataRaw = ccd;

        Data.DataOXFiles = Data.Get_OXFileData();

        return true;
    }
    public void WriteFile(string FileName, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);

        if (CanOverride || !e)
        {
            var wank = Data.ByteSizeOfData();
            SmallObscure(wank, 6969420);
            List<byte> bytes = new List<byte>();
            var oxconfirm = Encoding.UTF8.GetBytes("OXFile");
            foreach (byte b in oxconfirm) { bytes.Add(b); }
            var ver = BitConverter.GetBytes(ParserVersion);
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



    public OXFileData(byte[] dat, int index)
    {
        int initiniex = index;
        var length = BitConverter.ToInt32(dat, index);
        var bodylength = BitConverter.ToInt32(dat, index + 4);
        if (length == 0) goto end;
        index += 8;
        Name = Encoding.UTF8.GetString(WankFuckYou(dat, index, length));
        index += length;
        Type = (OXFileType)BitConverter.ToInt32(dat, index);
        index += 4;
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
                DataOXFiles = Get_OXFileData();
                break;
            case OXFileType.ListString:
                DataListString = Get_ListString();
                break;
            case OXFileType.DictStringString:
                DataDictStringString = Get_DictStringString();
                break;
            case OXFileType.ListOXFileData:
                DataListOXFiles = Get_ListOXFileData();
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

    public List<byte> ToByte()
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
        AppendAll(BitConverter.GetBytes(w.Length));
        AppendAll(BitConverter.GetBytes(w2.Length));
        AppendAll(w);
        AppendAll(BitConverter.GetBytes((int)Type));
        AppendAll(w2);
        return ret;
    }
    public List<byte> ByteSizeOfData()
    {
        List<byte> ret = new List<byte>();
        List<byte> bytes = new List<byte>();
        byte[] bytez;
        switch (Type)
        {
            case OXFileType.OXFileData:
                foreach (var a in DataOXFiles)
                {
                    if (a.Value.DataRaw != null && a.Value.DataRaw.Length > 0)
                    {
                        bytes = a.Value.ToByte();
                    }
                    else
                    {
                        a.Value.DataRaw = a.Value.ByteSizeOfData().ToArray();
                        bytes = a.Value.ToByte();
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
                        bytes = a.ToByte();
                    }
                    else
                    {
                        a.DataRaw = a.ByteSizeOfData().ToArray();
                        bytes = a.ToByte();
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
    private bool Get_Bool()
    {
        return DataRaw[0] == (byte)69;
    }
    public Dictionary<string, OXFileData> Get_OXFileData()
    {
        var ret = new Dictionary<string, OXFileData>();

        int index = 0;
        while (index + 7 < DataRaw.Length)
        {
            var cd = new OXFileData(DataRaw, index);
            cd.pVersion = pVersion;
            ret.Add(cd.Name, cd);
            index += cd.LengthOffset;
        }

        return ret;
    }
    public List<OXFileData> Get_ListOXFileData()
    {
        var ret = new List<OXFileData>();

        int index = 0;
        while (index + 7 < DataRaw.Length)
        {
            var cd = new OXFileData(DataRaw, index);
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
            default: return "Error";
        }
    }
}

public class FileRetData
{
    public int Length;
    public OXFileData me;
}