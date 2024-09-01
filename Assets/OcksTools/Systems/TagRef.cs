using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagRef : MonoBehaviour
{
    public string namer;
    public GameObject nerd;
    public void Awake()
    {
        if(nerd==null) nerd = gameObject;
        if (namer == "") namer = nerd.name;
        Tags.SetRef(namer, nerd);
    }
}
