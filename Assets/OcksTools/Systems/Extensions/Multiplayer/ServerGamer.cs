using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public static class Server
{
    public static ServerGamer Instance;
    public static ServerGamer Send(bool wait_first = false)
    {
        Instance._Handover = new OXNetworkRpcData(Instance.ClientID, "", wait_first);
        return Instance;
    }

    public static ServerGamer Send(FixedString64Bytes s, bool wait_first = false)
    {
        Instance._Handover = new OXNetworkRpcData(Instance.ClientID, s, wait_first);
        return Instance;
    }

    public static ServerGamer Send(string s, FixedString64Bytes n, bool wait_first = false)
    {
        Instance._Handover = new OXNetworkRpcData(s, n, wait_first);
        return Instance;
    }
    public static void handjoib(string spawndata)
    {
        Send().SpawnObject(spawndata);
    }

    public static Dictionary<FixedString64Bytes, NetworkIDSync> AllClients = new();
    public static Dictionary<ulong, NetworkIDSync> BADAllClients = new();

}



public class ServerGamer : NetworkBehaviour
{
    public override void OnDestroy()
    {
        Server.AllClients.Clear();
        Server.BADAllClients.Clear();
        base.OnDestroy();
    }
    public FixedString64Bytes ClientID = "";
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
        if (x.ToString() == "") return; // "" can not be locked
        if (LockedBacklogs.Contains(x)) return;
        LockedBacklogs.Add(x);
        FlushBacklog(x);
    }

    public void UnlockBacklog(FixedString64Bytes x)
    {
        if (x.ToString() == "") return; // "" can not be locked
        if (!LockedBacklogs.Contains(x)) return;
        LockedBacklogs.Remove(x);
        FlushBacklog(x);
    }

    public void ClearBacklog(FixedString64Bytes x)
    {
        MessageBacklog.GetOrDefine(x, new Queue<Action>()).Clear();
    }

    public void FlushBacklog(FixedString64Bytes x)
    {
        var q = MessageBacklog.GetOrDefine(x, new Queue<Action>());
        while (q.Count > 0)
        {
            q.Dequeue()();
        }
        q.Clear();
    }

    public void AddFrom(OXNetworkRpcData x, Action a)
    {
        if (x.Queue == "" || (!LockedBacklogs.Contains(x.Queue)) ^ x.WaitFirst)
        {
            a();
            return;
        }
        else
        {
            MessageBacklog.GetOrDefine(x.Queue, new Queue<Action>()).Enqueue(a);
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
        if (id.ClientID == ClientID) return;

        AddFrom(id, () =>
        {
            SpawnSystem.Spawn(new SpawnData(spawndata, 0));
        });
    }



    public void Message(FixedString64Bytes type, string data)
    {
        _MessagePingPongServerRpc(_Handover, type, data);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void _MessagePingPongServerRpc(OXNetworkRpcData id, FixedString64Bytes type, string data)
    {
        _RecieveMessageClientRpc(id, type, data);
    }
    //chat related method
    [ClientRpc]
    public void _RecieveMessageClientRpc(OXNetworkRpcData id, FixedString64Bytes type, string data)
    {
        if (id.ClientID == ClientID) return;
        AddFrom(id, () =>
        {
            switch (type.ToString())
            {
                case "Console":
                    data.Log();
                    break;
                case "LockB":
                    LockBacklog(data);
                    break;
                case "UnlockB":
                    UnlockBacklog(data);
                    break;
                case "ClearB":
                    ClearBacklog(data);
                    break;
                case "FlushB":
                    FlushBacklog(data);
                    break;
                default:
                    break;
            }
        });
    }


    public void IDSync(FixedString64Bytes ID, ulong bad_id)
    {
        _IDSyncServerRpc(_Handover, ID, bad_id);
    }
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void _IDSyncServerRpc(OXNetworkRpcData id, FixedString64Bytes ID, ulong bad_id)
    {
        _IDSyncClientRpc(id, ID, bad_id);
    }
    //chat related method
    [ClientRpc]
    public void _IDSyncClientRpc(OXNetworkRpcData id, FixedString64Bytes ID, ulong bad_id)
    {
        if (id.ClientID == ClientID) return;
        StartCoroutine(AddToNerds(ID, bad_id));
    }

    public IEnumerator AddToNerds(FixedString64Bytes ID, ulong bad_id)
    {
        if (Server.AllClients.ContainsKey(ID))
        {
            yield break; //already had this lol
        }
        if (Server.BADAllClients.ContainsKey(bad_id))
        {
            Server.AllClients.Add(ID, Server.BADAllClients[bad_id]);
            LockBacklog(ID);
            yield break;
        }

        //this shouldn't really happen, but just in case

        float x = Time.time + 2f;
        yield return new WaitUntil(() => Time.time >= x || Server.BADAllClients.ContainsKey(bad_id));
        if (Server.BADAllClients.ContainsKey(bad_id))
        {
            Server.AllClients.Add(ID, Server.BADAllClients[bad_id]);
            LockBacklog(ID);
            yield break;
        }

        //this really shouldn't happen

        Debug.LogError($"{ID} given with id {bad_id}, but never matched?");
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
        if (id.ClientID == ClientID) return;
        if (id.ClientID == "Host" && NetworkManager.Singleton.IsHost) return;
        AddFrom(id, () =>
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
    public bool WaitFirst;

    public OXNetworkRpcData(FixedString64Bytes ID, FixedString64Bytes num, bool wait)
    {
        ClientID = ID;
        Queue = num;
        WaitFirst = wait;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref Queue);
        serializer.SerializeValue(ref WaitFirst);
    }
    public static implicit operator OXNetworkRpcData(string ID) { return new OXNetworkRpcData(ID, "", false); }
    public static implicit operator string(OXNetworkRpcData nerd) { return nerd.ClientID.ToString(); }
}