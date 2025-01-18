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
    private void FixedUpdate()
    {
        for (int i = 0; i < ActiveNotifs.Count;i++)
        {
            ActiveNotifs[i].memenotif.self.anchoredPosition = Vector2.Lerp(ActiveNotifs[i].memenotif.self.anchoredPosition, CalcPos(i), 0.1f);
            if ((ActiveNotifs[i].sextimer -= Time.deltaTime) <= 0)
            {
                Destroy(ActiveNotifs[i].meme);
                ActiveNotifs.RemoveAt(i);
                i--;
                continue;
            }
        }
        if(ActiveNotifs.Count < MaxNotifsAtATime)
        {
            for(int i = 0; i < StoredNotifs.Count && ActiveNotifs.Count < MaxNotifsAtATime;)
            {
                PublishNotif(StoredNotifs[0]);
                StoredNotifs.RemoveAt(0);
            }
        }
    }

    public Vector2 CalcPos(int index)
    {
        Vector2 target = -ActiveNotifs[index].memenotif.self.sizeDelta / 2 - new Vector2(10, 0);
        target.y = 0;
        for(int i = index-1; i >= 0; i--)
        {
            target.y += 10 + ActiveNotifs[i].memenotif.self.sizeDelta.y;
        }
        return target;
    }

    public void PublishNotif(OXNotif notif)
    {
        var not = Instantiate(NotifPrefab, Vector3.zero, Quaternion.identity, NotifParent.transform);
        var thing = not.GetComponent<NotifOb>();

        thing.SetTitle(notif.Title);
        thing.SetDesc(notif.Description);
        thing.Background1.color = notif.BackgroundColor1;
        thing.Background2.color = notif.BackgroundColor2;

        thing.CalcSizeDelta();
        notif.meme = not;
        notif.memenotif = thing;
        notif.sextimer = notif.Time;
        ActiveNotifs.Insert(0, notif);
        var initpos = thing.self.sizeDelta / 2 - new Vector2(10, 0);
        initpos.y *= -1;
        thing.self.anchoredPosition = initpos;
    }

}

public class OXNotif
{
    // touch the following variables
    public float Time = 3;
    public string Title = "Notification";
    public string Description = "Something happened";
    public Sprite Image = null;
    public Color32 BackgroundColor1;
    public Color32 BackgroundColor2;
    // dont touch the following variables
    public float sextimer = 0;
    public GameObject meme;
    public NotifOb memenotif;
    public bool markedfordeath = false;
    public OXNotif()
    {
        BackgroundColor1 = new Color32(255, 255, 255, 255);
        BackgroundColor2 = new Color32(0, 0, 0, 255);
    }
}


