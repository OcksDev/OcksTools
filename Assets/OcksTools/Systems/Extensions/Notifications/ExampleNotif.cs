using TMPro;
using UnityEngine;

public class ExampleNotif : Notification
{
    public TextMeshProUGUI Text;
    private int b;
    public ExampleNotif(int ba)
    {
        b = ba;
    }
    public override Vector2 CalculateInitial(GameObject spawned)
    {
        var pp = GetRectTransform(spawned);
        Text = spawned.GetComponentInChildren<TextMeshProUGUI>();
        Text.text = "Your number is: " + b;
        return pp.GetActualSizeOfUI();
    }

    public override GameObject GetPrefab() => SpawnSystem.SpawnableDict["ExampleNotif"].Object;
}
