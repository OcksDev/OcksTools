using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingleInstance<InputManager>
{
    public static List<string> locklevel = new List<string>();
    public static Dictionary<KeyCode, string> keynames = new Dictionary<KeyCode, string>();
    public static Dictionary<string, KeyCode> namekeys = new Dictionary<string, KeyCode>();
    public static Dictionary<string, List<KeyCode>> gamekeys = new Dictionary<string, List<KeyCode>>();
    public static Dictionary<string, List<KeyCode>> defaultgamekeys = new Dictionary<string, List<KeyCode>>();
    public static Dictionary<string, string> gamekeynames = new Dictionary<string, string>();
    public static OXEvent CollectInputAllocs = new OXEvent();
    // Start is called before the first frame update
    private void Start()
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
        CreateKeyAllocation("slide", KeyCode.LeftControl);
        CreateKeyAllocation("dash", KeyCode.LeftShift);
        CreateKeyAllocation("reload", KeyCode.R);
        CreateKeyAllocation("interact", KeyCode.F);
        CreateKeyAllocation("close_menu", new List<KeyCode>() { KeyCode.Escape, KeyCode.JoystickButton1 });
        CreateKeyAllocation("tab_menu", KeyCode.Tab);

        CollectInputAllocs.Invoke();

    }

    public static string GetReadableStringOf(List<KeyCode> n, bool first_only = false)
    {
        var dd = n.AListToBList((x) => keynames[x]);
        if (first_only && dd.Count > 1)
        {
            dd = dd.GetRange(0, 1);
        }
        return dd.ListToString();
    }
    public static string GetReadableStringOf(string a, bool first_only = false)
    {
        return GetReadableStringOf(gamekeys[a], first_only);
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
        gamekeys[keyname] = defaultgamekeys[keyname];
    }


    public static bool InputPassesLockLevel(List<string> ide)
    {
        // ide = {"a", "b"}  locklevel = {"a", "b", "c"} returns: true
        // ide = {"a", "b", "no"}  locklevel = {"a", "b"} returns: false
        return locklevel.Count == 0 || ide.Count == 0 || ide[0] == ""
            || ide.AllItemsFromListInList(locklevel);
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
        locklevel = new List<string>() { e };
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

    public static bool AllowInputToPass(List<string> ide)
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
        if (!AllowInputToPass(ide)) return false;
        if (!InputPassesLockLevel(ide)) return false;
        return true;
    }
    public static bool IsKeyDown(InputManagerKeyVal baller)
    {
        var keys = baller.ToList();
        foreach (var key in keys)
        {
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }
    public static bool IsKey(InputManagerKeyVal baller)
    {
        var keys = baller.ToList();
        foreach (var key in keys)
        {
            if (Input.GetKey(key)) return true;
        }
        return false;
    }
    public static bool IsKeyUp(InputManagerKeyVal baller)
    {
        var keys = baller.ToList();
        foreach (var key in keys)
        {
            if (Input.GetKeyUp(key)) return true;
        }
        return false;
    }
    public static bool IsKeyDown(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!AllowInputToPass(ide)) return false;
        if (!InputPassesLockLevel(ide)) return false;
        return IsKeyDown(baller);
    }
    public static bool IsKey(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!AllowInputToPass(ide)) return false;
        if (!InputPassesLockLevel(ide)) return false;
        return IsKey(baller);
    }
    public static bool IsKeyUp(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!AllowInputToPass(ide)) return false;
        if (!InputPassesLockLevel(ide)) return false;
        return IsKeyUp(baller);
    }
    public static void SetGameKeys()
    {
        keynames = new Dictionary<KeyCode, string>
        {
            { KeyCode.A, "A" },
            { KeyCode.B, "B" },
            { KeyCode.C, "C" },
            { KeyCode.D, "D" },
            { KeyCode.E, "E" },
            { KeyCode.F, "F" },
            { KeyCode.G, "G" },
            { KeyCode.H, "H" },
            { KeyCode.I, "I" },
            { KeyCode.J, "J" },
            { KeyCode.K, "K" },
            { KeyCode.L, "L" },
            { KeyCode.M, "M" },
            { KeyCode.N, "N" },
            { KeyCode.O, "O" },
            { KeyCode.P, "P" },
            { KeyCode.Q, "Q" },
            { KeyCode.R, "R" },
            { KeyCode.S, "S" },
            { KeyCode.T, "T" },
            { KeyCode.U, "U" },
            { KeyCode.V, "V" },
            { KeyCode.W, "W" },
            { KeyCode.X, "X" },
            { KeyCode.Y, "Y" },
            { KeyCode.Z, "Z" },
            { KeyCode.Alpha0, "0" },
            { KeyCode.Alpha1, "1" },
            { KeyCode.Alpha2, "2" },
            { KeyCode.Alpha3, "3" },
            { KeyCode.Alpha4, "4" },
            { KeyCode.Alpha5, "5" },
            { KeyCode.Alpha6, "6" },
            { KeyCode.Alpha7, "7" },
            { KeyCode.Alpha8, "8" },
            { KeyCode.Alpha9, "9" },
            { KeyCode.Tab, "TAB" },
            { KeyCode.LeftAlt, "LALT" },
            { KeyCode.LeftControl, "LCTR" },
            { KeyCode.LeftShift, "LSH" },
            { KeyCode.LeftWindows, "LWIN" },
            { KeyCode.CapsLock, "CAP" },
            { KeyCode.RightAlt, "RALT" },
            { KeyCode.RightControl, "RCTR" },
            { KeyCode.RightShift, "RSH" },
            { KeyCode.RightWindows, "RWIN" },
            { KeyCode.Delete, "DEL" },
            { KeyCode.Backspace, "BACK" },
            { KeyCode.Insert, "INS" },
            { KeyCode.PageDown, "PGDN" },
            { KeyCode.PageUp, "PGUP" },
            { KeyCode.Print, "PRT" },
            { KeyCode.ScrollLock, "SLCK" },
            { KeyCode.Pause, "PAUS" },
            { KeyCode.End, "END" },
            { KeyCode.Home, "HOME" },
            { KeyCode.Mouse0, "m1" },
            { KeyCode.Mouse1, "m2" },
            { KeyCode.Mouse2, "m3" },
            { KeyCode.Mouse3, "m4" },
            { KeyCode.Mouse4, "m5" },
            { KeyCode.Mouse5, "m6" },
            { KeyCode.Mouse6, "m7" },
            { KeyCode.Return, "ENT" },
            { KeyCode.Backslash, "\\" },
            { KeyCode.Slash, "/" },
            { KeyCode.UpArrow, "UP" },
            { KeyCode.DownArrow, "DOWN" },
            { KeyCode.LeftArrow, "LEFT" },
            { KeyCode.RightArrow, "RIGHT" },
            { KeyCode.Space, "SPACE" },
            { KeyCode.Escape, "ESC" },
            { KeyCode.LeftBracket, "[" },
            { KeyCode.RightBracket, "]" },
            { KeyCode.Semicolon, ";" },
            { KeyCode.Quote, "'" },
            { KeyCode.Underscore, "_" },
            { KeyCode.Equals, "=" },
            { KeyCode.Numlock, "NML" },
            { KeyCode.F1, "f1" },
            { KeyCode.F2, "f2" },
            { KeyCode.F3, "f3" },
            { KeyCode.F4, "f4" },
            { KeyCode.F5, "f5" },
            { KeyCode.F6, "f6" },
            { KeyCode.F7, "f7" },
            { KeyCode.F8, "f8" },
            { KeyCode.F9, "f9" },
            { KeyCode.F10, "f10" },
            { KeyCode.F11, "f11" },
            { KeyCode.F12, "f12" },
            { KeyCode.F13, "f13" },
            { KeyCode.F14, "f14" },
            { KeyCode.F15, "f15" },
            { KeyCode.Keypad0, "n0" },
            { KeyCode.Keypad1, "n1" },
            { KeyCode.Keypad2, "n2" },
            { KeyCode.Keypad3, "n3" },
            { KeyCode.Keypad4, "n4" },
            { KeyCode.Keypad5, "n5" },
            { KeyCode.Keypad6, "n6" },
            { KeyCode.Keypad7, "n7" },
            { KeyCode.Keypad8, "n8" },
            { KeyCode.Keypad9, "n9" },
            { KeyCode.KeypadDivide, "n/" },
            { KeyCode.KeypadEquals, "n=" },
            { KeyCode.KeypadMinus, "n-" },
            { KeyCode.KeypadMultiply, "n*" },
            { KeyCode.KeypadPeriod, "n." },
            { KeyCode.KeypadPlus, "n+" },
            { KeyCode.KeypadEnter, "nENT" },
            { KeyCode.None, "NONE" },
            { KeyCode.Tilde, "~" },
            { KeyCode.AltGr, "grrr uwu" },
            { KeyCode.BackQuote, "`" },
            { KeyCode.Minus, "-" },
            { KeyCode.Period, "." },
            { KeyCode.Comma, "," },
            { KeyCode.WheelDown, "wDWN" },
            { KeyCode.WheelUp, "wUP" },
            { KeyCode.JoystickButton0, "cA" },
            { KeyCode.JoystickButton1, "cB" },
            { KeyCode.JoystickButton2, "cX" },
            { KeyCode.JoystickButton3, "cY" },
            { KeyCode.JoystickButton4, "cLB" },
            { KeyCode.JoystickButton5, "cRB" },
            { KeyCode.JoystickButton6, "cBACK" },
            { KeyCode.JoystickButton7, "cSTRT" },
            { KeyCode.JoystickButton8, "cLSB" },
            { KeyCode.JoystickButton9, "cRSB" }
        };
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


public readonly struct InputManagerKeyVal
{
    private readonly BetterList<KeyCode> _keyvals;
    private readonly string nerd;
    public InputManagerKeyVal(string value)
    {
        _keyvals = null;
        nerd = value;
    }

    public InputManagerKeyVal(KeyCode values)
    {
        nerd = "";
        _keyvals = values;
    }

    public static implicit operator InputManagerKeyVal(KeyCode value)
        => new InputManagerKeyVal(value);

    public static implicit operator InputManagerKeyVal(string value)
        => new InputManagerKeyVal(value);

    public static implicit operator List<KeyCode>(InputManagerKeyVal values)
        => values.ToList();

    public List<KeyCode> ToList() => nerd != "" ? InputManager.gamekeys[nerd] : _keyvals.ToList();
}