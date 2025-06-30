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
        SaveSystem.SaveAllData.Append(LockOut);
        FileSystem.Instance.AssembleFilePaths();
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Badges"]);
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
    }

    public static OXFile BadgeFile;
    public static OXFileData BadgeData;
    public static List<string> Owned_Badges = new List<string>();
    public static List<string> Pinned_Badges = new List<string>();

    public void LockIn(string dict)
    {
        var s = SaveSystem.Instance;
        s.GetDataFromFile("ox_profile");
        if(s.GetString("Username", "", "ox_profile") == "")
        {
            s.SetString("Username", $"Guest{RandomFunctions.CharPrepend(Random.Range(0,1000000).ToString(), 6, '0')}", "ox_profile");
        }

        BadgeFile = new OXFile();
        BadgeData = BadgeFile.Data;
        if (System.IO.File.Exists(FileSystem.Instance.FileLocations["Profile_Badges"]))
        {
            BadgeFile.ReadFile(FileSystem.Instance.FileLocations["Profile_Badges"]);
            BadgeData = BadgeFile.Data;
            Pinned_Badges = BadgeData["PBadges"].DataListString;
            Owned_Badges = BadgeData["OBadges"].DataListString;
        }
        else
        {
            //data no load lol
        }
        
    }

    public void LockOut(string dict)
    {
        AuthorizeBadgePush();
    }


    public void AuthorizeBadgePush()
    {
        BadgeData.Add("OBadges", Owned_Badges);
        BadgeData.Add("PBadges", Pinned_Badges);
        BadgeFile.WriteFile(FileSystem.Instance.FileLocations["Profile_Badges"], true);
    }



}
