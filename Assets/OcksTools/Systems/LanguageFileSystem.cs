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

    public List<OXLanguageFileIndex> Files = new List<OXLanguageFileIndex>();
    Dictionary<string, Dictionary<string, string>> Data = new Dictionary<string, Dictionary<string, string>>();

    private static LanguageFileSystem instance;
    public static LanguageFileSystem Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Lang"]);
        FileSystem.Instance.CreateFolder($"{FileSystem.Instance.FileLocations["Lang"]}\\{FileSystem.GameVer}");
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
        if (namespac == "unknown")
        {
            foreach(var f in Data)
            {
                if(f.Value.ContainsKey(key2)) return f.Value[key2];
            }
        }
        return Data[namespac][key2];
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
        if (!File.Exists(realme))
        {
            FileSystem.Instance.WriteFile(realme, file.DefaultFile.text, true);
            return;
        }
        var des = GetDict(file.FileName);
        des.Clear();
        var s = Converter.StringToList(f.ReadFile(realme), Environment.NewLine);
        foreach (var d in s)
        {
            if (d.IndexOf(": ") > -1)
            {
                des.Add(d.Substring(0, d.IndexOf(": ")), d.Substring(d.IndexOf(": ") + 2));
            }
        }
        s = Converter.StringToList(file.DefaultFile.text, Environment.NewLine);
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
}