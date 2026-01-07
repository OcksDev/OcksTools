using UnityEngine;

public class EntityObject : MonoBehaviour
{
    public EntityOXS Entity;
    public void Awake()
    {
        if (Entity.IsDead)
        {
            KillSelf(Entity);
            return;
        }
        Entity.OnKillEventFinal.Append("KillSelf", KillSelf);
    }

    public void KillSelf(EntityOXS a)
    {
        Destroy(gameObject);
    }
}
