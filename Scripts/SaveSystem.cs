using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;
    public bool UseFileSystem = true;
    public int SaveFile = 0;
    //idk how needed this is tbh
    private string UniqueGamePrefix = "oxt";
    public int test = 0;

    public static SaveSystem Instance
    {
        get { return instance; }
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private void Awake()
    {
        if (Instance == null) instance = this;
    }
    private void Start()
    {
        LoadGame();
    }

    public void ChangeFile(int i  = 0)
    {
        SaveFile = i;
        LoadGame(i);
    }
    private int filenum = -1;
    public void LoadGame(int i  = -1)
    {
        /* Input Modes:
         * -1 = Load whatever was the last used file (if being used for the first time it defaults to 0)
         * Any Other Value = Load the data of a specific file
         */



        InputManager.AssembleTheCodes();
        List<string> list = new List<string>();



        if (i == -1)
        {
            var xx = PlayerPrefs.GetInt("SaveFile", -1);
            if (xx != -1)
            {
                SaveFile = xx;
            }
            i = SaveFile;
        }
        filenum = i;
        var s = SoundSystem.Instance;

        int x = 0;




        var ghghgg = PlayerPrefs.GetString("keybinds", "fuck");
        list = StringToList(ghghgg);
        if (ghghgg != "fuck")
        {
            foreach (var a in list)
            {
                try
                {
                    var sseexx = StringToList(a, "<K>");
                    InputManager.gamekeys[sseexx[0]] = InputManager.namekeys[sseexx[1]];
                    x++;
                }
                catch
                {
                }
            }
        }
        x = 0;

        if(s != null)
        {
            s.MasterVolume = PlayerPrefs.GetFloat("snd_mas", 1);
            s.SFXVolume = PlayerPrefs.GetFloat("snd_sfx", 1);
            s.MusicVolume = PlayerPrefs.GetFloat("snd_mus", 1);
        }

        test = int.Parse(LoadString("test_num", "0"));
        //ConsoleLol.Instance.ConsoleLog(Prefix(i) + "test_num");


        SaveFile = i;
    }
    public void SaveGame(int i = -1)
    {
        /* Input Modes:
         * -1 = Save whatever is the currently selected file (by default is 0)
         * Any Other Value = Save curent data to a specfic file
         */


        List<string> list = new List<string>();
        if (i == -1)
        {
            PlayerPrefs.SetInt("SaveFile", SaveFile);
            i = SaveFile;
        }
        filenum = i;
        var s = SoundSystem.Instance;

        list.Clear();
        foreach (var a in InputManager.gamekeys)
        {
            list.Add(a.Key + "<K>" + InputManager.keynames[a.Value]);
        }
        PlayerPrefs.SetString("keybinds", ListToString(list));
        //PlayerPrefs.SetInt("UnitySelectMonitor", index); // sets the monitor that unity uses

        if (s != null)
        {
            PlayerPrefs.SetFloat("snd_mas", s.MasterVolume);
            PlayerPrefs.SetFloat("snd_sfx", s.SFXVolume);
            PlayerPrefs.SetFloat("snd_mus", s.MusicVolume);
        }

        SaveString("test_num", test.ToString());
    }
    public string GameDataFileName = "Game_Data";
    public void SaveString(string key, string data, string path = "")
    {
        if (UseFileSystem)
        {
            var f = FileSystem.Instance;
            if (path != "")
            {
                GameFilePath = path;
            }
            else
            {
                GameFilePath = $"{f.GameDirectory}\\{GameDataFileName}_{filenum}.txt";
            }
            if (!File.Exists(GameFilePath)) f.WriteFile(GameFilePath, "", false);
            var s = StringToDictionary(f.ReadFile(GameFilePath), Environment.NewLine, ": ");
            if (s.ContainsKey(key))
            {
                s[key] = data;
            }
            else
            {
                s.Add(key, data);
            }
            f.WriteFile(GameFilePath, DictionaryToString(s, Environment.NewLine, ": "), true);
        }
        else
        {
            PlayerPrefs.SetString(key, data);
        }
    }

    public string GameFilePath = "";

    public string LoadString(string key, string defaul = "", string path = "")
    {
        if (UseFileSystem)
        {
            var f = FileSystem.Instance;
            if(path != "")
            {
                GameFilePath = path;
            }
            else
            {
                GameFilePath = $"{f.GameDirectory}\\{GameDataFileName}_{filenum}.txt";
            }
            if (File.Exists(GameFilePath))
            {
                var s = StringToDictionary(f.ReadFile(GameFilePath), Environment.NewLine, ": ");
                if (s.ContainsKey(key))
                {
                    return s[key];
                }
                else
                {
                    return defaul;
                }
            }
            else
            {
                return defaul;
            }
        }
        else
        {
            return PlayerPrefs.GetString(key, defaul);
        }
    }


    public string Prefix(int file)
    {
        if (file == -1) file = SaveFile;
        return UniqueGamePrefix + "#" + file + "_";
    }


    public int BoolToInt(bool a)
    {
        return a ? 1 : 0;
    }
    public bool IntToBool(int a)
    {
        return a == 1;
    }

    public string ListToString(List<string> eee, string split = ", ")
    {
        return String.Join(split, eee);
    }

    public List<string> StringToList(string eee, string split = ", ")
    {
        return eee.Split(split).ToList();
    }

    public string DictionaryToString(Dictionary<string, string> dic, string splitter = ", ", string splitter2 = "<K>")
    {
        List<string> list = new List<string>();
        foreach (var a in dic)
        {
            list.Add(a.Key + splitter2 + a.Value);
        }
        return ListToString(list, splitter);
    }
    public Dictionary<string, string> StringToDictionary(string e, string splitter = ", ", string splitter2 = "<K>")
    {
        var dic = new Dictionary<string, string>();
        var list = StringToList(e, splitter);
        foreach (var a in list)
        {
            try
            {
                int i = a.IndexOf(splitter2);
                List<string> sseexx = new List<string>()
                {
                    a.Substring(0, i),
                    a.Substring(i + splitter2.Length),
                };
                if (dic.ContainsKey(sseexx[0]))
                {
                    dic[sseexx[0]] = dic[sseexx[1]];
                }
                else
                {
                    dic.Add(sseexx[0], sseexx[1]);
                }
            }
            catch
            {
            }
        }
        return dic;
    }

}
