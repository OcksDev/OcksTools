using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Converter : MonoBehaviour
{
    public static int BoolToInt(bool a)
    {
        return a ? 1 : 0;
    }
    public static bool IntToBool(int a)
    {
        return a == 1;
    }

    public static string ListToString(List<string> eee, string split = ", ")
    {
        return String.Join(split, eee);
    }

    public static List<string> StringToList(string eee, string split = ", ")
    {
        return eee.Split(split).ToList();
    }

    public static string DictionaryToString(Dictionary<string, string> dic, string splitter = ", ", string splitter2 = "<K>")
    {
        List<string> list = new List<string>();
        foreach (var a in dic)
        {
            list.Add(a.Key + splitter2 + a.Value);
        }
        return ListToString(list, splitter);
    }
    public static Dictionary<string, string> StringToDictionary(string e, string splitter = ", ", string splitter2 = "<K>")
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

    public static Vector3Int StringToVector3Int(string e)
    {
        var s = StringToList(e.Substring(1, e.Length - 2));
        return new Vector3Int(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
    }

    public static Vector3 StringToVector3(string e)
    {
        var s = StringToList(e.Substring(1, e.Length - 2));
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
    }

    public static Vector2Int StringToVector2Int(string e)
    {
        var s = Converter.StringToList(e.Substring(1, e.Length - 2));
        return new Vector2Int(int.Parse(s[0]), int.Parse(s[1]));
    }

    public static Vector2 StringToVector2(string e)
    {
        var s = Converter.StringToList(e.Substring(1, e.Length - 2));
        return new Vector2(float.Parse(s[0]), float.Parse(s[1]));
    }

    public static string BoolArrayToString(bool[] arr)
    {
        string op = arr.Length + ":";
        List<string> chars = new List<string>(){
"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","!","*"
        };

        int rollover = 0;
        int f = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            switch (rollover)
            {
                case 0:
                    f += arr[i] ? 1 : 0;
                    break;
                case 1:
                    f += arr[i] ? 2 : 0;
                    break;
                case 2:
                    f += arr[i] ? 4 : 0;
                    break;
                case 3:
                    f += arr[i] ? 8 : 0;
                    break;
                case 4:
                    f += arr[i] ? 16 : 0;
                    break;
                case 5:
                    f += arr[i] ? 32 : 0;
                    rollover = -1;
                    op += chars[f];
                    f = 0;
                    break;
            }
            rollover++;
        }
        if (rollover != 0)
        {
            op += chars[f];
        }
        return op;
    }

    public static bool[] StringToBoolArray(string e)
    {
        bool[] arr = new bool[int.Parse(e.Substring(0, e.IndexOf(":")))];
        e = e.Substring(e.IndexOf(":") + 1);
        List<string> chars = new List<string>(){
"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","!","*"
        };

        int f = 0;
        for (int i = 0; i < e.Length; i++)
        {
            f = chars.IndexOf(e[i].ToString());
            if ((f - 32) >= 0)
            {
                f -= 32;
                arr[(i * 6) + 5] = true;
            }
            if ((f - 16) >= 0)
            {
                f -= 16;
                arr[(i * 6) + 4] = true;
            }
            if ((f - 8) >= 0)
            {
                f -= 8;
                arr[(i * 6) + 3] = true;
            }
            if ((f - 4) >= 0)
            {
                f -= 4;
                arr[(i * 6) + 2] = true;
            }
            if ((f - 2) >= 0)
            {
                f -= 2;
                arr[(i * 6) + 1] = true;
            }
            if ((f - 1) >= 0)
            {
                arr[(i * 6)] = true;
            }
        }
        return arr;
    }

    public static Color32 StringToColor(string hex, string fallback = "FFFFFF")
    {
        //color inputs should be in hex format
        try
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
        catch
        {
            hex = fallback;
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
    }

    public static Sprite Texture2DToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
    }
    public static string NumToRead(string number, int style = 0)
    {
        //converts a raw string of numbers into a much nicer format of your choosing
        /* style values:
         * default/0 - Shorthand form (50.00M, 2.00B, 5.00Qa)
         * 1 - Scientific form (5.00E4, 20.00E75)
         * 2 - Long Form (5,267,500,000.69) (not very speedy at large numbers)
         */

        char dot = '.';
        char comma = ',';
        char dash = '-';

        string n = number;
        string deci = "";
        if (number.Contains(dot))
        {
            deci = number.Substring(number.IndexOf(dot));
            number = number.Substring(0, number.IndexOf(dot));
        }
        string final = "";

        string boner = "";
        if (number.Contains(dash))
        {
            boner = dash.ToString();
            number = number.Substring(1);
        }
        if (number.Length > 3 || style == 3)
        {
            switch (style)
            {
                case 0:
                    //short form, black magic lies below
                    int baller = (number.Length - 1) / 3;
                    int bbbb = baller;
                    baller--;
                    int baller2 = baller;
                    baller2 /= 10;
                    int baller3 = baller2;
                    baller3 /= 10;
                    baller3 *= 10;
                    baller2 *= 10;
                    baller -= baller2;
                    baller2 /= 10;
                    baller2 -= baller3;
                    baller3 /= 10;
                    List<string> bingle = new List<string>()
                    {
                       "","K","M","B","T","Qa","Qn","Sx","Sp","Oc","No",
                    };
                    List<string> bingle2 = new List<string>()
                    {
                       "","De","Vt","Tg","Qt","Qg","St","Sg","Og","Nt",
                    };
                    List<string> bingle3 = new List<string>()
                    {
                        "","Ce"
                    };
                    if (baller2 > 0 || baller3 > 0)
                    {
                        bingle[1] = "U";
                        bingle[2] = "D";
                        bingle.RemoveAt(3);
                    }
                    else
                    {
                        baller++;
                    }
                    if (baller3 > 1)
                    {
                        bingle3[1] = bingle3[1] + baller3;

                        baller3 = 1;
                    }
                    final = bingle[baller] + bingle2[baller2] + bingle3[Math.Clamp(baller3, 0, 1)];
                    int g = bbbb * 3;
                    string n2 = number.Substring(number.Length - g, 2);
                    string n1 = number.Substring(0, number.Length - g);
                    n = boner + n1 + dot + n2 + final;
                    break;
                case 1:
                    // scientific form
                    string gamerrr = (number.Length - 1).ToString();
                    string n22 = number.Substring(1, 3);
                    string n11 = number.Substring(0, 1);
                    n = boner + n11 + dot + n22 + "E" + gamerrr;
                    break;
                case 2:
                    //long form, kinda slow at large numbers
                    string nmb = number;
                    if (nmb.Length % 3 != 0) nmb = "0" + nmb;
                    if (nmb.Length % 3 != 0) nmb = "0" + nmb;
                    if (nmb.Length % 3 != 0) nmb = "0" + nmb;

                    List<string> result = new List<string>(Regex.Split(nmb, @"(?<=\G.{3})", RegexOptions.Singleline));
                    result.RemoveAt(result.Count - 1);
                    nmb = Converter.ListToString(result, comma.ToString());
                    if (nmb[0] == '0') nmb = nmb.Substring(1);
                    if (nmb[0] == '0') nmb = nmb.Substring(1);

                    n = boner + nmb + deci;
                    break;
                case 3:
                    // roman numerals, not very fast but cool, cant do big numbers but thats a fault of roman numerals
                    string fina = "";
                    Dictionary<string, string> weewee = new Dictionary<string, string>
                    {
                        { "0", "" },
                        { "1", "a" },
                        { "2", "aa" },
                        { "3", "aaa" },
                        { "4", "ab" },
                        { "5", "b" },
                        { "6", "ba" },
                        { "7", "baa" },
                        { "8", "baaa" },
                        { "9", "ac" },
                    };
                    List<string> peenies = new List<string>() { "I", "V", "X", "L", "C", "D", "M" };

                    for (int i = 0; i < number.Length; i++)
                    {
                        var s = weewee[number[(number.Length - 1) - i].ToString()];
                        s = s.Replace("a", peenies[i * 2]);
                        if (s.Contains("b")) s = s.Replace("b", peenies[(i * 2) + 1]);
                        if (s.Contains("c")) s = s.Replace("c", peenies[(i * 2) + 2]);
                        fina = s + fina;
                    }
                    n = boner + fina;
                    break;
            }
        }

        return n;
    }

    public static string TimeToRead(System.Numerics.BigInteger ine, int type = 0)
    {
        //converts a time (in whole seconds) into a readable format
        //type changes the format type:
        // 0 - 5w 4d 3h 2m 1s
        // 1 - 5:4:3:2:1
        var g = ine;
        string outp = "";
        List<string> things;
        switch (type)
        {
            default:
                things = new List<string>()
                {
                    "s",
                    "m ",
                    "h ",
                    "d ",
                    "w ",
                };
                break;
            case 1:
                things = new List<string>()
                {
                    "",
                    ":",
                    ":",
                    ":",
                    ":",
                };
                break;
        }
        bool fall = false;
        var x = (g / 604800);
        if (x > 0 || fall)
        {
            fall = true;
            outp += ((type == 1 && x < 10) ? "0" : "") + x + things[4];
            g %= 604800;
        }
        x = (g / 86400);
        if (x > 0 || fall)
        {
            fall = true;
            outp += ((type == 1 && x < 10) ? "0" : "") + x + things[3];
            g %= 86400;
        }
        x = (g / 3600);
        if (x > 0 || fall)
        {
            fall = true;
            outp += ((type == 1 && x < 10) ? "0" : "") + x + things[2];
            g %= 3600;
        }
        x = (g / 60);
        if (x > 0 || fall)
        {
            fall = true;
            outp += ((type == 1 && x < 10) ? "0" : "") + x + things[1];
            g %= 60;
        }
        outp += ((type == 1 && g < 10) ? "0" : "") + g.ToString() + things[0];

        return outp;
    }

}
