using UnityEngine;

public class OneShotAudioKiller : MonoBehaviour
{
    [HideInInspector]
    public AudioSource nb;
    private void FixedUpdate()
    {
        if (!nb.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
