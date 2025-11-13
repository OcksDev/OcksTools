using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileHandler : MonoBehaviour
{

    public static string Username = "";

    void Awake()
    {
        SaveSystem.LoadAllData.Append(LockIn);
        SaveSystem.SaveAllData.Append(LockOut);
    }


    public void LockIn(string dict)
    {
        var s = SaveSystem.Instance;
        s.GetDataFromFile("ox_profile");
        if (s.GetString("Username", "", "ox_profile") == "")
        {
            s.SetString("Username", $"Guest{RandomFunctions.CharPrepend(Random.Range(0, 1000000).ToString(), 6, '0')}", "ox_profile");
        }
        Username = s.GetString("Username", "", "ox_profile");
        Console.Log("Logged In User: " + Username);
    }
    public void LockOut(string dict)
    {
        var s = SaveSystem.Instance;
        if (Username == "") return;

        s.SetString("Username", Username, "ox_profile");

        s.SaveDataToFile("ox_profile");
    }

}
