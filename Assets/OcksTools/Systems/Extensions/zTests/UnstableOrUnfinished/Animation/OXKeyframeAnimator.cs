using System.Collections.Generic;
using UnityEngine;

public class OXKeyframeAnimator : MonoBehaviour
{
    [HideInInspector]
    public OXKeyframeAnimationRuntime CurrentAnim = null;

    public static OXKeyframeAnimator GetOrAdd(GameObject go)
    {
        var a = go.GetComponent<OXKeyframeAnimator>();
        if (a == null) a = go.AddComponent<OXKeyframeAnimator>();
        return a;
    }

    public OXKeyframeAnimationRuntime Play(OXKeyframeAnimation asset, IList<GameObject> objects)
    {
        if (CurrentAnim != null) CurrentAnim.Stop();

        var player = new OXKeyframeAnimationRuntime(asset, this, objects);
        CurrentAnim = player;
        player.Play();
        return player;
    }

    public void Stop()
    {
        if (CurrentAnim != null) CurrentAnim.Stop();
    }

    private void OnDisable()
    {
        // Coroutines die with a disabled component; keep the bookkeeping honest.
        if (CurrentAnim != null) CurrentAnim.Stop();
    }
}
