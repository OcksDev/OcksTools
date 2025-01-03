using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            foreach(var a in OXFile.Data)
            {
                Debug.Log($"Dat: {a.Value.Name} - {a.Value.DataRaw}");
            }
        }
        else
        {
            var cd = new OXFileData();
            cd.Name = "Penis";
            cd.DataRaw = Encoding.ASCII.GetBytes("TestCum");
            OXFile.Data.Add(cd.Name, cd);
            file.WriteFile(FileSystem.Instance.FileLocations["OXFileTest"], true);
        }
    }
}
