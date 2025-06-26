using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hover : MonoBehaviour
{
    public static List<RaycastResult> rcl = new List<RaycastResult>();
    public static bool CanHover = false;
    public static Hover Instance;
    private void Awake()
    {
        if(Instance != null)
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
        rcl.Clear();
        EventSystem.current.RaycastAll(ped, rcl);
    }
    public static bool IsHovering(GameObject sussy)
    {
        HoverDataCooler();
        foreach (var ray in rcl)
        {
            if (ray.gameObject == sussy)
            {
                return true;
            }
        }
        return false;
    }
}
