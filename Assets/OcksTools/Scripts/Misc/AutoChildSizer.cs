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
        if (AxisOfChange != 0) yeet.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ss.rect.size.y - (BorderAmount * 2));
        if (AxisOfChange != 1) yeet.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ss.rect.size.x - (BorderAmount * 2));
    }
}
