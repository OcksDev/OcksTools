using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.TestData.Definitions;
using UnityEngine;


public class OXGenerator
{
    public virtual string Pull(GenProfile pp)
    {
        return "";
    }
}

public class OXGenerator_Username : OXGenerator
{
    private GenProfile pp;
    private CapStyle ChosenCap = CapStyle.snake_case;
    private SurroundStyle SurrStyle = SurroundStyle.underscore_both;
    private List<string> Words;
    public override string Pull(GenProfile pp)
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
        return "";
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
        a.Add(3f, SurroundStyle.none);
        a.Add(0.1f, SurroundStyle.xXXx);
        a.Add(0.1f, SurroundStyle.xxxx);
        a.Add(0.1f, SurroundStyle.xX_Xx);
        a.Add(0.1f, SurroundStyle.underscore_start);
        a.Add(0.1f, SurroundStyle.underscore_end);
        a.Add(0.1f, SurroundStyle.underscore_both);

        return WeightedAverageHandler.DrawFromWeights<SurroundStyle>(a);
    }
    private List<string> GetMainWords()
    {
        var a = new WeightedAverage();
        a.Add(1f, 0); // noun
        a.Add(1.3f, 1); // name noun
        a.Add(1f, 2); // noun noun
        a.Add(1f, 3); // desc noun
        a.Add(1f, 4); // noun desc
        a.Add(1f, 5); // nounapend (draw from a list like [ito, ish, ily, ol, aito, er, ell, miz,um,arr] and merge vowels) 
        a.Add(1f, 6); // desc nounapend
        var dd = WeightedAverageHandler.DrawFromWeights<int>(a);
        switch (dd)
        {
            default: return null;
        }
        //add 10% chance to prepend with "the"
    }
    private void GetNumberEnder()
    {
        if (ChosenCap == CapStyle.PascelSpace) return;
        if (ChosenCap == CapStyle.snake_space) return;
        var a = new WeightedAverage();
        a.Add(0.5f, 0);
        a.Add(1f, 1);
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

