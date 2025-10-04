using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigid;
    public float move_speed = 2;
    public float decay = 0.8f;
    private Vector3 move = new Vector3(0, 0, 0);
    private void Start()
    {
        rigid= GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        move *= decay;
        Vector3 dir = new Vector3(0, 0, 0);
        if (InputManager.IsKey("move_forward", "Player")) dir += Vector3.up;
        if (InputManager.IsKey("move_back", "Player")) dir += Vector3.down;
        if (InputManager.IsKey("move_right", "Player")) dir += Vector3.right;
        if (InputManager.IsKey("move_left", "Player")) dir += Vector3.left;
        if(dir.magnitude > 0.5f)
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
