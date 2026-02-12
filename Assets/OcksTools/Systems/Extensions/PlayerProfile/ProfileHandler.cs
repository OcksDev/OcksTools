using UnityEngine;
using static SaveSystem;

public class ProfileHandler : SingleInstance<ProfileHandler>
{

    public const int MaxUsernameLength = 15;
    public static PlayerIdentity LocalUser = null;

    public override void Awake2()
    {
        SaveSystem.LoadAllData.Append(LockIn);
        SaveSystem.SaveAllData.Append(LockOut);
    }


    public void LockIn(SaveProfile dict)
    {
        var s = SaveSystem.Instance;
        var d = SaveSystem.Profile("ox_profile");
        d.SaveMethod = SaveMethod.TXTFile;

        s.GetDataFromFile(d);
        if (d.GetString("Username", "") == "")
        {
            string def_user = $"Guest{RandomFunctions.CharPrepend(Random.Range(0, 1000000).ToString(), 6, '0')}";
            d.SetString("Username", new PlayerIdentity(def_user, Tags.GenerateID()).ToString());
        }
        var def = d.GetString("Username", "");
        try
        {
            LocalUser = new PlayerIdentity("", "").FromString(def);
        }
        catch // catch allows for backporting of older versions
        {
            LocalUser = new PlayerIdentity(def, Tags.GenerateID());
        }
        Console.Log("Logged In User: " + LocalUser.GetCleanedUsername() + $" [{LocalUser.UUID}]");
    }
    public void LockOut(SaveProfile dict)
    {
        var s = SaveSystem.Instance;
        if (LocalUser == null)
        {
            "EXIT?".DLog();
            return;
        }
        var d = SaveSystem.Profile("ox_profile");
        d.SaveMethod = SaveMethod.TXTFile;

        d.SetString("Username", LocalUser.ToString());

        s.SaveDataToFile(d);
    }

}
