using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerGamer : NetworkBehaviour
{
    public string ClientID;
    public Style BaseStyle = Style.PeerToHostToPeer;
    // public NetworkVariable<int> PlayerNum = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // FixedString128Bytes
    public static ServerGamer Instance;
    public enum Style
    {
        DifferToBase,
        PeerToHostToPeer,
        PeerToPeer,
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        ClientID = Tags.GenerateID();
        Console.Log("My ID: " + ClientID);
        SpawnSystem.SpawnShareMethod = handjoib;
    }


    public void handjoib(string spawndata)
    {
        SpawnObjectCall(spawndata);
    }

    public void SpawnObjectCall(string spawndata, Style style = Style.DifferToBase)
    {
        if (style == Style.DifferToBase) style = BaseStyle;
        switch (style)
        {
            case Style.PeerToHostToPeer:
                _SpawnObjectPingPongServerRpc(ClientID, spawndata);
                break;
            case Style.PeerToPeer:
                _SpawnObjectPTPServerRpc(spawndata);
                break;
            case Style.DifferToBase: Debug.LogError("Base style can not be Differ"); break;
        }
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SpawnObjectPingPongServerRpc(string id, string spawndata)
    {
        _SpawnObjectClientRpc(id, spawndata);
    }

    [Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SpawnObjectPTPServerRpc(string spawndata)
    {
        _SpawnObjectCode(spawndata);
    }

    [ClientRpc]
    public void _SpawnObjectClientRpc(string id, string spawndata)
    {
        if (id == ClientID) return;
        _SpawnObjectCode(spawndata);
    }
    public void _SpawnObjectCode(string spawndata)
    {
        SpawnSystem.Spawn(new SpawnData(spawndata, 0));
    }



    public void MessageCall(string id, string type, string data, Style style = Style.PeerToHostToPeer)
    {
        if (style == Style.DifferToBase) style = BaseStyle;
        switch (style)
        {
            case Style.PeerToHostToPeer:
                _MessagePingPongServerRpc(id, type, data);
                break;
            case Style.PeerToPeer:
                _MessagePTPServerRpc(id, type, data);
                break;
            case Style.DifferToBase: Debug.LogError("Base style can not be Differ"); break;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void _MessagePingPongServerRpc(string id, string type, string data)
    {
        _RecieveMessageClientRpc(id, type, data);
    }
    [Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Everyone)]
    public void _MessagePTPServerRpc(string id, string type, string data)
    {
        _RecieveMessage(id, type, data);
    }
    //chat related method
    [ClientRpc]
    public void _RecieveMessageClientRpc(string id, string type, string data)
    {
        if (id == ClientID) return;
        _RecieveMessage(id, type, data);
    }
    public void _RecieveMessage(string id, string type, string data)
    {
        switch (type)
        {
            default:
                break;
        }
    }


    public void SendChatMessageCall(string id, string message, string hex, Style style = Style.DifferToBase)
    {
        if (style == Style.DifferToBase) style = BaseStyle;
        switch (style)
        {
            case Style.PeerToHostToPeer:
                _SendChatMessagePingPongServerRpc(id, message, hex);
                break;
            case Style.PeerToPeer:
                _SendChatMessagePTPServerRpc(message, hex);
                break;
            case Style.DifferToBase: Debug.LogError("Base style can not be Differ"); break;
        }
    }
    //chat related method
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SendChatMessagePingPongServerRpc(string id, string message, string hex)
    {
        _RecieveChatMessageClientRpc(id, message, hex);
    }
    //chat related method
    [Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SendChatMessagePTPServerRpc(string message, string hex)
    {
        _ChatMessageCode(message, hex);
    }

    //chat related method
    [ClientRpc]
    public void _RecieveChatMessageClientRpc(string id, string message, string hex)
    {
        if (id == ClientID) return;

        _ChatMessageCode(message, hex);
    }
    public void _ChatMessageCode(string message, string hex)
    {
        ChatLol.Instance.WriteChat(message, hex);
    }




    //OcksNetworkVars
    public void SendOcksVar(string poopid, string name, string data, Style style = Style.DifferToBase)
    {
        //Console.Log($"Sending {ClientID}, {name}, {data}");
        if (style == Style.DifferToBase) style = BaseStyle;
        switch (style)
        {
            case Style.PeerToHostToPeer:
                OcksVarPingPongServerRpc(ClientID, poopid, name, data);
                break;
            case Style.PeerToPeer:
                OcksVarPTPServerRpc(poopid, name, data);
                break;
            case Style.DifferToBase: Debug.LogError("Base style can not be Differ"); break;
        }
    }

    public void RequestOcksVar(string poopid, string name)
    {
        //Console.Log($"Requesting {ClientID} at {name}");
        AquireOcksVarServerRpc(ClientID, poopid, name);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OcksVarPingPongServerRpc(string id, string poopid, string name, string data)
    {
        //Console.Log($"(server) incoming request for set data");
        RecieveOcksVarClientRpc(id, poopid, name, data);
    }

    [Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Everyone)]
    public void OcksVarPTPServerRpc(string poopid, string name, string data)
    {
        //Console.Log($"(server) incoming request for set data");
        RecieveOcksVarCode(poopid, name, data);
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
        RecieveOcksVarCode(NetID, Name, data);
    }

    public void RecieveOcksVarCode(string NetID, string Name, string data)
    {
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
            if (b != null) b._DataWasChangedByServer();
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
