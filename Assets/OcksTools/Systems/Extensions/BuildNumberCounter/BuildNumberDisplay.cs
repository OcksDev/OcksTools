using TMPro;
using UnityEngine;

public class BuildNumberDisplay : MonoBehaviour
{
    public TextMeshProUGUI tt;
    public BuildNumberHolder buildNumberHolder;
    private void Awake()
    {
        var s = FileSystem.GameVer;
        //if (buildNumberHolder.BuildNumber > 0) 
        s += "_b" + buildNumberHolder.BuildNumber;
        tt.text = s;
    }
}
