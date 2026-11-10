using UnityEngine;

public class HoverTooltip : MonoBehaviour
{
    public GameObject HoverHitbox;
    public string NameOfTooltip;
    public string ExtraCustomData;
    private void OnEnable()
    {
        HoverTooltipManager.Tooltips.Add(this);
    }
    private void OnDisable()
    {
        HoverTooltipManager.Tooltips.Remove(this);
    }
}
