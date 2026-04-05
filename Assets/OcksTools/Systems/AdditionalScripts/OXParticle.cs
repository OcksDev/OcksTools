using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OXParticle : MonoBehaviour
{
    public List<ParticleSystem> ParticleSystems = new();
    public ParticleStartType StartType = ParticleStartType.All;
    public ResettableValue<float> LifeTimer = new(2);
    public bool LiveForever = false;
    [HideInInspector]
    public bool HasCustomPlayback = false;
    public enum ParticleStartType
    {
        All,
        First,
        Last,
        None,
        Random,
    }

    private void Awake()
    {
        OnAwake();
        LifeTimer.Reset();

        switch (StartType)
        {
            case ParticleStartType.All:
                foreach (var ps in ParticleSystems) ps.Play();
                break;
            case ParticleStartType.First:
                ParticleSystems[0].Play();
                break;
            case ParticleStartType.Last:
                ParticleSystems[ParticleSystems.Count - 1].Play();
                break;
            case ParticleStartType.None:
                break;
            case ParticleStartType.Random:
                ParticleSystems[Random.Range(0, ParticleSystems.Count)].Play();
                break;

        }

        if (HasCustomPlayback) StartCoroutine(Playback());
    }
    private void FixedUpdate()
    {
        FixedUpdatePlayback();
        if (LiveForever) return;
        LifeTimer.Current -= Time.deltaTime;
        if (LifeTimer <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }
    public virtual IEnumerator Playback() { yield return null; }
    public virtual void OnAwake() { }
    public virtual void FixedUpdatePlayback() { }
}