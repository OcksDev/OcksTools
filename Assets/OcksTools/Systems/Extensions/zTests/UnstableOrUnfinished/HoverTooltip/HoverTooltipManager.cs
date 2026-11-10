using System.Collections.Generic;
using UnityEngine;

public class HoverTooltipManager : SingleInstance<HoverTooltipManager>
{
    public Transform HoverTipHolder;
    public static List<HoverTooltip> Tooltips = new();
    public HoverTooltipHolder current_tip = null;
    private void Update()
    {
        foreach (var a in Tooltips)
        {
            if (Hover.IsHovering(a.HoverHitbox))
            {
                if (current_tip != null)
                {
                    if (current_tip.htooltip != null && current_tip.htooltip == a)
                    {
                        break;
                    }
                    current_tip.tooltip.KillMe();
                    current_tip = null;
                }
                current_tip = new HoverTooltipHolder(a.NameOfTooltip, a);
                current_tip.tooltip.AssignDataFromCreator(a);
                current_tip.Publish();
                break;
            }
        }
    }
    public override void Awake2()
    {
        OXFactory.DefineForInheritorsOf<OXTooltip>(x => x.Name);
    }
    public void Kill(GameObject a)
    {
        Destroy(a);
    }

    public GameObject FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(GameObject a)
    {
        var d = Vector3.one * 100000;
        d.z = HoverTipHolder.position.z;
        return Instantiate(a, d, Quaternion.identity, HoverTipHolder);
    }
}

public class HoverTooltipHolder
{
    public HoverTooltip htooltip;
    public HoverTooltipHolder(string a, HoverTooltip b)
    {
        htooltip = b;
        tooltip = OXFactory.Create<OXTooltip>(a);
        tooltip.Nerd = HoverTooltipManager.Instance.FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(tooltip.GetPrefab());
        rectTransform = tooltip.GetRectTransform(tooltip.Nerd);
    }
    public OXTooltip tooltip;
    public Vector2 size;
    public RectTransform rectTransform;
    public void Publish()
    {
        size = tooltip.CalculateInitial(tooltip.Nerd);
    }
}


public abstract class OXTooltip : SizedUIElement
{
    public HoverTooltipHolder Holder;
    public abstract string Name { get; }
    public override void KillMe() => HoverTooltipManager.Instance.Kill(Nerd);
    public virtual void AssignDataFromCreator(HoverTooltip creator) { }
}