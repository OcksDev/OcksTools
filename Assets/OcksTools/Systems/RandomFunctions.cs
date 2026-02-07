using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;



//using Unity.Netcode;
using UnityEngine;

public class RandomFunctions : SingleInstance<RandomFunctions>
{
    public delegate void JustFuckingRunTheMethods();

    /* Welcome to Random Functions, your one stop shop of random functions
     * 
     * This is the hub of all the useful or widely used functions that i can't be bothered to write 50000 times,
     * so ya this place doesn't have much of a real function other than to store a bunch of other actually useful things.
     * 
     */




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

    public static Dictionary<string, string> GenerateBlankHiddenData()
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
        if (max > 1) f /= (max - 1);
        float spread = f;
        SpreadCalc(index, max, spread, fix);
    }

    public static string CharPrepend(string input, int length, char nerd = '0')
    {
        var e = length - input.Length;
        if (e <= 0)
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
        if (perc <= 0.5f)
        {
            return Mathf.Pow(2 * perc, pow) / 2;
        }
        else
        {
            return (2 - Mathf.Pow(2 * (1 - perc), pow)) / 2;
        }
    }
    public static float EaseBounce(float perc, int bounces = 4, float pow = 5)
    {
        var a = perc * (bounces + 0.5f);
        var x = Mathf.Abs(Mathf.Cos(Mathf.PI * a));
        x /= Mathf.Pow(pow + 1, Mathf.Floor(a + 0.5f));
        return 1 - x;
    }
    public static float EaseOscillate(float perc, float quantity = 4, float pow = 1)
    {
        pow *= 5;
        var x = Mathf.Cos(Mathf.PI * perc * quantity);
        x *= 1 - perc;
        x /= (pow * perc) + 1;
        return 1 - x;
    }
    //pow shouldn't be less than 2
    public static float EaseOvershoot(float perc, float magnification = 2, float pow = 2)
    {
        var x = (1 - perc);
        x = Mathf.Pow(x, pow);
        var a = Mathf.Pow(magnification, pow);
        return x * a * perc * perc + (1 - x);

    }
    public static CompareState CompareTwoVersions(string I_Am, string compared_to)
    {
        //supports things in the format of v#.#.# or #.#.#
        // There can be any amount of #s, so "v1.2" is valid, so is "1.2.3.4.5",

        if (I_Am.Length < 1) return CompareState.Invalid;
        if (compared_to.Length < 1) return CompareState.Invalid;
        if (I_Am.ToLower()[0] == 'v') I_Am = I_Am.Substring(1);
        if (compared_to.ToLower()[0] == 'v') compared_to = compared_to.Substring(1);
        List<string> p = Converter.StringToList(I_Am, ".");
        List<string> p2 = Converter.StringToList(compared_to, ".");
        int amnt = System.Math.Max(p.Count, p2.Count);
        for (int i = 0; i < amnt; i++)
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
                try
                {
                    var a = Regex.Match(p[i], "^[0-9]*[a-zA-Z]+$");
                    var b = Regex.Match(p2[i], "^[0-9]*[a-zA-Z]+$");
                    var a2 = Regex.Match(p[i], "^[0-9]+");
                    var b2 = Regex.Match(p2[i], "^[0-9]+");
                    var a3 = Regex.Match(p[i], "[a-zA-Z]+$");
                    var b3 = Regex.Match(p2[i], "[a-zA-Z]+$");
                    if (a.Success || b.Success)
                    {
                        //return CompareState.Greater;
                        if (!a2.Success) return CompareState.Lesser;
                        if (!b2.Success) return CompareState.Greater;

                        if (int.Parse(a2.Value) < int.Parse(b2.Value))
                        {
                            return CompareState.Lesser;
                        }
                        else if (int.Parse(a2.Value) > int.Parse(b2.Value))
                        {
                            return CompareState.Greater;
                        }
                        int t1 = 0;
                        int t2 = 0;
                        int x = 1;
                        for (int j = 0; j < a3.Length; j++)
                        {
                            t1 += a3.Value[j] * x;
                            x *= 10;
                        }
                        x = 1;
                        for (int j = 0; j < b3.Length; j++)
                        {
                            t2 += b3.Value[j] * x;
                            x *= 10;
                        }
                        if (t1 < t2)
                        {
                            return CompareState.Lesser;
                        }
                        else if (t1 > t2)
                        {
                            return CompareState.Greater;
                        }
                    }
                    return CompareState.Invalid;
                }
                catch
                {
                    if (i == p.Count) return CompareState.Lesser;
                    if (i == p2.Count) return CompareState.Greater;
                    return CompareState.Invalid;
                }


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

#if UNITY_EDITOR
    public static T LoadResourceByPathEditor<T>(string assetPath) where T : ScriptableObject
    {
        // The path must be relative to the project folder and use forward slashes. 
        // Example path: "Assets/Data/PlayerData.asset"
        T scriptableObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if (scriptableObject == null)
        {
            UnityEngine.Debug.LogError($"Failed to load ScriptableObject at path: {assetPath}");
        }

        return scriptableObject;
    }
#endif


    //Why do you not specify the file type in the string path? 
    public static T LoadResourceByPathRuntime<T>(string assetPath) where T : ScriptableObject
    {
        // The path must be relative to the Resources folder, using forward slashes.
        // Example path: "Assets/Resources/Data/PlayerData.asset" -> "Data/PlayerData"
        T scriptableObject = Resources.Load<T>(assetPath);

        if (scriptableObject == null)
        {
            UnityEngine.Debug.LogError($"Failed to load ScriptableObject at path: {assetPath}");
        }

        return scriptableObject;
    }

    public static List<T> GetListOfInheritors<T>(params object[] constructorArgs) where T : class
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        }
        return objects;
    }

    public string CollapseSimilarString(string input)
    {
        string build = input.EscapeString(new List<string>() { "{" });
        var m = Regex.Matches(build, @"(?=.{9,}$)(.{3,}?)(?:\1){2,}").ToList();
        m.Reverse();
        foreach (var match in m)
        {
            var g = match.Groups.ToList()[1];
            string replace = $"{{{match.Value.Length / g.Value.Length},{g.Value.Length}}}" + g.Value;
            build = build.Substring(0, match.Index) + replace + build.Substring(match.Index + match.Value.Length);
        }
        return build;
    }

    public string ExpandSimilarString(string input)
    {
        var m = Regex.Matches(input, @"{(\d*),(\d*)}").ToList();
        m.Reverse();
        foreach (var match in m)
        {
            var gs = match.Groups.ToList();
            int x = int.Parse(gs[2].Value);
            string replace = input.Substring(match.Index + match.Value.Length, x);
            replace = string.Concat(Enumerable.Repeat(replace, int.Parse(gs[1].Value)));
            input = input.Substring(0, match.Index) + replace + input.Substring(match.Index + match.Value.Length + x);
        }
        input = input.UnescapeString(new List<string>() { "{" });
        return input;
    }



    //totally not a joke method
    public float Lerp01(float sex) => Mathf.Lerp(0, 1, sex);


}

public static class OXFunctions
{
    public static float TimeStablePow(this float expo)
    {
        expo = 1 - expo;
        return Mathf.Pow(expo, Time.deltaTime * 50 * Time.timeScale);
    }
    public static float TimeStableLerp(this float expo)
    {
        expo = 1 - expo;
        return 1 - Mathf.Pow(expo, Time.deltaTime * 50 * Time.timeScale);
    }

    public static Vector3 PerpendicularTowardDirection(this Vector3 baseVector, Vector3 directionVector)
    {
        Vector3 baseNorm = baseVector.normalized;

        Vector3 perpendicular = directionVector - Vector3.Dot(directionVector, baseNorm) * baseNorm;

        if (perpendicular.sqrMagnitude < 1e-6f)
        {
            perpendicular = Vector3.Cross(baseNorm, Vector3.up);

            if (perpendicular.sqrMagnitude < 1e-6f)
                perpendicular = Vector3.Cross(baseNorm, Vector3.right);
        }

        return perpendicular.normalized;
    }
    public static int Mod(this int r, int max) => ((r % max) + max) % max;
    public static float Mod(this float r, float max) => ((r % max) + max) % max;
    public static double Mod(this double r, double max) => ((r % max) + max) % max;
    public static long Mod(this long r, long max) => ((r % max) + max) % max;
    public static List<T> RemoveDuplicates<T>(this List<T> tee)
    {
        var tea = new List<T>();
        foreach (T t in tee)
        {
            if (!tea.Contains(t)) tea.Add(t);
        }
        return tea;
    }
    public static double Remap(this double value, double original_min, double original_max, double new_min, double new_max)
    {
        return (value - original_min) / (original_max - original_min) * (new_max - new_min) + new_min;
    }
    public static float Remap(this float value, float original_min, float original_max, float new_min, float new_max)
    {
        return (value - original_min) / (original_max - original_min) * (new_max - new_min) + new_min;
    }

    public static Dictionary<T, T2> MergeDictionary<T, T2>(this Dictionary<T, T2> ti, Dictionary<T, T2> tee)
    {
        var tea = new Dictionary<T, T2>(ti);
        foreach (var t in tee)
        {
            tea.AddOrUpdate(t);
        }
        return tea;
    }
    /// <summary>
    /// used for effecient data storage, only using the differences between two dictionaries,
    /// to be used in conjunction with MergeDictionary
    /// </summary>
    /// <param name="ti">base dictionary</param>
    /// <param name="tee">modified dictionary</param>
    /// <returns></returns>
    public static Dictionary<T, T2> DiffDictionary<T, T2>(this Dictionary<T, T2> ti, Dictionary<T, T2> tee)
    {
        var tea = MergeDictionary(ti, tee);
        var tii = new Dictionary<T, T2>();
        foreach (var t in tea)
        {
            if (t.Value == null) continue;
            if (!ti.ContainsKey(t.Key))
            {
                tii.Add(t.Key, t.Value);
                continue;
            }
            if (ti[t.Key] == null) continue;
            if (!t.Value.Equals(ti[t.Key]))
            {
                tii.Add(t.Key, t.Value);
            }
        }
        return tii;
    }
    public static void AddOrUpdate<T, T2>(this Dictionary<T, T2> ti, T K, T2 V)
    {
        if (ti.ContainsKey(K)) ti[K] = V;
        else ti.Add(K, V);
    }
    public static void AddOrUpdate<T, T2>(this Dictionary<T, T2> ti, KeyValuePair<T, T2> kv) => ti.AddOrUpdate(kv.Key, kv.Value);
    public static T2 GetOrDefine<T, T2>(this Dictionary<T, T2> ti, T inp, T2 def)
    {
        if (!ti.ContainsKey(inp))
        {
            ti.Add(inp, def);
            return def;
        }
        return ti[inp];
    }



    public static List<T> CombineLists<T>(this List<T> ti, List<T> tee)
    {
        var tea = new List<T>(ti);
        foreach (T t in tee)
        {
            tea.Add(t);
        }
        return tea;
    }
    public static bool ListContainsItemFromList<T>(this List<T> ti, List<T> tee)
    {
        foreach (T t in ti)
        {
            if (tee.Contains(t)) return true;
        }
        return false;
    }
    public static bool AllItemsFromListInList<T>(this List<T> ti, List<T> tee)
    {
        foreach (T t in ti)
        {
            if (!tee.Contains(t)) return false;
        }
        return true;
    }
    public static bool ListMatchesList<T>(this List<T> ti, List<T> tee)
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
    public static List<string> Clean(this List<string> ti)
    {
        for (int t = 0; t < ti.Count; t++)
        {
            if (ti[t] == null || ti[t].Length == 0 || Regex.IsMatch(ti[t], "^[\n \r\t]+$"))
            {
                ti.RemoveAt(t);
                t--;
            }
        }
        return ti;
    }
    public static List<T> Clean<T>(this List<T> ti)
    {
        for (int t = 0; t < ti.Count; t++)
        {
            if (ti[t] == null)
            {
                ti.RemoveAt(t);
                t--;
            }
        }
        return ti;
    }
    public static bool ListMatchesListOrderless<T>(this List<T> ti, List<T> tee)
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

    public static Vector2 GetActualSizeOfUI(this RectTransform re) =>
        //sizeDelta and Rect.width both dont get the actual size, somehow
        (re.rect.max - re.rect.min);

    public static Vector3 ReflectVector(this Vector3 incoming, Vector3 normal) => incoming - 2 * Vector3.Dot(incoming, normal) * normal;


    public static string GetCleanStackTraceRichtextified()
    {
        var a = GetCleanStackTrace().StringToList("\n");
        List<string> outp = new List<string>();
        foreach (var s in a)
        {
            // ConsoleLol.RecursiveCheck(OXCommand cum, Int32 lvl) : 428
            string nerd = s.Contains(".") ? $"<color=#34c25a>{s.Substring(0, s.IndexOf("."))}</color>." : s;
            string remain = Regex.Replace(s, @"^.*?\.", "");
            remain = Regex.Replace(remain, @"\(", $"<color=#298ed6>(<color=#6b8fdb>");
            remain = Regex.Replace(remain, @",", $"</color>,<color=#6b8fdb>");
            remain = Regex.Replace(remain, @"\)", $"</color>)</color>");
            remain = Regex.Replace(remain, @"\[", $"<color=#d68829>[");
            remain = Regex.Replace(remain, @"\]", $"]</color>");
            remain = Regex.Replace(remain, @" : ", $"<color=#d4d948> : ");
            outp.Add(nerd + remain + "</color>");
        }
        return outp.ListToString("\n");
    }

    public static string GetCleanStackTrace()
    {
        string stack = Environment.StackTrace;
        stack = Regex.Replace(stack, @".*GetCleanStackTrace.*", "");
        stack = Regex.Replace(stack, @".*get_StackTrace \(\).*", "");
        stack = Regex.Replace(stack, @" in .*?.cs", "");
        stack = Regex.Replace(stack, @" in <.*>", "");
        stack = Regex.Replace(stack, @" \[0x.*\]", "");
        stack = Regex.Replace(stack, @"\+<>.* \(", ".[LAMBDA] (");
        stack = Regex.Replace(stack, @"[\n\r]", "");
        stack = Regex.Replace(stack, @" at UnityEngine.*", "");
        stack = Regex.Replace(stack, @"^[ \n\r\t]+", "");
        stack = Regex.Replace(stack, @"  at ", "\n");
        stack = Regex.Replace(stack, @"^at ", "");
        stack = Regex.Replace(stack, @"`.*?\]", "");
        stack = Regex.Replace(stack, @"System\.", "");
        stack = Regex.Replace(stack, @"UnityEngine\.", "");
        stack = Regex.Replace(stack, @"\.ctor", "[CONSTRUCTOR]");
        stack = Regex.Replace(stack, @":", " : ");
        stack = Regex.Replace(stack, @" \(", "(");
        stack = Regex.Replace(stack, @" .*?,", ",");
        stack = Regex.Replace(stack, @"(?<!,) .*?\)", ")");
        return stack;
    }

    public static List<T> ShuffleList<T>(this List<T> ti)
    {
        System.Random rng = new System.Random();
        int n = ti.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (ti[k], ti[n]) = (ti[n], ti[k]);
        }
        return ti;
    }

    public static List<T> SetMinCount<T>(this List<T> ti, int desired)
    {
        while (ti.Count < desired)
        {
            ti.Add(default);
        }
        return ti;
    }

    public static Vector3 AllignZ(this Vector3 v, float z)
    {
        v.z = z;
        return v;
    }

    public static Vector3 AllignZ(this Vector3 v, Vector3 z)
    {
        v.z = z.z;
        return v;
    }

    public static Vector3 AllignZ(this Vector3 v, Transform z)
    {
        v.z = z.position.z;
        return v;
    }

    public static Vector3 AllignZ(this Vector3 v, Camera z)
    {
        v.z = z.transform.position.z;
        return v;
    }

    public static Vector3 AllignZ(this Vector3 v, MonoBehaviour z)
    {
        v.z = z.transform.position.z;
        return v;
    }

    public static Vector3 AllignY(this Vector3 v, float z)
    {
        v.y = z;
        return v;
    }

    public static Vector3 AllignY(this Vector3 v, Vector3 z)
    {
        v.y = z.y;
        return v;
    }

    public static Vector3 AllignY(this Vector3 v, Transform z)
    {
        v.y = z.position.y;
        return v;
    }

    public static Vector3 AllignY(this Vector3 v, Camera z)
    {
        v.y = z.transform.position.y;
        return v;
    }

    public static Vector3 AllignY(this Vector3 v, MonoBehaviour z)
    {
        v.y = z.transform.position.y;
        return v;
    }
    public static Vector3 AllignX(this Vector3 v, float z)
    {
        v.x = z;
        return v;
    }

    public static Vector3 AllignX(this Vector3 v, Vector3 z)
    {
        v.x = z.x;
        return v;
    }

    public static Vector3 AllignX(this Vector3 v, Transform z)
    {
        v.x = z.position.x;
        return v;
    }

    public static Vector3 AllignX(this Vector3 v, Camera z)
    {
        v.x = z.transform.position.x;
        return v;
    }

    public static Vector3 AllignX(this Vector3 v, MonoBehaviour z)
    {
        v.x = z.transform.position.x;
        return v;
    }
}
