using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigid;
    public float move_speed = 2;
    private Vector3 move = new Vector3(0, 0, 0);
    private void Start()
    {
        rigid= GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        move *= 0.8f;
        Vector3 dir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.S)) dir += Vector3.down;
        if (Input.GetKey(KeyCode.D)) dir += Vector3.right;
        if (Input.GetKey(KeyCode.A)) dir += Vector3.left;
        if(dir.magnitude > 0.5f)
        {
            dir.Normalize();
            move += dir;
        }
        Vector3 bgalls = move * Time.deltaTime * move_speed * 20;
        rigid.velocity += new Vector2(bgalls.x, bgalls.y);
        if (CameraLol.Instance != null)
        {
            CameraLol.Instance.pos = transform.position;
        }

    }
}
