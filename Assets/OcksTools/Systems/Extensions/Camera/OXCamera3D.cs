using UnityEngine;

public abstract class OXCamera3D : OXCamera
{
    public Transform HeadY;
    public Transform HeadXZ;
    public Transform HeadStyle;
    public float x_mouse_sense = 1;
    public float y_mouse_sense = 1;
    public float x_controller_sense = 1;
    public float y_controller_sense = 1;
    public float x_sense => InputManager.IsControllerLastInput ? x_controller_sense * 1.5f : x_mouse_sense;
    public float y_sense => InputManager.IsControllerLastInput ? y_controller_sense * 1.5f : y_mouse_sense;
}
