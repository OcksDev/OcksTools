using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoChildSizer : MonoBehaviour
{
    public float BorderAmount = 15;
    public bool OverrideParent = false;
    public bool UseLateUpdate = false;
    public RectTransform Objecty;
    private RectTransform ss;
    private RectTransform yeet;

    public int AxisOfChange = 2;
    /*
     * 0 = X axis
     * 1 = Y axis
     * 2 = All axis
     */
    void Start()
    {
        if (!OverrideParent)
        {
            ss = transform.parent.GetComponent<RectTransform>();
        }
        else
        {
            ss = Objecty;
        }
        yeet = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!UseLateUpdate)
        {
            segs();
        }
    }
    void LateUpdate()
    {
        if (UseLateUpdate)
        {
            segs();
        }
    }
    private void segs()
    {
        switch (AxisOfChange)
        {
            case 0:
                yeet.sizeDelta = new Vector2(ss.sizeDelta.x + BorderAmount * 2, yeet.sizeDelta.y);
                break;
            case 1:
                yeet.sizeDelta = new Vector2(yeet.sizeDelta.x, ss.sizeDelta.y + BorderAmount * 2);
                break;
            default:
                yeet.sizeDelta = ss.sizeDelta + new Vector2(BorderAmount * 2, BorderAmount * 2);
                break;
        }
    }
}
