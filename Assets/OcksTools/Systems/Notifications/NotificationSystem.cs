using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class NotificationSystem : MonoBehaviour
{
    public int MaxNotifsAtATime = 3;
    public Vector2 PositionOffset = Vector2.zero;
    public Directions Direction = Directions.BottomRight;
    public GameObject NotifPrefab;
    public RectTransform NotifParent;
    List<OXNotif> ActiveNotifs = new List<OXNotif>();
    List<OXNotif> StoredNotifs = new List<OXNotif>();
    public static NotificationSystem Instance;
    private void Awake()
    {
        Instance = this;

        switch (Direction)
        {
            case Directions.BottomRight:
                NotifParent.anchorMin = new Vector2(1,0);
                break;
            case Directions.BottomLeft:
                NotifParent.anchorMin = new Vector2(0,0);
                break;
            case Directions.TopRight:
                NotifParent.anchorMin = new Vector2(1, 1);
                break;
            case Directions.TopLeft:
                NotifParent.anchorMin = new Vector2(0,1);
                break;
        }

        NotifParent.anchorMax = NotifParent.anchorMin;
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
            bool marked = ActiveNotifs[i].markedfordeath;
            var banana = CalcPos(i);
            ActiveNotifs[i].memenotif.self.anchoredPosition = Vector2.Lerp(ActiveNotifs[i].memenotif.self.anchoredPosition, banana, marked?0.15f:0.1f);
            if (!marked && (ActiveNotifs[i].sextimer -= Time.deltaTime) <= 0)
            {
                ActiveNotifs[i].markedfordeath = true;
                marked = true;
                continue;
            }
            if (marked)
            {
                if((ActiveNotifs[i].memenotif.self.anchoredPosition-banana).magnitude < 5f)
                {
                    Destroy(ActiveNotifs[i].meme);
                    ActiveNotifs.RemoveAt(i);
                    i--;
                }
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
        if (ActiveNotifs[index].markedfordeath && ActiveNotifs[index].storeddeathlocation != new Vector2(-1,-1))
        {
            return ActiveNotifs[index].storeddeathlocation;
        }
        int m = 1;
        int m2 = 1;
        switch (Direction)
        {
            case Directions.TopLeft:
                m = -1;
                m2 = -1;
                break;
            case Directions.BottomLeft: 
                m2 = -1;
                break;
            case Directions.TopRight:
                m = -1;
                break;

        }

        Vector2 target = -ActiveNotifs[index].memenotif.self.sizeDelta / 2 - new Vector2(10, 0);
        target.y *= -1;
        target.y += 10;
        target.y *= m;
        target.x *= m2;
        for(int i = index-1; i >= 0; i--)
        {
            var bana = 10 + ActiveNotifs[i].memenotif.self.sizeDelta.y;
            target.y += bana * m;
        }
        target += PositionOffset;
        if (ActiveNotifs[index].markedfordeath)
        {
            target.x = (ActiveNotifs[index].memenotif.self.sizeDelta.x / 2 + 10) * m2;
            target.y += (ActiveNotifs[index].memenotif.self.sizeDelta.y / 2) * m;
            ActiveNotifs[index].storeddeathlocation = target;
        }
        return target;
    }
    public void PublishNotif(OXNotif notif)
    {
        var not = Instantiate(NotifPrefab, Vector3.zero, Quaternion.identity, NotifParent.transform);
        var thing = not.GetComponent<NotifOb>();

        thing.SetTitle(notif.Title);
        thing.SetDesc(notif.Description);
        thing.SetIMG(notif.Image);
        thing.Background1.color = notif.BackgroundColor1;
        thing.Background2.color = notif.BackgroundColor2;

        thing.CalcSizeDelta();
        notif.meme = not;
        notif.memenotif = thing;
        notif.sextimer = notif.Time;
        ActiveNotifs.Insert(0, notif);
        int m = 1;
        int m2 = 1;
        switch (Direction)
        {
            case Directions.TopLeft:
                m = -1;
                m2 = -1;
                break;
            case Directions.BottomLeft:
                m2 = -1;
                break;
            case Directions.TopRight:
                m = -1;
                break;

        }
        var initpos = new Vector2((-10 + thing.self.sizeDelta.x / 2) * m2, 10 * m);
        initpos += PositionOffset;
        thing.self.anchoredPosition = initpos;
    }
    public enum Directions
    {
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft,
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
    public Vector2 storeddeathlocation;
    public OXNotif()
    {
        storeddeathlocation = new Vector2(-1,-1);
        BackgroundColor1 = new Color32(255, 255, 255, 255);
        BackgroundColor2 = new Color32(0, 0, 0, 255);
    }
}


