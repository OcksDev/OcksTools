using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadTest : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Pool());
    }
    public static int what = 0;
    private List<Reetypebakakaka> reetypebakakakas = new List<Reetypebakakaka>();
    public IEnumerator Pool()
    {
        OXThreadPoolB pool = new OXThreadPoolB(10);
        for (int i = 0; i < 26; i++)
        {
            pool.Add(new Reetypebakakaka(i).print);
        }
        yield return null;
    }

}


public class Reetypebakakaka
{
    private int x;
    public Reetypebakakaka(int x)
    {
        this.x = x;
    }
    public void print()
    {
        Debug.Log(x);
    }
}
