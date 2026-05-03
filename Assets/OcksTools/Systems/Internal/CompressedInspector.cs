using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;

        // ✅ Draw the main label
        Rect labelRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label);

        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        int parentDepth = property.depth;

        var iterator = property.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        // ✅ Indent children like Unity normally does
        EditorGUI.indentLevel++;

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

        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float total = 0f;

        // ✅ Account for the label height
        total += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

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


#endif