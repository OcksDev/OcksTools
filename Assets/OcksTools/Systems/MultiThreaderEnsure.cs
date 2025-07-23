using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MultiThreaderEnsure : MonoBehaviour
{
    // neither method A, B, or C are nessecarily better than the other

    // OXThreadPoolA = Evenly distribute methods across threads.

    //Pros: No inherent problems, works each thread fairly evenly 
    //Cons: Doesn't decide based on time to execute methods, so they could pile up on one thread.

    // OXThreadPoolB = Pool methods and have threads pull from the pool.

    //Pros: Threads used more effeciently, never any downtime when possible, better handles methods of varying execution times (I think)
    //Cons: Threads can sometimes overreach and try to run methods that other threads already are pulling, which could slow down performance slightly. (it shouldn't effect functionality tho)

    // OXThreadPoolC = Only takes in on method, every thread just executes that method forever.

    //Pros: Threads used as efficiently as you let them, allows for greater control of program flow
    //Cons: Can only run one input method instead of being able to just handle arbitarary inputs at arbitrary times


    public static List<IOXThreadPool> Nerds = new List<IOXThreadPool>();
    public static MultiThreaderEnsure Instance;
    private void Awake()
    {
        Instance = this;
        for(int i = 0; i < Nerds.Count; i++)
        {
            StartCoroutine(FixSlackers(Nerds[i]));
        }
    }

    // in the case some that threads dont get created properly
    // for some reason Unity makes thread creation unreliable, at least on startup, so this fixes it.
    public IEnumerator FixSlackers(IOXThreadPool pool)
    {
        bool good = true;
        while (good)
        {
            yield return new WaitForFixedUpdate();
            Debug.Log("Checking");
            good = !pool.CheckAll();
        }
    }

}

public class OXThreadPoolA : IOXThreadPool
{
    public int ThreadCount;
    public Dictionary<int, Queue<System.Action>> ActionPool = new Dictionary<int, Queue<System.Action>>();
    public OXThreadPoolA(int threadCount)
    {
        ThreadCount = threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            ActionPool.Add(i, new Queue<System.Action>());
        }
        for (int i = 0; i < threadCount; i++)
        {
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        Debug.Log("A");
        if (MultiThreaderEnsure.Instance != null)
        {
            MultiThreaderEnsure.Instance.StartCoroutine(MultiThreaderEnsure.Instance.FixSlackers(this));
        }
        else
        {
            MultiThreaderEnsure.Nerds.Add(this);
        }
    }
    int gg = 0;
    int PullNextThread()
    {
        gg = RandomFunctions.Mod(gg + 1, ThreadCount);
        if (!allconfirmed && !SuccessfulThreads.Contains(gg) && SuccessfulThreads.Count > 0)
        {
            while (!SuccessfulThreads.Contains(gg))
            {
                gg = RandomFunctions.Mod(gg + 1, ThreadCount);
            }
        }
        return gg;
    }

    public HashSet<int> SuccessfulThreads = new HashSet<int>();
    public bool allconfirmed = false;
    private void Awaiter(int i)
    {
        bool a = false;
        try { SuccessfulThreads.Add(i); } catch { a = true; }
        if (a) return;
        if (i > ThreadCount) return;
        if (!ActionPool.ContainsKey(i))
            ActionPool.Add(i, new Queue<System.Action>());
        while (true)
        {
            if (ActionPool[i].Count > 0)
            {
                ActionPool[i].Dequeue()();
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    public void Add(System.Action gaming)
    {
        ActionPool[PullNextThread()].Enqueue(gaming);
    }

    public bool CheckAll()
    {
        bool good = true;
        for (int i = 0; i < ThreadCount; i++)
        {
            if (SuccessfulThreads.Contains(i)) continue;
            good = false;
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        if (good) allconfirmed = true;
        return good;
    }

}
public class OXThreadPoolB : IOXThreadPool
{
    public int ThreadCount;
    public Queue<System.Action> ActionPool = new Queue<System.Action>();
    public OXThreadPoolB(int threadCount)
    {
        ThreadCount = threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        Debug.Log("A");
        if (MultiThreaderEnsure.Instance != null)
        {
            MultiThreaderEnsure.Instance.StartCoroutine(MultiThreaderEnsure.Instance.FixSlackers(this));
        }
        else
        {
            MultiThreaderEnsure.Nerds.Add(this);
        }
    }

    public HashSet<int> SuccessfulThreads = new HashSet<int>();
    public bool allconfirmed = false;
    private void Awaiter(int i)
    {
        bool a = false;
        try { SuccessfulThreads.Add(i); } catch { a = true; }
        if (a) return;
        if (i > ThreadCount) return;
        System.Action weenor = null;
        bool smegs = true;
        while (true)
        {
            if (ActionPool.Count > 0)
            {
                smegs = false;
                try
                {
                    weenor = ActionPool.Dequeue();
                    smegs = true;
                }
                catch
                {
                    Debug.Log($"Thread {i} Failed To Pull (overreach)");
                    //tried to pull but another thread already did it
                }
                if (smegs)
                {
                    weenor(); //taken out of the try/catch so that debugging is easier
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    public void Add(System.Action gaming)
    {
        ActionPool.Enqueue(gaming);
    }

    public bool CheckAll()
    {
        bool good = true;
        for (int i = 0; i < ThreadCount; i++)
        {
            if (SuccessfulThreads.Contains(i)) continue;
            good = false;
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        if (good) allconfirmed = true;
        return good;
    }

}


public class OXThreadPoolC : IOXThreadPool
{
    public int ThreadCount;
    public Action<int> Method;
    public OXThreadPoolC(int threadCount, Action<int> MethodToRun)
    {
        Method = MethodToRun;
        ThreadCount = threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        Debug.Log("A");
        if (MultiThreaderEnsure.Instance != null)
        {
            MultiThreaderEnsure.Instance.StartCoroutine(MultiThreaderEnsure.Instance.FixSlackers(this));
        }
        else
        {
            MultiThreaderEnsure.Nerds.Add(this);
        }
    }
    int gg = 0;
    int PullNextThread()
    {
        gg = RandomFunctions.Mod(gg + 1, ThreadCount);
        if (!allconfirmed && !SuccessfulThreads.Contains(gg) && SuccessfulThreads.Count > 0)
        {
            while (!SuccessfulThreads.Contains(gg))
            {
                gg = RandomFunctions.Mod(gg + 1, ThreadCount);
            }
        }
        return gg;
    }

    public HashSet<int> SuccessfulThreads = new HashSet<int>();
    public bool allconfirmed = false;
    private void Awaiter(int i)
    {
        bool a = false;
        try { SuccessfulThreads.Add(i); } catch { a = true; }
        if (a) return;
        if (i > ThreadCount) return;
        while (true)
        {
            Method(i);
        }
    }
    [Obsolete("Does nothing lol")]
    public void Add(System.Action gaming)
    {

    }
    public bool CheckAll()
    {
        bool good = true;
        for (int i = 0; i < ThreadCount; i++)
        {
            if (SuccessfulThreads.Contains(i)) continue;
            good = false;
            new System.Threading.Thread(() => { Awaiter(i); }).Start();
        }
        if (good) allconfirmed = true;
        return good;
    }

}

public interface IOXThreadPool
{
    public void Add(System.Action gaming);
    public bool CheckAll();

}

