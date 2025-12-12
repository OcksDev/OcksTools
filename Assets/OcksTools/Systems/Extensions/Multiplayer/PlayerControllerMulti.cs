using Unity.Netcode;
using UnityEngine;

public class PlayerControllerMulti : NetworkBehaviour
{
    private Rigidbody2D rigid;
    public float move_speed = 2;
    public bool Owner;
    public Vector3 move = new Vector3(0, 0, 0);
    // Start is called before the first frame update
    private void Start()
    {
        Owner = GetComponent<NetworkObject>().IsOwner;
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!Owner) return;
        move *= 0.8f;
        Vector3 dir = new Vector3(0, 0, 0);
        if (InputManager.IsKey("move_forward")) dir += Vector3.up;
        if (InputManager.IsKey("move_back")) dir += Vector3.down;
        if (InputManager.IsKey("move_right")) dir += Vector3.right;
        if (InputManager.IsKey("move_left")) dir += Vector3.left;
        if (dir.magnitude > 0.5f)
        {
            dir.Normalize();
            move += dir;
        }
        Vector3 bgalls = move * Time.deltaTime * move_speed * 20;
        rigid.linearVelocity += new Vector2(bgalls.x, bgalls.y);
        if (CameraLol.Instance != null)
        {
            CameraLol.Instance.targetpos = transform.position;
        }

    }
}
