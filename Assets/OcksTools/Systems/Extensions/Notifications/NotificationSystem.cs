using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : SingleInstance<NotificationSystem>
{
    public Transform NotifParent;
    public int MaxNotifsOnScreen = 3;
    public PositionCorner corner = PositionCorner.TopRight;
    public StackingDirection stackingDirection = StackingDirection.TopDown;
    public Queue<OXNotif> backlog = new Queue<OXNotif>();
    public List<OXNotif> CurrentNotifs = new List<OXNotif>();
    public float VerticalPadding = 10f;
    public float HorizontalPadding = 10f;
    public float Spacing = 10f;
    private RectTransform rt;
    public override void Awake2()
    {
        rt = NotifParent.GetComponent<RectTransform>();
    }
    public float pos_y_inperp = 0f;
    private void Update()
    {
        var pos_y = 0f;
        var mesize = rt.GetActualSizeOfUI();
        bool reversed = false;
        if (stackingDirection == StackingDirection.BottomUp) reversed = !reversed;
        if (corner == PositionCorner.BottomLeft || corner == PositionCorner.BottomRight) reversed = !reversed;
        if (reversed)
        {
            int i = 0;
            foreach (var a in CurrentNotifs)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                pos_y += Spacing + a.size.y;
                i++;
            }
        }

        for (int i = 0; i < CurrentNotifs.Count; i++)
        {
            CurrentNotifs[i]._life -= Time.deltaTime;
            var pos = Vector2.zero;
            pos_y_inperp = pos_y;
            if (reversed)
                pos_y_inperp = -pos_y_inperp;
            switch (corner)
            {
                case PositionCorner.TopLeft:
                    pos.x = CurrentNotifs[i].size.x / 2;
                    pos.y = -CurrentNotifs[i].size.y / 2;
                    pos.x += HorizontalPadding;
                    pos.y -= VerticalPadding;
                    pos.y += pos_y_inperp;
                    pos_y -= Spacing + CurrentNotifs[i].size.y;
                    break;
                case PositionCorner.TopRight:
                    pos.x = -CurrentNotifs[i].size.x / 2 + mesize.x;
                    pos.y = -CurrentNotifs[i].size.y / 2;
                    pos.x -= HorizontalPadding;
                    pos.y -= VerticalPadding;
                    pos.y += pos_y_inperp;
                    pos_y -= Spacing + CurrentNotifs[i].size.y;
                    break;
                case PositionCorner.BottomLeft:
                    pos.x = CurrentNotifs[i].size.x / 2;
                    pos.y = CurrentNotifs[i].size.y / 2 - mesize.y;
                    pos.x += HorizontalPadding;
                    pos.y += VerticalPadding;
                    pos.y -= pos_y_inperp;
                    pos_y -= Spacing + CurrentNotifs[i].size.y;
                    break;
                case PositionCorner.BottomRight:
                    pos.x = -CurrentNotifs[i].size.x / 2 + mesize.x;
                    pos.y = CurrentNotifs[i].size.y / 2 - mesize.y;
                    pos.x -= HorizontalPadding;
                    pos.y += VerticalPadding;
                    pos.y -= pos_y_inperp;
                    pos_y -= Spacing + CurrentNotifs[i].size.y;
                    break;
            }
            pos.y += mesize.y;
            pos.x -= mesize.x;
            CurrentNotifs[i].target_pos = pos;
            CurrentNotifs[i].MoveToTarget();
            if (CurrentNotifs[i]._life <= 0)
            {
                KillNotif(CurrentNotifs[i]);
                i--;
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

    public void ClearAllNotifs()
    {
        backlog.Clear();
        for (int i = 0; i < CurrentNotifs.Count; i++)
        {
            KillNotif(CurrentNotifs[0]);
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


    public IEnumerator DestroyGameobject(GameObject o)
    {
        Destroy(o);
        yield return null;
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
        var d = Vector3.one * 100000;
        d.z = NotifParent.position.z;
        return Instantiate(a, d, Quaternion.identity, NotifParent);
    }
}


public class OXNotif
{
    public float duration = 5;
    public Notification notification;
    public Vector2 size;
    public RectTransform rectTransform;
    public Vector2 target_pos = Vector2.zero;
    public float _life = 0;
    public OXNotif(Notification notification, float duration)
    {
        this.notification = notification;
        this.duration = duration;
        notification.oxnotifref = this;
    }
    public void Publish()
    {
        var d = NotificationSystem.Instance.FuckYouAndSpawnTheNotifPrefabYouDingleButtWadd(notification.GetPrefab());
        notification.Nerd = d;
        size = notification.CalculateInitial(d);
        rectTransform = notification.GetRectTransform(d);
        Vector2 start_pos = Vector2.zero;
        rectTransform.anchoredPosition = start_pos;
    }
    public static implicit operator OXNotif(Notification n)
    {
        return new OXNotif(n, n._duration);
    }
    private int commands = 0;
    public void MoveToTarget()
    {
        commands++;
        if (commands < 3) return;
        //Debug.LogError($"{dt}, {Time.timeScale}. {dt * 50 * Time.timeScale}, {0.1f.TimeStableLerp(dt)}");
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, target_pos, 0.3f.TimeStableLerp());
    }
}

public abstract class Notification
{
    public GameObject Nerd;
    public OXNotif oxnotifref;
    private RectTransform _rectTransform;
    public float _duration = 5f;
    /// <summary>
    /// Calculates the initial state of the notification.
    /// </summary>
    /// <returns>size of UI element as vector2</returns>
    public abstract Vector2 CalculateInitial(GameObject spawned);
    public virtual RectTransform GetRectTransform(GameObject spawned)
    {
        if (_rectTransform == null) _rectTransform = spawned.GetComponent<RectTransform>();
        return _rectTransform;
    }
    public abstract GameObject GetPrefab();
    public virtual IEnumerator KillProcess(GameObject spawned) => NotificationSystem.Instance.DestroyGameobject(spawned);
    public virtual void KillMe(GameObject spawned) => NotificationSystem.Instance.KillNotif(oxnotifref);

    public Notification Duration(float t)
    {
        _duration = t;
        return this;
    }
}