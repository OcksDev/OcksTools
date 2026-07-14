using UnityEngine;

[System.Serializable]
public class OXTransform
{
    public Vector3 Position;
    public Quaternion Rotation;
}
[System.Serializable]
public class OXTransformWithScale : OXTransform
{
    public Vector3 Scale;
}

public static class _ApplyTransforms
{
    public static void SetPositionOX(this Transform t, OXTransform ox) => t.position = ox.Position;
    public static void SetLocalPositionOX(this Transform t, OXTransform ox) => t.localPosition = ox.Position;
    public static void SetRotationOX(this Transform t, OXTransform ox) => t.rotation = ox.Rotation;
    public static void SetLocalRotationOX(this Transform t, OXTransform ox) => t.localRotation = ox.Rotation;
    public static void SetScaleOX(this Transform t, OXTransformWithScale ox) => t.localScale = ox.Scale;
    public static void ModPositionOX(this Transform t, OXTransform ox) => t.position += ox.Position;
    public static void ModLocalPositionOX(this Transform t, OXTransform ox) => t.localPosition += ox.Position;
    public static void ModRotationOX(this Transform t, OXTransform ox) => t.rotation *= ox.Rotation;
    public static void ModLocalRotationOX(this Transform t, OXTransform ox) => t.localRotation *= ox.Rotation;
    public static void ModScaleOX(this Transform t, OXTransformWithScale ox)
    {
        var sz = t.localScale;
        sz.x *= ox.Scale.x;
        sz.y *= ox.Scale.y;
        sz.z *= ox.Scale.z;
        t.localScale = sz;
    }

    public static void SetAllOX(this Transform t, OXTransform ox)
    {
        t.SetPositionOX(ox);
        t.SetRotationOX(ox);
    }

    public static void SetAllOX(this Transform t, OXTransformWithScale ox)
    {
        t.SetPositionOX(ox);
        t.SetRotationOX(ox);
        t.SetScaleOX(ox);
    }

    public static void ModAllOX(this Transform t, OXTransform ox)
    {
        t.ModPositionOX(ox);
        t.ModRotationOX(ox);
    }

    public static void ModAllOX(this Transform t, OXTransformWithScale ox)
    {
        t.ModPositionOX(ox);
        t.ModRotationOX(ox);
        t.ModScaleOX(ox);
    }


    public static void SetPositionOX(this OXTransform ox, Transform t) => t.position = ox.Position;
    public static void SetLocalPositionOX(this OXTransform ox, Transform t) => t.localPosition = ox.Position;
    public static void SetRotationOX(this OXTransform ox, Transform t) => t.rotation = ox.Rotation;
    public static void SetLocalRotationOX(this OXTransform ox, Transform t) => t.localRotation = ox.Rotation;
    public static void SetScaleOX(this OXTransformWithScale ox, Transform t) => t.localScale = ox.Scale;
    public static void ModPositionOX(this OXTransform ox, Transform t) => t.position += ox.Position;
    public static void ModLocalPositionOX(this OXTransform ox, Transform t) => t.localPosition += ox.Position;
    public static void ModRotationOX(this OXTransform ox, Transform t) => t.rotation *= ox.Rotation;
    public static void ModLocalRotationOX(this OXTransform ox, Transform t) => t.localRotation *= ox.Rotation;
    public static void ModScaleOX(this OXTransformWithScale ox, Transform t)
    {
        var sz = t.localScale;
        sz.x *= ox.Scale.x;
        sz.y *= ox.Scale.y;
        sz.z *= ox.Scale.z;
        t.localScale = sz;
    }

    public static void SetAllOX(this OXTransform ox, Transform t)
    {
        t.SetPositionOX(ox);
        t.SetRotationOX(ox);
    }

    public static void SetAllOX(this OXTransformWithScale ox, Transform t)
    {
        t.SetPositionOX(ox);
        t.SetRotationOX(ox);
        t.SetScaleOX(ox);
    }

    public static void ModAllOX(this OXTransform ox, Transform t)
    {
        t.ModPositionOX(ox);
        t.ModRotationOX(ox);
    }

    public static void ModAllOX(this OXTransformWithScale ox, Transform t)
    {
        t.ModPositionOX(ox);
        t.ModRotationOX(ox);
        t.ModScaleOX(ox);
    }

    public static void Lerp(this Transform val, OXTransform target, float perc)
    {
        val.position = val.position.Lerp(target.Position, perc);
        val.rotation = val.rotation.Slerp(target.Rotation, perc);
    }
    public static void LerpT(this Transform val, OXTransform target, float perc)
    {
        val.position = val.position.LerpT(target.Position, perc);
        val.rotation = val.rotation.SlerpT(target.Rotation, perc);
    }

    public static void Lerp(this Transform val, OXTransformWithScale target, float perc)
    {
        val.position = val.position.Lerp(target.Position, perc);
        val.rotation = val.rotation.Slerp(target.Rotation, perc);
        val.localScale = val.localScale.Lerp(target.Scale, perc);
    }
    public static void LerpT(this Transform val, OXTransformWithScale target, float perc)
    {
        val.position = val.position.LerpT(target.Position, perc);
        val.rotation = val.rotation.SlerpT(target.Rotation, perc);
        val.localScale = val.localScale.LerpT(target.Scale, perc);
    }

    public static void Lerp(this OXTransform target, Transform val, float perc)
    {
        val.position = val.position.Lerp(target.Position, perc);
        val.rotation = val.rotation.Slerp(target.Rotation, perc);
    }
    public static void LerpT(this OXTransform target, Transform val, float perc)
    {
        val.position = val.position.LerpT(target.Position, perc);
        val.rotation = val.rotation.SlerpT(target.Rotation, perc);
    }

    public static void Lerp(this OXTransformWithScale target, Transform val, float perc)
    {
        val.position = val.position.Lerp(target.Position, perc);
        val.rotation = val.rotation.Slerp(target.Rotation, perc);
        val.localScale = val.localScale.Lerp(target.Scale, perc);
    }
    public static void LerpT(this OXTransformWithScale target, Transform val, float perc)
    {
        val.position = val.position.LerpT(target.Position, perc);
        val.rotation = val.rotation.SlerpT(target.Rotation, perc);
        val.localScale = val.localScale.LerpT(target.Scale, perc);
    }

}
