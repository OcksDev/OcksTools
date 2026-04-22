using UnityEngine;

public class TestShake : MonoBehaviour
{
    private ShakeHolder shakeHolder = new ShakeHolder();
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) shakeHolder.Add(new Shake(1, 1, 0.95f, Shake.RandomType.Circle));
        if (Input.GetKeyDown(KeyCode.Alpha2)) shakeHolder.Add(new Shake(1, 0.3f, 0.95f, Shake.RandomType.Circle));
        if (Input.GetKeyDown(KeyCode.Alpha3)) shakeHolder.Add(new Shake(1, 0.6f, 0.95f, Shake.RandomType.Circle));
        if (Input.GetKeyDown(KeyCode.Alpha4)) shakeHolder.Add(new Shake(1, 2f, 0.95f, Shake.RandomType.Circle));
        transform.position = shakeHolder.GetPos(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.Alpha5)) shakeHolder.Add(new Shake(1, 5f, 0.95f, Shake.RandomType.Circle));
        transform.position = shakeHolder.GetPos(Time.deltaTime);
    }
}
