using Unity.Netcode;

public class IDSync_Object : NetworkBehaviour
{
    private void Start()
    {
        if (IsHost)
        {
            Server.Send().SendObjectID(NetworkObjectId, Tags.GetIDOf(gameObject));
        }
        else
        {
            Server.AddObject(this);
        }
    }
    public string IDRead = "";
    private void FixedUpdate()
    {
        IDRead = Tags.GetIDOf(gameObject);
    }
}
