using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildNumber : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        //var dingle = RandomFunctions.LoadResourceByPathEditor<BuildNumberHolder>("Assets/OcksTools/Systems/Extensions/BuildNumberCounter/TheNumber.asset");
        var dingle = RandomFunctions.LoadResourceByPathEditor<BuildNumberHolder>("Assets/Resources/TheNumber.asset");
        dingle.BuildNumber++;
        EditorUtility.SetDirty(dingle);
        AssetDatabase.SaveAssets();
    }

}
