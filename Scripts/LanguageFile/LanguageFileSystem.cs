using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class LanguageFileSystem : MonoBehaviour
{
    /* Dependencies:
     *  File System
     *  Random Functions
     */

    public Dictionary<string,string> IndexValuePairs = new Dictionary<string,string>();

    public static LanguageFileSystem instance;
    public static LanguageFileSystem Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (Instance == null) instance = this;
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
        var dic = RandomFunctions.Instance.StringToDictionary(e, Environment.NewLine, ":: ");
        if (dic.ContainsKey("Game Version") && dic["Game Version"] != RandomFunctions.Instance.GameVer) dic.Clear();
        //where you set the base language file data
        IndexValuePairs.Add("Game Name", "Ocks Tools");

        //vital lang file data, do not remove
        IndexValuePairs.Add("Game Version", $"{RandomFunctions.Instance.GameVer}");

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
        FileSystem.Instance.WriteFile($"{FileSystem.Instance.GameDirectory}\\Lang.txt", RandomFunctions.Instance.DictionaryToString(IndexValuePairs, Environment.NewLine, ":: "), true);
    }
}
