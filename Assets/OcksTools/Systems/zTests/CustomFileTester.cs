using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomFileTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var file = new OXFile();
        if (File.Exists(FileSystem.Instance.FileLocations["OXFileTest"]))
        {
            file.ReadFile(FileSystem.Instance.FileLocations["OXFileTest"]);
            foreach(var a in file.Data)
            {
                Debug.Log($"Dat: {a.Value.Name} - {a.Value.Data}");
            }
        }
        else
        {
            var cd = new OXFileData();
            cd.Name = "Penis";
            cd.Data = "TestCum";
            file.Data.Add(cd.Name, cd);
            file.WriteFile(FileSystem.Instance.FileLocations["OXFileTest"], true);
        }
    }
}
