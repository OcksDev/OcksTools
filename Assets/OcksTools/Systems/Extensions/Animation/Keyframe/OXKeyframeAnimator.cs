using UnityEngine;

public class OXKeyframeAnimator : MonoBehaviour
{
    [HideInInspector]
    public OXKeyframeAnimationRuntime CurrentAnim;

    [HideInInspector]
    private OXKeyframeAnimationRuntime LastAnim;

    public static OXKeyframeAnimator GetOrAdd(GameObject go)
    {
        return go.TryGetComponent(out OXKeyframeAnimator a) ? a : go.AddComponent<OXKeyframeAnimator>();
    }

    public OXKeyframeAnimationRuntime Play(OXKeyframeAnimation asset, BetterList<GameObject> objects, float fadeTime = -1f, float speed = 1)
    {
        if (fadeTime < 0f) fadeTime = asset != null ? asset.FadeTime : 0f;

        OXKeyframeAnimationRuntime outgoing = null;
        if (CurrentAnim != null)
        {
            if (fadeTime > 0.0001f) outgoing = CurrentAnim;
            else CurrentAnim.StopAndReset();
        }

        var player = new OXKeyframeAnimationRuntime(asset, this, objects, outgoing, fadeTime, speed);
        CurrentAnim = LastAnim = player;
        player.Play();
        return player;
    }

    public void Stop() => CurrentAnim?.Stop();
    public void Reset() => LastAnim?.Reset();
    public void StopAndReset() => LastAnim?.StopAndReset();

    private void OnDisable() => CurrentAnim?.Stop();
}
