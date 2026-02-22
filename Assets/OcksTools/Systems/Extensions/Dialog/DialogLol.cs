using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogLol : SingleInstance<DialogLol>
{
    public Dictionary<string, OXLanguageFileIndex> LanguageFileIndexes = new Dictionary<string, OXLanguageFileIndex>();
    public bool UseLanguageFileSystem = false;
    public static string BadGex = @"[\^*!&,]";
    public GameObject DialogBoxObject;
    private DialogBoxL pp;
    public List<DialogHolder> DialogFiles = new List<DialogHolder>();
    public List<DialogHolder> ChooseFiles = new List<DialogHolder>();
    public bool IsActive = false;
    public string filename = "";
    public int CurrentLine = 0;
    [HideInInspector]
    public float CharactersPerSecond = -1;
    [HideInInspector]
    public float WordDelay = -1;
    [HideInInspector]
    public float NewLineDelay = -1;
    [HideInInspector]
    public float MinorPunctuationDelay = -1;
    [HideInInspector]
    public float MajorPunctuationDelay = -1;
    private int CurrentCharacter = 0;
    private float MainTimerDelay = -1;
    private float MainTimer = -1;
    private float DelayTimer = -1;
    [HideInInspector]
    public float AutoSkip = -1;
    [HideInInspector]
    public string speaker = "";
    [HideInInspector]
    public string fulltext = "";
    [HideInInspector]
    public string color = "";
    [HideInInspector]
    public string bg_color = "";
    [HideInInspector]
    public string tit_color = "";
    [HideInInspector]
    public string datatype = "Dialog";
    [HideInInspector]
    public bool RichTextEnabled = true;
    [HideInInspector]
    public bool InstantShowAllText = true;
    [HideInInspector]
    public bool CanSkip = true;
    [HideInInspector]
    public bool CanEscape = false;
    [HideInInspector]
    public bool CanSkipBack = false;
    [HideInInspector]
    public string PlaySoundOnType = "";
    private List<string> str = new List<string>();
    private string ActiveFileName = "";
    private string baldcharacters = " \n\t";
    private Dictionary<string, string> variables = new Dictionary<string, string>();
    private Dictionary<string, DialogSettings> name_to_setting = new Dictionary<string, DialogSettings>();


    public DialogDefaults TrueDefaults;
    private DialogSettings CurrentSettings;


    // Start is called before the first frame update

    public override void Awake2()
    {
        DialogBoxObject.SetActive(true);

        InputManager.CollectInputAllocs.Append("Dialog", () =>
        {
            InputManager.CreateInputSet("dialog_skip", new List<KeyCode>() { KeyCode.Space, KeyCode.Mouse0, KeyCode.RightArrow });
            InputManager.CreateInputSet("dialog_skip_back", KeyCode.LeftArrow);
        });
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("dialog", "Console", "Message_HelpDialog")
                .Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(DialogCommand)));
        });
    }

    public static void DialogCommand(OXCommandData r)
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

    private void Start()
    {
        ResetDialog();

        GlobalEvent.Append("SecondInChain", () => StartDialog("TestSecondChain"));

        pp = DialogBoxObject.GetComponent<DialogBoxL>();
        //lets you write <*=*Var> as shorthand to insert a variable into the dialog
        SetVariable("", "Text");
        //some testing variables for the dialog system
        SetVariable("TestVar", "*VarInSideAVar");
        SetVariable("VarInSideAVar", "Name");
        SetVariable("Fuck", "Shit");
        SetVariable("Wank", "Wank");
        SetVariable("AttributeInsideVar", "<Name=Bone Eater>");
        SetVariable("NestedAttribute", "<Animate=Text,Rainbow>");
        SetVariable("MassApplyVariable", "ApplyStyle");
        SetVariable("Green", "255,0");
        SetVariable("ExtraDialog", "</> <Name=Red></> This is some bonus content!</> </> <Name=Blue></> Sure is!</>");
        SetVariable("CoolNameColor", "<TitleColor=0,255,255>");

        if (GetUseLFS())
        {
            FileSystem.Instance.CreateFolder($"{FileSystem.Instance.FileLocations["Lang"]}\\{FileSystem.GameVer}\\Dialog");
        }

        foreach (var a in DialogFiles)
        {
            var d = new OXLanguageFileIndex();
            d.FileName = $"Dialog\\{a.Name}";
            d.DefaultFile = a.File;
            d.DontParseDict = true;
            LanguageFileIndexes.Add(a.Name, d);
        }
        foreach (var a in ChooseFiles)
        {
            var d = new OXLanguageFileIndex();
            d.FileName = $"Dialog\\{a.Name}";
            d.DefaultFile = a.File;
            d.DontParseDict = true;
            LanguageFileIndexes.Add(a.Name, d);
        }
        if (GetUseLFS())
        {
            foreach (var a in LanguageFileIndexes)
            {
                LanguageFileSystem.Instance.AddFile(a.Value);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (InputManager.IsKeyDown("dialog_skip", "Dialog"))
        {
            if (datatype != "Choose")
            {
                attemptskip = true;
            }
        }
        if (InputManager.IsKeyDown("dialog_skip_back", "Dialog"))
        {
            if (datatype != "Choose")
            {
                backwardskip = true;
                attemptskip = true;
            }
        }
        if (attemptskip && !waitforinput)
        {
            if (godlyattemptskip)
            {
                CurrentCharacter = fulltext.Length;
            }
            else
            {
                if (!CanSkip && CurrentCharacter < fulltext.Length)
                {
                    goto ex;
                }
            }
            godlyattemptskip = false;
            //Debug.Log("Skip registered");
            MainTimer = 0;
            if (backwardskip)
            {
                PrevLine();
            }
            else
            {
                NextLine();
            }
        }
        else if (attemptskip && waitforinput)
        {
            waitforinput = false;
        }
    ex:
        attemptskip = false;
        backwardskip = false;
        if (CanEscape && InputManager.IsKeyDown("close_menu", "Dialog"))
        {
            ResetDialog();
        }
        /* example of starting the dialog from a specific list
        if (InputManager.IsKeyDown(KeyCode.J))
        {
            StartDialogFromCustomList(new List<string>()
            {
                "gooners",
                "<Name=Giga Gooner>",
                "Gooning time",
                "<End>",
            });
        }*/

        if (DelayTimer <= 0 && !waitforinput)
        {
            MainTimer -= Time.deltaTime;
            if (MainTimer <= 0 && CurrentCharacter != fulltext.Length)
            {
                MainTimer += MainTimerDelay;
                if (MainTimer < 0) CurrentCharacter += Math.Abs((int)(MainTimer / MainTimerDelay));
                if (!waited) CurrentCharacter += 1;
                waited = false;
                MainTimer = MainTimerDelay;
                upt();
                if (DelayTimer <= 0)
                {
                    string e = GetText();
                    if (e.Length > 0)
                    {
                        e = e.Substring(e.Length - 1, 1);
                    }
                    if (CurrentCharacter < fulltext.Length)
                    {
                        var dd = fulltext.Substring(0, Mathf.Clamp(CurrentCharacter + 1, 0, fulltext.Length));
                        if (dd.Length > 0) dd = dd.Substring(dd.Length - 1, 1);
                        if (e == " " || e.Contains("\n"))
                        {
                            DelayTimer = e == " " ? WordDelay : NewLineDelay;
                        }
                        else if (dd != " ")
                        {
                            //nothing
                        }
                        else if (e == "," || e == ";" || e == ":")
                        {
                            DelayTimer = MinorPunctuationDelay;
                        }
                        else if (e == "." || e == "!" || e == "?")
                        {
                            DelayTimer = MajorPunctuationDelay;
                        }
                    }
                    if (PlaySoundOnType != "" && !baldcharacters.Contains(e))
                    {
                        PlaySoundPreset(PlaySoundOnType);
                    }
                }
            }
        }
        else
        {
            DelayTimer -= Time.deltaTime;
        }
        if (AutoSkip >= 0 && !isautoproc && CurrentCharacter >= fulltext.Length && datatype != "Choose")
        {
            banna = StartCoroutine(AutoSkipe());
        }
    }
    private Coroutine banna;
    public IEnumerator AutoSkipe()
    {
        isautoproc = true;
        if (AutoSkip > 0) yield return new WaitForSeconds(AutoSkip);
        MainTimer = 0;
        NextLine();
        isautoproc = false;
    }
    public void EndBanna()
    {
        isautoproc = false;
        if (banna != null) StopCoroutine(banna);
    }
    private bool isautoproc = false;
    public void PlaySoundPreset(string index)
    {
        switch (index)
        {
            case "A":
                SoundSystem.Instance.PlaySound(new OXSound("A", 0.2f).Pitch(0.5f).Clipping());
                break;
            default:
                Debug.LogWarning("Failed to find a sound preset with the index of " + index);
                break;
        }
    }

    [HideInInspector]
    public bool foundendcall = false;
    [HideInInspector]
    public bool waitforinput = false;
    [HideInInspector]
    public bool attemptskip = false;
    [HideInInspector]
    public bool godlyattemptskip = false;
    [HideInInspector]
    public bool backwardskip = false;

    public static string CleanText(string a)
    {
        a = Regex.Replace(a, @"^[ \n]+", "");
        a = Regex.Replace(a, @"[ \n]+$", "");
        return a;
    }


    public bool ApplyAttribute(string key_input, string data_input, bool ignorewarning = false)
    {
        // attribute style to be applied by default for this file
        //   ^AttributeName
        // attribute style to be applied to any time the same speaker talks in this file going forward
        //   &AttributeName
        // variable to have it's value replaced
        //   *VariableName
        // language file system variable to have it's value replaced 
        //   !VariableName
        // pre-processor that runs on file load, gets replaced with the contents of the variable
        //   @<VariableName>
        foundendcall = false;
        List<string> slist = new List<string>();
        var key = VariableParse(key_input)[0];
        var data = VariableParse(data_input);
        bool is_default_set = false;
        bool is_named_set = false;
        bool succeeded = false;
        bool succeeded_defaulting = false;
        bool succeeded_named = false;
        string aaa = "";

        is_default_set = key.Contains("^");
        is_named_set = key.Contains("&");

        key = Regex.Replace(key, BadGex, "");
        for (int i = 0; i < data.Count; i++)
        {
            data[i] = Regex.Replace(data[i], BadGex, "");
        }

        switch (key)
        {
            case "br": //fallthrough case to make sure this works properly
            case "Text":
                // Used to display text inside dialog, pretty much always used in conjunction with dialog variables
                succeeded = true;
                break;
            case "CanSkip":
                // Allows skipping to the end of the dialog
                CanSkip = data[0] == "True";
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("CanSkip", CanSkip.ToString());
                    succeeded_defaulting = true;
                }
                break;
            case "CanSkipBack":
                // Allows skipping backward in dialog
                CanSkipBack = data[0] == "True";
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("CanSkipBack", CanSkip.ToString());
                    succeeded_defaulting = true;
                }
                break;
            case "Skip":
                // Forces a skip
                if (LoadingNextDialog)
                {
                    InstantShowAllText = true;
                }
                else
                {
                    attemptskip = true;
                    godlyattemptskip = true;
                }
                succeeded = true;
                break;
            case "SkipBack":
                // Forces a skip backward
                attemptskip = true;
                godlyattemptskip = true;
                backwardskip = true;
                succeeded = true;
                if (LoadingNextDialog)
                {
                    throw new Exception("Invalid use of this attribute lol");
                }
                break;
            case "AutoSkip":
                // Automatically jumps to the next dialog line when available after x seconds
                // set to -1 to disable
                AutoSkip = float.Parse(data[0]);
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("AutoSkip", AutoSkip.ToString());
                    succeeded_defaulting = true;
                }
                break;
            case "Jump":
                // jumps x amount of characters. Since this deletes itself after use, I dont think it will cause infinite loops on negative jumps
                // be careful tho
                CurrentCharacter += int.Parse(data[0]);
                succeeded = true;
                break;
            case "SoundOnType":
                // Choses a sound preset from PlaySoundPreset() to play when a new character is displayd
                PlaySoundOnType = data[0];
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("PlaySoundOnType", PlaySoundOnType.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("PlaySoundOnType", PlaySoundOnType.ToString());
                    succeeded_named = true;
                }
                break;
            case "Wait":
                // Waits x seconds before moving forward
                DelayTimer = float.Parse(data[0]);
                succeeded = true;
                break;
            case "PlaySound":
                // Plays a given sound from sound presets
                PlaySoundPreset(data[0]);
                succeeded = true;
                break;
            case "Escape":
                // Allows the early escaping of dialog events
                CanEscape = data[0] == "True";
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("CanEscape", CanEscape.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("CanEscape", CanEscape.ToString());
                    succeeded_named = true;
                }
                break;
            case "Name":
                // Changes the title of the dialog window
                speaker = data[0];
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("speaker", speaker.ToString());
                    succeeded_defaulting = true;
                }
                if (name_to_setting.ContainsKey(speaker))
                {
                    ParseFromSettings(name_to_setting[speaker]);
                }
                break;
            case "RichText":
                // Skips ahead in the text whenever a richtext is detected in the text
                RichTextEnabled = data[0] == "True";
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("RichTextEnabled", RichTextEnabled.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("RichTextEnabled", RichTextEnabled.ToString());
                    succeeded_named = true;
                }
                break;
            case "InstantText":
                // instantly shows all text in the current segment, skipping to the end.
                InstantShowAllText = data[0] == "True";
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("InstantShowAllText", InstantShowAllText.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("InstantShowAllText", InstantShowAllText.ToString());
                    succeeded_named = true;
                }
                if (!LoadingNextDialog)
                {
                    ApplyAttribute("Skip", "");
                }
                break;
            case "Speed":
                //data should be formatted like    5, 1, 1    (spaces optional)
                // Characters per second
                if (data.Count >= 1 && data[0] != "-") CharactersPerSecond = float.Parse(data[0]);
                // Delay in seconds between each word
                if (data.Count >= 2 && data[1] != "-") WordDelay = float.Parse(data[1]);
                // Delay in seconds between each line
                if (data.Count >= 3 && data[2] != "-") NewLineDelay = float.Parse(data[2]);
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("cps", CharactersPerSecond.ToString());
                    CurrentSettings.Set("cps2", WordDelay.ToString());
                    CurrentSettings.Set("cps3", NewLineDelay.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("cps", CharactersPerSecond.ToString());
                    Set("cps2", WordDelay.ToString());
                    Set("cps3", NewLineDelay.ToString());
                    succeeded_named = true;
                }
                break;
            case "PunctuationDelay":
                //data should be formatted like    5, 1, 1    (spaces optional)
                // Delay in seconds between each small thing like comma, colon, and semicolon
                if (data.Count >= 1 && data[0] != "-") MinorPunctuationDelay = float.Parse(data[0]);
                // Delay in seconds between each big thing like period, questionmark, and exclamation point
                if (data.Count >= 2 && data[1] != "-") MajorPunctuationDelay = float.Parse(data[1]);
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("pps", MinorPunctuationDelay.ToString());
                    CurrentSettings.Set("pps2", MajorPunctuationDelay.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("pps", MinorPunctuationDelay.ToString());
                    Set("pps2", MajorPunctuationDelay.ToString());
                    succeeded_named = true;
                }
                break;
            case "TitleColor":
                //4 input color formated like 255,255,255,255
                if (data.Count < 4) data.Add("255");
                aaa = data[0] + "|" + data[1] + "|" + data[2] + "|" + data[3];
                tit_color = aaa;
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("tit_color", tit_color.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("tit_color", tit_color.ToString());
                    succeeded_named = true;
                }
                break;
            case "TextColor":
                //4 input color formated like 255,255,255,255
                if (data.Count < 4) data.Add("255");
                aaa = data[0] + "|" + data[1] + "|" + data[2] + "|" + data[3];
                color = aaa;
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("color", color.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("color", color.ToString());
                    succeeded_named = true;
                }
                break;
            case "BgColor":
                //4 input color formated like 255,255,255,255
                if (data.Count < 4) data.Add("255");
                aaa = data[0] + "|" + data[1] + "|" + data[2] + "|" + data[3];
                bg_color = aaa;
                succeeded = true;
                if (is_default_set)
                {
                    CurrentSettings.Set("bg_color", bg_color.ToString());
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    Set("bg_color", bg_color.ToString());
                    succeeded_named = true;
                }
                break;
            case "ApplyStyle":
                // applies a selected name's stored attributes to the current speaker

                if (!name_to_setting.ContainsKey(speaker)) name_to_setting.Add(speaker, new DialogSettings());
                if (!name_to_setting.ContainsKey(data[0])) name_to_setting.Add(data[0], new DialogSettings());
                ParseFromSettings(name_to_setting[data[0]]);

                if (is_default_set)
                {
                    CurrentSettings.CurrentData = CurrentSettings.CurrentData.MergeDictionary(name_to_setting[data[0]].CurrentData);
                    succeeded_defaulting = true;
                }
                if (is_named_set)
                {
                    name_to_setting[speaker].CopyFrom(name_to_setting[data[0]]);
                    succeeded_named = true;
                }
                succeeded = true;
                break;
            case "ClearStyle":
                // Clears any lasting style applied to a given name

                if (name_to_setting.ContainsKey(data[0])) name_to_setting.Remove(data[0]);

                succeeded = true;
                break;
            case "ResetDefaults":
                // Resets all lasting defaults to their base values

                CurrentSettings = new DialogSettings(TrueDefaults);

                succeeded = true;
                break;
            case "Scene":
                //Starts a new dialog file
                StartDialog(data[0]);
                foundendcall = true;
                succeeded = true;
                break;
            case "Choose":
                //Starts a new choose file
                StartDialog(data[0], "Choose");
                foundendcall = true;
                succeeded = true;
                break;
            case "End":
                // Ends current dialog
                ResetDialog();
                pp.text = "";
                pp.title = "";
                pp.UpdateColor();
                foundendcall = true;
                succeeded = true;
                break;
            case "WaitForInput":
                //pauses the dialog until the skip/continue input is triggered
                waitforinput = true;
                succeeded = true;
                break;
            case "Shutdown":
                // closes the application
                Application.Quit();
                succeeded = true;
                break;
            case "Set":
                //sets a variable
                variables[data[0]] = data[1];
                succeeded = true;
                break;
            case "Animate":
                // Animates part of the text using animations from TextAnimator.cs
                // Animate=Text, Wave, 10
                var e = new Func<GameObject>(() =>
                {
                    switch (data[0])
                    {
                        case "Text": return pp.TextObject;
                        case "Title": return pp.TitleObject;
                        case "C1": return pp.q_gameobjects[0];
                        case "C2": return pp.q_gameobjects[1];
                        case "C3": return pp.q_gameobjects[2];
                        case "C4": return pp.q_gameobjects[3];
                        default: return pp.TextObject;
                    }
                })();
                var animat = e.GetComponent<TextAnimator>();
                string h = "";
                try
                {
                    h = data[2];
                }
                catch
                {

                }
                switch (h)
                {
                    case "Remove":
                        ta = null;
                        var s = data[1];
                        for (int i = 0; i < animat.anims.Count; i++)
                        {
                            if (animat.anims[i].Type == s)
                            {
                                animat.anims.RemoveAt(i);
                                i--;
                            }
                        }
                        break;
                    default:
                        var a = new TextAnim();
                        a.Type = data[1];
                        try
                        {
                            a.endindex = int.Parse(data[2]);
                        }
                        catch
                        {
                            a.endindex = 100000;
                        }
                        ta = a;
                        animat.anims.Add(a);
                        break;
                }
                succeeded = true;
                break;
            case "Event":
                GlobalEvent.Invoke(data[0]);
                succeeded = true;
                break;
            default:
                if (!ignorewarning) Debug.LogWarning("Unknown Dialog Attribute: \"" + key + "\"  (Dialog File: " + ActiveFileName + ")");
                break;
        }

        if (!succeeded_defaulting && is_default_set)
        {
            Debug.LogWarning("Invalid default assignment: \"" + key + "\"  (Dialog File: " + ActiveFileName + ")\n(this attribute can not be used to set a default value)");
        }
        if (!succeeded_named && is_named_set)
        {
            Debug.LogWarning("Invalid named default assignment: \"" + key + "\"  (Dialog File: " + ActiveFileName + ")\n(this attribute can not be used to set a name based default value)");
        }

        return succeeded;
    }


    public void SetVariable(string key, string val)
    {
        variables.AddOrUpdate(key, val);
    }
    public string GetVariable(string key, string defaultval = "(No Data)")
    {
        if (variables.ContainsKey(key))
        {
            return variables[key];
        }
        else
        {
            return defaultval;
        }
    }
    private List<string> VariableParse(string data)
    {
        try
        {

            //attribute variable format
            //*var_name

            //language file system query format
            //!var_name
            List<string> nerds = data.StringToList(",");
            List<string> outpi = new List<string>();
            foreach (var a in nerds)
            {
                string ba = CleanText(a);
                var dd = Regex.Match(ba, $"^{BadGex}+");
                var prepend = Regex.Replace(dd.Value, $"[!*]+", "");
                var realdata = Regex.Replace(ba, $"{BadGex}+", "");
                if (dd.Success && dd.Value.Contains("*"))
                {
                    var smegleton = VariableParse(variables[realdata]);
                    foreach (var b in smegleton) { outpi.Add(prepend + b); }
                }
                else if (dd.Success && dd.Value.Contains("!"))
                {
                    outpi.Add(prepend + LanguageFileSystem.Instance.GetString("unknown", realdata));
                }
                else
                {
                    outpi.Add(ba);
                }
            }
            return outpi;

        }
        catch (Exception e)
        {
            Console.LogError(e);
            return null;
        }
    }

    public bool UseEnding(string r)
    {
        //Debug.Log("Ending To Parse: "+ r);
        string h = "";
        bool didf = false;
        for (int i = 0; i < r.Length && !didf; i++)
        {
            if (r[i] == '<' && !didf)
            {
                h = r.Substring(i);
                var ind = h.IndexOf(">");
                var e = r.Substring(i + 1, ind - 1);
                int ind2 = e.IndexOf("=");
                if (ind2 > -1)
                {
                    //Debug.Log(e.Substring(0, ind2));
                    ApplyAttribute(e.Substring(0, ind2), e.Substring(ind2 + 1));
                    didf = foundendcall;
                }
                else
                {
                    ApplyAttribute(e, "");
                    didf = foundendcall;
                }

                i += ind + 1;
            }
        }

        return didf;
    }

    public void ParseFromSettings(DialogSettings Settings)
    {
        if (Settings.CurrentData.ContainsKey("cps")) CharactersPerSecond = float.Parse(Settings.Get("cps"));
        if (Settings.CurrentData.ContainsKey("cps2")) WordDelay = float.Parse(Settings.Get("cps2"));
        if (Settings.CurrentData.ContainsKey("cps3")) NewLineDelay = float.Parse(Settings.Get("cps3"));
        if (Settings.CurrentData.ContainsKey("pps")) MinorPunctuationDelay = float.Parse(Settings.Get("pps"));
        if (Settings.CurrentData.ContainsKey("pps2")) MajorPunctuationDelay = float.Parse(Settings.Get("pps2"));
        if (Settings.CurrentData.ContainsKey("AutoSkip")) AutoSkip = float.Parse(Settings.Get("AutoSkip"));
        if (Settings.CurrentData.ContainsKey("speaker")) speaker = Settings.Get("speaker");
        if (Settings.CurrentData.ContainsKey("color")) color = Settings.Get("color");
        if (Settings.CurrentData.ContainsKey("tit_color")) tit_color = Settings.Get("tit_color");
        if (Settings.CurrentData.ContainsKey("bg_color")) bg_color = Settings.Get("bg_color");
        if (Settings.CurrentData.ContainsKey("PlaySoundOnType")) PlaySoundOnType = Settings.Get("PlaySoundOnType");
        if (Settings.CurrentData.ContainsKey("RichTextEnabled")) RichTextEnabled = bool.Parse(Settings.Get("RichTextEnabled"));
        if (Settings.CurrentData.ContainsKey("CanSkip")) CanSkip = bool.Parse(Settings.Get("CanSkip"));
        if (Settings.CurrentData.ContainsKey("CanEscape")) CanEscape = bool.Parse(Settings.Get("CanEscape"));
        if (Settings.CurrentData.ContainsKey("CanSkipBack")) CanSkipBack = bool.Parse(Settings.Get("CanSkipBack"));
        if (Settings.CurrentData.ContainsKey("InstantShowAllText")) InstantShowAllText = bool.Parse(Settings.Get("InstantShowAllText"));
    }

    public void SetDefaultParams()
    {
        if (CurrentSettings == null) CurrentSettings = new DialogSettings(TrueDefaults);
        ParseFromSettings(CurrentSettings);
        EndBanna();
        PlaySoundOnType = "";
        if (pp != null)
        {
            int i = 0;
            while (i < pp.qs.Count)
            {
                pp.qs[i] = "";
                i++;
            }
        }
    }


    public void ResetDialog()
    {
        filename = "";
        fulltext = "?";
        CurrentCharacter = 1;
        CurrentLine = -2;
        MainTimer = 0;
        IsActive = false;
        datatype = "Dialog";
        waitforinput = false;
        InputManager.RemoveLockLevel("Dialog");
        EndBanna();
        SetDefaultParams();
    }
    public void StartDialog(string dialog, string datat = "Dialog")
    {
        StartDialogOverhead(dialog, datat);

        //just closes the OcksTools Console when opening any dialog.
        ConsoleLol.Instance.CloseConsole();

        switch (datat)
        {
            case "Dialog":
                str = GetFormattedFromFile(filename);
                StartDialogOverhead2();
                break;
            case "Choose":
                str = GetFormattedFromFile(filename, datat);
                StartDialogOverhead2();
                break;
        }
    }

    public void StartDialogFromCustomList(List<string> data)
    {
        //expects properly formatted list, same style as normal dialog file but with no "</>", also no spaces at the start of a new index.

        StartDialogOverhead(data[0]);
        data.RemoveAt(0);
        str = data;
        StartDialogOverhead2();
    }

    private void StartDialogOverhead(string dialog, string datat = "Dialog")
    {
        CurrentSettings = new DialogSettings(TrueDefaults);
        name_to_setting.Clear();
        ResetDialog();
        IsActive = true;
        DialogBoxObject.SetActive(true);
        filename = dialog;
        datatype = datat;
        // charl = -1;
        InputManager.AddLockLevel("Dialog");
        //just closes the OcksTools Console when opening any dialog.
        ConsoleLol.Instance.CloseConsole();

    }

    private void StartDialogOverhead2()
    {
        Console.Log(datatype + ": " + ActiveFileName);
        NextLine();
    }

    public List<string> GetFormattedFromFile(string filename, string datat = "Dialog")
    {
        List<string> str = null;

        string ppsex = "";


        if (GetUseLFS())
        {
            ppsex = LanguageFileSystem.Instance.GetString(LanguageFileIndexes[filename], "");
        }
        else
        {
            ppsex = LanguageFileIndexes[filename].GetDefaultData();
        }

        //pre-processing
        var smegglesnin = Regex.Matches(ppsex, @"@<.*?>").ToList().AListToBList((x) => x.Value).RemoveDuplicates();
        for (int i = 0; i < smegglesnin.Count; i++)
        {
            var dingsing = smegglesnin[i];
            var gar = dingsing.Substring(2);
            gar = gar.Substring(0, gar.Length - 1);
            var single = variables.GetValueOrDefault(gar, "");
            ppsex = Regex.Replace(ppsex, Regex.Escape(dingsing), single);
        }

        str = new List<string>(ppsex.Split("</>"));


        string d1 = str[0];
        ActiveFileName = d1.Split(Environment.NewLine)[0];
        if (datat != "Choose") str.RemoveAt(0);
        for (int i = 0; i < str.Count; i++)
        {
            if (str[i].Length > 0 && str[i][0] == ' ') str[i] = str[i].Substring(1);
        }
        return str;
    }


    public void FixedUpdate()
    {
        DialogBoxObject.SetActive(IsActive);
        UpdateClickThing();
    }
    public void UpdateClickThing()
    {
        if (IsActive)
        {
            if ((CurrentCharacter >= fulltext.Length && AutoSkip < 0) || waitforinput)
            {
                pp.clikctoskpo.text = "Click To Continue";
            }
            else
            {
                pp.clikctoskpo.text = "";
            }
        }
    }

    public void upt()
    {
        MainTimerDelay = 1 / CharactersPerSecond;
        pp.text = GetText();
        pp.title = speaker;
        pp.color = color;
        pp.tit_color = tit_color;
        pp.UpdateColor();
        pp.UpdateText();
    }
    private TextAnim ta;
    private bool waited = false;
    private string ParseCharInFulltext(string e, bool waitoverride = false)
    {
    ithoughtifartedbutishit:
        //Debug.Log(charl);
        if (RichTextEnabled && CurrentCharacter < fulltext.Length && CurrentCharacter >= 0 && e.Substring(CurrentCharacter, 1) == "<" && (DelayTimer <= 0 || waitoverride))
        {
            var h = e.Substring(CurrentCharacter);
            var ii = h.IndexOf('>');
            if (ii > -1)
            {
                var oldcharl = CurrentCharacter;
                CurrentCharacter += ii + 1;
                string emu = "";
                //custom attribute parser
                try
                {
                    var sh = e.Substring(oldcharl + 1, ii - 1);
                    string[] stuff = sh.Split('=');
                    var charlpreatt = CurrentCharacter;
                    if (waitoverride && VariableParse(stuff[0])[0] == "Wait")
                    {
                        fulltext = fulltext.Substring(0, oldcharl) + fulltext.Substring(charlpreatt);
                        var off = CurrentCharacter - charlpreatt;
                        CurrentCharacter = oldcharl + off;
                        e = fulltext;
                    }
                    else
                    {
                        bool jjj = stuff.Length > 1 && ApplyAttribute(stuff[0], stuff[1], true);
                        if (stuff.Length == 1) jjj = ApplyAttribute(stuff[0], "", true);
                        if (jjj)
                        {
                            string mid = "";
                            string voop = VariableParse(stuff[0])[0];
                            if (voop == "Text")
                            {
                                mid = VariableParse(stuff[1])[0];
                            }
                            else if (voop == "br")
                            {
                                mid = "\n";
                            }
                            if (oldcharl < fulltext.Length)
                            {
                                fulltext = fulltext.Substring(0, oldcharl) + mid + fulltext.Substring(charlpreatt);
                            }
                            else
                            {
                                fulltext = fulltext + mid;
                            }
                            var off = CurrentCharacter - charlpreatt;
                            CurrentCharacter = oldcharl + off;
                            e = fulltext;
                            if (voop == "Animate" && ta != null)
                            {
                                ta.startindex = CurrentCharacter;
                                ta.endindex = CurrentCharacter + ta.endindex;
                            }
                            emu = voop;
                        }
                    }

                }
                catch (Exception ezez)
                {
                    try
                    {
                        var sh = e.Substring(oldcharl + 1, ii - 1);
                        Debug.LogWarning($"Something went fucked trying to parse \"{sh}\"\n{ezez}");
                    }
                    catch
                    {
                        Debug.LogWarning($"Something went fucked trying to parse a dialog attribute\n{ezez}");
                    }
                }
                if (emu != "Wait")
                {
                    goto ithoughtifartedbutishit;
                }
                else
                {
                    waited = true;
                }
            }
            else
            {
                Debug.LogWarning("No '>' found, baka");
            }
        }
        return e;
    }

    public string GetText()
    {
        string e = fulltext;
        DelayTimer = -1;
        e = ParseCharInFulltext(e);
        e = e.Substring(0, Math.Clamp(CurrentCharacter, 0, fulltext.Length));
        if (e == fulltext)
        {
            CurrentCharacter = fulltext.Length;
        }
        return e;
    }



    public string GetLineFrom(string index, int line, string boner = "Dialog")
    {
        var str2 = new List<string>();
        if (GetUseLFS())
        {
            str2 = new List<string>(LanguageFileSystem.Instance.GetString(LanguageFileIndexes[index], "").Split("</>"));
        }
        else
        {
            str2 = new List<string>(LanguageFileIndexes[index].GetDefaultData().Split("</>"));
        }
        return str2[line];
    }


    [HideInInspector]
    public bool LoadingNextDialog = false;
    public void NextLine(bool wank = false)
    {
        if (filename != "")
        {
            switch (datatype)
            {
                case "Dialog":
                    LoadingNextDialog = true;
                    if (CurrentCharacter >= fulltext.Length || backwardskip)
                    {
                        EndBanna();
                        DelayTimer = 0;
                        if (wank) CurrentLine -= 3;
                        else CurrentLine += 3;
                        pp.TitleObject.GetComponent<TextAnimator>().anims.Clear();
                        pp.TextObject.GetComponent<TextAnimator>().anims.Clear();
                        CurrentCharacter = -1;
                        int ln = Math.Clamp(CurrentLine - 2, 0, str.Count);
                        string r = str[ln];
                        if (ln == 0 || !UseEnding(r))
                        {
                            string g = "";
                            try
                            {
                                g = str[CurrentLine - 1];
                            }
                            catch
                            {
                                ResetDialog();
                                return;
                            }
                            List<string> list23 = new List<string>(g.Split("<"));
                            fulltext = str[CurrentLine];
                            fulltext = Regex.Replace(fulltext, @"[ \n\r\t]+$", "");
                            SetDefaultParams();
                            foreach (var attribute in list23)
                            {
                                if (attribute.Contains(">"))
                                {
                                    string he = attribute.Substring(0, attribute.IndexOf(">"));
                                    List<string> he2 = new List<string>(he.Split("="));
                                    if (he2.Count > 1)
                                    {
                                        ApplyAttribute(he2[0], he2[1]);
                                    }
                                    else
                                    {
                                        ApplyAttribute(he2[0], "");
                                    }
                                }
                            }
                            if (InstantShowAllText)
                            {
                                NextLine();
                            }
                            MainTimerDelay = 1 / CharactersPerSecond;
                            pp.text = "";
                            pp.title = speaker;
                            pp.color = color;
                            pp.tit_color = tit_color;
                            pp.bg_color = bg_color;
                            if (!InstantShowAllText) pp.UpdateText(); // Why does this break InstantShowAllText?
                            pp.UpdateColor();
                            UpdateClickThing();
                        }
                    }
                    else
                    {
                        if (CanSkip)
                        {
                            for (int i3 = CurrentCharacter; i3 < fulltext.Length; i3++)
                            {
                                CurrentCharacter = i3;
                                DelayTimer = 0;
                                fulltext = ParseCharInFulltext(fulltext, true);
                            }
                            CurrentCharacter = fulltext.Length;
                            upt();
                        }
                    }
                    break;
                case "Choose":
                    LoadingNextDialog = true;
                    pp.q_gameobjects[0].GetComponent<TextAnimator>().anims.Clear();
                    pp.q_gameobjects[1].GetComponent<TextAnimator>().anims.Clear();
                    pp.q_gameobjects[2].GetComponent<TextAnimator>().anims.Clear();
                    pp.q_gameobjects[3].GetComponent<TextAnimator>().anims.Clear();
                    pp.TitleObject.GetComponent<TextAnimator>().anims.Clear();
                    pp.TextObject.GetComponent<TextAnimator>().anims.Clear();

                    List<string> list23a = new List<string>(str[1].Split("<"));

                    foreach (var attribute in list23a)
                    {
                        if (attribute.Contains(">"))
                        {
                            string he = attribute.Substring(0, attribute.IndexOf(">"));
                            if (he == "/") continue;
                            //Debug.Log(he);
                            List<string> he2 = new List<string>(he.Split("="));
                            if (he2.Count > 1)
                            {
                                ApplyAttribute(he2[0], he2[1]);
                            }
                            else
                            {
                                ApplyAttribute(he2[0], "");
                            }
                        }
                    }
                    str.RemoveAt(1);


                    string g2 = str[1];
                    List<string> list2 = new List<string>(g2.Split(Environment.NewLine));
                    list2.RemoveAt(0);

                    int i = 0;
                    foreach (var s in list2)
                    {
                        pp.qs[i] = s;
                        i++;
                    }
                    speaker = str[0];
                    fulltext = " ";
                    upt();
                    break;
            }
        }
        LoadingNextDialog = false;
    }
    public void PrevLine()
    {
        try
        {
            var ficl = str[CurrentLine - 1 - 3];
        }
        catch
        {
            return;
        }
        NextLine(true);
    }
    public void Choose(int index)
    {
        //deprecated function
        string g2 = str[2];
        List<string> list2 = new List<string>(g2.Split(Environment.NewLine));
        list2.RemoveAt(0);
        UseEnding(list2[index]);

    }

    public bool GetUseLFS()
    {
        if (!UseLanguageFileSystem) return false;
        return LanguageFileSystem.Instance != null;
    }

    public void Set(string name, string data)
    {
        if (!name_to_setting.ContainsKey(speaker)) name_to_setting.Add(speaker, new DialogSettings());
        name_to_setting[speaker].Set(name, data);
    }

}
[System.Serializable]
public class DialogHolder
{
    public string Name;
    public TextAsset File;
}

[System.Serializable]
public class DialogDefaults
{
    public float CharactersPerSecond = 26;
    public float WordDelay = 0;
    public float NewLineDelay = 0;
    public float MinorPunctuationDelay = 0.2f;
    public float MajorPunctuationDelay = 0.8f;
    public float AutoSkip = -1;
    public string speaker = "?";
    public string color = "255|255|255|255";
    public string tit_color = "255|255|255|255";
    public string bg_color = "84|144|84|255";
    public string PlaySoundOnType = "";
    public bool RichTextEnabled = true;
    public bool CanSkip = true;
    public bool CanEscape = false;
    public bool CanSkipback = false;
    public bool InstantShowAllText = false;

}
public class DialogSettings
{
    public Dictionary<string, string> CurrentData = new Dictionary<string, string>();
    public DialogSettings() { }
    public DialogSettings(DialogDefaults a) { SetFromDefaults(a); }
    public DialogSettings(DialogSettings a) { CopyFrom(a); }
    public void SetFromDefaults(DialogDefaults a)
    {
        CurrentData.Clear();
        CurrentData.Add("cps", a.CharactersPerSecond.ToString());
        CurrentData.Add("cps2", a.WordDelay.ToString());
        CurrentData.Add("cps3", a.NewLineDelay.ToString());
        CurrentData.Add("pps", a.MinorPunctuationDelay.ToString());
        CurrentData.Add("pps2", a.MajorPunctuationDelay.ToString());
        CurrentData.Add("AutoSkip", a.AutoSkip.ToString());
        CurrentData.Add("speaker", a.speaker.ToString());
        CurrentData.Add("color", a.color.ToString());
        CurrentData.Add("tit_color", a.tit_color.ToString());
        CurrentData.Add("bg_color", a.bg_color.ToString());
        CurrentData.Add("PlaySoundOnType", a.PlaySoundOnType.ToString());
        CurrentData.Add("RichTextEnabled", a.RichTextEnabled.ToString());
        CurrentData.Add("CanSkip", a.CanSkip.ToString());
        CurrentData.Add("CanEscape", a.CanEscape.ToString());
        CurrentData.Add("CanSkipBack", a.CanSkipback.ToString());
        CurrentData.Add("InstantShowAllText", a.InstantShowAllText.ToString());
    }
    public string Get(string data)
    {
        return CurrentData[data];
    }
    public void Set(string data, string data2)
    {
        CurrentData.AddOrUpdate(data, data2);
    }
    public void CopyFrom(DialogSettings a)
    {
        CurrentData.Clear();
        foreach (var b in a.CurrentData)
        {
            CurrentData.Add(b.Key, b.Value);
        }
    }
}