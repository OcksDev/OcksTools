using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class InputManager : SingleInstance<InputManager>
{
    public bool PollForEvents = true;
    public static List<string> locklevel = new List<string>();
    public static Dictionary<KeyCode, string> keynames = new Dictionary<KeyCode, string>();
    public static Dictionary<string, KeyCode> namekeys = new Dictionary<string, KeyCode>();
    public static Dictionary<string, List<KeyCode>> gamekeys = new Dictionary<string, List<KeyCode>>();
    public static Dictionary<string, List<KeyCode>> defaultgamekeys = new Dictionary<string, List<KeyCode>>();
    public static OXEvent CollectInputAllocs = new OXEvent();


    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        SaveSystem.SaveAllData.Append(SaveInput);
        SaveSystem.LoadAllData.Append(LoadInput);
    }

    private static void SaveInput(SaveProfile dict)
    {
        Dictionary<string, string> dic = gamekeys.ABToCD(x => x, y => y.AToB(x => keynames[x]).ListToString(","));
        SaveSystem.GlobalProfile().SetDict("keybinds", dic);
    }

    private static void LoadInput(SaveProfile dict)
    {
        dict = SaveSystem.GlobalProfile();
        var dic = dict.GetDict("keybinds", new Dictionary<string, string>());
        List<KeyCode> shungite = new List<KeyCode>();
        if (dic.Count > 0)
        {
            foreach (var a in dic)
            {
                InputManager.gamekeys[a.Key] = a.Value.StringToList(",").AToB((x) => namekeys[x]);
            }
        }
    }


    // Start is called before the first frame update
    private void Start()
    {
        AssembleTheCodes();
        ResetLockLevel();
    }

    public static float StickDeadzone = 0.2f;

    public static Vector2 GetWishDirection(BetterList<string> ide, int controller = 0)
    {
        if (!CheckAxisAvailability(ide)) return Vector2.zero;

        Vector2 dir = Vector2.zero;

        // keyboard
        if (IsBindHeld("move_right")) dir.x += 1f;
        if (IsBindHeld("move_left")) dir.x -= 1f;
        if (IsBindHeld("move_forward")) dir.y += 1f;
        if (IsBindHeld("move_back")) dir.y -= 1f;

        // controller
        Vector2 stick = new Vector2(
            GetAxis(InputAxis.ControllerRightLeft, controller),
            GetAxis(InputAxis.ControllerUpDown, controller));
        if (stick.magnitude >= StickDeadzone) dir += stick;

        return Vector2.ClampMagnitude(dir, 1f);
    }

    public static Vector2 GetWishDirectionNormalized(BetterList<string> ide, int controller = 0)
    {
        return GetWishDirection(ide, controller).normalized;
    }

    private static bool IsBindHeld(string bind)
    {
        if (!gamekeys.TryGetValue(bind, out var keys)) return false;
        for (int i = 0; i < keys.Count; i++)
        {
            if (KeyHeld(keys[i])) return true;
        }
        return false;
    }

    public static void AssembleTheCodes()
    {
        namekeys.Clear();
        gamekeys.Clear();
        defaultgamekeys.Clear();

        SetGameKeys();

        //namekeys and keynames are both dictionaries
        foreach (var a in keynames)
        {
            namekeys.Add(a.Value, a.Key);
        }


        //create custom key allocations
        CreateInputSet("shoot", KeyCode.Mouse0);
        CreateInputSet("alt_shoot", KeyCode.Mouse1);
        CreateInputSet("move_forward", KeyCode.W);
        CreateInputSet("move_back", KeyCode.S);
        CreateInputSet("move_left", KeyCode.A);
        CreateInputSet("move_right", KeyCode.D);
        CreateInputSet("jump", KeyCode.Space, KeyCode.JoystickButton0);
        CreateInputSet("slide", KeyCode.LeftControl, KeyCode.JoystickButton5);
        CreateInputSet("dash", KeyCode.LeftShift, KeyCode.JoystickButton4);
        CreateInputSet("reload", KeyCode.R, KeyCode.JoystickButton3);
        CreateInputSet("interact", KeyCode.F, KeyCode.JoystickButton2);
        CreateInputSet("close_menu", KeyCode.Escape, KeyCode.JoystickButton1);
        CreateInputSet("tab_menu", KeyCode.Tab, KeyCode.JoystickButton7);

        CollectInputAllocs.Invoke();

    }

    public static Dictionary<string, MRef<string, List<string>, OXEvent>> InputEvents = new();
    public static Dictionary<string, MRef<string, List<string>, OXEvent>> InputEventsUp = new();
    public static Dictionary<string, MRef<string, List<string>, OXEvent>> InputEventsDown = new();
    private void Update()
    {
        RefreshVirtualKeys(); // always, so down/up edges for triggers + d-pad are never missed
        if (!PollForEvents) return;
        foreach (var a in gamekeys)
        {
            // Dispatch bails immediately when no listener is registered for this bind,
            // so unused binds cost nothing beyond the dictionary lookup.
            Dispatch(InputEvents, a.Key, a.Value, 0);
            Dispatch(InputEventsUp, a.Key, a.Value, 1);
            Dispatch(InputEventsDown, a.Key, a.Value, 2);
        }
    }

    // mode: 0 = held, 1 = released this frame, 2 = pressed this frame
    private static void Dispatch(Dictionary<string, MRef<string, List<string>, OXEvent>> events, string name, List<KeyCode> keys, int mode)
    {
        if (events.Count == 0 || !events.TryGetValue(name, out var c)) return;
        for (int i = 0; i < keys.Count; i++)
        {
            KeyCode key = keys[i];
            if (!(mode == 0 ? KeyHeld(key) : mode == 1 ? KeyUp(key) : KeyDown(key))) continue;
            if (c.b == null)
            {
                c.c.Invoke();
                continue;
            }
            if (!AllowInputToPass(c.b)) continue;
            if (!InputPassesLockLevel(c.b)) continue;
            c.c.Invoke();
            break;
        }
    }

    public static OXEvent IsKeyEvent(string name, BetterList<string>? ide = null)
    {
        var sex = GetExistingEvent(name);
        if (sex != null) return sex;
        MRef<string, List<string>, OXEvent> b = new();
        b.a = name;
        b.b = ide.HasValue ? ide.Value.ToList() : null;
        b.c = new OXEvent();
        InputEvents.GetOrDefine(name, b);
        return b.c;
    }

    public static OXEvent IsKeyUpEvent(string name, BetterList<string>? ide = null)
    {
        var sex = GetExistingEventUp(name);
        if (sex != null) return sex;
        MRef<string, List<string>, OXEvent> b = new();
        b.a = name;
        b.b = ide.HasValue ? ide.Value.ToList() : null;
        b.c = new OXEvent();
        InputEventsUp.GetOrDefine(name, b);
        return b.c;
    }

    public static OXEvent IsKeyDownEvent(string name, BetterList<string>? ide = null)
    {
        var sex = GetExistingEventDown(name);
        if (sex != null) return sex;
        MRef<string, List<string>, OXEvent> b = new();
        b.a = name;
        b.b = ide.HasValue ? ide.Value.ToList() : null;
        b.c = new OXEvent();
        InputEventsDown.GetOrDefine(name, b);
        return b.c;
    }

    public static OXEvent GetExistingEvent(string name)
    {
        return InputEvents.ContainsKey(name) ? InputEvents[name].c : null;
    }

    public static OXEvent GetExistingEventUp(string name)
    {
        return InputEventsUp.ContainsKey(name) ? InputEventsUp[name].c : null;
    }

    public static OXEvent GetExistingEventDown(string name)
    {
        return InputEventsDown.ContainsKey(name) ? InputEventsDown[name].c : null;
    }

    public static void RemoveEvent(string name)
    {
        InputEvents.Remove(name);
    }

    public static void RemoveEventUp(string name)
    {
        InputEventsUp.Remove(name);
    }

    public static void RemoveEventDown(string name)
    {
        InputEventsDown.Remove(name);
    }



    public static string GetReadableStringOf(List<KeyCode> n, bool first_only = false)
    {
        var dd = n.AToB((x) => keynames[x]);
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

    public static bool IsControllerKey(KeyCode k) =>
    (k >= KeyCode.JoystickButton0 && k <= KeyCode.JoystickButton19) || IsVirtualKey(k);

    // Tags are built once per key, so repeated UI refreshes don't allocate.
    private static readonly Dictionary<KeyCode, string> spriteTags = new();

    public static string GetKeyRichText(KeyCode k)
    {
        if (!IsControllerKey(k)) return keynames[k];
        if (!spriteTags.TryGetValue(k, out var tag))
            spriteTags[k] = tag = $"<sprite name=\"{keynames[k]}\">";
        return tag;
    }

    public static string GetRichStringOf(List<KeyCode> keys, bool first_only = false)
    {
        if (keys.Count == 0) return "";

        if (first_only || keys.Count == 1) return GetKeyRichText(keys[0]);
        var sb = new StringBuilder();
        for (int i = 0; i < keys.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(GetKeyRichText(keys[i]));
        }
        return sb.ToString();
    }

    public static string GetRichStringOf(string bind, bool first_only = false) =>
        GetRichStringOf(gamekeys[bind], first_only);




    public static KeyCode GetArbitraryKeyPressed()
    {
        if (Input.anyKeyDown)
        {
            foreach (var kb in keynames)
            {
                if (!IsVirtualKey(kb.Key) && Input.GetKeyDown(kb.Key)) return kb.Key;
            }
        }
        // triggers / d-pad never set Input.anyKeyDown, so check them separately
        RefreshVirtualKeys();
        if (vDown != 0)
        {
            for (int i = 0; i < VKeyCount; i++)
            {
                if ((vDown & (1 << i)) != 0) return (KeyCode)(VKeyStart + i);
            }
        }
        return KeyCode.None;
    }
    public static List<KeyCode> GetAllCurrentlyPressedKeys()
    {
        RefreshVirtualKeys();
        var boi = new List<KeyCode>();
        if (!Input.anyKeyDown && vDown == 0) return boi;
        foreach (var kb in keynames)
        {
            if (!IsVirtualKey(kb.Key) && Input.GetKey(kb.Key)) boi.Add(kb.Key);
        }
        for (int i = 0; i < VKeyCount; i++)
        {
            if ((vHeld & (1 << i)) != 0) boi.Add((KeyCode)(VKeyStart + i));
        }
        return boi;
    }

    public static void ResetBind(string keyname)
    {
        gamekeys[keyname] = new List<KeyCode>(defaultgamekeys[keyname]);
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
        //return false to deny inputs
        // Input.anyKey ignores triggers / d-pad (they're axes), so check the virtual keys too.
        if (Input.anyKey) return true;
        RefreshVirtualKeys();
        return (vHeld | vUp) != 0;
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
            if (KeyDown(key)) return true;
        }
        return false;
    }
    public static bool IsKey(InputManagerKeyVal baller)
    {
        var keys = baller.ToList();
        foreach (var key in keys)
        {
            if (KeyHeld(key)) return true;
        }
        return false;
    }
    public static bool IsKeyUp(InputManagerKeyVal baller)
    {
        var keys = baller.ToList();
        foreach (var key in keys)
        {
            if (KeyUp(key)) return true;
        }
        return false;
    }
    public static bool IsKeyDown(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!CheckAvailability(ide)) return false;
        return IsKeyDown(baller);
    }
    public static bool IsKey(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!CheckAvailability(ide)) return false;
        return IsKey(baller);
    }
    public static bool IsKeyUp(InputManagerKeyVal baller, BetterList<string> ide)
    {
        if (!CheckAvailability(ide)) return false;
        return IsKeyUp(baller);
    }

    // Axes must NOT go through CheckAvailability: it requires Input.anyKey, and
    // anyKey is false when only a stick / the mouse is moving. Axes only respect the lock level.
    public static bool CheckAxisAvailability(List<string> ide)
    {
        return InputPassesLockLevel(ide);
    }

    public static float GetAxis(InputAxis a, BetterList<string> ide, int controller = 0)
    {
        if (!CheckAxisAvailability(ide)) return 0;
        return GetAxis(a, controller);
    }

    private static readonly HashSet<string> missingAxes = new HashSet<string>();

    // Unity throws if an axis name isn't defined. Catch it so a missing axis
    // logs once instead of breaking the game every frame.
    private static float RawAxis(string axisName)
    {
        if (missingAxes.Contains(axisName)) return 0f;
        try
        {
            return Input.GetAxisRaw(axisName);
        }
        catch (ArgumentException)
        {
            missingAxes.Add(axisName);
            LogMissingAxis(axisName);
            return 0f;
        }
    }

    private static void LogMissingAxis(string axisName)
    {
        Debug.LogError($"[InputManager] Input axis '{axisName}' is not defined. Make sure InputAxisBootstrapper.cs is in the project (it registers controller axes automatically).");
    }

    // Axis names are built once, so polling never allocates a string per call.
    private const int MaxJoy = 4;
    private const int MaxJoyAxis = 10;
    private static readonly string[,] joyAxisNames = BuildJoyAxisNames();
    private static readonly bool[,] joyAxisMissing = new bool[MaxJoy + 1, MaxJoyAxis + 1];

    private static string[,] BuildJoyAxisNames()
    {
        var names = new string[MaxJoy + 1, MaxJoyAxis + 1];
        for (int c = 0; c <= MaxJoy; c++)
            for (int a = 0; a <= MaxJoyAxis; a++)
                names[c, a] = $"joystick {c} axis {a}";
        return names;
    }

    private static float Joy(int controller, int axisNumber)
    {
        if ((uint)controller > MaxJoy || (uint)axisNumber > MaxJoyAxis)
            return RawAxis($"joystick {controller} axis {axisNumber}");
        if (joyAxisMissing[controller, axisNumber]) return 0f;
        try
        {
            return Input.GetAxisRaw(joyAxisNames[controller, axisNumber]);
        }
        catch (ArgumentException)
        {
            joyAxisMissing[controller, axisNumber] = true;
            LogMissingAxis(joyAxisNames[controller, axisNumber]);
            return 0f;
        }
    }
    public const KeyCode VKeyLT = KeyCode.Joystick8Button10;
    public const KeyCode VKeyRT = KeyCode.Joystick8Button11;
    public const KeyCode VKeyDpadUp = KeyCode.Joystick8Button12;
    public const KeyCode VKeyDpadDown = KeyCode.Joystick8Button13;
    public const KeyCode VKeyDpadLeft = KeyCode.Joystick8Button14;
    public const KeyCode VKeyDpadRight = KeyCode.Joystick8Button15;

    private const int VKeyStart = (int)KeyCode.Joystick8Button10;
    private const int VKeyCount = 6;

    /// <summary>Axis value needed to count as pressed.</summary>
    public static float VirtualPressThreshold = 0.5f;
    /// <summary>Axis value an already-pressed virtual key must fall below to release (hysteresis, stops flicker).</summary>
    public static float VirtualReleaseThreshold = 0.35f;

    private static int vHeld, vDown, vUp;
    private static int vFrame = -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsVirtualKey(KeyCode k) => (uint)((int)k - VKeyStart) < VKeyCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int VBit(float value, int bit, int prev)
    {
        float threshold = (prev & (1 << bit)) != 0 ? VirtualReleaseThreshold : VirtualPressThreshold;
        return value > threshold ? 1 << bit : 0;
    }

    // Recomputes the virtual key state at most once per frame (3 axis reads total).
    private static void RefreshVirtualKeys()
    {
        int frame = Time.frameCount;
        if (vFrame == frame) return;
        vFrame = frame;

        float trig = Joy(0, 3); // shared trigger axis: LT = negative, RT = positive (matches GetAxis)
        float dx = Joy(0, 6);   // d-pad X, right = +
        float dy = Joy(0, 7);   // d-pad Y, up = +

        int prev = vHeld;
        int now = VBit(-trig, 0, prev) | VBit(trig, 1, prev)
                | VBit(dy, 2, prev) | VBit(-dy, 3, prev)
                | VBit(-dx, 4, prev) | VBit(dx, 5, prev);

        vDown = now & ~prev;
        vUp = prev & ~now;
        vHeld = now;
    }

    /// <summary>Drop-in for Input.GetKey that also understands the virtual trigger / d-pad keys.</summary>
    public static bool KeyHeld(KeyCode k)
    {
        if (!IsVirtualKey(k)) return Input.GetKey(k);
        RefreshVirtualKeys();
        return (vHeld & (1 << ((int)k - VKeyStart))) != 0;
    }

    /// <summary>Drop-in for Input.GetKeyDown that also understands the virtual trigger / d-pad keys.</summary>
    public static bool KeyDown(KeyCode k)
    {
        if (!IsVirtualKey(k)) return Input.GetKeyDown(k);
        RefreshVirtualKeys();
        return (vDown & (1 << ((int)k - VKeyStart))) != 0;
    }

    /// <summary>Drop-in for Input.GetKeyUp that also understands the virtual trigger / d-pad keys.</summary>
    public static bool KeyUp(KeyCode k)
    {
        if (!IsVirtualKey(k)) return Input.GetKeyUp(k);
        RefreshVirtualKeys();
        return (vUp & (1 << ((int)k - VKeyStart))) != 0;
    }

    /// <summary>Right stick look speed, in mouse-delta units per second at full tilt.</summary>
    public static float StickLookSpeed = 150f;
    /// <summary>Radial deadzone for the right stick (stops drift).</summary>
    public static float LookStickDeadzone = 0.15f;

    // Right stick with a radial deadzone, rescaled so it ramps smoothly from 0. Up = +y.
    private static Vector2 GetRightStick(int controller)
    {
        Vector2 s = new Vector2(Joy(controller, 4), -Joy(controller, 5));
        float mag = s.magnitude;
        if (mag < LookStickDeadzone) return Vector2.zero;
        float scaled = Mathf.Min(1f, (mag - LookStickDeadzone) / (1f - LookStickDeadzone));
        return s / mag * scaled;
    }

    /// <summary>
    /// Mouse + right stick look input for this frame (x = right, y = up).
    /// The mouse is already a per-frame delta; the stick is a rate, so it is scaled by
    /// StickLookSpeed * deltaTime to be the same kind of number. Call once per frame from Update.
    /// </summary>
    public static Vector2 GetLook(int controller = 0)
    {
        Vector2 mouse = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        return mouse + GetRightStick(controller) * (StickLookSpeed * Time.deltaTime);
    }

    /// <param name="controller">0 = any controller, 1-4 = a specific one.</param>
    public static float GetAxis(InputAxis a, int controller = 0)
    {
        switch (a)
        {
            case InputAxis.SharedHorizontal: return Input.GetAxisRaw("Horizontal");
            case InputAxis.SharedVertical: return Input.GetAxisRaw("Vertical");
            // Unity reports stick "up" as negative, so Y axes are flipped to make up = +1.
            case InputAxis.ControllerRightLeft: return Joy(controller, 1);        // left stick X
            case InputAxis.ControllerUpDown: return -Joy(controller, 2);          // left stick Y
            case InputAxis.MouseX: return Input.GetAxis("Mouse X");
            case InputAxis.MouseY: return Input.GetAxis("Mouse Y");
            case InputAxis.MouseWheel: return Input.GetAxis("Mouse ScrollWheel");
            case InputAxis.MouseXRaw: return Input.GetAxisRaw("Mouse X");
            case InputAxis.MouseYRaw: return Input.GetAxisRaw("Mouse Y");
            case InputAxis.MouseWheelRaw: return Input.GetAxisRaw("Mouse ScrollWheel");
            case InputAxis.ControllerLT: return Mathf.Clamp01(-Joy(controller, 3));
            case InputAxis.ControllerRT: return Mathf.Clamp01(Joy(controller, 3));
            case InputAxis.ControllerPlusRightLeft: return Joy(controller, 6);    // d-pad X
            case InputAxis.ControllerPlusUpDown: return Joy(controller, 7);       // d-pad Y
            case InputAxis.ControllerRightLeftOther: return Joy(controller, 4);   // right stick X
            case InputAxis.ControllerUpDownOther: return -Joy(controller, 5);     // right stick Y
            case InputAxis.SharedLookRightLight: return GetLook(controller).x;
            case InputAxis.SharedLookUpDown: return GetLook(controller).y;
            default: throw new NotImplementedException();
        }
    }



    public enum InputAxis
    {
        SharedHorizontal, SharedVertical,
        SharedLookUpDown, SharedLookRightLight,
        ControllerUpDown, ControllerRightLeft,
        MouseX, MouseY, MouseWheel,
        MouseXRaw, MouseYRaw, MouseWheelRaw,
        ControllerLT, ControllerRT, ControllerPlusUpDown, ControllerPlusRightLeft,
        ControllerUpDownOther, ControllerRightLeftOther,
    }

    public int GetNumberKeyDown()
    {
        if (!Input.anyKey) return -1;
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) return 6;
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) return 7;
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) return 8;
        if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) return 9;
        return -1;
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
            { KeyCode.JoystickButton4, "cLB" }, // bumper
            { KeyCode.JoystickButton5, "cRB" }, // bumper
            { KeyCode.JoystickButton6, "cBACK" }, // left helper button
            { KeyCode.JoystickButton7, "cSTRT" }, // right helper button
            { KeyCode.JoystickButton8, "cLSB" }, // stick press
            { KeyCode.JoystickButton9, "cRSB" }, // stick press
            { KeyCode.JoystickButton10, "c10" }, // do these controller inputs have real names?
            { KeyCode.JoystickButton11, "c11" },
            { KeyCode.JoystickButton12, "c12" },
            { KeyCode.JoystickButton13, "c13" },
            { KeyCode.JoystickButton14, "c14" },
            { KeyCode.JoystickButton15, "c15" },
            // virtual keys (axis-derived, see RefreshVirtualKeys)
            { VKeyLT, "cLT" },
            { VKeyRT, "cRT" },
            { VKeyDpadUp, "cDUP" },
            { VKeyDpadDown, "cDDOWN" },
            { VKeyDpadLeft, "cDLEFT" },
            { VKeyDpadRight, "cDRIGHT" },
        };
    }

    public static void CreateInputSet(string name, KeyCode key)
    {
        CreateInputSet(name, new List<KeyCode>() { key });
    }
    public static void CreateInputSet(string name, List<KeyCode> keys)
    {
        gamekeys.Add(name, new List<KeyCode>(keys));
        defaultgamekeys.Add(name, new List<KeyCode>(keys));
    }
    public static void CreateInputSet(string name, params KeyCode[] keys)
    {
        gamekeys.Add(name, keys.ToList());
        defaultgamekeys.Add(name, keys.ToList());
    }
}


public readonly struct InputManagerKeyVal
{
    private readonly BetterList<KeyCode> _keyvals;
    private readonly string nerd;
    public InputManagerKeyVal(string value)
    {
        _keyvals = default;
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