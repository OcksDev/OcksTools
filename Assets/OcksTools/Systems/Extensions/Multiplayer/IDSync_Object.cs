#if OXTLS_MULTIPLAYER
public class IDSync_Object : IDSync
{
    private void Start()
    {
        if (IsHost)
        {
            var id = Tags.ID.Get(gameObject);
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
#endif
