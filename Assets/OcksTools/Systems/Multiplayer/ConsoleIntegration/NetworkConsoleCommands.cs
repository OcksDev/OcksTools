using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkConsoleCommands : MonoBehaviour
{
    public OXLanguageFileIndex LanguageFileIndex;
    private void Start()
    {
        LanguageFileSystem.Instance.AddFile(LanguageFileIndex);
        ConsoleLol.ConsoleHook.Append(NetworkConsoleCommandHook);
    }

    public void NetworkConsoleCommandHook(List<string> com, List<string> com_cap)
    {
        var lang = LanguageFileSystem.Instance;
        switch (com[0])
        {
            case "host":
                RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGame();
                ConsoleLol.RecogHandover = true;
                break;
            case "join":
                try
                {
                    RelayMoment.Instance.GetComponent<PickThingymabob>().GoinGameE2(com_cap[1]);
                    ConsoleLol.RecogHandover = true;
                }
                catch
                {
                    Console.Log((

                        lang.GetString("NetConsole", "Error_BadCode")

                    ), "#ff0000ff");
                    ConsoleLol.RecogHandover = true;
                }
                break;
            case "disconnect":
                RelayMoment.Instance.GetComponent<PickThingymabob>().MakeGame();
                break;
        }
    }

}
