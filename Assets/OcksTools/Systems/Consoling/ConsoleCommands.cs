using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ConsoleCommands : MonoBehaviour
{
    public static void Test_tag()
    {
        Tags.AllTags["Exist"].Add("penis", ConsoleLol.Instance.gameObject);

        Console.Log((

            "test result: " + Tags.AllTags["Exist"]["penis"].name

        ), "#bdbdbdff");
        Tags.ClearAllOf("penis");
    }
    public static void Test_circle()
    {
        SpawnSystem.Instance.SpawnObject("Circle", ConsoleLol.Instance.gameObject, Vector3.zero, Quaternion.Euler(0, 0, 0));
    }
    public static void Test_chat()
    {
        for (int i = 0; i < 10; i++)
        {
            ChatLol.Instance.WriteChat("Chat Test Lol", "#" + UnityEngine.Random.ColorHSV().ToHexString());
        }
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
        foreach (var d in Tags.AllTags["Exist"])
            Destroy(d.Value);
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
    public static void Dialog(OXCommandData r)
    {
        switch (r.com[1])
        {
            case "stop":
                DialogLol.Instance.ResetDialog();
                Console.Log((

                    LanguageFileSystem.Instance.GetString("Console", "Message_StoppedDialog")

                ), "#bdbdbdff");
                break;
            default:
                if (DialogLol.Instance.LanguageFileIndexes.ContainsKey(r.com_caps[1]))
                {
                    DialogLol.Instance.StartDialog(r.com_caps[1]);
                    ConsoleLol.Instance.CloseConsole();
                }
                else
                {
                    Console.Log((

                        LanguageFileSystem.Instance.GetString("Console", "Error_NoDialogWithName") + $"\"{r.com_caps[1]}\""

                    ), "#ff0000ff");
                }
                break;
        }
    }
    public static void Clear()
    {
        ConsoleLol.Instance.ClearConsole();
    }
    public static void ScreenShot()
    {
        if (Screenshot.Instance != null)
        {
            ConsoleLol.Instance.ClearConsole();
            var ss = new ScreenshotData("test", 1000, 1000, Camera.main, true);
            Screenshot.Instance.TakeScreenshot(ss);
        }
        else
        {
            Console.Log((

                LanguageFileSystem.Instance.GetString("Console", "Error_NoScreenshot")

            ), "#bdbdbdff");
        }
    }

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
            SaveSystem.Instance.SetString(r.com_caps[2], eee);
            ConsoleLol.Instance.ConsoleLog($"Saved \"{eee}\" into {r.com_caps[2]}");
        }
        else
        {
            ConsoleLol.Instance.ConsoleLog(LanguageFileSystem.Instance.GetString("Console", "Error_NoReg"), "#ff0000ff");
        }
    }
    public static void Data_Read(OXCommandData r)
    {
        if (r.com[2] != "")
        {
            ConsoleLol.Instance.ConsoleLog($"{SaveSystem.Instance.GetString(r.com_caps[2], LanguageFileSystem.Instance.GetString("Console", "Error_NoData"))}");
        }
        else
        {
            ConsoleLol.Instance.ConsoleLog(LanguageFileSystem.Instance.GetString("Console", "Error_NoReg"), "#ff0000ff");
        }
    }
    public static void Data_listall()
    {
        switch (SaveSystem.Instance.SaveMethod_)
        {
            case SaveSystem.SaveMethod.PlayerPrefs:
                ConsoleLol.Instance.ConsoleLog(LanguageFileSystem.Instance.GetString("Console", "Error_NoDataDump"), "#ff0000ff");
                break;
            case SaveSystem.SaveMethod.TXTFile:
                ConsoleLol.Instance.ConsoleLog($"{Converter.DictionaryToString(SaveSystem.Instance.GetDict(), System.Environment.NewLine, ": ")}");
                break;
            case SaveSystem.SaveMethod.OXFile:

                string combined = "";
                foreach (var a in SaveSystem.Instance.GetDictOX().Data.DataOXFiles)
                {
                    if (combined != "") combined += "<br>";
                    combined += a.Key + ": " + a.Value.ToString();
                }
                ConsoleLol.Instance.ConsoleLog(combined);
                break;
        }
    }

    public static void Help(OXCommandData r)
    {
        int cmds_per_page = 10;

        int page = 0;
        int maxpg = ConsoleLol.CommandDict.Count / cmds_per_page;
        try
        {
            page = int.Parse(r.com[1])-1;
            page= System.Math.Clamp(page, 0, maxpg);
        }
        catch
        {

        }

        Console.Log(LanguageFileSystem.Instance.GetString("Console", "Message_Help") + $"({page+1}/{maxpg+1})");

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
        foreach(var a in r.com)
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
        if(ccccc == m)
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
