using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileHandler : MonoBehaviour
{
    // This is an Experimental Class, mostly used to begin interacting with an idea I had about a universal profile, needs much work


    // Start is called before the first frame update
    void Awake()
    {
        SaveSystem.LoadAllData.Append(LockIn);
    }

    public void LockIn(string dict)
    {
        var s = SaveSystem.Instance;
        s.GetDataFromFile("ox_profile");
        if(s.GetString("Username", "", "ox_profile") == "")
        {
            s.SetString("Username", $"Guest{RandomFunctions.CharPrepend(Random.Range(0,1000000).ToString(), 6, '0')}", "ox_profile");
        }
    }
}
