using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float player_height = 2;
    public float player_width = 1;

    [ReadOnly]
    public Vector3 Vcel;
    private Rigidbody rigid;
    private CapsuleCollider coll;
    public AllowedMovements Movements;
    public MoveState CurrentState = MoveState.Neutral;
    public float mouse_sense = 1;
    public float move_speed = 2;
    public float jump_str = 2;
    public float grav_str = 2;
    public float air_turn = 0.05f;
    public float slip_decay = 0.8f;
    public float xz_decay = 0.9f;
    public float slide_decay_steep = 0.9f;
    public float slide_decay_flat = 0.9f;
    public float slide_slope_mult = 1f;
    public float slide_min_time = 0.5f;
    public float air_xz_decay = 0.9f;
    public float air_crouched_xz_decay = 0.9f;
    public float max_floor_angle = 45f;
    public float slide_steep_angle = 10f;
    public float fast_punish = 0.9f;
    public float fast_punish_speed = 10f;
    public float dash_str = 20;
    public float dash_dur = 2;
    public float dash_end_str = 2;
    public float wall_grav_mod = 0.5f;
    public float wall_up_str = 0.5f;
    public float wall_velocity_add = 0.5f;
    public float wall_orig_up_perc = 0.5f;
    public float wall_leave_force = 10f;
    public Transform HeadY;
    public Transform HeadXZ;
    [EnumFlags] public RigidbodyConstraints Normal;
    [EnumFlags] public RigidbodyConstraints Stationary;
    private float slip = -1;
    private Vector3 InitPos;
    private EntityOXS entity;
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        InitPos = transform.position;
        entity = new EntityOXS();
        SkillManager.Instance.AllSkills["DashSkill"].OnSkillActivation.Append(Dash);
        entity.Skill().Add(new Skill("DashSkill"));
        ToggleMouseState(true);
    }

    private int jump_bananas = 0;
    private int ticks_on_ground = 0;
    private int ticks_in_air = 0;
    private int ticks_wall_cooldown = 0;
    private int wall_shungle = 0;
    private int wall_cur_shungle = 0;
    private float wall_shungle_consecutive = 1;
    private bool air_couched = false;

    public void ApplyVelocity(Vector3 force)
    {
        rigid.linearVelocity += force;
        Vcel += force;
    }

    public Coroutine ApplyVelocityOverTime(Vector3 force, float time)
    {
        return ApplyVelocityOverTime((x) => { return force; }, time);
    }

    public Coroutine ApplyVelocityOverTime(Func<float, Vector3> force_func, float time)
    {
        return StartCoroutine(OXLerp.LinearFixed((x) =>
        {
            var force = force_func(x);
            rigid.linearVelocity += force;
            Vcel += force;
        }, time));
    }
    private Vector3 start_dash_vel = Vector3.zero;
    private Vector3 move_dir = Vector3.zero;
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
        if (air_couched && ticks_in_air > 4) airsex = air_crouched_xz_decay;
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
        if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
        if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
        if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
        if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
        move_dir = dir;
        if (allow_movement_inputs)
        {

        }
        if (grounded)
        {
            if (dir.magnitude > 0.5f)
            {
                slip = 1;
                dir.Normalize();
            }
        }
        else
        {
            if (dir.magnitude > 0.5f && allow_movement_inputs)
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
            bgalls += ground_normal.PerpendicularTowardDirection(dir) * move_speed * Time.deltaTime * 20;
        }

        float grav_str = this.grav_str;
        if (CurrentState == MoveState.WallRunning)
        {
            grav_str *= wall_grav_mod;
        }


        if (CurrentState == MoveState.Sliding)
        {
            //float str = 1 - Vector3.Dot(ground_normal, Vector3.up);
            //var dirr = RandomFunctions.PerpendicularTowardDirection(ground_normal, Vector3.down);

            var dirr = Vector3.down * grav_str;
            dirr += ground_normal * grav_str;
            dirr *= slide_slope_mult;
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

        switch (CurrentState)
        {
            case MoveState.WallRunning:
            case MoveState.Dashing:
            case MoveState.Jumping:
                if (wall_shungle == -1)
                {
                    Debug.Log("Blocking RIGHT");
                }
                else if (wall_shungle == 1)
                {
                    Debug.Log("Blocking LEFT");
                }
                var a = Physics.RaycastAll(transform.position, HeadY.right, (player_width / 2) + 0.15f);
                var aa = Physics.RaycastAll(transform.position, HeadY.right * -1, (player_width / 2) + 0.15f);
                bool riding = false;
                RaycastHit hit = default;
                bool boosteleibibi = false;
                Action<RaycastHit[], int> bana = (x, y) =>
                {
                    foreach (var dd in x)
                    {
                        if (dd.collider.isTrigger) continue;
                        if (dd.collider.gameObject == gameObject) continue;
                        if (dd.point == Vector3.zero) continue;
                        riding = true;
                        hit = dd;
                        boosteleibibi = y != wall_cur_shungle;
                        wall_cur_shungle = y;
                        if (!boosteleibibi)
                        {
                            if (CurrentState != MoveState.WallRunning) wall_shungle_consecutive *= 2f;
                        }
                        else wall_shungle_consecutive = 1;
                        break;
                    }
                };

                var d = rigid.linearVelocity;
                var xz = d;
                xz.y = 0;
                if (Vector3.Angle(xz, HeadXZ.forward) < 45)
                {
                    if (!riding && wall_shungle >= 0) bana(a, -1);
                    if (!riding && wall_shungle <= 0) bana(aa, 1);
                }
                if (ticks_wall_cooldown <= 0)
                {
                    wall_shungle = 0;
                }
                else
                {
                    ticks_wall_cooldown--;
                }
                if (riding)
                {
                    wall_normal = hit.normal;
                    if (CurrentState == MoveState.Dashing)
                    {
                        var newdir = wall_normal;
                        newdir.y = 0;
                        newdir = newdir.PerpendicularTowardDirection(xz);
                        StopCoroutine(Dc);
                        EndDash(newdir);
                        d = rigid.linearVelocity;
                        xz = d;
                        xz.y = 0;
                    }
                    switch (CurrentState)
                    {
                        case MoveState.WallRunning:
                            break;
                        case MoveState.Jumping:
                        case MoveState.Neutral:
                            SetState(MoveState.WallRunning);
                            if (boosteleibibi) d.y = wall_up_str + (d.y * wall_orig_up_perc);
                            else d.y = (d.y * wall_orig_up_perc);
                            var newdir = wall_normal;
                            newdir.y = 0;
                            newdir = newdir.PerpendicularTowardDirection(xz);
                            if (boosteleibibi) newdir *= (xz.magnitude + wall_velocity_add / Mathf.Clamp(xz.magnitude, 1, 1000) * 10);
                            else newdir *= xz.magnitude;
                            d.x = newdir.x;
                            d.z = newdir.z;
                            rigid.linearVelocity = d;
                            break;

                    }
                }
                else
                {

                    switch (CurrentState)
                    {
                        case MoveState.WallRunning:
                            wall_shungle = wall_cur_shungle;
                            ticks_wall_cooldown = 10;
                            SetState(MoveState.Jumping);
                            break;

                    }
                }
                break;
        }

        Vcel = rigid.linearVelocity;
    }
    public bool Jump()
    {
        slip = 1;
        jump_bananas = 1;
        grounded = false;
        var dd = rigid.linearVelocity;
        switch (CurrentState)
        {
            case MoveState.Sliding:
                dd.y = 0;
                dd += ground_normal * jump_str;
                break;
            case MoveState.WallRunning:
                wall_shungle = wall_cur_shungle;
                ticks_wall_cooldown = 5;
                var d = wall_normal;
                d.y = 0;
                dd += d.normalized * wall_leave_force;
                dd.y = jump_str / wall_shungle_consecutive;
                break;
            default:
                dd.y = jump_str;
                break;
        }
        if (ticks_on_ground < 2)
        {
            if (rigid.linearVelocity.magnitude > (fast_punish_speed / fast_punish))
            {
                dd.x *= fast_punish;
                dd.z *= fast_punish;
            }
        }
        rigid.linearVelocity = dd;
        SetState(MoveState.Jumping);
        return true;
    }

    public bool JumpChecked()
    {
        if (!grounded && !override_allow_jump) return false;
        return Jump();
    }
    private Coroutine Dc;
    public void Dash(EntityOXS a, Skill b)
    {
        switch (CurrentState)
        {
            case MoveState.Dashing: return;
            default:
                Vector3 dir = Vector3.zero;
                if (InputManager.IsKey("move_forward", "Player")) dir += HeadXZ.forward;
                if (InputManager.IsKey("move_back", "Player")) dir += HeadXZ.forward * -1;
                if (InputManager.IsKey("move_right", "Player")) dir += HeadY.right;
                if (InputManager.IsKey("move_left", "Player")) dir += HeadY.right * -1;
                if (dir.magnitude > 0.5)
                {
                    Dc = StartCoroutine(DashCour(dir.normalized));
                    OXEvent.SuccessfulHit = true;
                    return;
                }
                OXEvent.SuccessfulHit = false;
                return;
        }
    }
    public IEnumerator DashCour(Vector3 dir)
    {
        SetState(MoveState.Dashing);
        float d = dash_dur;
        start_dash_vel = rigid.linearVelocity;
        while (d > 0)
        {
            rigid.linearVelocity = dir * dash_str;
            d -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        EndDash(dir);
    }

    public void EndDash(Vector3 dir)
    {
        start_dash_vel.y = 0f;
        rigid.linearVelocity = (dir.normalized * start_dash_vel.magnitude) + (dir * dash_end_str / Mathf.Clamp(start_dash_vel.magnitude, 2, 1000));
        SetState(MoveState.Jumping);
    }


    public GameObject nerd = null;
    private float rot_y = 0;
    private float rot_x = 0;
    private float slidemin = 0;
    private void Update()
    {
        if (Movements.HasFlag(AllowedMovements.Jump)) InputBuffer.Instance.BufferListen("jump", "Player", "Jump", 0.1f);
        if (Movements.HasFlag(AllowedMovements.Dash)) InputBuffer.Instance.BufferListen("dash", "Player", "Dash", 0.1f);
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
        if (Movements.HasFlag(AllowedMovements.Dash))
        {
            if (InputBuffer.Instance.GetBuffer("Dash"))
            {
                if (entity.Skill().Get("DashSkill").Activate(entity))
                {
                    InputBuffer.Instance.RemoveBuffer("Dash");
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
                        slidemin = slide_min_time;
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
                    slidemin -= Time.deltaTime;
                    if (!InputBuffer.Instance.GetBuffer("Slide") && slidemin <= 0)
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
    private Vector3 wall_normal = Vector3.zero;
    public void CollisionGroundCheck()
    {
        if (jump_bananas >= 0)
        {
            return;
        }
        switch (CurrentState)
        {
            case MoveState.Dashing: return;
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
                wall_cur_shungle = 0;
                wall_shungle_consecutive = 1;
                if (CurrentState != MoveState.Sliding) SetState();
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
    public bool override_allow_jump = false;
    public void SetState(MoveState sta = MoveState.Neutral)
    {
        CurrentState = sta;
        grounded = false;
        allow_movement_inputs = true;
        override_allow_jump = false;
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
                ground_normal = Vector3.up;
                override_allow_jump = true;
                break;
            case MoveState.Jumping:
                ground_normal = Vector3.up;
                break;
        }
    }

    private Vector3 walldir = Vector3.zero;


}
