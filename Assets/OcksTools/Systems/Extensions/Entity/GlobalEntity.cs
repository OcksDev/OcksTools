public class GlobalEntity : SingleInstanceGeneric<EntityObject>
{
    public override void SetBaseInstance()
    {
        var eo = gameObject.AddComponent<EntityObject>();
        var e = new EntityOXS();
        eo.Entity = e;
        eo.Initialize();
        SetInstance(eo);
        e.SetHealths(1);
        e.Type = EntityOXS.EntityType.World;
    }
}
