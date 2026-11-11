using System.Collections.Generic;
using UnityEngine;

public class HoverTooltipManager : SingleInstance<HoverTooltipManager>
{
    public Transform HoverTipHolder;
    public static List<HoverTooltip> Tooltips = new();
    public HoverTooltipHolder current_tip = null;
    public Vector2 MousePadding = new Vector2(10f, 10f);
    public Vector2 ViewportPadding = new Vector2(10f, 10f);
    public bool SmoothLocation = true;
    [NaughtyAttributes.ShowIf("SmoothLocation")]
    public float SmoothLocationSpeed = 0.1f;
    private void Update()
    {
        bool shouldkill = true;
        foreach (var a in Tooltips)
        {
            if (Hover.IsHovering(a.HoverHitbox))
            {
                if (current_tip != null)
                {
                    if (current_tip.htooltip != null && current_tip.htooltip == a)
                    {
                        shouldkill = false;
                        break;
                    }
                    current_tip.tooltip.KillMe();
                    current_tip = null;
                }
                current_tip = new HoverTooltipHolder(a.NameOfTooltip, a);
                current_tip.tooltip.AssignDataFromCreator(a);
                current_tip.Publish();
                shouldkill = false;
                break;
            }
        }
        if (current_tip != null)
        {
            if (shouldkill)
            {
                current_tip.tooltip.KillMe();
                current_tip = null;
            }
            else
            {
                PositionTip(current_tip);
            }
        }
    }

    private void PositionTip(HoverTooltipHolder tip)
    {
        var rt = tip.rectTransform;
        var parent = rt.parent as RectTransform;
        if (parent == null) return;

        // Camera is null for Screen Space - Overlay canvases.
        Camera cam = null;
        var canvas = parent.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas = canvas.rootCanvas;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;
        }

        Vector2 mouse = Input.mousePosition;
        Vector2 screen = new Vector2(Screen.width, Screen.height);

        Vector2 screenPoint;
        Vector2 pivot;
        switch (tip.htooltip.Location)
        {
            case TooltipLocation.MouseLeftClamp:
                screenPoint = mouse; pivot = new Vector2(1f, 0.5f); break;
            case TooltipLocation.MouseRightClamp:
                screenPoint = mouse; pivot = new Vector2(0f, 0.5f); break;
            case TooltipLocation.MouseAboveClamp:
                screenPoint = mouse; pivot = new Vector2(0.5f, 0f); break;
            case TooltipLocation.MouseBelowClamp:
                screenPoint = mouse; pivot = new Vector2(0.5f, 1f); break;
            case TooltipLocation.ViewportTopLeft:
                screenPoint = new Vector2(0f, screen.y); pivot = new Vector2(0f, 1f); break;
            case TooltipLocation.ViewportTopRight:
                screenPoint = new Vector2(screen.x, screen.y); pivot = new Vector2(1f, 1f); break;
            case TooltipLocation.ViewportBottomLeft:
                screenPoint = Vector2.zero; pivot = new Vector2(0f, 0f); break;
            case TooltipLocation.ViewportBottomRight:
                screenPoint = new Vector2(screen.x, 0f); pivot = new Vector2(1f, 0f); break;
            default:
                screenPoint = mouse; pivot = new Vector2(0.5f, 0.5f); break;
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = pivot;

        // Screen pixels -> local point in the parent (handles CanvasScaler / camera).
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, cam, out Vector2 local);

        Rect pr = parent.rect;
        Vector2 anchored = local - pr.min;
        switch (tip.htooltip.Location)
        {
            case TooltipLocation.MouseLeftClamp:
                anchored.x -= MousePadding.x; break;
            case TooltipLocation.MouseRightClamp:
                anchored.x += MousePadding.x; break;
            case TooltipLocation.MouseAboveClamp:
                anchored.y += MousePadding.y; break;
            case TooltipLocation.MouseBelowClamp:
                anchored.y -= MousePadding.y; break;
            case TooltipLocation.ViewportTopLeft:
                anchored.x += ViewportPadding.x; anchored.y -= ViewportPadding.y; break;
            case TooltipLocation.ViewportTopRight:
                anchored.x -= ViewportPadding.x; anchored.y -= ViewportPadding.y; break;
            case TooltipLocation.ViewportBottomLeft:
                anchored.x += ViewportPadding.x; anchored.y += ViewportPadding.y; break;
            case TooltipLocation.ViewportBottomRight:
                anchored.x -= ViewportPadding.x; anchored.y += ViewportPadding.y; break;
        }

        // Keep the whole tooltip inside the screen, leaving ViewportPadding at every edge.
        Vector2 size = tip.size;
        float minX = pivot.x * size.x + ViewportPadding.x;
        float maxX = pr.width - (1f - pivot.x) * size.x - ViewportPadding.x;
        float minY = pivot.y * size.y + ViewportPadding.y;
        float maxY = pr.height - (1f - pivot.y) * size.y - ViewportPadding.y;
        anchored.x = Mathf.Clamp(anchored.x, minX, Mathf.Max(minX, maxX));
        anchored.y = Mathf.Clamp(anchored.y, minY, Mathf.Max(minY, maxY));
        if (!tip.hasposset)
        {
            rt.anchoredPosition = anchored;
            tip.hasposset = true;
        }
        else
        {
            rt.anchoredPosition = rt.anchoredPosition.LerpT(anchored, SmoothLocationSpeed);
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

    public enum TooltipLocation
    {
        MouseLeftFlip,
        MouseRightFlip,
        MouseAboveFlip,
        MouseBelowFlip,
        MouseLeftClamp,
        MouseRightClamp,
        MouseAboveClamp,
        MouseBelowClamp,
        ViewportTopLeft,
        ViewportTopRight,
        ViewportBottomLeft,
        ViewportBottomRight
    }
}

public class HoverTooltipHolder
{
    public HoverTooltip htooltip;
    public HoverTooltipHolder(string a, HoverTooltip b)
    {
        htooltip = b;
        tooltip = OXFactory.Create<OXTooltip>(a);
        tooltip.Holder = this;
        tooltip.Nerd = HoverTooltipManager.Instance.FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(tooltip.GetPrefab());
        rectTransform = tooltip.GetRectTransform(tooltip.Nerd);
    }
    public OXTooltip tooltip;
    public Vector2 size;
    public RectTransform rectTransform;
    public bool hasposset = false;
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
