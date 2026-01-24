using NaughtyAttributes;
using System;
using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float player_height = 2;
    public float player_width = 1;

    public float wall_cameratilt = 1;
    [ReadOnly]
    public Vector3 Vcel;
    private Rigidbody rigid;
    private CapsuleCollider coll;
    public AllowedMovements Movements;
    public MoveState CurrentState = MoveState.Neutral;
    public float move_speed = 2;
    public float jump_str = 2;
    public float input_decay = 0.8f;
    public float slip_decay = 0.8f;
    public float xz_decay = 0.9f;
    public float slide_decay_steep = 0.9f;
    public float slide_decay_flat = 0.9f;
    public float slide_slope_mult = 1f;
    public float air_xz_decay = 0.9f;
    public float air_crouched_xz_decay = 0.9f;
    public float mouse_sense = 1;
    public float grav_str = 2;
    public float air_turn = 0.05f;
    public float max_floor_angle = 45f;
    public float slide_steep_angle = 10f;
    public Transform HeadY;
    public Transform HeadXZ;
    [EnumFlags] public RigidbodyConstraints Normal;
    [EnumFlags] public RigidbodyConstraints Stationary;
    private float slip = -1;
    private Vector3 InitPos;
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        InitPos = transform.position;
        ToggleMouseState(true);
    }

    private int jump_bananas = 0;
    private int ticks_on_ground = 0;
    private int ticks_in_air = 0;
    private bool air_couched = false;
    private void FixedUpdate()
    {
        if (grounded)
        {
            ticks_on_ground++;
            ticks_in_air = 0;
        }
        else
        {
            ticks_in_air++;
            ticks_on_ground = 0;
        }
        jump_bananas--;
        float xzd = xz_decay;
        float airsex = air_xz_decay;
        if (air_couched && ticks_in_air > 2) airsex = air_crouched_xz_decay;
        if (CurrentState == MoveState.Sliding && grounded)
        {
            //Debug.Log("Diff: " + (rigid.linearVelocity.y - Vcel.y));
            if (ticks_on_ground > 5 && Vcel.y - rigid.linearVelocity.y < -2)
            {
                var diff = Mathf.Abs(Vcel.y - rigid.linearVelocity.y);
                var dirr = new Vector3(rigid.linearVelocity.x, 0, rigid.linearVelocity.z).normalized;
                rigid.linearVelocity += dirr * diff * (Vector3.Dot(ground_normal, Vector3.up));
                //Debug.Log("BOOSTED: " + dirr * diff * (Vector3.Dot(ground_normal, Vector3.up)));
            }


            if (Vector3.Angle(ground_normal, Vector3.up) >= slide_steep_angle)
            {
                xzd = slide_decay_steep;
            }
            else
            {
                xzd = slide_decay_flat;
            }
        }
        if (grounded)
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x * xzd, rigid.linearVelocity.y * xzd, rigid.linearVelocity.z * xzd);
            slip *= slip_decay;
        }
        else
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x * airsex, rigid.linearVelocity.y, rigid.linearVelocity.z * airsex);
        }

        Vector3 dir = new Vector3(0, 0, 0);
        if (grounded)
        {
            if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
            if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
            if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
            if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
            if (dir.magnitude > 0.5f)
            {
                slip = 1;
                dir.Normalize();
            }
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
        Vector3 bgalls = Vector3.zero;
        if (dir.magnitude > 0.5f && allow_movement_inputs)
        {
            bgalls += RandomFunctions.PerpendicularTowardDirection(ground_normal, dir) * move_speed * Time.deltaTime * 20;
        }
        if (CurrentState == MoveState.Sliding)
        {
            //float str = 1 - Vector3.Dot(ground_normal, Vector3.up);
            //var dirr = RandomFunctions.PerpendicularTowardDirection(ground_normal, Vector3.down);

            var dirr = Vector3.down * grav_str;
            dirr += ground_normal * grav_str;
            dirr *= slide_slope_mult;
            float min_speed = 0.5f;
            bgalls += dirr;
        }
        rigid.linearVelocity += bgalls;
        if (Movements.HasFlag(AllowedMovements.AntiSlopeSlip))
        {
            if (!grounded) rigid.linearVelocity += Vector3.down * grav_str;
            else rigid.linearVelocity += -ground_normal * grav_str;
        }
        else
        {
            rigid.linearVelocity += Vector3.down * grav_str;
        }
        /*if (Movements.HasFlag(AllowedMovements.AntiSlopeSlip) && grounded)
        {
            if (slip < 0.0005)
            {
                rigid.constraints = Stationary;
            }
            else
            {
                rigid.constraints = Normal;
            }
        }*/
        Vcel = rigid.linearVelocity;
    }
    public bool Jump()
    {
        slip = 1;
        jump_bananas = 1;
        grounded = false;
        var dd = rigid.linearVelocity;
        dd.y = jump_str;
        rigid.linearVelocity = dd;
        SetState(MoveState.Jumping);
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
        if (Movements.HasFlag(AllowedMovements.Slide)) InputBuffer.Instance.BufferListen("slide", "Player", "Slide", 0.1f, false);

        CollisionGroundCheck();


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
        air_couched = false;
        if (Movements.HasFlag(AllowedMovements.Slide))
        {
            var xzy = rigid.linearVelocity;
            var xz = xzy;
            xz.y = 0;
            switch (CurrentState)
            {
                case MoveState.Neutral:
                    if (InputBuffer.Instance.GetBuffer("Slide") && (xz.sqrMagnitude > 0.01f || Vector3.Dot(ground_normal, Vector3.up) < 0.999f) && CurrentState != MoveState.Sliding)
                    {
                        SetState(MoveState.Sliding);
                        InputBuffer.Instance.RemoveBuffer("Slide");
                    }
                    break;
                case MoveState.Jumping:
                    if (InputBuffer.Instance.GetBuffer("Slide"))
                    {
                        air_couched = true;
                        InputBuffer.Instance.RemoveBuffer("Slide");
                    }
                    break;
                case MoveState.Sliding:
                    if (!InputBuffer.Instance.GetBuffer("Slide"))
                    {
                        SetState();
                    }
                    break;
            }
        }



        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleMouseState();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosition();
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
    public void ResetPosition()
    {
        transform.position = InitPos;
    }
    [Flags]
    public enum AllowedMovements
    {
        Jump = 1 << 0,
        Dash = 1 << 1,
        Wallride = 1 << 2,
        Slide = 1 << 3,
        AntiSlopeSlip = 1 << 4,
        Sprint = 1 << 4,
    }
    public enum MoveState
    {
        Neutral,
        Sprinting,
        Jumping,
        Sliding,
        WallRunning,
        Dashing,
    }
    private Vector3 ground_normal = Vector3.zero;
    public void CollisionGroundCheck()
    {
        if (jump_bananas >= 0)
        {
            return;
        }
        var tpos = transform.position - (player_height / 4) * Vector3.up;
        var a = Physics.SphereCastAll(tpos, 0.45f, Vector3.down, 0.1f);
        for (int i = 0; i < a.Length; i++)
        {
            var dd = a[i];
            if (dd.collider.isTrigger) continue;
            if (dd.collider.gameObject == gameObject) continue;
            //Debug.Log($"Hit: {dd.collider.gameObject.name}, {}");
            if (dd.point != Vector3.zero && Vector3.Angle(dd.normal, Vector3.up) <= max_floor_angle)
            {
                ground_normal = dd.normal;
                SetState();
                return;
            }
        }
        if (grounded)
        {
            SetState(MoveState.Jumping);
        }
        return;
    }
    [ReadOnly]
    public bool grounded = false;
    [ReadOnly]
    public bool allow_movement_inputs = false;
    public void SetState(MoveState sta = MoveState.Neutral)
    {
        CurrentState = sta;
        grounded = false;
        allow_movement_inputs = true;
        switch (sta)
        {
            case MoveState.Neutral:
                grounded = true;
                break;
            case MoveState.Sliding:
                grounded = true;
                allow_movement_inputs = false;
                break;
            case MoveState.WallRunning:
                allow_movement_inputs = false;
                break;
        }
    }

    private Vector3 walldir = Vector3.zero;


}
