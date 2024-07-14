using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileHandler : MonoBehaviour
{
    // This is an Experimental Class, mostly used to begin interacting with an idea I had about a universal profile, needs much work


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForSaveData());
    }

    public IEnumerator WaitForSaveData()
    {
        var s = SaveSystem.Instance;
        yield return new WaitUntil(() => { return s.LoadedData; });
        s.GetDataFromFile(2, "profile");
        if(s.GetString("Username", "", "profile") == "")
        {
            s.SetString("Username", $"Guest{RandomFunctions.Instance.CharPrepend(Random.Range(0,1000000).ToString(), 6, '0')}", "profile");
        }
    }
}
