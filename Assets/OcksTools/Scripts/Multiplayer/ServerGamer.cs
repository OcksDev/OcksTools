using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerGamer : NetworkBehaviour
{
    private static ServerGamer instance;
    public string ClientID;

    // public NetworkVariable<int> PlayerNum = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // FixedString128Bytes
    public static ServerGamer Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (Instance == null) instance = this;
        ClientID = Tags.GenerateID();
    }

    public static Dictionary<string, OcksNetworkVar> OcksVars = new Dictionary<string, OcksNetworkVar>();
    public static Dictionary<NetworkObject, List<OcksNetworkVar>> OcksVars2 = new Dictionary<NetworkObject, List<OcksNetworkVar>>();

    /* working code, commented out to prevent error messages when importing oxtools*/
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(int refe, Vector3 pos, Quaternion rot, string id, string data, string hdata)
    {
        SpawnObjectClientRpc(refe, pos, rot, id, data, hdata);
    }

    [ClientRpc]
    public void SpawnObjectClientRpc(int refe, Vector3 pos, Quaternion rot, string id, string data = "", string hdata = "")
    {
        if (id == ClientID) return;

        SpawnSystem.Instance.SpawnObject(refe, gameObject, pos, rot, false, data, hdata);
    }



    [ServerRpc(RequireOwnership = false)]
    public void MessageServerRpc(string id, string type, string data)
    {
        RecieveMessageClientRpc(id, type, data);
    }

    //chat related method
    [ClientRpc]
    public void RecieveMessageClientRpc(string id, string type, string data)
    {
        if (id == ClientID) return;
        switch (type)
        {
            default:
                break;
        }
    }


    //chat related method
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string id, string message, string hex)
    {
        RecieveChatMessageClientRpc(id, message, hex);
    }

    //chat related method
    [ClientRpc]
    public void RecieveChatMessageClientRpc(string id, string message, string hex)
    {
        if (id == ClientID) return;

        ChatLol.Instance.WriteChat(message, hex);
    }

    public void SendOcksVar(string name, string data)
    {
        OcksVarServerRpc(ClientID, name, data);
    }


    [ServerRpc(RequireOwnership = false)]
    public void OcksVarServerRpc(string id, string type, string data)
    {
        RecieveOcksVarClientRpc(id, type, data);
    }

    //chat related method
    [ClientRpc]
    public void RecieveOcksVarClientRpc(string id, string type, string data)
    {
        if (id == ClientID) return;
        switch (type)
        {
            default:
                break;
        }
    }

}
