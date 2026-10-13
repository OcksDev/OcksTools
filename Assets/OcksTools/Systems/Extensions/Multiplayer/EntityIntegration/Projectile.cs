using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public abstract void Hit(EntityObject victim);
}
