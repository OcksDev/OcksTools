using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public GameObject fard;
    private Rigidbody2D rig;
    public float strength = 1f;
    public float up_help = 1.2f;
    public float sw_max_vel = 1f;
    public float rg_max_vel = 10f;
    public float movespeed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // the vector3 called "b" is the target position, so you just change this to change the swing point
        var b = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var a = b - transform.position;
        a *= strength;
        if (a.magnitude > sw_max_vel) a = a.normalized * sw_max_vel;
        if (a.y > 0) a.y *= up_help;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            rig.velocity += new Vector2(a.x, a.y);
            var c = b;
            c.z = 0;
            Debug.DrawLine(transform.position, c, new Color32(255, 255, 255, 255), 0.02f);
        }
        if(Mathf.Abs(rig.velocity.x) > rg_max_vel)
        {
            var bb = rig.velocity;
            bb.x *= 0.97f;
            rig.velocity = bb;
        }
        Vector3 m = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            m -= Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            m += Vector3.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            m += Vector3.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            m -= Vector3.up;
        }
        m *= movespeed;
        rig.velocity += new Vector2(m.x, m.y);
    }
}
