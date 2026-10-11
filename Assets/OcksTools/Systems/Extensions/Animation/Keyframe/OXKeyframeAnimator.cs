using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimator : MonoBehaviour
{
    [HideInInspector]
    public OXKeyframeAnimationRuntime CurrentAnim;

    private OXKeyframeAnimationRuntime LastAnim;

    public static OXKeyframeAnimator GetOrAdd(GameObject go)
    {
        return go.TryGetComponent(out OXKeyframeAnimator a) ? a : go.AddComponent<OXKeyframeAnimator>();
    }

    /// <summary>
    /// Plays an animation on these objects. fadeTime &lt; 0 uses the asset's FadeTime; 0 stops and resets the
    /// current animation first. While fading, the old animation keeps running and is stopped when the fade ends.
    /// </summary>
    public OXKeyframeAnimationRuntime Play(OXKeyframeAnimation asset, IList<GameObject> objects, float fadeTime = -1f)
    {
        if (fadeTime < 0f) fadeTime = asset != null ? asset.FadeTime : 0f;

        OXKeyframeAnimationRuntime outgoing = null;
        if (CurrentAnim != null)
        {
            if (fadeTime > 0.0001f) outgoing = CurrentAnim;
            else CurrentAnim.StopAndReset();
        }

        var player = new OXKeyframeAnimationRuntime(asset, this, objects, outgoing, fadeTime);
        CurrentAnim = LastAnim = player;
        player.Play();
        return player;
    }

    public void Stop() => CurrentAnim?.Stop();
    public void Reset() => LastAnim?.Reset();
    public void StopAndReset() => LastAnim?.StopAndReset();

    private void OnDisable() => CurrentAnim?.Stop();
}
