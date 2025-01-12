using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DialogLol : MonoBehaviour
{
    public GameObject DialogBoxObject;
    private DialogBoxL pp;
    public List<DialogHolder> DialogFiles = new List<DialogHolder>();
    public List<DialogHolder> ChooseFiles = new List<DialogHolder>();
    public Dictionary<string, DialogHolder> DialogFilesDict = new Dictionary<string, DialogHolder>();
    public Dictionary<string, DialogHolder> ChooseFilesDict = new Dictionary<string, DialogHolder>();
    public bool dialogmode = false;
    public string filename = "";
    public int linenum = 0;
    public int charnum = -1;
    public float cps = -1;
    public float cps2 = -1;
    public float cps3 = -1;
    private int charl = 0;
    private float cp2 = -1;
    private float cp = -1;
    private float cp3 = -1;
    public float AutoSkip = -1;
    public string speaker = "";
    public string fulltext = "";
    public string color = "";
    public string bg_color = "";
    public string tit_color = "";
    public string datatype = "Dialog";
    public bool RichTextEnabled = true;
    public bool CanSkip = true;
    public bool CanEscape = false;
    public string PlaySoundOnType = "";
    private List<string> str = new List<string>();
    private string ActiveFileName = "";
    private string baldcharacters = " \n\t";
    private Dictionary<string, string> variables = new Dictionary<string, string>();

    private static DialogLol instance;

    // Start is called before the first frame update
    public static DialogLol Instance
    {
        get { return instance; }

        //bug: you can use rich text like <br> and <i> in the console 
    }

    private void Awake()
    {
        if (Instance == null) instance = this;
        DialogBoxObject.SetActive(true);
        foreach(var a in DialogFiles)
        {
            DialogFilesDict.Add(a.Name, a);
        }
        foreach(var a in ChooseFiles)
        {
            ChooseFilesDict.Add(a.Name, a);
        }
    }


    void Start()
    {
        ResetDialog();
        pp = DialogBoxObject.GetComponent<DialogBoxL>();
        //lets you write <*=*Var> as shorthand to insert a variable into the dialog
        SetVariable("", "Show");
        //some testing variables for the dialog system
        SetVariable("TestVar", "*VarInSideAVar");
        SetVariable("VarInSideAVar", "Name");
        SetVariable("Wank", "Wank");
        SetVariable("AttributeInsideVar", "<Name=Bone Eater>");

    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.IsKeyDown("dialog_skip"))
        {
            if(datatype != "Choose")
            {
                attemptskip = true;
            }
        }
        if (InputManager.IsKeyDown("dialog_skip_back"))
        {
            if(datatype != "Choose")
            {
                backwardskip = true;
                attemptskip = true;
            }
        }
        if (attemptskip)
        {
            if (godlyattemptskip)
            {
                charl = fulltext.Length;
            }
            else
            {
                if(!CanSkip && charl < fulltext.Length)
                {
                    goto ex;
                }
            }
            godlyattemptskip = false;
            //Debug.Log("Skip registered");
            cp = 0;
            if (backwardskip)
            {
                PrevLine();
            }
            else
            {
                NextLine();
            }
        }
        ex:
        attemptskip = false;
        backwardskip = false;
        if (CanEscape && InputManager.IsKeyDown("close_menu"))
        {
            ResetDialog();
        }
        /* example of starting the dialog from a specific line
        if (InputManager.IsKeyDown(KeyCode.K))
        {
            StartDialogFromLine(0, 4);
        }*/

        if (cp3 <= 0)
        {
            cp -= Time.deltaTime;
            if (cp <= 0 && charl != fulltext.Length)
            {
                cp += cp2;
                if (cp < 0) charl += Math.Abs((int)(cp / cp2));
                if(!waited)charl += 1;
                waited = false;
                cp = cp2;
                upt();
                if (cp3 <= 0)
                {
                    string e = GetText();
                    if(e.Length > 0)
                    {
                        e = e.Substring(e.Length-1,1);
                    }
                    if (e == " " || e.Contains("\n"))
                    {
                        cp3 = e == " " ? cps2 : cps3;
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
            cp3 -= Time.deltaTime;
        }
        if(AutoSkip >= 0 && !isautoproc && charl >= fulltext.Length && datatype != "Choose")
        {
            banna = StartCoroutine(AutoSkipe());
        }
    }
    Coroutine banna;
    public IEnumerator AutoSkipe()
    {
        isautoproc = true;
        if(AutoSkip > 0) yield return new WaitForSeconds(AutoSkip);
        cp = 0;
        NextLine();
        isautoproc = false;
    }
    public void EndBanna()
    {
        isautoproc = false;
        if (banna != null) StopCoroutine(banna);
    }
    bool isautoproc = false;
    public void PlaySoundPreset(string index)
    {
        switch (index)
        {
            case "A":
                SoundSystem.Instance.PlaySoundWithClipping("A", false, 0.2f, 0.5f);
                break;
            default:
                Debug.LogWarning("Failed to find a sound preset with the index of " + index);
                break;
        }
    }

    public bool foundendcall = false;
    public bool attemptskip = false;
    public bool godlyattemptskip = false;
    public bool backwardskip = false;
    public bool ApplyAttribute(string key, string data, bool ignorewarning = false)
    {
        foundendcall = false;
        List<string> list = new List<string>();
        key = VariableParse(key);
        data = VariableParse(data);
        bool succeeded = false;
        string aaa = "";
        switch (key)
        {
            case "br": //fallthrough case to make sure this works properly
            case "Show":
                // Used to display text inside dialog, pretty much always used in conjucnction with dialog variables
                succeeded = true;
                break;
            case "CanSkip":
                // Allows skipping to the end of the dialog
                CanSkip = data == "True";
                succeeded = true;
                break;
            case "Skip":
                // Forces a skip
                attemptskip = true;
                godlyattemptskip = true;
                succeeded = true;
                break;
            case "SkipBack":
                // Forces a skip backward
                attemptskip = true;
                godlyattemptskip = true;
                backwardskip = true;
                succeeded = true;
                break;
            case "AutoSkip":
                // Automatically jumps to the next dialog line when available after x seconds
                AutoSkip = float.Parse(data);
                succeeded = true;
                break;
            case "Jump":
                // jumps x amount of characters. Since this deletes itself after use, I dont think it will cause infinite loops on negative jumps
                // be careful tho
                charl += int.Parse(data);
                succeeded = true;
                break;
            case "SoundOnType":
                // Choses a sound preset from PlaySoundPreset() to play when a new character is displayd
                PlaySoundOnType = data;
                succeeded = true;
                break;
            case "Wait":
                // Waits x seconds before moving forward
                cp3 = float.Parse(data);
                succeeded = true;
                break;
            case "PlaySound":
                // Waits x seconds before moving forward
                PlaySoundPreset(data);
                succeeded = true;
                break;
            case "Escape":
                // Allows the early escaping of dialog events
                CanEscape = data == "True";
                succeeded = true;
                break;
            case "Name":
                // Changes the title of the dialog window
                speaker = data;
                succeeded = true;
                break;
            case "RichText":
                // Skips ahead in the text whenever a richtext is detected in the text
                RichTextEnabled = data=="True";
                succeeded = true;
                break;
            case "Speed":
                //data should be formatted like    5, 1, 1
                list = new List<string>(data.Split(","));
                // Characters per second
                if (list.Count >= 1 && VariableParse(list[0]) != "-") cps = float.Parse(VariableParse(list[0]));
                // Delay in seconds between each word
                if (list.Count >= 2 && VariableParse(list[1]) != "-") cps2 = float.Parse(VariableParse(list[1]));
                // Delay in seconds between each line
                if (list.Count >= 3 && VariableParse(list[2]) != "-") cps3 = float.Parse(VariableParse(list[2]));
                succeeded = true;
                break;
            case "TitleColor":
                data = data.Replace(" ", "");
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = VariableParse(list[0]) + "|" + VariableParse(list[1]) + "|" + VariableParse(list[2]) + "|" + VariableParse(list[3]);
                tit_color = aaa;
                succeeded = true;
                break;
            case "TextColor":
                data = data.Replace(" ", "");
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = VariableParse(list[0]) + "|" + VariableParse(list[1]) + "|" + VariableParse(list[2]) + "|" + VariableParse(list[3]);
                color = aaa;
                succeeded = true;
                break;
            case "BgColor":
                data = data.Replace(" ", "");
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = VariableParse(list[0]) + "|" + VariableParse(list[1]) + "|" + VariableParse(list[2]) + "|" + VariableParse(list[3]);
                bg_color = aaa;
                succeeded = true;
                break;
            case "Scene":
                //Starts a new dialog file
                StartDialog(data);
                foundendcall = true;
                succeeded = true;
                break;
            case "Choose":
                //Starts a new choose file
                StartDialog(data, "Choose");
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
            case "Shutdown":
                // closes the application
                Application.Quit();
                succeeded = true;
                break;
            case "Set":
                data = data.Replace(" ", "");
                list = new List<string>(data.Split(","));
                //sets a variable
                variables[VariableParse(list[0])] = VariableParse(list[1]);
                succeeded = true;
                break;
            case "Animate":
                data = data.Replace(" ", "");
                // Waits x seconds before moving forward
                // Animate=Text, Wave, 10
                list = new List<string>(data.Split(","));
                var e = new Func<GameObject>(() => { 
                    switch (VariableParse(list[0]))
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
                    h = list[2];
                }
                catch
                {

                }
                switch (h)
                {
                    case "Remove":
                        ta = null;
                        var s = VariableParse(list[1]);
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
                        a.Type = VariableParse(list[1]);
                        try
                        {
                            a.endindex = int.Parse(VariableParse(list[2]));
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
            default:
                if(!ignorewarning)Debug.LogWarning("Unknown Dialog Attribute: \"" + key + "\"  (Dialog File: " + ActiveFileName + ")");
                break;
        }
        return succeeded;
    }
    public void SetVariable(string key, string val)
    {
        if (variables.ContainsKey(key))
        {
            variables[key] = val;
        }
        else
        {
            variables.Add(key, val);
        }
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
    private string VariableParse(string data)
    {
        //attribute variable format
        //*var_name
        if (data == "") return data;
        if (data[0] == ' ') data = data.Substring(1);
        if (data[0] == '*' || data[0] == '!')
        {
            string p2 = "";
            string newdat = "";
            p2 = data.Substring(1);
            try
            {
                if (data[0] == '*')
                {
                    newdat = variables[p2];
                }
                else
                {
                    newdat = LanguageFileSystem.Instance.GetString("unknown", p2);
                }
            }
            catch
            {
                return data;
            }
            newdat = VariableParse(newdat);
            data = newdat;
        }
        return data;
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
                var e = r.Substring(i + 1, ind-1);
                int ind2 = e.IndexOf("=");
                if(ind2 > -1)
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

                i += ind +1;
            }
        }

        return didf;
    }

    public void SetDefaultParams()
    {
        cps = 20;
        cps2 = 0;
        cps3 = 0;
        speaker = "?";
        color = "255|255|255|255";
        tit_color = "255|255|255|255";
        bg_color = "59|50|84|255";
        RichTextEnabled = true;
        CanSkip = true;
        CanEscape = false;
        AutoSkip = -1;
        EndBanna();
        PlaySoundOnType = "";
        if (pp != null)
        {
            int i = 0;
            while(i < pp.qs.Count)
            {
                pp.qs[i] = "";
                i++;
            }
        }
    }


    public void ResetDialog()
    {
        filename = "";
        charnum = 0;
        fulltext = "?";
        charl = 1;
        linenum= -2;
        cp = 0;
        dialogmode = false;
        datatype = "Dialog";
        EndBanna();
        SetDefaultParams();
    }
    public void StartDialog(string dialog, string datat = "Dialog")
    {
        ResetDialog();
        dialogmode = true;
        DialogBoxObject.SetActive(true);
        filename = dialog;
        datatype = datat;

        //just closes the OcksTools Console when opening any dialog.
        ConsoleLol.Instance.CloseConsole();

        switch (datat)
        {
            case "Dialog":
                str = GetFormattedFromFile(filename);
                ConsoleLol.Instance.ConsoleLog(datatype + ": " + ActiveFileName, "#bdbdbdff");
                NextLine();
                break;
            case "Choose":
                str = GetFormattedFromFile(filename, datat);
                ConsoleLol.Instance.ConsoleLog(datatype + ": " + ActiveFileName, "#bdbdbdff");
                NextLine();
                break;
        }
    }

    public List<string> GetFormattedFromFile(string filename, string datat = "Dialog")
    {
        var cd = DialogFilesDict;
        switch (datat)
        {
            case "Choose":
                cd = ChooseFilesDict;
                break;
        }
        var str = new List<string>(cd[filename].File.text.Split("</>"));
        string d1 = str[0];
        ActiveFileName = d1.Split(Environment.NewLine)[0];
        if(datat != "Choose")str.RemoveAt(0);
        for (int i = 0; i < str.Count; i++)
        {
            if (str[i].Length > 0 && str[i][0] == ' ') str[i] = str[i].Substring(1);
        }
        return str;
    }

    public void StartDialogFromLine(string dialog, int line)
    {
        // linenum/3 = line
        // lines start at 0 and go up from there jus like arrays
        
        ResetDialog();
        dialogmode = true;
        DialogBoxObject.SetActive(true);
        filename = dialog;
        datatype = "Dialog";

        //just closes the OcksTools Console when opening any dialog.
        ConsoleLol.Instance.CloseConsole();

        str = GetFormattedFromFile(filename);
        ConsoleLol.Instance.ConsoleLog(datatype + ": " + ActiveFileName, "#bdbdbdff");
        NextLine();
    }



    public void FixedUpdate()
    {
        DialogBoxObject.SetActive(dialogmode);
        if (dialogmode)
        {
            if(charl >= fulltext.Length && AutoSkip < 0)
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
        cp2 = 1 / cps;
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
        if (RichTextEnabled && charl < fulltext.Length && charl >= 0 &&e.Substring(charl, 1) == "<" && (cp3 <= 0 || waitoverride))
        {
            var h = e.Substring(charl);
            var ii = h.IndexOf('>');
            if (ii > -1)
            {
                var oldcharl = charl;
                charl += ii + 1;
                string emu = "";
                //custom attribute parser
                try
                {
                    var sh = e.Substring(oldcharl + 1, ii - 1);
                    string[] stuff = sh.Split('=');
                    var charlpreatt = charl;
                    if(waitoverride && VariableParse(stuff[0]) == "Wait")
                    {
                        fulltext = fulltext.Substring(0, oldcharl) + fulltext.Substring(charlpreatt);
                        var off = charl - charlpreatt;
                        charl = oldcharl + off;
                        e = fulltext;
                    }
                    else
                    {
                        bool jjj = stuff.Length >1 && ApplyAttribute(stuff[0], stuff[1], true);
                        if(stuff.Length == 1) jjj = ApplyAttribute(stuff[0], "", true);
                        if (jjj)
                        {
                            string mid = "";
                            string voop = VariableParse(stuff[0]);
                            if (voop == "Show")
                            {
                                mid = VariableParse(stuff[1]);
                            }
                            else if(voop == "br")
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
                            var off = charl - charlpreatt;
                            charl = oldcharl + off;
                            e = fulltext;
                            if (voop == "Animate" && ta != null)
                            {
                                ta.startindex = charl;
                                ta.endindex = charl + ta.endindex;
                            }
                            emu = voop;
                        }
                    }
                    
                }
                catch
                {
                    try
                    {
                        var sh = e.Substring(oldcharl + 1, ii - 1);
                        Debug.LogWarning($"Something went fucked trying to parse \"{sh}\"");
                    }
                    catch
                    {
                        Debug.LogWarning("Something went fucked trying to parse a dialog attribute");
                    }
                }
                if(emu != "Wait")
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
        cp3 = -1;
        e = ParseCharInFulltext(e);
        e = e.Substring(0, Math.Clamp(charl, 0, fulltext.Length));
        if (e == fulltext)
        {
            charl = fulltext.Length;
        }
        return e;
    }



    public string GetLineFrom(string index, int line, string boner = "Dialog")
    {
        var str = new List<string>();
        switch (boner)
        {
            case "Dialog":
                str = new List<string>(DialogFilesDict[index].File.text.Split("</> "));
                break;
            case "Choose":
                str = new List<string>(ChooseFilesDict[index].File.text.Split("</>"));
                break;
        }
        return str[line];
    }



    public void NextLine(bool wank = false)
    {
        if (filename != "")
        {
            switch (datatype)
            {
                case "Dialog":
                    if (charl >= fulltext.Length || backwardskip)
                    {
                        EndBanna();
                        cp3 = 0;
                        if (wank) linenum -= 3;
                        else linenum += 3;
                        pp.TitleObject.GetComponent<TextAnimator>().anims.Clear();
                        pp.TextObject.GetComponent<TextAnimator>().anims.Clear();
                        charl = -1;
                        int ln = Math.Clamp(linenum - 2, 0, str.Count);
                        string r = str[ln];
                        if (ln == 0 || !UseEnding(r))
                        {
                            string g = str[linenum - 1];
                            List<string> list = new List<string>(g.Split(", "));
                            List<string> list23 = new List<string>(g.Split("<"));
                            fulltext = str[linenum];
                            SetDefaultParams();
                            foreach (var attribute in list23)
                            {
                                if (attribute.Contains(">"))
                                {
                                    string he = attribute.Substring(0, attribute.IndexOf(">"));
                                    List<string> he2 = new List<string>(he.Split("="));
                                    ApplyAttribute(he2[0], he2[1]);
                                }
                            }
                            cp2 = 1 / cps;
                            pp.text = "";
                            pp.title = speaker;
                            pp.color = color;
                            pp.tit_color = tit_color;
                            pp.bg_color = bg_color;
                            pp.UpdateColor();
                        }
                    }
                    else
                    {
                        if (CanSkip)
                        {
                            for (int i3 = charl; i3 < fulltext.Length; i3++)
                            {
                                charl = i3;
                                cp3 = 0;
                                fulltext = ParseCharInFulltext(fulltext, true);
                            }
                            charl = fulltext.Length;
                            upt();
                        }
                    }
                    break;
                case "Choose":
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
                            List<string> he2 = new List<string>(he.Split("="));
                            ApplyAttribute(he2[0], he2[1]);
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
    }
    public void PrevLine()
    {
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
}
[System.Serializable]
public class DialogHolder
{
    public string Name;
    public TextAsset File; 
}