using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class SpawnInitial : MonoBehaviour
{
    public void Awake()
    {
        SpawnSystem.Spawn(
            new SpawnData("pre").DontSpawn()
            .Position(transform.position)
            .Rotation(transform.rotation)
            );
    }
}
