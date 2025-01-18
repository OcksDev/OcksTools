using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var not = new OXNotif();
            not.Title = "Banana";
            not.Description = "Fuck me man, giga nut";
            NotificationSystem.Instance.AddNotif(not);
        }
    }
}
