using System;
using System.Text;
using Unity.Collections;
using UnityEngine;


// One big file of AI slop code right here
// I dont care though because this is entirely editor-side inspector code that never gets compiled into a final exe

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowFixedBetterAttribute : Attribute { }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(FixedString32Bytes))]
public class _FixedString32Drawer : _FixedStringDrawerBase
{
    protected override int MaxBytes => 29;
    protected override string ReadStr(object o) => ((FixedString32Bytes)o).ToString();
    protected override object ToFixed(string s) => (FixedString32Bytes)s;
}

[CustomPropertyDrawer(typeof(FixedString64Bytes))]
public class _FixedString64Drawer : _FixedStringDrawerBase
{
    protected override int MaxBytes => 61;
    protected override string ReadStr(object o) => ((FixedString64Bytes)o).ToString();
    protected override object ToFixed(string s) => (FixedString64Bytes)s;
}

[CustomPropertyDrawer(typeof(FixedString128Bytes))]
public class _FixedString128Drawer : _FixedStringDrawerBase
{
    protected override int MaxBytes => 125;
    protected override string ReadStr(object o) => ((FixedString128Bytes)o).ToString();
    protected override object ToFixed(string s) => (FixedString128Bytes)s;
}

[CustomPropertyDrawer(typeof(FixedString512Bytes))]
public class _FixedString512Drawer : _FixedStringDrawerBase
{
    protected override int MaxBytes => 509;
    protected override string ReadStr(object o) => ((FixedString512Bytes)o).ToString();
    protected override object ToFixed(string s) => (FixedString512Bytes)s;
}

[CustomPropertyDrawer(typeof(FixedString4096Bytes))]
public class _FixedString4096Drawer : _FixedStringDrawerBase
{
    protected override int MaxBytes => 4093;
    protected override string ReadStr(object o) => ((FixedString4096Bytes)o).ToString();
    protected override object ToFixed(string s) => (FixedString4096Bytes)s;
}

public abstract class _FixedStringDrawerBase : PropertyDrawer
{
    protected abstract int MaxBytes { get; }
    protected abstract string ReadStr(object o);
    protected abstract object ToFixed(string s);

    private const float BadgeWidth = 52f;
    private const float BadgeMargin = 2f;

    private static GUIStyle _badgeStyle;
    private static GUIStyle _badgeStyleWarn;

    private static void EnsureStyles()
    {
        if (_badgeStyle != null) return;

        _badgeStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.5f, 0.8f, 0.5f) },
            padding = new RectOffset(0, 0, 0, 0),
        };

        _badgeStyleWarn = new GUIStyle(_badgeStyle)
        {
            normal = { textColor = new Color(1f, 0.6f, 0.2f) },
        };
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnsureStyles();

        EditorGUI.BeginProperty(position, label, property);

        string currentStr = ReadStr(property.boxedValue);

        Rect fieldRect = new Rect(position.x, position.y,
                                  position.width - BadgeWidth - BadgeMargin, position.height);
        Rect badgeRect = new Rect(position.xMax - BadgeWidth, position.y,
                                  BadgeWidth, position.height);

        // ── Text field ──────────────────────────────────────────────
        string controlName = $"FixedStr_{property.propertyPath}";
        GUI.SetNextControlName(controlName);

        EditorGUI.BeginChangeCheck();
        string edited = EditorGUI.TextField(fieldRect, label, currentStr);

        if (GUI.GetNameOfFocusedControl() == controlName)
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(
                typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null && Encoding.UTF8.GetByteCount(te.text) > MaxBytes)
            {
                te.text = ClampUtf8(te.text, MaxBytes);
                te.SelectNone();
                te.MoveTextEnd();
                edited = te.text;
                GUI.changed = true;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            edited = ClampUtf8(edited, MaxBytes);

            Undo.RecordObject(property.serializedObject.targetObject, $"Edit {property.name}");
            property.boxedValue = ToFixed(edited);
            property.serializedObject.ApplyModifiedProperties();

            currentStr = edited;
        }

        // ── Badge ────────────────────────────────────────────────────
        int usedBytes = Encoding.UTF8.GetByteCount(currentStr);
        float fillRatio = (float)usedBytes / MaxBytes;
        bool nearLimit = fillRatio > 0.8f;

        GUI.DrawTexture(badgeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                        true, 0, new Color(0.15f, 0.15f, 0.15f, 0.45f), 0, 3f);

        GUIStyle style = nearLimit ? _badgeStyleWarn : _badgeStyle;
        GUI.Label(badgeRect, $"{usedBytes}/{MaxBytes}", style);

        EditorGUI.EndProperty();
    }

    private static string ClampUtf8(string s, int maxBytes)
    {
        if (string.IsNullOrEmpty(s)) return s;
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        if (bytes.Length <= maxBytes) return s;
        int len = maxBytes;
        while (len > 0 && (bytes[len] & 0xC0) == 0x80) len--;
        return Encoding.UTF8.GetString(bytes, 0, len);
    }
}
#endif