using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimator : MonoBehaviour
{
    [HideInInspector]
    public OXKeyframeAnimationRuntime CurrentAnim = null;

    private OXKeyframeAnimationRuntime LastAnim;

    public static OXKeyframeAnimator GetOrAdd(GameObject go)
    {
        var a = go.GetComponent<OXKeyframeAnimator>();
        if (a == null) a = go.AddComponent<OXKeyframeAnimator>();
        return a;
    }

    /// <summary>
    /// Plays an animation on these objects.
    /// fadeTime: seconds to crossfade from the animation that is currently playing on this animator.
    /// Pass a negative value (the default) to use the asset's own FadeTime, or 0 to cut straight over
    /// (the old animation is stopped and its objects reset to rest first).
    /// While fading, the old animation keeps running underneath and is stopped when the fade ends.
    /// </summary>
    public OXKeyframeAnimationRuntime Play(OXKeyframeAnimation asset, IList<GameObject> objects, float fadeTime = -1f)
    {
        if (fadeTime < 0f) fadeTime = asset != null ? asset.FadeTime : 0f;

        OXKeyframeAnimationRuntime outgoing = null;
        if (CurrentAnim != null)
        {
            if (fadeTime > 0.0001f) outgoing = CurrentAnim; // blended away from, then stopped by the new runtime
            else CurrentAnim.StopAndReset();
        }

        var player = new OXKeyframeAnimationRuntime(asset, this, objects, outgoing, fadeTime);
        CurrentAnim = player;
        LastAnim = player;
        player.Play();
        return player;
    }

    public void Stop()
    {
        if (CurrentAnim != null) CurrentAnim.Stop();
    }

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
        if (CurrentAnim != null) CurrentAnim.Stop();
    }
}
