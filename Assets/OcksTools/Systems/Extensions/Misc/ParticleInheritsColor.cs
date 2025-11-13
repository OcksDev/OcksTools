using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInheritsColor : MonoBehaviour
{
    public string balls = "Bullet";
    public SpriteRenderer gaming2;
    public ParticleSystem gaming;
    void Start()
    {
        gaming = GetComponent<ParticleSystem>();
            gaming2 = GetComponentInParent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
        settings.startColor = gaming2.color;
    }
}
