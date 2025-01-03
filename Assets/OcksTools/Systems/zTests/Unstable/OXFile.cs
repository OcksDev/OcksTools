using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class OXFile
{
    private static List<byte> databytes = new List<byte>();
    private static byte[] data;
    public static Dictionary<string, OXFileData> Data = new Dictionary<string, OXFileData>();
    public string ReadFile(string str)
    {
        //var data= File.ReadAllBytes(str); 
        data = File.ReadAllBytes(str);
        Data.Clear();
        int index = 0;
        while(index+7 < data.Length)
        {
            var cd = GetOXData(index);
            index += cd.Length;
        }
        

        databytes = data.ToList();
        return "";
    }

    public FileRetData GetOXData(int index)
    {
        int lengthret = 0;
        OXFileData ban = null;

        var length = BitConverter.ToInt32(data, index);
        var bodylength = BitConverter.ToInt32(data, index + 4);
        lengthret += 8;
        index += 8;
        if (length == 0) goto end;
        ban = new OXFileData(WankFuckYou(data, index, length), WankFuckYou(data, index + length, bodylength));
        Data.Add(ban.Name, ban);
        lengthret += length + bodylength;
        index += length + bodylength;


end:
        var ret = new FileRetData();
        ret.me = ban;
        ret.Length = lengthret;
        return ret;
    }

    public void AppendData_String()
    {

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
            databytes.Clear();
            foreach (var a in Data)
            {
                AppendAll(a.Value.ToByte());
            }

            File.WriteAllBytes(FileName, databytes.ToArray());
        }


        //Environment.NewLine
    }

    public void AppendAll(List<byte> arr)
    {
        foreach(var a in arr)
        {
            databytes.Add(a);
        }
    }
}

public class OXFileData
{
    public string Name = "";
    public OXFileType Type;
    public byte[] DataRaw;
    //public string Data = "";

    public OXFileData() { }
    public OXFileData(byte[] Name2, byte[] Data2) 
    {
        Name = Encoding.UTF8.GetString(Name2);
        DataRaw = Data2;
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
        AppendAll(w2);
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
    }
}

public class FileRetData
{
    public int Length;
    public OXFileData me;
}