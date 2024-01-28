using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class DialogLol : MonoBehaviour
{
    public GameObject DialogBoxObject;
    private DialogBoxL pp;
    public TextAsset[] files = new TextAsset[1];
    public TextAsset[] chooses = new TextAsset[1];
    public bool dialogmode = false;
    public int filenum = -1;
    public int linenum = 0;
    public int charnum = -1;
    public float cps = -1;
    public float cps2 = -1;
    public float cps3 = -1;
    private int charl = 0;
    private float cp2 = -1;
    private float cp = -1;
    private float cp3 = -1;
    public string speaker = "";
    public string fulltext = "";
    public string color = "";
    public string bg_color = "";
    public string tit_color = "";
    public string datatype = "Dialog";
    public bool RichTextEnabled = true;
    public bool CanSkip = true;
    public bool CanEscape = false;
    private List<string> str = new List<string>();
    private string ActiveFileName = "";
    private Dictionary<string, string> variables = new Dictionary<string, string>();

    public static DialogLol instance;

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

    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.IsKeyDown(InputManager.gamekeys["dialog_skip"]) || InputManager.IsKeyDown(InputManager.gamekeys["dialog_skip_mouse"]))
        {
            attemptskip = true;
        }
        if (attemptskip)
        {
            if (godlyattemptskip)
            {
                charl = fulltext.Length;
            }
            attemptskip = false;
            godlyattemptskip = false;
            //Debug.Log("Skip registered");
            cp = 0;
            NextLine();
        }
        if (CanEscape && InputManager.IsKeyDown(InputManager.gamekeys["close_menu"]))
        {
            ResetDialog();
        }

        if (cp3 <= 0)
        {
            cp -= Time.deltaTime;
            if (cp <= 0 && charl != fulltext.Length)
            {
                cp += cp2;
                if (cp < 0) charl += Math.Abs((int)(cp / cp2));
                charl += 1;
                cp = cp2;
                upt();
                if (cp3 <= 0)
                {
                    string e = GetText();
                    e = e.Substring(Math.Clamp(e.Length - 1, 0, e.Length));
                    if (e == " " || e.Contains("\n"))
                    {
                        cp3 = e == " " ? cps2 : cps3;
                    }
                }
            }
        }
        else
        {
            cp3 -= Time.deltaTime;
        }
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
    public bool foundendcall = false;
    public bool attemptskip = false;
    public bool godlyattemptskip = false;
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
            case "Jump":
                // jumps x amount of characters. Since this deletes itself after use, I dont think it will cause infinite loops on negative jumps
                // be careful tho
                charl += int.Parse(data);
                succeeded = true;
                break;
            case "Wait":
                // Waits x seconds before moving forward
                cp3 = float.Parse(data);
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
                list = new List<string>(data.Split(", "));
                // Characters per second
                if (list.Count > 1 && list[0] != "-") cps = float.Parse(list[0]);
                // Delay in seconds between each word
                if (list.Count > 2 && list[1] != "-") cps2 = float.Parse(list[1]);
                // Delay in seconds between each line
                if (list.Count > 3 && list[2] != "-") cps3 = float.Parse(list[2]);
                succeeded = true;
                break;
            case "TitleColor":
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = list[0] + "|" + list[1] + "|" + list[2] + "|" + list[3];
                tit_color= aaa;
                succeeded = true;
                break;
            case "TextColor":
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = list[0] + "|" + list[1] + "|" + list[2] + "|" + list[3];
                color = aaa;
                succeeded = true;
                break;
            case "BgColor":
                list = new List<string>(data.Split(","));
                //4 input color formated like 255,255,255,255
                aaa = list[0] + "|" + list[1] + "|" + list[2] + "|" + list[3];
                bg_color = aaa;
                succeeded = true;
                break;
            case "Scene":
                //Starts a new dialog file
                StartDialog(int.Parse(data));
                foundendcall = true;
                succeeded = true;
                break;
            case "Choose":
                //Starts a new choose file
                StartDialog(int.Parse(data), "Choose");
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
            default:
                if(!ignorewarning)Debug.LogWarning("Unknown Dialog Attribute: \"" + key + "\"  (Dialog File: " + ActiveFileName + ")");
                break;
        }
        return succeeded;
    }
    private string VariableParse(string data)
    {
        //attribute variable format
        //*var_name
        if (data.Contains("*"))
        {
            string p2 = data.Substring(data.IndexOf("*") + 1);
            var newdat = GetVariable(p2);
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
        filenum = -1;
        charnum = 0;
        fulltext = "?";
        charl = 1;
        linenum= -2;
        cp = 0;
        dialogmode = false;
        datatype = "Dialog";
        SetDefaultParams();
    }
    public void StartDialog(int dialog, string datat = "Dialog")
    {
        ResetDialog();
        dialogmode = true;
        DialogBoxObject.SetActive(true);
        filenum = dialog;
        datatype= datat;

        //just closes the OcksTools Console when opening any dialog.
        ConsoleLol.Instance.CloseConsole();

        switch (datat)
        {
            case "Dialog":
                str = new List<string>(files[filenum].text.Split("</> "));
                string d1 = str[0];
                str.RemoveAt(0);
                ActiveFileName= d1.Split(Environment.NewLine)[0];
                ConsoleLol.Instance.ConsoleLog(datatype + ": " + ActiveFileName, "#bdbdbdff");
                NextLine();
                break;
            case "Choose":
                str = new List<string>(chooses[filenum].text.Split("</>"));
                string d2 = str[0];
                ActiveFileName = d2.Split(Environment.NewLine)[0];
                ConsoleLol.Instance.ConsoleLog(datatype + ": " + ActiveFileName, "#bdbdbdff");
                NextLine();
                break;
        }
    }



    public void FixedUpdate()
    {
        DialogBoxObject.SetActive(dialogmode);
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


    private string ParseCharInFulltext(string e)
    {
    ithoughtifartedbutishit:
        //Debug.Log(charl);
        if (RichTextEnabled && charl < fulltext.Length && e.Substring(charl, 1) == "<")
        {
            var h = e.Substring(charl);
            var ii = h.IndexOf('>');
            if (ii > -1)
            {
                var oldcharl = charl;
                charl += ii + 1;

                //custom attribute parser
                try
                {
                    var sh = e.Substring(oldcharl + 1, ii - 1);
                    string[] stuff = sh.Split('=');
                    var charlpreatt = charl;
                    if (stuff.Length > 1 && ApplyAttribute(stuff[0], stuff[1], true))
                    {
                        string mid = "";
                        if (VariableParse(stuff[0]) == "Show")
                        {
                            mid = VariableParse(stuff[1]);
                        }
                        fulltext = fulltext.Substring(0, oldcharl) + mid + fulltext.Substring(charlpreatt);
                        var off = charl - charlpreatt;
                        charl = oldcharl + off;
                        e = fulltext;
                    }
                }
                catch
                {
                    Debug.LogWarning("Something went fucked trying to parse a dialog attribute");
                }
                goto ithoughtifartedbutishit;
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
        e = ParseCharInFulltext(e);
        e = e.Substring(0, Math.Clamp(charl, 0, fulltext.Length));
        if (e == fulltext)
        {
            charl = fulltext.Length;
        }
        return e;
    }



    public string GetLineFrom(int index, int line, string boner = "Dialog")
    {
        var str = new List<string>();
        switch (boner)
        {
            case "Dialog":
                str = new List<string>(files[index].text.Split("</> "));
                break;
            case "Choose":
                str = new List<string>(chooses[index].text.Split("</>"));
                break;
        }
        return str[line];
    }



    public void NextLine()
    {
        if (filenum != -1)
        {
            switch (datatype)
            {
                case "Dialog":
                    if (charl >= fulltext.Length)
                    {
                        linenum += 3;
                        charl = -1;
                        int ln = Math.Clamp(linenum-2, 0, str.Count);
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
                                fulltext = ParseCharInFulltext(fulltext);
                            }
                            charl = fulltext.Length;
                            upt();
                        }
                    }
                    break;
                case "Choose":
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

    public void Choose(int index)
    {
        //deprecated function
        string g2 = str[2];
        List<string> list2 = new List<string>(g2.Split(Environment.NewLine));
        list2.RemoveAt(0);
        UseEnding(list2[index]);
    }
}
