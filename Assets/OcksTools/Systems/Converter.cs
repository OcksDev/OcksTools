using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Converter
{

    public static Dictionary<string, Func<string, object>> ConversionMethods = new Dictionary<string, Func<string, object>>();


    // to use custom objects, mark a function which takes in a string as [ConversionMethod]
    public static A StringToObject<A>(this string data, string explicit_type = null)
    {
        string typename = explicit_type != null ? explicit_type : typeof(A).Name;
        if (!ConversionMethods.ContainsKey(typename))
        {
            switch (typename)
            {
                case "String":
                    return (A)(object)data;
                case "Single":
                    return (A)(object)float.Parse(data);
                case "Double":
                    return (A)(object)double.Parse(data);
                case "Byte":
                    return (A)(object)byte.Parse(data);
                case "Int32":
                    return (A)(object)int.Parse(data);
                case "Int64":
                    return (A)(object)long.Parse(data);
                case "Boolean":
                    return (A)(object)bool.Parse(data);
                case "Decimal":
                    return (A)(object)decimal.Parse(data);
                case "Vector2":
                    return (A)(object)data.StringToVector2();
                case "Vector2Int":
                    return (A)(object)data.StringToVector2Int();
                case "Vector3":
                    return (A)(object)data.StringToVector3();
                case "Vector3Int":
                    return (A)(object)data.StringToVector3Int();
                case "Quaternion":
                    return (A)(object)data.StringToQuaternion();
                case "Color":
                case "Color32":
                    return (A)(object)data.StringToColor();
                default:
                    throw new Exception($"No conversion created for type \"{typeof(A).Name}\"");
            }
        }
        return (A)ConversionMethods[typename](data);
    }

    public static List<string> AListToStringList<A>(this List<A> eee)
    {
        var a = new List<string>();
        foreach (var b in eee)
        {
            a.Add(b.ToString());
        }
        return a;
    }
    public static List<B> AListToBList_DownCast<A, B>(this List<A> eee) where A : B
    {
        var a = new List<B>();
        foreach (var b in eee)
        {
            a.Add((B)b);
        }
        return a;
    }
    public static List<B> AListToBList<A, B>(this List<A> eee, Func<A, B> convert)
    {
        var a = new List<B>();
        foreach (var b in eee)
        {
            a.Add(convert(b));
        }
        return a;
    }
    public static List<B> AListToBList_UpCast<A, B>(this List<A> eee) where B : A
    {
        var a = new List<B>();
        foreach (var b in eee)
        {
            a.Add((B)b);
        }
        return a;
    }
    public static List<A> StringListToAList<A>(this List<string> eee)
    {
        var a = new List<A>();
        string tA = typeof(A).Name;
        foreach (var b in eee)
        {
            var dd = b.StringToObject<A>(tA);
            a.Add(dd);
        }
        return a;
    }
    public static int BoolToInt(this bool a)
    {
        return a ? 1 : 0;
    }
    public static bool IntToBool(this int a)
    {
        return a == 1;
    }

    public static string ListToString<A>(this List<A> eee, string split = ", ")
    {
        return String.Join(split, eee);
    }

    public static List<string> StringToList(this string eee, string split = ", ")
    {
        return eee.Split(split).ToList();
    }

    public static Dictionary<string, string> ABDictionaryToStringDictionary<A, B>(this Dictionary<A, B> dic)
    {
        var t = new Dictionary<string, string>();
        foreach (var key in dic)
        {
            t.Add(key.Key.ToString(), key.Value.ToString());
        }
        return t;
    }

    public static Dictionary<C, D> ABDictionaryToCDDictionary<A, B, C, D>(this Dictionary<A, B> eee, Func<A, C> convertAC, Func<B, D> convertBD)
    {
        var d = new Dictionary<C, D>();
        foreach (var thing in eee)
        {
            d.Add(convertAC(thing.Key), convertBD(thing.Value));
        }
        return d;
    }
    public static Dictionary<C, D> ABDictionaryToCDDictionary<A, B, C, D>(this Dictionary<A, B> eee, Func<A, B, C> convertAC, Func<A, B, D> convertBD)
    {
        var d = new Dictionary<C, D>();
        foreach (var thing in eee)
        {
            d.Add(convertAC(thing.Key, thing.Value), convertBD(thing.Key, thing.Value));
        }
        return d;
    }


    public static Dictionary<A, B> StringDictionaryToABDictionary<A, B>(this Dictionary<string, string> dic)
    {
        var t = new Dictionary<A, B>();
        string tA = typeof(A).Name;
        string tB = typeof(B).Name;
        foreach (var key in dic)
        {
            t.Add(key.Key.StringToObject<A>(tA),
                  key.Value.StringToObject<B>(tB));
        }
        return t;
    }
    public static string DictionaryToRead<A, B>(this Dictionary<A, B> dic)
    {
        List<string> a = new List<string>();
        foreach (var b in dic)
        {
            a.Add("{ " + b.Key.ToString() + ", " + b.Value.ToString() + " }");
        }
        return ListToString(a, "\n");
    }

    public static string DictionaryToString<A, B>(this Dictionary<A, B> dic, string splitter = "<K>", string splitter2 = "<->")
    {
        List<string> list = new List<string>();
        foreach (var a in dic)
        {
            list.Add(a.Key + splitter2 + a.Value);
        }
        return ListToString(list, splitter);
    }
    public static Dictionary<string, string> StringToDictionary(this string e, string splitter = "<K>", string splitter2 = "<->")
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
                dic.AddOrUpdate(sseexx[0], sseexx[1]);
            }
            catch
            {
            }
        }
        return dic;
    }
    public static string EscapedListToString<A>(this List<A> eee, string split = ", ")
    {
        List<string> dupe = new List<string>();
        List<string> esc = new List<string>() { split };
        for (int i = 0; i < eee.Count; i++)
        {
            dupe.Add(EscapeString(eee[i].ToString(), esc));
        }
        return String.Join(split, dupe);
    }

    public static List<string> EscapedStringToList(this string eee, string split = ", ")
    {
        var dupe = eee.Split(split).ToList();
        List<string> esc = new List<string>() { split };
        for (int i = 0; i < dupe.Count; i++)
        {
            dupe[i] = UnescapeString(dupe[i], esc);
        }
        return dupe;
    }

    public static string EscapedDictionaryToString<A, B>(this Dictionary<A, B> dic, string splitter = "<K>", string splitter2 = "<->")
    {
        List<string> list = new List<string>();
        List<string> esc = new List<string>() { splitter, splitter2 };
        foreach (var a in dic)
        {
            list.Add(EscapeString(a.Key.ToString(), esc) + splitter2 + EscapeString(a.Value.ToString(), esc));
        }
        return ListToString(list, splitter);
    }
    public static Dictionary<string, string> EscapedStringToDictionary(this string e, string splitter = "<K>", string splitter2 = "<->")
    {
        var dic = new Dictionary<string, string>();
        List<string> esc = new List<string>() { splitter, splitter2 };
        var list = StringToList(e, splitter);
        foreach (var a in list)
        {
            try
            {
                int i = a.IndexOf(splitter2);
                List<string> sseexx = new List<string>()
                {
                    UnescapeString(a.Substring(0, i), esc),
                    UnescapeString(a.Substring(i + splitter2.Length), esc),
                };
                dic.AddOrUpdate(sseexx[0], sseexx[1]);
            }
            catch
            {
            }
        }
        return dic;
    }

    public static Vector3Int StringToVector3Int(this string e)
    {
        var s = StringToList(e.Substring(1, e.Length - 2));
        return new Vector3Int(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
    }

    public static Vector3 StringToVector3(this string e)
    {
        var s = StringToList(e.Substring(1, e.Length - 2));
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
    }

    public static Quaternion StringToQuaternion(this string e)
    {
        var s = StringToList(e.Substring(1, e.Length - 2));
        return new Quaternion(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
    }

    public static Vector2Int StringToVector2Int(this string e)
    {
        var s = Converter.StringToList(e.Substring(1, e.Length - 2));
        return new Vector2Int(int.Parse(s[0]), int.Parse(s[1]));
    }

    public static Vector2 StringToVector2(this string e)
    {
        var s = Converter.StringToList(e.Substring(1, e.Length - 2));
        return new Vector2(float.Parse(s[0]), float.Parse(s[1]));
    }

    public static string BoolArrayToString(this bool[] arr)
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

    public static bool[] StringToBoolArray(this string e)
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



    public static string ColorToString(this Color cc)
    {
        return ColorUtility.ToHtmlStringRGB(cc);
    }

    public static string ColorToString(this Color32 cc) // lol
    {
        return ColorUtility.ToHtmlStringRGB(cc);
    }

    public static Color32 StringToColor(this string hex, string fallback = "FFFFFF")
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

    public static Sprite Texture2DToSprite(this Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
    }
    public static string NumToRead(this string number, int style = 0)
    {
        //converts a raw string of numbers into a much nicer format of your choosing
        /* style values:
         * default/0 - Shorthand form (50.00M, 2.00B, 5.00Qa)
         * 1 - Scientific form (5.00E4, 20.00E75)
         * 2 - Long Form (5,267,500,000.69) (not very speedy at large numbers)
         * 3 - Roman Numerals
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
    public static Dictionary<T, int> ListToDictionary<T>(this List<T> input)
    {
        var output = new Dictionary<T, int>();
        foreach (var item in input)
        {
            if (output.ContainsKey(item))
            {
                output[item]++;
            }
            else
            {
                output.Add(item, 1);
            }
        }
        return output;
    }

    public static string TimeToRead(this System.Numerics.BigInteger ine, int type = 0)
    {
        //converts a time (in whole seconds) into a readable format
        //type changes the format type:
        // 0 - 5w 4d 3h 2m 1s
        // 1 - 05:04:03:02:01
        // 2 - 5432.1h
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
            case 2:
                ine /= 60;
                var dd = ((double)(ine % 60)) / 60.0;
                dd *= 10;
                dd = Math.Round(dd);
                var hs = ine / 60;
                return hs.ToString() + "." + ((int)dd).ToString() + "h";
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

    public static string EscapeString(this string e, List<string> thingstoremove)
    {
        e = e.Replace("(", "=(");
        e = e.Replace(")", "=)");
        for (int i = 0; i < thingstoremove.Count; i++)
        {
            e = e.Replace($"{thingstoremove[i]}", $"({i})");
        }
        return e;

    }
    public static string UnescapeString(this string e, List<string> thingstoremove)
    {
        for (int i = 0; i < thingstoremove.Count; i++)
        {
            int j = (thingstoremove.Count - 1) - i;
            e = e.Replace($"({j})", $"{thingstoremove[j]}");
        }
        e = e.Replace("=(", "(");
        e = e.Replace("=)", ")");
        return e;
    }


}

public class ConvertType<T> { }



public class ConversionMethod : Attribute
{

}

[System.Serializable]
public struct MultiRef<A, B>
{
    public A a;
    public B b;
    public MultiRef(A a, B b)
    {
        this.a = a;
        this.b = b;
    }
}

[System.Serializable]
public struct MultiRef<A, B, C>
{
    public A a;
    public B b;
    public C c;
    public MultiRef(A a, B b, C c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}
[System.Serializable]
public struct MultiRef<A, B, C, D>
{
    public A a;
    public B b;
    public C c;
    public D d;
    public MultiRef(A a, B b, C c, D d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
}

[System.Serializable]
public class SDictionary<A, B>
{
    public List<MultiRef<A, B>> List = new List<MultiRef<A, B>>();
    public Dictionary<A, B> Dict = new Dictionary<A, B>();
    public void Compile()
    {
        Dict.Clear();
        foreach (var a in List)
        {
            Dict.Add(a.a, a.b);
        }
    }
}