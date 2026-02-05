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
    }
}
