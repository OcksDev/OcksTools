using UnityEngine;

public class AutomaticIDCleanup : MonoBehaviour
{
    public SpawnData SD;
    private void OnDestroy()
    {
        if (SD != null) SpawnSystem.Kill(SD);
        else SpawnSystem.Kill(gameObject);
    }
}
