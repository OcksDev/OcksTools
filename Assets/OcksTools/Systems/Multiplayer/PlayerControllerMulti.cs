using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking;
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
    void FixedUpdate()
    {
        if (!Owner) return;
        move *= 0.8f;
        Vector3 dir = new Vector3(0, 0, 0);
        if (InputManager.IsKey(InputManager.gamekeys["move_forward"])) dir += Vector3.up;
        if (InputManager.IsKey(InputManager.gamekeys["move_back"])) dir += Vector3.down;
        if (InputManager.IsKey(InputManager.gamekeys["move_right"])) dir += Vector3.right;
        if (InputManager.IsKey(InputManager.gamekeys["move_left"])) dir += Vector3.left;
        if (dir.magnitude > 0.5f)
        {
            dir.Normalize();
            move += dir;
        }
        Vector3 bgalls = move * Time.deltaTime * move_speed * 20;
        rigid.velocity += new Vector2(bgalls.x, bgalls.y);
        if (CameraLol.Instance != null)
        {
            CameraLol.Instance.targetpos = transform.position;
        }

    }
}
