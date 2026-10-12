#if OXTLS_MULTIPLAYER
using NaughtyAttributes;
using UnityEngine;

public class RegisterEntity : MonoBehaviour
{
    public bool IsPlayer;
    [Required]
    public EntityObject Object;

    void Awake()
    {
        string id = Object.GetOXID();
        Tags.From("Net").Add(Object, id);
        Object.Entity.OnKillEvent.Append(9999, "Dereg", (x, y) =>
        {
            Tags.From("Net").ClearAllOf(id, Object);
        });
    }
}
#endif
