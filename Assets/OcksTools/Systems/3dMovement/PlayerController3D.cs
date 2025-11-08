using Codice.CM.Common;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class PlayerController3D : MonoBehaviour
{
    public float player_height = 2;
    public float player_width = 1;

    public float wall_cameratilt = 1;

    private Rigidbody rigid;
    public AllowedMovements Movements;
    public float move_speed = 2;
    public float air_speed = 0.1f;
    public float jump_str = 2;
    public float decay = 0.8f;
    public float air_decay = 0.98f;
    public float xz_decay = 0.9f;
    public float mouse_sense = 1;
    public float grav_str = 2;
    public float wall_grav_str = 2;
    public float max_dot = 0.5f;
    public float air_str = 2;
    public float wall_boost_str = 6;
    public float wall_leave_boost_str = 6;
    private Vector3 move = new Vector3(0, 0, 0);
    public Transform HeadY;
    public Transform HeadXZ;
    [EnumFlags] public RigidbodyConstraints Normal;
    [EnumFlags] public RigidbodyConstraints Stationary;
    private void Start()
    {
        rigid= GetComponent<Rigidbody>();
        ToggleMouseState(true);
    }

    private bool im_walling_it = false;
    private Vector3 wall_direction = default;
    bool waswallin = false;

    void FixedUpdate()
    {
        rigid.linearVelocity = new Vector3(rigid.linearVelocity.x * xz_decay, rigid.linearVelocity.y, rigid.linearVelocity.z * xz_decay);
        if (grounded)
        {
            rigid.useGravity = false;
        }
        else
        {
            if (waswallin && !im_walling_it)
            {
                rigid.useGravity = true;
                waswallin = false;
            }
            else if ((!waswallin && im_walling_it))
            {
                rigid.useGravity = false;
                waswallin = true;
            }
        }

        Vector3 dir = new Vector3(0, 0, 0);
        if (grounded || !Movements.HasFlag(AllowedMovements.BunnyHop))
        {
            move *= decay;
            if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
            if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
            if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
            if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
            if(dir.magnitude > 0.5f) dir.Normalize();
        }
        else
        {
            move *= air_decay;
            if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
            if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
            if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
            if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;

            if (InputManager.IsKey("move_forward", "Player"))
            {
                var dd = Vector3.Dot(dir, Quaternion.Euler(0,90,0) * move.normalized);
                move = Quaternion.Euler(0, dd * air_str, 0) * move;
            }
            if (InputManager.IsKey("move_back", "Player"))
            {
                var dd = Vector3.Dot(dir, Quaternion.Euler(0,-90,0) * move.normalized);
                move = Quaternion.Euler(0, -dd * air_str, 0) * move;
            }
            if (InputManager.IsKey("move_right", "Player"))
            {
                var dd = Vector3.Dot(dir, Quaternion.Euler(0,90,0) * move.normalized);
                move = Quaternion.Euler(0, dd * air_str, 0) * move;
            }
            if (InputManager.IsKey("move_left", "Player"))
            {
                var dd = Vector3.Dot(dir, Quaternion.Euler(0,-90,0) * move.normalized);
                move = Quaternion.Euler(0, -dd * air_str, 0) * move;
            }
        }
        if(dir.magnitude > 0.5f)
        {
            move += dir * (grounded? move_speed :air_speed);
        }
        Vector3 bgalls = move * Time.deltaTime * 20;
        rigid.linearVelocity += bgalls;
        rigid.linearVelocity += Vector3.down * (im_walling_it? wall_grav_str : grav_str);
        if (grounded && move.magnitude < 0.0005)
        {
            rigid.constraints = Stationary;
        }
        else
        {
            rigid.constraints = Normal;
        }
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

    private float rot_y = 0;
    private float rot_x = 0;
    private void Update()
    {
        if (Movements.HasFlag(AllowedMovements.Jump)) InputBuffer.Instance.BufferListen("jump", "Player", "Jump", 0.1f);
        CollisionGroundCheck();
        WallCheck();




        var x = Input.GetAxis("Mouse X") * mouse_sense;
        var y = Input.GetAxis("Mouse Y") * mouse_sense;
        rot_x += x;
        rot_y = Mathf.Clamp(rot_y-y,-90,90);
        HeadY.localRotation = Quaternion.Euler(rot_y, 0, 0);
        HeadXZ.localRotation = Quaternion.Euler(0, rot_x, 0);

        if (Movements.HasFlag(AllowedMovements.Jump) && !im_walling_it)
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
    bool locked = false;
    public void ToggleMouseState(bool? over = null)
    {
        locked = !locked;
        if (over.HasValue) locked = over.Value;
        // Lock the cursor to the center of the game window
        Cursor.lockState = locked?CursorLockMode.Locked:CursorLockMode.None;
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
    public void CollisionGroundCheck()
    {
        var a = Physics.RaycastAll(transform.position, Vector3.down, (player_height/2) + 0.1f);
        for(int i = 0; i < a.Length; i++)
        {
            var dd = a[i];
            if(dd.collider.isTrigger) continue;
            if (dd.collider.gameObject == gameObject) continue;
            if (Vector3.Dot(dd.normal, Vector3.up) >= max_dot)
            {
                grounded = true;
                return;
            }
        }

        grounded = false;
        return;
    }
    private GameObject LastWallRidden = null;
    private Vector3 walldir = Vector3.zero;


    public void WallCheck()
    {
        if (!Movements.HasFlag(AllowedMovements.Wallride)) return;
        if (grounded)
        {
            im_walling_it = false;
            LastWallRidden = null;
            walldir = Vector3.down;
            return;
        }

        var gg = rigid.linearVelocity;
        var imgg = new Vector3(0, gg.y, 0);
        var imxz = new Vector3(gg.x, 0, gg.z);

        if (imxz.magnitude <= 1 || Vector3.Dot(imxz.normalized, HeadXZ.forward) < 0)
        {
            im_walling_it = false;
            LastWallRidden = null;
            return;
        }

        var a = Physics.RaycastAll(transform.position, HeadY.right, (player_width / 2) + 0.1f);
        var aa = Physics.RaycastAll(transform.position, HeadY.right*-1, (player_width / 2) + 0.1f);


        var curpos = transform.position;
        Action<RaycastHit> banana = (dd) =>
        {
            var altered = Quaternion.Euler(0, 90, 0) * dd.normal;

            if (Vector3.Dot(altered, move.normalized) < 0f)
            {
                altered *= -1;
            }

            if (!im_walling_it && LastWallRidden != dd.collider.gameObject)
            {
                if (Vector3.Dot(dd.normal, walldir) < 0.8f) imgg.y = wall_boost_str + imgg.y/2;
            }
            move = altered * move.magnitude;
            rigid.linearVelocity = imgg + (altered * imxz.magnitude);
            LastWallRidden = dd.collider.gameObject;
            im_walling_it = true;

            if (InputBuffer.Instance.GetBuffer("Jump"))
            {
                if(Vector3.Dot(dd.normal, walldir) < 0.8f) Jump();
                InputBuffer.Instance.RemoveBuffer("Jump");
                im_walling_it = false;
                walldir = dd.normal;
                Vector3 dir = Vector3.zero;
                if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
                if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
                if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
                if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;

                rigid.linearVelocity += dd.normal * wall_leave_boost_str;
            }

        };

        for (int i = 0; i < a.Length; i++)
        {
            var dd = a[i];
            if(dd.collider.isTrigger) continue;
            if (dd.collider.gameObject == gameObject) continue;
            if (!im_walling_it && Vector3.Dot(dd.normal, imxz.normalized) <= 0) LastWallRidden = null;
            if (!im_walling_it && dd.collider.gameObject == LastWallRidden) continue;
            if(Vector3.Dot(dd.normal, HeadY.right * -1) >= 0.45f)
            {
                banana(dd);
                return;
            }
        }
        for (int i = 0; i < aa.Length; i++)
        {
            var dd = aa[i];
            if(dd.collider.isTrigger) continue;
            if (dd.collider.gameObject == gameObject) continue;
            if (!im_walling_it && Vector3.Dot(dd.normal, imxz.normalized) <= 0) LastWallRidden = null;
            if (!im_walling_it && dd.collider.gameObject == LastWallRidden) continue;
            if (Vector3.Dot(dd.normal, HeadY.right) >= 0.45f)
            {
                banana(dd);
                return;
            }
        }
        im_walling_it = false;
        return;
    } 
}
