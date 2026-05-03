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

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AutoCompressFieldAttribute))]
public class AutoCompressDrawer : AutoCompressedInspector
{
}
[CustomPropertyDrawer(typeof(AutoCompressFieldWithNameAttribute))]
public class AutoCompressWithNameDrawer : AutoCompressedInspector
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

#endif