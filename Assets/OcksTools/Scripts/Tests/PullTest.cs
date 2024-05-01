using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTest : MonoBehaviour
{
    public bool DoSpawn = false;
    public float Timer = 1f;
    public int mult = 1;
    public float tim = 0;
    // Start is called before the first frame update
    void Start()
    {
        tim = Timer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tim -= Time.deltaTime;
        if (tim <= 0)
        {
            tim = Timer;
            if (DoSpawn)
            {
                for (int i = 0; i < mult; i++)
                {
                    var e = Instantiate(SpawnSystem.Instance.Pools[0].Object, transform.position + new Vector3(Random.Range(-7f, 7f), Random.Range(-5f, 5f), 0), transform.rotation);
                    e.SetActive(true);
                    e.GetComponent<Rigidbody2D>().gravityScale = Random.Range(0, 2) == 1 ? -1 : 1;
                    e.GetComponent<SpriteRenderer>().color = Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(0, 0, 255, 255), (e.transform.position.y + 5f) / 10f);
                }
            }
            else
            {
                for (int i = 0; i < mult; i++)
                {
                    var e = SpawnSystem.Instance.QuickSpawnObject(0);
                    e.transform.position = transform.position + new Vector3(Random.Range(-7f, 7f), Random.Range(-5f, 5f), 0);
                    e.transform.rotation = Quaternion.identity;
                    e.GetComponent<Rigidbody2D>().gravityScale = Random.Range(0, 2) == 1 ? -1 : 1;
                    e.GetComponent<SpriteRenderer>().color = Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(0, 0, 255, 255), (e.transform.position.y + 5f) / 10f);
                }
            }
        }
    }
}
