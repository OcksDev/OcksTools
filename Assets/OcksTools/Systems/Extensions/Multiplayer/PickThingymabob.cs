using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PickThingymabob : MonoBehaviour
{
    private RelayMoment relay;
    public void GoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void GoinGameE()
    {
        var p = GetComponent<TMP_InputField>();
        GoinGameE2(p.text);
    }
    public async void GoinGameE2(string code)
    {
        relay = RelayMoment.Instance;

        if (relay.SignInState != "Good")
        {
            var e = await relay.SignIn();
            if (e == 0)
            {
                return;
            }
        }

        int i = await relay.JoinRelay(code);
        if (i == 1)
        {
            NetworkManager.Singleton.StartClient();
            if (relay.JoinCodeTextDick != null) relay.JoinCodeTextDick.text = relay.Join_Code;
        }
        else
        {
            //code goes here for if you failed to join
        }
    }

    public void MakeGameButton()
    {
        _ = MakeGame();
    }

    public async Task<string> MakeGame()
    {
        var x = await MakeGame2();
        if (x != "Error")
        {
            relay = RelayMoment.Instance;
            var p = Instantiate(relay.ServerGamerObject, relay.transform.position, relay.transform.rotation, relay.transform);
            p.GetComponent<NetworkObject>().Spawn();
            if (relay.JoinCodeTextDick != null) relay.JoinCodeTextDick.text = relay.Join_Code;
        }
        return x;
    }

    public async void MakeGameAndCopy()
    {
        var x = await MakeGame();
        if (x != "Error")
        {
            OXClip.SetClipboard("join " + RelayMoment.Instance.Join_Code);
        }
    }

    public OXEvent<string> DebugCode = new OXEvent<string>();

    public async Task<string> MakeGame2()
    {
        relay = RelayMoment.Instance;


        if (relay.SignInState != "Good")
        {
            var e = await relay.SignIn();
            if (e == 0)
            {
                return "Error";
            }
        }

        int i = await relay.CreateRelay();
        if (i == 1)
        {
            DebugCode.Invoke(relay.Join_Code);

            NetworkManager.Singleton.StartHost();

            return relay.Join_Code;
        }
        else
        {
            return "Error";
        }
    }
}
