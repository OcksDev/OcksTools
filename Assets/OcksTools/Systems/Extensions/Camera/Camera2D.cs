using UnityEngine;

public class Camera2D : OXCamera
{
    public bool FollowsMouse = true;
    public float MouseFollowSpeed = 0.2f;
    public float MouseFollowStrength = 1;
    public bool WaypointStopsMouseFollow = true;
    private Vector3 CurMouse = Vector3.zero;
    private void Reset()
    {
        Is2D = true;
    }
    private void LateUpdate()
    {
        //handles getting the mouse position and making the camera adjust to move to it

        var (a, _) = Advance(Time.deltaTime);

        var p = GetMouseFollow();
        CurMouse = Vector3.Lerp(CurMouse, p, MouseFollowSpeed.TimeStableLerp());
        a += CurMouse;
        Vector3 ss = a.Value;
        ss.z = CameraZ;
        transform.position = ss;
    }
    public Vector3 GetMouseFollow()
    {
        Vector3 p = Vector3.zero;
        if (FollowsMouse)
        {
            if (WaypointStopsMouseFollow && WaypointState != 0) return p;
            p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p -= transform.position;
            p /= 5;
            p *= MouseFollowStrength;
        }
        p.z = 0;
        return p;
    }
}
