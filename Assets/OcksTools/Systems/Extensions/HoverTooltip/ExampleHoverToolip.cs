using TMPro;
using UnityEngine;

public class ExampleHoverToolip : OXTooltip
{
    public override string Name => "Example Tooltip";
    public TextMeshProUGUI Text;

    public override Vector2 CalculateInitial(GameObject spawned)
    {
        var pp = GetRectTransform(spawned);
        Text = spawned.GetComponentInChildren<TextMeshProUGUI>();
        Text.text = "Hovering: " + Holder.htooltip.ExtraCustomData;
        return pp.GetActualSizeOfUI();
    }

    public override GameObject GetPrefab() => SpawnSystem.Instance.Spawnables["ExampleTip"].Object;
}
