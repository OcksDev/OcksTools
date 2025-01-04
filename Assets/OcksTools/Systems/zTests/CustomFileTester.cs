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
            Debug.Log("Version Detected: " + file.FileVersion);
            foreach(var a in file.Data.DataOXFiles)
            {
                switch (a.Value.Type)
                {
                    case OXFileData.OXFileType.String:
                        Debug.Log($"Dat: {a.Value.Name} - {a.Value.Get_String()}");
                        break;
                    case OXFileData.OXFileType.Bool:
                        Debug.Log($"Dat: {a.Value.Name} - {a.Value.Get_Bool()}");
                        break;
                    case OXFileData.OXFileType.OXFileData:
                        Debug.Log($"Dat: {a.Value.Name} - SUBDATAMODULE");
                        foreach (var b in a.Value.DataOXFiles)
                        {
                            switch (b.Value.Type)
                            {
                                case OXFileData.OXFileType.String:
                                    //Debug.Log($"SubDat: {b.Value.Name} - {b.Value.Get_String()}");
                                    break;
                            }
                        }
                        break;
                    case OXFileData.OXFileType.DictStringString:
                        foreach (var b in a.Value.DataDictStringString)
                        {
                            Debug.Log($"DICT: {b.Key} - {b.Value}");
                        }
                        break;
                }
            }
        }
        else
        {
            file.Data.Add("Cum", "Wankfuck");
            file.Data.Add("Bitch", "DINGLEBERRY");
            file.Data.Add("Hariy", true);
            file.Data.Add("Stinky", false);
            file.Data.Add("pps", new List<string>() { "cum1","banana","cum2", "cum2", "cum2" });
            file.Data.Add("dick", new Dictionary<string,string>() { { "Test1", "Cum" }, { "Test2", "WEEE" } });

            var subdata = new OXFileData();
            subdata.Type = OXFileData.OXFileType.OXFileData;
            subdata.Add("Pineapple", "Pen");
            subdata.Add("Apple", "Pen");
            subdata.Add("Ugh", "Penpineappleapplepen");

            file.Data.Add("PPAP", subdata);

            file.WriteFile(FileSystem.Instance.FileLocations["OXFileTest"], true);
        }
    }
}
