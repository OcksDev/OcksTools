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
    private List<byte> data = new List<byte>();
    public Dictionary<string, OXFileData> Data = new Dictionary<string, OXFileData>();
    public string ReadFile(string str)
    {
        //var data= File.ReadAllBytes(str); 
        var data = File.ReadAllBytes(str);
        Data.Clear();
        int index = 0;
        while(index+7 < data.Length)
        {
            var length = BitConverter.ToInt32(data, index);
            var bodylength = BitConverter.ToInt32(data, index+4);
            index+=8;
            if (length == 0) continue;
            var ban = new OXFileData();
            ban.Name = Encoding.ASCII.GetString(WankFuckYou(data, index, length));
            ban.Data = Encoding.ASCII.GetString(WankFuckYou(data, index+length, bodylength));
            Data.Add(ban.Name, ban);
            index += length + bodylength;
        }
        

        this.data = data.ToList();
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
            data.Clear();
            foreach (var a in Data)
            {
                var w = Encoding.ASCII.GetBytes(a.Value.Name);
                var w2 = Encoding.ASCII.GetBytes(a.Value.Data);
                AppendAll(BitConverter.GetBytes(w.Length));
                AppendAll(BitConverter.GetBytes(w2.Length));
                AppendAll(w);
                AppendAll(w2);
            }

            File.WriteAllBytes(FileName, data.ToArray());
        }


        //Environment.NewLine
    }
    public void AppendAll(byte[] arr)
    {
        foreach(var a in arr)
        {
            data.Add(a);
        }
    }
}

public class OXFileData
{
    public string Name = "";
    public string Data = "";
}
