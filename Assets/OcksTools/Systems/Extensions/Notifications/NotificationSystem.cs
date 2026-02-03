using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : SingleInstance<NotificationSystem>
{
    public int MaxNotifsOnScreen = 3;
    public PositionCorner corner = PositionCorner.TopRight;
    public StackingDirection stackingDirection = StackingDirection.TopDown;
    public Queue<OXNotif> backlog = new Queue<OXNotif>();
    public List<OXNotif> CurrentNotifs = new List<OXNotif>();
    private void Update()
    {
        for (int i = 0; i < CurrentNotifs.Count; i++)
        {
            CurrentNotifs[i]._life -= Time.deltaTime;
            if (CurrentNotifs[i]._life <= 0)
            {
                KillNotif(CurrentNotifs[i]);
                if (backlog.Count > 0) PublishNotif(backlog.Dequeue());
            }
        }
    }
    public void AddNotif(OXNotif n)
    {
        if (CurrentNotifs.Count >= MaxNotifsOnScreen)
        {
            backlog.Enqueue(n);
        }
        else
        {
            PublishNotif(n);
        }
    }

    public void PublishNotif(OXNotif o)
    {
        CurrentNotifs.Add(o);
        o._life = o.duration;
        o.Publish();
    }

    public void KillNotif(OXNotif o)
    {
        CurrentNotifs.Remove(o);
        StartCoroutine(o.notification.KillProcess(o.notification.Nerd));
    }


    public enum PositionCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public enum StackingDirection
    {
        TopDown,
        BottomUp
    }

    public GameObject FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(GameObject a)
    {
        return Instantiate(a, Vector3.one * 100000, Quaternion.identity, transform);
    }
}


public class OXNotif
{
    public float duration = 5;
    public Notification notification;
    public Vector2 size;
    public RectTransform rectTransform;
    public float _life = 0;
    public OXNotif(Notification notification, float duration = 5)
    {
        this.notification = notification;
        this.duration = duration;
    }
    public void Publish()
    {
        var d = NotificationSystem.Instance.FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(notification.GetPrefab());
        notification.Nerd = d;
        size = notification.CalculateInitial(d);
        rectTransform = notification.GetRectTransform(d);
    }
}

public abstract class Notification
{
    public GameObject Nerd;
    /// <summary>
    /// Calculates the initial state of the notification.
    /// </summary>
    /// <returns>size of UI element as vector2</returns>
    public abstract Vector2 CalculateInitial(GameObject spawned);
    public abstract RectTransform GetRectTransform(GameObject spawned);
    public abstract GameObject GetPrefab();
    public abstract IEnumerator KillProcess(GameObject spawned);
}