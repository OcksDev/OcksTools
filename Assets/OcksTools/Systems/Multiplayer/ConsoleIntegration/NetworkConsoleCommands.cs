using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkConsoleCommands : MonoBehaviour
{
    public OXLanguageFileIndex LanguageFileIndex;
    private void Start()
    {
        LanguageFileSystem.Instance.AddFile(LanguageFileIndex);
        //ConsoleLol.ConsoleHook.Append(NetworkConsoleCommandHook);
        ConsoleLol.ConsoleCommandHook.Append(NetworkHelpCommands);

        RelayMoment.Instance.GetComponent<PickThingymabob>().DebugCode.Append(PrintCode);
    }

    public static void host()
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGame();
    }
    public static void join(OXCommandData r)
    {
        RelayMoment.Instance.GetComponent<PickThingymabob>().GoinGameE2(r.com[1]);
    }
    public static void disconnect()
    {
        RelayMoment.Instance.EndConnection();
    }

    public void NetworkHelpCommands()
    {
        ConsoleLol.Instance.Add(new OXCommand("host").Action(host));
        ConsoleLol.Instance.Add(new OXCommand("join").Append(new OXCommand(OXCommand.ExpectedInputType.String).Action(join)));
        ConsoleLol.Instance.Add(new OXCommand("disconnect").Action(host));
    }

    public void PrintCode(string a)
    {
        Console.Log("Join Code: " + a);
    }
}
