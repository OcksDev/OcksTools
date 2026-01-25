using UnityEngine;
using static EntityOXS;

public class EntityObject : MonoBehaviour
{
    public EntityOXS Entity;
    public void Awake()
    {
        Entity.SetSelf(gameObject);
        if (Entity.IsDead)
        {
            KillSelf(Entity, new MultiRef<object, EntityType>(null, EntityType.World));
            return;
        }
        Entity.OnKillEvent.Append(99999, "KillSelf", KillSelf);
    }

    public void KillSelf(EntityOXS a, MultiRef<object, EntityType> b)
    {
        Destroy(gameObject);
    }
}
