using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerGamer : NetworkBehaviour
{
    public string ClientID;

    // public NetworkVariable<int> PlayerNum = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // FixedString128Bytes
    public static ServerGamer Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        ClientID = Tags.GenerateID();
        Console.Log("My ID: " + ClientID);
        SpawnSystem.SpawnShareMethod = handjoib;
    }


    public void handjoib(string spawndata)
    {
        SpawnObjectServerRpc(spawndata);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnObjectServerRpc(string spawndata)
    {
        SpawnObjectClientRpc(ClientID, spawndata);
    }

    [ClientRpc]
    public void SpawnObjectClientRpc(string id, string spawndata)
    {
        if (id == ClientID) return;

        SpawnSystem.Spawn(new SpawnData(spawndata, 0));
    }



    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
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
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
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




    //OcksNetworkVars
    public void SendOcksVar(string poopid, string name, string data)
    {
        //Console.Log($"Sending {ClientID}, {name}, {data}");
        OcksVarServerRpc(ClientID, poopid, name, data);
    }
    
    public void RequestOcksVar(string poopid, string name)
    {
        //Console.Log($"Requesting {ClientID} at {name}");
        AquireOcksVarServerRpc(ClientID, poopid, name);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OcksVarServerRpc(string id, string poopid, string name, string data)
    {
        //Console.Log($"(server) incoming request for set data");
        RecieveOcksVarClientRpc(id, poopid, name, data);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AquireOcksVarServerRpc(string id, string poopid, string name)
    {
        //Console.Log($"(server) incoming request for aquire");
        CreateEmpty(poopid, name);
        RecieveOcksVarClientRpc("Host", poopid, name, ONVManager.OcksVars[poopid][name].Data);
    }

    [ClientRpc]
    public void RecieveOcksVarClientRpc(string id, string NetID, string Name, string data)
    {
        //Console.Log($"Recieved {id}, {Name}, {data}");
        if (id == ClientID) return;
        if (id == "Host" && NetworkManager.Singleton.IsHost) return;
        CreateEmpty(NetID, Name);
        //Console.Log($"Changed {NetID} to {data}");
        var a = ONVManager.OcksVars[NetID][Name];
        a.Data = data;
        PassAlongUpdate(a);
    }
    public void PassAlongUpdate(OcksNetworkVarData a)
    {
        foreach (var b in a.OcksNetworkVars)
        {
            if (b != null) b.DataWasChangedByServer();
        }
    }
    public void CreateEmpty(string NetID, string Name)
    {
        if (!ONVManager.OcksVars.ContainsKey(NetID))
        {
            ONVManager.OcksVars.Add(NetID, new Dictionary<string, OcksNetworkVarData>());
        }
        if (!ONVManager.OcksVars[NetID].ContainsKey(Name))
        {
            ONVManager.OcksVars[NetID].Add(Name, new OcksNetworkVarData());
        }
    }

}
