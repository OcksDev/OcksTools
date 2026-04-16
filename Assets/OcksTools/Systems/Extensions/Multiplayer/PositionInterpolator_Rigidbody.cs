using UnityEngine;

public class PositionInterpolator_Rigidbody : MonoBehaviour
{
    public Rigidbody Tracker;
    public float Amount = 0.5f;
    private Vector3 pos;
    private void Awake()
    {
        pos = Tracker.position;
    }
    private void LateUpdate()
    {
        pos = Vector3.Lerp(pos, Tracker.position, Amount.TimeStableLerp());
        transform.position = pos;
    }
}
