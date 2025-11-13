using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hover : MonoBehaviour
{
    public static List<GameObject> AllHovers = new List<GameObject>();
    public static bool CanHover = false;
    public static Hover Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    void Update()
    {
        CanHover = true;
    }

    public static void HoverDataCooler()
    {
        if (!CanHover) return;
        CanHover = false;
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;
        List<RaycastResult> rcl = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, rcl);

        Camera cam = Camera.main;
        var winkle = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        var dd = Physics2D.LinecastAll(winkle, winkle + new Vector2(0.001f, 0));


        AllHovers.Clear();
        foreach (var a in dd)
        {
            AllHovers.Add(a.collider.gameObject);
        }
        foreach (var a in rcl)
        {
            AllHovers.Add(a.gameObject);
        }
    }
    public static bool IsHovering(GameObject sussy)
    {
        HoverDataCooler();
        foreach (var ray in AllHovers)
        {
            if (ray.gameObject == sussy)
            {
                return true;
            }
        }
        return false;
    }
}
