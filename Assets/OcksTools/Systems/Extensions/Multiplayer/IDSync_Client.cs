#if OXTLS_MULTIPLAYER
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class IDSync_Client : IDSync
{
    private void Awake()
    {
        StartCoroutine(waitforserver());
    }
    private IEnumerator waitforserver()
    {
        yield return new WaitUntil(() => Server.Instance != null);
        Server.BADAllClients.Add(OwnerClientId, this);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        if (IsLocalPlayer)
        {
            Server.AllClients.Add(Server.Instance.ClientID, this);
            Server.Send().IDSync(OwnerClientId);
            MyID = Server.Instance.ClientID;
        }
    }

    public override void OnDestroy()
    {
        Server.BADAllClients.Remove(OwnerClientId);
        Server.AllClients.Remove(MyID);
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        base.OnDestroy();
    }

    private void OnClientConnected(ulong XXXXXclientIdXXXX)
    {
        if (!IsLocalPlayer) return;
        Server.Send().IDSync(OwnerClientId);
    }
}
#endif
