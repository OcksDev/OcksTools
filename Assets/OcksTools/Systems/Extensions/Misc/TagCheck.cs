using System.Collections.Generic;
using UnityEngine;

public class TagCheck
{
    protected static Dictionary<string, List<string>> TagChecks = new();
    public static void Add(string tag, List<string> a) => TagChecks.AddOrUpdate(tag, a);

    public static bool PassesTagCheck(GameObject a, string tag)
    {
        var p = TagChecks.GetValueOrDefault(tag, null);
        foreach (var b in p)
        {
            if (a.tag == b) return false;
        }
        return true;
    }
    public static bool PassesTagCheck(Collider a, string tag) => PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(Collider2D a, string tag) => PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(Collision a, string tag) => PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(Collision2D a, string tag) => PassesTagCheck(a.gameObject, tag);

}

public static class _TagCheckExtensions
{
    public static bool PassesTagCheck(this GameObject a, string tag) => TagCheck.PassesTagCheck(a, tag);
    public static bool PassesTagCheck(this Collider a, string tag) => TagCheck.PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(this Collider2D a, string tag) => TagCheck.PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(this Collision a, string tag) => TagCheck.PassesTagCheck(a.gameObject, tag);
    public static bool PassesTagCheck(this Collision2D a, string tag) => TagCheck.PassesTagCheck(a.gameObject, tag);
}