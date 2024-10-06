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
    public delegate void JustFuckingRunTheMethods();

    /* Welcome to Random Functions, your one stop shop of random functions
     * 
     * This is the hub of all the useful or widely used functions that i can't be bothered to write 50000 times,
     * so ya this place doesn't have much of a real function other than to store a bunch of other actually useful things.
     * 
     */



    //Default setup to make this a singleton
    public static RandomFunctions Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnApplicationQuit()
    {
        //save game or something idk man
    }

    public static void Close()
    {
        Application.Quit();
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
            /*Object ID, Tags*/ Tags.GenerateID(),
            /*Is Real (multiplayer object handling)*/ "false",
            /*Parent ID, Tags*/ "-",
        };
    }

    public static float SpreadCalc(int index, int max, float spread, bool fix = false)
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
    public static void SpreadCalcArc(int index, int max, float total_arc, int buffer = 2, bool fix = false)
    {
        //untested, should allow for slightly more complex arcs
        // should work the same as SpreadCalc(), except that it expands up to a point first
        buffer = Math.Clamp(buffer, 2, 1000000);
        float f = (total_arc * (buffer - 1));
        if(max > 1) f /= (max - 1);
        float spread = f;
        SpreadCalc(index, max, spread, fix);
    }

    public static string CharPrepend(string input, int length, char nerd = '0')
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

    public static Vector3 MousePositon(Camera cam)
    {
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }
    public static List<T> RemoveDuplicates<T>(List<T> tee)
    {
        var tea = new List<T>();
        foreach (T t in tee)
        {
            if (!tea.Contains(t)) tea.Add(t);
        }
        return tea;
    }
    public static List<T> CombineLists<T>(List<T> ti, List<T> tee)
    {
        var tea = new List<T>(ti);
        foreach (T t in tee)
        {
            tea.Add(t);
        }
        return tea;
    }
    public static bool ListContainsItemFromList<T>(List<T> ti, List<T> tee)
    {
        foreach (T t in ti)
        {
            if (tee.Contains(t)) return true;
        }
        return false;
    }
    public static bool ListMatchesList<T>(List<T> ti, List<T> tee)
    {
        if (ti.Count != tee.Count) return false;
        for (int t = 0; t < ti.Count; t++)
        {
            if (!ti[t].Equals(tee[t]))
            {
                return false;
            }
        }
        return true;
    }
    public static bool ListMatchesListOrderless<T>(List<T> ti, List<T> tee)
    {
        if (ti.Count != tee.Count) return false;
        var tea = new List<T>(ti);
        var teatea = new List<T>(tee);
        for (int t = 0; t < tea.Count;)
        {
            if (!teatea.Contains(tea[0])) return false;
            else
            {
                teatea.Remove(tea[0]);
                tea.RemoveAt(0);
            }
        }
        return true;
    }

    /*
     * Screen.currentResolution.refreshRate
     * Application.targetFrameRate = 60
     * FPS: (int)(1.0f / Time.smoothDeltaTime)
     */

    private Quaternion RotateTowards(Vector3 target, float max_angle_change)
    {
        var b = Quaternion.LookRotation((target - transform.position).normalized);
        return Quaternion.RotateTowards(transform.rotation, b, max_angle_change);
    }

    public static float EaseIn(float perc, float pow = 3)
    {
        return 1 - Mathf.Pow(1 - perc, pow);
    }
    public static float EaseOut(float perc, float pow = 3)
    {
        return Mathf.Pow(perc, pow);
    }
    public static float EaseInAndOut(float perc, float pow = 3)
    {
        //using values like 0.4 make it go fast at the start, slow down in the middle, then speed up again at the end
        if(perc <= 0.5f)
        {
            return Mathf.Pow(2*perc, pow)/2;
        }
        else
        {
            return (2-Mathf.Pow(2 * (1-perc), pow)) / 2;
        }
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
            default:
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
        }
        return cur_time;
    }


    public void DisconnectFromMatch()
    {
        //NetworkManager.Singleton.Shutdown();
    }
    public static float Dist(Vector3 p1, Vector3 p2)
    {
        float distance = Mathf.Sqrt(
                Mathf.Pow(p2.x - p1.x, 2f) +
                Mathf.Pow(p2.y - p1.y, 2f) +
                Mathf.Pow(p2.z - p1.z, 2f));
        return distance;
    }
    public static int Mod(int r, int max)
    {
        return ((r % max) + max) % max;
    }
    public static Quaternion PointAtPoint(Vector3 start_location, Vector3 location)
    {
        Quaternion _lookRotation =
            Quaternion.LookRotation((location - start_location).normalized);
        return _lookRotation;
    }
    public static Quaternion RotateLock(Quaternion start_rot, Quaternion target, float max_speed)
    {
        return Quaternion.RotateTowards(start_rot, target, max_speed);
    }

    private Quaternion Point2D(float offset2)
    {
        //returns the rotation required to make the current gameobject point at the mouse, untested in 3D.
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var sex = Quaternion.Euler(0f, 0f, rotation_z + offset2);
        return sex;
    }
    private Quaternion PointFromTo2D(Vector3 from_pos, Vector3 to_pos, float offset2)
    {
        //returns the rotation required to make the current gameobject point at the mouse, this method is 2D only.
        Vector3 difference = from_pos - to_pos;
        difference.Normalize();
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var sex = Quaternion.Euler(0f, 0f, rotation_z + offset2);
        return sex;
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
        List<string> dadalol = Converter.StringToList(data);
        List<string> hidden_dadalol = Converter.StringToList(hidden_data);
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
