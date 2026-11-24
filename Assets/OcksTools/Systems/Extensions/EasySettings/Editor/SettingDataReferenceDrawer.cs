#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Lets any SettingData reference field also accept a BaseSettingInput&lt;T&gt; component
/// (or a GameObject that has one). The component's SettingData is assigned instead.
/// Dragging a SettingData asset directly, or using the object picker, works as before.
/// PLACE THIS FILE INSIDE AN "Editor" FOLDER.
/// </summary>
[CustomPropertyDrawer(typeof(SettingData), true)]
public class SettingDataReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.LabelField(position, label, new GUIContent("Not an object reference"));
            return;
        }

        Type required = GetRequiredType();
        EditorGUI.BeginProperty(position, label, property);

        // 1) Intercept drags of components / GameObjects BEFORE the object field sees them
        //    (the stock field refuses anything that isn't a SettingData).
        Event e = Event.current;
        if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
            && position.Contains(e.mousePosition))
        {
            SettingData resolved = ResolveFromDraggedComponent(required);
            if (resolved != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    property.objectReferenceValue = resolved;
                    GUI.changed = true;
                }
                e.Use();
            }
        }

        // 2) Stock object field: picker + dragging SettingData assets still work.
        EditorGUI.BeginChangeCheck();
        UnityEngine.Object result = EditorGUI.ObjectField(position, label, property.objectReferenceValue, required, false);
        if (EditorGUI.EndChangeCheck())
            property.objectReferenceValue = result;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;

    /// <summary>The exact SettingData subtype this field expects (handles arrays and lists).</summary>
    private Type GetRequiredType()
    {
        Type t = fieldInfo != null ? fieldInfo.FieldType : typeof(SettingData);
        if (t.IsArray) t = t.GetElementType();
        else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            t = t.GetGenericArguments()[0];
        return typeof(SettingData).IsAssignableFrom(t) ? t : typeof(SettingData);
    }

    /// <summary>
    /// Finds a BaseBaseSettingLol (non-generic base of BaseSettingInput&lt;T&gt;) in the drag and
    /// returns its SettingData if it fits the field's type. Returns null for plain SettingData
    /// assets so the stock handling takes over.
    /// </summary>
    private static SettingData ResolveFromDraggedComponent(Type required)
    {
        UnityEngine.Object[] dragged = DragAndDrop.objectReferences;
        if (dragged == null) return null;

        foreach (UnityEngine.Object obj in dragged)
        {
            if (obj == null || obj is SettingData) continue;

            if (obj is BaseBaseSettingLol single)
            {
                SettingData d = single.GetData();
                if (Fits(d, required)) return d;
            }
            else if (obj is GameObject go)
            {
                foreach (BaseBaseSettingLol comp in go.GetComponents<BaseBaseSettingLol>())
                {
                    SettingData d = comp.GetData();
                    if (Fits(d, required)) return d;
                }
            }
        }
        return null;
    }

    private static bool Fits(SettingData data, Type required)
        => data != null && required.IsInstanceOfType(data);
}
#endif
