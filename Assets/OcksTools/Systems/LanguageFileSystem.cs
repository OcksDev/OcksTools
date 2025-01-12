using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static SaveSystem;

public class LanguageFileSystem : MonoBehaviour
{
    /* Dependencies:
     *  File System
     *  Random Functions
     */
    public bool EditorAuthorityOnFile = true;
    public bool AllowPublicAccess = true;
    public List<OXLanguageFileIndex> Files = new List<OXLanguageFileIndex>();
    Dictionary<string, Dictionary<string, string>> Data = new Dictionary<string, Dictionary<string, string>>();

    private static LanguageFileSystem instance;
    public static LanguageFileSystem Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (AllowPublicAccess)
        {
            FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Lang"]);
            FileSystem.Instance.CreateFolder($"{FileSystem.Instance.FileLocations["Lang"]}\\{FileSystem.GameVer}");
        }
        foreach(var file in Files)
        {
            ReadFile(file);
        }
        if (Instance == null) instance = this;
    }
    public void AddFile(OXLanguageFileIndex oxl)
    {
        foreach(var f in Files)
        {
            if (f.FileName == oxl.FileName) return;
        }
        Files.Add(oxl);
        ReadFile(oxl);
    }

    private void OnApplicationQuit()
    {
        WriteAllFiles();
    }
    public void WriteAllFiles()
    {
        if (!AllowPublicAccess) return;
        var f = FileSystem.Instance;
        string meme = $"{f.FileLocations["Lang"]}\\{FileSystem.GameVer}";
        foreach (var file in Files)
        {
            var des = GetDict(file.FileName);
            string realme = $"{meme}\\{file.FileName}.txt";
            f.WriteFile(realme, Converter.DictionaryToString(des, Environment.NewLine, ": "), true);
        }
    }

    public string GetString(string namespac, string key2)
    {
        if (namespac == "unknown" || namespac == "")
        {
            foreach(var f in Data)
            {
                if(f.Value.ContainsKey(key2)) return f.Value[key2];
            }
        }
        return Data[namespac][key2];
    }
    public void SetString(string namespac, string key2, string str)
    {
        if(!Data.ContainsKey(namespac)) return;
        if (!Data[namespac].ContainsKey(key2))
        {
            Data[namespac].Add(key2, str);
        }
        else
        {
            Data[namespac][key2] = str;
        }

    }

    public Dictionary<string, string> GetDict(string name)
    {
        if (Data.ContainsKey(name))
        {
            return Data[name];
        }
        else
        {
            Data.Add(name, new Dictionary<string, string>());
            return Data[name];
        }
    }
    public void ReadFile(OXLanguageFileIndex file)
    {
        var f = FileSystem.Instance;
        string meme = $"{f.FileLocations["Lang"]}\\{FileSystem.GameVer}";
        string realme = $"{meme}\\{file.FileName}.txt";
        var des = GetDict(file.FileName);
        des.Clear();

        if (EditorAuthorityOnFile || !AllowPublicAccess)
        {
            if(AllowPublicAccess)FileSystem.Instance.WriteFile(realme, file.GetDefaultData(), true);
            var ss = Converter.StringToList(file.GetDefaultData(), Environment.NewLine);
            foreach (var d in ss)
            {
                if (d.IndexOf(": ") > -1 && !des.ContainsKey(d.Substring(0, d.IndexOf(": "))))
                {
                    des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
                }
            }
            return;
        }

        if (!File.Exists(realme))
        {
            if (AllowPublicAccess) FileSystem.Instance.WriteFile(realme, file.GetDefaultData(), true);
            goto d9;
        }
        var s = Converter.StringToList(f.ReadFile(realme), Environment.NewLine);
        Debug.Log("Penis: " + s.Count);
        foreach (var d in s)
        {
            if (d.IndexOf(": ") > -1)
            {
                des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
            }
        }
        d9:
        s = Converter.StringToList(file.GetDefaultData(), Environment.NewLine);
        foreach (var d in s)
        {
            if (d.IndexOf(": ") > -1 && !des.ContainsKey(d.Substring(0, d.IndexOf(": "))))
            {
                des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
            }
        }
    }
}

[System.Serializable]
public class OXLanguageFileIndex
{
    public string FileName;
    public TextAsset DefaultFile;
    public string DefaultString = "";
    public string GetDefaultData()
    {
        if(DefaultFile != null)
        {
            return DefaultFile.text;
        }
        else
        {
            return DefaultString;
        }
    }
}