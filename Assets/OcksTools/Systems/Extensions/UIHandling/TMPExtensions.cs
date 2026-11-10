using TMPro;
using UnityEngine;

// super AI generated file lol


public static class TMPExtensions
{
    /// <summary>
    /// Resizes the RectTransform of a TextMeshProUGUI so it tightly fits its text.
    /// </summary>
    /// <param name="tmp">The text component.</param>
    /// <param name="maxWidth">Maximum width before text wraps. Use infinity for no wrapping.</param>
    /// <param name="maxHeight">Maximum height. Use infinity for no limit.</param>
    /// <param name="padding">Extra padding added on top of the text's own margins (x = horizontal total, y = vertical total).</param>
    /// <param name="fitWidth">Whether to resize the width.</param>
    /// <param name="fitHeight">Whether to resize the height.</param>
    /// <returns>The new size of the rect.</returns>
    public static Vector2 FitToText(
        this TextMeshProUGUI tmp,
        float maxWidth = float.PositiveInfinity,
        float maxHeight = float.PositiveInfinity,
        Vector2 padding = default,
        bool fitWidth = true,
        bool fitHeight = true)
    {
        if (tmp == null) return Vector2.zero;

        // Make sure text info is up to date (e.g. right after setting .text).
        tmp.ForceMeshUpdate();

        // Account for the component's own margins (left, top, right, bottom).
        Vector4 margin = tmp.margin;
        float marginX = margin.x + margin.z;
        float marginY = margin.y + margin.w;

        // Available space for the text itself, excluding margins/padding.
        float availW = float.IsInfinity(maxWidth) ? maxWidth : Mathf.Max(0f, maxWidth - marginX - padding.x);
        float availH = float.IsInfinity(maxHeight) ? maxHeight : Mathf.Max(0f, maxHeight - marginY - padding.y);

        // Preferred size of the text given the constraints (wraps if width is limited).
        Vector2 preferred = tmp.GetPreferredValues(tmp.text, availW, availH);

        float width = Mathf.Min(preferred.x + marginX + padding.x, maxWidth);
        float height = Mathf.Min(preferred.y + marginY + padding.y, maxHeight);

        RectTransform rt = tmp.rectTransform;

        if (fitWidth)
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        if (fitHeight)
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        return new Vector2(
            fitWidth ? width : rt.rect.width,
            fitHeight ? height : rt.rect.height);
    }

    /// <summary>
    /// Fits only the height to the text at the current width (handy for wrapped paragraphs).
    /// </summary>
    public static float FitHeightToText(this TextMeshProUGUI tmp, float verticalPadding = 0f)
    {
        float currentWidth = tmp.rectTransform.rect.width;
        return tmp.FitToText(
            maxWidth: currentWidth,
            padding: new Vector2(0f, verticalPadding),
            fitWidth: false,
            fitHeight: true).y;
    }
}
