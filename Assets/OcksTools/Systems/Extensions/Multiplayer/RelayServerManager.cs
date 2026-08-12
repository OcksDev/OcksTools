using NaughtyAttributes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayServerManager : SingleInstance<RelayServerManager>
{
    public GameObject ServerGamerObject;
    [ReadOnly]
    public SignState SignInState = SignState.None;
    [ReadOnly]
    public string Join_Code = "";
    public OXEvent<string> DebugCode = new();
    public static OXEvent<List<string>> DenyJoinCheck = new();
    public HashSet<string> BannedPlayerIDs = new();
    private Dictionary<ulong, string> _clientPlayerIds = new();

    private async void Start()
    {
        await SignIn();
    }

    public async Task<string> CreateGame()
    {
        var x = await CreateGame2();
        if (x != "Error")
        {
            var p = Instantiate(ServerGamerObject, transform.position, transform.rotation);
            p.GetComponent<NetworkObject>().Spawn();
            DebugCode.Invoke(x);
        }
        return x;
    }

    public async Task<string> CreateGameAndCopy()
    {
        var x = await CreateGame();
        if (x != "Error")
        {
            OXClip.SetClipboard("join " + RelayServerManager.Instance.Join_Code);
        }
        return x;
    }
    public async void JoinGame(string code)
    {
        if (SignInState != SignState.Good)
        {
            var e = await SignIn();
            if (e != RelayState.Success)
            {
                return;
            }
        }

        RelayState i = await JoinRelay(code);
    }
    public async Task<RelayState> SignIn()
    {
        if (SignInState == SignState.Good) return RelayState.Success;
        try
        {
            SignInState = SignState.Connecting;
            await UnityServices.InitializeAsync();

            /* AuthenticationService.Instance.SignedIn += () =>
             {
                 "Connected".Log();
             };*/

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            SignInState = SignState.Good;
            return RelayState.Success;
        }
        catch
        {
            "Failed to sign in, probably due to internet issues lol".LogError();
            SignInState = SignState.Fail;
            return RelayState.Error;
        }
    }



    private async Task<RelayState> CreateRelay()
    {
        try
        {
            //MAX CONNECTIONS IS SET HERE   VERY IMPORTANT
            Allocation allo = await RelayService.Instance.CreateAllocationAsync(5);

            Join_Code = await RelayService.Instance.GetJoinCodeAsync(allo.AllocationId);
            Debug.Log(Join_Code);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allo, "dtls"));

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;


            NetworkManager.Singleton.StartHost();
            return RelayState.Success;
        }
        catch
        {
            "Failed to create server instance".LogError();
        }
        return RelayState.Error;
    }


    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (request.ClientNetworkId == NetworkManager.ServerClientId)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            return;
        }

        var theirshit = System.Text.Encoding.UTF8.GetString(request.Payload).StringToList();
        bool approved = theirshit.Count >= 2;
        if (approved) approved = VersionSame(theirshit[0]);
        if (approved) approved = !BannedPlayerIDs.Contains(theirshit[1]);
        if (approved)
        {
            DenyJoinCheck.InvokeWithHitCheck(theirshit, true);
        }

        // approved logic, DO NOT TOUCH
        if (approved) _clientPlayerIds[request.ClientNetworkId] = theirshit[1];

        response.Approved = approved;
        response.CreatePlayerObject = approved;

        if (!approved)
            $"Rejected client".LogWarning();
        else
            $"Client accepted".LogWarning();
    }


    private async Task<RelayState> JoinRelay(string joinC)
    {
        try
        {
            JoinAllocation ja = await RelayService.Instance.JoinAllocationAsync(joinC);

            RelayServerData rsd = new RelayServerData(
                ja.RelayServer.IpV4,                                      // host
                (ushort)ja.RelayServer.Port,                              // port
                ja.AllocationIdBytes,                                     // allocationId
                ja.ConnectionData,                                        // connectionData
                ja.HostConnectionData,                                    // hostConnectionData
                ja.Key,                                                   // key
                true                                                      // isSecure (dtls)
            );

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            Debug.Log("Relay Data = " + AllocationUtils.ToRelayServerData(ja, "dtls"));
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(ja, "dtls"));

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            string connectionPayload = Converter.ListToString(new List<string>(){
                FileSystem.GameVer,
                AuthenticationService.Instance.PlayerId
            });
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(connectionPayload);

            NetworkManager.Singleton.StartClient();

            Join_Code = joinC.ToUpper();
            return RelayState.Success;
        }
        catch
        {
            "Failed to join server".LogError();
        }
        return RelayState.Error;
    }

    public void EndConnection()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void KickConnection(ulong clientID)
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        {
            "Only the host/server can kick clients bro".DLogError();
            return;
        }
        NetworkManager.Singleton.DisconnectClient(clientID, "Kicked by Host");
    }

    public void KickConnection(NetworkBehaviour client)
    {
        KickConnection(client.OwnerClientId);
    }
    public void BanConnection(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) return;
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        {
            "Only the host/server can ban clients bro".DLogError();
            return;
        }
        string playerId = _clientPlayerIds.TryGetValue(clientId, out var id) ? id : "";
        if (playerId != "")
        {
            BannedPlayerIDs.Add(playerId);
        }
        KickConnection(clientId);
    }

    public void BanConnection(NetworkBehaviour client)
    {
        BanConnection(client.OwnerClientId);
    }


    private async Task<string> CreateGame2()
    {
        if (SignInState != SignState.Good)
        {
            var e = await SignIn();
            if (e == 0)
            {
                return "Error";
            }
        }

        RelayState i = await CreateRelay();
        if (i == RelayState.Success)
        {
            return Join_Code;
        }
        else
        {
            return "Error";
        }
    }

    public enum RelayState
    {
        Success,
        Error,
    }
    public enum SignState
    {
        None,
        Good,
        Connecting,
        Fail,
    }
    public bool VersionSame(string theirversion)
    {
        return RandomFunctions.CompareTwoVersions(FileSystem.GameVer, theirversion) == RandomFunctions.CompareState.Equal;
    }
}