using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGamin : MonoBehaviour
{
    [Button]
    public void TestButton()
    {
        Debug.Log("RAHHH");
    }

    [HorizontalLine]
    public int zzzz = 1;
}
