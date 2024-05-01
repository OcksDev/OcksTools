using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerGamer : NetworkBehaviour
{
    private static ServerGamer instance;

    // public NetworkVariable<int> PlayerNum = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // FixedString128Bytes
    public static ServerGamer Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (Instance == null) instance = this;
    }


    /* working code, commented out to prevent error messages when importing oxtools*/
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(int refe, Vector3 pos, Quaternion rot, string id, string data, string hdata)
    {
        SpawnObjectClientRpc(refe, pos, rot, id, data, hdata);
    }

    [ClientRpc]
    public void SpawnObjectClientRpc(int refe, Vector3 pos, Quaternion rot, string id, string data = "", string hdata = "")
    {
        if (id == RandomFunctions.Instance.ClientID) return;

        RandomFunctions.Instance.SpawnObject(refe, gameObject, pos, rot, false, data, hdata);
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
        if (id == RandomFunctions.Instance.ClientID) return;
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
        if (id == RandomFunctions.Instance.ClientID) return;

        ChatLol.Instance.WriteChat(message, hex);
    }

}
