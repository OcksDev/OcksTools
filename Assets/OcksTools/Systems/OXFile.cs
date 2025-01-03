using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UIElements;

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
    public OXFileData Data = new OXFileData(OXFileData.OXFileType.OXFileData);
    public string ReadFile(string str)
    {
        //var data= File.ReadAllBytes(str); 
        Data = new OXFileData();

        Data.DataRaw = File.ReadAllBytes(str);

        Data.DataOXFiles = Data.Get_OXFileData();
        
        return "";
    }
    public byte[] WankFuckYou(byte[] array, int offset, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }
    public void WriteFile(string FileName, bool CanOverride)
    {
        //string fullpath = //Path.Combine(DirectoryLol, FileName);
        bool e = File.Exists(FileName);

        if (CanOverride || !e)
        {
            File.WriteAllBytes(FileName, Data.ByteSizeOfData().ToArray());
        }


        //Environment.NewLine
    }
}

public class OXFileData
{
    public string Name = "";
    public OXFileType Type;
    public string DataString;
    public float DataFloat;
    public double DataDouble;
    public long DataLong;
    public byte DataByte;
    public int DataInt;
    public Dictionary<string, OXFileData> DataOXFiles = new Dictionary<string, OXFileData>();
    public byte[] DataRaw;
    //public string Data = "";
    public int LengthOffset;
    public OXFileData() { }
    public OXFileData(OXFileType tp)
    {
        Type = tp;
        Name = "Mother";
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
                DataString = Get_String();
                break;
            case OXFileType.OXFileData:
                DataOXFiles = Get_OXFileData();
                break;
            default: break;
        }


        end:
        LengthOffset = index-initiniex;
    }

    public void Add(string Name, string DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataString = DataIn;
        dat.DataRaw = Encoding.UTF8.GetBytes(DataIn);
        dat.Type = OXFileData.OXFileType.String;
        DataOXFiles.Add(Name, dat);
    }
    public void Add(string Name, int DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataInt = DataIn;
        dat.DataRaw = BitConverter.GetBytes(DataIn);
        dat.Type = OXFileData.OXFileType.Int;
        DataOXFiles.Add(Name, dat);
    }
    public void Add(string Name, float DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataFloat = DataIn;
        dat.DataRaw = BitConverter.GetBytes(DataIn);
        dat.Type = OXFileData.OXFileType.Float;
        DataOXFiles.Add(Name, dat);
    }
    public void Add(string Name, double DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataDouble = DataIn;
        dat.DataRaw = BitConverter.GetBytes(DataIn);
        dat.Type = OXFileData.OXFileType.Double;
        DataOXFiles.Add(Name, dat);
    }
    public void Add(string Name, long DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataLong = DataIn;
        dat.DataRaw = BitConverter.GetBytes(DataIn);
        dat.Type = OXFileData.OXFileType.Long;
        DataOXFiles.Add(Name, dat);
    }
    public void Add(string Name, OXFileData DataIn)
    {
        var dat = new OXFileData();
        dat.Name = Name;
        dat.DataRaw = DataIn.ByteSizeOfData().ToArray();
        dat.Type = OXFileData.OXFileType.OXFileData;
        DataOXFiles.Add(Name, dat);
    }


    public List<byte> ToByte()
    {
        List<byte> ret = new List<byte>();
        var w = Encoding.UTF8.GetBytes(Name);
        var w2 = DataRaw;
        Func<byte[], int> AppendAll = (x) =>
        {
            foreach(var a in x)
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
                foreach(var a in DataOXFiles)
                {
                    if(a.Value.DataRaw != null && a.Value.DataRaw.Length > 0)
                    {
                        bytes = a.Value.ToByte();
                    }
                    else
                    {
                        a.Value.DataRaw = a.Value.ByteSizeOfData().ToArray();
                        bytes = a.Value.ToByte();
                    }
                    foreach(var b in bytes)
                    {
                        ret.Add(b);
                    }
                }
                break;
            case OXFileType.String:
                FileSystem.WEE("A: " + DataString);
                bytez = Encoding.UTF8.GetBytes(DataString);
                foreach (var b in bytez)
                {
                    ret.Add(b);
                }
                break;
        }
        return ret;
    }

    public string Get_String()
    {
        return Encoding.UTF8.GetString(DataRaw);
    }
    public int Get_Int()
    {
        return BitConverter.ToInt32(DataRaw,0);
    }
    public long Get_Long()
    {
        return BitConverter.ToInt64(DataRaw,0);
    }
    public float Get_Float()
    {
        return BitConverter.ToSingle(DataRaw,0);
    }
    public double Get_Double()
    {
        return BitConverter.ToDouble(DataRaw,0);
    }
    public Dictionary<string, OXFileData> Get_OXFileData()
    {
        var ret = new Dictionary<string, OXFileData>();

        int index = 0;
        while (index + 7 < DataRaw.Length)
        {
            var cd = new OXFileData(DataRaw, index);
            ret.Add(cd.Name, cd);
            index += cd.LengthOffset;
        }

        return ret;
    }


    public byte[] WankFuckYou(byte[] array, int offset, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }
    public enum OXFileType
    {
        String,
        OXFileData,
        Int,
        Float,
        Long,
        Double,
    }
}

public class FileRetData
{
    public int Length;
    public OXFileData me;
}