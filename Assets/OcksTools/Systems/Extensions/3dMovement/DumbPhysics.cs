using UnityEngine;

public static class DumbPhysics
{
    private const float Skin = 0.001f;

    public static Vector3 SweepCollider(
        Collider collider,
        Vector3 direction,
        float distance,
        LayerMask collisionMask,
        out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();

        if (distance <= 0f || direction == Vector3.zero)
            return collider.transform.position;

        direction.Normalize();

        bool hit = false;

        // CAPSULE
        if (collider is CapsuleCollider capsule)
        {
            Transform t = capsule.transform;

            float radius = capsule.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.z);
            float height = Mathf.Max(capsule.height * t.lossyScale.y, radius * 2f);

            Vector3 center = t.TransformPoint(capsule.center);
            Vector3 up = t.up;

            float halfHeight = (height * 0.5f) - radius;
            Vector3 p1 = center + up * halfHeight;
            Vector3 p2 = center - up * halfHeight;

            hit = Physics.CapsuleCast(
                p1, p2, radius,
                direction,
                out hitInfo,
                distance + Skin,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );
        }
        // BOX
        else if (collider is BoxCollider box)
        {
            Transform t = box.transform;
            Vector3 center = t.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, t.lossyScale);

            hit = Physics.BoxCast(
                center,
                halfExtents,
                direction,
                out hitInfo,
                t.rotation,
                distance + Skin,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );
        }
        // SPHERE
        else if (collider is SphereCollider sphere)
        {
            Transform t = sphere.transform;
            Vector3 center = t.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

            hit = Physics.SphereCast(
                center,
                radius,
                direction,
                out hitInfo,
                distance + Skin,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            Debug.LogError("Unsupported collider type");
            return collider.transform.position;
        }

        // MOVE
        float moveDistance = hit
            ? Mathf.Max(hitInfo.distance - Skin, 0f)
            : distance;

        Vector3 movement = direction * moveDistance;
        //collider.transform.position += movement;

        return movement;
    }
}
