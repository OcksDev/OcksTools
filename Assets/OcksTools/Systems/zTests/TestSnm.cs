using System.Collections;
using UnityEngine;

public class TestSnm : MonoBehaviour
{
    public float dist = 5;
    public bool smegleton = true;
    private void FixedUpdate()
    {
        var d = (Quaternion.Euler(0, 0, Time.time * 90) * Vector3.right) * dist;
        if (smegleton)
        {
            float z = 1 / 50f;
            var d2 = (Quaternion.Euler(0, 0, (Time.time + z) * 90) * Vector3.right) * dist;
            //var d2 = Vector3.zero;
            banana = OXLerp.Linear((x) =>
            {
                transform.position = Vector3.Lerp(d, d2, x);
            },z);
        }
        else
        {
            transform.position = d;
        }
    }
    public IEnumerator banana = null;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Time.timeScale = 0.2f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        if (banana != null)
        {
            StartCoroutine(banana);
            banana = null;
        }
    }
}
