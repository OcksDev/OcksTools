using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static List<string> locklevel = new List<string>();
    public static Dictionary<KeyCode, string> keynames = new Dictionary<KeyCode, string>();
    public static Dictionary<string, KeyCode> namekeys = new Dictionary<string, KeyCode>();
    public static Dictionary<string, List<KeyCode>> gamekeys = new Dictionary<string, List<KeyCode>>();
    public static Dictionary<string, List<KeyCode>> defaultgamekeys = new Dictionary<string, List<KeyCode>>();
    public static Dictionary<string, string> gamekeynames = new Dictionary<string, string>();
    public static OXEvent CollectInputAllocs = new OXEvent();
    // Start is called before the first frame update
    void Start()
    {
        AssembleTheCodes();
        ResetLockLevel();
    }

    public static void AssembleTheCodes()
    {
        namekeys.Clear();
        gamekeys.Clear();
        defaultgamekeys.Clear();
        gamekeynames.Clear();

        SetGameKeys();

        //namekeys and keynames are both dictionaries
        foreach (var a in keynames)
        {
            namekeys.Add(a.Value, a.Key);
        }


        //create custom key allocations
        CreateKeyAllocation("shoot", KeyCode.Mouse0);
        CreateKeyAllocation("alt_shoot", KeyCode.Mouse1);
        CreateKeyAllocation("move_forward", KeyCode.W);
        CreateKeyAllocation("move_back", KeyCode.S);
        CreateKeyAllocation("move_left", KeyCode.A);
        CreateKeyAllocation("move_right", KeyCode.D);
        CreateKeyAllocation("jump", KeyCode.Space);
        CreateKeyAllocation("reload", KeyCode.R);
        CreateKeyAllocation("interact", KeyCode.F);
        CreateKeyAllocation("close_menu", KeyCode.Escape);
        CreateKeyAllocation("tab_menu", KeyCode.Tab);

        CollectInputAllocs.Invoke();

    }

    public static KeyCode GetArbitraryKeyPressed()
    {
        if (Input.anyKeyDown)
        {
            bool goodboi = false;
            KeyCode boi = KeyCode.Mouse0;
            foreach (var kb in keynames)
            {
                if (Input.GetKeyDown(kb.Key))
                {
                    boi = kb.Key;
                    goodboi = true;
                    break;
                }
            }
            if (goodboi)
            {
                return boi;
            }
        }
        return KeyCode.None;
    }
    public static List<KeyCode> GetAllCurrentlyPressedKeys()
    {
        if (Input.anyKeyDown)
        {
            bool goodboi = false;
            List<KeyCode> boi = new List<KeyCode>();
            foreach (var kb in keynames)
            {
                if (Input.GetKey(kb.Key))
                {
                    boi.Add(kb.Key);
                    goodboi = true;
                }
            }
            if (goodboi)
            {
                return boi;
            }
        }
        return new List<KeyCode>();
    }

    public static void ResetBind(string keyname)
    {
        //this is ResetBind but misspelled, I cant be bothered to fix this
        gamekeys[keyname] = defaultgamekeys[keyname];
    }


    public static bool GetSelected(List<string> ide)
    {
        return locklevel.Count==0 || ide.Count == 0 || ide[0] == "" || RandomFunctions.AllItemsFromListInList(ide, locklevel);
    }

    public static void ResetLockLevel()
    {
        locklevel.Clear();
    }

    public static void SetLockLevel(List<string> e)
    {
        locklevel = e;
    }

    public static void SetLockLevel(string e)
    {
        locklevel = new List<string>() {e};
    }

    public static bool AddLockLevel(string e)
    {
        if (!locklevel.Contains(e))
        {
            locklevel.Add(e);
            return true;
        }
        return false;
    }
    public static bool RemoveLockLevel(string e)
    {
        if (locklevel.Contains(e))
        {
            locklevel.Remove(e);
            return true;
        }
        return false;
    }

    public static bool IsDie(List<string> ide)
    {
        //set a = false to deny inputs
        bool a = true;



        return a;
    }


    public static bool CheckAvailability(string ide = "")
    {
        return CheckAvailability(new List<string>() { ide });
    }
    public static bool CheckAvailability(List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return true;
    }

    public static bool IsKeyDown(KeyCode baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetKeyDown(baller);
    }
    public static bool IsKey(KeyCode baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetKey(baller);
    }
    public static bool IsKeyUp(KeyCode baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetKeyUp(baller);
    }
    public static bool IsKeyDown(KeyCode baller, string ide)
    {
        return IsKeyDown(baller, new List<string>() { ide});
    }
    public static bool IsKey(KeyCode baller, string ide)
    {
        return IsKey(baller, new List<string>() { ide });
    }
    public static bool IsKeyUp(KeyCode baller, string ide)
    {
        return IsKeyUp(baller, new List<string>() { ide });
    }
    public static bool IsKeyDown(KeyCode baller)
    {
        return IsKeyDown(baller, new List<string>() { });
    }
    public static bool IsKey(KeyCode baller)
    {
        return IsKey(baller, new List<string>() { });
    }
    public static bool IsKeyUp(KeyCode baller)
    {
        return IsKeyUp(baller, new List<string>() { });
    }
    public static bool IsKeyDown(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        var keys = gamekeys[baller];
        foreach(var key in keys)
        {
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }
    public static bool IsKey(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        var keys = gamekeys[baller];
        foreach (var key in keys)
        {
            if (Input.GetKey(key)) return true;
        }
        return false;
    }
    public static bool IsKeyUp(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        var keys = gamekeys[baller];
        foreach (var key in keys)
        {
            if (Input.GetKeyUp(key)) return true;
        }
        return false;
    }
    public static bool IsKeyDown(string baller, string a = "")
    {
        return IsKeyDown(baller, new List<string>() { a});
    }
    public static bool IsKey(string baller, string a = "")
    {
        return IsKey(baller, new List<string>() { a });
    }
    public static bool IsKeyUp(string baller, string a = "")
    {
        return IsKeyUp(baller, new List<string>() { a });
    }
    [Obsolete]
    public static bool IsMouseDown(int baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetMouseButtonDown(baller);
    }
    [Obsolete]
    public static bool IsMouse(int baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetMouseButton(baller);
    }
    [Obsolete]
    public static bool IsMouseUp(int baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetMouseButtonUp(baller);
    }
    [Obsolete]
    public static bool IsButtonDown(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetButtonDown(baller);
    }
    [Obsolete]
    public static bool IsButton(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetButton(baller);
    }
    [Obsolete]
    public static bool IsButtonUp(string baller, List<string> ide)
    {
        if (!IsDie(ide)) return false;
        if (!GetSelected(ide)) return false;
        return Input.GetButtonUp(baller);
    }
    [Obsolete]
    public static bool IsMouseDown(int baller, string ide = "")
    {
        return IsMouseDown(baller, new List<string>() { ide });
    }
    [Obsolete]
    public static bool IsMouse(int baller, string ide = "")
    {
        return IsMouse(baller, new List<string>() { ide });
    }
    [Obsolete]
    public static bool IsMouseUp(int baller, string ide = "")
    {
        return IsMouseUp(baller, new List<string>() { ide });
    }
    [Obsolete]
    public static bool IsButtonDown(string baller, string ide = "")
    {
        return IsButtonDown(baller, new List<string>() { ide });
    }
    [Obsolete]
    public static bool IsButton(string baller, string ide = "")
    {
        return IsButton(baller, new List<string>() { ide });
    }
    [Obsolete]
    public static bool IsButtonUp(string baller, string ide = "")
    {
        return IsButtonUp(baller, new List<string>() { ide });
    }

    public static void SetGameKeys()
    {
        keynames.Clear();
        keynames.Add(KeyCode.A, "A");
        keynames.Add(KeyCode.B, "B");
        keynames.Add(KeyCode.C, "C");
        keynames.Add(KeyCode.D, "D");
        keynames.Add(KeyCode.E, "E");
        keynames.Add(KeyCode.F, "F");
        keynames.Add(KeyCode.G, "G");
        keynames.Add(KeyCode.H, "H");
        keynames.Add(KeyCode.I, "I");
        keynames.Add(KeyCode.J, "J");
        keynames.Add(KeyCode.K, "K");
        keynames.Add(KeyCode.L, "L");
        keynames.Add(KeyCode.M, "M");
        keynames.Add(KeyCode.N, "N");
        keynames.Add(KeyCode.O, "O");
        keynames.Add(KeyCode.P, "P");
        keynames.Add(KeyCode.Q, "Q");
        keynames.Add(KeyCode.R, "R");
        keynames.Add(KeyCode.S, "S");
        keynames.Add(KeyCode.T, "T");
        keynames.Add(KeyCode.U, "U");
        keynames.Add(KeyCode.V, "V");
        keynames.Add(KeyCode.W, "W");
        keynames.Add(KeyCode.X, "X");
        keynames.Add(KeyCode.Y, "Y");
        keynames.Add(KeyCode.Z, "Z");
        keynames.Add(KeyCode.Alpha0, "0");
        keynames.Add(KeyCode.Alpha1, "1");
        keynames.Add(KeyCode.Alpha2, "2");
        keynames.Add(KeyCode.Alpha3, "3");
        keynames.Add(KeyCode.Alpha4, "4");
        keynames.Add(KeyCode.Alpha5, "5");
        keynames.Add(KeyCode.Alpha6, "6");
        keynames.Add(KeyCode.Alpha7, "7");
        keynames.Add(KeyCode.Alpha8, "8");
        keynames.Add(KeyCode.Alpha9, "9");
        keynames.Add(KeyCode.Tab, "TAB");
        keynames.Add(KeyCode.LeftAlt, "LALT");
        keynames.Add(KeyCode.LeftControl, "LCTR");
        keynames.Add(KeyCode.LeftShift, "LSH");
        keynames.Add(KeyCode.LeftWindows, "LWIN");
        keynames.Add(KeyCode.CapsLock, "CAP");
        keynames.Add(KeyCode.RightAlt, "RALT");
        keynames.Add(KeyCode.RightControl, "RCTR");
        keynames.Add(KeyCode.RightShift, "RSH");
        keynames.Add(KeyCode.RightWindows, "RWIN");
        keynames.Add(KeyCode.Delete, "DEL");
        keynames.Add(KeyCode.Backspace, "BACK");
        keynames.Add(KeyCode.Insert, "INS");
        keynames.Add(KeyCode.PageDown, "PGDN");
        keynames.Add(KeyCode.PageUp, "PGUP");
        keynames.Add(KeyCode.Print, "PRT");
        keynames.Add(KeyCode.ScrollLock, "SLCK");
        keynames.Add(KeyCode.Pause, "PAUS");
        keynames.Add(KeyCode.End, "END");
        keynames.Add(KeyCode.Home, "HOME");
        keynames.Add(KeyCode.Mouse0, "m1");
        keynames.Add(KeyCode.Mouse1, "m2");
        keynames.Add(KeyCode.Mouse2, "m3");
        keynames.Add(KeyCode.Mouse3, "m4");
        keynames.Add(KeyCode.Mouse4, "m5");
        keynames.Add(KeyCode.Mouse5, "m6");
        keynames.Add(KeyCode.Mouse6, "m7");
        keynames.Add(KeyCode.Return, "ENT");
        keynames.Add(KeyCode.Backslash, "\\");
        keynames.Add(KeyCode.Slash, "/");
        keynames.Add(KeyCode.UpArrow, "UP");
        keynames.Add(KeyCode.DownArrow, "DOWN");
        keynames.Add(KeyCode.LeftArrow, "LEFT");
        keynames.Add(KeyCode.RightArrow, "RIGHT");
        keynames.Add(KeyCode.Space, "SPACE");
        keynames.Add(KeyCode.Escape, "ESC");
        keynames.Add(KeyCode.LeftBracket, "[");
        keynames.Add(KeyCode.RightBracket, "]");
        keynames.Add(KeyCode.Semicolon, ";");
        keynames.Add(KeyCode.Quote, "'");
        keynames.Add(KeyCode.Underscore, "_");
        keynames.Add(KeyCode.Equals, "=");
        keynames.Add(KeyCode.Numlock, "NML");
        keynames.Add(KeyCode.F1, "f1");
        keynames.Add(KeyCode.F2, "f2");
        keynames.Add(KeyCode.F3, "f3");
        keynames.Add(KeyCode.F4, "f4");
        keynames.Add(KeyCode.F5, "f5");
        keynames.Add(KeyCode.F6, "f6");
        keynames.Add(KeyCode.F7, "f7");
        keynames.Add(KeyCode.F8, "f8");
        keynames.Add(KeyCode.F9, "f9");
        keynames.Add(KeyCode.F10, "f10");
        keynames.Add(KeyCode.F11, "f11");
        keynames.Add(KeyCode.F12, "f12");
        keynames.Add(KeyCode.F13, "f13");
        keynames.Add(KeyCode.F14, "f14");
        keynames.Add(KeyCode.F15, "f15");
        keynames.Add(KeyCode.Keypad0, "n0");
        keynames.Add(KeyCode.Keypad1, "n1");
        keynames.Add(KeyCode.Keypad2, "n2");
        keynames.Add(KeyCode.Keypad3, "n3");
        keynames.Add(KeyCode.Keypad4, "n4");
        keynames.Add(KeyCode.Keypad5, "n5");
        keynames.Add(KeyCode.Keypad6, "n6");
        keynames.Add(KeyCode.Keypad7, "n7");
        keynames.Add(KeyCode.Keypad8, "n8");
        keynames.Add(KeyCode.Keypad9, "n9");
        keynames.Add(KeyCode.KeypadDivide, "n/");
        keynames.Add(KeyCode.KeypadEquals, "n=");
        keynames.Add(KeyCode.KeypadMinus, "n-");
        keynames.Add(KeyCode.KeypadMultiply, "n*");
        keynames.Add(KeyCode.KeypadPeriod, "n.");
        keynames.Add(KeyCode.KeypadPlus, "n+");
        keynames.Add(KeyCode.KeypadEnter, "nENT");
        keynames.Add(KeyCode.None, "NONE");
        keynames.Add(KeyCode.Tilde, "~");
        keynames.Add(KeyCode.AltGr, "grrr uwu");
        keynames.Add(KeyCode.BackQuote, "`");
        keynames.Add(KeyCode.Minus, "-");
        keynames.Add(KeyCode.Period, ".");
        keynames.Add(KeyCode.Comma, ",");
    }

    public static void CreateKeyAllocation(string name, KeyCode key)
    {
        CreateKeyAllocation(name, new List<KeyCode>() { key });
    }
    public static void CreateKeyAllocation(string name, List<KeyCode> keys)
    {
        gamekeys.Add(name, keys);
        defaultgamekeys.Add(name, keys);
    }
}
