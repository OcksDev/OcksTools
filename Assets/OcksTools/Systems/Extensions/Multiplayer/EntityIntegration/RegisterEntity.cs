#if OXTLS_MULTIPLAYER
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class RegisterEntity : MonoBehaviour
{
    public bool IsPlayer;
    [Required]
    public EntityObject Object;
    [Required]
    public IDSync Sync;

    IEnumerator Start()
    {
        if (Sync.MyID == "-") yield return new WaitUntil(() => Sync.MyID != "-");
        string id = Sync.MyID.ToString();
        Tags.From("Net").Add(Object, id);
        Object.Entity.OnKillEvent.Append("Dereg", (x, y) =>
        {
            Server.Send().Message("kill", $"{Tags.ID.Get(y.gameObject)}, {id}");
        });
        Object.Entity.OnKillEvent.Append(9999, "Dereg", (x, y) =>
        {
            Tags.From("Net").ClearAllOf(id, Object);
        });
    }
}
#endif
