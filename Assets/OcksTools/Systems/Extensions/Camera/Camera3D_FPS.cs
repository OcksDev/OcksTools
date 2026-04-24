using UnityEngine;

public class Camera3D_FPS : OXCamera3D
{
    public float vert_mouse_sense = 1;
    public float hori_mouse_sense = 1;
    private float rot_y = 0;
    private float rot_x = 0;
    private void Update()
    {
        var x = Input.GetAxis("Mouse X") * hori_mouse_sense;
        var y = Input.GetAxis("Mouse Y") * vert_mouse_sense;
        rot_x += x;
        rot_y = Mathf.Clamp(rot_y - y, -90, 90);
        HeadY.localRotation = Quaternion.Euler(rot_y, 0, 0);
        HeadXZ.localRotation = Quaternion.Euler(0, rot_x, 0);
        var (a, b) = Advance(Time.deltaTime);
        if (WaypointState == 0)
        {
            transform.localPosition = a.Value;
            transform.localRotation = b.Value;
        }
        else
        {
            transform.position = a.Value;
            transform.rotation = b.Value;
        }
    }
}
