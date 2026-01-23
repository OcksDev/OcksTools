using NaughtyAttributes;
using System;
using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float player_height = 2;
    public float player_width = 1;

    public float wall_cameratilt = 1;
    public Vector3 Vcel;
    private Rigidbody rigid;
    public AllowedMovements Movements;
    public float move_speed = 2;
    public float air_speed = 0.1f;
    public float jump_str = 2;
    public float input_decay = 0.8f;
    public float xz_decay = 0.9f;
    public float air_xz_decay = 0.9f;
    public float mouse_sense = 1;
    public float grav_str = 2;
    public float air_turn = 0.05f;
    public float max_floor_angle = 45f;
    private Vector3 move = new Vector3(0, 0, 0);
    public Transform HeadY;
    public Transform HeadXZ;
    [EnumFlags] public RigidbodyConstraints Normal;
    [EnumFlags] public RigidbodyConstraints Stationary;
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        ToggleMouseState(true);
    }


    private void FixedUpdate()
    {
        if (grounded)
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x * xz_decay, rigid.linearVelocity.y, rigid.linearVelocity.z * xz_decay);
        }
        else
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x * air_xz_decay, rigid.linearVelocity.y, rigid.linearVelocity.z * air_xz_decay);
        }

        Vector3 dir = new Vector3(0, 0, 0);

        move *= input_decay;
        if (grounded)
        {
            if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
            if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
            if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
            if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
            if (dir.magnitude > 0.5f) dir.Normalize();
        }
        else
        {
            if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
            if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
            if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
            if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
            if (dir.magnitude > 0.5f)
            {
                dir.Normalize();
                var xzy = rigid.linearVelocity;
                var xz = xzy;
                xz.y = 0;
                float ang = Vector3.Angle(xz, dir) - 90;
                ang *= -1;

                //ang:
                // +90 = same dir
                // 0 = right angle
                // -90 = opposite dir
                int flipped = 1;
                if (ang < 0)
                {
                    xz *= Mathf.Lerp(1, 0.95f, Mathf.Abs(ang) / 90);
                    ang = 90 + ang;
                }
                else
                {
                    ang = 90 - ang;
                }
                if (Vector3.Dot(xz, Quaternion.Euler(0, 90, 0) * dir) > 0)
                {
                    flipped = -1;
                }
                xz = Quaternion.Euler(0, ang * flipped * air_turn, 0) * xz;

                xzy.x = xz.x;
                xzy.z = xz.z;
                rigid.linearVelocity = xzy;
            }




            dir = Vector3.zero;
        }

        if (dir.magnitude > 0.5f)
        {
            move += dir * move_speed;
        }
        Vector3 bgalls = move * Time.deltaTime * 20;
        rigid.linearVelocity += bgalls;
        rigid.linearVelocity += Vector3.down * grav_str;
        /*if (grounded && move.magnitude < 0.0005)
        {
            rigid.constraints = Stationary;
        }
        else
        {
            rigid.constraints = Normal;
        }*/
        Physics.Simulate(0f);
        Vcel = rigid.linearVelocity;
    }
    public bool Jump()
    {
        grounded = false;
        var dd = rigid.linearVelocity;
        dd.y = jump_str;
        rigid.linearVelocity = dd;
        return true;
    }

    public bool JumpChecked()
    {
        if (!grounded) return false;
        return Jump();
    }
    public GameObject nerd = null;
    private float rot_y = 0;
    private float rot_x = 0;
    private void Update()
    {
        if (Movements.HasFlag(AllowedMovements.Jump)) InputBuffer.Instance.BufferListen("jump", "Player", "Jump", 0.1f);
        CollisionGroundCheck();

        if (InputManager.IsKeyDown(KeyCode.G))
        {
            var g = Instantiate(nerd, HeadY.position + HeadY.forward * 2f, Quaternion.identity);
            g.transform.position = DumbPhysics.SweepCollider(g.GetComponent<BoxCollider>(), HeadY.forward, 10, ~0, out RaycastHit banana);
        }


        var x = Input.GetAxis("Mouse X") * mouse_sense;
        var y = Input.GetAxis("Mouse Y") * mouse_sense;
        rot_x += x;
        rot_y = Mathf.Clamp(rot_y - y, -90, 90);
        HeadY.localRotation = Quaternion.Euler(rot_y, 0, 0);
        HeadXZ.localRotation = Quaternion.Euler(0, rot_x, 0);

        if (Movements.HasFlag(AllowedMovements.Jump))
        {
            if (InputBuffer.Instance.GetBuffer("Jump"))
            {
                var a = JumpChecked();
                if (a)
                {
                    InputBuffer.Instance.RemoveBuffer("Jump");
                }
            }
        }



        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleMouseState();
        }

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Time.timeScale = 0.2f;
        }
        else
        {
            Time.timeScale = 1f;
        }
#endif

    }
    private bool locked = false;
    public void ToggleMouseState(bool? over = null)
    {
        locked = !locked;
        if (over.HasValue) locked = over.Value;
        // Lock the cursor to the center of the game window
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        // Optionally, also hide the cursor
        Cursor.visible = !locked;
    }
    [Flags]
    public enum AllowedMovements
    {
        Jump = 1 << 0,
        Dash = 1 << 1,
        Wallride = 1 << 2,
        Slide = 1 << 3,
        BunnyHop = 1 << 4,
        GroundSnap = 1 << 5,
    }
    public bool grounded = false;
    private Vector3 tpos = Vector3.zero;
    public void CollisionGroundCheck()
    {
        tpos = transform.position - (player_height / 4) * Vector3.up;
        var a = Physics.SphereCastAll(tpos, 0.45f, Vector3.down, 0.1f);
        for (int i = 0; i < a.Length; i++)
        {
            var dd = a[i];
            if (dd.collider.isTrigger) continue;
            if (dd.collider.gameObject == gameObject) continue;
            //Debug.Log($"Hit: {dd.collider.gameObject.name}, {}");
            if (dd.point != Vector3.zero && Vector3.Angle(dd.normal, Vector3.up) <= max_floor_angle)
            {
                grounded = true;
                return;
            }
        }

        grounded = false;
        return;
    }


    private Vector3 walldir = Vector3.zero;


}
