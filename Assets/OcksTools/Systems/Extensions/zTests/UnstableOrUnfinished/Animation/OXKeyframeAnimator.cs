using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimator : MonoBehaviour
{
    [HideInInspector]
    public OXKeyframeAnimationRuntime CurrentAnim = null;

    // Kept after an animation finishes or is stopped, so Reset() still knows what to restore.
    private OXKeyframeAnimationRuntime LastAnim;

    public static OXKeyframeAnimator GetOrAdd(GameObject go)
    {
        var a = go.GetComponent<OXKeyframeAnimator>();
        if (a == null) a = go.AddComponent<OXKeyframeAnimator>();
        return a;
    }

    public OXKeyframeAnimationRuntime Play(OXKeyframeAnimation asset, IList<GameObject> objects)
    {
        if (CurrentAnim != null) CurrentAnim.StopAndReset();

        var player = new OXKeyframeAnimationRuntime(asset, this, objects);
        CurrentAnim = player;
        LastAnim = player;
        player.Play();
        return player;
    }

    public void Stop()
    {
        if (CurrentAnim != null) CurrentAnim.Stop();
    }

    /// <summary>Resets the most recently played animation's objects (works even after it finished).</summary>
    public void Reset()
    {
        if (LastAnim != null) LastAnim.Reset();
    }

    public void StopAndReset()
    {
        if (LastAnim != null) LastAnim.StopAndReset();
    }

    private void OnDisable()
    {
        // Coroutines die with a disabled component; keep the bookkeeping honest.
        if (CurrentAnim != null) CurrentAnim.Stop();
    }
}
