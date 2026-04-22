using UnityEngine;

public class Camera2D : SingleInstance<Camera2D>
{
    public bool FollowsMouse = true;
    public float MouseFollowSpeed = 1;
    public float MouseFollowStrength = 1;
    public Vector3 targetpos = new Vector3(0, 0, 0);
    private Vector3 ppos = new Vector3(0, 0, 0);
    // Start is called before the first frame update
    public ShakeHolder Shake = new ShakeHolder();

    private void LateUpdate()
    {
        /* some an example for what hurting a player could be like
        if (Input.GetKeyDown(KeyCode.N))
        {
            ppos.y += 1f;
            Shake(0.4f, 0.8f);
        }
        */
        transform.position = ppos;
        //handles getting the mouse position and making the camera adjust to move to it
        Vector3 p = Vector3.zero;
        if (FollowsMouse)
        {
            p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p -= transform.position;
            p /= 5;
            p *= MouseFollowStrength;
        }

        // "pos" is the location the camera tries to get to
        p += targetpos;
        p.z = -10;
        var z = Vector3.MoveTowards(transform.position, p, Dist(p, transform.position) * 8 * Time.deltaTime * MouseFollowSpeed);

        /* zz can be the max size the camera can go to, remove/change as needed
        float zz = 99999;
        z.x = Mathf.Clamp(z.x, -zz, zz);
        z.y = Mathf.Clamp(z.y, -zz, zz);
        */
        ppos = z;
        Vector3 ss = ppos;
        ss += Shake.GetPos(Time.deltaTime);
        transform.position = ss;
    }

    private float Dist(Vector3 p1, Vector3 p2)
    {
        float distance = Mathf.Sqrt(
                Mathf.Pow(p2.x - p1.x, 2f) +
                Mathf.Pow(p2.y - p1.y, 2f) +
                Mathf.Pow(p2.z - p1.z, 2f));
        return distance;
    }

}
