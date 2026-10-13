#if OXTLS_MULTIPLAYER
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public abstract void Hit(EntityObject victim);
}
#endif
