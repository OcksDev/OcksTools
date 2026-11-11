using UnityEngine;

public class TestNotifs : MonoBehaviour
{
    private int x = 0;
    private void Update()
    {
        if (InputManager.IsKeyDown(KeyCode.Space))
        {
            x++;
            NotificationSystem.Instance.AddNotif(new ExampleNotif(x).Duration(2));
        }
        if (InputManager.IsKeyDown(KeyCode.M))
        {
            x++;
            NotificationSystem.Instance.AddNotif(new ExampleNotif2(x).Duration(2));
        }
    }
}
