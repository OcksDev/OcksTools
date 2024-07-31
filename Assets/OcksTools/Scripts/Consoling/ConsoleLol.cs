using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using static System.Net.Mime.MediaTypeNames;

public class ConsoleLol : MonoBehaviour
{
    public GameObject ConsoleObject;
    private static ConsoleLol instance;

    public bool enable = false;
    private List<string> prev_commands = new List<string>();
    private string s = "";
    private List<string> command = new List<string>();
    private List<string> command_caps = new List<string>();
    public string BackLog = "";
    private int balls = 0;
    private int comm = 0;


    /* the setup process!
     * 
     * 1. Use the ockstools window found at the top of the unity editor in the path OcksTools/Console/Utils
     * 2. Place the console object parent in the given field and click the setup button
     * 3. Navigate to the newly spawned console object and go to the input field within it
     * 4. in the "On End Edit" section, set the object to be run to the ConsoleHandler object that was also spawned \
     * 5. Set the method to be ConsoleLol.Submit()
     * 6. You are done
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
        ConsoleChange(false);
        if (Instance == null) instance = this;
    }
    public void Start()
    {
        //console data
        var f = new Dictionary<string, string>
        {
            { "Message_ChangeTime", "Time scale changed to " },
            { "Message_StoppedDialog", "All dialog has been stopped" },
            { "Message_Help", "Available Commands:" },
            { "Message_HelpData", "Allows for the modification of saved game data" },
            { "Message_HelpTest", "Runs some tests and stuff" },
            { "Message_HelpDialog", "General dialog manager" },
            { "Message_HelpTime", "Sets the scale of time" },
            { "Message_HelpClear", "Clears the console of all messages" },
            { "Error_InvalidCommand", "Invalid command" },
            { "Error_UnknownCommand", "Unknown Command: " },
            { "Error_NoDataDump", "Game not configured to allow for mass data viewing" },
            { "Error_NoReg", "No registry inputted" },
            { "Error_NoData", "- No Data -" },
            { "Error_InvalidTime", "Invalid time scale input" },
            { "Error_InvalidData", "Invalid data modification selected" }
        };

        var l = LanguageFileSystem.Instance;
        bool changed = false;
        foreach(var item in f)
        {
            if (!l.IndexValuePairs.ContainsKey(item.Key))
            {
                changed = true;
                l.IndexValuePairs.Add(item.Key, item.Value);
            }
        }
        if (changed)
        {
            l.UpdateTextFile();
        }

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
        if (InputManager.IsKeyDown(InputManager.gamekeys["console"]))
        {
            ConsoleChange(!enable);
        }else if(InputManager.IsKeyDown(InputManager.gamekeys["close_menu"]))
        {
            ConsoleChange(false);
        }


        if (enable && InputManager.IsKeyDown(InputManager.gamekeys["console_up"]))
        {
            CommandChange(-1);
        }
        if (enable && InputManager.IsKeyDown(InputManager.gamekeys["console_down"]))
        {
            CommandChange(1);
        }
    }
    private void LateUpdate()
    {
        if (balls > 0)
        {
            balls--;
            var pp = ConsoleObject.GetComponentInChildren<Scrollbar>();
            if (pp != null) pp.value = 1;
        }
    }

    public void CommandChange(int i)
    {
        var sp = ConsoleObject.GetComponentInChildren<TMP_InputField>();
        if(prev_commands.Count > 0)
        {
            comm += i;
            comm = Math.Clamp(comm, 0, prev_commands.Count - 1);
            sp.text = prev_commands[comm];
        }
    }

    public void Submit()
    {
        if (InputManager.IsKeyDown(InputManager.gamekeys["console"]) || InputManager.IsKeyDown(InputManager.gamekeys["close_menu"]) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;
        balls = 3;
        var pp = ConsoleObject.GetComponentInChildren<Scrollbar>();
        if (pp != null) pp.value = 1;
        var lang = LanguageFileSystem.Instance;
        //this must be run when the text is finished editing
        try
        {
            s = ConsoleObject.GetComponentInChildren<TMP_InputField>().text;
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

                                lang.IndexValuePairs["Message_HelpTime"] +
                                "<br> - settimescale <time scale>"

                            ), "#bdbdbdff");
                            break;
                        case "dialog":
                            ConsoleLog((

                                lang.IndexValuePairs["Message_HelpDialog"] +
                                "<br> - dialog <#>" + 
                                "<br> - dialog stop"

                            ), "#bdbdbdff");
                            break;
                        case "test":
                            ConsoleLog((

                                lang.IndexValuePairs["Message_HelpTest"] +
                                "<br> - test chat" +
                                "<br> - test tag" +
                                "<br> - test circle" +
                                "<br> - test destroy" +
                                "<br> - test garbage" +
                                "<br> - test listall"

                            ), "#bdbdbdff");
                            break;
                        case "data":
                            ConsoleLog((

                                lang.IndexValuePairs["Message_HelpData"] +
                                "<br> - data edit <key> <value>" +
                                "<br> - data read <key>" +
                                "<br> - data listall"

                            ), "#bdbdbdff");
                            break;
                        case "clear":
                            ConsoleLog((

                                lang.IndexValuePairs["Message_HelpClear"] +
                                "<br> - clear"

                            ), "#bdbdbdff");
                            break;
                        default:
                            ConsoleLog((

                                lang.IndexValuePairs["Message_Help"] +
                                "<br> - joe" +
                                "<br> - settimescale" +
                                "<br> - test" +
                                "<br> - dialog" +
                                "<br> - data" +
                                "<br> - clear"

                            ), "#bdbdbdff");
                            break;

                    }
                    break;

                case "test":
                    switch (command[1])
                    {
                        case "tag":
                            Tags.dict.Add("penis", gameObject);

                            ConsoleLog((

                                "test result: " + Tags.dict["penis"].name

                            ), "#bdbdbdff");
                            Tags.ClearAllOf("penis");
                            break;
                        case "circle":
                            SpawnSystem.Instance.SpawnObject(0, gameObject, Vector3.zero, Quaternion.Euler(0, 0, 0));
                            break;
                        case "chat":
                            for(int i = 0; i < 10; i++)
                            {
                                ChatLol.Instance.WriteChat("Chat Test Lol", "#" + UnityEngine.Random.ColorHSV().ToHexString());
                            }
                            break;
                        case "listall":
                            foreach (var d in Tags.dict)
                                ConsoleLog((

                                    "test result: " + d

                                ), "#bdbdbdff");
                            break;
                        case "max":
                            ConsoleLog((

                                "Double Max: " + double.MaxValue.ToString()

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
                            foreach (var d in Tags.dict)
                                Destroy(d.Value);
                            break;
                        case "garbage":
                            Tags.GarbageCleanup();
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

                                lang.IndexValuePairs["Message_StoppedDialog"]

                            ), "#bdbdbdff");
                            break;
                        default:
                            DialogLol.Instance.StartDialog(int.Parse(command[1]));
                            CloseConsole();
                            break;
                    }
                    break;
                case "settimescale":
                    try
                    {
                        float f = float.Parse(command[1]);
                        Time.timeScale = f;
                        ConsoleLog((

                            lang.IndexValuePairs["Message_ChangeTime"] + f

                        ), "#bdbdbdff");
                    }
                    catch
                    {
                        ConsoleLog((

                            lang.IndexValuePairs["Error_InvalidTime"]

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
                                ConsoleLol.Instance.ConsoleLog(lang.IndexValuePairs["Error_NoReg"], "#ff0000ff");
                            }
                            break;
                        case "read":
                            if (command[2] != "")
                            {
                                ConsoleLol.Instance.ConsoleLog($"{SaveSystem.Instance.GetString(command_caps[2], lang.IndexValuePairs["Error_NoData"])}");
                            }
                            else
                            {
                                ConsoleLol.Instance.ConsoleLog(lang.IndexValuePairs["Error_NoReg"], "#ff0000ff");
                            }
                            break;
                        case "listall":
                            if (SaveSystem.Instance.UseFileSystem)
                            {
                                ConsoleLol.Instance.ConsoleLog($"{Converter.DictionaryToString( SaveSystem.Instance.GetDict(), Environment.NewLine, ": ")}");
                            }
                            else
                            {
                                ConsoleLol.Instance.ConsoleLog(lang.IndexValuePairs["Error_NoDataDump"], "#ff0000ff");
                            }
                            break;
                        default:
                            ConsoleLol.Instance.ConsoleLog(lang.IndexValuePairs["Error_InvalidData"], "#ff0000ff");
                            break;
                    }
                    break;
                default:
                    ConsoleLog(lang.IndexValuePairs["Error_UnknownCommand"] + command[0], "#ff0000ff");

                    break;
            }
        }
        catch
        {
            ConsoleLog(lang.IndexValuePairs["Error_InvalidCommand"], "#ff0000ff");
        }

        balls = 3;
        comm = prev_commands.Count;
    }

    public void ConsoleLog(string text = "Logged", string hex = "\"white\"")
    {
        BackLog = BackLog + "<br><color=" + hex + ">" + text;
        balls = 1;
    }
    public void CloseConsole()
    {
        ConsoleChange(false);
    }
    void ConsoleChange(bool e = false)
    {
        enable = e;
        ConsoleObject.SetActive(e);
        if (e)
        {
            var imp = ConsoleObject.GetComponentInChildren<TMP_InputField>();
            var imp2 = ConsoleObject.GetComponentInChildren<Button>();
            imp.text = "";
            //stupid fucking selection fix I hate this.
            //if I dont have this useless line of code it stops working and I dont know why
            imp2.Select();
            imp.Select();
        }
    }
}

public class Console : MonoBehaviour
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