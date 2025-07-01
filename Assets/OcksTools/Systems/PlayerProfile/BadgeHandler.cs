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
     * Simply add whatever you want to the authorized badge list in the unity inspector
     */


    public static OXFile BadgeFile;
    public static OXFileData BadgeData;
    public static List<string> Owned_Badges = new List<string>();
    public static List<string> Pinned_Badges = new List<string>();
    public static Dictionary<string, OXBadge> Badges = new Dictionary<string, OXBadge>();
    public List<OXBadge> GameAuthorizedBadges = new List<OXBadge>();

    private void Awake()
    {
        SaveSystem.LoadAllData.Append(LockIn2);
        SaveSystem.SaveAllData.Append(LockOut2);
        FileSystem.Instance.AssembleFilePaths();
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Badges"]);
        FileSystem.Instance.CreateFolder(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
    }

    public void LockIn2(string dict)
    {

        foreach (var a in GameAuthorizedBadges)
        {
            a.Version = Version;
            a.GameOrigin = FileSystem.GameTrueName;
            Badges.Add(a.Name, a);
        }


        var all = System.IO.Directory.GetFiles(FileSystem.Instance.FileLocations["Profile_Badge_Data"]);
        foreach (var a in all)
        {
            var z = a.Substring(a.LastIndexOf("\\") + 1);
            if (!z.EndsWith(".ox")) continue;
            z = z.Substring(0, z.IndexOf(".ox"));
            if (!Badges.ContainsKey(z))
            {
                StartCoroutine(LoadBadge(a));
            }
        }

        try
        {
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
        catch
        {
            Console.Log("Badge Data Corrupted: " + FileSystem.Instance.FileLocations["Profile_Badges"]);
        }



    }

    public IEnumerator LoadBadge(string a)
    {
        try
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
            if (x.width <= 128) x.filterMode = FilterMode.Point;
            n.Icon = Converter.Texture2DToSprite(x);
            Badges.Add(n.Name, n);
        }
        catch
        {
            Console.Log("Badge Corrupted: " + a);
        }

        yield return null;
    }


    public void LockOut2(string dict)
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
                dat.Add("Version", a.Value.Version);
                dat.Add("IMG", ImageConversion.EncodeToPNG(a.Value.Icon.texture));
                o.Data = dat;
                o.WriteFile($"{FileSystem.Instance.FileLocations["Profile_Badge_Data"]}\\{a.Key}.ox", true);
            }
        }
    }

    public static void AuthorizeBadgePush()
    {
        BadgeData.Add("OBadges", Owned_Badges);
        BadgeData.Add("PBadges", Pinned_Badges);
        BadgeFile.WriteFile(FileSystem.Instance.FileLocations["Profile_Badges"], true);
    }

    public static void AttemptGrantBadge(string nerd)
    {
        if(!Owned_Badges.Contains(nerd)) Owned_Badges.Add(nerd);
    }
    public static void AttemptRevokeBadge(string nerd)
    {
        if(Owned_Badges.Contains(nerd)) Owned_Badges.Remove(nerd);
        if(Pinned_Badges.Contains(nerd)) Pinned_Badges.Remove(nerd);
    }
    
    public static void AttemptPinBadge(string nerd)
    {
        if(!Pinned_Badges.Contains(nerd) && Owned_Badges.Contains(nerd)) Pinned_Badges.Add(nerd);
    }
    public static void AttemptUnpinBadge(string nerd)
    {
        if(Pinned_Badges.Contains(nerd)) Pinned_Badges.Remove(nerd);
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

