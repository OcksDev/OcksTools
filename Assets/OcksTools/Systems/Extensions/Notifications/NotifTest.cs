using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifTest : MonoBehaviour
{
    public Sprite Bananal;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var not = new OXNotif();
            not.Title = "Banana";
            not.Description = "Weenor Testing";
            not.Image = Bananal;
            NotificationSystem.Instance.AddNotif(not);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var not = new OXNotif();
            not.Time = 10;
            not.Title = "Longer Banana";
            not.Description = "This Weenor Sure Be Testing";
            NotificationSystem.Instance.AddNotif(not);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            var not = new OXNotif();
            not.Time = 3;
            not.Title = "Banana";
            not.Description = new string('A', Random.Range(1,35));
            not.BackgroundColor1 = Random.ColorHSV();
            NotificationSystem.Instance.AddNotif(not);
        }
    }
}
