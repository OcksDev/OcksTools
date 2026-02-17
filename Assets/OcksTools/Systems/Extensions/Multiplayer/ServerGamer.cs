using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public static class Server
{
    public static ServerGamer Instance;
    public static ServerGamer Send()
    {
        Instance._Handover = new OXNetworkRpcData(Instance.ClientID, "");
        return Instance;
    }

    public static ServerGamer Send(FixedString64Bytes s)
    {
        Instance._Handover = new OXNetworkRpcData(Instance.ClientID, s);
        return Instance;
    }

    public static ServerGamer Send(string s, FixedString64Bytes n)
    {
        Instance._Handover = new OXNetworkRpcData(s, n);
        return Instance;
    }
    public static void handjoib(string spawndata)
    {
        Send().SpawnObject(spawndata);
    }
}



public class ServerGamer : NetworkBehaviour
{
    public string ClientID;
    // public NetworkVariable<int> PlayerNum = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // FixedString128Bytes

    public Dictionary<FixedString64Bytes, Queue<Action>> MessageBacklog = new Dictionary<FixedString64Bytes, Queue<Action>>();
    public List<FixedString64Bytes> LockedBacklogs = new List<FixedString64Bytes>();

    private void Awake()
    {
        if (Server.Instance == null) Server.Instance = this;
        ClientID = Tags.GenerateID();
        Console.Log("My ID: " + ClientID);
        SpawnSystem.SpawnShareMethod = Server.handjoib;

    }

    public void LockBacklog(FixedString64Bytes x)
    {
        if (x.ToString() == "") return; // 0 can not be locked
        if (LockedBacklogs.Contains(x)) return;
        LockedBacklogs.Add(x);
    }

    public void UnlockBacklog(FixedString64Bytes x)
    {
        if (x.ToString() == "") return; // 0 can not be locked
        if (!LockedBacklogs.Contains(x)) return;
        LockedBacklogs.Remove(x);
        var q = MessageBacklog.GetOrDefine(x, new Queue<Action>());
        while (q.Count > 0)
        {
            q.Dequeue()();
        }
    }

    public void AddFrom(FixedString64Bytes x, Action a)
    {
        if (x.ToString() == "" || !LockedBacklogs.Contains(x))
        {
            a();
            return;
        }
        else
        {
            MessageBacklog.GetOrDefine(x, new Queue<Action>()).Enqueue(a);
        }
    }


    public OXNetworkRpcData _Handover;




    public void SpawnObject(string spawndata)
    {
        _SpawnObjectPingPongServerRpc(_Handover, spawndata);
    }


    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SpawnObjectPingPongServerRpc(OXNetworkRpcData id, string spawndata)
    {
        _SpawnObjectClientRpc(id, spawndata);
    }

    [ClientRpc]
    public void _SpawnObjectClientRpc(OXNetworkRpcData id, string spawndata)
    {
        if (id == ClientID) return;

        AddFrom(id.Queue, () =>
        {
            SpawnSystem.Spawn(new SpawnData(spawndata, 0));
        });
    }



    public void Message(FixedString32Bytes type, string data)
    {
        _MessagePingPongServerRpc(_Handover, type, data);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void _MessagePingPongServerRpc(OXNetworkRpcData id, FixedString32Bytes type, string data)
    {
        _RecieveMessageClientRpc(id, type, data);
    }
    //chat related method
    [ClientRpc]
    public void _RecieveMessageClientRpc(OXNetworkRpcData id, FixedString32Bytes type, string data)
    {
        if (id == ClientID) return;
        AddFrom(id.Queue, () =>
        {
            switch (type.ToString())
            {
                case "Console":
                    data.Log();
                    break;
                case "Lock":
                    LockBacklog(data);
                    break;
                case "Unlock":
                    UnlockBacklog(data);
                    break;
                default:
                    break;
            }
        });
    }


    public void ChatMessage(string message, string hex)
    {
        _SendChatMessagePingPongServerRpc(_Handover, message, hex);
    }
    //chat related method
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void _SendChatMessagePingPongServerRpc(OXNetworkRpcData id, string message, string hex)
    {
        _RecieveChatMessageClientRpc(id, message, hex);
    }

    //chat related method
    [ClientRpc]
    public void _RecieveChatMessageClientRpc(OXNetworkRpcData id, string message, string hex)
    {
        if (id == ClientID) return;

        AddFrom(id.Queue, () =>
        {
            ChatLol.Instance.WriteChat(message, hex);
        });
    }




    //OcksNetworkVars
    public void OcksVar(string poopid, string name, string data)
    {
        //Console.Log($"Sending {ClientID}, {name}, {data}");
        OcksVarPingPongServerRpc(_Handover, poopid, name, data);
    }

    public void RequestForOcksVar(string poopid, string name)
    {
        //Console.Log($"Requesting {ClientID} at {name}");
        AquireOcksVarServerRpc(_Handover, poopid, name);
    }


    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void OcksVarPingPongServerRpc(OXNetworkRpcData id, string poopid, string name, string data)
    {
        //Console.Log($"(server) incoming request for set data");
        RecieveOcksVarClientRpc(id, poopid, name, data);
    }



    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AquireOcksVarServerRpc(OXNetworkRpcData id, string poopid, string name)
    {
        //Console.Log($"(server) incoming request for aquire");
        CreateEmpty(poopid, name);
        RecieveOcksVarClientRpc("Host", poopid, name, ONVManager.OcksVars[poopid][name].Data);
    }

    [ClientRpc]
    public void RecieveOcksVarClientRpc(OXNetworkRpcData id, string NetID, string Name, string data)
    {
        //Console.Log($"Recieved {id}, {Name}, {data}");
        if (id == ClientID) return;
        if (id == "Host" && NetworkManager.Singleton.IsHost) return;
        AddFrom(id.Queue, () =>
        {
            CreateEmpty(NetID, Name);
            //Console.Log($"Changed {NetID} to {data}");
            var a = ONVManager.OcksVars[NetID][Name];
            a.Data = data;
            PassAlongUpdate(a);
        });
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

public struct OXNetworkRpcData : INetworkSerializable
{
    public FixedString64Bytes ClientID;
    public FixedString64Bytes Queue;

    public OXNetworkRpcData(FixedString64Bytes ID, FixedString64Bytes num)
    {
        ClientID = ID;
        Queue = num;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref Queue);
    }
    public static implicit operator OXNetworkRpcData(string ID) { return new OXNetworkRpcData(ID, ""); }
    public static implicit operator string(OXNetworkRpcData nerd) { return nerd.ClientID.ToString(); }
}