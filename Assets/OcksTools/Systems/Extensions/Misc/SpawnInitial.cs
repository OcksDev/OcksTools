using UnityEngine;

public class SpawnInitial : MonoBehaviour
{
    public string UniqueName = "Gamer";
    public bool AutomaticCleanOnDestroy = true;
    public void Start()
    {
        var SD = new SpawnData("pre").DontSpawn(gameObject)
            .Position(transform.position)
            .Rotation(transform.rotation)
            .ID(UniqueName);
        SpawnSystem.Spawn(SD
            );
        if (AutomaticCleanOnDestroy)
        {
            var a = gameObject.AddComponent<AutomaticIDCleanup>();
            a.SD = SD;
        }
    }
}
