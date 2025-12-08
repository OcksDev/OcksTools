using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayMoment : SingleInstance<RelayMoment>
{
    public string Join_Code = "";
    public GameObject ServerGamerObject;
    public string SignInState = "";
    public TextMeshProUGUI JoinCodeTextDick;
    //Default setup to make this a singleton
    // Start is called before the first frame update
    async void Start()
    {
        await SignIn();
    }


    public async Task<int> SignIn()
    {
        if (SignInState == "Good") return 1;
        try
        {
            SignInState = "Connecting";
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () => {
                Debug.Log("Shitted fardly");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            SignInState = "Good";
            return 1;
        }
        catch
        {
            Debug.Log("Failed to sign in, probably due to internet issues lol");
            SignInState = "Fail";
            return 0;
        }
    }



    public async Task<int> CreateRelay()
    {
        try
        {
            //MAX CONNECTIONS IS SET HERE   VERY IMPORTANT
            Allocation allo = await RelayService.Instance.CreateAllocationAsync(5);

            Join_Code = await RelayService.Instance.GetJoinCodeAsync(allo.AllocationId);
            Debug.Log(Join_Code);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allo, "dtls"));

            NetworkManager.Singleton.StartHost();
            return 1;
        }
        catch
        {
            Debug.Log("SHID FUKED");
        }
        return 0;
    }

    public async Task<int> JoinRelay(string joinC)
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

            NetworkManager.Singleton.StartClient();

            Join_Code = joinC.ToUpper();
            return 1;
        }
        catch
        {
            Debug.Log("HAH STUIPDIUD");
        }
        return 0;
    }

    public void EndConnection()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
