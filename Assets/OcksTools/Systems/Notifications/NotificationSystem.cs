using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public int MaxNotifsAtATime = 3;
    public GameObject NotifPrefab;
    public RectTransform NotifParent;
    List<OXNotif> ActiveNotifs = new List<OXNotif>();
    List<OXNotif> StoredNotifs = new List<OXNotif>();
    public static NotificationSystem Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void AddNotif(OXNotif notif)
    {
        if(ActiveNotifs.Count >= MaxNotifsAtATime)
        {
            StoredNotifs.Add(notif);
        }
        else
        {
            PublishNotif(notif);
        }
    }

    public void PublishNotif(OXNotif notif)
    {

    }

}

public class OXNotif
{
    public float Time = 0;
    public string Title = "Notification";
    public string Description = "Something happened";
    public Sprite Image = null;
    public Color32 BackgroundColor1;
    public Color32 BackgroundColor2;
    public OXNotif()
    {
        Time = 3;
        BackgroundColor1 = new Color32(255, 255, 255, 255);
        BackgroundColor2 = new Color32(0, 0, 0, 255);
    }
}


