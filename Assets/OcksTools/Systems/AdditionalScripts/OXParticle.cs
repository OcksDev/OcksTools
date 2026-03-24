using System.Collections;
using UnityEngine;

public class OXParticle : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public ResettableValue<float> LifeTimer = new(2);
    public bool LiveForever = false;
    [HideInInspector]
    public bool HasCustomPlayback = false;
    private void Awake()
    {
        OnAwake();
        LifeTimer.Reset();
        if (ParticleSystem != null) ParticleSystem.Play();
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