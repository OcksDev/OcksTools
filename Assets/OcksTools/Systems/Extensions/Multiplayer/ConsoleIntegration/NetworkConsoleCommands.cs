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
    }
    public static void host()
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGame();
    }

    public static void host_and_copy()
    {
        RelayMoment.Instance.Join_Code = "";
        RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGame();
        ServerGamer.Instance.StartCoroutine(wait());
    }

    public static IEnumerator wait()
    {
        yield return new WaitUntil(() => RelayMoment.Instance.Join_Code != "");
        OXClip.SetClipboard("join " + RelayMoment.Instance.Join_Code);
    }
    public static void objectspawn()
    {
        SpawnSystem.Spawn(new SpawnData("Triangle")
            .Position(new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0))
            .MultiplayerShare());
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
}
