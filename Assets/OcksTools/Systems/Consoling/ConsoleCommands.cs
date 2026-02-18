using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConsoleCommands : MonoBehaviour
{
    public static void Test_tag()
    {
        Tags.AllTags["Exist"].Add("penis", ConsoleLol.Instance.gameObject);

        Console.Log((

            "test result: " + ((GameObject)Tags.AllTags["Exist"]["penis"]).name

        ), "#bdbdbdff");
        Tags.ClearAllOf("penis");
    }
    public static void Test_dict()
    {
        Console.Log("50".StringToObject<TestClass>());
        "---".Log();
        List<string> bananas = new List<string>();
        bananas.Add("1");
        bananas.Add("2");
        bananas.Add("3");
        bananas.Add("4");
        bananas.ListToString().Log();
        var dd = bananas.StringListToAList<TestClass>();
        dd.ListToString().Log();
        "---".Log();
        Dictionary<string, string> d1 = new Dictionary<string, string>()
        {
            {"v1", "aaa"},
            {"v2", "bbb"},
            {"v3", "ccc"},
        };
        Dictionary<string, string> d2 = new Dictionary<string, string>()
        {
            {"v1", "aaa"},
            {"v2", "bbb22"},
            {"v69", "ccc"},
        };
        d1.DiffDictionary(d2).DictionaryToRead().Log();
        "---".Log();
        string s = "abcabcabc123123123123123123whatthefuck{{{{{{{{{5,3}h";
        s.Log();
        s = RandomFunctions.Instance.CollapseSimilarString(s);
        s.Log();
        s = RandomFunctions.Instance.ExpandSimilarString(s);
        s.Log();
        "---".Log();

        var dict = new Dictionary<(int, int), string>()
        {
            {(0,0), "zero"},
            {(1,2), "onetwo"},
            {(3,4), "threefour"},
        };
        dict.ContainsKey((0, 0)).Log();
        dict.ContainsKey((0, 1)).Log();

    }
    public static void Test_cleanstack()
    {
        var a = new TestClass2(OXFunctions.GetCleanStackTrace, null, Vector3.one, null);
        "--------------".Log();
        a.run(OXFunctions.GetCleanStackTrace);
        "----".Log();
        a.run(OXFunctions.GetCleanStackTraceRichtextified);
    }
    private static OXThreadPoolA thread_a;
    public static void Test_threadedlogs()
    {
        if (thread_a == null)
        {
            Test_createthread();
            Console.LogWarning("Thread created, please run again");
            Console.LogWarning("Some logs might not have appeared");
        }
        thread_a.Add("1 Banana".Log);
        thread_a.Add("2 Hello".Log);
        thread_a.Add("3 PP".Log);
        thread_a.Add("4 Fart".Log);
        thread_a.Add("5 Ballz".Log);
        thread_a.Add("6 Smeg".Log);
        thread_a.Add("7 Npoopo".Log);
        thread_a.Add("8 Garzunzulbub".Log);
    }
    public static void Test_createthread()
    {
        thread_a = new OXThreadPoolA(3);
    }
    public static void Test_gevent()
    {
        GlobalEvent.Invoke("TestEvent");
    }
    public static void Test_remap()
    {
        Console.Log(0.5.Remap(0, 1, 0, 10));
        Console.Log(5.0.Remap(0, 10, 0, 1));
    }

    [AddToEvent("TestEvent")]
    public void Test_Event1()
    {
        "Hello".Log();
        "Hello".LogWarning();
        "Hello".LogError();
    }

    [AddToEvent("TestEvent")]
    public void Test_Event2()
    {
        "Hello".DLog();
        "Hello".DLogWarning();
        "Hello".DLogError();
    }



    public static void Test_circle(OXCommandData r)
    {
        var a = SpawnSystem.Spawn(new SpawnData("Circle")
            .Parent("Holder")
            .Data(new Dictionary<string, string>() { { "TestOB", "Circle" } })
            .MultiplayerShare()
            );
        if (r.com[2] == "p")
        {
            Console.Log(SpawnSystem.GetSpawnData(a).ConvertToString());
        }
    }
    public static void Test_read(OXCommandData r)
    {
        Console.Log(r.com_caps[2]);
        Console.Log(Tags.GetFromTag<GameObject>("Exist", r.com_caps[2]).ToString());
    }

    public static void Test_listall()
    {
        foreach (var d in Tags.AllTags["Exist"])
            Console.Log((

                "test result: " + d

            ), "#bdbdbdff");
    }
    public static void Test_compver()
    {
        Console.Log((

                            "comp1: " + RandomFunctions.CompareTwoVersions("v1.0", "v1.1").ToString()

                        ), "#bdbdbdff");
        Console.Log((

            "comp2: " + RandomFunctions.CompareTwoVersions("v1.2", "v1.1").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp2: " + RandomFunctions.CompareTwoVersions("v1.2", "v2.1").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp3: " + RandomFunctions.CompareTwoVersions("v1.2", "v2.1.3").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp4: " + RandomFunctions.CompareTwoVersions("v1.2.0", "2.1.3").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp5: " + RandomFunctions.CompareTwoVersions("1.2.0.7.32.2.1.3", "1.2.0.7.32.2.1.2").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp6: " + RandomFunctions.CompareTwoVersions("1.2.3.4.5.6.7.8", "1.2.a").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp7: " + RandomFunctions.CompareTwoVersions("1.2", "1.2.a").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp8: " + RandomFunctions.CompareTwoVersions("1.2.5", "1.2.a").ToString()

        ), "#bdbdbdff");
        Console.Log((

            "comp9: " + RandomFunctions.CompareTwoVersions("v1.2", "1.2.3.4.5").ToString()

        ), "#bdbdbdff");
    }

    public static void Test_escape()
    {
        string banana = "help(0)eat()cumjjragbanana your_welcum";
        List<string> escape = new List<string>() { "eat", "cum", "rag", "jj" };
        Console.Log((

            "input: " + banana

        ), "#bdbdbdff");
        Console.Log((

            $"remove: {escape[0]}, {escape[1]}, {escape[2]}, {escape[3]}"

        ), "#bdbdbdff");
        banana = Converter.EscapeString(banana, escape);
        Console.Log((

            "escaped: " + banana

        ), "#bdbdbdff");
        banana = Converter.UnescapeString(banana, escape);
        Console.Log((

            "result: " + banana

        ), "#bdbdbdff");
    }
    public static void Test_max()
    {
        Console.Log((

                            "Double Max: " + double.MaxValue.ToString()

                        ), "#bdbdbdff");
    }
    public static void Test_comp(OXCommandData r)
    {
        Console.Log((

            $"{r.com[2]} {r.com[3]}"

        ), "#bdbdbdff");
        Console.Log((

            "Result: " + RandomFunctions.CompareTwoVersions(r.com[2], r.com[3]).ToString()

        ), "#bdbdbdff");
    }
    public static void Test_roman()
    {
        Console.Log((

            "Roman of 129: " + Converter.NumToRead("129", 3)

        ), "#bdbdbdff");
        Console.Log((

            "Roman of 3999: " + Converter.NumToRead("3999", 3)

        ), "#bdbdbdff");
    }
    public static void LogToFile()
    {
        var f = FileSystem.Instance;
        f.CreateFolder($"{f.GameDirectory}\\Logs");
        DateTime currentDateTime = DateTime.Now;
        var x = $"{f.GameDirectory}\\Logs\\{currentDateTime.ToString("MM-dd-yyyy_HH-mm-ss")}.txt";
        f.WriteFile(x, ConsoleLol.Instance.BackLog.Replace("<br>", "\n"), true);
        Console.Log("Saved console to: " + x);
    }
    public static void Test_refs()
    {
        string cum = "";
        int icum = 0;
        foreach (var d in Tags.refs)
        {
            cum += d.Key + ": " + d.Value.name;
            if (icum < Tags.refs.Count - 1) cum += "<br>";
            icum++;
        }
        Console.Log((

            cum

        ), "#bdbdbdff");
    }
    public static void Test_destroy()
    {
        var a = new Dictionary<string, object>(Tags.AllTags["Exist"]);
        foreach (var b in a)
        {
            var penis = (GameObject)b.Value;
            var d = SpawnSystem.GetSpawnData(penis).GetData();

            if (d.ContainsKey("TestOB"))
            {
                SpawnSystem.Kill(penis);
            }
        }
    }
    public static void Test_events()
    {
        var weenor = new OXEvent();
        var banan = new OXEvent<string, string>();
        weenor.Append("test1", ConsoleLol.Instance.TestMethod);
        weenor.Invoke();
        banan.Append("logger", Console.Log);
        banan.Invoke("Hello World", "\"green\"");
        weenor.Remove("test1");
        weenor.Invoke();
        banan.Invoke("Removed test log", "\"yellow\"");
        banan.Append("logger2", Console.Log);
        banan.Append("logger3", Console.Log);
        banan.Invoke("multiple calls?", "\"orange\"");
    }
    public static void joe()
    {
        Console.Log((

            "Who is joe?"

        ), "#bdbdbdff");
    }
    public static void joe_mother()
    {

        Console.Log((

            "AYYYYYEEEEEE"

        ), "#bdbdbdff");
    }
    public static void Clear()
    {
        ConsoleLol.Instance.ClearConsole();
    }

    public static System.Action ScreenshotAction;


    public static void settimescale(OXCommandData r)
    {
        float f = float.Parse(r.com[1]);
        Time.timeScale = f;
        Console.Log((

            LanguageFileSystem.Instance.GetString("Console", "Message_ChangeTime") + f

        ), "#bdbdbdff");
    }

    public static void Data_Edit(OXCommandData r)
    {
        if (r.com[2] != "")
        {
            string eee = r.raw_caps.Substring(r.raw.IndexOf(r.com[2]) + r.com[2].Length + 1);
            SaveSystem.ActiveProf.SetString(r.com_caps[2], eee);
            Console.Log($"Saved \"{eee}\" into {r.com_caps[2]}");
        }
        else
        {
            Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_NoReg"));
        }
    }
    public static void Data_Read(OXCommandData r)
    {
        if (r.com[2] != "")
        {
            Console.Log($"{SaveSystem.ActiveProf.GetString(r.com_caps[2], LanguageFileSystem.Instance.GetString("Console", "Error_NoData"))}");
        }
        else
        {
            Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_NoReg"));
        }
    }
    public static void Data_listall()
    {
        switch (SaveSystem.Instance.SaveMethod_)
        {
            case SaveSystem.SaveMethod.PlayerPrefs:
                Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_NoDataDump"));
                break;
            case SaveSystem.SaveMethod.TXTFile:
                Console.Log($"{Converter.DictionaryToString(SaveSystem.ActiveProf.SavedData, System.Environment.NewLine, ": ")}");
                break;
            case SaveSystem.SaveMethod.OXFile:

                string combined = "";
                foreach (var a in SaveSystem.ActiveProf.GetOX().Data.DataOXFiles)
                {
                    if (combined != "") combined += "<br>";
                    combined += a.Key + ": " + a.Value.ToString();
                }
                Console.Log(combined);
                break;
        }
    }

    public static void Help(OXCommandData r)
    {
        int cmds_per_page = 10;

        int page = 0;
        int maxpg = (ConsoleLol.CommandDict.Count - 1) / cmds_per_page;
        try
        {
            page = int.Parse(r.com[1]) - 1;
            page = System.Math.Clamp(page, 0, maxpg);
        }
        catch
        {

        }

        Console.Log(LanguageFileSystem.Instance.GetString("Console", "Message_Help") + $"({page + 1}/{maxpg + 1})");

        try
        {
            for (int i = 0; i < cmds_per_page; i++)
            {
                Console.Log((

                    "- " + ConsoleLol.CommandDict2.ElementAt(i + (page * cmds_per_page)).Key

                ), "#bdbdbdff");
            }
        }
        catch
        {

        }

    }

    public static void HelpSpecif(OXCommandData r)
    {
        OXCommand m = null;
        try
        {
            m = ConsoleLol.CommandDict[r.com[1]];
        }
        catch
        {
            Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_HelpInspect") + r.com[1]);
            return;
        }

        string laste = "";
        foreach (var a in r.com)
        {
            if (a == "") break;
            laste = a;
        }
        escaped = false;
        ccccc = m;
        RecursiveDesc(m, laste);
        if (!escaped)
        {
            Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_HelpInspect") + laste);
            return;
        }
        if (ccccc == m)
        {
            RecursiveHelp(ccccc, " - " + m.Value);
        }
        else
        {
            RecursiveHelp(ccccc, " - ... " + ccccc.Value);
        }
    }
    private static bool escaped = false;
    public static void RecursiveDesc(OXCommand c, string laste)
    {
        if (escaped) return;
        if (c.Value == laste)
        {
            if (c.ParentLanguage != "")
            {
                Console.Log(LanguageFileSystem.Instance.GetString(c.ParentLanguage, c.LanguageIndex));
            }
            else
            {
                Console.Log(LanguageFileSystem.Instance.GetString("Console", "Message_NoHelp"));
            }
            ccccc = c;
            escaped = true;
            return;
        }

        foreach (var a in c.SubCommands)
        {
            if (a.Value == "-=-") continue;
            RecursiveDesc(a, laste);
            if (escaped) return;
        }
        if (escaped) return;
    }
    private static OXCommand ccccc;
    public static void RecursiveHelp(OXCommand c, string based)
    {
        foreach (var a in c.SubCommands)
        {
            if (a.Value != "-=-")
            {
                RecursiveHelp(a, based + " " + a.Value);
            }
            else
            {
                switch (a.Expected)
                {
                    case OXCommand.ExpectedInputType.Double: RecursiveHelp(a, based + " <#f>"); break;
                    case OXCommand.ExpectedInputType.Long: RecursiveHelp(a, based + " <#>"); break;
                    case OXCommand.ExpectedInputType.String: RecursiveHelp(a, based + " <abc>"); break;
                }
            }
        }
        if (c.Execution != null)
        {
            Console.Log(based);
        }
    }

}


public class TestClass
{
    public int Gamer = 1;
    public override string ToString()
    {
        return $"TestClass[{Gamer}]";
    }

    [ConversionMethod]
    public TestClass ConvertStringToMe(string input)
    {
        var a = new TestClass();
        a.Gamer = int.Parse(input);
        return a;
    }

}


public class TestClass2
{
    public TestClass2()
    {
    }
    public TestClass2(Func<string> inp, Action<int, int> g, Vector3 e, OXEvent<int, int> gg)
    {
        inp().Log();
    }
    public void run(Func<string> inp)
    {
        inp().Log();
    }
}