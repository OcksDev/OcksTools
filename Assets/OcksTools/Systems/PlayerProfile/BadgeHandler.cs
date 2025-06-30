using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeHandler : MonoBehaviour
{

    public const string Version = "v1.0";

    /*
     * Creating New Badges:
     *  - All images must be set as Read/Write (Advanced > Read/Write > True)
     *  - All images must be set as Uncompressed (Default > Compression > None)
     * Simply add whatever you want to the authorized badge list
     */


    public static OXFile BadgeFile;
    public static OXFileData BadgeData;
    public static List<string> Owned_Badges = new List<string>();
    public static List<string> Pinned_Badges = new List<string>();
    public static Dictionary<string, OXBadge> Badges = new Dictionary<string, OXBadge>();
    public List<OXBadge> GameAuthorizedBadges = new List<OXBadge>();

    private void Awake()
    {
        SaveSystem.LoadAllData.Append(LockIn);
        SaveSystem.SaveAllData.Append(LockOut);
        FileSystem.Instance.AssembleFilePaths();
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Badges"]);
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
    }

    public void LockIn(string dict)
    {

        foreach (var a in GameAuthorizedBadges)
        {
            Badges.Add(a.Name, a);
        }
        BadgeFile = new OXFile();
        BadgeData = BadgeFile.Data;
        if (System.IO.File.Exists(FileSystem.Instance.FileLocations["Profile_Badges"]))
        {
            BadgeFile.ReadFile(FileSystem.Instance.FileLocations["Profile_Badges"]);
            BadgeData = BadgeFile.Data;
            Pinned_Badges = BadgeData["PBadges"].DataListString;
            Owned_Badges = BadgeData["OBadges"].DataListString;

            foreach (var a in Owned_Badges)
            {
                Console.Log("Owned Badge: " + a);
            }
        }
        else
        {
            //data no load lol
        }


        var all = System.IO.Directory.GetFiles(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
        foreach(var a in all)
        {
            if (a.EndsWith(".ox"))
            {
                StartCoroutine(LoadBadge(a));
            }
        }

    }

    public IEnumerator LoadBadge(string a)
    {
        var o = new OXFile();
        o.ReadFile(a);
        var n = new OXBadge();
        n.Name = o.Data["Name"].DataString;
        if (Badges.ContainsKey(n.Name)) yield break;
        n.Description = o.Data["Desc"].DataString;
        n.Version = o.Data["Version"].DataString;
        n.GameOrigin = o.Data["Game"].DataString;
        var x = new Texture2D(1, 1);
        x.LoadImage(o.Data["IMG"].DataRaw);
        n.Icon = Converter.Texture2DToSprite(x);
        Console.Log("Loaded: " + n.Name);
        Badges.Add(n.Name, n);
        yield return null;
    }


    public void LockOut(string dict)
    {
        AuthorizeBadgePush();
        WriteAllBadgeData();


    }
    public void WriteAllBadgeData()
    {
        var all = System.IO.Directory.GetFiles(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
        foreach (var a in Badges)
        {
            bool found = false;
            foreach (var b in all)
            {
                if (b.Substring(b.LastIndexOf("\\")).Contains(a.Key))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var o = new OXFile();
                var dat = new OXFileData(OXFileData.OXFileType.OXFileData);
                dat.Add("Name", a.Value.Name);
                dat.Add("Desc", a.Value.Description);
                dat.Add("Game", a.Value.GameOrigin);
                dat.Add("Version", Version);
                dat.Add("IMG", ImageConversion.EncodeToPNG(a.Value.Icon.texture));
                o.Data = dat;
                o.WriteFile($"{FileSystem.Instance.FileLocations["Profile_Badge_Data"]}\\{a.Key}.ox", true);
            }
        }
    }

    public void AuthorizeBadgePush()
    {
        //TESTING DATA
        Owned_Badges = new List<string>()
        {
            "Gooning",
            "Dev",
        };

        BadgeData.Add("OBadges", Owned_Badges);
        BadgeData.Add("PBadges", Pinned_Badges);
        BadgeFile.WriteFile(FileSystem.Instance.FileLocations["Profile_Badges"], true);
    }



}
[System.Serializable]
public class OXBadge
{
    public string Name;
    public string Description;
    [HideInInspector]
    public string GameOrigin;
    [HideInInspector]
    public string Version; 
    public Sprite Icon;
}

