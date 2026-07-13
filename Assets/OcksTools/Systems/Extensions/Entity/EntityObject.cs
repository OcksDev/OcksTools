using UnityEngine;

public class EntityObject : MonoBehaviour
{
    [AutoCompressField]
    public EntityOXS Entity;
    public SpawnData SpawnData;
    public void Awake()
    {
        if (Entity != null) InitializeGameobjectSpecific();
    }
    public void Initialize()
    {
        Entity.SetSelf(this);
    }
    public void InitializeGameobjectSpecific()
    {
        Initialize();
        if (Entity.IsDead)
        {
            KillSelf(Entity, null);
            return;
        }
        Entity.ClampHealth();
        Entity.OnKillEvent.Append(99999, "KillSelf", KillSelf);
    }

    public void KillSelf(EntityOXS a, EntityObject b)
    {
        if (SpawnData != null)
        {
            SpawnSystem.Kill(SpawnData);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
