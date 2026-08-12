using TMPro;
using UnityEngine;

public class MultiplayerUIStuff : MonoBehaviour
{
    public void GoinGameE()
    {
        var p = GetComponent<TMP_InputField>();
        RelayServerManager.Instance.JoinGame(p.text);
    }

    public void MakeGameButton()
    {
        _ = RelayServerManager.Instance.CreateGame();
    }

    public void MakeGameAndCopyButton()
    {
        _ = RelayServerManager.Instance.CreateGameAndCopy();
    }

}
