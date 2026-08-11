using UnityEngine;

public class ObjectRef : MonoBehaviour
{
    public string namer;
    public GameObject nerd;
    public void Awake()
    {
        if (nerd == null) nerd = gameObject;
        if (namer == "") namer = nerd.name;
        GlobalRefs.SetRef(namer, nerd);
    }
}
