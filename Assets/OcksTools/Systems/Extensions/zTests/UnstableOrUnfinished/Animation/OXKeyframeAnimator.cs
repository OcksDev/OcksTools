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
