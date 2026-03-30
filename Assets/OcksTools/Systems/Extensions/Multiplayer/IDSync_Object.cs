using Unity.Netcode;

public class IDSync_Object : NetworkBehaviour
{
    private void Start()
    {
        if (IsHost)
        {
            var sp = new SpawnData("").DontSpawn(gameObject);
            SpawnSystem.Spawn(sp);
            Server.Send().SendObjectID(NetworkObjectId, sp._IDValue);
        }
        else
        {
            Server.AddObject(this);
        }
    }

}
