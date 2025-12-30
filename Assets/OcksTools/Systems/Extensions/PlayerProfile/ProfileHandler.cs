using UnityEngine;

public class ProfileHandler : SingleInstance<ProfileHandler>
{

    public static string Username = "";

    public override void Awake2()
    {
        SaveSystem.LoadAllData.Append(LockIn);
        SaveSystem.SaveAllData.Append(LockOut);
    }


    public void LockIn(SaveProfile dict)
    {
        var s = SaveSystem.Instance;
        var d = SaveSystem.Profile("ox_profile");
        s.GetDataFromFile(d);
        if (d.GetString("Username", "") == "")
        {
            d.SetString("Username", $"Guest{RandomFunctions.CharPrepend(Random.Range(0, 1000000).ToString(), 6, '0')}");
        }
        Username = d.GetString("Username", "");
        Console.Log("Logged In User: " + Username);
    }
    public void LockOut(SaveProfile dict)
    {
        var s = SaveSystem.Instance;
        if (Username == "") return;
        var d = SaveSystem.Profile("ox_profile");

        d.SetString("Username", Username);

        s.SaveDataToFile(d);
    }

}
