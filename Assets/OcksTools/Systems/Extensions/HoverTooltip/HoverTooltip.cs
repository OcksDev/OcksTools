using UnityEngine;

public class HoverTooltip : MonoBehaviour
{
    public GameObject HoverHitbox;
    public string NameOfTooltip;
    public string ExtraCustomData;
    [NaughtyAttributes.ReadOnly]
    public bool Hovering = false;
    public HoverTooltipManager.TooltipLocation Location = HoverTooltipManager.TooltipLocation.MouseLeftFlip;
    private void OnEnable()
    {
        HoverTooltipManager.Tooltips.Add(this);
    }
    private void OnDisable()
    {
        HoverTooltipManager.Tooltips.Remove(this);
    }
}
