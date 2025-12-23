using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildNumberHolder", menuName = "Scriptable Objects/BuildNumberHolder")]
public class BuildNumberHolder : ScriptableObject
{
    public int BuildNumber;

    [Button]
    public void ResetBuildNum()
    {
        BuildNumber = 0;
    }
}
