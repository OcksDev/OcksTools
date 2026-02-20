using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkIDSync : NetworkBehaviour
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
            Server.Send().IDSync(Server.Instance.ClientID, OwnerClientId);
        }
    }

    public override void OnDestroy()
    {
        Server.BADAllClients.Remove(OwnerClientId);
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        base.OnDestroy();
    }

    private void OnClientConnected(ulong XXXXXclientIdXXXX)
    {
        if (!IsLocalPlayer) return;
        Server.Send().IDSync(Server.Instance.ClientID, OwnerClientId);
    }
}
