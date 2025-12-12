using UnityEngine;
using UnityEngine.UI;

public class AutoGridSize : MonoBehaviour
{
    public bool UseLateUpdate = false;
    private RectTransform ss;
    private GridLayoutGroup yeet;

    public int AxisOfChange = 0;
    /*
     * 0 = X axis
     * 1 = Y axis
     */
    private void Start()
    {
        yeet = GetComponent<GridLayoutGroup>();
        ss = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!UseLateUpdate)
        {
            segs();
        }
    }
    private void LateUpdate()
    {
        if (UseLateUpdate)
        {
            segs();
        }
    }
    private void segs()
    {
        bool e = AxisOfChange == 0;
        float to = e ? ss.rect.width : ss.rect.height;
        float ignore = yeet.spacing.x;
        ignore *= yeet.constraintCount - 1;
        ignore += e ? yeet.padding.left + yeet.padding.right : yeet.padding.top + yeet.padding.bottom;
        var x = to - ignore;
        x /= yeet.constraintCount;
        yeet.cellSize = e ? new Vector2(x, yeet.cellSize.y) : new Vector2(yeet.cellSize.x, x);
    }
}
