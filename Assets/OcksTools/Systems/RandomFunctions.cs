using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;


//using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RandomFunctions : MonoBehaviour
{
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
    public GameObject SpawnParticle(GameObject particle, GameObject parent, Vector3 pos, Quaternion rot)
    {
        // this is a watered down version of SpawnObject which is specialized for particles
        var f = Instantiate(particle, pos, rot, parent.transform);

        var d = f.GetComponent<ParticleSystem>();
        if (d != null && !d.isPlaying)
        {
            d.Play();
        }
        return f;
    }

    public static Vector2 GetActualSizeOfUI(RectTransform re) 
    {
        //sizeDelta and Rect.width both dont get the actual size, somehow
        return (re.rect.max - re.rect.min);
    }

    public static Vector3 ReflectVector(Vector3 incoming, Vector3 axis)
    {
        return incoming - 2 * Vector3.Dot(incoming, axis) * axis;
    }
    public static Dictionary<string,string> GenerateBlankHiddenData()
    {
        return new Dictionary<string, string>()
        {
            /*Object ID, Tags*/ {"ID", Tags.GenerateID()},
            /*Is Real (multiplayer object handling)*/ {"IsReal", "false"},
            /*Parent ID, Tags*/ {"ParentID", "-"},
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
    public static bool AllItemsFromListInList<T>(List<T> ti, List<T> tee)
    {
        foreach (T t in ti)
        {
            if (!tee.Contains(t)) return false;
        }
        return true;
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

    public static void OpenURLInBrowser(string url)
    {
        var info = new ProcessStartInfo(url);
        info.UseShellExecute = true;
        Process.Start(info);
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
    public static float EaseBounce(float perc, int bounces = 4, float pow = 5)
    {
        var a = perc * (bounces+0.5f);
        var x = Mathf.Abs(Mathf.Cos(Mathf.PI * a));
        x /= Mathf.Pow(pow + 1, Mathf.Floor(a + 0.5f));
        return 1-x;
    }
    public static float EaseOvershoot(float perc, float quantity = 4, float pow = 1)
    {
        pow *= 5;
        var x = Mathf.Cos(Mathf.PI * perc * quantity);
        x *= 1 - perc;
        x /= (pow * perc) + 1;
        return 1-x;
    }

    public static CompareState CompareTwoVersions(string I_Am, string compared_to)
    {
        //supports things in the format of v#.#.# or #.#.#
        // There can be any amount of #s, so "v1.2" is valid, so is "1.2.3.4.5",

        if(I_Am.Length < 1) return CompareState.Invalid;
        if(compared_to.Length < 1) return CompareState.Invalid;
        if (I_Am.ToLower()[0]=='v') I_Am = I_Am.Substring(1);
        if (compared_to.ToLower()[0]=='v') compared_to = compared_to.Substring(1);
        List<string> p = Converter.StringToList(I_Am,".");
        List<string> p2 = Converter.StringToList(compared_to, ".");
        int amnt = System.Math.Max(p.Count, p2.Count);
        for(int i = 0; i < amnt; i++)
        {
            try
            {
                if (int.Parse(p[i]) < int.Parse(p2[i]))
                {
                    return CompareState.Lesser;
                }
                else if (int.Parse(p[i]) > int.Parse(p2[i]))
                {
                    return CompareState.Greater;
                }
            }
            catch
            {
                if(i==p.Count) return CompareState.Lesser;
                if(i==p2.Count) return CompareState.Greater;
                return CompareState.Invalid;
            }
        }

        return CompareState.Equal;

    }


    public enum CompareState
    {
        Greater,
        Equal,
        Lesser,
        Invalid,
    }

    public static double GetUnixTime(int type = -1)
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
        switch (type)
        {
            case 0:
                return ah.TotalMilliseconds;
            default:
            case 1:
                return ah.TotalSeconds;
            case 2:
                return ah.TotalMinutes;
            case 3:
                return ah.TotalHours;
            case 4:
                return ah.TotalDays;
        }
    }


    public void DisconnectFromMatch()
    {
        //NetworkManager.Singleton.Shutdown();
    }
    public static float Dist(Vector3 p1, Vector3 p2)
    {
        var x = p2.x - p1.x;
        var y = p2.y - p1.y;
        var z = p2.z - p1.z;
        return MathF.Sqrt((x * x) + (y * y) + (z * z));
    }
    public static float DistNoSQRT(Vector3 p1, Vector3 p2)
    {
        var x = p2.x - p1.x;
        var y = p2.y - p1.y;
        var z = p2.z - p1.z;
        return (x * x) + (y * y) + (z * z);
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
    public static Quaternion PointAtPoint2D(Vector3 from_pos, Vector3 to_pos, float offset2)
    {
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



}
