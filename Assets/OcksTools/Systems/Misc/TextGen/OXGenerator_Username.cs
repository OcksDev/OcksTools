using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class OXGenerator_Username : MonoBehaviour
{
    public TextAsset Nouns;
    public TextAsset Names;
    public TextAsset Mods;
    public TextAsset Enders;
    [Button]
    public void GenerateTest()
    {
        Debug.Log(Pull(new GenProfile().MinLength(10)));
    }


    private GenProfile pp;
    private CapStyle ChosenCap = CapStyle.snake_case;
    private SurroundStyle SurrStyle = SurroundStyle.underscore_both;
    private List<string> Words;
    public string Pull(GenProfile pp)
    {
        this.pp = pp;

        ChosenCap = GetCap();
        SurrStyle = GetSurround();
        Words = GetMainWords();
        GetNumberEnder();


        for(int i = 0; i < Words.Count; i++)
        {
            switch (ChosenCap)
            {
                default:
                    Words[i] = Words[i].ToLower();
                        break;
                case CapStyle.SCREAMINGNODIFF:
                case CapStyle.SCREAMING_UNDERSCORE:
                case CapStyle.S_P_A_C_E_D:
                    Words[i] = Words[i].ToUpper();
                        break;
                case CapStyle.Pascel:
                case CapStyle.PascelSpace:
                    Words[i] = Words[i].Substring(0,1).ToUpper() +  Words[i].Substring(1).ToLower();
                        break;
            }
        }
        if (ChosenCap == CapStyle.Firstonly)
        {
            Words[0] = Words[0].Substring(0, 1).ToUpper() + Words[0].Substring(1).ToLower();
        }

        for (int i = 0; i < Words.Count; i++)
        {
            Words[i] = ChanceModWord(Words[i]);
        }




            string combiner = "";
        switch (ChosenCap)
        {
            case CapStyle.SCREAMING_UNDERSCORE:
            case CapStyle.snake_case: combiner = "_"; break;
        }

        string pre = "";
        string post = "";

        switch (SurrStyle)
        {
            case SurroundStyle.underscore_start: pre = "_"; break;
            case SurroundStyle.underscore_end: post = "_"; break;
            case SurroundStyle.underscore_both: post = "_"; pre = "_"; break;
            case SurroundStyle.xxxx: post = "xx"; pre = "xx"; break;
            case SurroundStyle.xXXx: post = "Xx"; pre = "xX"; break;
            case SurroundStyle.xX_Xx: post = "_Xx"; pre = "xX_"; break;
        }
        var main = pre + Converter.ListToString(Words, combiner) + post;



        if(main.Length < pp.min || main.Length > pp.max)
        {
            return Pull(pp);
        }


        return main;
    }

    public string ChanceModWord(string dd)
    {
        if (Random.Range(0f, 1f) >= 0.2f) return dd;
        List<string> vowel = new List<string>() { "a", "e", "i", "o", "u", "y" };
        for (int i = 0; i < dd.Length; i++)
        {
            /*if(Random.Range(0f,1f) <= 0.02f)
            {
                dd = dd.Substring(0, i) + dd.Substring(i + 1);
                i--;
            }*/
            if(Random.Range(0f,1f) <= 0.15f)
            {
                string aa = dd.Substring(i, 1).ToLower();

                if (aa == "o")
                {
                    dd = dd.Substring(0, i) + "0" + dd.Substring(i + 1);
                }
                else if(aa == "i")
                {
                    dd = dd.Substring(0, i) + "1" + dd.Substring(i + 1);
                }
                else if(aa == "e")
                {
                    dd = dd.Substring(0, i) + "3" + dd.Substring(i + 1);
                }
                else if(aa == "l")
                {
                    dd = dd.Substring(0, i) + "1" + dd.Substring(i + 1);
                }
                else if(aa == "s")
                {
                    dd = dd.Substring(0, i) + "5" + dd.Substring(i + 1);
                }
                else if(aa == "b")
                {
                    dd = dd.Substring(0, i) + "8" + dd.Substring(i + 1);
                }
            }
        }

        return dd;
    }



    public enum CapStyle
    {
        snake_case,           // hello_world
        snakenounderscore,    // helloworld
        Pascel,               // HelloWorld
        Firstonly,            // Helloworld
        SCREAMINGNODIFF,      // HELLOWORLD
        SCREAMING_UNDERSCORE, // HELLO_WORLD
        //spaces allowed:
        snake_space,          // hello world
        PascelSpace,          // Hello World
        s_p_a_c_e_d,          // h e l l o w o r l d
        S_P_A_C_E_D,          // H E L L O W O R L D
    }
    public enum SurroundStyle
    {
        none,              // hello
        underscore_start,  // _hello
        underscore_end,    // hello_
        underscore_both,   // _hello_
        xX_Xx,             // xX_hello_Xx
        xXXx,              // xXhelloXx
        xxxx,              // xxhelloxx
    }
    public enum NounStyle
    {
        noun,
        noun_noun,
        name_noun,
    }

    private List<string> GetMainWords()
    {
        var a = new WeightedAverage();
        a.Add(1f, 0); // noun
        a.Add(1f, 1); // name noun
        a.Add(1f, 2); // noun noun
        a.Add(1f, 3); // desc noun
        a.Add(1f, 4); // noun desc
        a.Add(0.5f, 5); // nounapend (draw from a list like [ito, ish, ily, ol, aito, er, ell, miz,um,arr] and merge vowels
        a.Add(0.5f, 6); // desc nounapend
        var dd = WeightedAverageHandler.DrawFromWeights<int>(a);

        List<string> words = new List<string>();

        switch (dd)
        {
            default: words = new List<string>() { "pass" }; break;
            case 0: words = new List<string>() { PullNoun() }; break;
            case 1: words = new List<string>() { PullName(), PullNoun() }; break;
            case 2: words = new List<string>() { PullNoun(), PullNoun() }; break;
            case 3: words = new List<string>() { PullMod(), PullNoun() }; break;
            case 4: words = new List<string>() { PullNoun(), PullMod() }; break;
            case 5: words = new List<string>() { PullNounEnd() }; break;
            case 6: words = new List<string>() { PullMod(), PullNounEnd() }; break;
        }

        if (Random.Range(0f, 1f) <= 0.05) words.Insert(0, PullNoun());
        if (Random.Range(0f, 1f) <= 0.05) words.Insert(0, PullMod());
        if (Random.Range(0f, 1f) <= 0.02) words.Add(PullNoun());


        if (Random.Range(0f, 1f) <= 0.1) words.Insert(0, "the");


        return words;
        //add 10% chance to prepend with "the"
    }


    private string PullNoun()
    {
        var a = Converter.StringToList(Nouns.text,System.Environment.NewLine);
        return a[Random.Range(0, a.Count)];
    }
    private string PullNounEnd()
    {
        var dd = PullNoun();
        var zz = PullEnder();
        List<string> vowel = new List<string>() {"a","e","i","o","u","y"};
        if (vowel.Contains(dd.Substring(dd.Length - 1).ToString().ToLower()))
        {
            while (vowel.Contains(dd.Substring(dd.Length - 1).ToString().ToLower()))
            {
                dd = dd.Substring(0, dd.Length - 1);
            }
        }

        return dd+zz;
    }
    private string PullName()
    {
        var a = Converter.StringToList(Names.text,System.Environment.NewLine);
        return a[Random.Range(0, a.Count)];
    }
    private string PullEnder()
    {
        var a = Converter.StringToList(Enders.text,System.Environment.NewLine);
        return a[Random.Range(0, a.Count)];
    }
    private string PullMod()
    {
        var a = Converter.StringToList(Mods.text,System.Environment.NewLine);
        return a[Random.Range(0, a.Count)];
    }
    private CapStyle GetCap()
    {
        var a = new WeightedAverage();
        a.Add(0.2f, CapStyle.snake_case);
        a.Add(1f, CapStyle.snakenounderscore);
        a.Add(1.75f, CapStyle.Firstonly);
        a.Add(3f, CapStyle.Pascel);
        a.Add(0.5f, CapStyle.SCREAMINGNODIFF);
        a.Add(0.2f, CapStyle.SCREAMING_UNDERSCORE);
        if (pp.allow_spaces)
        {
            a.Add(0.1f, CapStyle.s_p_a_c_e_d);
            a.Add(0.1f, CapStyle.s_p_a_c_e_d);
            a.Add(0.4f, CapStyle.PascelSpace);
            a.Add(1f, CapStyle.snake_space);
        }


        //small chance to l33t speak a given word?

        return WeightedAverageHandler.DrawFromWeights<CapStyle>(a);
    }
    private SurroundStyle GetSurround()
    {
        if (ChosenCap == CapStyle.s_p_a_c_e_d) return SurroundStyle.none;
        if (ChosenCap == CapStyle.PascelSpace) return SurroundStyle.none;
        if (ChosenCap == CapStyle.snake_space) return SurroundStyle.none;

        var a = new WeightedAverage();
        a.Add(7f, SurroundStyle.none);
        a.Add(0.1f, SurroundStyle.xXXx);
        a.Add(0.1f, SurroundStyle.xxxx);
        a.Add(0.1f, SurroundStyle.xX_Xx);
        a.Add(0.05f, SurroundStyle.underscore_start);
        a.Add(0.05f, SurroundStyle.underscore_end);
        a.Add(0.1f, SurroundStyle.underscore_both);

        return WeightedAverageHandler.DrawFromWeights<SurroundStyle>(a);
    }
    private void GetNumberEnder()
    {
        if (ChosenCap == CapStyle.PascelSpace) return;
        if (ChosenCap == CapStyle.snake_space) return;
        var a = new WeightedAverage();
        a.Add(1f, 0);
        a.Add(0.5f, 1);
        var dd = WeightedAverageHandler.DrawFromWeights<int>(a);
        if (dd == 0) return;

        var b = new WeightedAverage();
        b.Add(1f, Random.Range(0, 10));
        b.Add(1f, Random.Range(0, 100));
        b.Add(1f, Random.Range(0, 1000));
        b.Add(1f, Random.Range(0, 10000));
        b.Add(1f, Random.Range(System.DateTime.Now.Year-35, System.DateTime.Now.Year+1));

        Words.Add(WeightedAverageHandler.DrawFromWeights<int>(b).ToString());
    }


}

public class GenProfile
{
    public int min = 0;
    public int max = 999;
    public bool allow_naughty = false;
    public bool allow_spaces = false;
    //public bool allow_specialchars = false;
    public GenProfile MinLength(int m)
    {
        min = m; return this;
    }
    public GenProfile MaxLength(int m)
    {
        max = m; return this;
    }
    public GenProfile AllowNaughtyWords()
    {
        allow_naughty = true; return this;
    }
    public GenProfile AllowSpaces()
    {
        allow_spaces = true; return this;
    }
    /*public GenProfile AllowSpecialChars()
    {
        // default is a-z, A-Z, 0-9, _, -
        allow_specialchars = true; return this;
    }*/
} 

