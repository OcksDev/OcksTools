using UnityEngine;

public class BuildNumberConsoleHook : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void Gaming()
    {
        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("version")
                .Action(LogVersion));
        });
    }
    public static void LogVersion()
    {
        string s = FileSystem.GameVer;
        var dingle = RandomFunctions.LoadResourceByPathRuntime<BuildNumberHolder>("TheNumber");
        if (dingle.BuildNumber > 0) s += " Build " + dingle.BuildNumber;
        Console.Log(s);
    }

}
