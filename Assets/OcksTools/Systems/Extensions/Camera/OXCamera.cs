using NaughtyAttributes;
using UnityEngine;

public abstract class OXCamera : MonoBehaviour
{
    public TargetForCamera<Vector3> Position = new(Vector3.zero);
    public TargetForCamera<Quaternion> Rotation = new(Quaternion.identity);
    public ShakeHolder Shake = new ShakeHolder();
    protected ResettableValue<float> progress_to_waypoint = new(0f);
    public OXEvent OnWaypointReach = new();
    [HideInInspector]
    public int WaypointState = 0;
    [HideInInspector]
    public bool Is2D = false;
    [ShowIf("Is2D")]
    public float CameraZ = -10;
    public void GoToWaypoint(float seconds, Vector3? pos = null, Quaternion? rot = null, bool HoldOnFinish = true)
    {
        Position.Override = pos.HasValue;
        Rotation.Override = rot.HasValue;
        if (pos.HasValue)
        {
            var a = pos.Value;
            if (Is2D) a.z = CameraZ;
            Position.Target.Current = transform.position;
            Position.Target.Current = a;
        }
        if (rot.HasValue)
        {
            Rotation.Target.Initial = transform.rotation;
            Rotation.Target.Current = rot.Value;
        }
        progress_to_waypoint.Initial = seconds;
        progress_to_waypoint.Current = 0;
        if (WaypointState != 2 && HoldOnFinish) WaypointState = HoldOnFinish ? 2 : 1;
    }
    public void TakeControlBack()
    {
        WaypointState = 0;
    }
    public (Vector3? pos, Quaternion? rot) AdvanceTowardWaypoint(float dt)
    {
        progress_to_waypoint.Current += dt;
        float p = Mathf.Clamp01(progress_to_waypoint.Percent());
        Vector3? pos = null;
        Quaternion? rot = null;
        if (Position.Override) pos = Vector3.Lerp(Position.Target.Initial, Position.Target.Current, RandomFunctions.EaseSinInAndOut(p));
        if (Rotation.Override) rot = Quaternion.Slerp(Rotation.Target.Initial, Rotation.Target.Current, RandomFunctions.EaseSinInAndOut(p));
        if (p >= 1)
        {
            OnWaypointReach.Invoke();
            if (WaypointState == 1) WaypointState = 0;
            Position.Override = false;
            Rotation.Override = false;
        }
        return (pos, rot);
    }

    public (Vector3? pos, Quaternion? rot) Advance(float dt)
    {
        Vector3? pos = Position.AllowFollow ? Position.Target : Vector3.zero;
        Quaternion? rot = Rotation.AllowFollow ? Rotation.Target : Quaternion.identity;
        if (WaypointState == 0)
        {
            if (Position.AllowFollow)
            {
                pos = Vector3.Lerp(pos.Value, Position.Target.Initial, Position.FollowStrength.TimeStableLerp());
                Position.Target.Current = pos.Value;
            }
            if (Rotation.AllowFollow)
            {
                rot = Quaternion.Slerp(rot.Value, Rotation.Target.Initial, Rotation.FollowStrength.TimeStableLerp());
                Rotation.Target.Current = rot.Value;
            }
        }
        else
        {
            return AdvanceTowardWaypoint(dt);
        }
        return (pos, rot);
    }

    public void SetTargetPosition(Vector3 a)
    {
        if (Is2D) a.z = CameraZ;
        Position.Target.Initial = a;
    }

    public void SetTargetRotation(Quaternion a)
    {
        Rotation.Target.Initial = a;
    }
}
[System.Serializable]
public class TargetForCamera<T>
{
    public bool AllowFollow = false;
    public float FollowStrength = 0.2f;
    public ResettableValue<T> Target;
    [HideInInspector]
    public bool Override = false;
    public TargetForCamera(T t)
    {
        Target = new(t);
    }
}