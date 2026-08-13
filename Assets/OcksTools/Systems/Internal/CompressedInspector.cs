using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * This entire file is vibe-coded lol
 * I hate dealing with the inspector stuff, I'm letting an AI do this BS for me
 */




public class AutoCompressFieldAttribute : PropertyAttribute { }
public class AutoCompressFieldWithNameAttribute : PropertyAttribute { }
public class SideBySideFieldAttribute : PropertyAttribute { }
public class SideBySideFieldWithNameAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AutoCompressFieldAttribute))]
public class AutoCompressDrawer : AutoCompressedInspector
{
}
[CustomPropertyDrawer(typeof(AutoCompressFieldWithNameAttribute))]
public class AutoCompressWithNameDrawer : AutoCompressedInspectorWithName
{
}
[CustomPropertyDrawer(typeof(SideBySideFieldAttribute))]
public class SideBySideDrawer : AutoCompressedSideBySideInspector
{
}
[CustomPropertyDrawer(typeof(SideBySideFieldWithNameAttribute))]
public class SideBySideWithNameDrawer : AutoCompressedSideBySideInspectorWithName
{
}


public abstract class AutoCompressedInspector : PropertyDrawer
{
    protected virtual bool ShouldDraw(SerializedProperty prop, int parentDepth)
    {
        if (prop.name == "m_Script")
            return false;

        return prop.depth == parentDepth + 1;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            if (!ShouldDraw(iterator, parentDepth))
            {
                enterChildren = false;
                continue;
            }

            float height = EditorGUI.GetPropertyHeight(iterator, true);

            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, height),
                iterator,
                true
            );

            y += height + EditorGUIUtility.standardVerticalSpacing;
            enterChildren = false;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float total = 0f;
        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            if (!ShouldDraw(iterator, parentDepth))
            {
                enterChildren = false;
                continue;
            }

            total += EditorGUI.GetPropertyHeight(iterator, true);
            total += EditorGUIUtility.standardVerticalSpacing;

            enterChildren = false;
        }

        return total;
    }
}
public abstract class CompressedInspector : PropertyDrawer
{
    /// <summary>
    /// Override this to define which relative property names should be drawn.
    /// </summary>
    protected abstract IEnumerable<string> GetPropertyNames(SerializedProperty property);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float width = position.width;

        foreach (var propName in GetPropertyNames(property))
        {
            var child = property.FindPropertyRelative(propName);

            if (child == null)
            {
                EditorGUI.LabelField(
                    new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight),
                    propName,
                    "Not found"
                );
                y += EditorGUIUtility.singleLineHeight;
                continue;
            }

            float height = EditorGUI.GetPropertyHeight(child, true);

            EditorGUI.PropertyField(
                new Rect(position.x, y, width, height),
                child,
                true
            );

            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float total = 0f;

        foreach (var propName in GetPropertyNames(property))
        {
            var child = property.FindPropertyRelative(propName);

            if (child == null)
            {
                total += EditorGUIUtility.singleLineHeight;
            }
            else
            {
                total += EditorGUI.GetPropertyHeight(child, true);
            }

            total += EditorGUIUtility.standardVerticalSpacing;
        }

        return total;
    }
}

public abstract class AutoCompressedInspectorWithName : PropertyDrawer
{
    protected virtual bool ShouldDraw(SerializedProperty prop, int parentDepth)
    {
        if (prop.name == "m_Script")
            return false;

        return prop.depth == parentDepth + 1;
    }

    // 🔹 Helper: get all direct children
    private System.Collections.Generic.List<SerializedProperty> GetChildren(SerializedProperty property)
    {
        var list = new System.Collections.Generic.List<SerializedProperty>();

        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            if (ShouldDraw(iterator, parentDepth))
            {
                list.Add(iterator.Copy());
            }

            enterChildren = false;
        }

        return list;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var children = GetChildren(property);

        // ✅ CASE 1: Single field → inline
        if (children.Count == 1)
        {
            var child = children[0];

            EditorGUI.PropertyField(
                position,
                child,
                label, // 👈 use object label instead of field name
                true
            );

            EditorGUI.EndProperty();
            return;
        }

        // ✅ CASE 2: Multiple fields → your original layout
        float y = position.y;

        Rect labelRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label);

        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.indentLevel++;

        foreach (var child in children)
        {
            float height = EditorGUI.GetPropertyHeight(child, true);

            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, height),
                child,
                true
            );

            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var children = GetChildren(property);

        // ✅ Single field → just return its height
        if (children.Count == 1)
        {
            return EditorGUI.GetPropertyHeight(children[0], true);
        }

        // ✅ Multiple fields → label + children
        float total = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        foreach (var child in children)
        {
            total += EditorGUI.GetPropertyHeight(child, true);
            total += EditorGUIUtility.standardVerticalSpacing;
        }

        return total;
    }
}

public abstract class AutoCompressedSideBySideInspector : PropertyDrawer
{
    // Fixed width given to boolean toggles so they don't get a huge
    // equal-share column full of empty space. Tweak to taste.
    private const float BoolFieldWidth = 16f;
    private const float Spacing = 4f;

    protected virtual bool ShouldDraw(SerializedProperty prop, int parentDepth)
    {
        if (prop.name == "m_Script")
            return false;

        return prop.depth == parentDepth + 1;
    }

    private List<SerializedProperty> GetChildren(SerializedProperty property)
    {
        var list = new List<SerializedProperty>();

        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            if (ShouldDraw(iterator, parentDepth))
            {
                list.Add(iterator.Copy());
            }

            enterChildren = false;
        }

        return list;
    }

    // Computes the width to draw each child at: bools get a small fixed
    // width, and every other field shares the remaining space equally.
    private float[] GetWidths(List<SerializedProperty> children, float totalWidth)
    {
        var widths = new float[children.Count];

        int flexCount = 0;
        float fixedWidth = 0f;

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].propertyType == SerializedPropertyType.Boolean)
            {
                widths[i] = BoolFieldWidth;
                fixedWidth += BoolFieldWidth;
            }
            else
            {
                flexCount++;
            }
        }
        float totalSpacing = Spacing * (children.Count - 1);
        float remaining = Mathf.Max(0f, totalWidth - totalSpacing - fixedWidth);
        float flexWidth = flexCount > 0 ? remaining / flexCount : 0f;

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].propertyType != SerializedPropertyType.Boolean)
            {
                widths[i] = flexWidth;
            }
        }

        return widths;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var children = GetChildren(property);

        if (children.Count == 0)
        {
            EditorGUI.EndProperty();
            return;
        }

        // No parent label, no per-field labels — just boxes in a row.
        int prevIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var widths = GetWidths(children, position.width);

        float x = position.x;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            float w = widths[i];
            float height = EditorGUI.GetPropertyHeight(child, GUIContent.none, true);

            EditorGUI.PropertyField(
                new Rect(x, position.y, w, height),
                child,
                GUIContent.none,
                true
            );

            x += w + Spacing;
        }

        EditorGUI.indentLevel = prevIndent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var children = GetChildren(property);

        if (children.Count == 0)
            return 0f;

        float maxHeight = 0f;

        foreach (var child in children)
        {
            float height = EditorGUI.GetPropertyHeight(child, GUIContent.none, true);
            if (height > maxHeight)
                maxHeight = height;
        }

        return maxHeight;
    }
}

public abstract class AutoCompressedSideBySideInspectorWithName : PropertyDrawer
{
    // Fixed width given to boolean toggles so they don't get a huge
    // equal-share column full of empty space. Tweak to taste.
    private const float BoolFieldWidth = 20f;
    private const float Spacing = 4f;

    protected virtual bool ShouldDraw(SerializedProperty prop, int parentDepth)
    {
        if (prop.name == "m_Script")
            return false;

        return prop.depth == parentDepth + 1;
    }

    private List<SerializedProperty> GetChildren(SerializedProperty property)
    {
        var list = new List<SerializedProperty>();

        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            if (ShouldDraw(iterator, parentDepth))
            {
                list.Add(iterator.Copy());
            }

            enterChildren = false;
        }

        return list;
    }

    // Computes the width to draw each child at: bools get a small fixed
    // width, and every other field shares the remaining space equally.
    private float[] GetWidths(List<SerializedProperty> children, float totalWidth)
    {
        var widths = new float[children.Count];

        int flexCount = 0;
        float fixedWidth = 0f;

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].propertyType == SerializedPropertyType.Boolean)
            {
                widths[i] = BoolFieldWidth;
                fixedWidth += BoolFieldWidth;
            }
            else
            {
                flexCount++;
            }
        }

        float totalSpacing = Spacing * (children.Count - 1);
        float remaining = Mathf.Max(0f, totalWidth - totalSpacing - fixedWidth);
        float flexWidth = flexCount > 0 ? remaining / flexCount : 0f;

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].propertyType != SerializedPropertyType.Boolean)
            {
                widths[i] = flexWidth;
            }
        }

        return widths;
    }

    private void DrawRow(Rect position, List<SerializedProperty> children)
    {
        int prevIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var widths = GetWidths(children, position.width);

        float x = position.x;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            float w = widths[i];
            float height = EditorGUI.GetPropertyHeight(child, GUIContent.none, true);

            EditorGUI.PropertyField(
                new Rect(x, position.y, w, height),
                child,
                GUIContent.none,
                true
            );

            x += w + Spacing;
        }

        EditorGUI.indentLevel = prevIndent;
    }

    private float RowHeight(List<SerializedProperty> children)
    {
        float maxHeight = 0f;

        foreach (var child in children)
        {
            float height = EditorGUI.GetPropertyHeight(child, GUIContent.none, true);
            if (height > maxHeight)
                maxHeight = height;
        }

        return maxHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var children = GetChildren(property);

        if (children.Count == 0)
        {
            EditorGUI.EndProperty();
            return;
        }

        // ✅ CASE 1: Single field → inline, using the object label (same as WithName)
        if (children.Count == 1)
        {
            EditorGUI.PropertyField(
                position,
                children[0],
                label,
                true
            );

            EditorGUI.EndProperty();
            return;
        }

        // ✅ CASE 2: Multiple fields → label as a prefix, boxes on the SAME line
        float rowHeight = RowHeight(children);

        Rect fullRect = new Rect(position.x, position.y, position.width, rowHeight);
        Rect fieldsRect = EditorGUI.PrefixLabel(fullRect, label);

        DrawRow(fieldsRect, children);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var children = GetChildren(property);

        if (children.Count == 0)
            return 0f;

        // ✅ Single field → just its own height
        if (children.Count == 1)
        {
            return EditorGUI.GetPropertyHeight(children[0], true);
        }

        // ✅ Multiple fields → single row height (label is inline now, not a separate line)
        return RowHeight(children);
    }
}

#endif
