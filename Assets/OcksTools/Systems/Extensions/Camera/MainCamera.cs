public class MainCamera : SingleInstanceGeneric<OXCamera>
{
    public OXCamera Camera;
    public override void SetBaseInstance()
    {
        if (Camera == null) Camera = GetComponent<OXCamera>();
        SetInstance(Camera);
    }
}
