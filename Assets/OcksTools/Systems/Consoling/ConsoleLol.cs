using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleLol : SingleInstance<ConsoleLol>
{
    public GameObject ConsoleObject;
    public ConsolRefs ConsoleObjectRef;
    public OXLanguageFileIndex LanguageFileIndex;
    public bool UseLanguageFileSystem = true;
    public bool SavePrevCommands = true;
    [HideInInspector]
    public bool enable = false;
    private List<string> prev_commands = new List<string>();
    private string s = "";
    private List<string> command = new List<string>();
    private List<string> command_caps = new List<string>();
    public string BackLog = "";
    private int comm = 0;


    /* the setup process!
     * 
     * 1. Use the ockstools window found at the top of the unity editor in the path OcksTools/Console/Utils
     * 2. Place the console object parent (canvas) in the given field and click the setup button
     * 3. You are done
     */



    /*
     * To create a hook:
     * Link up a method to be ran from the ConsoleHook event
     * Set the bool RecogHandover to true if the method detected a recognised a command, regardless of success state
     */

    public static OXEvent<List<string>, List<string>> ConsoleHook = new OXEvent<List<string>, List<string>>();
    public static Dictionary<string, OXCommand> CommandDict = new Dictionary<string, OXCommand>();
    public static IOrderedEnumerable<KeyValuePair<string, OXCommand>> CommandDict2;

    public static Thread _mainthread;

    public override void Awake2()
    {
        _mainthread = Thread.CurrentThread;
        prev_commands.Clear();
        BackLog = "";
        ConsoleObjectRef = ConsoleObject.GetComponent<ConsolRefs>();
        ConsoleChange(false);


        InputManager.CollectInputAllocs.Append("Console", () =>
        {
            InputManager.CreateInputSet("console", KeyCode.Slash);
            InputManager.CreateInputSet("console_up", KeyCode.UpArrow);
            InputManager.CreateInputSet("console_down", KeyCode.DownArrow);
            InputManager.CreateInputSet("console_autofill", KeyCode.Tab);
        });
    }

    public void Start()
    {

        var l = LanguageFileSystem.Instance;
        if (l != null && UseLanguageFileSystem)
        {
            l.AddFile(LanguageFileIndex);
        }

        RunThroughConsoleBacklog();

        StartCoroutine(AssembleHelpMenu());

        LoadConsole(null);


    }
    private void FixedUpdate()
    {
        RunThroughConsoleBacklog();
    }
    private void RunThroughConsoleBacklog()
    {
        if (Console.console_backlog.Count > 0)
        {
            for (int i = 0; i < Console.console_backlog.Count; i++)
            {
                Debug.Log($"{i}: {Console.console_backlog.Count}");
                _SuperSecretInternalConsoleLog(Console.console_backlog[i].a, Console.console_backlog[i].b);
            }
            Console.console_backlog.Clear();
        }
    }

    private void OnApplicationQuit()
    {
        SaveConsole(null);
    }

    public IEnumerator AssembleHelpMenu()
    {
        yield return new WaitForFixedUpdate();

        GlobalEvent.Append("Console", SelfCommands);
        GlobalEvent.Invoke("Console");

        yield return new WaitForFixedUpdate();
        foreach (var a in pathcomps)
        {
            RealAppend(a.a, a.b);
        }
        CommandDict2 = (from entry in CommandDict orderby entry.Key ascending select entry);
    }


    private void Update()
    {
        if (InputManager.IsKeyDown("console") && !enable)
        {
            ConsoleChange(!enable);
        }
        else if (InputManager.IsKeyDown("console", "Console"))
        {
            if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.name != ConsoleObjectRef.input.gameObject.name)
            {
                ConsoleObjectRef.fix.Select();
                ConsoleObjectRef.input.Select();
            }
        }
        if (enable)
        {
            if (InputManager.IsKeyDown("console_autofill", "Console"))
            {
                AutoFill();
            }
            else if (InputManager.IsKeyDown("close_menu"))
            {
                ConsoleChange(false);
            }
            if (InputManager.IsKeyDown("console_up"))
            {
                CommandChange(-1);
            }
            if (InputManager.IsKeyDown("console_down"))
            {
                CommandChange(1);
            }
        }

    }

    public void CommandChange(int i)
    {
        var sp = ConsoleObjectRef.input;
        var a = new List<string>(prev_commands);
        a.Add(OldS);
        var b = comm;
        if (prev_commands.Count > 0)
        {
            comm += i;
            comm = Math.Clamp(comm, 0, a.Count - 1);
            b = comm;
            sp.text = a[comm];
        }
        ConsoleObjectRef.input.MoveTextEnd(false);
        OldS = a[a.Count - 1];
        comm = b;
    }
    private Coroutine botju;
    public IEnumerator BottomJump()
    {
        var pp = ConsoleObjectRef.scrollbar;
        if (pp != null) pp.value = 1;
        yield return new WaitForFixedUpdate();
        var ppw = ConsoleObjectRef.scrollview;
        ConsoleObjectRef.backlog.text = BackLog;
        ConsoleObjectRef.backlog_size.SetLayoutVertical();
        ppw.CalculateLayoutInputVertical();
        ppw.SetLayoutVertical();
        if (pp != null) pp.value = 1;
        yield return new WaitForFixedUpdate();
        if (pp != null) pp.value = 1;
        yield return new WaitForFixedUpdate();
        if (pp != null) pp.value = 1;
        yield return new WaitForFixedUpdate();
        if (pp != null) pp.value = 1;
        yield return new WaitForFixedUpdate();
        if (pp != null) pp.value = 1;
    }


    public void SelfCommands()
    {
        Add(new OXCommand("test", "Console", "Message_HelpTest")
            .Append(new OXCommand("tag").Action(ConsoleCommands.Test_tag))
            .Append(new OXCommand("circle").Action(ConsoleCommands.Test_circle))
            .Append(new OXCommand("listall").Action(ConsoleCommands.Test_listall))
            .Append(new OXCommand("compver").Action(ConsoleCommands.Test_compver))
            .Append(new OXCommand("escape").Action(ConsoleCommands.Test_escape))
            .Append(new OXCommand("max").Action(ConsoleCommands.Test_max))
            .Append(new OXCommand("dict").Action(ConsoleCommands.Test_dict))
            .Append(new OXCommand("gevent").Action(ConsoleCommands.Test_gevent))
            .Append(new OXCommand("remap").Action(ConsoleCommands.Test_remap))
            .Append(new OXCommand("cleanstack").Action(ConsoleCommands.Test_cleanstack))
            .Append(new OXCommand("comp")
                .Append(new OXCommand(OXCommand.ExpectedInputType.String)
                    .Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(ConsoleCommands.Test_comp))))
            .Append(new OXCommand("roman").Action(ConsoleCommands.Test_roman))
            .Append(new OXCommand("refs").Action(ConsoleCommands.Test_refs))
            .Append(new OXCommand("threadedlogs").Action(ConsoleCommands.Test_threadedlogs))
            .Append(new OXCommand("destroy").Action(ConsoleCommands.Test_destroy))
            .Append(new OXCommand("read")
                .Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(ConsoleCommands.Test_read)))
            .Append(new OXCommand("events").Action(ConsoleCommands.Test_events)));


        Add(new OXCommand("settimescale", "Console", "Message_HelpTime")
            .Append(new OXCommand(OXCommand.ExpectedInputType.Double).Action(ConsoleCommands.settimescale)));
        Add(new OXCommand("joe").Action(ConsoleCommands.joe)
            .Append(new OXCommand("mother").Action(ConsoleCommands.joe_mother)));
        Add(new OXCommand("help", "Console", "Message_HelpHelp").Action(ConsoleCommands.Help)
            .Append(new OXCommand(OXCommand.ExpectedInputType.Long).Action(ConsoleCommands.Help))
            .Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(ConsoleCommands.HelpSpecif)));
        Add(new OXCommand("clear", "Console", "Message_HelpClear").Action(ConsoleCommands.Clear));
        Add(new OXCommand("data", "Console", "Message_HelpData")
            .Append(new OXCommand("edit").Action(ConsoleCommands.Data_Edit))
            .Append(new OXCommand("read").Action(ConsoleCommands.Data_Read))
            .Append(new OXCommand("listall").Action(ConsoleCommands.Data_listall))
            );
        Add(new OXCommand("howmanywouldyoutake").Action(() => { Console.Log("49 Bullets!"); }));
        Add(new OXCommand("logtofile", "Console", "Message_HelpLogToFile")
            .Action(ConsoleCommands.LogToFile));
    }
    private static OXCommandData raa;

    private string OldS = "";
    public void Submit(string inputgaming)
    {
        if (InputManager.IsKeyDown("console") || InputManager.IsKeyDown("close_menu") || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;
        if (InputManager.IsKeyDown("console_autofill", "Console")) return;
        try
        {
            if (botju != null) StopCoroutine(botju);
        }
        catch
        {
            return;
        }
        botju = StartCoroutine(BottomJump());
        var pp = ConsoleObjectRef.scrollbar;
        if (pp != null) pp.value = 1;
        var lang = LanguageFileSystem.Instance;
        //this must be run when the text is finished editing
        try
        {
            ConsoleObjectRef.input.text = "";
            s = inputgaming;
            if (s != "" && (prev_commands.Count == 0 || prev_commands[prev_commands.Count - 1] != s))
            {
                prev_commands.Add(s);
            }
            s = Regex.Replace(s, "[<>]", "");
            if (prev_commands.Count > 50)
            {
                prev_commands.RemoveAt(0);
            }
            var s2 = s;
            if (s == "") return;
            s = s.ToLower();
            command = s.Split(' ').ToList();
            command_caps = s2.Split(' ').ToList();

            for (int i = 0; i < 5; i++)
            {
                command.Add("");
                command_caps.Add("");
            }
            Console.Log("> " + s2, "#7a7a7aff");

            raa = new OXCommandData();
            raa.com = command;
            raa.com_caps = command_caps;
            raa.raw = s;
            raa.raw_caps = s2;

            OXCommand d = null;
            d = CommandDict[command[0]];
            RecursiveCheck(d, 1);



        }
        catch
        {
            Console.LogError(lang.GetString("Console", "Error_InvalidCommand"));
        }

        comm = prev_commands.Count;
        if (enable)
        {
            ConsoleObjectRef.fix.Select();
            ConsoleObjectRef.input.Select();
        }
    }

    private OXCommand bestmatch;
    public string NewVal(string e, bool ReturnInstead)
    {
        OldS = e;
        comm = prev_commands.Count;
        e = e.ToLower();

        var a = Converter.StringToList(e, " ");
        bestmatch = null;
        string besttext = "";

        if (e == null || e == "")
        {
            if (!ReturnInstead) ConsoleObjectRef.predictr.text = "";
            return "";
        }
        foreach (var c in a)
        {
            if (c == "")
            {
                if (!ReturnInstead) ConsoleObjectRef.predictr.text = "";
                return "";
            }
        }

        foreach (var c in CommandDict2)
        {
            if (c.Key.StartsWith(a[0]))
            {
                bestmatch = c.Value;
                besttext = bestmatch.Value;
                break;
            }
        }
        int fd = 0;
        if (bestmatch != null)
        {
            for (int i = 1; i < a.Count; i++)
            {
                if (a[i] == "") break;
                if (i == 1 && bestmatch.Value == "help")
                {
                    foreach (var banana in CommandDict2)
                    {
                        if (banana.Value.Value.StartsWith(a[i]))
                        {
                            bestmatch = banana.Value;
                            besttext = bestmatch.Value;
                            fd++;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var banana in bestmatch.SubCommands)
                    {
                        if (banana.Value == "-=-") continue;
                        if (banana.Value.StartsWith(a[i]))
                        {
                            bestmatch = banana;
                            besttext = bestmatch.Value;
                            fd++;
                            break;
                        }
                    }
                }
            }
        }

        if (bestmatch != null && fd == a.Count - 1)
        {
            if (!ReturnInstead) ConsoleObjectRef.predictr.text = $"<color=#00000000>{ConsoleObjectRef.input.text}</color>" + besttext.Substring(a[fd].Length);
            return besttext.Substring(a[fd].Length);
        }
        else
        {
            bestmatch = null;
            if (!ReturnInstead) ConsoleObjectRef.predictr.text = "";
            return "";
        }

    }
    public void AutoFill()
    {
        if (bestmatch != null)
        {
            var e = ConsoleObjectRef.input.text;
            var a = Converter.StringToList(e, " ");
            var dd = a[a.Count - 1].Length;
            e += bestmatch.Value.Substring(dd) + " ";
            ConsoleObjectRef.input.text = e;
            NewVal(e, false);


            ConsoleObjectRef.input.MoveTextEnd(false);
        }
    }


    public void RecursiveCheck(OXCommand cum, int lvl)
    {
        if (cum.SubCommands.Count == 0)
        {
            if (cum.Execution != null)
            {
                cum.Execution(raa);
            }
            else
            {
                Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_NoCode"));
            }
            return;
        }
        OXCommand next_cum = null;
        foreach (var a in cum.SubCommands)
        {
            if (a.Value == command[lvl])
            {
                next_cum = a;
                break;
            }
        }
        if (next_cum != null)
        {
            RecursiveCheck(next_cum, lvl + 1);
            return;
        }
        if (command[lvl] != "")
        {
            List<OXCommand.ExpectedInputType> accepteds = new List<OXCommand.ExpectedInputType>();
            accepteds.Add(OXCommand.ExpectedInputType.String);
            try
            {
                double y = double.Parse(command[lvl]);
                accepteds.Add(OXCommand.ExpectedInputType.Double);
                long x = long.Parse(command[lvl]);
                accepteds.Add(OXCommand.ExpectedInputType.Long);
            }
            catch
            {

            }
            foreach (var a in cum.SubCommands)
            {
                if (accepteds.Contains(a.Expected))
                {
                    next_cum = a;
                    break;
                }
            }
            if (next_cum != null)
            {
                RecursiveCheck(next_cum, lvl + 1);
                return;
            }
        }

        if (cum.Execution != null && command[lvl] == "")
        {
            cum.Execution(raa);
            return;
        }
        Console.LogError(LanguageFileSystem.Instance.GetString("Console", "Error_InvalidParameter") + command_caps[lvl]);
    }
    public void _SuperSecretInternalConsoleLog(string text = "Logged", string hex = "\"white\"")
    {
        BackLog = BackLog + "<br><color=" + hex + ">" + text;
        if (BackLog.Length > 10000)
        {
            var pp = BackLog.IndexOf("<br>");
            BackLog = BackLog.Substring(pp + 4);
        }
        if (botju != null) StopCoroutine(botju);
        botju = StartCoroutine(BottomJump());
    }
    public void CloseConsole()
    {
        ConsoleChange(false);
    }


    public void TestMethod()
    {
        Console.Log("Test Log");
    }

    public void ClearConsole()
    {
        BackLog = "";
    }
    private void ConsoleChange(bool e = false)
    {
        enable = e;
        ConsoleObject.SetActive(e);
        if (e)
        {
            InputManager.AddLockLevel("Console");
            FixLol();
            ConsoleObjectRef.input.text = "";
            NewVal("", false);
        }
        else
        {
            InputManager.RemoveLockLevel("Console");
        }
    }
    public void FixLol()
    {
        ConsoleObjectRef.fix.Select();
        ConsoleObjectRef.input.Select();
        ConsoleObjectRef.input.MoveTextEnd(false);
    }

    public void Add(OXCommand x)
    {
        CommandDict.Add(x.Value, x);
    }
    public List<MultiRef<string, OXCommand>> pathcomps = new List<MultiRef<string, OXCommand>>();
    public void Append(string path, OXCommand x)
    {
        pathcomps.Add(new MultiRef<string, OXCommand>(path, x));
    }
    private void RealAppend(string path, OXCommand x)
    {
        // path formatted like "help listall", like how you would type in the console normally
        var a = path.StringToList(" ").Clean();
        OXCommand latest = CommandDict[a[0]];
        a.RemoveAt(0);
        foreach (var b in a)
        {
            foreach (var c in latest.SubCommands)
            {
                if (c.Value == b)
                {
                    latest = c;
                    break;
                }
            }
        }
        latest.Append(x);
    }

    public void SaveConsole(SaveProfile dict)
    {
        if (!SavePrevCommands) return;
        var d = SaveSystem.Profile("console");
        d.SetList("prev", prev_commands);
        SaveSystem.Instance.SaveDataToFile(d);
    }
    public void LoadConsole(SaveProfile dict)
    {
        if (!SavePrevCommands) return;
        var d = SaveSystem.Profile("console");
        SaveSystem.Instance.GetDataFromFile(d);
        prev_commands = d.GetList("prev", prev_commands);
    }

}

public static class Console
{
    public static List<MultiRef<string, string>> console_backlog = new List<MultiRef<string, string>>();
    // a shortcut/shorthand for the console, makes writing to the console faster
    public static void Log(this object text, string hex = "#bdbdbdff")
    {
        Log(text.ToString(), hex);
    }
    public static void Log(string text, string hex = "#bdbdbdff")
    {
        // hex can also = "\"white\""
        if (ConsoleLol.Instance != null && Thread.CurrentThread == ConsoleLol._mainthread)
        {
            ConsoleLol.Instance._SuperSecretInternalConsoleLog(text, hex);
        }
        else
        {
            console_backlog.Add(new MultiRef<string, string>(text, hex));
        }
    }
    public static void Log(this object text)
    {
        Log(text.ToString(), "#bdbdbdff");
    }
    public static void LogError(this object text)
    {
        Log(text, "#ff0000ff");
    }
    public static void LogWarning(this object text)
    {
        Log(text, "#ffb861ff");
    }
    public static void DLog(this object text)
    {
        Debug.Log(text);
    }
    public static void DLogError(this object text)
    {
        Debug.LogError(text);
    }
    public static void DLogWarning(this object text)
    {
        Debug.LogWarning(text);
    }
}




public class OXCommand
{
    public string Value;
    public List<OXCommand> SubCommands = new List<OXCommand>();
    public string ParentLanguage = "";
    public string LanguageIndex = "";
    public bool IsLeaf = false;
    public bool NoDesc = false;
    public ExpectedInputType Expected = ExpectedInputType.None;
    public System.Action<OXCommandData> Execution;
    public OXCommand(string Value, string parent_lang, string lang_index)
    {
        this.Value = Value;
        IsLeaf = true;
        ParentLanguage = parent_lang;
        LanguageIndex = lang_index;
    }
    public OXCommand(string Value)
    {
        this.Value = Value;
        IsLeaf = true;
        NoDesc = true;
    }
    public OXCommand(ExpectedInputType ex)
    {
        Expected = ex;
        Value = "-=-";
        IsLeaf = true;
        NoDesc = true;
    }
    public OXCommand Append(OXCommand x)
    {
        SubCommands.Add(x);
        IsLeaf = false;
        return this;
    }
    public OXCommand Action(System.Action<OXCommandData> exe)
    {
        Execution = exe;
        return this;
    }
    public OXCommand Action(System.Action exe)
    {
        Execution = (x) => { exe(); };
        return this;
    }

    public enum ExpectedInputType
    {
        None,
        Double,
        Long,
        String,
    }

}

public class OXCommandData
{
    public List<string> com = new List<string>();
    public List<string> com_caps = new List<string>();
    public string raw = "";
    public string raw_caps = "";
}

public class ConsoleCommandBuilder
{
    public static void Build(Action build)
    {
        GlobalEvent.Append("Console", build);
    }
}