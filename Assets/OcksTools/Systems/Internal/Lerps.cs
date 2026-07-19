using UnityEngine;

public static class _Lerps
{
    public static float Lerp(this float val, float target, float perc) => Mathf.Lerp(val, target, perc);
    public static float LerpT(this float val, float target, float perc) => Mathf.Lerp(val, target, perc.TimeStableLerp());

    public static Vector2 Lerp(this Vector2 val, Vector2 target, float perc) => Vector2.Lerp(val, target, perc);
    public static Vector2 LerpT(this Vector2 val, Vector2 target, float perc) => Vector2.Lerp(val, target, perc.TimeStableLerp());

    public static Vector3 Lerp(this Vector3 val, Vector3 target, float perc) => Vector3.Lerp(val, target, perc);
    public static Vector3 LerpT(this Vector3 val, Vector3 target, float perc) => Vector3.Lerp(val, target, perc.TimeStableLerp());

    public static Quaternion Lerp(this Quaternion val, Quaternion target, float perc) => Quaternion.Lerp(val, target, perc);
    public static Quaternion LerpT(this Quaternion val, Quaternion target, float perc) => Quaternion.Lerp(val, target, perc.TimeStableLerp());

    public static Quaternion Slerp(this Quaternion val, Quaternion target, float perc) => Quaternion.Slerp(val, target, perc);
    public static Quaternion SlerpT(this Quaternion val, Quaternion target, float perc) => Quaternion.Slerp(val, target, perc.TimeStableLerp());
}
