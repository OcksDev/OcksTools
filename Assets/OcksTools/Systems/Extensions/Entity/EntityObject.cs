using UnityEngine;
using static EntityOXS;

public class EntityObject : MonoBehaviour
{
    [AutoCompressField]
    public EntityOXS Entity;
    public void Awake()
    {
        Entity.SetSelf(this);
        if (Entity.IsDead)
        {
            KillSelf(Entity, new MultiRef<EntityObject, EntityType>(null, EntityType.World));
            return;
        }
        Entity.ClampHealth();
        Entity.OnKillEvent.Append(99999, "KillSelf", KillSelf);
    }

    public void KillSelf(EntityOXS a, MultiRef<EntityObject, EntityType> b)
    {
        Destroy(gameObject);
    }
}
