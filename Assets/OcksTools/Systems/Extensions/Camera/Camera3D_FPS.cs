using UnityEngine;

public class Camera3D_FPS : OXCamera, CameraFor3D
{
    public Transform HeadY;
    public Transform HeadXZ;
    public Transform HeadStyle;
    public Transform GetHeadStyle() => HeadStyle;
    public Transform GetHeadXZ() => HeadXZ;
    public Transform GetHeadY() => HeadY;
}
