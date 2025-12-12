using UnityEngine;

public class ParticleInheritsColor : MonoBehaviour
{
    public string balls = "Bullet";
    public SpriteRenderer gaming2;
    public ParticleSystem gaming;
    private void Start()
    {
        gaming = GetComponent<ParticleSystem>();
        gaming2 = GetComponentInParent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
        settings.startColor = gaming2.color;
    }
}
