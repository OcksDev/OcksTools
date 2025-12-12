using UnityEngine;

public static class OXClip
{
    public static void SetClipboard(this string str)
    {
        GUIUtility.systemCopyBuffer = str;
    }

    public static string GetClipboard()
    {
        return GUIUtility.systemCopyBuffer;
    }
}
