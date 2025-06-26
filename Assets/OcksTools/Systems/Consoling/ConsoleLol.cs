using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.EventSystems;

public class ConsoleLol : MonoBehaviour
{
    public GameObject ConsoleObject;
    public ConsolRefs ConsoleObjectRef;
    public OXLanguageFileIndex LanguageFileIndex;
    private static ConsoleLol instance;

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







    // Start is called before the first frame update
    public static ConsoleLol Instance
    {
        get { return instance; }

        //bug: you can use rich text like <br> and <i> in the console 
    }

    private void Awake()
    {
        prev_commands.Clear();
        BackLog = "";
        ConsoleObjectRef = ConsoleObject.GetComponent<ConsolRefs>();
        ConsoleChange(false);
        if (Instance == null) instance = this;
    }
    public void Start()
    {

        var l = LanguageFileSystem.Instance;
        l.AddFile(LanguageFileIndex);

        if(Console.texts.Count > 0)
        {
            for(int i = 0; i < Console.texts.Count; i++)
            {
                ConsoleLog(Console.texts[i], Console.hexes[i]);
            }
            Console.texts.Clear();
            Console.hexes.Clear();
        }

    }

    private void Update()
    {
        if (InputManager.IsKeyDown("console", "def"))
        {
            ConsoleChange(!enable);
        }
        else if (InputManager.IsKeyDown("console", "Console"))
        {;
            if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.name != ConsoleObjectRef.input.gameObject.name)
            {
                ConsoleObjectRef.fix.Select();
                ConsoleObjectRef.input.Select();
            }
        }
        else if (InputManager.IsKeyDown("close_menu"))
        {
            ConsoleChange(false);
        }


        if (enable && InputManager.IsKeyDown("console_up"))
        {
            CommandChange(-1);
        }
        if (enable && InputManager.IsKeyDown("console_down"))
        {
            CommandChange(1);
        }
    }

    public void CommandChange(int i)
    {
        var sp = ConsoleObjectRef.input;
        if(prev_commands.Count > 0)
        {
            comm += i;
            comm = Math.Clamp(comm, 0, prev_commands.Count - 1);
            sp.text = prev_commands[comm];
        }
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

    public void Submit(string inputgaming)
    {
        if (InputManager.IsKeyDown("console") || InputManager.IsKeyDown("close_menu") || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;
        if(botju != null) StopCoroutine(botju);
        botju = StartCoroutine(BottomJump());
        var pp = ConsoleObjectRef.scrollbar;
        if (pp != null) pp.value = 1;
        var lang = LanguageFileSystem.Instance;
        //this must be run when the text is finished editing
        try
        {
            s = inputgaming;
            if (s != "" && (prev_commands.Count == 0 || prev_commands[prev_commands.Count - 1] != s)) prev_commands.Add(s);
            var s2 = s;
            if (s == "") return;
            s = s.ToLower();
            command = s.Split(' ').ToList();
            command_caps = s2.Split(' ').ToList();

            for (int i = 0; i<10; i++)
            {
                command.Add("");
            }
            ConsoleLog("> " + s2, "#7a7a7aff");
            switch (command[0])
            {
                case "help":
                    switch (command[1])
                    {
                        case "joe":
                            ConsoleLog((

                                "Joe" +
                                "<br> - joe <mother>"

                            ), "#bdbdbdff");
                            break;
                        case "settimescale":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpTime") +
                                "<br> - settimescale <time scale>"

                            ), "#bdbdbdff");
                            break;
                        case "screenshot":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpScreenshot") +
                                "<br> - screenshot"

                            ), "#bdbdbdff");
                            break;
                        case "dialog":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpDialog") +
                                "<br> - dialog <#>" + 
                                "<br> - dialog stop"

                            ), "#bdbdbdff");
                            break;
                        case "test":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpTest") +
                                "<br> - test chat" +
                                "<br> - test tag" +
                                "<br> - test circle" +
                                "<br> - test destroy" +
                                "<br> - test listall" +
                                "<br> - test events" +
                                "<br> - test escape" +
                                "<br> - test compver"+
                                "<br> - test roman"

                            ), "#bdbdbdff");
                            break;
                        case "data":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpData") +
                                "<br> - data edit <key> <value>" +
                                "<br> - data read <key>" +
                                "<br> - data listall"

                            ), "#bdbdbdff");
                            break;
                        case "clear":
                            ConsoleLog((

                                lang.GetString("Console", "Message_HelpClear") +
                                "<br> - clear"

                            ), "#bdbdbdff");
                            break;
                        default:
                            ConsoleLog((

                                lang.GetString("Console", "Message_Help") +
                                "<br> - joe" +
                                "<br> - settimescale" +
                                "<br> - test" +
                                "<br> - dialog" +
                                "<br> - data" +
                                "<br> - screenshot" +
                                "<br> - clear"

                            ), "#bdbdbdff");
                            break;

                    }
                    break;

                case "test":
                    switch (command[1])
                    {
                        case "tag":
                            Tags.AllTags["Exist"].Add("penis", gameObject);

                            ConsoleLog((

                                "test result: " + Tags.AllTags["Exist"]["penis"].name

                            ), "#bdbdbdff");
                            Tags.ClearAllOf("penis");
                            break;
                        case "circle":
                            SpawnSystem.Instance.SpawnObject("Circle", gameObject, Vector3.zero, Quaternion.Euler(0, 0, 0));
                            break;
                        case "chat":
                            for(int i = 0; i < 10; i++)
                            {
                                ChatLol.Instance.WriteChat("Chat Test Lol", "#" + UnityEngine.Random.ColorHSV().ToHexString());
                            }
                            break;
                        case "listall":
                            foreach (var d in Tags.AllTags["Exist"])
                                ConsoleLog((

                                    "test result: " + d

                                ), "#bdbdbdff");
                            break;
                        case "compver":
                            ConsoleLog((

                                "comp1: " + RandomFunctions.CompareTwoVersions("v1.0","v1.1").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp2: " + RandomFunctions.CompareTwoVersions("v1.2","v1.1").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp2: " + RandomFunctions.CompareTwoVersions("v1.2","v2.1").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp3: " + RandomFunctions.CompareTwoVersions("v1.2","v2.1.3").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp4: " + RandomFunctions.CompareTwoVersions("v1.2.0","2.1.3").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp5: " + RandomFunctions.CompareTwoVersions("1.2.0.7.32.2.1.3", "1.2.0.7.32.2.1.2").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp6: " + RandomFunctions.CompareTwoVersions("1.2.3.4.5.6.7.8", "1.2.a").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp7: " + RandomFunctions.CompareTwoVersions("1.2", "1.2.a").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp8: " + RandomFunctions.CompareTwoVersions("1.2.5", "1.2.a").ToString()

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "comp9: " + RandomFunctions.CompareTwoVersions("v1.2", "1.2.3.4.5").ToString()

                            ), "#bdbdbdff");
                            break;
                        case "escape":
                            string banana = "help(0)eat()cumjjragbanana your_welcum";
                            List<string> escape = new List<string>() { "eat" , "cum", "rag", "jj" };
                            ConsoleLog((

                                "input: " + banana

                            ), "#bdbdbdff");
                            ConsoleLog((

                                $"remove: {escape[0]}, {escape[1]}, {escape[2]}, {escape[3]}"

                            ), "#bdbdbdff");
                            banana = Converter.EscapeString(banana, escape);
                            ConsoleLog((

                                "escaped: " + banana

                            ), "#bdbdbdff");
                            banana = Converter.UnescapeString(banana, escape);
                            ConsoleLog((

                                "result: " + banana

                            ), "#bdbdbdff");
                            break;
                        case "max":
                            ConsoleLog((

                                "Double Max: " + double.MaxValue.ToString()

                            ), "#bdbdbdff");
                            break;
                        case "roman":
                            ConsoleLog((

                                "Roman of 129: " + Converter.NumToRead("129",3)

                            ), "#bdbdbdff");
                            ConsoleLog((

                                "Roman of 3999: " + Converter.NumToRead("3999",3)

                            ), "#bdbdbdff");
                            break;
                        case "refs":
                            string cum = "";
                            int icum = 0;
                            foreach(var d in Tags.refs)
                            {
                                cum += d.Key + ": " + d.Value.name;
                                if (icum < Tags.refs.Count - 1) cum += "<br>";
                                icum++;
                            }
                            ConsoleLog((

                                cum

                            ), "#bdbdbdff");
                            break;
                        case "destroy":
                            foreach (var d in Tags.AllTags["Exist"])
                                Destroy(d.Value);
                            break;
                        case "events":
                            var weenor = new OXEvent();
                            var banan = new OXEvent<string, string>();
                            weenor.Append("test1", TestMethod);
                            weenor.Invoke();
                            banan.Append("logger", Console.Log);
                            banan.Invoke("Hello World", "\"green\"");
                            weenor.Remove("test1");
                            weenor.Invoke();
                            banan.Invoke("Removed test log", "\"yellow\"");
                            banan.Append("logger2", Console.Log);
                            banan.Append("logger3", Console.Log);
                            banan.Invoke("multiple calls?", "\"orange\"");
                            break;
                        default:
                            ConsoleLog((

                                "Invalid Test"

                            ), "#ff0000ff");
                            break;

                    }
                    break;
                case "joe":
                    switch (command[1])
                    {
                        case "mother":
                            ConsoleLog((

                                "AYYYYYEEEEEE"

                            ), "#bdbdbdff");
                            break;
                        default:
                            ConsoleLog((

                                "Who is joe?"

                            ), "#bdbdbdff");
                            break;
                    }
                    break;
                case "dialog":
                    switch (command[1])
                    {
                        case "stop":
                            DialogLol.Instance.ResetDialog();
                            ConsoleLog((

                                lang.GetString("Console", "Message_StoppedDialog")

                            ), "#bdbdbdff");
                            break;
                        default:
                            if (DialogLol.Instance.DialogFilesDict.ContainsKey(command_caps[1]) || DialogLol.Instance.ChooseFilesDict.ContainsKey(command_caps[1]))
                            {
                                DialogLol.Instance.StartDialog(command_caps[1]);
                                CloseConsole();
                            }
                            else
                            {
                                ConsoleLog((

                                    lang.GetString("Console", "Error_NoDialogWithName") + $"\"{command_caps[1]}\""

                                ), "#ff0000ff");
                            }
                            break;
                    }
                    break;
                case "screenshot":
                    if(Screenshot.Instance != null)
                    {
                        CloseConsole();
                        var ss = new ScreenshotData("test", 1000, 1000, Camera.main, true);
                        Screenshot.Instance.TakeScreenshot(ss);
                    }
                    else
                    {
                        ConsoleLog((

                            lang.GetString("Console", "Error_NoScreenshot")

                        ), "#bdbdbdff");
                    }
                    break;
                case "settimescale":
                    try
                    {
                        float f = float.Parse(command[1]);
                        Time.timeScale = f;
                        ConsoleLog((

                            lang.GetString("Console", "Message_ChangeTime") + f

                        ), "#bdbdbdff");
                    }
                    catch
                    {
                        ConsoleLog((

                            lang.GetString("Console", "Error_InvalidTime")

                        ), "#bdbdbdff");
                    }
                    break;
                case "clear":
                    BackLog = "";
                    break;
                case "data":
                    switch (command[1])
                    {
                        case "edit":
                            if (command[2] != "")
                            {
                                string eee = s2.Substring(s.IndexOf(command[2]) + command[2].Length + 1);
                                SaveSystem.Instance.SetString(command_caps[2], eee);
                                ConsoleLol.Instance.ConsoleLog($"Saved \"{eee}\" into {command_caps[2]}");
                            }
                            else
                            {
                                ConsoleLol.Instance.ConsoleLog(lang.GetString("Console", "Error_NoReg"), "#ff0000ff");
                            }
                            break;
                        case "read":
                            if (command[2] != "")
                            {
                                ConsoleLol.Instance.ConsoleLog($"{SaveSystem.Instance.GetString(command_caps[2], lang.GetString("Console", "Error_NoData"))}");
                            }
                            else
                            {
                                ConsoleLol.Instance.ConsoleLog(lang.GetString("Console", "Error_NoReg"), "#ff0000ff");
                            }
                            break;
                        case "listall":
                            switch (SaveSystem.Instance.SaveMethod_)
                            {
                                case SaveSystem.SaveMethod.PlayerPrefs:
                                    ConsoleLol.Instance.ConsoleLog(lang.GetString("Console", "Error_NoDataDump"), "#ff0000ff");
                                    break;
                                case SaveSystem.SaveMethod.TXTFile:
                                    ConsoleLol.Instance.ConsoleLog($"{Converter.DictionaryToString(SaveSystem.Instance.GetDict(), Environment.NewLine, ": ")}");
                                    break;
                                case SaveSystem.SaveMethod.OXFile:

                                    string combined = "";
                                    foreach(var a in SaveSystem.Instance.GetDictOX().Data.DataOXFiles)
                                    {
                                        if(combined != "") combined += "<br>";
                                        combined += a.Key + ": " + a.Value.ToString();
                                    }
                                    ConsoleLol.Instance.ConsoleLog(combined);
                                    break;
                            }
                            break;
                        default:
                            ConsoleLol.Instance.ConsoleLog(lang.GetString("Console", "Error_InvalidData"), "#ff0000ff");
                            break;
                    }
                    break;
                default:
                    ConsoleLog(lang.GetString("Console", "Error_UnknownCommand") + command[0], "#ff0000ff");

                    break;
            }
        }
        
        catch
        {
            ConsoleLog(lang.GetString("Console", "Error_InvalidCommand"), "#ff0000ff");
        }

        comm = prev_commands.Count;
        ConsoleObjectRef.fix.Select();
        ConsoleObjectRef.input.Select();
    }


    public void ConsoleLog(string text = "Logged", string hex = "\"white\"")
    {
        BackLog = BackLog + "<br><color=" + hex + ">" + text;
        if(BackLog.Length > 10000)
        {
            var pp = BackLog.IndexOf("<br>");
            BackLog = BackLog.Substring(pp+4);
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


    void ConsoleChange(bool e = false)
    {
        enable = e;
        ConsoleObject.SetActive(e);
        if (e)
        {
            InputManager.AddLockLevel("Console");
            ConsoleObjectRef.fix.Select();
            ConsoleObjectRef.input.Select();
            ConsoleObjectRef.input.text = "";
        }
        else
        {
            InputManager.RemoveLockLevel("Console");
        }
    }
}

public class Console
{
    public static List<string> texts = new List<string>();
    public static List<string> hexes = new List<string>();
    // a shortcut/shorthand for the console, makes writing to the console faster
    public static void Log(string text = "Logged", string hex = "\"white\"")
    {
        if(ConsoleLol.Instance != null)
        {
            ConsoleLol.Instance.ConsoleLog(text, hex);
        }
        else
        {
            texts.Add(text);
            hexes.Add(hex);
        }
    }
}