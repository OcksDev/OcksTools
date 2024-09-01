using System;
using System.Collections.Generic;
using UnityEngine;

public class LanguageFileSystem : MonoBehaviour
{
    /* Dependencies:
     *  File System
     *  Random Functions
     */

    public Dictionary<string,string> IndexValuePairs = new Dictionary<string,string>();

    private static LanguageFileSystem instance;
    public static LanguageFileSystem Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (Instance == null) instance = this;
    }

    private void OnApplicationQuit()
    {
        UpdateTextFile();
    }

    // Start is called before the first frame update
    void Start()
    {
        FileGet();
    }

    // Update is called once per frame
    public void UpdateGameFromFile()
    {
        FileGet();
    }

    public void FileGet()
    {
        IndexValuePairs.Clear();
        var f = FileSystem.Instance;
        var file = $"{f.GameDirectory}\\Lang.txt";
        f.WriteFile(file, "", false);
        var e = f.ReadFile(file);
        var dic = Converter.StringToDictionary(e, Environment.NewLine, ":: ");
        if (dic.ContainsKey("Game Version") && dic["Game Version"] != f.GameVer) dic.Clear();
        //where you set the base language file data
        IndexValuePairs.Add("Game Name", "Ocks Tools");

        //vital lang file data, do not remove
        IndexValuePairs.Add("Game Version", $"{f.GameVer}");

        //comparing saved lang file to base data, and fixing changes
        foreach(var d in dic)
        {
            if (IndexValuePairs.ContainsKey(d.Key))
            {
                IndexValuePairs[d.Key] = d.Value;
            }
            else
            {
                IndexValuePairs.Add(d.Key, d.Value);
            }
        }

        UpdateTextFile();
    }
    public void UpdateTextFile()
    {
        FileSystem.Instance.WriteFile($"{FileSystem.Instance.GameDirectory}\\Lang.txt", Converter.DictionaryToString(IndexValuePairs, Environment.NewLine, ":: "), true);
    }
}
