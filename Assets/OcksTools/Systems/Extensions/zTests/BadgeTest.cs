using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BadgeTest : MonoBehaviour
{
    public GameObject SpawnUnder;
    public GameObject BadgeThing;
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(wai());
    }
    public IEnumerator wai()
    {
        yield return new WaitForSeconds(1);
        foreach (var a in BadgeHandler.Badges)
        {
            var b = Instantiate(BadgeThing, Vector3.zero, Quaternion.identity, SpawnUnder.transform).GetComponent<Image>();
            b.sprite = a.Value.Icon;
            Console.Log("Attempted To Make: " + a.Key);
        }
    }
}
