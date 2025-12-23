using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "BuildNumberHolder", menuName = "Scriptable Objects/BuildNumberHolder")]
public class BuildNumberHolder : ScriptableObject
{
    public int BuildNumber;
#if UNITY_EDITOR
    [Button]
    public void ResetBuildNum()
    {
        BuildNumber = 0;
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
