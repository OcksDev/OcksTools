using UnityEngine;

public static class OXRandomUnityExtensions
{
    /// <returns>(x,y) where axis range from -1 to 1</returns>
    public static Vector2 NextInSquareUnity(this OXRandom r)
    {
        var a = r.NextInSquare();
        return new Vector2((float)a.x, (float)a.y);
    }

    /// <returns>(x,y,z) where axis range from -1 to 1</returns>
    public static Vector3 NextInCubeUnity(this OXRandom r)
    {
        var c = r.NextInCube();
        return new Vector3((float)c.x, (float)c.y, (float)c.z);
    }

    /// <returns>(x,y) where axis range from -1 to 1</returns>
    public static Vector2 NextInCircleUnity(this OXRandom r)
    {
        var c1 = r.NextInCircle();
        return new Vector2((float)c1.x, (float)c1.y);
    }
    /// <returns>(x,y,z) where axis range from -1 to 1</returns>
    public static Vector3 NextInSphereUnity(this OXRandom r)
    {
        var c = r.NextInSphere();
        return new Vector3((float)c.x, (float)c.y, (float)c.z);
    }
}
