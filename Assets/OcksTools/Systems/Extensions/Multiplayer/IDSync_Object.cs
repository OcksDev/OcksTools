using Unity.Collections;
using Unity.Netcode;

public class IDSync_Object : NetworkBehaviour
{
    [ShowFixedBetter]
    public FixedString64Bytes MyID = "-";
    private void Start()
    {
        if (IsHost)
        {
            var id = Tags.GetIDOf(gameObject);
            Server.Send().SendObjectID(NetworkObjectId, id);
            MyID = id;
        }
        else
        {
            Server.ObjectIDSync.AddT1(NetworkObjectId, this);
            Console.Log($"Stored: {NetworkObjectId}");
        }
    }
}
