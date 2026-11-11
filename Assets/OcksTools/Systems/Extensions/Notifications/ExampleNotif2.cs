using TMPro;
using UnityEngine;

public class ExampleNotif2 : Notification
{
    public TextMeshProUGUI Text;
    private int b;
    public ExampleNotif2(int ba)
    {
        b = ba;
    }
    public override Vector2 CalculateInitial(GameObject spawned)
    {
        var pp = GetRectTransform(spawned);
        Text = spawned.GetComponentInChildren<TextMeshProUGUI>();
        Text.text = "Your instant is: " + b;
        return pp.GetActualSizeOfUI();
    }
    public override OXAnimationSet GetSpawnAnimation() => null;
    public override OXAnimationSet GetKillAnimation() => null;
    public override GameObject GetPrefab() => SpawnSystem.Instance.Spawnables["ExampleNotif"].Object;
}
