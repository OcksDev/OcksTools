using System.Collections;
using System.Collections.Generic;
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
