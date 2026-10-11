#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws an OXKeyframeChannel as a foldout that only shows the easing parameters
/// the selected interpolation mode actually uses.
/// Wrapped in UNITY_EDITOR, so it can live anywhere, but an Editor folder is tidier.
/// </summary>
[CustomPropertyDrawer(typeof(OXKeyframeChannel))]
public class OXKeyframeChannelDrawer : PropertyDrawer
{
    /// <summary>Which OXKeyframeChannel fields each mode reads. Keep in sync with OXKeyframeChannel.Evaluate.</summary>
    private static string[] RelevantFields(OXKeyframeInterpolationMode mode)
    {
        switch (mode)
        {
            case OXKeyframeInterpolationMode.In:
            case OXKeyframeInterpolationMode.Out:
            case OXKeyframeInterpolationMode.InAndOut:
                return new[] { nameof(OXKeyframeChannel.Power) };
            case OXKeyframeInterpolationMode.CircIn:
            case OXKeyframeInterpolationMode.CircOut:
                return new[] { nameof(OXKeyframeChannel.CircPower) };
            case OXKeyframeInterpolationMode.Bounce:
                return new[] { nameof(OXKeyframeChannel.Bounces), nameof(OXKeyframeChannel.BouncePower) };
            case OXKeyframeInterpolationMode.Elastic:
                return new[] { nameof(OXKeyframeChannel.Oscillations) };
            case OXKeyframeInterpolationMode.Overshoot:
                return new[] { nameof(OXKeyframeChannel.Magnification), nameof(OXKeyframeChannel.OvershootPower) };
            default: // Linear, Sin, Cos, SinInAndOut take no parameters
                return new string[0];
        }
    }

    private static OXKeyframeInterpolationMode GetMode(SerializedProperty property)
    {
        var mode = property.FindPropertyRelative(nameof(OXKeyframeChannel.InterpMode));
        return (OXKeyframeInterpolationMode)mode.intValue;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (!property.isExpanded) return line;

        int rows = 2 + RelevantFields(GetMode(property)).Length; // Enabled + InterpMode + parameters
        return line * (1 + rows);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var enabled = property.FindPropertyRelative(nameof(OXKeyframeChannel.Enabled));
        var mode = property.FindPropertyRelative(nameof(OXKeyframeChannel.InterpMode));

        float h = EditorGUIUtility.singleLineHeight;
        float step = h + EditorGUIUtility.standardVerticalSpacing;
        var row = new Rect(position.x, position.y, position.width, h);

        string title = enabled.boolValue ? label.text : label.text + " (off)";
        property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, title, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            row.y += step;
            EditorGUI.PropertyField(row, enabled);

            // Grey out the rest while the channel is opted out; values are still editable-looking but clearly inactive.
            using (new EditorGUI.DisabledScope(!enabled.boolValue))
            {
                row.y += step;
                EditorGUI.PropertyField(row, mode);

                foreach (var fieldName in RelevantFields(GetMode(property)))
                {
                    row.y += step;
                    EditorGUI.PropertyField(row, property.FindPropertyRelative(fieldName));
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
#endif
