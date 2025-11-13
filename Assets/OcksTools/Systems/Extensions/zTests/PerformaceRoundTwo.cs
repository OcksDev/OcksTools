using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformaceRoundTwo : MonoBehaviour
{
    public int amnt = 10000;
    bool ready = false;
    public GameObject testob;
    public List<GameObject> testobList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(spawnme());
    }
    public IEnumerator spawnme()
    {
        Debug.Log(gameObject.GetInstanceID());
        for(int i = 0; i < amnt/2; i++)
        {
            yield return null;
            var x = Instantiate(testob);
            testobList.Add(x);
            OXComponent.StoreComponent(x.GetComponent<AudioSource>());
            x = Instantiate(testob);
            testobList.Add(x);
            OXComponent.StoreComponent(x.GetComponent<AudioSource>());
        }
        yield return null;
        ready = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!ready) return;
        Profiler.BeginSample("Reeree - Unity");
        for(int i = 0; i < amnt; i++)
        {
            var x = testobList[i].GetComponent<AudioSource>();
            x.volume = Random.Range(0.0f, 1.0f);
        }
        Profiler.EndSample();
        Profiler.BeginSample("Reeree - Ocks");
        for(int i = 0; i < amnt; i++)
        {
            var x = OXComponent.GetComponent<AudioSource>(testobList[i]);
            x.volume = Random.Range(0.0f, 1.0f);
        }
        Profiler.EndSample();
    }
}
