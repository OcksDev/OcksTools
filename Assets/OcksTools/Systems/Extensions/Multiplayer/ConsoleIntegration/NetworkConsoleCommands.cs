using System.Collections;
using UnityEngine;

public class NetworkConsoleCommands : MonoBehaviour
{
    public OXLanguageFileIndex LanguageFileIndex;
    private void Start()
    {
        LanguageFileSystem.Instance.AddFile(LanguageFileIndex);
        //ConsoleLol.ConsoleHook.Append(NetworkConsoleCommandHook);
        GlobalEvent.Append("Console", NetworkCommands);

        RelayMoment.Instance.GetComponent<PickThingymabob>().DebugCode.Append(PrintCode);
    }

    public void NetworkCommands()
    {
        ConsoleLol.Instance.Add(new OXCommand("host").Action(host).Append(new OXCommand("copy").Action(host_and_copy)));
        ConsoleLol.Instance.Add(new OXCommand("join").Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(join)));
        ConsoleLol.Instance.Add(new OXCommand("disconnect").Action(disconnect));
        ConsoleLol.Instance.Append("test", new OXCommand("objectspawn").Action(objectspawn));
        ConsoleLol.Instance.Append("test", new OXCommand("message").Action(message));
        ConsoleLol.Instance.Append("test", new OXCommand("locking").Action(locking));
    }
    public static void host()
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGameButton();
    }

    public static void host_and_copy()
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGameAndCopy();
    }
    public static void objectspawn()
    {
        SpawnSystem.Spawn(new SpawnData("Triangle")
            .Position(new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0))
            .MultiplayerShare());
    }
    public static void message()
    {
        Server.Send().Message("Console", "Hello there!");
    }
    public static void join(OXCommandData r)
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().GoinGameE2(r.com[1]);
    }
    public static void disconnect()
    {
        RelayMoment.Instance.EndConnection();
    }

    public void PrintCode(string a)
    {
        Console.Log("Join Code: " + a);
    }
    public static void locking()
    {
        Server.Instance.StartCoroutine(locking_test());
    }

    public static IEnumerator locking_test()
    {
        yield return new WaitForSeconds(2);
        Server.Send().Message("Console", "Lock test");
        Server.Send().Message("Lock", "test queue");
        yield return new WaitForSeconds(2);
        Server.Send().Message("Console", "Messages incoming");
        Server.Send("test queue").Message("Console", "Message 1");
        Server.Send("test queue").Message("Console", "Message 2");
        Server.Send("test queue").Message("Console", "Message 3");
        yield return new WaitForSeconds(2);
        Server.Send().Message("Console", "Sent, about to unlock test");
        yield return new WaitForSeconds(2);
        Server.Send().Message("Unlock", "test queue");
    }
}
