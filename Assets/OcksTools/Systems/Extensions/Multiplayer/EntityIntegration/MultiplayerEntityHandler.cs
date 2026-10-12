#if OXTLS_MULTIPLAYER
using Unity.Collections;

public abstract class MultiplayerEntityHandler
{
    public void RegisterSelf()
    {
        Server.MessageEvent.Append(EntityMessages);
    }

    public bool EntityMessages(OXNetworkRpcData id, FixedString64Bytes type, string data)
    {
        bool full_res = false;
        EntityObject att;
        EntityObject vict;
        var q = data.StringToList();
        switch (type.ToString())
        {
            case "hit":
                att = Tags.From("Net").Get<EntityObject>(q[0]);
                vict = Tags.From("Net").Get<EntityObject>(q[1]);
                Hit(att, vict);
                full_res = true;
                break;
            case "heal":
                att = Tags.From("Net").Get<EntityObject>(q[0]);
                vict = Tags.From("Net").Get<EntityObject>(q[1]);
                Heal(att, vict);
                full_res = true;
                break;
            case "kill":
                att = Tags.From("Net").Get<EntityObject>(q[0]);
                vict = Tags.From("Net").Get<EntityObject>(q[1]);
                Kill(att, vict);
                full_res = true;
                break;
        }
        return full_res;
    }
    public abstract void Hit(EntityObject attacker, EntityObject victom);
    public abstract void Heal(EntityObject attacker, EntityObject victom);
    public abstract void Kill(EntityObject attacker, EntityObject victom);
}
#endif
