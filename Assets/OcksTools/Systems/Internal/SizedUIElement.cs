using UnityEngine;

public abstract class SizedUIElement
{
    public GameObject Nerd;
    private RectTransform _rectTransform;
    /// <summary>
    /// Calculates the initial state of the notification.
    /// </summary>
    /// <returns>size of UI element as vector2</returns>
    public abstract Vector2 CalculateInitial(GameObject spawned);
    public virtual RectTransform GetRectTransform(GameObject spawned)
    {
        if (_rectTransform == null) _rectTransform = spawned.GetComponent<RectTransform>();
        return _rectTransform;
    }
    public abstract GameObject GetPrefab();

    public abstract void KillMe();

    public virtual BetterList<GameObject> GetHandoffObjects() => Nerd;
    public virtual OXAnimationSet GetSpawnAnimation() => OXAnimationSet.FromBasicWithHandoff(OXDefaultAnimations.WobbleInEven);
    public virtual OXAnimationSet GetKillAnimation() => OXAnimationSet.FromBasicWithHandoff(OXDefaultAnimations.EaseOutEven);
}
