using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogChoose : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clicky()
    {
        DialogLol.Instance.Choose(int.Parse(gameObject.name));
    }
}
