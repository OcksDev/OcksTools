using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInitial : MonoBehaviour
{
    public string UniqueName = "Gamer";
    public void Start()
    {
        SpawnSystem.Spawn(
            new SpawnData("pre").DontSpawn(gameObject)
            .Position(transform.position)
            .Rotation(transform.rotation)
            .ID(UniqueName)
            );
        Console.Log("E");
    }
}
