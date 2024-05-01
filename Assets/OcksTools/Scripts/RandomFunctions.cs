using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

//using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RandomFunctions : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] SpawnRefs = new GameObject[1];
    public GameObject[] ParticleSpawnRefs = new GameObject[1];
    [HideInInspector]
    public GameObject ParticleSpawnObject;
    public string ClientID;

    /* Welcome to Random Functions, your one stop shop of random functions
     * 
     * This is the hub of all the useful or widely used functions that i can't be bothered to qrite 50000 times,
     * so ya this place doesn't have much of a real function other than to store a bunch of other actually useful things.
     * 
     * Any function not marked as public is meant to be copied/pasted into a different script for usage
     */



    //Default setup to make this a singleton
    public static RandomFunctions Instance;
    private void Awake()
    {
        ClientID = GenerateID();
        if (Instance == null) Instance = this;
    }

    private void OnApplicationQuit()
    {
        //save game or something idk man
    }

    public void Close()
    {
        Application.Quit();
    }
    public string ListToString(List<string> eee, string split = ", ")
    {
        return String.Join(split, eee);
    }

    public List<string> StringToList(string eee, string split = ", ")
    {
        return eee.Split(split).ToList();
    }
    public GameObject SpawnParticle(int refe, GameObject parent, Vector3 pos, Quaternion rot)
    {
        // this is a watered down version of SpawnObject which is specialized for particles
        var f = Instantiate(ParticleSpawnRefs[refe], pos, rot, parent.transform);

        var d = f.GetComponent<ParticleSystem>();
        if (d != null && !d.isPlaying)
        {
            d.Play();
        }
        return f;
    }

    public List<string> GenerateBlankHiddenData()
    {
        return new List<string>()
        {
            /*Object ID, Tags*/ GenerateObjectID(),
            /*Is Real (multiplayer object handling)*/ "false",
            /*Parent ID, Tags*/ "-",
        };
    }

    public float SpreadCalc(int index, int max, float spread, bool fix = false)
    {
        // a spread calculation used to spread out objects over an angle
        int i = max;
        int j = index;
        float k = spread;
        k /= 2;
        float p = j * spread;
        p += fix ? k : -k;
        p -= i * spread / 2;
        return p;
    }
    public void SpreadCalcArc(int index, int max, float total_arc, int buffer = 2, bool fix = false)
    {
        //untested, should allow for slightly more complex arcs
        // should work the same as SpreadCalc(), except that it expands up to a point first
        buffer = Math.Clamp(buffer, 2, 1000000);
        float f = (total_arc * (buffer - 1));
        if(max > 1) f /= (max - 1);
        float spread = f;
        SpreadCalc(index, max, spread, fix);
    }

    public string GenerateObjectID()
    {
        //a more secrure method of making gameobject ids for the Tags system
        string e = GenerateID();
        e = e + Tags.dict.Count.ToString();
        return e;
    }
    public string CharPrepend(string input, int length, char nerd = '0')
    {
        var e = length - input.Length;
        if(e <= 0)
        {
            return input;
        }
        else
        {
            return new string(nerd, e) + input;
        }
    }

    public Vector3 MousePositon(Camera cam)
    {
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }
    public List<T> RemoveDuplicates<T>(List<T> ti)
    {
        var tea = new List<T>();
        foreach(T t in ti)
        {
            if(!tea.Contains(t)) tea.Add(t);
        }
        return tea;
    }

    public int ArrayWrap(int index, int length)
    {
        return Mod(index, length);
    }

    /*
     * Screen.currentResolution.refreshRate
     * Application.targetFrameRate = 60
     * FPS: (int)(1.0f / Time.smoothDeltaTime)
     */

    private Quaternion RotateTowards(GameObject target, float max_angle_change)
    {
        Vector3 a = target.transform.position;
        var b = Quaternion.LookRotation((a - transform.position).normalized);
        return Quaternion.RotateTowards(transform.rotation, b, max_angle_change);
    }

    public string NumToRead(string number, int style = 0)
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
                    nmb = ListToString(result, comma.ToString());
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
                    List<string> peenies = new List<string>() { "I" , "V" , "X" , "L", "C", "D" , "M"};

                    for(int i = 0; i < number.Length; i++)
                    {
                        var s = weewee[number[(number.Length-1)-i].ToString()];
                        s = s.Replace("a", peenies[i * 2]);
                        if (s.Contains("b")) s = s.Replace("b", peenies[(i * 2) + 1]);
                        if(s.Contains("c")) s = s.Replace("c", peenies[(i * 2) + 2]);
                        fina = s + fina;
                    }
                    n = boner + fina;
                    break;
            }
        }

        return n;
    }

    public long GetUnixTime(int type = -1)
    {
        //returns the curret unix time
        /* Type values:
         * default - seconds
         * 0 - miliseconds
         * 1 - seconds
         * 2 - minutes
         * 3 - hours
         * 4 - days
         */
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        var ah = (System.DateTime.UtcNow - epochStart);
        long cur_time = -1;
        switch (type)
        {
            case 0:
                cur_time = (long)ah.TotalMilliseconds;
                break;
            case 1:
                cur_time = (long)ah.TotalSeconds;
                break;
            case 2:
                cur_time = (long)ah.TotalMinutes;
                break;
            case 3:
                cur_time = (long)ah.TotalHours;
                break;
            case 4:
                cur_time = (long)ah.TotalDays;
                break;
            default:
                cur_time = (long)ah.TotalSeconds;
                break;
        }
        return cur_time;
    }


    public string TimeToRead(string ine, int type = 0)
    {
        //converts a time (in whole seconds) into a readable format
        //type changes the format type:
        // 0 - 5w 4d 3h 2m 1s
        // 1 - 5:4:3:2:1
        var g = System.Numerics.BigInteger.Parse(ine);
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
            outp += ((type==1 && x < 10)? "0":"") + x + things[4];
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

    public void DisconnectFromMatch()
    {
        //NetworkManager.Singleton.Shutdown();
    }
    public float Dist(Vector3 p1, Vector3 p2)
    {
        float distance = Mathf.Sqrt(
                Mathf.Pow(p2.x - p1.x, 2f) +
                Mathf.Pow(p2.y - p1.y, 2f) +
                Mathf.Pow(p2.z - p1.z, 2f));
        return distance;
    }
    public int Mod(int r, int max)
    {
        return ((r % max) + max) % max;
    }
    private Quaternion PointAtPoint(Vector3 start_location, Vector3 location)
    {
        Quaternion _lookRotation =
            Quaternion.LookRotation((location - start_location).normalized);
        return _lookRotation;
    }
    private Quaternion RotateLock(Quaternion start_rot, Quaternion target, float max_speed)
    {
        return Quaternion.RotateTowards(start_rot, target, max_speed);
    }

    private Quaternion Point2D(float offset2, float spread)
    {
        //returns the rotation required to make the current gameobject point at the mouse, untested in 3D.
        var offset = UnityEngine.Random.Range(-spread, spread);
        offset += offset2;
        //Debug.Log(offset);
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var sex = Quaternion.Euler(0f, 0f, rotation_z + offset);
        return sex;
    }

    private Quaternion PointAtPoint2D(Vector3 location, float spread)
    {
        // a different version of PointAtPoint with some extra shtuff
        //returns the rotation the gameobject requires to point at a specific location
        var offset = UnityEngine.Random.Range(-spread, spread);

        //Debug.Log(offset);
        Vector3 difference = location - transform.position;
        difference.Normalize();
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var sex = Quaternion.Euler(0f, 0f, rotation_z + offset);
        return sex;
    }

    public string BoolArrayToString(bool[] arr)
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

    public string DictionaryToString(Dictionary<string, string> dic, string splitter = ", ", string splitter2 = "<K>")
    {
        List<string> list = new List<string>();
        foreach (var a in dic)
        {
            list.Add(a.Key + splitter2 + a.Value);
        }
        return ListToString(list, splitter);
    }
    public Dictionary<string, string> StringToDictionary(string e, string splitter = ", ", string splitter2 = "<K>")
    {
        var dic = new Dictionary<string, string>();
        var list = StringToList(e, splitter);
        foreach (var a in list)
        {
            try
            {
                var sseexx = StringToList(a, splitter2);
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
    public bool[] StringToBoolArray(string e)
    {
        bool[] arr = new bool[int.Parse(e.Substring(0, e.IndexOf(":")))];
        e = e.Substring(e.IndexOf(":") + 1);
        List<string> chars = new List<string>(){
"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","!","*"
        };

        int rollover = 0;
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

    public string GenerateID()
    {
        //generates an id
        List<string> bpp = new List<string>()
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
        };

        string e = "";
        for (int i = 0; i < 15; i++)
        {
            e = e + bpp[UnityEngine.Random.Range(0, bpp.Count)];
        }
        return e;
    }


    //totally not a joke method
    public float Lerp01(float sex)
    {
        return Mathf.Lerp(0, 1, sex);
    }


    List<string> parentdata = new List<string>();
    [Obsolete("Use SpawnSystem.Instance.SpawnObject() instead.")]
    public GameObject SpawnObject(int refe, GameObject parent, Vector3 pos, Quaternion rot, bool SendToEveryone = false, string data = "", string hidden_data = "")
    {
        //object parenting over multiplayer is untested
        List<string> dadalol = StringToList(data);
        List<string> hidden_dadalol = StringToList(hidden_data);
        if (hidden_data == "")
        {
            hidden_dadalol = GenerateBlankHiddenData();
        }

        //object parenting using Tags, should work over multiplayer, untested
        if (hidden_dadalol[2] != "-" && Tags.dict.ContainsKey(hidden_dadalol[2]))
        {
            parent = Tags.dict[hidden_dadalol[2]];
        }
        if (Tags.gameobject_dict.ContainsKey(parent))
        {
            hidden_dadalol[2] = Tags.gameobject_dict[parent];
        }

        //incase you want to run some stuff here based on the object that is going to be spawned
        switch (refe)
        {
            case 0:
                break;
        }

        var f = Instantiate(SpawnRefs[refe], pos, rot, parent.transform);

        var d = f.GetComponent<SpawnData>();
        if (d != null)
        {
            //Requires objects to have SpawnData.cs to allow for data sending
            d.Data = dadalol;
            d.Hidden_Data = hidden_dadalol;
            d.IsReal = hidden_dadalol[1] == "true";
        }

        if (SendToEveryone)
        {
            // This code works, its just commented out by default because it requires Ocks Tools Multiplayer to be added
            //used for syncing the spawn of a local gameobject over the network instead of being a networked object

            //known issue: object parent is not preserved when spawning a local object over multiplayer


            //ServerGamer.Instance.SpawnObjectServerRpc(refe, pos, rot, ClientID, ListToString(dadalol), ListToString(hidden_dadalol));
        }
        return f;
    }


}
